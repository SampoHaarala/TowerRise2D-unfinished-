using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DaveAction01 : MonoBehaviour
{
    public bool actionDone = false;
    private Rigidbody2D rb;
    private Image davePanel;
    private Text text = null;
    private bool interactedWith = false;
    private Vector2 dir = new Vector3(-7.5f, 5.5f) - new Vector3(15.5f, 5.5f);
    private bool atDestination = false;
    private MenuScript menuScript;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        this.transform.position = new Vector3(12.5f, 5f);
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        if (!rb) Debug.Log("Missing rigidbody component.");
        davePanel = DaveController.current.chatMenu.GetComponent<Image>();
        Text[] textList = davePanel.gameObject.GetComponentsInChildren<Text>();
        foreach(Text child in textList)
        {
            if (child.name == "text") text = child;
        }
        DaveController.current.onDaveInteract += DaveHasBeenInteractedWith;

        text.text = "Just follow me for now.";
        if (gameObject.GetComponents<DaveAction01>().Length > 1) Destroy(this);

        menuScript = davePanel.gameObject.GetComponent<MenuScript>();
        if (!menuScript) Debug.Log("DaveAction01 missing a menu script!");
    }

    // Update is called once per frame
    void Update()
    {
        if (interactedWith)
        {
            switch(menuScript.currentPage)
            {
                case 0:
                    text.text = "Well now, see that huge ass tower.";
                    break;
                case 1:
                    // disable canvas.
                    davePanel.gameObject.GetComponentInParent<Canvas>().enabled = false;
                    // Play zoom out animation.
                    Camera.main.gameObject.GetComponent<Animator>().SetBool("zoomOut", true);
                    break;
                case 2:
                    // re-enable canvas.
                    davePanel.gameObject.GetComponentInParent<Canvas>().enabled = true;
                    text.text = "That is THE tower.";
                    break;
                case 3:
                    text.text = "And you will go to the top of that.";
                    break;
                case 4:
                    text.text = "I will help you do so.";
                    break;
                case 5:
                    text.text = "So go on in now.";
                    break;
                case 6:
                    Camera.main.gameObject.GetComponent<Animator>().SetBool("zoomOut", false);
                    menuScript.CloseAndDisable();
                    break;
            }
        }
        if (this.transform.position.x >= -8)
        {
            dir = dir.normalized;
            rb.velocity = 1.5f * dir;
        }
        else
        {
            atDestination = true;
            rb.velocity = Vector2.zero;
        }
        
    }
    void DaveHasBeenInteractedWith()
    {
        if (atDestination)
        {
            Debug.Log("InteractedWith");
            davePanel.gameObject.SetActive(true);
            interactedWith = true;
        }
    }
}
