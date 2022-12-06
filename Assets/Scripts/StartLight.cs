using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLight : MonoBehaviour
{
    [SerializeField] GameObject torch;

    public void EnableLight()
    {
        torch.SetActive(true);
    }
}
