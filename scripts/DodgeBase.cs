using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeBase : MonoBehaviour
{
    public float dodgeDistance = 0.5f;
    public float dodgeCD = 0.5f;
    public bool bCanDodge = true;
    private Vector3 lastMoveDir = new Vector3(0, 0, 0);

    public string dodgeName;
    private float staminaCost = 10;
    private bool isPlayer;

    public void SetDodge(string dodgeName, float staminaCost, bool isPlayer = false)
    {
        this.dodgeName = dodgeName;
        this.isPlayer = isPlayer;
        this.staminaCost = staminaCost;
    }

    void Update()
    {
        if (isPlayer && Input.GetKeyDown(EventSystem.currentSave.dodgeKey))
        {
            if (staminaCost <= 0)
            {
                Debug.Log("No cheating now.");
                return;
            }
            DoDodge();
        }
    }

    public void DoDodge(Vector2 dir = new Vector2())
    {
        if (bCanDodge)
        {
            bCanDodge = false;
            StartCoroutine(DodgeCD());

            if (!isPlayer)
            {
                switch (dodgeName)
                {
                    case "":
                        Debug.Log("Please enter the name of dodge.");
                        break;
                    case "dash":
                        DoDashDodge(dir);
                        break;
                    default:
                        Debug.Log("DodgeBase player: Dodge with the name " + dodgeName + " was not found. Make sure you spelled the name correctly.");
                        break;
                }
            }
            else
            {
                if (!PlayerController.current.isBalanceBroken)
                {

                    if (PlayerController.current.stamina < staminaCost)
                    {
                        if (PlayerController.current.stamina <= 0) return;
                        PlayerController.current.stamina = 0;
                    }
                    else
                    {
                        PlayerController.current.stamina -= staminaCost;
                    }
                    switch (dodgeName)
                    {
                        case "":
                            Debug.Log("Please enter the name of dodge.");
                            break;
                        case "dash":
                            DoDashDodge();
                            break;
                        default:
                            Debug.Log("DodgeBase player: Dodge with the name " + dodgeName + " was not found. Make sure you spelled the name correctly.");
                            break;
                    }
                }
            }
        }
    }

    // Dodges
    private void DoDashDodge(Vector2 dir = new Vector2())
    {
        if (isPlayer)
        {
            PlayerController.current.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            Vector3 beforeDashPosition = transform.position;
            int layerMask = LayerMask.GetMask("Player");
            layerMask |= LayerMask.GetMask("Ignore Raycast");
            Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, dodgeDistance, ~layerMask);

            if (hit.point == new Vector2(0, 0))
            {
                this.gameObject.transform.position = new Vector2(this.transform.position.x + moveDirection.x * dodgeDistance, this.transform.position.y + moveDirection.y * dodgeDistance);
            }
            else
            {
                this.gameObject.transform.position = new Vector2(hit.point.x - moveDirection.x * 0.10f, hit.point.y - moveDirection.y * 0.10f);
                // consider stumble animation here
            }
        }
        else
        {
            int layerMask = LayerMask.GetMask("Enemy");
            layerMask |= LayerMask.GetMask("Ignore Raycast");

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dodgeDistance, ~layerMask);

            if (hit.point == new Vector2(0, 0))
            {
                this.gameObject.transform.position = new Vector2(this.transform.position.x + dir.normalized.x * dodgeDistance, this.transform.position.y + dir.normalized.y * dodgeDistance);
            }
            else
            {
                this.gameObject.transform.position = new Vector2(hit.point.x - dir.x * 0.10f, hit.point.y - dir.y * 0.10f);
                // consider stumble animation here
            }
        }
    }

    private IEnumerator DodgeCD()
    {
        yield return new WaitForSeconds(dodgeCD);
        bCanDodge = true;
    }
}
