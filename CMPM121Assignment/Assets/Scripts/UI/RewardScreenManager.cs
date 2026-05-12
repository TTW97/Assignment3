using TMPro;
using UnityEngine;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public GameObject Next;
    public GameObject Retry;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            Next.SetActive(true);
            Retry.SetActive(false);
            rewardUI.SetActive(true);

            titleText.text = "Wave Cleared";
            messageText.text =
                "Wave cleared\n" +
                "Kills: " + GameManager.Instance.kills + "\n" +
                "Time: " + GameManager.Instance.waveTime + "s\n";
        }
        else if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            Next.SetActive(false);
            Retry.SetActive(true);
            rewardUI.SetActive(true);

            titleText.text = "Game Over";
            messageText.text = "You died.";
        }
        else if (GameManager.Instance.state == GameManager.GameState.GAMEFINISH)
        {
            Next.SetActive(false);
            Retry.SetActive(true);
            rewardUI.SetActive(true);

            titleText.text = "Game Over";
            messageText.text = "You survived.";
        }
        else
        {
            rewardUI.SetActive(false);
        }
    }
}
