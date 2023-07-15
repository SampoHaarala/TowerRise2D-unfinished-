using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    int health = 0;
    int stamina = 0;
    int balance = 0;

    public RectTransform healthBar;
    public Text healthText;
    public RectTransform staminaBar;
    public Text staminaText;
    public Image staminaExhaustionImage;
    public RectTransform balanceBar;

    public Text stunned;

    PlayerController player;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerController.current;
        player.UI = this;
        stunned.enabled = false;
        anim = staminaExhaustionImage.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        health = (int)player.health;
        if (health < 0) health = 0;
        float healthBarFloat = (float)player.GetMaxHealth() / 400;

        balance = (int)player.balance;
        if (balance < 0) balance = 0;
        float balanceBarFloat = (float)player.GetMaxBalance() / 400;

        stamina = (int)player.stamina;
        if (stamina < 0) stamina = 0;
        float staminaBarFloat = (float)player.GetMaxStamina() / 400;

        healthBar.sizeDelta = new Vector2(health / healthBarFloat, 20);
        healthText.text = health + "/" + player.GetMaxHealth();

        staminaBar.sizeDelta = new Vector2(stamina / staminaBarFloat, 20);
        staminaText.text = stamina + "/" + player.GetMaxStamina();
        if (stamina > player.GetMaxStamina() * 0.4f)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("stamina_exhaustion0")) anim.SetTrigger("staminaExhaustion0");
        }
        else if (stamina > player.GetMaxStamina() * 0.2f)
        {
            anim.SetTrigger("staminaExhaustion1");
        }
        else if (stamina > 1)
        {
            anim.SetTrigger("staminaExhaustion2");
        }
        else
        {
            anim.SetTrigger("staminaExhaustion3");
        }



            balanceBar.sizeDelta = new Vector2(balance / balanceBarFloat, 20);
    }

    public void ShowStunnedText()
    {
        stunned.enabled = true;
    }

    public void HideStunnedText()
    {
        stunned.enabled = false;
    }
}
