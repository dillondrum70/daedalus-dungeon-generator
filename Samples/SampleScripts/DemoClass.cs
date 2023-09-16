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

	    DemoClass.cs

	    ********************************************
	    *** Dungeon tool controls in play mode   ***
	    ********************************************
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that demonstrates how the Dungeon Generator can be used.
/// </summary>
public class DemoClass : MonoBehaviour
{
[Header("Gameplay")]
    [SerializeField] GameObject playerPrefab;
    Camera orbitCamera;
    Camera playerCamera;

    DungeonGenerator dungeonGenerator;

    private void Start()
    {
        orbitCamera = FindObjectOfType<Camera>();
    }

    private void OnEnable()
    {
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        dungeonGenerator.onDungeonClear.AddListener(OnDungeonClear);
        dungeonGenerator.onDungeonGenerate.AddListener(OnDungeonGenerate);
    }

    private void OnDisable()
    {
        dungeonGenerator.onDungeonClear.RemoveListener(OnDungeonClear);
        dungeonGenerator.onDungeonGenerate.RemoveListener(OnDungeonGenerate);
    }

    /// <summary>
    /// Handles player input for generating the dungeon and toggling to first person and orbit camera mode
    /// </summary>
    private void Update()
    {
        //Clear last dungeon and generate new dungeon
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dungeonGenerator.Generate();
        }

        //Toggle between orbiting camera and first person view
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(playerCamera == null)
            {
                return;
            }

            //Make sure this isn't partway through dungeon generation where player
            //has been destroyed and not reinitialized yet
            if (playerCamera) 
            {
                //Toggle cameras
                if (orbitCamera.enabled)
                {
                    orbitCamera.enabled = false;
                    playerCamera.enabled = true;
                }
                else
                {
                    orbitCamera.enabled = true;
                    playerCamera.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("Could not get player camera");
            }
        }
    }

    /// <summary>
    /// Destroys player when dungeon is reset
    /// </summary>
    void OnDungeonClear()
    {
        //Empty all arrays and delete all current rooms
        if (playerCamera)
        {
            orbitCamera.enabled = true;
            playerCamera.enabled = false;

            Destroy(playerCamera.transform.parent.gameObject);
        }
    }

    /// <summary>
    /// Creates new player once dungeon is generated
    /// </summary>
    void OnDungeonGenerate()
    {
        //DEMO: Create a player inside the first cell of the first room
        GameObject player = Instantiate(playerPrefab, dungeonGenerator.GetRooms()[0].cells[0].center, Quaternion.identity, null);
        player.transform.localScale = Vector3.one;
        playerCamera = player.transform.Find("Main Camera").GetComponent<Camera>();
        player.GetComponent<PlayerMovement>().PlayerStart();
    }
}
