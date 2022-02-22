using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject[] enemies;
    
    private void Start() {
        int rand = Random.Range(0, enemies.Length);
        GameObject instance = Instantiate(enemies[rand], transform.position, Quaternion.identity);
    }
}
