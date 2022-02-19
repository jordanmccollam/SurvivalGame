using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartDrop : MonoBehaviour
{
    // Start is called before the first frame update
    public int amount;
    public GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            Player player = other.GetComponent<Player>();
            player.PickUpHearts(amount);

            Instantiate(pickupEffect, transform.position, Quaternion.identity, transform.parent);
            Destroy(gameObject);
        }
    }
}
