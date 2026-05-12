using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FlowPanelManager : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button actionButton;
    public EnemySpawner enemySpawner;

    private bool returnToStart;

    void Start()
    {
        panel.SetActive(false);
        actionButton.onClick.AddListener(OnActionClicked);
    }

    public void ShowWaveEnd(int wave)
    {
        panel.SetActive(true);
        titleText.text = "Wave Cleared";
        messageText.text = "You survived Wave " + wave + ".";
        actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
        returnToStart = false;
    }

    public void ShowGameOver()
    {
        panel.SetActive(true);
        titleText.text = "Game Over";
        messageText.text = "You died.";
        actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return";
        returnToStart = true;
    }

    public void ShowWin()
    {
        panel.SetActive(true);
        titleText.text = "Victory";
        messageText.text = "You survived all waves.";
        actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return";
        returnToStart = true;
    }

    void OnActionClicked()
    {
        panel.SetActive(false);

        if (returnToStart) {
            Debug.Log("click return");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else {
            enemySpawner.NextWave();
        }
    }
}