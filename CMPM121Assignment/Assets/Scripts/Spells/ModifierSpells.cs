using UnityEngine;
using System.Collections;

// ── Damage Amplifier ──────────────────────────────────────────────────────────
public class DamageAmpSpell : ModifierSpell
{
    private float damageMult;
    private float manaMult;

    public DamageAmpSpell(Spell inner, float damageMult, float manaMult) : base(inner)
    {
        this.damageMult = damageMult;
        this.manaMult   = manaMult;
    }

    public override string GetName()     => inner.GetName() + " (Amplified)";
    public override int    GetDamage()   => Mathf.RoundToInt(inner.GetDamage()   * damageMult);
    public override int    GetManaCost() => Mathf.RoundToInt(inner.GetManaCost() * manaMult);
}

// ── Speed Amplifier ───────────────────────────────────────────────────────────
public class SpeedAmpSpell : ModifierSpell
{
    private float speedMult;
    public float SpeedMult => speedMult;

    public SpeedAmpSpell(Spell inner, float speedMult) : base(inner)
    {
        this.speedMult = speedMult;
    }

    public override string GetName() => inner.GetName() + " (Swift)";

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;
        if (inner is BaseSpell baseSpell)
        {
            var stats = new SpellStats();
            stats.speedModifiers.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, speedMult));
            yield return baseSpell.CastWithStats(where, target, team, stats);
        }
        else
        {
            yield return inner.Cast(where, target, team);
        }
    }
}


// ── Homing ────────────────────────────────────────────────────────────────────
public class HomingSpell : ModifierSpell
{
    private float damageMult;
    private int   manaAdd;

    public HomingSpell(Spell inner, float damageMult, int manaAdd) : base(inner)
    {
        this.damageMult = damageMult;
        this.manaAdd    = manaAdd;
    }

    public override string GetName()     => inner.GetName() + " (Homing)";
    public override int    GetDamage()   => Mathf.RoundToInt(inner.GetDamage() * damageMult);
    public override int    GetManaCost() => inner.GetManaCost() + manaAdd;

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;
        int damage = GetDamage();
        GameManager.Instance.projectileManager.CreateProjectile(0, "homing", where, target - where, 8f,
            (other, impact) => {
                if (other.team != team)
                    other.Damage(new Damage(damage, Damage.Type.ARCANE));
            });
        yield return new WaitForEndOfFrame();
    }
}

// ── CUSTOM 1: Poison
public class PoisonSpell : ModifierSpell
{
    public PoisonSpell(Spell inner) : base(inner) { }

    public override string GetName() => inner.GetName() + " (Poison)";

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;
        int baseDamage = inner.GetDamage();
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 10f,
            (other, impact) => {
                if (other.team != team)
                {
                    other.Damage(new Damage(baseDamage, Damage.Type.ARCANE));
                    CoroutineRunner.Instance.Run(PoisonTick(other));
                }
            });
        yield return new WaitForEndOfFrame();
    }

    System.Collections.IEnumerator PoisonTick(Hittable target)
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1f);
            if (target != null && target.hp > 0)
                target.Damage(new Damage(5, Damage.Type.ARCANE));
        }
    }
}



// ── CUSTOM 3: Frost
public class FrostSpell : ModifierSpell
{
    public FrostSpell(Spell inner) : base(inner) { }

    public override string GetName()   => inner.GetName() + " (Frost)";
    public override int    GetDamage() => Mathf.RoundToInt(inner.GetDamage() * 0.8f);

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;
        int damage = GetDamage();
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 12f,
            (other, impact) => {
                if (other.team != team)
                {
                    other.Damage(new Damage(damage, Damage.Type.ARCANE));
                    var ec = other.owner.GetComponent<EnemyController>();
                    if (ec != null)
                        CoroutineRunner.Instance.Run(SlowEnemy(ec));
                }
            });
        yield return new WaitForEndOfFrame();
    }

    System.Collections.IEnumerator SlowEnemy(EnemyController ec)
    {
        int orig = ec.speed;
        ec.speed = Mathf.RoundToInt(orig * 0.4f);
        yield return new WaitForSeconds(2f);
        if (ec != null) ec.speed = orig;
    }
}