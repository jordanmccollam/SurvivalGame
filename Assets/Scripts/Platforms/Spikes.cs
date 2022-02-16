using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    public float gravityOnImpact;

    bool playerSpiked = false;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {            
            Player playerComp = other.GetComponent<Player>();
            if (playerComp.isFalling && !playerSpiked) {
                playerSpiked = true; // So trigger doesn't KEEP triggering
                playerComp.rb.gravityScale = gravityOnImpact;
                playerComp.TakeDamage(playerComp.health);
            }
        }
    }
}
