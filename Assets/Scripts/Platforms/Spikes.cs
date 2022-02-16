using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    bool playerSpiked = false;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {            
            Player playerComp = other.GetComponent<Player>();
            // If player isn't already spiked, and is falling DOWNWARDS
            if (!playerSpiked && playerComp.isFalling && playerComp.rb.velocity.y < 0) {
                playerSpiked = true;
                playerComp.TakeDamage(playerComp.health);
            }
        }
    }
}
