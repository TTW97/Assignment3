using UnityEngine;
using System.Collections;

public class ModifierSpell : Spell
{
    protected Spell inner;

    public ModifierSpell(Spell inner) : base(inner.owner)
    {
        this.inner = inner;
    }

    public override string GetName()     => inner.GetName();
    public override int    GetIcon()     => inner.GetIcon();
    public override int    GetDamage()   => inner.GetDamage();
    public override int    GetManaCost() => inner.GetManaCost();
    public override float  GetCooldown() => inner.GetCooldown();

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;
        yield return inner.Cast(where, target, team);
    }
}