using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder 
{
    private Dictionary<string, SpellData> spellDatabase;

    public SpellBuilder()
    {
        LoadSpellDatabase();
    }

    private void LoadSpellDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("spells");

        if (jsonFile == null)
        {
            Debug.LogError("Could not find spells.json in Assets/Resources.");
            spellDatabase = new Dictionary<string, SpellData>();
            return;
        }

        spellDatabase = JsonConvert.DeserializeObject<Dictionary<string, SpellData>>(jsonFile.text);

        Debug.Log("Loaded " + spellDatabase.Count + " spells.");

        foreach (var pair in spellDatabase)
        {
            Debug.Log(pair.Key + " -> " + pair.Value.name);
        }
    }

    public SpellData GetSpellData(string id)
    {
        if (spellDatabase.ContainsKey(id))
        {
            return spellDatabase[id];
        }

        Debug.LogWarning("Spell not found: " + id);
        return null;
    }

    public Spell Build(SpellCaster owner)
    {
        SpellData data = GetSpellData("arcane_bolt");
        return new Spell(owner, data);
    }

}
