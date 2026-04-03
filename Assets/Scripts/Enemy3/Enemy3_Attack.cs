using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy3_Attack : MonoBehaviour
{
    //Обращение к скрипту 
    public Enemy3_View enemy3_View;

    //Задержка перед атакой
    public float timeBtwAttack = 0.5f;

    //Перезарядка после атаки
    public float startTimeBtwAttack = 2;

    //Провел ли враг атаку
    public bool takeDamage = false;

    //Урон врага
    public int damage;

    //Зона атаки
    public Transform attackPos;
    public float attackRange = 1.5f;

    //Игрок
    public LayerMask Player;

    void Update()
    {
        OnAttack();
    }

    //Атака врага
    public void OnAttack()
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(attackPos.position, attackRange, Player);

        if(enemy3_View.dash == true)
        {
            timeBtwAttack = 0;
            startTimeBtwAttack = 0;
        }
        else 
        {
            startTimeBtwAttack = 2;

            //Срабатывает когда < 0.5 или игрок находится в зоене и урон еще не был нанесен
            if (timeBtwAttack < 0.5f || players.Length > 0 && takeDamage == false)
            {
                timeBtwAttack -= Time.deltaTime;
            }

            //Срабатывает когда задержка <= 0 и урон был нанесен
            if (timeBtwAttack <= 0 && takeDamage == true)
            {
                timeBtwAttack = 0.5f;

                takeDamage = false;
            }

            //Срабатывает когда задержка <= 0 и игрок находится в зоне
            if (timeBtwAttack <= 0 && players.Length > 0)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    players[i].GetComponent<Player>().TakeDamage(damage);

                    timeBtwAttack = startTimeBtwAttack;

                    takeDamage = true;
                }
            }

            if (timeBtwAttack > 0.5f)
            {
                timeBtwAttack -= Time.deltaTime;
            }

            //Срабатывает когда задержка <= 0 и игрок находится не в зоне
            if (timeBtwAttack <= 0)
            {
                timeBtwAttack = 0.5f;
            }
        }
    }

    //Отобржаение зоны атаки
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;    
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }
}
