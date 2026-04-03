using UnityEngine;
using static NewBehaviourScript;

/// <summary>
/// Управление здоровьем, смертью и респавном врагов
/// </summary>
public class Enemy : MonoBehaviour, TakeDamage
{
    [Header("Настройки здоровья")]
    public int maxHealth = 5;

    private int currentHealth;
    private Vector3 initialPosition;
    private Transform mainEnemyBody;

    // Инициализация врага
    void Start()
    {
        mainEnemyBody = transform.parent;

        if (mainEnemyBody == null)
        {
            return;
        }

        initialPosition = mainEnemyBody.position;
        currentHealth = maxHealth;
        RegisterWithManager();
    }

    // Регистрация при активации
    void OnEnable()
    {
        RegisterWithManager();
    }

    // Регистрация в менеджере врагов
    private void RegisterWithManager()
    {
        EnemyManager manager = FindObjectOfType<EnemyManager>();
        if (manager != null)
        {
            manager.RegisterEnemy(this);
        }
    }

    // Получение урона
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    // Смерть врага
    private void Die()
    {
        if (mainEnemyBody != null)
        {
            mainEnemyBody.gameObject.SetActive(false);
        }
    }

    // Полное восстановление врага
    public void FullReset()
    {
        if (mainEnemyBody == null)
        {
            mainEnemyBody = transform.parent;
            if (mainEnemyBody == null) return;
        }

        mainEnemyBody.position = initialPosition;
        currentHealth = maxHealth;
        mainEnemyBody.gameObject.SetActive(true);
    }
}