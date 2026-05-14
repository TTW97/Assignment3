using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster 
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public Hittable.Team team;
    //public Spell spell;
    public List<Spell> spells = new List<Spell>();
    public int selectedSpellIndex = 0;

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public SpellCaster(int mana, int mana_reg, Hittable.Team team)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.team = team;
        //spell = new SpellBuilder().Build(this);
        spells.Add(new SpellBuilder().Build(this));
    }

    public IEnumerator Cast(Vector3 where, Vector3 target)
    {
        if (spells.Count == 0) yield break;

        if (selectedSpellIndex < 0 || selectedSpellIndex >= spells.Count)
            selectedSpellIndex = 0;

        Spell spell = spells[selectedSpellIndex];

        if (mana >= spell.GetManaCost() && spell.IsReady())
        {
            mana -= spell.GetManaCost();
            yield return spell.Cast(where, target, team);
        }

        yield break;
    }

}
