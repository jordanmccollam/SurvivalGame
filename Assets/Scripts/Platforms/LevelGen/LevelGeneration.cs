using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject startingPlatform;
    public Transform roomParent;
    public Transform[] startingPositions;
    public GameObject[] rooms; // index 0 -> LR, 1 -> LRB, 2 -> LRT, 3 -> LRBT
    public GameObject startingRoom;
    public float moveAmount;
    public float startTimeBtwRoom = 0.25f;
    public float minX;
    public float maxX;
    public float minY;
    public LayerMask room;
    public float loadingBufferTime;

    int direction;
    float timeBtwRoom;
    [HideInInspector] public bool stopGeneration = false;
    int downCounter;
    Vector2 playerSpawnPos;
    Loader loader;

    private void Start() {
        loader = GameObject.FindGameObjectWithTag("Loader").GetComponent<Loader>();
        int randStartPos = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randStartPos].position;
        GameObject firstRoom = Instantiate(startingRoom, transform.position, Quaternion.identity, roomParent);
        playerSpawnPos = firstRoom.GetComponent<RoomType>().playerSpawnPos.position;
        loader.Add(1);

        direction = Random.Range(1, 6);
    }

    private void Update() {
        if (timeBtwRoom <= 0 && stopGeneration == false) {
            Move();
            timeBtwRoom = startTimeBtwRoom;
        } else {
            timeBtwRoom -= Time.deltaTime;
        }
    }

    void Move() {
        if (direction == 1 || direction == 2) { // move RIGHT
            downCounter = 0;

            if (transform.position.x < maxX) {
                Vector2 newPos = new Vector2(transform.position.x + moveAmount, transform.position.y);
                transform.position = newPos;

                int rand = Random.Range(0, rooms.Length);
                Instantiate(rooms[rand], transform.position, Quaternion.identity, roomParent);

                direction = Random.Range(1, 6);
                if (direction == 3) {
                    direction = 2;
                } else if (direction == 4) {
                    direction = 5;
                }
            } else {
                direction = 5;
            }
        }
        else if (direction == 3 || direction == 4) { // move LEFT
            downCounter = 0;

            if (transform.position.x > minX) {
                Vector2 newPos = new Vector2(transform.position.x - moveAmount, transform.position.y);
                transform.position = newPos;

                int rand = Random.Range(0, rooms.Length);
                Instantiate(rooms[rand], transform.position, Quaternion.identity, roomParent);

                direction = Random.Range(3, 6);
            } else {
                direction = 5;
            }
        }
        else if (direction == 5) { // move DOWN
            downCounter++;

            if (transform.position.y > minY) {
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, room);
                RoomType _room = roomDetection.GetComponent<RoomType>();
                if (_room.type != 1 && _room.type != 3) {
                    if (downCounter >= 2) {
                        _room.RoomDestruction();
                        Instantiate(rooms[3], transform.position, Quaternion.identity, roomParent);
                    } else {
                        _room.RoomDestruction();

                        int randBottomRoom = Random.Range(1, 4);
                        if (randBottomRoom == 2) {
                            randBottomRoom = 1;
                        }
                        Instantiate(rooms[randBottomRoom], transform.position, Quaternion.identity, roomParent);
                    }
                }

                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmount);
                transform.position = newPos;

                int rand = Random.Range(2, 4);
                Instantiate(rooms[rand], transform.position, Quaternion.identity, roomParent);

                direction = Random.Range(1, 6);
                loader.Add(1);
            } else {
                // STOP LEVEL GEN
                transform.position = Vector2.zero;
                stopGeneration = true;
                loader.Add(1);
                Invoke("LoadLevel", loadingBufferTime);
            }
        }
    }
    void LoadLevel() {
        Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity);
        Instantiate(startingPlatform, new Vector2(playerSpawnPos.x, playerSpawnPos.y - 1f), Quaternion.identity);
        loader.gameObject.SetActive(false);
    }
}
