using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using RPNEvaluator;

public class SpellBuilder
{
    private static JObject spellsJson;

    static SpellBuilder()
    {
        TextAsset ta = Resources.Load<TextAsset>("spells");
        if (ta == null) { Debug.LogError("Could not find spells.json"); return; }
        spellsJson = JObject.Parse(ta.text);
    }

    public SpellBuilder() { }

    public Spell Build(SpellCaster owner)
    {
        return BuildBase("arcane_bolt", owner);

        //Spell spell = BuildBase("arcane_bolt", owner);

        //spell = ApplyModifier("chaos_trail", spell);

        //return spell;
    }

    public Spell BuildBase(string key, SpellCaster owner)
    {
        if (spellsJson == null || !spellsJson.ContainsKey(key))
        {
            Debug.LogWarning("Spell key not found: " + key);
            return new Spell(owner);
        }

        JObject def = (JObject)spellsJson[key];

        string name     = def["name"]?.ToString()          ?? key;
        int    icon     = def["icon"]?.Value<int>()        ?? 0;
        string mana     = def["mana_cost"]?.ToString()     ?? "10";
        string cooldown = def["cooldown"]?.ToString()      ?? "2";

        JObject dmgObj  = def["damage"] as JObject;
        string  damage  = dmgObj?["amount"]?.ToString()   ?? "10";
        string  dmgType = dmgObj?["type"]?.ToString()     ?? "arcane";

        JObject proj    = def["projectile"] as JObject;
        string  speed   = proj?["speed"]?.ToString()      ?? "8";
        string  traj    = proj?["trajectory"]?.ToString() ?? "straight";
        int     sprite  = proj?["sprite"]?.Value<int>()   ?? 0;

        return new BaseSpell(owner, name, icon, damage, dmgType, mana, cooldown, speed, traj, sprite);
    }

    public Spell BuildRandom(SpellCaster owner)
    {
        Spell spell = BuildBase("arcane_bolt", owner);

        List<string> modKeys = new List<string>
            { "damage_amp", "speed_amp", "doubler", "splitter", "chaos", "homing", "chaos_trail" };

        Debug.Log("RandomBuild");

        int count = 0;
        while (Random.value < 0.6f)
        {
            Debug.Log("in while");
            string mod = modKeys[Random.Range(0, modKeys.Count)];
            spell = ApplyModifier(mod, spell);
            ++count;
            if(count == 5)
            {
                break;
            }
        }
        return spell;
    }

    public Spell ApplyModifier(string key, Spell inner)
    {
        if (spellsJson == null || !spellsJson.ContainsKey(key)) return inner;

        JObject def  = (JObject)spellsJson[key];
        var     vars = MakeVars();

        switch (key)
        {
            case "damage_amp":
                return new DamageAmpSpell(inner,
                    Eval(def["damage_multiplier"], vars, 1.5f),
                    Eval(def["mana_multiplier"],   vars, 1.5f));

            case "speed_amp":
                return new SpeedAmpSpell(inner,
                    Eval(def["speed_multiplier"], vars, 1.75f));

            case "homing":
                return new HomingSpell(inner,
                    Eval(def["damage_multiplier"], vars, 0.75f),
                    Mathf.RoundToInt(Eval(def["mana_adder"], vars, 10f)));

            case "doubler":
                return new DoublerSpell(
                    inner,
                    Eval(def["delay"], vars, 0.5f),
                    Eval(def["mana_multiplier"], vars, 1.5f),
                    Eval(def["cooldown_multiplier"], vars, 1.5f)
                );

            case "splitter":
                return new SplitterSpell(
                    inner,
                    Eval(def["angle"], vars, 10f),
                    Eval(def["mana_multiplier"], vars, 1.5f)
                );

            case "chaos":
                return new ChaosSpell(
                    inner,
                    Eval(def["damage_multiplier"], vars, 1.5f)
                );

            case "chaos_trail":
                return new ChaosTrailSpell(
                    inner,
                    Eval(def["mana_multiplier"], vars, 1.6f),
                    Eval(def["cooldown_multiplier"], vars, 1.25f),
                    Eval(def["spacing"], vars, 1f),
                    Eval(def["trail_lifetime"], vars, 2f)
                );


            default:
                return inner;
        }
    }

    Dictionary<string, int> MakeVars()
    {
        return new Dictionary<string, int>
        {
            { "wave",  GameManager.Instance.waveNumber },
            { "power", GameManager.Instance.playerSpellPower }
        };
    }

    float Eval(JToken token, Dictionary<string, int> vars, float fallback)
    {
        if (token == null) return fallback;
        try { return RPNEvaluator.RPNEvaluator.Evaluatef(token.ToString(), vars); }
        catch { return fallback; }
    }
}