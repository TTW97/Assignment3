using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RPNEvaluator;

public class BaseSpell : Spell
{
    // Raw values from JSON (RPN strings)
    private string damageExpr;
    private string manaExpr;
    private string cooldownExpr;
    private string speedExpr;
    private string trajectoryBase;
    private int spriteIndex;
    private string damageType;
    private int iconIndex;
    private string spellName;

    public BaseSpell(SpellCaster owner, string name, int icon,
                     string damage, string damageType, string mana,
                     string cooldown, string speed, string trajectory, int sprite)
        : base(owner)
    {
        this.spellName    = name;
        this.iconIndex    = icon;
        this.damageExpr   = damage;
        this.damageType   = damageType;
        this.manaExpr     = mana;
        this.cooldownExpr = cooldown;
        this.speedExpr    = speed;
        this.trajectoryBase = trajectory;
        this.spriteIndex  = sprite;
    }

    Dictionary<string, int> MakeVars()
    {
        return new Dictionary<string, int>
        {
            //{ "wave",  GameManager.Instance.waveNumber },
            //{ "power", GameManager.Instance.playerSpellPower }
        };
    }

    //public override string GetName()    => spellName;
    public override int    GetIcon()    => iconIndex;
    //public override int    GetDamage()  => Mathf.RoundToInt(RPNEvaluator.RPNEvaluator.Evaluatef(damageExpr,   MakeVars()));
    //public override int    GetManaCost()=> Mathf.RoundToInt(RPNEvaluator.RPNEvaluator.Evaluatef(manaExpr,     MakeVars()));
    //public override float  GetCooldown()=> RPNEvaluator.RPNEvaluator.Evaluatef(cooldownExpr, MakeVars());

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        var stats = new SpellStats();
        stats.trajectory = trajectoryBase;
        stats.sprite     = spriteIndex;
        return CastWithStats(where, target, team, stats);
    }

    public IEnumerator CastWithStats(Vector3 where, Vector3 target, Hittable.Team team, SpellStats stats)
    {
        this.team = team;
        last_cast = Time.time;

        var vars = MakeVars();
        float damage  = ValueModifier.Apply(stats.damageModifiers,  RPNEvaluator.RPNEvaluator.Evaluatef(damageExpr, vars));
        float speed   = ValueModifier.Apply(stats.speedModifiers,   RPNEvaluator.RPNEvaluator.Evaluatef(speedExpr,  vars));
        string traj   = string.IsNullOrEmpty(stats.trajectory) ? trajectoryBase : stats.trajectory;
        int spr       = stats.sprite;

        int finalDamage = Mathf.RoundToInt(damage);

        GameManager.Instance.projectileManager.CreateProjectile(spr, traj, where, target - where, speed,
            (other, impact) => {
                if (other.team != team)
                    other.Damage(new Damage(finalDamage, Damage.Type.ARCANE));
            });
        yield return new WaitForEndOfFrame();
    }
}