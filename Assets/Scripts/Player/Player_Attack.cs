using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NewBehaviourScript;

/// <summary>
/// Система атаки игрока с обнаружением врагов в поле зрения
/// </summary>
public class Player_Attack : MonoBehaviour
{
    [Header("Настройки поля зрения")]
    public float radius = 3;                  
    [Range(1, 360)] public float angle = 160;

    [Header("Настройки слоев")]
    public LayerMask targetLayer;            
    public LayerMask ObstructionLayer;        

    [Header("Цели атаки")]
    public GameObject enemyRef;               
    public List<GameObject> visibleEnemies = new List<GameObject>();

    [Header("Настройки атаки")]
    public float timeBtwAttack;               
    public float startTimeBtwAttack = 2;     
    public int damage = 5;                    

    [Header("Настройки поворота")]
    public float offset = -90;               
    public Joystick joystick;               

    private bool facingRighth = true;        
    private Vector2 moveInput;              

    // Флаг видимости врагов
    public bool CanSeeEnemy { get; private set; }

    // Инициализация системы обнаружения врагов
    void Start()
    {
        StartCoroutine(FOVCheck());
    }

    // Постоянная проверка поля зрения на наличие врагов
    private IEnumerator FOVCheck()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            yield return wait;
            FOV();
        }
    }

    // Проверка поля зрения на наличие видимых врагов
    public void FOV()
    {
        // Ищем все коллайдеры в радиусе на целевом слое
        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);
        visibleEnemies.Clear();

        if (rangeCheck.Length > 0)
        {
            foreach (var enemyCollider in rangeCheck)
            {
                Transform target = enemyCollider.transform;
                Vector2 directionToTarget = (target.position - transform.position).normalized;

                // Проверяем находится ли враг в угле обзора
                if (Vector2.Angle(transform.up, directionToTarget) < angle / 2)
                {
                    float distanceToTarget = Vector2.Distance(transform.position, target.position);

                    // Проверяем нет ли препятствий между игроком и врагом
                    if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, ObstructionLayer))
                    {
                        visibleEnemies.Add(target.gameObject);
                    }
                }
            }

            CanSeeEnemy = visibleEnemies.Count > 0;

            if (CanSeeEnemy)
            {
                enemyRef = GetNearestEnemy();
                if (enemyRef != null)
                {
                    RotateTowards(enemyRef.transform);
                }
            }
        }
        else
        {
            CanSeeEnemy = false;
        }
    }

    // Поиск ближайшего врага из списка видимых
    GameObject GetNearestEnemy()
    {
        GameObject nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in visibleEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    // Поворот персонажа в сторону цели
    void RotateTowards(Transform target)
    {
        Vector3 difference = target.position - transform.position;
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
    }

    // Обработка нажатия кнопки атаки
    public void OnAttackButtonDown()
    {
        // Ищем все цели в радиусе атаки
        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);

        if (rangeCheck.Length > 0)
        {
            foreach (var targetCollider in rangeCheck)
            {
                Transform target = targetCollider.transform;
                Vector2 directionToTarget = (target.position - transform.position).normalized;

                // Проверяем находится ли цель в угле атаки
                if (Vector2.Angle(transform.up, directionToTarget) < angle / 2)
                {
                    float distanceToTarget = Vector2.Distance(transform.position, target.position);

                    // Проверяем нет ли препятствий и готовность к атаке
                    if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, ObstructionLayer) && timeBtwAttack <= 0)
                    {
                        // Проверяем можно ли нанести урон цели
                        TakeDamage damageable = targetCollider.GetComponent<TakeDamage>();
                        if (damageable != null)
                        {
                            damageable.TakeDamage(damage); // Наносим урон
                        }

                        timeBtwAttack = startTimeBtwAttack; // Запускаем перезарядку
                    }
                }
            }
        }
    }

    // Обновление логики атаки и поворота
    void Update()
    {
        // Тестовая атака по клавише Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnAttackButtonDown();
            Debug.Log("Атака");
        }

        // Обновление таймера перезарядки
        if (timeBtwAttack <= 0)
        {
            timeBtwAttack = 0;
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
        }

        HandleCharacterRotation();
    }


    // Обработка поворота персонажа ввода с джойстика
    private void HandleCharacterRotation()
    {
        moveInput = new Vector2(joystick.Horizontal, joystick.Vertical);
        Vector3 rotate = transform.eulerAngles;

        // Поворот вправо
        if (!facingRighth && moveInput.x > 0)
        {
            facingRighth = !facingRighth;
            rotate.z = 90;
            transform.rotation = Quaternion.Euler(rotate);
        }
        // Поворот влево
        else if (facingRighth && moveInput.x < 0)
        {
            facingRighth = !facingRighth;
            rotate.z = -90;
            transform.rotation = Quaternion.Euler(rotate);
        }

        // Фиксация направления взгляда
        if (facingRighth)
        {
            rotate.z = -90;
            transform.rotation = Quaternion.Euler(rotate);
        }
        else if (!facingRighth)
        {
            rotate.z = 90;
            transform.rotation = Quaternion.Euler(rotate);
        }
    }

    // Визуализация
    private void OnDrawGizmos()
    {
        // Визуализация радиуса обнаружения
        Gizmos.color = Color.white;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, radius);

        // Визуализация углов обзора
        Vector3 angle01 = DirectionFromAngle(-transform.eulerAngles.z, -angle / 2);
        Vector3 angle02 = DirectionFromAngle(-transform.eulerAngles.z, angle / 2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + angle01 * radius);
        Gizmos.DrawLine(transform.position, transform.position + angle02 * radius);

        // Визуализация линии к врагу
        if (CanSeeEnemy && enemyRef != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, enemyRef.transform.position);
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }
}