using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsGame : MonoBehaviour
{
    [Header("Панель для открытия")]
    public GameObject settingsPanel;

    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            if (settingsPanel.activeSelf == false)
            {
                settingsPanel.SetActive(true);
            }
            else
            {
                settingsPanel.SetActive(false);
            }
        }
    }

    public void OpenMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}