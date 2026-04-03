using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Основной класс управления игроком
/// Обрабатывает движение, здоровье, анимацию и смерть игрока
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Настройки джойстика")]
    public Joystick joystick;            

    [Header("Настроки скорости")]
    public float speed;                     
    float speed_normal = 5;                 
    float speed_max = 15;                    
    float speed_min = 0;                   

    private Rigidbody2D rb;                  
    private Vector2 moveInput;               
    private Vector2 moveVelocity;           

    [Header("Настройки респавна")]
    public Transform respawnPoint;           
    private Vector3 playerInitialPosition;

    [Header("Настройки здоровья")]
    public int health;                       
    public int numOfHearts;                 

    [Header("Спрайт сердец")]
    public Image[] hearts;                   
    public Sprite fullHeart;                 
    public Sprite emptyHeart;                

    [Header("Направление взгляда игрока")]
    private bool facingRighth = true;  

    [Header("Панель")]
    public GameObject panel;

    // Анимация
    private Animator animator;

    // Инициализация компонентов при старте
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        playerInitialPosition = transform.position;
    }

    // Метод для управления анимацией
    private void UpdateAnimation()
    {
        bool isWalking = Mathf.Abs(joystick.Horizontal) > 0.1f || Mathf.Abs(joystick.Vertical) > 0.1f;

        // Передаем значение в Animator
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
        }
    }

    // Обработка ввода и обновление логики каждый кадр
    void Update()
    {
        UpdateHealth();                     
        ProcessMovementInput();              
        HandleCharacterFlip();
        UpdateAnimation();
    }

    // Обновление здоровья игрока
    private void UpdateHealth()
    {
        health = HealBar.Heal; 

        // Восстановление если здоровье ниже 1
        if (HealBar.Heal < 1)
        {
            HealBar.Heal += 5;
        }
    }

    // Обработка ввода движения от джойстика
    private void ProcessMovementInput()
    {
        moveInput = new Vector2(joystick.Horizontal, joystick.Vertical);
        moveVelocity = moveInput.normalized * speed; 

        // Определяем скорость в зависимости от активности джойстика
        if (joystick.Horizontal <= -0.3f || joystick.Horizontal >= 0.3f ||
            joystick.Vertical <= -0.3f || joystick.Vertical >= 0.3f)
        {
            speed = speed_max;               // Максимальная скорость при активном движении
        }
        else if (joystick.Horizontal == 0 && joystick.Vertical == 0)
        {
            speed = speed_min;               // Остановка при отсутствии ввода
        }
        else
        {
            speed = speed_normal;            // Нормальная скорость
        }
    }

    // Обработка поворота персонажа в зависимости от направления движения
    private void HandleCharacterFlip()
    {
        if (!facingRighth && moveInput.x > 0)
        {
            Flip();
        }
        else if (facingRighth && moveInput.x < 0)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();                     
        UpdateHeartsUI();                    
    }

    // Применение физического движения к персонажу
    private void ApplyMovement()
    {
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
    }

    
    // Обновление UI отображения сердец
    private void UpdateHeartsUI()
    {
        // Ограничиваем здоровье максимальным количеством сердец
        if (HealBar.Heal > numOfHearts)
        {
            HealBar.Heal = numOfHearts;
        }

        // Обновляем спрайты сердец в UI
        for (int i = 0; i < hearts.Length; i++)
        {
            // Устанавливаем полное или пустое сердце
            if (i < Mathf.RoundToInt(HealBar.Heal))
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }

            // Включаем/выключаем отображение сердца в зависимости от максимального количества
            if (i < numOfHearts)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }
    }

    // Поворот персонажа по горизонтали
    private void Flip()
    {
        facingRighth = !facingRighth;
        Vector3 Scaler = transform.localScale;
        Scaler.x = Scaler.x * -1;         
        transform.localScale = Scaler;
    }

    // Получение урона игроком
    public void TakeDamage(int damage)
    {
        HealBar.Heal -= damage; 

        // Активируем панель при получении урона
        panel.SetActive(true);
        Time.timeScale = 0f;             
    }

    // Респавн игрока в указанной точке
    public void Respawn()
    {
        // Используем точку респавна если она задана, иначе начальную позицию
        transform.position = respawnPoint != null ? respawnPoint.position : playerInitialPosition;
    }
}