using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttackSpecials : MonoBehaviour
{
    public AttackBase attack = null;
    public bool isPlayer = true;
    PlayerController player;

    public Vector3 grabPositionDifferencionFromParent = new Vector3();
    public float fixedGrapTime = 0;
    private bool bGrab = false;
    private bool bFixedGrab = false;

    public List<string> specialEffects = new List<string>();
    // Start is called before the first frame update

    public static AttackSpecials CreateANewAttackSpecials(AttackBase attack, List<string> specialEffects, bool isPlayer, Vector3 grabPositionDifferencionFromParent = new Vector3(), float grabTime = 0)
    {
        AttackSpecials newAttackSpecials = attack.gameObject.AddComponent<AttackSpecials>();

        newAttackSpecials.attack = attack;
        newAttackSpecials.specialEffects = specialEffects;
        newAttackSpecials.isPlayer = isPlayer;
        newAttackSpecials.fixedGrapTime = grabTime;

        newAttackSpecials.grabPositionDifferencionFromParent = grabPositionDifferencionFromParent;

        return newAttackSpecials;
    }

    void Start()
    {
        player = PlayerController.current;

        attack.onButtonUp += DoSpecialActions;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoSpecialActions()
    {
        foreach (string action in specialEffects)
        {
            switch (action)
            {
                case "teleport":
                    Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    player.Teleport(position, "physical", 1);
                    Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
                    break;
                case "grabRelease":
                    GrabRelease();
                    break;
                case "moveMainHandToRightSide":
                    Animator anim = player.GetMainHandWeapon().GetComponent<Animator>();
                    anim.SetTrigger("move_rightSide");
                    break;
                default:
                    break;
            }
        }
    }
    
    public void DoSpecialOnHitEffects(GameObject target)
    {
        Vector3 grabPosition = player.transform.position + grabPositionDifferencionFromParent;
        if (isPlayer)
        {
            foreach(string effect in specialEffects)
            {
                switch(effect)
                {
                    case "grabHoldTillRealese":
                        StartCoroutine(GrabTillRelease(grabPosition, target));
                        break;
                    case "grabForFixedTime":
                        StartCoroutine(GrabForFixedTime(grabPosition, target, fixedGrapTime));
                        break;
                }
            }
        }
    }

    private IEnumerator GrabTillRelease(Vector3 grabPosition, GameObject target)
    {
        if (isPlayer)
        {
            bGrab = true;
            while (bGrab)
            {
                player.Grab(grabPosition, target);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private IEnumerator GrabForFixedTime(Vector3 grabPosition, GameObject target, float time)
    {
        if (isPlayer)
        {
            bGrab = true;
            bFixedGrab = false;
            StartCoroutine(FixedGrabTimer(time));
            while (bGrab && bFixedGrab)
            {
                player.Grab(grabPosition, target);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private IEnumerator FixedGrabTimer(float time)
    {
        yield return new WaitForSeconds(time);
        bFixedGrab = true;
    }

    private void GrabRelease()
    {
        bGrab  = false;
    }
}
