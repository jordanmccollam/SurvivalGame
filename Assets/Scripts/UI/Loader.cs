using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public Slider slider;

    public void Add(int toAdd) {
        slider.value += toAdd;
    }
}
