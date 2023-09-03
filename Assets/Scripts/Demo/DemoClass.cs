using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        dungeonGenerator.onDungeonClear.AddListener(OnDungeonClear);
        dungeonGenerator.onDungeonGenerate.AddListener(OnDungeonGenerate);
    }

    private void Update()
    {
        //Toggle between orbiting camera and first person view
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (playerCamera) //Sanity check
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

    void OnDungeonClear()
    {
        //Empty all arrays and delete all current rooms
        if (playerCamera)
        {
            Destroy(playerCamera.transform.parent.gameObject);
        }
    }

    void OnDungeonGenerate()
    {
        //DEMO: Create a player inside the first cell of the first room
        GameObject player = Instantiate(playerPrefab, dungeonGenerator.GetRooms()[0].cells[0].center, Quaternion.identity, null);
        player.transform.localScale = Vector3.one;
        playerCamera = player.transform.Find("Main Camera").GetComponent<Camera>();
        player.GetComponent<PlayerMovement>().PlayerStart();
    }
}
