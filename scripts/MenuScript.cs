using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    private GameObject player = null;
    private Rigidbody2D rb2D;
    private PlayerController playerController;
    private int i = 0;

    public int currentPage = 0;

    void Awake()
    {
        player = PlayerController.current.gameObject;
        rb2D = player.GetComponent<Rigidbody2D>();
        playerController = PlayerController.current;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) currentPage += 1;
        if (i == 0)
        {
            rb2D.Sleep();
            playerController.doingAction = true;
            playerController.enabled = false;
            i += 1;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAndDisable();
        }
    }
    public void CloseAndDisable()
    {
        i = 0;
        currentPage = 0;
        rb2D.WakeUp();
        playerController.enabled = true;
        playerController.doingAction = false;
        this.gameObject.SetActive(false);
    }
}
