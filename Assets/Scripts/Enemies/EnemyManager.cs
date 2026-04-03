using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Централизованное управление всеми врагами на сцене
/// </summary>
public class EnemyManager : MonoBehaviour
{
    private List<Enemy> allEnemies = new List<Enemy>(); // Список всех врагов
    private List<Enemy> killedEnemies = new List<Enemy>(); // Cписок убитых врагов
    private bool allEnemiesKilled = false;

    [Header("Настройки портала")]
    public GameObject portalPrefab;
    private GameObject currentPortal;

    // Инициализация - кэширование всех врагов при старте
    void Start()
    {
        CacheAllEnemies();
    }

    // Постоянная проверка состояния врагов
    void Update()
    {
        CheckAllEnemiesKilled();
    }

    // Регистрация нового врага в системе
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy != null && !allEnemies.Contains(enemy))
        {
            allEnemies.Add(enemy);
            allEnemiesKilled = false;

            // Удаление портала при появлении новых врагов
            if (currentPortal != null)
            {
                Destroy(currentPortal);
                currentPortal = null;
            }
        }
    }

    // Регистрация убитого врага
    public void RegisterKilledEnemy(Enemy enemy)
    {
        if (enemy != null && !killedEnemies.Contains(enemy))
        {
            killedEnemies.Add(enemy);
        }
    }

    // Очистка списка убитых врагов
    public void ClearKilledEnemies()
    {
        killedEnemies.Clear();
    }

    // Полная очистка всех списков врагов
    public void ClearAllEnemies()
    {
        allEnemies.Clear();
        killedEnemies.Clear();
        allEnemiesKilled = false;

        // Удаление портала
        if (currentPortal != null)
        {
            Destroy(currentPortal);
            currentPortal = null;
        }
    }

    // Проверка уничтожения всех врагов
    private void CheckAllEnemiesKilled()
    {
        if (allEnemiesKilled || allEnemies.Count == 0) return;

        // Поиск живых врагов
        bool anyAlive = allEnemies.Any(enemy =>
            enemy != null &&
            enemy.transform.parent != null &&
            enemy.transform.parent.gameObject.activeSelf
        );

        // Создание портала если все враги убиты
        if (!anyAlive)
        {
            allEnemiesKilled = true;
            SpawnPortalOnPlayer(); // ИЗМЕНЕНО: теперь вызываем спавн на игроке
        }
    }

    // ИЗМЕНЕНО: Создание портала на позиции игрока
    private void SpawnPortalOnPlayer()
    {
        if (portalPrefab != null && currentPortal == null)
        {
            // Находим игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                // Спавним портал на позиции игрока
                currentPortal = Instantiate(portalPrefab, player.transform.position, Quaternion.identity);
                Debug.Log("Портал заспавнен на игроке!");
            }
            else
            {
                Debug.LogError("Игрок не найден! Портал не заспавнен.");

                // Запасной вариант - спавн в центре если игрок не найден
                currentPortal = Instantiate(portalPrefab, Vector3.zero, Quaternion.identity);
            }
        }
    }

    // Обновление списка всех врагов на сцене
    public void CacheAllEnemies()
    {
        allEnemies.Clear();
        var foundEnemies = FindObjectsOfType<Enemy>(true);
        allEnemies.AddRange(foundEnemies);
    }

    // Полный респавн всех врагов и сброс портала
    public void ResetAllEnemies()
    {
        CacheAllEnemies();
        allEnemiesKilled = false;

        // Удаление портала при респавне врагов
        if (currentPortal != null)
        {
            Destroy(currentPortal);
            currentPortal = null;
        }

        // Восстановление каждого основного врага
        foreach (var enemy in allEnemies.ToList())
        {
            if (enemy != null && enemy.transform.parent != null)
            {
                enemy.FullReset();
            }
        }

        // Восстановление убитых врагов (для обычного рестарта)
        foreach (var killedEnemy in killedEnemies.ToList())
        {
            if (killedEnemy != null && killedEnemy.transform.parent != null)
            {
                killedEnemy.FullReset();
            }
        }
    }
}