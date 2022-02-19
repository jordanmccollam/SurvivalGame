using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDrop : MonoBehaviour
{
    public int amount;
    public GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            Player player = other.GetComponent<Player>();
            player.PickUpFood(amount);

            Instantiate(pickupEffect, transform.position, Quaternion.identity, transform.parent);
            Destroy(gameObject);
        }
    }
}
