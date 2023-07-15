using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInTheSameScene : MonoBehaviour
{
    public List<GameObject> otherRooms;
    public GameObject enteredRoom;
    public float distanceWhenEnteringX = 0.0f;
    public float distanceWhenEnteringY = 0.0f;

    void OnTriggerEnter2D(Collider2D col)
    {

        if (col.gameObject.tag == "Player")
        {
            if (!enteredRoom.gameObject.activeSelf)
            {
                enteredRoom.gameObject.SetActive(true);
                Vector2 enteringPoint = new Vector2(col.gameObject.transform.position.x + distanceWhenEnteringX, col.gameObject.transform.position.y + distanceWhenEnteringY);
                col.gameObject.transform.position = enteringPoint;

                foreach (GameObject room in otherRooms)
                {
                    room.SetActive(false);
                }
            }
            else
            {
                enteredRoom.gameObject.SetActive(false);
                Vector2 enteringPoint = new Vector2(col.gameObject.transform.position.x - distanceWhenEnteringX, col.gameObject.transform.position.y - distanceWhenEnteringY);
                col.gameObject.transform.position = enteringPoint;

                foreach (GameObject room in otherRooms)
                {
                    room.SetActive(true);
                }
            }
        }
    }
}
