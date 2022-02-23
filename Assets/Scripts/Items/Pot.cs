using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : MonoBehaviour
{
    public GameObject[] drops;
    public GameObject breakEffect;
    public float dropOffScaleY;

    // private void OnTriggerEnter2D(Collider2D other) {
    //     if (other.tag == "fist") {
    //         Player player = other.transform.parent.GetComponent<Player>();
    //         if (player.isPunching) {
    //             BreakPot();
    //         }
    //     }
    // }

    public void BreakPot(AudioManager audio) {
        audio.Play("shatter");
        Instantiate(breakEffect, transform.position, Quaternion.identity);

        GameObject randDrop = drops[Random.Range(0, drops.Length)];
        Instantiate(randDrop, transform.position, Quaternion.identity, transform.parent);

        Destroy(gameObject);
    }
}
