using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "fist") {
            Player player = other.transform.parent.GetComponent<Player>();
            if (player.isPunching) {
                Debug.Log("Broke pot! It gave you: " + "___");
                Destroy(gameObject);
            }
        }
    }
}
