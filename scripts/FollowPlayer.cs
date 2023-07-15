using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private GameObject player = null;
    private Rigidbody2D rb2d;
    public float speed = 50;
    public float knockBackMultiplier = 50;
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerController.current.gameObject;
        if (player == null) Debug.Log("Missing player variable!");

        rb2d = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 dir = PlayerController.current.transform.position - transform.position;
         if (transform.position != player.transform.position)
         {
            rb2d.AddForce(dir * speed);
         }
    }

    public void CameraKnockback(Vector3 dir, float force)
    {
        rb2d.AddForce(dir * force * knockBackMultiplier);
    }
}
