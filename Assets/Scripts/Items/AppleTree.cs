using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleTree : MonoBehaviour
{
    [Header("Spawning")]
    public Transform[] spawnPoints;
    public GameObject[] spawnOptions;

    [Header("Other")]
    public ParticleSystem shakeEffect;
    public Transform dropPos;
    public GameObject apple;
    public List<GameObject> apples;
    
    private void Start() {
        SpawnFruit();
    }

    void SpawnFruit() {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // spawn a random "spawn option" (could be an apple or nothing for example)
            int randIndex = Random.Range(0, spawnOptions.Length);
            GameObject instance = Instantiate(spawnOptions[randIndex], spawnPoints[i].position, Quaternion.identity, spawnPoints[i]);
            if (instance.tag == "Apple") {
                apples.Add(instance);
            }
        }
    }

    public void ShakeTree() {
        shakeEffect.Play();

        if (apples.Count > 0) {
            Instantiate(apple, dropPos.position, Quaternion.identity, transform);
            GameObject instance = apples[apples.Count-1];
            apples.Remove(instance);
            Destroy(instance);
        }
    }
}
