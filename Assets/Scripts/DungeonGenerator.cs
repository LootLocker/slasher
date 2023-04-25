using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    public int currentRoomAmount;
    public int roomAmount;

    public Room roomPrefab;

    public List<Room> rooms = new List<Room>();

    public float waitTime;

    public GameObject player;

    public CinemachineVirtualCamera virtualCamera;

    public EnemyAI enemyPrefab;

    public float enemyPercentageChance;

    public int maxEnemyAmount;

    public GameObject endBlock;

    public bool finished;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Start()
    {
        GenerateRooms();
    }

    public void GenerateRooms()
    {
        StartCoroutine(GenerateRoomsRoutine());
    }

    // Function to generate rooms
    IEnumerator GenerateRoomsRoutine(Vector3 nextRoomPosition = default(Vector3), Quaternion nextRoomRotation = default(Quaternion))
    {
        // Initialize variables
        roomAmount = 2+(PersistentData.instance.DungeonLevel+Mathf.CeilToInt((float)PersistentData.instance.DungeonLevel*0.8f));
        nextRoomPosition = transform.position;
        nextRoomRotation = transform.rotation;
        Vector3 previousExitPosition = Vector3.zero;
        int retries = 5;
        int i = 0;
        int randomRoom = Random.Range(0, rooms.Count);
        bool isStartRoom = true;
        while (i < roomAmount)
        {
            // Stop if the current room amount is larger or equal to the room amount
            if (currentRoomAmount >= roomAmount)
            {
                break;
            }
            Room newRoom = Instantiate(roomPrefab, nextRoomPosition, nextRoomRotation, transform);
            newRoom.Generate(isStartRoom);
            isStartRoom = false;
            rooms.Add(newRoom);


            if (rooms[randomRoom].exitWall != null)
            {
                newRoom.UpdateBlocksList();
                // Remove overlapping walls with previous room, those are okay
                if (i != 0)
                {
                    for (int j = 0; j < newRoom.blocks.Count; j++)
                    {
                        for (int k = 0; k < rooms[randomRoom].blocks.Count; k++)
                        {
                            if (rooms[randomRoom].blocks[k] != null && newRoom.blocks[j] != null && rooms[randomRoom].blocks[k].transform.position == newRoom.blocks[j].transform.position)
                            {
                                Destroy(newRoom.blocks[j]);
                            }
                        }
                    }
                }
                newRoom.UpdateBlocksList();
            }
            bool overlappingWithOtherRoom = false;
            // Check if overlapping with any other room, if so start over
            if (i != 0)
            {
                foreach (var room in rooms)
                {
                    if (room != newRoom)
                    {
                        if (room.IsOverlappingWithOtherRoom(newRoom))
                        {
                            rooms.Remove(newRoom);
                            Destroy(newRoom.gameObject);
                            overlappingWithOtherRoom = true;
                            retries++;
                            break;
                        }
                    }
                }
                if (overlappingWithOtherRoom == false)
                {
                    newRoom.UpdateBlocksList();
                    // All done, good room
                    //Debug.Log("Room is good, place it. Retries:" + retries);
                    retries = 0;

                    rooms[randomRoom].exits.Add(rooms[randomRoom].exitWall);
                    currentRoomAmount++;
                    i++;
                    yield return new WaitForSeconds(waitTime);
                }
                randomRoom = Random.Range(0, rooms.Count);
                rooms[randomRoom].PickNewExit();
                nextRoomPosition = rooms[randomRoom].exitWall.transform.position;
                nextRoomRotation = rooms[randomRoom].exitWall.transform.rotation;
                rooms[randomRoom].exitWall.SetActive(false);
            }
            else
            {
                i++;
                yield return new WaitForSeconds(waitTime);
            }


        }
        yield return null;
        // Rooms are done, now fix exits
        foreach (var room in rooms)
        {
            foreach (var block in room.blocks)
            {
                if (block != null && block.activeInHierarchy == false)
                {
                    block.SetActive(true);
                }
            }
            foreach (var exit in room.exits)
            {
                if (exit != null)
                {
                    exit.SetActive(false);
                    // Create floor at exit
                    room.floors.Add(Instantiate(room.floor, exit.transform.position, Quaternion.identity, room.transform));
                }
            }
            room.UpdateBlocksList();
        }
        foreach (var room in rooms)
        {
            foreach (var floor in room.floors)
            {
                floor.transform.position += Vector3.down * 2.5f;
            }
            room.UpdateBlocksList();
        }

        // -- Place the end block --
        // Pick a floor in the center of the last room
        Room lastRoom = rooms[rooms.Count - 1];
        lastRoom.UpdateBlocksList();
        List<GameObject> floors = new List<GameObject>();
        // Find all floors in the center of the room
        Vector3 middle = Vector3.zero;
        foreach (var floor in lastRoom.floors)
        {
            middle += floor.transform.position;
        }
        // Get the average/middle position
        middle /= lastRoom.floors.Count;

        // Find all floors that are close to the middle
        foreach (var floor in lastRoom.floors)
        {
            if (Vector3.Distance(floor.transform.position, middle) <= 2f)
            {
                floors.Add(floor);
            }
        }
        GameObject floorToPlaceEndBlock = floors[Random.Range(0, floors.Count)];
        Instantiate(endBlock, floorToPlaceEndBlock.transform.position, Quaternion.identity, lastRoom.transform);
        // Delete the floor that was choosen at that position
        Destroy(floorToPlaceEndBlock);

        lastRoom.UpdateBlocksList();    

        yield return new WaitForSeconds(waitTime);

        // Spawn player at the start of the dungeon
        // Get middlepos of room[0] from the floors
        Vector3 middlePos = Vector3.zero;
        foreach (var floor in rooms[0].floors)
        {
            middlePos += floor.transform.position;
        }
        middlePos /= rooms[0].floors.Count;

        // Wait until it's time to play
        yield return new WaitWhile(() => GameController.instance.gameState != GameController.Menu.Game);

        // Spawn player
        GameObject newPlayer = Instantiate(player, middlePos + Vector3.up * 1.25f, player.transform.rotation);
        GameController.instance.playerTransform = newPlayer.transform;
        // Set up camera
        virtualCamera.Follow = newPlayer.GetComponent<PlayerController>().cameraTarget;
        virtualCamera.LookAt = newPlayer.transform;

        // Create enemies
        // Skip first room, since the player starts in that room
        for (int j = 1; j < rooms.Count; j++)
        {
            Room room = rooms[j];
            var randomFloorsList = room.floors.ToList();
   
            float randomChance = Random.Range(0.0f, 100.0f);
            if(randomChance <= enemyPercentageChance)
            {
                int randomEnemyAmount = Random.Range(1, maxEnemyAmount);
                for (int k = 0; k < randomEnemyAmount; k++)
                {
                    var randomFloor = randomFloorsList[Random.Range(0, randomFloorsList.Count)];
                    if (randomFloor != null)
                    {
                        // Spawn and initialize the enemy
                        Vector3 randomFloorPos = randomFloor.transform.position;
                        Instantiate(enemyPrefab, randomFloorPos + Vector3.up, Quaternion.identity).Initialize(newPlayer);
                        randomFloorsList.Remove(randomFloor);
                    }
                }
            }
        }
        // All done!
    }
}
