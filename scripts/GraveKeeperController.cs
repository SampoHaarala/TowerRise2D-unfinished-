using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveKeeperController : MonoBehaviour
{
    public GameObject chatMenu;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Interactor")
        {
            chatMenu.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
