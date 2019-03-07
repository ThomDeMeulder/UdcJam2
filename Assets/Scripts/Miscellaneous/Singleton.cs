﻿using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get => !instance ? instance = FindObjectOfType<T>() : instance;
    }
}
