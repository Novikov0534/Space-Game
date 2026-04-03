using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy4_View : MonoBehaviour
{
    // Дочерний объект
    public GameObject childObject;

    //Вражеский персонаж
    public Enemy_4 enemy;

    //Спрайт врага
    public SpriteRenderer spriteRenderer;

    //Зона зрения
    public float radius = 20;
    [Range(1, 360)] public float angle = 160;

    //Перезарядка
    public float timeBtwShots;
    public float startTimeBtwShots = 1;

    //Количество выстрелов
    public int shotCount = 0;

    //Пуля
    public GameObject bullet;

    //Зона стрельбы
    public Transform shotPos;
    public Transform shotPoint;

    //Поворот персонажа
    private bool facingRighth = true;
    public float offset = -90;

    //Слой на который таргетится поворот
    public LayerMask targetLayer;
    public LayerMask ObstructionLayer;

    public GameObject playerRef;

    public bool CanSeePlayer { get; private set; }

    void Start()
    {
        enemy = FindObjectOfType<Enemy_4>();

        playerRef = GameObject.FindGameObjectWithTag("Player");

        StartCoroutine(FOVCheck());
    }

    private IEnumerator FOVCheck()
    {
        WaitForSeconds wait = new(0);

        while (true)
        {
            yield return wait;
            FOV();
        }
    }

    private void FOV()
    {
        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);

        if (rangeCheck.Length > 0)
        {
            Transform target = rangeCheck[0].transform;
            Vector2 directionToTarget = (target.position - transform.position).normalized;

            if (Vector2.Angle(transform.up, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);

                if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, ObstructionLayer))
                {
                    CanSeePlayer = true;
                }
                else
                {
                    CanSeePlayer = false;
                }

                CanSeePlayer = true;

                Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);

                //Если игрок попадает в зону, то враг стреляет в него
                Collider2D[] shot = Physics2D.OverlapCircleAll(shotPos.position, radius, targetLayer);
                for (int i = 0; i < shot.Length; i++)
                {
                    if (players[i].enabled)
                    {
                        if (timeBtwShots <= 0)
                        {
                            shotCount++;

                            if(shotCount == 4)
                            {
                                startTimeBtwShots = 5;

                                shotCount = 0;
                            }
                            else
                            {
                                startTimeBtwShots = 1;
                            }

                            for(int j = 0; j < 3; j++)
                            {
                                Instantiate(bullet, shotPoint.position, transform.rotation);
                                timeBtwShots = startTimeBtwShots;
                            }
                        }
                        else
                        {
                            timeBtwShots -= Time.deltaTime;
                        }
                    }
                }

                //Поворот врага в разные стороны
                if (enemy != null)
                {
                    if (!facingRighth && playerRef.transform.position.x < enemy.transform.position.x)
                    {
                        Flip();
                    }
                    else if (facingRighth && playerRef.transform.position.x > enemy.transform.position.x)
                    {
                        Flip();
                    }
                }
            }
            else
            {
                CanSeePlayer = false; 
            }
        }
        else if(CanSeePlayer)
        {
            CanSeePlayer = false;
        }

        //Чтоб перезарядка шла даже когда персонажа нет в зоне
        else if (CanSeePlayer == false && timeBtwShots < 0.5f || timeBtwShots > 0.5f)
        {
            timeBtwShots -= Time.deltaTime;
        }

        if (timeBtwShots <= 0 && CanSeePlayer == false)
        {
            timeBtwShots = 0.5f;
        }
    }

    //Отображает зону видимости
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.white;
    //    UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, radius);

    //    Vector3 angle01 = DirectionFromAngle(-transform.eulerAngles.z, -angle / 2);
    //    Vector3 angle02 = DirectionFromAngle(-transform.eulerAngles.z, angle / 2);

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawLine(transform.position, transform.position + angle01 * radius);
    //    Gizmos.DrawLine(transform.position, transform.position + angle02 * radius);

    //    if (CanSeePlayer)
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawLine(transform.position, playerRef.transform.position);
    //    }
    //}

    private Vector2 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void Flip()
    {
        facingRighth = !facingRighth;
        Vector3 Scaler = transform.localScale;
        Scaler.x = Scaler.x * -1;
        transform.localScale = Scaler;
    }

    void Update()
    {
        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);

        Vector3 rotate = transform.eulerAngles;

        if (CanSeePlayer == false && facingRighth == true)
        {
            rotate.z = 90;
            transform.rotation = Quaternion.Euler(rotate);
        }
        else if (CanSeePlayer == false && facingRighth == false)
        {
            rotate.z = -90;
            transform.rotation = Quaternion.Euler(rotate);
        }

        //Нормализует поворот спрайта
        spriteRenderer.transform.rotation = Quaternion.identity;

        for (int i = 0; i < rangeCheck.Length; i++)
        {
            if (CanSeePlayer == true)
            {
                Vector3 difference = playerRef.transform.position - transform.position;
                float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
            }
        }

        // Проверяем, уничтожен ли дочерний объект
        if (childObject == null)
        {
            // Уничтожаем родительский объект
            Destroy(gameObject);
        }
    }
}

