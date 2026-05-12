using System.Collections.Generic;

public class ValueModifier
{
    public enum ModType { ADD, MULTIPLY }
    public ModType type;
    public float value;

    public ValueModifier(ModType type, float value)
    {
        this.type = type;
        this.value = value;
    }

    public static float Apply(List<ValueModifier> modifiers, float baseValue)
    {
        float result = baseValue;
        foreach (var mod in modifiers)
        {
            if (mod.type == ModType.ADD)      result += mod.value;
            else if (mod.type == ModType.MULTIPLY) result *= mod.value;
        }
        return result;
    }
}