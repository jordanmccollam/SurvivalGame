using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{
    public TextMeshProUGUI health;

    public void UpdateUI(int updatedHealth) {
        if (updatedHealth <= 0) {
            health.text = "0";
        } else {
            health.text = updatedHealth.ToString();
        }
    }
}
