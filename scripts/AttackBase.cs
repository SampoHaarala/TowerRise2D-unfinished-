using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AttackBase : MonoBehaviour
{
    private float dmgMultiplier = 1f;
    private float balanceDamageMultiplier = 1f;
    public int staminaCost = 10;

    public Animator animator;
    private bool isPlayer;
    private EnemyBaseController enemy;
    public AttackSpecials specials;
    public bool blockable = true;
    public bool isRanged = false;

    public bool attackHitSuccessfully = false;

    public string attackAnimationName;
    private string startAnimationName;
    private string holdAnimationState;
    private string chargeAnimationState;
    private string endAnimationState;

    public string key;

    public float chargingMultiplierBoost = 0f;
    public float chargingAmountPerClick = 0.05f;
    private bool chargeClick = true;
    public float maximumCharge = 1f;

    private bool stepBool = true;
    public float stepDistance = 0f;

    public List<int> damageScalingStats;
    public List<int> balanceDamageScalingStats;

    public bool attackInProcess = false;
    public int comboOrder = -1;
    private bool doOncePerSwing = false;

    public int slotNumber = -1;

    public Animator GetAnimator() { return animator; }

    public AttackBase CreateANewAttackBase(bool isPlayer, float dmgMultiplier, float balancedDmgmultiplier, float chargingAmountPerClick,
                                                  Animator animator, string attackAnimationName, string key,
                                                  List<int> damageScalingStats, List<int> balanceDamageScalingStats, int staminaCost, int comboOrder = -1, float stepDistance = 0, int slotNumber = -1, bool blockable = true, List<string> specials = null, bool isRanged = false)
    {
        this.DefineDamageMultiplier(dmgMultiplier);
        this.DefineIsPlayer(isPlayer);
        this.DefineBalanceDamageMultiplier(balancedDmgmultiplier);
        this.DefineChargeBoostPerClickAmount(chargingAmountPerClick);
        this.DefineAnimator(animator);
        this.DefineAttackAnimation(attackAnimationName);
        string[] animationParts = attackAnimationName.Split('_');
        string holdAnimation = "hold_" + animationParts[1];
        string startAnimation = "move_idle_" + animationParts[1];
        chargeAnimationState = "charge_" + animationParts[1];
        string endAnimation = "hold_" + animationParts[2];
        this.DefineStartAnimation(startAnimation);
        this.DefineHoldAnimationState(holdAnimation);
        this.DefineEndAnimationState(endAnimation);
        this.DefineKeyCodeValue(key);

        if (comboOrder != -1 && isPlayer) PlayerController.current.maxComboOrder += 1;
        this.DefineComboOrder(comboOrder);

        this.DefineDamageScalingStats(damageScalingStats);
        this.DefineBalanceDamageScalingStats(balanceDamageScalingStats);
        this.stepDistance = stepDistance;
        this.staminaCost = staminaCost;
        this.blockable = blockable;

        PlayerController.current.attackList.Add(this);

        if (specials != null)
        {
            this.specials = AttackSpecials.CreateANewAttackSpecials(this, specials, true, new Vector3(0, 1));
        }
        return this;
    }

    void Update()
    {
        if (isPlayer)
        {
            if (PlayerController.current.GetCurrentAttack() == this && animator.GetCurrentAnimatorStateInfo(0).IsName(endAnimationState))
            {
                PlayerController.current.SetCurrentAttack(null);
                EventSystem.current.PlayerAttackEnd();
                chargingMultiplierBoost = 0;
            }
            PlayerController player = PlayerController.current;
            if (!player.isBalanceBroken && !player.doingAction && player.stamina > 0)
            {
                if (Input.GetKey(key) && player.stamina > 0)
                {
                    InitiateCharge();
                }
                else
                {
                    StopCharging();
                }

                if (Input.GetKeyDown(EventSystem.currentSave.cancelingKey))
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName(chargeAnimationState) && !animator.IsInTransition(0)) Cancel();
                }
                else
                {
                    if (Input.GetKeyDown(key))
                    {
                        if (comboOrder != -1)
                        {
                            if (player.comboOrder == comboOrder)
                            {
                                if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle") || animator.GetBool("parry") && !animator.IsInTransition(0)) animator.SetTrigger(startAnimationName);
                            }
                        }
                    }
                    if (Input.GetKeyUp(key) && !animator.GetBool(attackAnimationName))
                    {
                        if (comboOrder != -1)
                        {
                            if (player.comboOrder == comboOrder &&
                                !animator.GetNextAnimatorStateInfo(0).IsName(attackAnimationName))
                            {
                                if (player.currentAttack == null)
                                {
                                    ButtonUpAction();
                                    if (player.stamina <= 0)
                                    {
                                        Reset();
                                        return;
                                    }
                                    Debug.Log(chargingMultiplierBoost);

                                    animator.SetTrigger(attackAnimationName);
                                    InitiateAttack();

                                    EventSystem.current.PlayerAttack();

                                    StartCoroutine(TakeStep());
                                }
                            }
                        }
                        else
                        {
                            if (player.currentAttack == null)
                            {
                                ButtonUpAction();
                                if (player.stamina <= 0)
                                {
                                    Reset();
                                    return;
                                }
                                Debug.Log(chargingMultiplierBoost);

                                animator.SetBool("parry", false);
                                animator.SetTrigger(attackAnimationName);

                                EventSystem.current.PlayerAttack();

                                StartCoroutine(TakeStep());
                            }
                        }
                    }
                }
            }
            
        }
        else
        {
            if (enemy.comboCount == comboOrder && animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimationName))
            {
                attackInProcess = true;
            }
            else
            {
                if (attackInProcess)
                {
                    enemy.hit = false;
                    enemy.NextInComboOrder();
                    enemy.currentAttack = null;
                }
                attackInProcess = false;
            }
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                Reset();
            }
        }
    }

    void FixedUpdate()
    {
        PlayerController player = PlayerController.current;
        if (isPlayer)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimationName))
            {
                player.canMove = false;
                player.faceMouse = false;
                attackInProcess = true;
                player.SetCurrentAttack(this);
                doOncePerSwing = true;
            }
            else
            {
                if (doOncePerSwing)
                {
                    if (!attackHitSuccessfully)
                    {
                        if (player.stamina < staminaCost)
                        {
                            player.stamina = 0;
                        }
                        else
                        {
                            player.stamina -= staminaCost;
                        }
                    }
                    attackHitSuccessfully = false;
                    player.canMove = true;
                    player.faceMouse = true;
                    attackInProcess = false;
                    doOncePerSwing = false;
                }
                else
                {
                    doOncePerSwing = false;
                }
                if (attackInProcess) chargingMultiplierBoost = 0;
                if (comboOrder == player.comboOrder) player.SetCurrentAttack(null);
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName(endAnimationState) && comboOrder == player.comboOrder)
            {
                EventSystem.current.PlayerAttackEnd();

                player.NextInComboOrder();

                if (player.comboOrder >= player.maxComboOrder)
                {
                    player.ResetComboOrder();
                }
            }
        }
    }

    void Awake()
    {
        if (!isPlayer)
        {
            enemy = gameObject.GetComponentInParent<EnemyBaseController>();
        }
    }

    void Start()
    {
    }

    public void DefineDamageMultiplier(float newValue)
    {
        dmgMultiplier = newValue;
    }
    public void DefineBalanceDamageMultiplier(float newValue)
    {
        balanceDamageMultiplier = newValue;
    }
    public void DefineIsPlayer(bool newValue) { isPlayer = newValue; }
    public void DefineChargeBoostPerClickAmount(float newValue) { chargingAmountPerClick = newValue; }
    public void DefineKeyCodeValue(string value) { key = value; }
    public void DefineComboOrder(int newValue) { comboOrder = newValue; }
    public void DefineAnimator(Animator newAnimator) { animator = newAnimator; }
    private void DefineDamageScalingStats(List<int> newValue) { damageScalingStats = newValue; }
    private void DefineBalanceDamageScalingStats(List<int> newValue) { balanceDamageScalingStats = newValue; }

    public void InitiateStartAnimation()
    {
        animator.SetBool(startAnimationName, true);
        StartCoroutine(IniateStartAnimationCD());
    }
    private IEnumerator IniateStartAnimationCD()
    {
        yield return new WaitForSeconds(0.2f);
        animator.SetBool(startAnimationName, false);
    }

    public void InitiateCharge()
    {
        if (isPlayer)
        {
            PlayerController player = PlayerController.current;
            StopCoroutine(TakeStep());

            animator.SetBool("charging", true);
            player.RestartStaminaRegenTimer();
            if (chargeClick)
            {
                StartCoroutine(Charge());
                chargeClick = false;
            }
        }
        else
        {
            animator.SetBool("charging", true);
            if (chargeClick)
            {
                StartCoroutine(Charge());
                chargeClick = false;
            }
        }
    }

    public void StopCharging()
    {
        animator.SetBool("charging", false);
    }

    public void Cancel()
    {
        if(isPlayer && !animator.GetBool("cancel"))
        {
            animator.SetTrigger("cancel");
            animator.ResetTrigger(attackAnimationName);
            animator.ResetTrigger(startAnimationName);
            PlayerController player = PlayerController.current;
            player.NextInComboOrder();

            player.stamina -= staminaCost;
            if (player.comboOrder >= player.maxComboOrder)
            {
                player.ResetComboOrder();
            }
        }
        else
        {
            // enemy.NextInComboOrder();
        }
    }

    public IEnumerator InitiateAttack()
    {
        if (isPlayer)
        {
        }
        else
        {
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(holdAnimationState) || !animator.IsInTransition(0) && animator.GetBool("charging"));
            animator.SetTrigger(attackAnimationName);
            attackInProcess = true;

            enemy.NextInComboOrder();
            enemy.SetCurrentAttack(this);
        }
    }
    
    private IEnumerator IniateAttackCD()
    {
        yield return new WaitForSeconds(0.3f);
        animator.SetBool(attackAnimationName, false);
    }

    private IEnumerator TakeStep()
    {
        if (stepBool)
        {
            stepBool = false;
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimationName));
            yield return new WaitForSeconds(0.05f);
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimationName))
            {
                PlayerController.current.GetComponent<Rigidbody2D>().AddForce(stepDistance * PlayerController.current.transform.up);
            }
            yield return new WaitUntil(() => PlayerController.current.currentAttack == null);
            stepBool = true;
        }
    }

    public void AttackEnded()
    {
            Debug.Log("Attack ended.");
            if (isPlayer)
            {
                PlayerController player = PlayerController.current;

                chargingMultiplierBoost = 0;

                if (player.maxComboOrder <= comboOrder)
                {
                    player.ResetComboOrder();
                    StartCoroutine(LastOfACombo());
                }

                player.doingAction = false;
                EventSystem.current.PlayerAttackEnd();
            }
            else
            {
                attackInProcess = false;
                enemy.SetCurrentAttack(null);
                enemy.doingAction = false;
                enemy.hit = false;
            }
    }
    private IEnumerator LastOfACombo()
    {
        animator.SetBool("lastOfACombo", true);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("idle"));
        animator.SetBool("lastOfACombo", false);
    }

    public float GetDamageStatMultiplier()
    {
        float statMultiplier = 0f;
        if (isPlayer)
        {
            Dictionary<int, int> playerStats = PlayerController.current.GetStats();
            foreach(int stat in damageScalingStats)
            {
                statMultiplier += (10 / playerStats[stat]) * dmgMultiplier;
            }
        }
        else
        {
            Dictionary<int, int> enemyStats = enemy.GetStats();
            foreach (int stat in damageScalingStats)
            {
                statMultiplier += (10 / enemyStats[stat]) * dmgMultiplier;
            }
        }
        return statMultiplier;
    }

    public float GetBalanceDamageStatMultiplier()
    {
        float statMultiplier = 0f;
        if (isPlayer)
        {
            Dictionary<int, int> playerStats = PlayerController.current.GetStats();
            foreach (int stat in balanceDamageScalingStats)
            {
                statMultiplier += (10 / playerStats[stat]) * balanceDamageMultiplier;
            }
        }
        else
        {
            Dictionary<int, int> enemyStats = enemy.GetStats();
            foreach (int stat in balanceDamageScalingStats)
            {
                statMultiplier += (10 / enemyStats[stat]) * dmgMultiplier;
            }
        }
        return statMultiplier;
    }

    public float GetDamageMultiplier()
    {
        float multiplier = chargingMultiplierBoost + GetDamageStatMultiplier();
        return multiplier;
    }
    public float GetStablityDamageMultiplier()
    {
        float multiplier = chargingMultiplierBoost + GetDamageStatMultiplier();
        return multiplier;
    }
    public float GetChargingBoostMultiplier() { return chargingMultiplierBoost; }
    public Vector3 GetCurrentPosition()
    {
        return gameObject.transform.position;
    }

    public void DefineAttackAnimation(string animationName) { attackAnimationName = animationName; }
    public void DefineStartAnimation(string animationName) { startAnimationName = animationName; }
    public void DefineHoldAnimationState(string stateName) { holdAnimationState = stateName; }
    public void DefineEndAnimationState(string stateName) { endAnimationState = stateName; }
    
    private IEnumerator Charge()
    {
        yield return new WaitForSeconds(0.1f);
        if (chargingMultiplierBoost < maximumCharge) chargingMultiplierBoost += chargingAmountPerClick;
        chargeClick = true;
    }

    public void IsHit()
    {
        if (isPlayer)
        {
            animator.SetTrigger("isHit");
            animator.ResetTrigger(attackAnimationName);
            animator.SetBool("charging", false);

            PlayerController player = PlayerController.current;
            player.faceMouse = true;
            player.canMove = true;
            PlayerController.current.ResetComboOrder();
        }
        else
        {
            animator.SetTrigger("isHit");
            animator.ResetTrigger(attackAnimationName);
            animator.SetBool("charging", false);

            enemy.doingAction = false;
            enemy.SetFacePlayer(true);
            enemy.comboCount = 0;
        }
    }

    public void Reset()
    {
        if (isPlayer)
        {
            animator.ResetTrigger("lastOfACombo");
            animator.ResetTrigger(attackAnimationName);
            animator.SetBool("charging", false);

            chargingMultiplierBoost = 0;

            PlayerController player = PlayerController.current;
            player.faceMouse = true;
            player.canMove = true;
            player.ResetComboOrder();
        }
        else
        {
            animator.ResetTrigger("lastOfACombo");
            animator.ResetTrigger(attackAnimationName);
            animator.SetBool("charging", false);

            enemy.currentAttack = null;

            chargingMultiplierBoost = 0;
        }
    }

    // Actions
    public event Action onButtonUp;
    private void ButtonUpAction()
    {
        if (onButtonUp != null)
            onButtonUp();
    }
}
