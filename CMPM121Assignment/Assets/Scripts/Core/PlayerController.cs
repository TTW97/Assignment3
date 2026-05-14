using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using RPNEvaluator;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    //public SpellUI spellui;
    public SpellUIContainer spellui;

    public int speed;
    public Unit unit;

    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
        StartCoroutine(spellcaster.ManaRegeneration());

        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);
        //spellui.SetSpell(spellcaster.spells);
        spellui.Refresh();
    }

    public void OnWaveEnd(int wave)
    {
        var vars = new Dictionary<string, int> { { "wave", wave } };

        int newMaxHp   = RPNEvaluator.RPNEvaluator.Evaluate("95 wave 5 * +",  vars);
        int newMana    = RPNEvaluator.RPNEvaluator.Evaluate("90 wave 10 * +", vars);
        int newManaReg = RPNEvaluator.RPNEvaluator.Evaluate("10 wave +",      vars);
        int newPower   = RPNEvaluator.RPNEvaluator.Evaluate("wave 10 *",      vars);

        hp.SetMaxHP(newMaxHp);
        spellcaster.max_mana = newMana;
        spellcaster.mana_reg = newManaReg;
        GameManager.Instance.playerSpellPower = newPower;
        speed = 5;
    }

    void Update() {
        if (spellcaster == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSpell(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSpell(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSpell(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSpell(3);
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME ||
            GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;

        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld  = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME ||
            GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>() * speed;
    }

    void Die()
    {
        Debug.Log("You Lost");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        FindFirstObjectByType<EnemySpawner>()?.OnPlayerDied();
    }

    void SelectSpell(int index)
    {
        if (index < 0 || index >= spellcaster.spells.Count) return;

        spellcaster.selectedSpellIndex = index;
        spellui.Refresh();
    }
}