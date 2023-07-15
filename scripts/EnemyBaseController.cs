using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseController : MonoBehaviour
{
    public GameObject head;

    private float maxHealth = 100;
    public float health = 100;
    private float maxBalance = 100;
    public float balance = 100;
    public float movementSpeed = 1f;
    public bool isBoss = false;

    public GameObject weaponGameobject;
    private WeaponBase weapon;
    private float closeRange = 0;
    private float mediumRange = 0;
    public AttackBase currentAttack = null;

    public DodgeBase dodge;

    public GameObject room = null;
    public bool hit = false;
    public AttackBase hasBeenHit = null;

    private GameObject player;
    public bool isPlayerSeen = false;
    private bool facePlayer = true;
    private string knownPlayerRange = "melee";

    public bool doingAction = false;
    private bool IsHitThisFrame = false;
    private bool hitBlocked = false;

    private int parrying = 0;
    private bool isBlocking = false;
    private bool invincible = false;
    public bool isBalanceBroken = false;

    Rigidbody2D rb2 = null;
    public Save stats;
    private int rayFilter;

    public int difficulty = 1; // difficulty cant be 0
    private string strategy = "basic";
    public Dictionary<string, int> myAction = new Dictionary<string, int>() { { "action", -1 }, { "myPositioning", -1} }; // -1 = doing nothing, 0 = attacking, 1 = charging, 2 = parrying, 3 = dodging, 4 = backing off, 5 = trying to get close
    public Dictionary<string, int> playerAction = new Dictionary<string, int>() { { "action", -1 }, { "playerPositioning", -1} };
    private List<Dictionary<Dictionary<string, int>, string>> memories = new List<Dictionary<Dictionary<string, int>, string>>();  // Defines how long memory can be. If memory is exceeded,

    private float weaponLenght = 0;
    private float playerWeaponLenght = 0;
    private float playerRange = 0;
    private bool bDeterminePlayerRange = true;

    public Dictionary<string, List<AttackBase>> attackDic = new Dictionary<string, List<AttackBase>>()
    { { "closeRange", new List<AttackBase>() } };
    public int comboCount = 0;
    public int currentComboLenght = 0;

    private Dictionary<int, bool> statusEffects = EventSystem.GetStatusEffectBools();
    public bool GetIsBlocking() { return isBlocking; }

    // Start is called before the first frame update
    void Start()
    {
        rayFilter = LayerMask.GetMask("Enemy");
        rayFilter |= LayerMask.GetMask("Ignore Raycast");
        rayFilter = ~rayFilter;

        EventSystem.current.onPlayerAttack += DidPlayerHit;
        EventSystem.current.onPlayerAttackEnd += PlayerAttackEndEvents;

        player = PlayerController.current.gameObject;
        if (weaponGameobject == null) Debug.Log(gameObject + " is missing a weapon");
        else
        {
            weapon = weaponGameobject.GetComponent<WeaponBase>();
            weapon.CreateANewWeaponBase(true, "blunt", true, 1, false, "strenght", 0.5f, 1, "strenght", 0.5f);
            weaponLenght = weapon.gameObject.transform.lossyScale.y;

            Debug.Log("Creating attack for " + name);
            AttackBase swing = weaponGameobject.AddComponent<AttackBase>();
            List<int> damageList = new List<int>() { 2, 3 };
            List<int> balanceDamageList = new List<int>() { 2 };
            swing.CreateANewAttackBase(false, 1, 1, 0.1f, weapon.gameObject.GetComponent<Animator>(), "swing_rightSide_leftSide", "mouse 0", damageList, balanceDamageList, 15, 0);
            attackDic["closeRange"].Add(swing);

            AttackBase swing2 = weaponGameobject.AddComponent<AttackBase>();
            swing2.CreateANewAttackBase(false, 1, 1, 0.1f, weapon.gameObject.GetComponent<Animator>(), "swing_leftSide_rightSide", "mouse 0", damageList, balanceDamageList, 15, 1);
            attackDic["closeRange"].Add(swing2);
        }
        if (dodge == null)
        {
            dodge = gameObject.AddComponent<DodgeBase>();
            dodge.SetDodge("dash", 10, false);
        }

        rb2 = gameObject.GetComponent<Rigidbody2D>();

        if (weapon)
        {
            closeRange = weapon.gameObject.transform.lossyScale.y + 0.15f;
            mediumRange = closeRange * 3;
        }

        playerRange = DeterminePlayerWeaponLenght();
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Die();
        }
        StartCoroutine(WaitForEndOfFrame());

        if (comboCount > currentComboLenght) comboCount = 0;

        if (!isBalanceBroken)
        {
            if (CanPlayerBeSeen())
            {
                head.transform.up = GetPlayerDirection();
                if (PlayerController.current.currentAttack)
                    StartCoroutine(DeterminePlayerRange());
                
                string threat = CompareMemoryToTheMoment();
                if (threat != null && currentAttack != null)
                {
                    TakeCounterAct(threat);
                }
                // TO-DO Korjaa oppiminen. Lue netistä ohjeita.

                if (facePlayer) FacePlayer();

                Alert();
                if (parrying == 0 && currentAttack == null && !doingAction)
                    TakeAction(WhatIsPlayerDoing());
            }
        }
    }

    private void Die()
    {
        if (!isBoss)
        {
            if (room != null) room.GetComponent<RoomRenderingScript>().children.Remove(transform);
            EventSystem.current.onPlayerAttack -= DidPlayerHit;
            EventSystem.current.onPlayerAttackEnd -= PlayerAttackEndEvents;
            Destroy(gameObject);
        }
    }

    private void ResetAI()
    {
        isBlocking = false;
        AddNewMyAction(-1);
    }
    
    private IEnumerator WaitForEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        if(!CanPlayerBeSeen())
        {
            if (currentAttack != null)
            {
                currentAttack.Cancel();
            }
        }
        IsHitThisFrame = false;
    }

    private int balanceBreakStack = 0;
    private IEnumerator BreakAndRegainBalance(string hitSeverity)
    {
        balanceBreakStack++;
        int i = balanceBreakStack;
        isBalanceBroken = true;
        isBlocking = false;
        balance = maxBalance;

        while(rb2.velocity != Vector2.zero)
        {
            yield return new WaitForEndOfFrame();
        }
        if (hitSeverity == "minor") yield return new WaitForSeconds(0.5f);
        else if (hitSeverity == "medium") yield return new WaitForSeconds(1f);
        else if (hitSeverity == "major") yield return new WaitForSeconds(2f);
        if (balanceBreakStack == i)
        {
            isBalanceBroken = false;
            balanceBreakStack = 0;
            balance = maxBalance;
        }
        else if (balance >= 3)
        {
            isBalanceBroken = false;
            balanceBreakStack = 0;
            balance = 100;
        }
    }

    private void Move(Vector3 dir, float speedMultiplier = 1) // speed multiplier has to be atleast 1
    {
        dir = dir.normalized;
        float movementX = (dir.x * movementSpeed) - rb2.velocity.x;
        float movementY = (dir.y * movementSpeed) - rb2.velocity.y;

        dir = new Vector2(movementX, movementY) * speedMultiplier;
        
        rb2.AddForce(dir);
    }

    public bool IsHit(int dmg, int balanceDMG)
    {
        if (!hasBeenHit)
        {
            Debug.Log(this.gameObject + " has been hit with " + dmg + " damage and " + balanceDMG + " balance damage.");

            IsHitThisFrame = true;
            PlayerController player = PlayerController.current;
            if (!isBalanceBroken) transform.up = player.transform.position - transform.position;

            if (currentAttack)
                currentAttack.IsHit();

            if (parrying > 0 && player.currentAttack.blockable)
            {
                switch (parrying)
                {
                    case 1:
                        player.TakeBalanceDamage(stats.dexterity * 6, player.gameObject.transform.position - gameObject.transform.position, true);
                        hitBlocked = true;
                        return false;
                    case 2:
                        player.TakeBalanceDamage(stats.dexterity * 4, player.gameObject.transform.position - gameObject.transform.position, true);
                        hitBlocked = true;
                        return false;
                    case 3:
                        player.TakeBalanceDamage(stats.dexterity * 2, player.gameObject.transform.position - gameObject.transform.position, true);
                        hitBlocked = true;
                        return false;
                    case 4:
                        player.TakeBalanceDamage(stats.dexterity * 1, player.gameObject.transform.position - gameObject.transform.position, true);
                        TakeBalanceDamage(balanceDMG - stats.dexterity, gameObject.transform.position - player.gameObject.transform.position);
                        hitBlocked = true;
                        return true;
                    case 5:
                        TakeBalanceDamage(balanceDMG, gameObject.transform.position - player.gameObject.transform.position);
                        hitBlocked = true;
                        return false;
                    default:
                        Debug.Log(gameObject.name + " parring value is over qualified value of 5.");
                        TakeDamage(dmg);
                        TakeBalanceDamage(balanceDMG, gameObject.transform.position - player.gameObject.transform.position);
                        return true; ;
                }
            }
            else if (invincible) { return true; }
            else
            {
                string threat = "";

                if (player.currentAttack)
                    threat += "attacked/";
                if (!player.currentAttack.blockable)
                    threat += "unBlockable/";
                if (player.currentAttack.isRanged)
                    threat += "ranged/";
                if (AmIInRange() && playerRange != 0)
                    threat += "inPlayerRange/";
                if (IsPlayerLookingAtMe())
                    threat += "lookingAtMe/";
                if (IsPlayerComingTowardsMe())
                    threat += "comingTowardsMe/";

                if (threat != "")
                    CreateADefensiveMemory(threat);

                Animator anim = weapon.GetComponent<Animator>();
                if (currentAttack != null)
                {
                    anim.SetTrigger("isHit");
                    currentAttack.AttackEnded();
                }
                PlayerController.current.currentAttack.attackHitSuccessfully = true;
                TakeDamage(dmg);
                TakeBalanceDamage(balanceDMG, transform.position - player.gameObject.transform.position);

                return true;
            }
        }
        else return false;
    }

    public void TakeDamage(int amount)
    {
        health = health - amount;
    }

    public void TakeBalanceDamage(int amount, Vector3 dir, bool isParry = false)
    {
        if (isParry)
        {
            if (amount > balance)
            {
                balance = 0;
            }
            else
            {
                balance = balance - amount;
            }
        }
        else
        {
            if (amount > balance)
            {
                balance = 0;
                float knockback = amount - balance;

                if (knockback < 20) StartCoroutine(BreakAndRegainBalance("minor"));
                else if (knockback < 40) StartCoroutine(BreakAndRegainBalance("medium"));
                else if (knockback >= 40) StartCoroutine(BreakAndRegainBalance("major"));

                Debug.Log(gameObject.name + " is taking " + knockback + " amount of knockback.");
                rb2.AddForce(dir * knockback * 25);
            }
            else
            {
                balance = balance - amount;
                Debug.Log(dir);
                rb2.AddForce(dir * 300);
            }
        }
    }

    public void ApplyStatusEffectsToSelf(int statusEffect, float duration)
    {
        statusEffects[statusEffect] = true;
    }

    private IEnumerator EffectStatusEffect(int statusEffect, float duration)
    {
        yield return new WaitForSeconds(duration);
        statusEffects[statusEffect] = false;
    }

    public void SetParrying(int newValue)
    {
        parrying = newValue;
    }

    //*************
    // TRY- METHODS
    // ************

    private void TakeAction(string sPlayerAction)
    {
        float range = 0;
        PlayerController player = PlayerController.current;
        switch (sPlayerAction)
        {
            case "attacking":

                bool isPlayerCharging = false;
                if (player.GetMainHandWeapon())
                    isPlayerCharging = player.GetMainHandWeapon().charging;
                if (currentAttack == null && AmIInRange() && IsPlayerComingTowardsMe() && (IsPlayerLookingAtMe() || isPlayerCharging))
                {
                    if (parrying == 0)
                    {
                        AddNewMyAction(2);
                        StartCoroutine(TryToParry());
                    }
                }
                else
                {
                    if (IsPlayerComingTowardsMe() && IsPlayerLookingAtMe())
                    {
                        AddNewMyAction(3);
                        StartCoroutine(TryToEvade());
                    }
                    MoveTowardsPlayer();
                    AttackAccordingToRange();
                }
                break;
            case "blocking":
                if (player.parrying < 5 && IsPlayerInRange(out range))
                {
                    if (currentAttack == null)
                    {
                        if (attackDic.ContainsKey("faint"))
                        {
                            StartToAttack("faint");
                        }
                        else
                        {
                            StartCoroutine(ChargeForTimeAndAttack(1));
                            AttackAccordingToRange();
                        }
                    }
                }
                else if (attackDic.ContainsKey("breakBalance") && IsPlayerInRange(out range))
                {
                    if (range == closeRange) StartToAttack("breakBalance");
                }
                else if (strategy == "defensive")
                {
                    Move(GetTacticalDirection());
                }
                else
                {
                    if (currentAttack == null)
                    {
                        if (strategy != "ranged")
                        {
                            TryToAttack("closeRange");
                        }
                    }
                    else if (currentAttack.chargingMultiplierBoost < currentAttack.maximumCharge)
                    {
                        AddNewMyAction(1);
                        currentAttack.InitiateCharge();
                        MoveTowardsPlayer();
                        Debug.Log("Charge boost: " + currentAttack.chargingMultiplierBoost);
                    }
                    else
                    {
                        currentAttack.StopCharging();
                        currentAttack.InitiateAttack();
                    }
                }
                break;
            case "missed":
                MoveTowardsPlayer();
                AttackAccordingToRange();
                break;
            case "charging":
                if (IsPlayerLookingAtMe())
                {
                    if(AmIInRange() || IsPlayerComingTowardsMe())
                    {
                        Move(GetTacticalDirection());
                        if (currentAttack == null)
                            AttackAccordingToRange();
                        else
                            TryToEvade();
                    }
                }
                else
                {
                    MoveTowardsPlayer();
                    AttackAccordingToRange();
                }
                break;
            case "balanceBroken":
                if (IsPlayerInRange(out range))
                    AttackAccordingToRange();
                else
                    dodge.DoDodge(GetPlayerDirection());
                break;
            default:
                if (weapon.charging)
                {
                    AttackAccordingToRange();
                    StopCharge();
                }

                if (strategy == "defensive")
                {
                    if (!IsPlayerLookingAtMe())
                    {
                        AttackAccordingToRange();
                    }
                    else
                    {
                        Move(GetTacticalDirection());
                        AttackAccordingToRange();
                    }
                }
                else
                {
                    MoveTowardsPlayer();
                    if (IsPlayerInRange(out range))
                    {
                        if (weapon.charging) StopCharge();
                        AttackAccordingToRange();
                    }
                    else if (currentAttack != null)
                    {
                        if (!currentAttack.attackInProcess) Charge();
                    }
                }
                break;
        }
    }

    private void TakeCounterAct(string threat)
    {
        Debug.Log(name + " is taking counter act to " + threat);
        PlayerController player = PlayerController.current;
        switch (threat)
        {
            case "attacking/unBlockable/ranged/inPlayerRange/lookingAtMe/":
                if (AmIInRange() && IsPlayerLookingAtMe() && player.currentAttack)
                    StartCoroutine(TryToEvade());
                break;
            case "attacking/unBlockable/inPlayerRange/lookingAtMe/":
                if (AmIInRange() && IsPlayerLookingAtMe() && player.currentAttack)
                    StartCoroutine(TryToEvade());
                break;
            case "attacked/inPlayerRange/lookingAtMe/comingTowardsMe/":
                if (AmIInRange() && IsPlayerLookingAtMe() && player.currentAttack && IsPlayerComingTowardsMe())
                {
                    TryToParry();
                }
                break;
            case "attacked/inPlayerRange/comingTowardsMe/":
                if (AmIInRange() && player.currentAttack && IsPlayerComingTowardsMe())
                {
                    TryToParry();
                }
                break;
            case "attacked/inPlayerRange/lookingAtMe/":
                if (AmIInRange() && IsPlayerLookingAtMe() && player.currentAttack)
                {
                    TryToEvade();
                    AttackAccordingToRange();
                }
                break;
            case "attacked/inPlayerRange/":
                if (AmIInRange() && player.currentAttack)
                {
                    StartCoroutine(TryToEvade());
                    AttackAccordingToRange();
                }
                break;
            case "attacked/lookingAtMe/":
                if (IsPlayerLookingAtMe() && player.currentAttack)
                {
                    Move(GetPlayerDirection());
                    AttackAccordingToRange();
                }
                break;
            case "attacked/":
                if (IsPlayerLookingAtMe() && player.currentAttack)
                    Move(GetTacticalDirection());
                break;
        }
    }

    private bool AttackAccordingToRange()
    {
        float range = 0;
        if (IsPlayerInRange(out range))
        {
            if (range == closeRange)
            {
                TryToAttack("closeRange");
                return true;
            }
            else if (attackDic.ContainsKey("mediumRange"))
            {
                if (range == mediumRange)
                {
                    TryToAttack("mediumRange");
                    return true;
                }
            }
            else if (attackDic.ContainsKey("longRange"))
            {
                if (range == -1)
                {
                    TryToAttack("LongRange");
                    return true;
                }
            }
        }
        return false;
    }

    private void TryToAttack(string attackType)
    {
        AddNewMyAction(0);
        if (currentAttack)
            return;

        if (comboCount <= currentComboLenght)
        {
            AttackBase attack = StartToAttack(attackType);
            StartCoroutine(attack.InitiateAttack());
        }
    }

    private IEnumerator TryToEvade()
    {
        if (PlayerController.current.currentAttack && !doingAction)
        {
            doingAction = true;
            while (AmIInRange() || PlayerController.current.currentAttack)
            {
                if (dodge.dodgeDistance < GetPlayerDistance() && dodge.bCanDodge)
                {
                    dodge.DoDodge(-GetPlayerDirection());
                    Move(-GetPlayerDirection(), 2);
                }
                Move(GetTacticalDirection(), 1.5f);

                yield return new WaitForEndOfFrame();
            }
            StopCoroutine(TryToEvade());
            doingAction = false;
        }
    }

    private IEnumerator Faint()
    {
        yield return new WaitUntil(() => currentAttack != null);
        if (currentAttack)
            currentAttack.animator.SetTrigger("cancel");
    }

    private IEnumerator TryToParry()
    {
        if (!isBalanceBroken && currentAttack == null && !doingAction)
        {
            AddNewMyAction(2);
            StartCoroutine(weapon.Parry());
            yield return new WaitWhile(() => parrying != 0);
        }
    }

    private bool isItOkayToChooseANewDir = true;
    private Vector2 lastDirChosen = Vector2.zero;
    private Vector2 GetDirectionWithLongestDistance()
    {
        if (isItOkayToChooseANewDir)
        {
            
            Vector2 rayUpDir = transform.up;
            Vector2 rayAwayFromDir = -GetPlayerDirection();
            Vector2 rayRightDir = transform.right;
            Vector2 rayDownDir = -transform.up;
            Vector2 rayLeftDir = -transform.right;

            RaycastHit2D rayAwayFromPlayer = Physics2D.Raycast(transform.position, rayAwayFromDir);
            RaycastHit2D rayRight = Physics2D.Raycast(transform.position, rayRightDir, float.PositiveInfinity, rayFilter);
            RaycastHit2D rayDown = Physics2D.Raycast(transform.position, rayDownDir, float.PositiveInfinity, rayFilter);
            RaycastHit2D rayLeft = Physics2D.Raycast(transform.position, rayLeftDir, float.PositiveInfinity, rayFilter);

            float[] lenghts = new float[4] { rayAwayFromPlayer.distance, rayRight.distance, rayDown.distance, rayLeft.distance };
            float longest = Mathf.Max(lenghts);

            int i = 0;
            foreach (float lenght in lenghts)
            {
                Debug.Log("Raycast lenght " + i + ": " + lenght);
                i++;
            }

            if (longest == rayAwayFromPlayer.distance)
            {
                isItOkayToChooseANewDir = false;
                StartCoroutine(GetDirectionWithLongestDistanceCooldown());
                lastDirChosen = rayAwayFromDir;
                return rayAwayFromDir;
            }
            else if (longest == rayRight.distance)
            {
                if (rayRight.distance >= rayAwayFromPlayer.distance * 3)
                {
                    isItOkayToChooseANewDir = false;
                    StartCoroutine(GetDirectionWithLongestDistanceCooldown());
                    lastDirChosen = rayRightDir;
                    return rayRightDir;
                }
            }
            else if (longest == rayDown.distance)
            {
                if (rayDown.distance >= rayAwayFromPlayer.distance * 3)
                {
                    isItOkayToChooseANewDir = false;
                    StartCoroutine(GetDirectionWithLongestDistanceCooldown());
                    lastDirChosen = rayDownDir;
                    return rayDownDir;
                }
            }
            else if (longest == rayLeft.distance)
            {
                if (rayLeft.distance >= rayAwayFromPlayer.distance * 3)
                {
                    isItOkayToChooseANewDir = false;
                    StartCoroutine(GetDirectionWithLongestDistanceCooldown());
                    lastDirChosen = rayLeftDir; 
                    return rayLeftDir;
                }
            }
            else
            {
                isItOkayToChooseANewDir = false;
                StartCoroutine(GetDirectionWithLongestDistanceCooldown());
                lastDirChosen = rayAwayFromDir;
                return rayAwayFromDir;
            }

            Debug.Log(name + " TryToBackOff() return unknown number.");
            return Vector3.zero;
        }
        return lastDirChosen;
    }
    public Vector3 GetTacticalDirection()
    {
        Vector3 parentPosition = transform.position;
        float shortestDistance = float.PositiveInfinity;
        Vector3 closestAlly = new Vector3();

        List<EnemyBaseController> allies = GetAllLivingAlliesInRoom();
        if (allies.Count > 0)
        {
            foreach (EnemyBaseController ally in allies)
            {
                if (ally != this)
                {
                    float distance = (parentPosition - ally.transform.position).magnitude;
                    if (distance < shortestDistance)
                    {
                        closestAlly = ally.transform.position;
                        shortestDistance = distance;
                    }
                }
            }
            if (shortestDistance > DeterminePlayerWeaponLenght() * 2.5f)
            {
                return transform.right;
            }
            else
            {
                Vector2 dir = parentPosition - closestAlly;
                Debug.DrawLine(closestAlly, parentPosition);
                return dir;
            }
        }
        else return GetDirectionWithLongestDistance();
        
    }

    private IEnumerator GetDirectionWithLongestDistanceCooldown()
    {
        yield return new WaitForSeconds(0.5f / difficulty);
        isItOkayToChooseANewDir = true;
    }

    private AttackBase StartToAttack(string attackType)
    {
        List<AttackBase> attacks = attackDic[attackType];
        currentComboLenght = attacks.Count - 1;
        if(currentComboLenght < 0)
        {
            Debug.Log("Attack type: " + attackType + " of object " + name + " is empty.");
            return null;
        }
        else if(comboCount > currentComboLenght)
        {
            comboCount = 0;
        }

        AttackBase attack = attacks[comboCount];
        attack.InitiateStartAnimation();
        currentAttack = attack;

        return attack;
    }

    private IEnumerator ChargeForTimeAndAttack(float time)
    {
        Charge();
        yield return new WaitForSeconds(time);
        if (weapon.charging)
            AttackAccordingToRange();
        StopCharge();
    }

    private void Charge() //If return false, charging has been cancelled.
    {
        AddNewMyAction(1);
        if (currentAttack) currentAttack.InitiateCharge();
    }

    private void StopCharge()
    {
        if (currentAttack) currentAttack.StopCharging();
    }

    private void MoveTowardsPlayer()
    {
        AddNewMyAction(5);
        Move(GetPlayerDirection());
    }

    public void Alert()
    {
        if (!room.activeSelf) return;

        try
        {
            List<EnemyBaseController> allies = GetAllLivingAlliesInRoom();
            foreach (EnemyBaseController ally in allies)
            {
                if (!ally.CanPlayerBeSeen()) ally.transform.up = ally.GetPlayerDirection();
            }
        }
        catch (UnassignedReferenceException)
        {
            return;
        }
    }

    private IEnumerator ParryAndCounter()
    {
        StartCoroutine(TryToParry());
        yield return new WaitUntil(() => parrying == 1);
        yield return new WaitUntil(() => parrying == 0);
        AttackAccordingToRange();
    }

    // ****************** AI Judging stuff ******************

    private void DidPlayerHit()
    {
        if (isPlayerSeen) StartCoroutine(EnumDidPlayerHit());
    }

    private IEnumerator EnumDidPlayerHit()
    {
        yield return new WaitUntil(() => (IsHitThisFrame || player.GetComponent<PlayerController>().GetCurrentAttack() != null));
        if (hitBlocked && IsHitThisFrame) TakeAction("playerHasBennParried");
        if (!IsHitThisFrame) TakeAction("missed");

        hitBlocked = false;
    }


    private bool IsPlayerInRange(out float detailedResult)
    {
        // float playerDistance = GetPlayerDistance();
        Vector2 predictedPoint = PredictNextPlayerPositionThroughVelocity();
        float playerDistance = Vector3.Distance(PredictMyNextPositionThroughVelocity(), predictedPoint);

        if (attackDic.ContainsKey("closeRange"))
        {
            if (playerDistance < closeRange)
            {
                detailedResult = closeRange;
                return true;
            }
        }
        if (attackDic.ContainsKey("mediumRange"))
        {
            if (playerDistance < mediumRange)
            {
                detailedResult = mediumRange;
                return true;
            }
        }
        if (attackDic.ContainsKey("longRange"))
        {
            if (playerDistance > mediumRange)
            {
                detailedResult = -1;
                return true;
            }
        }
        detailedResult = 0;
        return false;
    }

    private bool AmIInRange()
    {
        return GetPlayerDistance() < playerRange;
    }

    private bool IsPlayerComingTowardsMe()
    {
        if (PlayerController.current.rb2d.velocity.normalized == GetPlayerDirection().normalized)
        {
            return true;
        }
        else
            return false;
    }

    private IEnumerator DeterminePlayerRange()
    {
        if (bDeterminePlayerRange)
        {
            PlayerController player = PlayerController.current;
            bDeterminePlayerRange = false;

            if (player.currentAttack)
            {
                if (player.currentAttack.isRanged)
                {

                }
                else
                {
                    Vector3 startingPosition = player.gameObject.transform.position;
                    yield return new WaitUntil(() => player.currentAttack == null);
                    Vector3 endPosition = player.gameObject.transform.position;
                    float range = Vector3.Distance(startingPosition, endPosition);

                    if (playerRange < range)
                        playerRange = range;
                    bDeterminePlayerRange = true;
                }
            }
        }
    }

    private float DeterminePlayerWeaponLenght()
    {
         WeaponBase mainWeapon = PlayerController.current.GetMainHandWeapon();
         WeaponBase offWeapon = PlayerController.current.GetOffHandWeapon();
         float mainWeaponLenght = 0f;
         float offWeaponLenght = 0f;

         if (mainWeapon)
         {
            if (mainWeapon.isVisible) mainWeaponLenght = mainWeapon.gameObject.transform.lossyScale.y;
         }
         if (offWeapon)
         {
            if (offWeapon.isVisible) offWeaponLenght = offWeapon.gameObject.transform.lossyScale.y;
         }
         if (mainWeaponLenght < offWeaponLenght) return offWeaponLenght;
         else return mainWeaponLenght;
    }

    private bool CanPlayerBeSeen()
    {
        Vector3 dir = GetPlayerDirection();
        float angle = Vector3.Angle(dir, head.transform.up);
        if (angle <= 100)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, float.PositiveInfinity, rayFilter);
            if (hit)
            {
                if (hit.collider.CompareTag(player.tag))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsPlayerLookingAtMe()
    {
        float angle = Vector2.Angle(player.transform.up, -GetPlayerDirection());
        if (angle <= 20)
        {
            return true;
        }
        else return false;
    }
    // ACTIONS
    // -1 = doing nothing, 0 = attacking, 1 = charging, 2 = parrying, 3 = dodging, 4 = backing off, 5 = trying to get close, 6 = missing, 7 = balanceBroken, 8 = getting hit
    // MOMENTS
    // 0 = within player range,
    private string WhatIsPlayerDoing()
    {
        PlayerController player = PlayerController.current;
        Animator anim = player.gameObject.GetComponent<Animator>();
        if (player.GetMainHandWeapon().charging)
        {
            AddNewPlayerAction(1);
            return "charging";
        }
        else if (player.currentAttack)
        {
            return "attacking";
        }
       else if (player.parrying != 0)
        {
            AddNewPlayerAction(2);
            return "blocking";
        }
       else if (player.isBalanceBroken)
        {
            AddNewPlayerAction(7);
            return "balanceBroken";
        }
       else if (player.currentAttack == hasBeenHit)
        {
            return "hit";
        }
        else if (IsPlayerComingTowardsMe())
        {
            AddNewPlayerAction(5);
            return "comingTowardsMe";
        }
        else
        {
            AddNewPlayerAction(-1);
            return "nothing";
        }
    }

    private Vector2 PredictNextPlayerPositionThroughVelocity(float predictedTime = 0.2f)
    {
        Vector2 velocity = player.GetComponent<Rigidbody2D>().velocity;
        Vector2 predictedPoint = new Vector2(player.transform.position.x, player.transform.position.y) + velocity * predictedTime;
        return predictedPoint;
    }

    private Vector2 PredictMyNextPositionThroughVelocity(float predictTime = 0.2f)
    {
        Vector2 velocity = rb2.velocity;
        Vector2 predictedPoint = new Vector2(transform.position.x, transform.position.y) + velocity * predictTime;
        return predictedPoint;
    }

    private void CreateADefensiveMemory(string threat)
    {
        Debug.Log(this.gameObject.name + " is creating a defensive memory.");

        Dictionary<Dictionary<string, int>, string> memory = new Dictionary<Dictionary<string, int>, string>() { { playerAction, threat } };
        memories.Add(memory);
    }

    private Vector2 WhatDirectionIsPlayerGoing()
    {
        return rb2.velocity;
    }

    // ****************** Getters and functions ******************

    private void PlayerAttackEndEvents() { hasBeenHit = null; }
    public float GetMaxHealth() { return maxHealth;  }
    public float GetMaxBalance() { return maxBalance; }
    public float GetPlayerDistance()
    {
        Vector2 playerPos = PredictNextPlayerPositionThroughVelocity();
        Vector2 thisPos = PredictMyNextPositionThroughVelocity();
        float distance = Vector2.Distance(playerPos, thisPos);

        return distance;
    }

    public void NextInComboOrder() { comboCount += 1; }

    public void SetCurrentAttack(AttackBase newValue) { currentAttack = newValue; }

    private void FacePlayer()
    {
        Vector2 facingDirenction = GetPlayerDirection();
        transform.up = facingDirenction;
    }

    public Vector2 GetPlayerDirection()
    {
        Vector2 playerPosition = PlayerController.current.transform.position;
        Vector2 playerDirection = new Vector2(playerPosition.x - transform.position.x,
                                               playerPosition.y - transform.position.y);
        return playerDirection;
    }

    public void SetFacePlayer(bool newValue) { facePlayer = newValue; }

    public Dictionary<int, int> GetStats()
    {
        Dictionary<int, int> returnStats = new Dictionary<int, int>
        {
            {0, stats.vigor },
            {1,stats.endurance},
            {2, stats.strenght },
            {3, stats.dexterity },
            {4, stats.intelligence },
            {5, stats.magic },
            {6, stats.spirit }
        };
        return returnStats;
    }

    public List<EnemyBaseController> GetAllLivingAlliesInRoom()
    {
        List<EnemyBaseController> allies = new List<EnemyBaseController>();
        if (!room.activeSelf) return null;
        foreach (Transform enemyTransform in room.GetComponent<RoomRenderingScript>().children)
        {
            EnemyBaseController enemy = null;
            if (enemyTransform != null)
            {
                if (enemyTransform.TryGetComponent<EnemyBaseController>(out enemy))
                {
                    allies.Add(enemy);
                }
            }
        }
        if (allies.Count == 0) return null;
        else return allies;
    }

    private void AddNewMyAction(int action)
    {
        myAction["action"] = action;
    }

    private void AddNewPlayerAction(int action)
    {
        playerAction["action"] = action;
    }

    private string CompareMemoryToTheMoment()
    {
        foreach (Dictionary<Dictionary<string, int>, string> memory in memories)
        {
            if (memory.ContainsKey(playerAction))
                return memory[playerAction];
        }
        return null;
    }
}
