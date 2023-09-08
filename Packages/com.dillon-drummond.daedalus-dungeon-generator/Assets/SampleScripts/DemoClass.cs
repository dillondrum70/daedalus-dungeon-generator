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
