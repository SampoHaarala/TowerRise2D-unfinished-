using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    public EnemyBaseController parent;
    public Canvas canvas;
    public Image healthBar;

    public bool canSeeHealth = true;
    // Start is called before the first frame update
    void Start()
    {
        healthBar.color = Color.green;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        healthBar.rectTransform.position = Camera.main.WorldToScreenPoint(parent.transform.position) + new Vector3(0, 30, 0);

        if (canSeeHealth)
        {
            float health = parent.health;
            float maxHealth = parent.GetMaxHealth();
            if (health > maxHealth * 0.6) healthBar.color = Color.green;
            else if (health > maxHealth * 0.3) healthBar.color = Color.yellow;
            else healthBar.color = Color.red;
        }
    }
}
