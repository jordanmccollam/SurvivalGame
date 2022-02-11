using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            Player playerComp = other.GetComponent<Player>();
            if (playerComp.isJumping && playerComp.canLedgeJump) {
                playerComp.anim.SetTrigger("grabLedge");
            }
        }
    }
}
