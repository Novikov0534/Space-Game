using System.Collections;
using UnityEngine;
using NavMeshPlus.Components;

/// <summary>
/// Отвечает за финализацию комнат и пересборку NavMesh.
/// </summary>
public class RoomPostProcess : MonoBehaviour
{
    [Header("Настройки NavMesh")]
    public NavMeshSurface surface;
    public float buildDelay = 1f;

    // Флаг для пересборки после портала
    public static bool forceRebuild = false;

    // Флаг Готовности NAVMESH
    public static bool IsNavMeshReady { get; private set; } = false;

    private void Start()
    {
        // Сбрасываем флаг при старте
        IsNavMeshReady = false;

        // Запускаем пересборку при старте (после генерации комнат)
        StartCoroutine(FinalizeRooms());
    }

    private void Update()
    {
        // Пересборка вручную по кнопке (для теста)
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(FinalizeRooms());
        }

        // Пересборка после входа в портал
        if (forceRebuild)
        {
            forceRebuild = false;
            StartCoroutine(FinalizeRooms());
        }
    }

    // Дожидается окончания генерации комнат и пересобирает NavMesh.
    private IEnumerator FinalizeRooms()
    {
        // Ждём пока генератор закончит спавн комнат
        yield return new WaitUntil(() => Spawner.stopSpawned == true);

        // Небольшая задержка для корректной инициализации объектов
        if (buildDelay > 0)
            yield return new WaitForSeconds(buildDelay);

        // Пересборка навигационной сетки
        if (surface != null)
        {
            surface.collectObjects = CollectObjects.All;
            surface.BuildNavMesh();
        }

        // Переносим комнаты в постоянного родителя
        if (Spawner.allRooms != null && Spawner.persistentParent != null)
        {
            foreach (var room in Spawner.allRooms)
            {
                if (room != null)
                    room.transform.SetParent(Spawner.persistentParent.transform);
            }
        }

        IsNavMeshReady = true;
    }
}