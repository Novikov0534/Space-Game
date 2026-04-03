using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Система спавна комнат и врагов для процедурной генерации уровней
/// Управляет созданием комнат, размещением врагов и обработкой пересечений
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Статические данные")]
    public static List<GameObject> allRooms = new List<GameObject>(); // Список всех созданных комнат
    public static GameObject persistentParent;
    public static bool stopSpawned = false;

    [Header("Направление спавна")]
    public Direction direction;

    [Header("Настройки генерации")]
    private RoomsVariant variantRoom;
    private EnemyVariant variantEnemy;
    private int rnd;
    public bool spawned = false;
    private bool spawnedEnemy = false;

    // Перечисление возможных направлений спавна
    public enum Direction
    {
        Top,
        Bottom,
        Left,
        Right,
        None,
        Centre
    }

    // Инициализация спавнера и создание родительского объекта
    private void Start()
    {
        InitializePersistentParent();
        LoadRoomVariants();

        // Планирование спавна комнат с небольшой задержкой
        Invoke("SpawnRooms", 0.3f);
    }

    // Создание родительского объекта для комнат
    private void InitializePersistentParent()
    {
        if (persistentParent == null)
        {
            persistentParent = new GameObject("PersistentRooms");
            DontDestroyOnLoad(persistentParent);
        }
    }

    // Загрузка вариантов комнат и врагов из сцены
    private void LoadRoomVariants()
    {
        variantRoom = GameObject.FindGameObjectWithTag("Rooms").GetComponent<RoomsVariant>();
        variantEnemy = GameObject.FindGameObjectWithTag("EnemySpawner").GetComponent<EnemyVariant>();
    }

    // Основной метод спавна комнат в указанном направлении
    public void SpawnRooms()
    {
        if (spawned) return; // Защита от повторного спавна
        spawned = true;

        // Проверка лимита комнат
        if (CountRooms.countRooms >= CountRooms.maxRooms)
        {
            stopSpawned = true;

            if (direction != Direction.None && direction != Direction.Centre)
            {
                Destroy(gameObject);
            }
            return;
        }

        GameObject newRoom = CreateRoomBasedOnDirection(); // Создаем комнату

        if (newRoom != null)
        {
            RegisterNewRoom(newRoom); // Регистрируем новую комнату
        }
    }

    private GameObject CreateRoomBasedOnDirection()
    {
        GameObject newRoom = null;

        switch (direction)
        {
            case Direction.Top:
                rnd = Random.Range(0, variantRoom.topRooms.Length);
                newRoom = Instantiate(variantRoom.topRooms[rnd], transform.position, variantRoom.topRooms[rnd].transform.rotation);
                break;

            case Direction.Bottom:
                rnd = Random.Range(0, variantRoom.bottomRooms.Length);
                newRoom = Instantiate(variantRoom.bottomRooms[rnd], transform.position, variantRoom.bottomRooms[rnd].transform.rotation);
                break;

            case Direction.Left:
                rnd = Random.Range(0, variantRoom.leftRooms.Length);
                newRoom = Instantiate(variantRoom.leftRooms[rnd], transform.position, variantRoom.leftRooms[rnd].transform.rotation);
                break;

            case Direction.Right:
                rnd = Random.Range(0, variantRoom.rightRooms.Length);
                newRoom = Instantiate(variantRoom.rightRooms[rnd], transform.position, variantRoom.rightRooms[rnd].transform.rotation);
                break;
        }

        // Уничтожаем спавнер после создания комнаты
        //if (direction != Direction.None && direction != Direction.Centre)
        //{
        //    Destroy(gameObject);
        //}

        return newRoom;
    }

    // Регистрация новой комнаты в системе
    private void RegisterNewRoom(GameObject newRoom)
    {
        allRooms.Add(newRoom);
    }


    // Полный спавн врагов в комнатах с обработкой всех этапов
    public void SpawnEnemiesInRoom()
    {
        if (spawnedEnemy || direction != Direction.Centre) return;

        spawnedEnemy = true;

        for (int i = 0; i < 4; i++)
        {
            // Создание врага
            int enemyIndex = Random.Range(0, variantEnemy.enemies.Length);
            Vector3 spawnPosition = CalculateRandomSpawnPosition();

            GameObject enemy = Instantiate(
                variantEnemy.enemies[enemyIndex],
                spawnPosition,
                variantEnemy.enemies[enemyIndex].transform.rotation
            );
            enemy.SetActive(true);
        }

        // Обновление менеджера врагов
        UpdateEnemyManager();
    }

    // Расчет случайной позиции для спавна врага
    private Vector3 CalculateRandomSpawnPosition()
    {
        float randomX = Random.Range(-13, 13);
        float randomY = Random.Range(-8, 8);
        return transform.position + new Vector3(-randomX, randomY, 0);
    }

    // Обновление менеджера врагов после создания нового врага
    private void UpdateEnemyManager()
    {
        EnemyManager manager = FindObjectOfType<EnemyManager>();
        if (manager != null)
        {
            manager.CacheAllEnemies();
        }
    }

    // Постоянная проверка условий для спавна врагов
    public void Update()
    {
        SpawnEnemiesInRoom();
    }






    // Удаление комнат
    private void OnTriggerEnter2D(Collider2D other)
    {
        Spawner otherSpawner = other.GetComponent<Spawner>();
        if (otherSpawner == null) return;

        // Проверяем условия для удаления
        bool shouldCheckCollision = false;

        // 1. Centre ↔ Centre
        if (this.direction == Direction.Centre && otherSpawner.direction == Direction.Centre)
            shouldCheckCollision = true;

        // 2. Centre ↔ None
        if ((this.direction == Direction.Centre && otherSpawner.direction == Direction.None) ||
            (this.direction == Direction.None && otherSpawner.direction == Direction.Centre))
            shouldCheckCollision = true;

        // 3. None ↔ None
        if (this.direction == Direction.None && otherSpawner.direction == Direction.None)
            shouldCheckCollision = true;

        if (!shouldCheckCollision) return;

        // Определяем, кто спавнился позже
        if (!this.spawned && otherSpawner.spawned)
        {
            Destroy(this.transform.root.gameObject);
            Debug.Log($"[Collision] Destroy later spawned room: {this.transform.root.name}");
        }
        else if (this.spawned && !otherSpawner.spawned)
        {
            Destroy(otherSpawner.transform.root.gameObject);
            Debug.Log($"[Collision] Destroy later spawned room: {otherSpawner.transform.root.name}");
        }
        else
        {
            // Если оба уже заспавнены, используем ID для выбора "поздней" комнаты
            if (this.GetInstanceID() > otherSpawner.GetInstanceID())
            {
                Destroy(this.transform.root.gameObject);
                Debug.Log($"[Collision] Destroy later spawned room (by ID): {this.transform.root.name}");
            }
            else
            {
                Destroy(otherSpawner.transform.root.gameObject);
                Debug.Log($"[Collision] Destroy later spawned room (by ID): {otherSpawner.transform.root.name}");
            }
        }
    }
}