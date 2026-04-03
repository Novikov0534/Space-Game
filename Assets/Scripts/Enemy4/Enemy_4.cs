using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NewBehaviourScript;
using static UnityEngine.GraphicsBuffer;

public class Enemy_4 : MonoBehaviour, TakeDamage
{
    public int health = 5;

    void LateUpdate()
    {
        // —брасываем вращение дочернего объекта
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}