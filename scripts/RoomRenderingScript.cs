using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomRenderingScript : MonoBehaviour
{
    public bool isPlayerInThisRoom = false;
    private bool hasAllRoomsSpawned = false;

    private TilemapRenderer tilemapRenderer = null;

    private static GameObject currentRoom = null;
    public List<Transform> children = null;

    void Start()
    {
        EventSystem.current.onAllRoomsSpawned += RoomsSpawned;
        tilemapRenderer = GetComponent<TilemapRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        if (hasAllRoomsSpawned)
        {
            if (isPlayerInThisRoom)
            {
                foreach (Transform child in children)
                {
                    if (child != null) if (child.gameObject.name != gameObject.name) child.gameObject.SetActive(true);
                }
                tilemapRenderer.enabled = true;
            }
            else
            {
                foreach (Transform child in children)
                {
                    if(child != null) if(child.gameObject.name != gameObject.name) child.gameObject.SetActive(false);
                }
                tilemapRenderer.enabled = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (currentRoom != gameObject) isPlayerInThisRoom = false;
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            isPlayerInThisRoom = true;
            currentRoom = gameObject;
        }
    }

    void RoomsSpawned()
    {
        hasAllRoomsSpawned = true;
        children =  new List<Transform>(GetComponentsInChildren<Transform>());
    }
}
