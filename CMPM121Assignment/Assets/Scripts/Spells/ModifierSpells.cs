using UnityEngine;
using System.Collections;
using UnityEditor;

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

// -- Doubler ────────────────────────────────────────────────────────────────────
//"doubler": {
//       "name": "doubled",
//       "description": "Spell is cast a second time after a small delay; increased mana cost and cooldown.",
//       "delay": "0.5",
//       "mana_multiplier": "1.5",
//       "cooldown_multiplier": "1.5"
//   }
public class DoublerSpell : ModifierSpell
{
    private float delay;
    private float mana_Multiplier;
    private float cooldown_multiplier;
    public DoublerSpell(Spell inner, float delay, float mana_multiplier, float cooldown_multiplier) : base(inner)
    {
        this.delay = delay;
        this.mana_Multiplier = mana_multiplier;
        this.cooldown_multiplier = cooldown_multiplier;
    }

    public override string GetName()
    {
        return inner.GetName() + " (Doubler)";
    }

    public override int GetManaCost()
    {
        return Mathf.RoundToInt(inner.GetManaCost() * mana_Multiplier);
    }

    public override float GetCooldown()
    {
        return inner.GetCooldown() * cooldown_multiplier;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;
        yield return inner.Cast(where, target, team);
        yield return new WaitForSeconds(delay);
        yield return inner.Cast(where, target, team);
    }


}
// Splitter ────────────────────────────────────────────────────────────────────
//"splitter": {
//    "name": "split",
//       "description": "Spell is cast twice in slightly different directions; increased mana cost.",
//       "angle": "10",
//       "mana_multiplier": "1.5"
//   }
public class SplitterSpell : ModifierSpell
{
    private float angle;
    private float mana_multiplier;

    public SplitterSpell(Spell inner, float angle, float mana_multiplier) : base(inner)
    {
        this.angle = angle;
        this.mana_multiplier = mana_multiplier;
    }

    public override string GetName()
    {
        return inner.GetName() + " (Split)";
    }

    public override int GetManaCost()
    {
        return Mathf.RoundToInt(inner.GetManaCost() * mana_multiplier);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;

        Vector3 direction = target - where;

        float randomAngle1 = Random.Range(-angle, angle);
        float randomAngle2 = Random.Range(-angle, angle);

        Vector3 dir1 = Quaternion.Euler(0, 0, randomAngle1) * direction;
        Vector3 dir2 = Quaternion.Euler(0, 0, randomAngle2) * direction;

        yield return inner.Cast(where, where + dir1, team);
        yield return inner.Cast(where, where + dir2, team);
    }
}
// Chaos ────────────────────────────────────────────────────────────────────
//"chaos": {
//    "name": "chaotic",
//       "description": "Significantly increased damage, but projectile is spiraling.",
//       "damage_multiplier": "1.5 wave 5 / +",
//       "projectile_trajectory": "spiraling"
//   }
public class ChaosSpell : ModifierSpell
{
    private float damage_multiplier;

    public ChaosSpell(Spell inner, float damage_multiplier) : base(inner)
    {
        this.damage_multiplier = damage_multiplier;
    }

    public override string GetName()
    {
        return inner.GetName() + " (Chaotic)";
    }
    public override int GetDamage()
    {
        return Mathf.RoundToInt(inner.GetDamage() * damage_multiplier);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;

        int damage = GetDamage();

        GameManager.Instance.projectileManager.CreateProjectile(0, "spiraling", where, target - where, 8f, 
            (other, impact) =>
            {
                if (other.team != team)
                {
                    other.Damage(new Damage(damage, Damage.Type.ARCANE));
                }
            }
        );

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


// CUSTOM 2: Chaos Trail
public class ChaosTrailSpell : ModifierSpell
{
    private float mana_multiplier;
    private float cooldown_multiplier;
    private float spacing;
    private float trail_lifetime;

    public ChaosTrailSpell(Spell inner, float mana_multiplier, float cooldown_multiplier, float spacing, float trail_lifetime) : base(inner)
    {
        this.mana_multiplier = mana_multiplier;
        this.cooldown_multiplier = cooldown_multiplier;
        this.spacing = spacing;
        this.trail_lifetime = trail_lifetime;
    }

    public override string GetName()
    {
        return inner.GetName() + " (Chaos Trail)";
    }

    public override int GetManaCost()
    {
        return Mathf.RoundToInt(inner.GetManaCost() * mana_multiplier);
    }

    public override float GetCooldown()
    {
        return inner.GetCooldown() * cooldown_multiplier;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        last_cast = Time.time;

        yield return inner.Cast(where, target, team);

        Vector3 direction = (target - where).normalized;

        int damage = inner.GetDamage();

        float elapsed = 0f;

        float trailSpeed = 8f;

        while (elapsed < trail_lifetime)
        {
            Vector3 spawnPosition =
                where + direction * trailSpeed * elapsed;

            GameManager.Instance.projectileManager.CreateProjectile(0, "spiraling", spawnPosition, direction, 5f,
                (other, impact) =>
                {
                    if (other.team != team)
                    {
                        other.Damage(
                            new Damage(
                                damage,
                                Damage.Type.ARCANE
                            )
                        );
                    }
                },
                trail_lifetime
            );

            elapsed += spacing;

            yield return new WaitForSeconds(spacing);
        }
    }
}