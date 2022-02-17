using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomType : MonoBehaviour
{
    public Transform spawnLoc;
    public int type;

    public void RoomDestruction() {
        Destroy(gameObject);
    }
}
