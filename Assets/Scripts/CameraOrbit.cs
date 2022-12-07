using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    Grid grid;

    [SerializeField] float turnSpeed = 5f;
    [SerializeField] float scrollSpeed = 10f;

    [SerializeField] float dist = 50f;

    void Start()
    {
        grid = FindObjectOfType<Grid>();
    }

    void Update()
    {
        Vector3 orbitCenter = grid.GetGridCenter();

        float zoom = -Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;

        float rotX = 0;
        float rotY = 0;

        if(Input.GetMouseButton(1))
        {
            rotX = -Input.GetAxis("Mouse X");
            rotY = Input.GetAxis("Mouse Y");
        }

        dist += zoom;

        transform.position += ((rotY * transform.up) + (rotX * transform.right)) * turnSpeed;
        Vector3 diff = (transform.position - orbitCenter).normalized * dist;
        transform.position = orbitCenter + diff;

        transform.rotation = Quaternion.LookRotation(orbitCenter - transform.position, Vector3.up);
    }
}
