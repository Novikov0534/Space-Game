using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Cистема ИИ для врагов с улучшенным преследованием
/// </summary>
public class Enemy2_View : MonoBehaviour, IEnemyAI
{
    [Header("Настройки зрения")]
    public float visionRadius = 15f;
    [Range(1, 360)] public float visionAngle = 160f;
    public LayerMask targetLayer;
    public LayerMask obstructionLayer;
    public float rotationOffset = -90f;

    [Header("Настройки движения")]
    public float chaseSpeed = 7f;
    public float stopDistance = 10f;
    private bool facingRight = true;

    [Header("Ссылки на объекты")]
    public GameObject childObject;
    public SpriteRenderer spriteRenderer;
    public Transform playerTarget;

    [Header("Патрулирование")]
    public float startWaitTime = 2;
    public float waitTime;
    public Transform[] moveSpot;
    private int randomSpot;

    [Header("Застревание")]
    public float stuckTime;
    public float maxStuckTime = 2f;
    private Vector3 lastPosition;
    private MoveSpot _moveSpot;

    private NavMeshAgent agent;
    public bool CanSeePlayer { get; private set; }

    private bool isAIActive = false;

    // Инициализация ИИ врага
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stopDistance;

        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }

        waitTime = startWaitTime;
        randomSpot = UnityEngine.Random.Range(0, moveSpot.Length);
        _moveSpot = GetComponentInChildren<MoveSpot>();

        // Начинаем проверку готовности NavMesh
        StartCoroutine(WaitForNavMesh());
    }

    private IEnumerator WaitForNavMesh()
    {
        // Ждем пока NavMesh будет готов
        while (!RoomPostProcess.IsNavMeshReady)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // NavMesh готов - активируем ИИ
        isAIActive = true;
        StartCoroutine(VisionCheck());
    }

    // Постоянная проверка видимости игрока
    private IEnumerator VisionCheck()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            yield return wait;

            // Проверяем только если ИИ активен
            if (isAIActive)
            {
                CheckPlayerVisibility();
            }
        }
    }

    // Проверка видимости игрока с улучшенной логикой
    private void CheckPlayerVisibility()
    {
        if (!isAIActive) return;

        Collider2D[] targetsInRadius = Physics2D.OverlapCircleAll(transform.position, visionRadius, targetLayer);

        if (targetsInRadius.Length > 0)
        {
            Transform target = targetsInRadius[0].transform;
            Vector2 directionToTarget = (target.position - transform.position).normalized;

            if (Vector2.Angle(transform.up, directionToTarget) < visionAngle / 2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);

                if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionLayer))
                {
                    CanSeePlayer = true;

                    if (agent != null && agent.isOnNavMesh)
                    {
                        agent.stoppingDistance = 10; // Дистанция для преследования
                        agent.SetDestination(target.position);
                    }

                    // Поворот при остановке во время преследования
                    if (!facingRight && agent.velocity.x == 0f && playerTarget.transform.position.x < transform.position.x)
                    {
                        Flip();
                    }
                    else if (facingRight && agent.velocity.x == 0f && playerTarget.transform.position.x > transform.position.x)
                    {
                        Flip();
                    }
                }
                else
                {
                    CanSeePlayer = false;
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
    }

    // Патрулирование между точками
    private void Patrol()
    {
        if (!isAIActive || agent == null || !agent.isOnNavMesh) return;

        if (CanSeePlayer == false)
        {
            agent.stoppingDistance = 1.5f; // Дистанция для патрулирования
            agent.SetDestination(moveSpot[randomSpot].position);

            if (Vector2.Distance(transform.position, moveSpot[randomSpot].position) < 1.5f)
            {
                if (waitTime <= 0)
                {
                    randomSpot = UnityEngine.Random.Range(0, moveSpot.Length);
                    waitTime = startWaitTime;
                }
                else
                {
                    waitTime -= Time.deltaTime;
                }
            }

            // Проверка застревания
            if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
            {
                stuckTime += Time.deltaTime;
                if (stuckTime >= maxStuckTime)
                {
                    randomSpot = UnityEngine.Random.Range(0, moveSpot.Length);
                    stuckTime = 0f;
                }
            }
            else
            {
                stuckTime = 0f;
            }

            lastPosition = transform.position;
        }
    }

    // Основной цикл обновления
    private void Update()
    {
        if (!isAIActive) return;

        _moveSpot.DetachFromEnemy();
        spriteRenderer.transform.rotation = Quaternion.identity;

        if (!CanSeePlayer)
        {
            Vector3 rotate = transform.eulerAngles;
            rotate.z = facingRight ? 90 : -90;
            transform.rotation = Quaternion.Euler(rotate);
        }
        else if (playerTarget != null)
        {
            Vector3 difference = playerTarget.position - transform.position;
            float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rotZ + rotationOffset);
        }

        if (childObject == null)
        {
            Destroy(gameObject);
        }

        Patrol();

        // Поворот при движении с проверкой на нулевую скорость
        if (!facingRight && agent.velocity.x < -0.1f && agent.velocity.x != 0)
        {
            Flip();
        }
        else if (facingRight && agent.velocity.x > 0.1f && agent.velocity.x != 0)
        {
            Flip();
        }
    }

    // Переворот спрайта
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Сброс состояния ИИ
    public void ResetAI()
    {
        CanSeePlayer = false;

        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = false;
        }

        waitTime = startWaitTime;
        randomSpot = UnityEngine.Random.Range(0, moveSpot.Length);
        stuckTime = 0f;

        StopAllCoroutines();

        // Перезапускаем корутину ожидания если нужно
        if (!isAIActive)
        {
            StartCoroutine(WaitForNavMesh());
        }
        else
        {
            StartCoroutine(VisionCheck());
        }
    }

    // Визуализация зон в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Vector3 angle01 = DirectionFromAngle(-transform.eulerAngles.z, -visionAngle / 2);
        Vector3 angle02 = DirectionFromAngle(-transform.eulerAngles.z, visionAngle / 2);

        Gizmos.DrawLine(transform.position, transform.position + angle01 * visionRadius);
        Gizmos.DrawLine(transform.position, transform.position + angle02 * visionRadius);

        if (CanSeePlayer && playerTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTarget.position);
        }
    }

    // Расчет направления из угла
    private Vector2 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}