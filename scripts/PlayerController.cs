using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject interactor;
    public PlayerUI UI;
    public GameObject menu;
    private DodgeBase dodge = null;

    public Rigidbody2D rb2d = null;
    public bool doingAction = false;
    public AttackBase currentAttack = null;
    public float moveSpeed = 3;
    public GameObject playerCamera = null;
    private Vector3 lastMoveDir = new Vector3(0, 0, 0);
    public static PlayerController current = null;
    public bool faceMouse = true;
    public bool canMove = true;

    private float maxHealth = 300;
    public float health = 300;

    private float maxStamina = 100;
    public float stamina = 100;
    public float staminaRegen = 1f;
    public float staminaRegenTimer = 3;
    public bool bStaminaRegen = true;
    private bool bCompleteStaminaExhaustion = false;

    private float maxBalance = 100;
    public float balance = 100;
    public float balanceRegen = 0.05f;
    public bool isBalanceBroken = false;

    public int comboOrder = 0;
    public int maxComboOrder = 0;

    public bool invincible = false;
    public int parrying = 0;

    private Save save = new Save();
    private float parryingWindowBonus = 0f;
    private WeaponBase mainHand = null;
    private WeaponBase offHand = null;
    private WeaponBase bodyWeapon = null;
    public WeaponBase blockingHand = null;
    public float parryCost = 15;

    private float attackDamageMultiplier = 0f;
    private float attackBalanceDamageMultiplier = 0f;
    private float chargingPerClick = 0f;

    public List<AttackBase> attackList = new List<AttackBase>()
                                                                    {
                                                                    
                                                                    };
    

    private Dictionary<int, bool> statusEffects = EventSystem.GetStatusEffectBools();

    void Awake()
    {
        if (current)
        {
            Destroy(this.gameObject);
        }
        else
        {
            current = this;
            DontDestroyOnLoad(this.gameObject);
        }
        if (EventSystem.currentSave != null) save = EventSystem.currentSave;
        else
        {
            save = new Save();
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.onPlayerAttack += ResetStaminaRegen;
        EventSystem.current.onPlayerAttackEnd += ResetStaminaRegen;

        EventSystem.currentSave.inventory = new List<int>() { 00 };

        if (dodge == null)
        {
            dodge = gameObject.AddComponent<DodgeBase>();
            dodge.SetDodge("dash", 10, true);
        }
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (!rb2d)
        {
            Debug.Log("Missing rigidbody 2D component");
        }
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (EventSystem.currentSave != null)
        {
            Save save = EventSystem.currentSave;
            maxHealth = 300 + save.vigor * 10;
            health = maxHealth;

            maxBalance = save.vigor * 10;
            balance = maxBalance;

            maxStamina = save.endurance * 10;
            stamina = maxStamina;
        }
        if (playerCamera == null) playerCamera = Camera.main.gameObject;

        WeaponBase[] weapons = GetComponentsInChildren<WeaponBase>();
        foreach(WeaponBase weapon in weapons)
        {
            if (weapon.gameObject.name == "mainWeapon")
                mainHand = weapon;
            else if (weapon.gameObject.name == "bodyWeapon")
                bodyWeapon = weapon;
        }

        mainHand.CreateANewWeaponBase(false, "sword", false, 10, false, "strenght", .1f, 10, "strenght", 0.1f);
        blockingHand = mainHand;

        AttackBase attack = mainHand.gameObject.AddComponent<AttackBase>();
        List<int> damageList = new List<int>() {2, 3};
        List<int> balanceDamageList = new List<int>() { 2 };
        List<string> specials = new List<string>() {  };
        attack.CreateANewAttackBase(true, 1, 1, 0.1f, mainHand.GetComponent<Animator>(), "swing_rightSide_leftSide", "mouse 0",damageList, balanceDamageList, 15, 0, 50, 0, true, specials);

        attack = mainHand.gameObject.AddComponent<AttackBase>();
        damageList = new List<int>() { 2, 3 };
        balanceDamageList = new List<int>() { 2 };
        attack.CreateANewAttackBase(true, 1, 1, 0.1f, mainHand.GetComponent<Animator>(), "swing_leftSide_rightSide", "mouse 0", damageList, balanceDamageList, 15, 1, 50, 0);

        AttackBase kick = bodyWeapon.gameObject.AddComponent<AttackBase>();
        damageList = new List<int>() { 2, 3 };
        balanceDamageList = new List<int>() { 5 };
        List<string> specials2 = new List<string>() { "moveMainHandToRightSide" };
        kick.CreateANewAttackBase(true, 1, 5, 0.2f, bodyWeapon.gameObject.GetComponent<Animator>(), "kick_any_rightSide", "mouse 1", damageList, balanceDamageList, 15, -1, 0, -1, false , specials2);
    }


    void Update()
    {
        if (menu.activeSelf)
        {
            canMove = false;
            doingAction = true;
            if (Input.GetKeyDown(KeyCode.Escape))
                menu.SetActive(false);
        }
        else
        {
            canMove = true;
            doingAction = false;
            if (Input.GetKeyDown(KeyCode.Escape))
                menu.SetActive(true);
        }

        if (stamina >= 0) bCompleteStaminaExhaustion = true;
        if (bStaminaRegen && stamina < 100)
        {
            if (staminaRegen + stamina > 100) stamina = 100;
            else stamina += staminaRegen;
        }

        if (balance < maxBalance)
        {
            balance += balanceRegen;
            if (rb2d.velocity == Vector2.zero)
                balance += 1;
        }

        if (Input.GetKeyDown(save.interactKey))
        {
            Interact();
        }

        if (!doingAction)
        {
            //put idleanimation here (playerAnimationBase.PlayIdleAnimation(lastMoveDir))
        }
        else
        {
            lastMoveDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        if (mainHand != null)
        {
            if (mainHand.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("idle") && !mainHand.GetComponent<Animator>().IsInTransition(0))
            {
                ResetComboOrder();
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isBalanceBroken && canMove)
        {
            Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            float movementX = movement.x - rb2d.velocity.x;
            float movementY = movement.y - rb2d.velocity.y;
            rb2d.AddForce(new Vector2(movementX * moveSpeed, movementY * moveSpeed));

            if (faceMouse) FaceMouse();
        }
    }

    public void SetFaceMouse(bool newValue) { faceMouse = newValue; }
    public AttackBase GetCurrentAttack() { return currentAttack; }
    public void SetCurrentAttack(AttackBase newAttack) { currentAttack = newAttack; }
    public bool GetDoingAction() { return doingAction; }
    public void SetDoingAction(bool newValue) { doingAction = newValue; }
    public void SetParrying(int newValue) { parrying = newValue; }
    public float GetParryingWindowBonus() { return parryingWindowBonus; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetMaxBalance() { return maxBalance; }
    public float GetMaxStamina() { return maxStamina; }

    private void FaceMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 facingDirenction = new Vector2(mousePosition.x - transform.position.x,
                                               mousePosition.y - transform.position.y);
        transform.up = facingDirenction;
    }

    void Interact()
    {
        Instantiate(interactor, this.gameObject.transform);
    }

    public void AssignAttack(string attackName, AttackBase attackSlot, string key)
    {
        switch (attackName)
        {
            default:
                Debug.Log("No attack found with search" + attackName + ".");
                break;
            case "swing_RightSide_LeftSide":
                AttackBase attack = gameObject.AddComponent<AttackBase>();
                List<int> damageList = new List<int>() { 2, 3 };
                List<int> balanceDamageList = new List<int>() { 2 };
                attack.CreateANewAttackBase(true, 1, 1, 0.1f, mainHand.GetComponent<Animator>(), "swing_rightSide_leftSide", "mouse 0", damageList, balanceDamageList, -1);
                attackSlot = attack;
                break;
        }
    }
    public void AssignWeapon(WeaponBase newWeapon)
    {
        if (newWeapon.twoHanded)
        {
            mainHand = newWeapon;
            offHand = null;
        }
        newWeapon = mainHand;
    }

    public bool IsHit(EnemyBaseController attacker, float dmg, float balanceDMG)
    {
        if (!attacker.hit)
        {
            attacker.hit = true;
            Animator animator = mainHand.GetComponent<Animator>();
            animator.SetTrigger("isHit");

            if (currentAttack)
            {
                if (currentAttack) currentAttack.IsHit();
            }
            if (parrying > 0 && !isBalanceBroken)
            {
                switch (parrying)
                {
                    case 1:
                        attacker.TakeBalanceDamage(save.dexterity * 6, gameObject.transform.position - attacker.gameObject.transform.position, true);
                        blockingHand.GetComponent<Animator>().SetBool("parry", false);
                        attacker.currentAttack.IsHit();
                        return false;
                    case 2:
                        attacker.TakeBalanceDamage(save.dexterity * 4, gameObject.transform.position - attacker.gameObject.transform.position, true);
                        blockingHand.GetComponent<Animator>().SetBool("parry", false);
                        return false;
                    case 3:
                        attacker.TakeBalanceDamage(save.dexterity * 2, gameObject.transform.position - attacker.gameObject.transform.position, true);
                        blockingHand.GetComponent<Animator>().SetBool("parry", false);
                        stamina -= 15;
                        ResetStaminaRegen();
                        return false;
                    case 4:
                        attacker.TakeBalanceDamage(save.dexterity * 1, gameObject.transform.position - attacker.gameObject.transform.position, true);
                        blockingHand.GetComponent<Animator>().SetBool("parry", false);
                        TakeBalanceDamage(balanceDMG - save.dexterity, attacker.transform.position - transform.position);
                        stamina -= 15;
                        ResetStaminaRegen();
                        return true;
                    case 5:
                        if (mainHand.type == "shield")
                        {
                            TakeBalanceDamage(Mathf.RoundToInt(balanceDMG / mainHand.blockingEffiency), gameObject.transform.position - attacker.gameObject.transform.position);
                            return false;
                        }
                        else if (offHand)
                        {
                            if (offHand.type == "shield")
                            {
                                TakeBalanceDamage(Mathf.RoundToInt(balanceDMG / offHand.blockingEffiency), gameObject.transform.position - attacker.gameObject.transform.position);
                                return false;
                            }
                        }
                        TakeBalanceDamage(balanceDMG, transform.position - attacker.transform.position);
                        stamina -= 15;
                        ResetStaminaRegen();
                        return true;
                    default:
                        Debug.Log("Player parring value is over qualified value of 5.");
                        TakeDamage(dmg);
                        TakeBalanceDamage(balanceDMG, transform.position - attacker.transform.position);
                        return true;
                }
            }
            else if (invincible) { return true; }
            else
            {
                playerCamera.GetComponent<FollowPlayer>().CameraKnockback(transform.position - attacker.transform.position, balanceDMG);

                Animator anim = mainHand.GetComponent<Animator>();
                if (currentAttack != null && currentAttack.attackInProcess)
                {
                    anim.SetTrigger("isHit");
                    currentAttack.AttackEnded();
                }
                TakeDamage(dmg);
                if (!isBalanceBroken) TakeBalanceDamage(balanceDMG, transform.position - attacker.transform.position);
                return true;
            }
        }
        return false;
    }

    public void TakeDamage(float amount)
    {
        health = health - amount;
    }

    public void TakeBalanceDamage(float amount, Vector3 dir, bool isParry = false)
    {
        if (amount > balance)
        {
            float knockback = amount - balance;
            Debug.Log("Knocback: " + knockback);
            balance = 0;
            if (knockback < 0) knockback = 0;

            if (knockback < 20) StartCoroutine(BreakAndRegainBalance("minor"));
            else if (knockback < 40) StartCoroutine(BreakAndRegainBalance("medium"));
            else if (knockback >= 40) StartCoroutine(BreakAndRegainBalance("major"));

            if (!isParry)
                rb2d.velocity = Vector2.zero;
            else
                rb2d.AddForce(dir * (knockback * 30));
            Debug.Log("Player was hit from: " + dir + "with the velocity of: " + rb2d.velocity);
        }
        else
        {
            if (isParry)
                balance -= amount;
            else
            {
                float knockback = amount * 30;

                balance = balance - amount;
                rb2d.AddForce(dir * knockback);
            }
        }
    }
    
    private IEnumerator BreakAndRegainBalance(string hitSeverity)
    {
        Debug.Log("Player balance broken.");

        if (!isBalanceBroken)
        {
            UI.ShowStunnedText();
            isBalanceBroken = true;
            balance = 100;
            Camera.main.GetComponent<Animator>().SetTrigger("shake");

            if (hitSeverity == "minor") yield return new WaitForSeconds(0.8f);
            else if (hitSeverity == "medium") yield return new WaitForSeconds(1.2f);
            else if (hitSeverity == "major") yield return new WaitForSeconds(1.8f);
            isBalanceBroken = false;
            UI.HideStunnedText();
        }
    }

    public void ApplyStatusEffectsToPlayer(int statusEffect, float duration)
    {
        statusEffects[statusEffect] = true;
    }

    private IEnumerator EffectStatusEffect(int statusEffect, float duration)
    {
        yield return new WaitForSeconds(duration);
        statusEffects[statusEffect] = false;
    }

    public int GetComboOrder()
    {
        return comboOrder;
    }

    public void NextInComboOrder()
    {
        comboOrder += 1;
    }
    public void ResetComboOrder()
    {
        comboOrder = 0;
    }


    public Dictionary<int, int> GetStats()
    {
        Dictionary<int, int> returnStats = new Dictionary<int, int>
        {
            {0, save.vigor },
            {1,save.endurance},
            {2, save.strenght },
            {3, save.dexterity },
            {4, save.intelligence },
            {5, save.magic },
            {6, save.spirit }
        };
        return returnStats;
    }
    public WeaponBase GetMainHandWeapon() { return mainHand; }
    public WeaponBase GetOffHandWeapon() { return offHand; }

    private int callTimes = 0;
    public void ResetStaminaRegen()
    {
        StartCoroutine(RestartStaminaRegenTimer());
    }
    public IEnumerator RestartStaminaRegenTimer()
    {
        bStaminaRegen = false;
        callTimes++;
        int i = callTimes;
        if (bCompleteStaminaExhaustion)
        {
            yield return new WaitForSeconds(staminaRegenTimer * 1.5f);
        }
        else
        {
            yield return new WaitForSeconds(staminaRegenTimer);
        }
        if (i == callTimes) bStaminaRegen = true;
    }

    public void Teleport(Vector3 teleportPosition, string teleportElement, float maxDistance)
    {
        // TO-DO add teleport effects
        switch (teleportElement)
        {
            case "physical":
                int layerMask = LayerMask.GetMask("EdgeCollider");
                float distance = Vector3.Distance(transform.position, teleportPosition);

                if (distance > maxDistance) distance = maxDistance;
                Debug.Log("Distance: " + distance);

                Vector2 dir = (teleportPosition - transform.position).normalized;
                Debug.Log("Dir: " + dir);

                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, layerMask);
                Debug.Log("Point: " + hit.point);
                if (hit.collider)
                {
                    Debug.Log("Collider: " + hit.collider.name);
                    transform.position = hit.point - dir *0.1f;
                }
                else
                {
                    // if (distance < 1) distance = 1;
                    transform.position = new Vector2(transform.position.x + dir.x * distance, transform.position.y + dir.y * distance);
                }
                break;
        }
    }

    public void Grab(Vector3 grabPosition, GameObject target)
    {
        target.transform.position = grabPosition;
    }

    public void UpdateEquipment()
    {
        int i = 0;

        InventorySlot[] equipment = InventoryScript.current.GetComponentsInChildren<InventorySlot>();
        foreach (InventorySlot slot in equipment)
        {
            if (slot.isEquiped && slot.item != null)
            {
                if (i == 0) // mainHand slot so must be weaponry.
                {
                    if (mainHand.GetComponent<WeaponBase>())
                    {
                        if (mainHand == blockingHand)
                            blockingHand = null;
                        Destroy(mainHand.GetComponent<WeaponBase>());
                    }

                    mainHand = slot.item.GetWeaponry(mainHand.gameObject, false);
                    Debug.Log("blocking hand: " + blockingHand);
                    if (!blockingHand)
                        blockingHand = mainHand;
                }
            }
            i++;
        }
    }
}