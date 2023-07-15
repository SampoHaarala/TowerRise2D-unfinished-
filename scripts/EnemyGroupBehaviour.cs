using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static public Vector3 GetTacticalDirection(EnemyBaseController parent)
    {
        Vector3 parentPosition = parent.transform.position;
        float shortestDistance = float.PositiveInfinity;
        Vector3 closestAlly = new Vector3();
        float playerDistance = parent.GetPlayerDistance();

        List<EnemyBaseController> allies = parent.GetAllLivingAlliesInRoom();
        if (allies.Count > 0)
        {
            foreach (EnemyBaseController ally in allies)
            {
                float distance = (parentPosition - ally.transform.position).magnitude;
                if (distance < shortestDistance)
                {
                    closestAlly = ally.transform.position;
                }
            }
        }
        if (shortestDistance < playerDistance)
        {
            Vector3 dir = parentPosition - closestAlly;
            return dir;
        }
        else return parent.GetPlayerDirection();
    }
}
