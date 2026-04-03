using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NewBehaviourScript;

public class Bullet_1 : MonoBehaviour, TakeDamage
{
    public float speed = 14;
    public float distance  = 0.5f;
    public int damage = 1;
    public LayerMask whatIsSolid;

    //Çהמנמגüו
    public int health = 1;

    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }

        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, transform.up, distance, whatIsSolid);

        if(hitInfo.collider != null)
        {
            if (hitInfo.collider.CompareTag("Player"))
            {
                hitInfo.collider.GetComponent<Player>().TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
