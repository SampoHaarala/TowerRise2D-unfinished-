using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaveTheJudgeScript : MonoBehaviour
{
    private EnemyBaseController enemyBase;
    // Start is called before the first frame update
    void Start()
    {
        enemyBase = GetComponent<EnemyBaseController>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
