using System;

[Serializable]
public class SpellData
{
   // "arcane_bolt":  {
   //    "name": "Arcane Bolt",
   //    "description": "A straight-flying bolt.",
   //    "icon": 0,
   //    "damage": {"amount": "25 power 5 / +", "type": "arcane"},
   //    "mana_cost": "10",
   //    "cooldown": "2",
   //    "projectile": {"trajectory": "straight", "speed": "8 power 5 / +", "sprite": 0}
   //}
    public string name;
    public string description;

    // Spell
    public int icon;
    public DamageData damage;
    public string mana_cost;
    public string cooldown;
    public ProjectileData projectile;

    // Only some spells have these
    public string N;
    public string spray;
    public string secondary_damage;
    public ProjectileData secondary_projectile;

    // Modifier fields
    public string damage_multiplier;
    public string mana_multiplier;
    public string speed_multiplier;
    public string cooldown_multiplier;
    public string mana_adder;

    public string delay;
    public string angle;
    public string projectile_trajectory;


    // Chaos trail
    public string spacing;
    public string trail_lifetime;
}

[Serializable]
public class DamageData
{
    public string amount;
    public string type;
}

[Serializable]
public class ProjectileData
{
    public string trajectory;
    public string speed;
    public int sprite;
    public string lifetime;
}
