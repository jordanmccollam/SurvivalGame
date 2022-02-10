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

    public void UpdateUI(int playerHealth) {
        for (var i = 0; i < (playerHealth > numOfHearts ? playerHealth : numOfHearts); i++)
        {
            if (i < playerHealth) {
                GameObject newHeart = Instantiate(heart, heart.transform.position, heart.transform.rotation, heartsContainer);
                currentHearts.Add(newHeart);
                numOfHearts+=1;
            } else {
                GameObject dyingHeart = currentHearts[currentHearts.Count-1];
                currentHearts.Remove(dyingHeart);
                Destroy(dyingHeart);
                numOfHearts-=1;
            }
        }
    }
}
