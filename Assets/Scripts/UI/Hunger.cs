using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Hunger : MonoBehaviour
{
    public TextMeshProUGUI food;

    public void UpdateUI(int updatedFood) {
        if (updatedFood <= 0) {
            food.text = "0";
        } else {
            food.text = updatedFood.ToString();
        }
    }
}
