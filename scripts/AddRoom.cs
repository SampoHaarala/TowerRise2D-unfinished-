﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRoom : MonoBehaviour
{
    void Start()
    {
        RoomTemplates.current.rooms.Add(gameObject);
    }
}
