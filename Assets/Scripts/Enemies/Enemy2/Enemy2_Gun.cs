using System.Collections;
using UnityEngine;

public class Enemy2_Gun : MonoBehaviour
{
    [Header("Target Settings")]
    public LayerMask targetLayer;
    public LayerMask ObstructionLayer;

    [Header("Weapon Settings")]
    public GameObject bullet;
    public Transform shotPoint;

    [Header("Vision Settings")]
    public float radius = 15;
    [Range(1, 360)] public float angle = 160;

    [Header("Shooting Settings")]
    public Transform shotPos;
    public float shotRange = 10;
    public float timeBtwShots;
    public float startTimeBtwShots = 2;

    public bool CanSeePlayer { get; private set; }

    private void Start()
    {
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
                Collider2D[] shot = Physics2D.OverlapCircleAll(shotPos.position, shotRange, targetLayer);
                for (int i = 0; i < shot.Length; i++)
                {
                    if (players[i].enabled && !Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, ObstructionLayer))
                    {
                        if (timeBtwShots <= 0)
                        {
                            Instantiate(bullet, shotPoint.position, transform.rotation);
                            timeBtwShots = startTimeBtwShots;
                        }
                        else
                        {
                            timeBtwShots -= Time.deltaTime;
                        }
                    }
                }
            }
            else
            {
                CanSeePlayer = false;
            }
        }
        else if (CanSeePlayer)
        {
            CanSeePlayer = false;
        }

        //Чтоб перезарядка шла даже когда персонажа нет в зоне
        if(timeBtwShots < 2 && timeBtwShots > 0)
        {
            timeBtwShots -= Time.deltaTime;
        }
    }

    //Отображает зону стрельбы
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(shotPos.position, shotRange);
    }
}
