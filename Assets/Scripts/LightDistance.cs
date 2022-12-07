using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDistance : MonoBehaviour
{
    [SerializeField] float disableDist = 30f;
    Light torchLight;
    private void Start()
    {
        torchLight = GetComponent<Light>();
    }
    void Update()
    {
        if((Camera.main.transform.position - transform.position).sqrMagnitude > disableDist * disableDist)
        {
            torchLight.enabled = false;
        }
        else
        {
            torchLight.enabled = true;
        }
    }
}
