using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorInTheDungeon : MonoBehaviour
{
    public float distanceWhenEnteringX = 0.0f;
    public float distanceWhenEnteringY = 0.0f;
    public GameObject room;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Player"))
        {
            GameObject player = col.gameObject;
            if (room.GetComponent<TilemapRenderer>().enabled)
            {
                col.gameObject.transform.position = new Vector3(player.transform.position.x - distanceWhenEnteringX, player.transform.position.y - distanceWhenEnteringY, 0);
            }
            else
            {
                col.gameObject.transform.position = new Vector3(player.transform.position.x + distanceWhenEnteringX, player.transform.position.y + distanceWhenEnteringY, 0);
            }
        }
    }
}
