using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealBar : MonoBehaviour
{
    public static int Heal = 5;

    void Start()
    {
        Heal = PlayerPrefs.GetInt("Heal", Heal);
    }
}
