using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLight : MonoBehaviour
{
    [SerializeField] GameObject light;

    public GameObject GetLight() { return light; }

    public void EnableLight()
    {
        light.SetActive(true);
    }
}
