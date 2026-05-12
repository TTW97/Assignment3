using System.Collections.Generic;

public class SpellStats
{
    public List<ValueModifier> damageModifiers  = new List<ValueModifier>();
    public List<ValueModifier> manaModifiers    = new List<ValueModifier>();
    public List<ValueModifier> cooldownModifiers = new List<ValueModifier>();
    public List<ValueModifier> speedModifiers   = new List<ValueModifier>();
    public string trajectory;
    public int sprite;
}