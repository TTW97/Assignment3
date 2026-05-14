using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;

    public GameObject Next;
    public GameObject Retry;

    public GameObject TakeButton;
    public GameObject SkipButton;

    //public GameObject ReplacePanel;
    //public GameObject[] ReplaceButtons;

    private Spell generatedSpell;
    private bool generatedReward;

    public Image rewardSpellIcon;
    public TextMeshProUGUI spellInfoText;

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            ShowWaveReward();
        }
        else if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            Next.SetActive(false);
            Retry.SetActive(true);
            rewardUI.SetActive(true);

            HideRewardButtons();

            titleText.text = "Game Over";
            messageText.text = "You died.";
        }
        else if (GameManager.Instance.state == GameManager.GameState.GAMEFINISH)
        {
            Next.SetActive(false);
            Retry.SetActive(true);
            rewardUI.SetActive(true);

            HideRewardButtons();

            titleText.text = "Game Over";
            messageText.text = "You survived.";
        }
        else
        {
            rewardUI.SetActive(false);
            generatedReward = false;
        }
    }

    void ShowWaveReward()
    {
        rewardUI.SetActive(true);
        Next.SetActive(true);
        Retry.SetActive(false);

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (!generatedReward)
        {
            generatedSpell = new SpellBuilder().BuildRandom(player.spellcaster);
            generatedReward = true;
        }

        titleText.text = "Wave Cleared";

        messageText.text =
            "Wave cleared\n" +
            "Kills: " + GameManager.Instance.kills + "\n" +
            "Time: " + GameManager.Instance.waveTime + "s\n";

        if (rewardSpellIcon != null)
        {
            GameManager.Instance.spellIconManager.PlaceSprite(
                generatedSpell.GetIcon(),
                rewardSpellIcon
            );
        }

        if (spellInfoText != null)
        {
            spellInfoText.text =
                generatedSpell.GetName() + "\n" +
                "Damage: " + generatedSpell.GetDamage() + "\n" +
                "Mana: " + generatedSpell.GetManaCost() + "\n" +
                "Cooldown: " + generatedSpell.GetCooldown().ToString("0.##") + "s";
        }

        //TakeButton.SetActive(true);
        //SkipButton.SetActive(true);

        //ReplacePanel.SetActive(player.spellcaster.spells.Count >= 4);
    }

    public void TakeSpell()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player.spellcaster.spells.Count >= 4)
        {
            Debug.Log("Already have 4 spells.");
            return;
        }

        player.spellcaster.spells.Add(generatedSpell);

        Debug.Log(
            "Added spell: " +
            generatedSpell.GetName() +
            " | Total spells: " +
            player.spellcaster.spells.Count
        );

        player.spellui.Refresh();

        generatedSpell = null;
        generatedReward = false;

        Debug.Log("setbutton flase");
        TakeButton.SetActive(false);
    }

    public void SkipSpell()
    {
        generatedReward = false;
        generatedSpell = null;
    }

    public void ReplaceSpell(int index)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (index < 0 || index >= player.spellcaster.spells.Count) return;

        player.spellcaster.spells[index] = generatedSpell;
        player.spellcaster.selectedSpellIndex = index;

        player.spellui.Refresh();

        generatedReward = false;
        generatedSpell = null;
    }

    void HideRewardButtons()
    {
        if (TakeButton != null) TakeButton.SetActive(false);
        if (SkipButton != null) SkipButton.SetActive(false);
        //if (ReplacePanel != null) ReplacePanel.SetActive(false);
    }
}
