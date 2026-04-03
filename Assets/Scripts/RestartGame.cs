using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Система перезапуска игры при нажатии на кнопку рестарта
/// </summary>
public class RestartGame : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки интерфейса")]
    public GameObject deathPanel;

    [Header("Ссылки на объекты")]
    private EnemyManager enemyPosition;      
    private Player playerPosition;         

    // Поиск и сохранение ссылок на необходимые компоненты при старте
    void Start()
    {
        enemyPosition = FindObjectOfType<EnemyManager>();
        playerPosition = FindObjectOfType<Player>();
    }

    // Обработчик клика по кнопке рестарта
    public void OnPointerClick(PointerEventData eventData)
    {
        RestartLevel();
    }

    // Полный перезапуск игрового уровня
    private void RestartLevel()
    {
        playerPosition.Respawn();            
        enemyPosition.ResetAllEnemies();     
        deathPanel.SetActive(false);  
 
        Time.timeScale = 1f;              
    }
}