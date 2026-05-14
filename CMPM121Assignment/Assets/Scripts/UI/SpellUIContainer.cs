using UnityEngine;

public class SpellUIContainer : MonoBehaviour
{
    public GameObject[] spellUIs;
    public PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // we only have one spell (right now)
        //spellUIs[0].SetActive(true);
        //for(int i = 1; i< spellUIs.Length; ++i)
        //{
        //    spellUIs[i].SetActive(false);
        //}
    }

    public void Refresh()
    {
        if (player == null || player.spellcaster == null) return;

        for (int i = 0; i < spellUIs.Length; ++i)
        {
            if (i < player.spellcaster.spells.Count)
            {
                spellUIs[i].SetActive(true);

                SpellUI ui = spellUIs[i].GetComponent<SpellUI>();
                ui.slotIndex = i;
                ui.SetSpell(player.spellcaster.spells[i]);

                if (ui.highlight != null)
                    ui.highlight.SetActive(i == player.spellcaster.selectedSpellIndex);
            }
            else
            {
                spellUIs[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
