using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplates : MonoBehaviour
{
    public static RoomTemplates current;

    public GameObject[] topRooms;
    public GameObject[] rightRooms;
    public GameObject[] bottomRooms;
    public GameObject[] leftRooms;

    public GameObject closedRoom;

    public List<GameObject> rooms;

    public GameObject door;

    public GameObject[] enemies;

    public int currentFloor = 1;
    public float waitTime;
    private bool spawnedBoss;
    public GameObject boss;
    public GameObject lastRoom;

    void Update()
    {
        if(waitTime <= 0 && spawnedBoss == false)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (i == rooms.Count - 1)
                {
                    spawnedBoss = true;
                    Instantiate(boss, rooms[i].transform.position, Quaternion.identity);
                    EventSystem.current.AllRoomsSpawned();
                    /*
                    foreach(GameObject room in rooms)
                    {
                        room.SetActive(false);
                    }
                    */
                }
            }
        }
        else
        {
            waitTime -= Time.deltaTime;
        }
    }

    private void Awake()
    {
        if (current) Destroy(this.gameObject);
        else current = this;
    }
}