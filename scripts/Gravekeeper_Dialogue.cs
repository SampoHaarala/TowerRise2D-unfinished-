using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gravekeeper_Dialogue : MonoBehaviour
{
    private int chatFrame = 0;
    public GameObject chatTextObj;
    public GameObject player;
    private Text chatText;

    // Start is called before the first frame update
    void Start()
    {
        chatText = chatTextObj.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            chatFrame++;
        }
        switch (chatFrame)
        {
            case 0:
                chatText.text = "Goodmorning.";
                break;
            case 1:
                chatText.text = "Thy might be wandering where thou are but I won't answer that question.";
                break;
            case 2:
                chatText.text = "If thou haven't guessed yet, thou have been rised from the dead.";
                break;
            case 3:
                chatText.text = "Thy life ended early leaving thy potential unrevelead so we decided to give thee a new chance at life. But if this life will be more pleasant than thy first life, I can't guranteed. But I hope thou makes the most of it.";
                break;
            case 4:
                chatText.text = "Go ahead this road and take a right to the mansion. There you shall be told more.";
                break;
            case 5:
                chatText.text = "Goodluck and see thou soon.";
                break;
        }
        if(chatFrame > 5)
        {
            gameObject.GetComponent<MenuScript>().CloseAndDisable();
        }
        
    }
}
