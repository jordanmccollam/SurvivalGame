using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider health;
    public Slider hunger;

    public void SetHealth(int value) {
        health.value = value;
    }
    public void SetMaxHealth(int value) {
        health.maxValue = value;
        SetHealth(value);
    }

    public void SetHunger(int value) {
        hunger.value = value;
    }
    public void SetMaxHunger(int value) {
        hunger.maxValue = value;
        SetHunger(value);
    }
}
