using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Spell
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public Spell(SpellCaster owner, SpellData data) : this(owner) { }

    public virtual string GetName()     => "Bolt";
    public virtual int    GetManaCost() => 10;
    public virtual int    GetDamage()   => 100;
    public virtual float  GetCooldown() => 0.75f;
    public virtual int    GetIcon()     => 0;

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        last_cast = Time.time;
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 15f, OnHit);
        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
    }
}