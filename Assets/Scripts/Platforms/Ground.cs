using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public GameObject top;
    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Start()
    {
        CheckIfTop();
        Invoke("CheckIfTop", 3f); // Second check in case a room was changed in level gen
    }

    void CheckIfTop() {
        Vector2 point = new Vector2(transform.position.x, transform.position.y + 1f);
        Vector2 size = new Vector2(.5f, .5f);
        Collider2D groundDetection = Physics2D.OverlapBox(point, size, 0, whatIsGround);
        if (groundDetection == null) {
            top.SetActive(true);
        } else {
            top.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 point = new Vector2(transform.position.x, transform.position.y + 1f);
        Vector3 size = new Vector3(.5f, .5f, .5f);

        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(point, size);
    }
}
