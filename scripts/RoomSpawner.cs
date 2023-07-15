using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    private RoomTemplates templates;
    public int openingDir;
    private int rand;
    public bool spawned = false;
    private GameObject grid;

    public int currentFloor = 1;

    private GameObject door = null;
    private GameObject room = null;
    private DoorInTheDungeon doorScript = null;
    // Start is called before the first frame update
    void Start()
    {
        templates = RoomTemplates.current;
        grid = GameObject.FindGameObjectWithTag("Grid");
        Invoke("Spawn", 0.1f);
    }

    // Update is called once per frame
    void Spawn()
    {
        if (spawned == false)
        {
            if (openingDir == 1)
            {
                rand = Random.Range(0, templates.bottomRooms.Length - 1);
                room = Instantiate(templates.bottomRooms[rand], transform.position, templates.bottomRooms[rand].transform.rotation);
                room.transform.SetParent(grid.transform);

                door = Instantiate(templates.door);
                door.transform.localPosition = transform.position + new Vector3(0, -1.04f);
                door.GetComponent<BoxCollider2D>().size = new Vector3(0.32f, 0.1f);

                doorScript = door.GetComponent<DoorInTheDungeon>();
                doorScript.distanceWhenEnteringY = 0.55f;
                doorScript.room = room;
            }
            if (openingDir == 2)
            {
                rand = Random.Range(0, templates.leftRooms.Length - 1);
                room = Instantiate(templates.leftRooms[rand], transform.position, templates.leftRooms[rand].transform.rotation);
                room.transform.SetParent(grid.transform);

                door = Instantiate(templates.door);
                door.transform.localPosition = transform.position + new Vector3(-1.04f, 0);
                door.GetComponent<BoxCollider2D>().size = new Vector3(0.1f, 0.32f);

                doorScript = door.GetComponent<DoorInTheDungeon>();
                doorScript.distanceWhenEnteringX = 0.55f;
                doorScript.room = room;
            }
            if (openingDir == 3)
            {
                rand = Random.Range(0, templates.topRooms.Length - 1);
                room = Instantiate(templates.topRooms[rand], transform.position, templates.topRooms[rand].transform.rotation);
                room.transform.SetParent(grid.transform);

                door = Instantiate(templates.door);
                door.transform.localPosition = transform.position + new Vector3(0, 1.04f);
                door.GetComponent<BoxCollider2D>().size = new Vector3(0.32f, 0.1f);

                doorScript = door.GetComponent<DoorInTheDungeon>();
                doorScript.distanceWhenEnteringY = -0.55f;
                doorScript.room = room;

            }
            if (openingDir == 4)
            {
                rand = Random.Range(0, templates.rightRooms.Length - 1);
                room = Instantiate(templates.rightRooms[rand], transform.position, templates.rightRooms[rand].transform.rotation);
                room.transform.SetParent(grid.transform);


                door = Instantiate(templates.door);
                door.transform.localPosition = transform.position + new Vector3(1.04f, 0);
                door.GetComponent<BoxCollider2D>().size = new Vector3(0.1f, 0.32f);

                doorScript = door.GetComponent<DoorInTheDungeon>();
                doorScript.distanceWhenEnteringX = -0.55f;
                doorScript.room = room;
            }

            if (currentFloor == 1)
            {
                int randInt = Random.Range(1, 3);
                int i = 0;
                while (i < randInt)
                {
                    rand = Random.Range(0, templates.enemies.Length - 1);
                    GameObject enemy = Instantiate(templates.enemies[rand], transform.position + new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(-0.8f, 0.8f)), transform.rotation);
                    enemy.transform.parent = room.transform;
                    enemy.GetComponent<EnemyBaseController>().room = room;
                    i++;
                }
            }
            spawned = true;
        }
    }

    private void SpawnEmptyRoom()
    {
        if (!spawned)
        {
            GameObject room = Instantiate(templates.closedRoom, transform.position, templates.closedRoom.transform.rotation);
            room.transform.SetParent(grid.transform);
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "room") spawned = true;
        else if (other.transform.tag == gameObject.tag && !spawned)
        {
            if (other.GetComponent<RoomSpawner>().spawned == false && spawned == false)
            {
                Invoke("SpawnEmptyRoom", 0.05f);
            }
        }
    }
}
