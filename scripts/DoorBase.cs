using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorBase : MonoBehaviour
{
    public int openingScene = 0;
    public Vector2 spawnPointAfterEntering = new Vector2(0.0f,0.0f);
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            SceneManager.LoadScene(openingScene);
            PlayerController.current.transform.position = spawnPointAfterEntering;
        }
    }
}
