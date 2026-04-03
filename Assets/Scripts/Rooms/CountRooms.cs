using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Система подсчета и контроля количества комнат на уровне
/// </summary>
public class CountRooms : MonoBehaviour
{
    [Header("Статистика комнат")]
    public static int countRooms;
    public int _countRooms;

    [Header("Настройки генерации")]
    public static int maxRooms = 6;
    public float timeSpawn;

    private void Awake()
    {
        timeSpawn = maxRooms * 0.3f;
    }

    // Основной метод обновления, проверяет условия завершения генерации
    private void Update()
    {
        countRooms = CountActualRooms();
        _countRooms = countRooms;

        UpdateSpawnTimer();
        CheckSpawnCompletion();
    }

    // Метод для подсчета реальных комнат на сцене
    private int CountActualRooms()
    {
        GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");
        return rooms.Length;
    }

    // Обновление таймера спавна комнат
    private void UpdateSpawnTimer()
    {
        if (timeSpawn > 0)
        {
            timeSpawn -= Time.deltaTime;
        }
    }

    // Проверка условий завершения генерации комнат
    private void CheckSpawnCompletion()
    {
        // Условия завершения: закончилось время или достигнут лимит комнат
        if (timeSpawn <= 0 || _countRooms >= maxRooms)
        {
            timeSpawn = 0; // Сбрасываем таймер

            // Сигнализируем о завершении спавна
            Spawner.stopSpawned = true;

            // Если спавн завершен, активируем все стены
            if (Spawner.stopSpawned == true)
            {
                ActivateInactiveWalls();
            }
        }
    }

    // Находит и активирует все неактивные стены на сцене
    private void ActivateInactiveWalls()
    {
        // Находим все объекты на сцене (включая неактивные)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Проверяем что объект - стена и она неактивна
            if (obj.CompareTag("Wall") && !obj.activeSelf)
            {
                obj.SetActive(true); // Активируем стену
            }
        }
    }
}