using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class DaveDialogue1 : MonoBehaviour
{
    private int chatFrame = 0;
    public GameObject chatMenu;
    private Text chatText;
    private static GameObject player = null;
    private Vector2 dir = new Vector2(0, 0);
    private Rigidbody2D rb;
    public bool actionDone = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        if (chatMenu != null)
        {
            chatMenu.SetActive(true);
            Text[] textList = chatMenu.GetComponentsInChildren<Text>();
            foreach (Text child in textList)
            {
                if (child.name == "text") chatText = child;
            }
        }
        else
        {
            Debug.Log("daveDialogue1 is missing chatMenu variable.");
        }
        DaveController daveController = this.gameObject.GetComponent<DaveController>();
        player = PlayerController.current.gameObject;
        if (!player) Debug.Log("DaveDialogue01 missing Player!");
        chatText.text = "Welcome!";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            chatFrame++;
            Debug.Log("Page: " + chatFrame);
            if (chatText != null)
            {
                switch (chatFrame)
                {
                    case 0:
                        chatText.text = "Welcome!";
                        break;
                    case 1:
                        chatText.text = "I am Davidaio, but just call me Dave, and these...";
                        // TO-DO put turning animation here!
                        break;
                    case 2:
                        chatText.text = "Well, there is more of us here. But the others are...";
                        // TO-DO put turning animation here!
                        break;
                    case 3:
                        chatText.text = "...too invested in their work to spare time for such things as a greeting.";
                        break;
                    case 4:
                        chatText.text = "But their focus is a proof of their talent so don't mind it!";
                        break;
                    case 5:
                        chatText.text = "We are here in a similar situation as you. Or well, the only thing we have in common is that none of us can leave this place.";
                        break;
                    case 6:
                        chatText.text = "I hope that you'll accept that fast. 'Cus ya don't have a choice.";
                        break;
                    case 7:
                        chatText.text = "Well then, to help you understand better let's go outside.";
                        EventSystem.current.AddCurrentDaveInteractionCount();
                        Debug.Log("Current Dave action count: " + EventSystem.current.GetCurrentDaveActionCount());
                        dir = new Vector3(0.156f, -1.206f) - this.transform.position;
                        chatMenu.GetComponent<MenuScript>().CloseAndDisable();
                        chatFrame++;
                        break;
                }
            }
            if (chatFrame > 7)
            {
                chatMenu.GetComponent<MenuScript>().CloseAndDisable();
                dir = dir.normalized;
                rb.velocity = dir * 2f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Door")
        {
            actionDone = true;
        }
    }
}