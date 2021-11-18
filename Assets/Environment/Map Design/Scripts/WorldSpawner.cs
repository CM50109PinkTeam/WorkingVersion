using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpawner : MonoBehaviour
{

    [SerializeField]
    private float roomHeight;
    [SerializeField]
    private float roomWidth;
    [SerializeField]
    private int numRoomsHor;
    [SerializeField]
    private int numRoomsVer;

    [SerializeField]
    private int chanceToMoveDown = 10;
    [SerializeField]
    private float timeBetweenRoomSpawns = 2.0f;


    public GameObject[] rooms;
    public GameObject borderBlock;


    private float timeElapsedSinceLastRoom;
    private GameObject[] roomSpawnPointsArray;
    private int firstRoomPosition;
    private bool reachedExit = false;
    private Vector2 nextRoomPosition;

    private bool canMoveRight = true;
    private bool canMoveLeft = true;
    private bool lastRoomWasBottom = false;
    private enum Direction { Down, Left, Right, ImpossibleMove };
    public LayerMask Room;
    public LayerMask GroundTileLayer;

    GameObject lastRoomCreated;
    public GameObject door;


    // Start is called before the first frame update
    void Start()
    {
        GenerateBorders();
        SpawnEntranceRoom();

    }
    /// <summary>
    /// Generates the one tile border around the map
    /// </summary>
    private void GenerateBorders()
    {
        float mapWidth = roomWidth * numRoomsHor;
        float mapHeight = roomHeight * numRoomsVer;
        GameObject borders = new GameObject();
        borders.name = "Borders of Map";

        //bottom Border
        GameObject bottomBorder = Instantiate(borderBlock, new Vector2((roomWidth * numRoomsHor) / 2, transform.position.y - 0.5f), Quaternion.identity);
        bottomBorder.GetComponent<SpriteRenderer>().size = new Vector2(mapWidth + 2, 1);
        bottomBorder.transform.parent = borders.transform;

        //top Border
        GameObject topBorder = Instantiate(borderBlock, new Vector2((roomWidth * numRoomsHor) / 2, transform.position.y + roomHeight * numRoomsVer + 0.5f), Quaternion.identity);
        topBorder.GetComponent<SpriteRenderer>().size = new Vector2(mapWidth + 2, 1);
        topBorder.transform.parent = borders.transform;

        //Right Border
        GameObject rightBorder = Instantiate(borderBlock, new Vector2(transform.position.x - 0.5f, (roomHeight * numRoomsVer) / 2), Quaternion.identity);
        rightBorder.GetComponent<SpriteRenderer>().size = new Vector2(1, mapHeight);
        rightBorder.transform.parent = borders.transform;

        //Left Border
        GameObject leftBorder = Instantiate(borderBlock, new Vector2((roomWidth * numRoomsHor) + 0.5f, (roomHeight * numRoomsVer) / 2), Quaternion.identity);
        leftBorder.GetComponent<SpriteRenderer>().size = new Vector2(1, mapHeight);
        leftBorder.transform.parent = borders.transform;
    }

    /// <summary>
    /// Function No Longer used, Keeping it here incase we need it in for future 
    /// </summary>
    private void GenerateRoomSpawnPoints()
    {

        roomSpawnPointsArray = new GameObject[numRoomsVer * numRoomsHor];
        for (int x = 0; x < numRoomsHor; x++)
        {
            for (int y = 0; y < numRoomsVer; y++)
            {
                Vector2 roomSpawnPosition = new Vector2(transform.position.x + (x * roomWidth), transform.position.y - (y * roomHeight));
                GameObject roomSpawnPoint = new GameObject("Room Spawn Point X:" + x + " Y:" + y);
                roomSpawnPoint.transform.position = roomSpawnPosition;
                roomSpawnPointsArray[x + (y * numRoomsVer)] = roomSpawnPoint;
            }
        }


    }

    /// <summary>
    /// Function responsible for spawing entrace room on first row.
    /// </summary>
    private void SpawnEntranceRoom()
    {
        //pick entrance room location
        firstRoomPosition = Random.Range(0, numRoomsHor);

        int roomType = 0;
        Vector2 firstRoomSpawnPosition = new Vector2(transform.position.x + (firstRoomPosition * roomWidth), roomHeight * (numRoomsVer - 1));

        //spawn starting room at random location on first row
        GameObject entranceRoom = Instantiate(rooms[roomType], firstRoomSpawnPosition, Quaternion.identity);
        //sets name in object inspector
        entranceRoom.name = "Entrance";
        nextRoomPosition = firstRoomSpawnPosition;
        SpawnDoor(nextRoomPosition, "Entrance");

    }

    /// <summary>
    /// Spawns a door randomly in the given room and sets its name in the object inspector
    /// </summary>
    /// <param name="roomPosition"></param>
    /// <param name="objectName"></param>
    private void SpawnDoor(Vector3 roomPosition, string objectName)
    {
        StartCoroutine(ExecuteAfterTime(1));
        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);

            roomPosition += new Vector3(0.5f, 0.5f, 0);
            List<Vector3> potentialDoorSpawnLocations = new List<Vector3>();

            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight - 2; y++)
                {
                    Vector3 rayCastOrigin = roomPosition + new Vector3(x, y, 0);
                    RaycastHit2D checkForAirHit = Physics2D.Raycast(rayCastOrigin, transform.TransformDirection(Vector2.up), 1f);
                    RaycastHit2D checkForSolidBlock = Physics2D.Raycast(rayCastOrigin, transform.TransformDirection(-Vector2.up), 0.6f, GroundTileLayer);
                    
                    if (checkForAirHit == false && checkForSolidBlock)
                    {
                        potentialDoorSpawnLocations.Add(rayCastOrigin);
                    }
                }
            }

            int randDoor = Random.Range(0, potentialDoorSpawnLocations.Count);
            GameObject doorObj = Instantiate(door, (potentialDoorSpawnLocations[randDoor] + new Vector3(0, 0, 0)), Quaternion.identity);
          
            doorObj.name = objectName;

           
        }


    }

    /// <summary>
    /// Generates Critical path for room spawning and spawns the rooms along the path.
    /// </summary>
    private void GenerateNextMove()
    {
        //random chance to move in a direction
        int directionToMove = Random.Range(0, 100);
        //selects a random room type
        //int roomType = Random.Range(0, rooms.Length);
        int roomType = 2;
        Direction direction = Direction.ImpossibleMove;

        if (directionToMove > chanceToMoveDown)
        {
            //if we can either move left or right we choose randomly
            if ((canMoveLeft == true && nextRoomPosition.x > 0) && canMoveRight == true && nextRoomPosition.x < (roomWidth * (numRoomsHor - 1)))
            {
                int leftOrRight = Random.Range(0, 2);
                if (leftOrRight == 0)
                {
                    direction = Direction.Left;
                    canMoveRight = false;
                }
                else
                {
                    direction = Direction.Right;
                    canMoveLeft = false;
                }
            }
            //if we can only move left we set direction to left
            else if (canMoveLeft == true && nextRoomPosition.x > 0) //left
            {
                direction = Direction.Left;
                canMoveRight = false;
            }
            //if we can only move right we set direction to right
            else if (canMoveRight == true && nextRoomPosition.x < (roomWidth * (numRoomsHor - 1))) //right
            {
                direction = Direction.Right;
                canMoveLeft = false;
            }
            else
            {
                direction = Direction.Down;
                //if we move down we can move either left or right again next room
                canMoveLeft = true;
                canMoveRight = true;
            }

        }
        else
        {
            //if we move down we can move either left or right again next
            canMoveLeft = true;
            canMoveRight = true;
            direction = Direction.Down;
            roomType = Random.Range(0, 2);
        }


        if (direction == Direction.Left)
        {
            nextRoomPosition.x -= roomWidth;
            lastRoomWasBottom = false;
        }
        else if (direction == Direction.Right)
        {
            nextRoomPosition.x += roomWidth;
            lastRoomWasBottom = false;
        }
        else if (direction == Direction.Down)
        {

            /*checks if the last room spawned has a bottom exit, 
            if not we destory it and replace it with one that does */
            if (nextRoomPosition.y > 0)
            {
                Collider2D roomDetection = Physics2D.OverlapCircle(nextRoomPosition, 1, Room);
                if (roomDetection != null)
                {


                    if (roomDetection.GetComponent<RoomType>().roomType != RoomType.RoomEntrances.LRB &&
                        roomDetection.GetComponent<RoomType>().roomType != RoomType.RoomEntrances.LRTB)
                    {
                        roomDetection.GetComponent<RoomType>().destroyRoom();

                        int randomRoomWithBottom = Random.Range(0, 2);
                        if (randomRoomWithBottom == 0)
                        {
                            GameObject roomCreated = Instantiate(rooms[0], nextRoomPosition, Quaternion.identity);
                            roomCreated.name = "Critical Path Room";
                        }
                        else
                        {
                            GameObject roomCreated = Instantiate(rooms[1], nextRoomPosition, Quaternion.identity);
                            roomCreated.name = "Critical Path Room";
                        }

                    }
                }
            }


            nextRoomPosition.y -= roomHeight;
            lastRoomWasBottom = true;
            roomType = 0;

        }

        if (direction != Direction.ImpossibleMove)
        {
            if (nextRoomPosition.y < 0)
            {
                reachedExit = true;
                Debug.Log("Finished Critical Path Generation");
                lastRoomCreated.name = "Exit";
                SpawnDoor(nextRoomPosition + new Vector2(0f,roomHeight), "Exit");
                generateNonCriticalRooms();
            
            }
            else
            {
                lastRoomCreated = Instantiate(rooms[roomType], nextRoomPosition, Quaternion.identity);
                lastRoomCreated.name = "Critical Path Room";

            }
        }


    }
    /// <summary>
    /// Loops Over every room position and spawns a room if its empty
    /// </summary>

    private void generateNonCriticalRooms()
    {
        for (int x = 0; x < numRoomsHor; x++)
        {
            for (int y = 0; y < numRoomsVer; y++)
            {
                Vector2 checkRoomPostion = new Vector2(x * roomWidth, y * roomHeight);
                Collider2D roomDetection = Physics2D.OverlapCircle(checkRoomPostion, 1, Room);
                int randomRoom = Random.Range(0, 4);
                if (roomDetection == null)
                {
                    GameObject fillerRoom = Instantiate(rooms[randomRoom], checkRoomPostion, Quaternion.identity);
                    fillerRoom.name = "Padding Room";
                }
            }
        }
    }
    /// <summary>
    /// Spawns a room at a given spawn point and the type of room to be spawned.
    /// Currently Unused
    /// </summary>
    /// <param name="roomSpawnPoint"></param>
    /// <param name="roomType"></param>  
    private void SpawnRoom(GameObject roomSpawnPoint, int roomType = 0)
    {
        Instantiate(rooms[roomType], roomSpawnPoint.transform.position, Quaternion.identity);
    }

    /// <summary>
    /// 
    /// </summary>
    // Update is called once per frame
    void Update()
    {
        if (timeElapsedSinceLastRoom <= 0 && reachedExit == false)
        {
            GenerateNextMove();
            timeElapsedSinceLastRoom = timeBetweenRoomSpawns;
        }
        else
        {
            timeElapsedSinceLastRoom -= Time.deltaTime;
        }
    }


}
