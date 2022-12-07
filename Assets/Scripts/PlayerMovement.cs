using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Transform camera;

    [Header("Rotation")]
    [SerializeField] float turnSpeed = 2f;
    [SerializeField] float lookDownMaxAngle = -80f;
    [SerializeField] float lookUpMaxAngle = 80f;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float maxUpwardMoveAngle = 60f;

    Vector3 forwardDir = Vector3.forward;
    Vector3 rightDir = Vector3.right;
    Rigidbody rb;

    List<Collision> colliders = new();

    float rotY = 0;

    public void PlayerStart()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camera = transform.Find("Main Camera");

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Rotation
        float lookRight = Input.GetAxis("Mouse X");
        float lookUp = Input.GetAxis("Mouse Y");

        rotY = Mathf.Clamp(rotY + (lookUp * turnSpeed), lookDownMaxAngle, lookUpMaxAngle);

        camera.rotation = Quaternion.Euler(new Vector3(rotY, transform.eulerAngles.y, transform.eulerAngles.z));
        transform.Rotate(0, lookRight * turnSpeed, 0);

        //Collision
        if(colliders.Count > 0)
        {
            forwardDir = Vector3.zero;
            rightDir = Vector3.zero;
        }
        

        foreach (Collision collision in colliders)
        {
            Vector3 thisForward = Vector3.Cross(-collision.contacts[0].normal, transform.right);
            if (Vector3.Angle(transform.forward, thisForward) > maxUpwardMoveAngle)
            {
                thisForward = transform.forward;
            }

            forwardDir += thisForward;

            Vector3 thisRight = Vector3.Cross(collision.contacts[0].normal, transform.forward);
            if (Vector3.Angle(transform.right, thisRight) > maxUpwardMoveAngle)
            {
                thisRight = transform.right;
            }

            rightDir += thisRight;
        }

        forwardDir.Normalize();
        rightDir.Normalize();

        Debug.DrawRay(transform.position, forwardDir, Color.green);
        Debug.DrawRay(transform.position, rightDir, Color.red);

        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");

        //Movement
        Vector3 move = (moveZ * forwardDir) + (moveX * rightDir).normalized;

        if(moveX < .1f && moveZ < .1f)
        {
            //rb.velocity -= Vector3.Dot(rb.velocity, rightDir) * (rb.velocity - rightDir).normalized;
            rb.constraints &= ~RigidbodyConstraints.FreezePositionX;
            rb.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            rb.constraints |= RigidbodyConstraints.FreezePositionX & RigidbodyConstraints.FreezePositionZ;
        }

        rb.AddForce(move * moveSpeed * Time.deltaTime);

        colliders.Clear();
    }

    private void OnCollisionStay(Collision collision)
    {
        colliders.Add(collision);
    }
}
