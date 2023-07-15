using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmiaditeSelfdestruct : MonoBehaviour
{
    public float Delay = 0.2f;

    void Awake()
    {
        Destroy(this.gameObject, Delay);
    }
}
