using UnityEngine;

/// <summary>
/// Система управления дверьми и обработки их взаимодействия с окружением
/// </summary>
public class Doors : MonoBehaviour
{
    [Header("Настройки проверки")]
    public float checkRadius = 1.3f; 

    // Проверка условий для активации проверки дверей
    private void Update()
    {
        // Проверяем двери только после завершения генерации уровня
        if (Spawner.stopSpawned == true)
        {
            DoorCheck();
        }
    }

    // Проверка взаимодействия двери с другими объектами
    public void DoorCheck()
    {
        // Получаем все коллайдеры в радиусе проверки
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, checkRadius);

        //bool isTouchingSomething = false;

        foreach (Collider2D collider in colliders)
        {
            // Игнорируем сам объект двери
            if (collider.gameObject != gameObject)
            {
                // Если дверь касается другой двери
                if (collider.CompareTag("Door"))
                {
                    //isTouchingSomething = true;
                    CheckForWallAtStart(); // Проверяем и удаляем стены
                    break;
                }
            }
        }

        // Удаление двери если она не касается другой двери
        // if (isTouchingSomething == false)
        // {
        //     Destroy(gameObject);
        // }
    }

    // Проверка и удаление стен в зоне расположения двери
    private void CheckForWallAtStart()
    {
        // Получаем все коллайдеры в радиусе проверки
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, checkRadius);

        foreach (Collider2D collider in colliders)
        {
            // Если найден объект с тегом Wall, удаляем его
            if (collider.CompareTag("Wall"))
            {
                Destroy(collider.gameObject);
            }
        }
    }

    // Отрисовка радиуса проверки в редакторе Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}