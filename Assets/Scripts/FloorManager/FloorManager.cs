using UnityEngine;
using TMPro;
using System.Collections;

public class FloorManager : MonoBehaviour
{
    [Header("Настройки этажа ")]
    public int currentFloor = 1;
    public int maxFloors = 5;
    
    [Header("UI Элементы")]
    public TextMeshProUGUI floorText;
    public GameObject floorDisplayPanel;
    public float displayTime = 3f;

    public static FloorManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Вызывается когда игрок входит в портал
    public void ShowFloorDisplay()
    {
        StartCoroutine(DisplayFloorCoroutine());
    }

    private IEnumerator DisplayFloorCoroutine()
    {
        if (floorText != null)
        {
            floorText.text = $"{currentFloor}-{maxFloors}";
        }

        if (floorDisplayPanel != null)
        {
            floorDisplayPanel.SetActive(true);
        }

        yield return new WaitForSeconds(displayTime);

        if (floorDisplayPanel != null)
        {
            floorDisplayPanel.SetActive(false);
        }

        currentFloor++;
        
        if (currentFloor > maxFloors)
        {
            GameCompleted();
        }
    }

    private void GameCompleted()
    {
        Debug.Log("Game Completed! All floors finished.");
    }

    public void ResetFloors()
    {
        currentFloor = 1;
    }
}