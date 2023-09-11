/*
        Copyright (c) 2022 - 2023 Dillon Drummond

        Permission is hereby granted, free of charge, to any person obtaining
        a copy of this software and associated documentation files (the
        "Software"), to deal in the Software without restriction, including
        without limitation the rights to use, copy, modify, merge, publish,
        distribute, sublicense, and/or sell copies of the Software, and to
        permit persons to whom the Software is furnished to do so, subject to
        the following conditions:

        The above copyright notice and this permission notice shall be
        included in all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
        EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
        MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
        NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
        LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
        OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
        WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

/*
        Daedalus Dungeon Generator: 3D Dungeon Generator Tool
	    By Dillon W. Drummond

	    PlayerMovement.cs

	    ********************************************
	    ***       Player movement for demo       ***
	    ********************************************
 */


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
    [SerializeField] bool freezeCam = false;

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
        if(Input.GetKeyDown(KeyCode.F))
        {
            freezeCam = !freezeCam;
        }

        float lookRight = 0;
        float lookUp = 0;

        //Rotation
        if (!freezeCam)
        {
            lookRight = Input.GetAxis("Mouse X");
            lookUp = Input.GetAxis("Mouse Y");
        }

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
            if(collision.contacts.Length > 0)
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
        }

        forwardDir.Normalize();
        rightDir.Normalize();

        Debug.DrawRay(transform.position, forwardDir, Color.green);
        Debug.DrawRay(transform.position, rightDir, Color.red);

        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");

        //Movement
        Vector3 move = (moveZ * forwardDir) + (moveX * rightDir).normalized;

        if(Mathf.Abs(moveX) > .1f || Mathf.Abs(moveZ) > .1f)
        {
            //Allow movement on input
            rb.constraints &= ~RigidbodyConstraints.FreezePositionX;
            rb.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            //Stop from sliding down stairs when not pressing anything
            rb.constraints |= RigidbodyConstraints.FreezePositionX;
            rb.constraints |= RigidbodyConstraints.FreezePositionZ;
        }

        if(Vector3.Angle(transform.forward, forwardDir) > 20f)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }

        rb.AddForce(move * moveSpeed * Time.deltaTime);

        colliders.Clear();
    }

    private void OnCollisionStay(Collision collision)
    {
        colliders.Add(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        forwardDir = Vector3.zero;
        rightDir = Vector3.zero;
    }
}
