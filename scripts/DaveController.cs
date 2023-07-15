using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DaveController : MonoBehaviour
{
    public GameObject chatMenu;
    private Rigidbody2D rb;
    public static DaveController current = null;

    private DaveDialogue1 dialoge1 = null;

    public Vector3 firstInteractionPosition = new Vector3(-1, 0);
    public Vector3 normalPosition1 = new Vector3(-1, 0);
    // Start is called before the first frame update
    private void Start()
    {
        if (current != null) Destroy(gameObject);
        else
        {
            current = this;
            DontDestroyOnLoad(this);
        }

            rb = GetComponent<Rigidbody2D>();
        if (!chatMenu)
        {
            Debug.Log("DaveController missing chatMenu variable!");
        }

        SceneManager.sceneLoaded += NewSceneDaveActionCheck;
    }

    // Update is called once per frame
    void Update()
    {
        if (dialoge1 != null)
        {
            if (dialoge1.actionDone)
            {
                Destroy(dialoge1);
                this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
    void NewSceneDaveActionCheck(Scene newScene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("New scene Dave Action check.");
        GetComponent<SpriteRenderer>().enabled = true;
        switch(newScene.buildIndex)
        {
            case 1:
            switch (EventSystem.current.GetCurrentDaveActionCount())
            {
                case 1:
                    EventSystem.current.DaveEvent01();
                    DaveAction01 da1 = this.gameObject.AddComponent<DaveAction01>();
                    break;
            }
            break;
            case 2:
                switch (EventSystem.current.GetCurrentDaveActionCount())
                {
                    case 0:
                        transform.position = firstInteractionPosition;
                        break;
                }
                break;
            case 3:
                GetComponent<SpriteRenderer>().enabled = false;
                break;
        }

    }

    public void DoCurrentDaveAction(int i = -1)
    {
        if (i == -1)
        {
            switch (EventSystem.current.GetCurrentDaveActionCount())
            {
                case 0:
                    EventSystem.current.DaveEvent00();
                    dialoge1 = gameObject.AddComponent<DaveDialogue1>();
                    dialoge1.chatMenu = chatMenu;
                    break;
                case 1:
                    chatMenu.SetActive(true);
                    break;
            }
            Debug.Log("Current Dave-action: " + EventSystem.current.GetCurrentDaveActionCount());
        }
        else
        {
            switch(i)
            {
                case 0:
                    EventSystem.current.DaveEvent00();
                    DaveDialogue1 dialoge1 = this.gameObject.AddComponent<DaveDialogue1>();
                    dialoge1.chatMenu = chatMenu;
                    break;
            }
        }
    }
    public event Action onDaveInteract;
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Interactor")
        {
            if (onDaveInteract != null) onDaveInteract();
            DoCurrentDaveAction();
            Debug.Log("do current dave interaction!");
        }
    }
}