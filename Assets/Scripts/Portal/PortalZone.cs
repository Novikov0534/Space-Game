using NavMeshPlus.Components;
using UnityEngine;

/// <summary>
/// Зона портала: после активации очищает все комнаты, врагов и навигационную сетку,
/// спавнит новую главную комнату и инициирует пересборку NavMesh.
/// </summary>
public class PortalZone : MonoBehaviour
{
    [Header("Настройки зоны")]
    public float activationTime = 3f;
    public Color zoneColor = Color.green;
    public float zoneRadius = 2f;

    [Header("Префабы")]
    public GameObject mainRoomPrefab;

    private float playerEnterTime;
    private bool playerInZone;
    private bool activated;
    private GameObject newMainRoomInstance;

    [Header("Настройки портала")]
    public GameObject portalPrefab; // Префаб портала для спавна на игроке

    private GameObject spawnedPortal; // Ссылка на заспавненный портал

    void Start()
    {
        // Автоматически настраиваем триггер
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.radius = zoneRadius;
            collider.isTrigger = true;
        }
    }

    void Update()
    {
        // Проверка активации портала
        if (playerInZone && !activated && Time.time - playerEnterTime >= activationTime)
        {
            ActivatePortal();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activated)
        {
            playerInZone = true;
            playerEnterTime = Time.time;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }

    // Основной процесс активации портала
    private void ActivatePortal()
    {
        activated = true;

        // 0. ПОКАЗАТЬ НАДПИСЬ С ЭТАЖОМ
        if (FloorManager.Instance != null)
        {
            FloorManager.Instance.ShowFloorDisplay();
        }

        // 1. Полностью очищаем старую сетку
        UnityEngine.AI.NavMesh.RemoveAllNavMeshData();

        // 2. Сохраняем позицию игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPosition = player != null ? player.transform.position : Vector3.zero;

        // 3. Удаляем старые комнаты и врагов
        DeleteAllRoomsAndEnemies();

        // 4. Спавним новую главную комнату
        newMainRoomInstance = SpawnNewMainRoom();

        // 5. ИЗМЕНЕНО: Телепортируем игрока в новую главную комнату
        if (player != null && newMainRoomInstance != null)
        {
            // Находим точку спавна в новой комнате
            Transform spawnPoint = FindSpawnPointInNewRoom();
            if (spawnPoint != null)
            {
                player.transform.position = spawnPoint.position;
            }
            else
            {
                // Если нет точки спавна, ставим игрока в центр новой комнаты
                player.transform.position = newMainRoomInstance.transform.position;
            }

            Debug.Log("Игрок перемещен в новую комнату");
        }

        // 6. НОВОЕ: Удаляем портал, если он был заспавнен
        DestroyPortal();

        // 7. Спавним новый портал на позиции игрока (опционально - если нужен портал для следующего уровня)
        // SpawnPortalOnPlayer();

        // 8. Сбрасываем системные переменные
        ResetAllSystems();

        // 9. Сообщаем RoomPostProcess пересобрать сетку
        RoomPostProcess.forceRebuild = true;

        gameObject.SetActive(false);
    }

    // НОВЫЙ МЕТОД: Поиск точки спавна в новой комнате
    private Transform FindSpawnPointInNewRoom()
    {
        if (newMainRoomInstance == null) return null;

        // Ищем объект с тегом "SpawnPoint" внутри новой комнаты
        Transform[] allChildren = newMainRoomInstance.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("SpawnPoint"))
            {
                return child;
            }
        }

        return null;
    }

    // ИЗМЕНЕНО: Спавн портала на позиции игрока с сохранением ссылки
    private void SpawnPortalOnPlayer()
    {
        if (portalPrefab == null)
        {
            Debug.LogWarning("Portal prefab не назначен в инспекторе!");
            return;
        }

        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Удаляем старый портал, если есть
            if (spawnedPortal != null)
            {
                Destroy(spawnedPortal);
            }

            // Спавним новый портал на позиции игрока
            spawnedPortal = Instantiate(portalPrefab, player.transform.position, Quaternion.identity);
            Debug.Log("Портал заспавнен на позиции игрока: " + player.transform.position);
        }
        else
        {
            Debug.LogError("Игрок не найден! Портал не заспавнен.");
        }
    }

    // НОВЫЙ МЕТОД: Уничтожение портала
    private void DestroyPortal()
    {
        if (spawnedPortal != null)
        {
            Destroy(spawnedPortal);
            spawnedPortal = null;
            Debug.Log("Портал уничтожен");
        }
    }

    // Удаляет все комнаты и врагов, кроме только что заспавнённой главной комнаты
    private void DeleteAllRoomsAndEnemies()
    {
        DeleteAllEnemies();
        DeleteAllRooms();
    }

    private void DeleteAllEnemies()
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>(true);
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null && !IsPartOfNewRoom(enemy.transform))
            {
                Destroy(enemy.transform.parent != null ? enemy.transform.parent.gameObject : enemy.gameObject);
            }
        }

        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null) enemyManager.ClearAllEnemies();
    }

    private void DeleteAllRooms()
    {
        // УНИЧТОЖАЕМ ПОРТАЛ ПЕРЕД УДАЛЕНИЕМ КОМНАТ
        DestroyPortal();

        foreach (GameObject room in GameObject.FindGameObjectsWithTag("Room"))
        {
            if (room != null && room != newMainRoomInstance) Destroy(room);
        }

        foreach (GameObject mainRoom in GameObject.FindGameObjectsWithTag("MainRoom"))
        {
            if (mainRoom != null && mainRoom != newMainRoomInstance) Destroy(mainRoom);
        }

        foreach (Spawner spawner in FindObjectsOfType<Spawner>())
        {
            if (spawner != null && !IsPartOfNewRoom(spawner.transform))
                Destroy(spawner.gameObject);
        }

        if (Spawner.persistentParent != null)
        {
            foreach (Transform child in Spawner.persistentParent.transform)
                if (child != null && !IsPartOfNewRoom(child)) Destroy(child.gameObject);
        }

        Spawner.allRooms?.Clear();
    }

    private bool IsPartOfNewRoom(Transform obj)
    {
        return newMainRoomInstance != null && obj.IsChildOf(newMainRoomInstance.transform);
    }

    private void ResetAllSystems()
    {
        Spawner.stopSpawned = false;
        CountRooms.countRooms = 0;
    }

    private GameObject SpawnNewMainRoom()
    {
        if (mainRoomPrefab == null)
        {
            return null;
        }

        GameObject newMainRoom = Instantiate(mainRoomPrefab, Vector3.zero, Quaternion.identity);
        newMainRoom.tag = "MainRoom";
        newMainRoom.name = "MainRoom_New";

        return newMainRoom;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = zoneColor;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position, zoneRadius);
    }
}