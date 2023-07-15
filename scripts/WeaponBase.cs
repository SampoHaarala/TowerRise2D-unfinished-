using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public Dictionary<int, int> requirements = new Dictionary<int, int>
    {
        {0,0 },
        {1,0 },
        {2,0 },
        {3,0 },
        {4,0 },
        {5,0 },
        {6,0 }
    };

    public bool isEnemy = false;
    public bool isVisible = true;
    private EnemyBaseController enemy = null;
    private PlayerController player = null;
    public bool charging = false;

    public string type = "";
    public bool twoHanded = false;

    public float baseDamage = 1;
    public bool isMagicDamage = false;
    public string damageAttribute = "strenght"; 
    public float damageScalingAmount = 0.1f; // 0 = 0%, 0.5 = 50%, 1 = 100%, 1.2 = 120% of stat score

    public float baseBalanceDamage = 1f;
    public string balanceDamageAttribute = "strenght"; 
    public float balanceDamageScalingAmount = 0.1f; // 0 = 0%, 0.5 = 50%, 1 = 100%, 1.2 = 120% of stat score

    public float blockingEffiency = 0f;

    private AttackBase previousAttack;

    private Collider2D col;

    // in order 0,1,2... bleed, fear
    public Dictionary<int, float> statusEffectChances = EventSystem.GetStatusEffectChances();
    public Dictionary<int, float> statusEffectDurations = EventSystem.GetStatusEffectDurations();

    public void AssignIsEnemy(bool newValue) { isEnemy = newValue; }

    public void AssignType(string newValue) { type = newValue; }
    public void AssignTwoHanded(bool newValue) { twoHanded = newValue; }

    public void AssignBaseDamage(float newValue) { baseDamage = newValue; }
    public void AssignIsMagicDamage(bool newValue) { isMagicDamage = newValue; }
    public void AssignDamageAttribute(string newValue) { damageAttribute = newValue; }
    public void AssignDamageScalingAmount(float newValue) { damageScalingAmount = newValue; }

    public void AssignBaseBalanceDamage(float newValue) { baseBalanceDamage = newValue; }
    public void AssignBalanceDamageAttribute(string newValue) { balanceDamageAttribute = newValue; }
    public void AssignBalanceDamageScalingAmount(float newValue) { balanceDamageScalingAmount = newValue; }

    public WeaponBase CreateANewWeaponBase(bool isEnemy, string type, bool twoHanded, float baseDamage, bool isMagicDamage,
                                                string damageAttribute, float damageScalingAmount,
                                                float baseBalanceDamage, string balanceDamageAttribute,
                                                float balanceDamageScalingAmount)
    {
        this.AssignIsEnemy(isEnemy);

        this.AssignType(type);
        this.AssignTwoHanded(twoHanded);

        this.AssignBaseDamage(baseDamage);
        this.AssignDamageAttribute(damageAttribute);
        this.AssignDamageScalingAmount(damageScalingAmount);
        this.AssignIsMagicDamage(isMagicDamage);

        this.AssignBaseBalanceDamage(baseBalanceDamage);
        this.AssignBalanceDamageAttribute(balanceDamageAttribute);
        this.AssignBalanceDamageScalingAmount(balanceDamageScalingAmount);

        return this;
    }

    void Start()
    { 
        EventSystem.current.onPlayerAttackEnd += AttackReset;
        if (isEnemy) enemy = GetComponentInParent<EnemyBaseController>();
        player = PlayerController.current;
    }

    private void AttackReset()
    {
        previousAttack = null;
    }

    private int UpdateDamage()
    {
        float damageScaling = 0f;
        int damage = 0;
        if (isEnemy)
        {
            switch (damageAttribute) // setting damage
            {
                case "vigor":
                    damageScaling = enemy.stats.vigor * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage + baseDamage * damageScaling);
                    break;
                case "strenght":
                    damageScaling = enemy.stats.strenght * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage + baseDamage * damageScaling);
                    break;
                case "endurance":
                    damageScaling = enemy.stats.endurance * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage + baseDamage * damageScaling);
                    break;
                case "dexterity":
                    damageScaling = enemy.stats.dexterity * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage + baseDamage * damageScaling);
                    break;
                case "magic":
                    damageScaling = enemy.stats.magic * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage + baseDamage * damageScaling);
                    break;
                default:
                    Debug.Log("Uknown attribute damage scaling type");
                    damageScaling = 1;
                    damage = Mathf.RoundToInt(baseDamage * damageScaling);
                    break;
            }
        }
        else
        {
            switch (damageAttribute) // setting damage
            {
                case "vigor":
                    damageScaling = EventSystem.currentSave.vigor * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage * damageScaling);
                    break;
                case "strenght":
                    damageScaling = EventSystem.currentSave.strenght * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage * damageScaling);
                    break;
                case "endurance":
                    damageScaling = EventSystem.currentSave.endurance * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage * damageScaling);
                    break;
                case "dexterity":
                    damageScaling = EventSystem.currentSave.dexterity * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage * damageScaling);
                    break;
                case "magic":
                    damageScaling = EventSystem.currentSave.magic * damageScalingAmount;
                    damage = Mathf.RoundToInt(baseDamage * damageScaling);
                    break;
                default:
                    Debug.Log("Uknown attribute damage scaling type");
                    damageScaling = 0;
                    damage = Mathf.RoundToInt(baseDamage * damageScaling);
                    break;
            }
            if (damage < 1) damage = 1;
        }
        return damage;
    }

    public int UpdateBalanceDamage()
    {
        float balanceDamageScaling = 0f;
        int balanceDamage = 0;
        if (isEnemy)
        {
            switch (balanceDamageAttribute)
            {
                case "vigor":
                    balanceDamageScaling = enemy.stats.vigor * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage + baseBalanceDamage * balanceDamageScaling);
                    break;
                case "strenght":
                    balanceDamageScaling = enemy.stats.strenght * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage + baseBalanceDamage * balanceDamageScaling);
                    break;
                case "endurance":
                    balanceDamageScaling = enemy.stats.endurance * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage + baseBalanceDamage * balanceDamageScaling);
                    break;
                case "dexterity":
                    balanceDamageScaling = enemy.stats.dexterity * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage + baseBalanceDamage * balanceDamageScaling);
                    break;
                case "magic":
                    balanceDamageScaling = enemy.stats.magic * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage + baseBalanceDamage * balanceDamageScaling);
                    break;
                default:
                    Debug.Log("Uknown attribute damage scaling type");
                    balanceDamageScaling = 1;
                    balanceDamage = Mathf.RoundToInt(baseDamage * balanceDamageScaling);
                    break;
            }
        }
        else
        {
            switch (balanceDamageAttribute)
            {
                case "vigor":
                    balanceDamageScaling = EventSystem.currentSave.vigor * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage * balanceDamageScaling);
                    break;
                case "strenght":
                    balanceDamageScaling = EventSystem.currentSave.strenght * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage * balanceDamageScaling);
                    break;
                case "endurance":
                    balanceDamageScaling = EventSystem.currentSave.endurance * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage * balanceDamageScaling);
                    break;
                case "dexterity":
                    balanceDamageScaling = EventSystem.currentSave.dexterity * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage * balanceDamageScaling);
                    break;
                case "magic":
                    balanceDamageScaling = EventSystem.currentSave.magic * balanceDamageScalingAmount;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage * balanceDamageScaling);
                    break;
                default:
                    Debug.Log("Unknown attribute damage scaling type");
                    balanceDamageScaling = 1;
                    balanceDamage = Mathf.RoundToInt(baseBalanceDamage * balanceDamageScaling);
                    break;
            }
        }
        return balanceDamage;
    }
    
    void Update()
    {
        if (!isEnemy && Input.GetKeyDown(EventSystem.currentSave.parryingKey)
                          && !player.isBalanceBroken && player.blockingHand == this)
            {
                StartCoroutine(Parry());
            }
        Animator anim = GetComponent<Animator>();
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("idle") && anim.GetBool("isHit"))
        {
            anim.ResetTrigger("isHit");
        }
        if (anim.GetBool("charging")) charging = true;
        else charging = false;
    }

    public IEnumerator Parry()
    {
        if (isEnemy)
        {
            if (enemy.currentAttack != null) enemy.currentAttack.Reset();

            enemy.SetFacePlayer(false);
            Animator anim = enemy.weaponGameobject.GetComponent<Animator>();
            anim.SetBool("parry", true);
            enemy.SetParrying(1);
            yield return new WaitForSeconds(0.1f);
            enemy.SetParrying(2);
            yield return new WaitForSeconds(0.2f);
            enemy.SetParrying(3);
            yield return new WaitForSeconds(0.2f);
            enemy.SetParrying(4);
            yield return new WaitForSeconds(0.2f);

            enemy.SetFacePlayer(true);
            enemy.SetParrying(5);
            yield return new WaitWhile(() => enemy.GetIsBlocking());
            enemy.SetParrying(0);
            anim.SetBool("parry", false);
        }
        else
        {
            Animator anim = player.blockingHand.GetComponent<Animator>();
            if (player.currentAttack)
            {
                player.currentAttack.Reset();
                player.currentAttack = null;
            }

            player.SetFaceMouse(false);
            anim.SetBool("parry", true);
            if (player.currentAttack) anim.ResetTrigger(player.currentAttack.attackAnimationName);

            Debug.Log("Parrying");
            player.SetParrying(1);
            Debug.Log("Parry frame 1");
            yield return new WaitForSeconds(0.1f + player.GetParryingWindowBonus());
            if (anim.GetBool("parry"))
            {
                player.SetParrying(2);
                CancelInvoke("Parry");
            }
            Debug.Log("Parry frame 2");
            yield return new WaitForSeconds(0.1f + player.GetParryingWindowBonus());
            if (anim.GetBool("parry"))
            {
                player.SetParrying(3);
                CancelInvoke("Parry");
            }
            Debug.Log("Parry frame 3");
            yield return new WaitForSeconds(0.1f + player.GetParryingWindowBonus());
            if (anim.GetBool("parry"))
            {
                player.SetParrying(4);
                CancelInvoke("Parry");
            }
            Debug.Log("Parry frame 4");
            yield return new WaitForSeconds(0.1f + player.GetParryingWindowBonus());

            player.SetFaceMouse(true);
            player.SetParrying(5);
            Debug.Log("Blocking");
            yield return new WaitWhile(() => Input.GetKey(EventSystem.currentSave.parryingKey));
            player.SetParrying(0);
            Debug.Log("Parrying ended.");
            anim.SetBool("parry", false);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isEnemy && col.gameObject == player.gameObject)
        {
            if (enemy.currentAttack)
            {
                if (enemy.currentAttack.attackInProcess)
                {
                    float damage = UpdateDamage();
                    float balanceDamage = UpdateBalanceDamage();
                    damage = enemy.currentAttack.GetDamageMultiplier() * damage;
                    balanceDamage = enemy.currentAttack.GetStablityDamageMultiplier() * balanceDamage;

                    PlayerController.current.IsHit(GetComponentInParent<EnemyBaseController>(), damage, balanceDamage);
                }
            }
        }
        else
        {
            if (col.gameObject.CompareTag("Enemy") && PlayerController.current.currentAttack != null)
            {
                AttackBase currentAttack = PlayerController.current.GetCurrentAttack();

                if (currentAttack.specials != null)
                    currentAttack.specials.DoSpecialOnHitEffects(col.gameObject);

                EventSystem.current.PlayerHit();

                int damage = UpdateDamage();
                int balanceDamage = UpdateBalanceDamage();

                Debug.Log(col.gameObject.name + " has been hit.");

                damage = Mathf.RoundToInt(currentAttack.GetDamageMultiplier() * damage);
                balanceDamage = Mathf.RoundToInt(currentAttack.GetStablityDamageMultiplier() * balanceDamage);

                col.GetComponent<EnemyBaseController>().IsHit(damage, balanceDamage);
            }
        }
    }

    private void ApplyStatusEffects(GameObject target, float duration, bool isPlayer)
    {
        for(int i = 0; i < statusEffectChances.Count - 1; i++)
        {
            if (Random.Range(0f,1f) > statusEffectChances[i])
            {
                if(isPlayer)
                {
                    PlayerController.current.ApplyStatusEffectsToPlayer(i, statusEffectDurations[i]);
                }
                else
                {
                    target.GetComponent<EnemyBaseController>().ApplyStatusEffectsToSelf(i, statusEffectDurations[i]);
                }
            }
        }
    }
}