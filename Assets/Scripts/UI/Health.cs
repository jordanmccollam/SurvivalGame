using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public GameObject heart;
    public Transform heartsContainer;
    [HideInInspector]
    public List<GameObject> currentHearts;
    int numOfHearts = 0;

    public void AddHearts(int toAdd) {
        for (var i = 0; i < toAdd; i++)
        {
            GameObject newHeart = Instantiate(heart, heart.transform.position, heart.transform.rotation, heartsContainer);
            currentHearts.Add(newHeart);
            numOfHearts+=1;
        }
    }
    public void RemoveHearts(int toRemove) {
        for (var i = 1; i < (toRemove + 1); i++)
        {
            GameObject dyingHeart = currentHearts[currentHearts.Count - i];
            currentHearts.Remove(dyingHeart);
            Destroy(dyingHeart);
            numOfHearts-=1;
        }
    }
}
