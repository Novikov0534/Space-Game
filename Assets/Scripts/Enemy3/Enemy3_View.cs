using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy3_View : MonoBehaviour
{
    //Дочерний объект
    public GameObject childObject;

    //Вражеский персонаж
    public Enemy3 enemy;

    //Спрайт врага
    public SpriteRenderer spriteRenderer;

    //Зона зрения
    public float radius = 19;
    [Range(1, 360)] public float angle = 160;

    //Поворот персонажа
    private bool facingRighth = true;
    public float offset = -90;

    //Скорость
    public float speed = 16;

    //Слой на который таргетится поворот
    public LayerMask targetLayer;
    public LayerMask ObstructionLayer;

    public GameObject playerRef;

    //Движение к месту где стоял игрок
    private bool hasTarget = false;
    private Vector2 targetPosition;

    //Совершен ли рывок в этот момент
    public bool dash = false;

    //Проверка на первый рывок
    public bool firstDash = false;

    public bool CanSeePlayer { get; private set; }

    void Start()
    {
        enemy = FindObjectOfType<Enemy3>();

        playerRef = GameObject.FindGameObjectWithTag("Player");

        StartCoroutine(FOVCheck());
    }

    private IEnumerator FOVCheck()
    {
        WaitForSeconds wait = new(0.2f);

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

                    //Если игрок в зоне и у нас нет цели, сохраняем его позицию
                    if (!hasTarget)
                    {
                        targetPosition = playerRef.transform.position;
                        hasTarget = true;
                        dash = false;
                    }

                    //Поворот врага в разные стороны
                    if (enemy != null)
                    {
                        if (!facingRighth && playerRef.transform.position.x < enemy.transform.position.x && dash == false)
                        {
                            Flip();
                        }
                        else if (facingRighth && playerRef.transform.position.x > enemy.transform.position.x && dash == false)
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
            //Игрок находится в зоне, где враг его не видит
            else if(firstDash == true)
            {
                CanSeePlayer = false;

                if (enemy != null)
                {
                    if (!facingRighth && playerRef.transform.position.x < enemy.transform.position.x && dash == false)
                    {
                        Flip();
                    }
                    else if (facingRighth && playerRef.transform.position.x > enemy.transform.position.x && dash == false)
                    {
                        Flip();
                    }
                }
            }
        }
        else
        {
            CanSeePlayer = false;
        }
    }

    //Рывок в сторону игрока
    private void Dash()
    {
        hasTarget = false;
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

    //Разворот модельки противника
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

        //Если игрок в зоне, двигаемся в позицию где он был
        if (hasTarget)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            
            dash = true;

            firstDash = true;

            //Если достигли цели, ждем 2 секунды
            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                dash = false;

                if (!IsInvoking("Dash"))
                {
                    Invoke("Dash", 2f);
                }
            }
        }

        //Зона треугольника следует за игроком
        if (CanSeePlayer == true && dash == false)
        {
            Vector3 difference = playerRef.transform.position - transform.position;
            float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
        }

        //Проверяем, уничтожен ли дочерний объект
        if (childObject == null)
        {
            //Уничтожаем родительский объект
            Destroy(gameObject);
        }
    }
}

