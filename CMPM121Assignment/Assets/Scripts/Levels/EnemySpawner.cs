using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    //public GameEndController  gameEndPanel;
    //public WaveInfoController waveInfoPanel;

    public Dictionary<string, Enemy> EnemyTypes;
    public List<Level> difficulties;

    private Level currentLevel;
    private int currentWave = 1;

    void Start()
    {
        EnemyTypes   = new Dictionary<string, Enemy>();
        difficulties = new List<Level>();

        JToken enemyTokens = JToken.Parse(Resources.Load<TextAsset>("enemies").text);
        JToken levelTokens = JToken.Parse(Resources.Load<TextAsset>("levels").text);

        foreach (var e in enemyTokens)
        {
            Enemy en = e.ToObject<Enemy>();
            EnemyTypes[en.name] = en;
        }

        difficulties = levelTokens.ToObject<List<Level>>();

        for (int i = 0; i < difficulties.Count; i++)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, 130 - i * 120);
            selector.transform.localScale    = new Vector3(2, 2, 2);
            selector.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(difficulties[i].name);
        }
    }

    public void StartLevel(string levelname)
    {
        currentLevel = difficulties.FirstOrDefault(l => l.name == levelname);
        currentWave  = 1;
        if (currentLevel == null) { Debug.LogError("Level not found: " + levelname); return; }

        level_selector.gameObject.SetActive(false);
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void Reload()
    {
        GameManager.Instance.state = GameManager.GameState.PREGAME;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextWave()
    {
        currentWave++;
        if (currentLevel.waves > 0 && currentWave > currentLevel.waves)
        {
            GameManager.Instance.state = GameManager.GameState.GAMEFINISH;
            return;
        }
        StartCoroutine(SpawnWave());
    }

    public void OnPlayerDied()
    {
        StopAllCoroutines();
        //gameEndPanel?.ShowDefeat();
    }

    IEnumerator SpawnWave()
    {
        float startTime = Time.time;
        GameManager.Instance.ResetWaveStats();
        GameManager.Instance.waveNumber = currentWave;

        GameManager.Instance.state     = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;

        foreach (Spawn spawn in currentLevel.spawns)
        {
            if (!EnemyTypes.ContainsKey(spawn.enemy))
            {
                Debug.LogError("Unknown enemy type: " + spawn.enemy);
                continue;
            }

            Enemy enemyType = EnemyTypes[spawn.enemy];
            var variables = new Dictionary<string, int>
            {
                { "wave", currentWave },
                { "base", enemyType.hp }
            };

            int   count = RPNEvaluator.RPNEvaluator.Evaluate(spawn.count, variables);
            float delay = string.IsNullOrEmpty(spawn.delay) ? 1f
                          : RPNEvaluator.RPNEvaluator.Evaluate(spawn.delay, variables);

            int spawned = 0;

            if (spawn.sequence != null && spawn.sequence.Count > 0)
            {
                int seqIdx = 0;
                while (spawned < count)
                {
                    int amount = spawn.sequence[seqIdx];
                    for (int i = 0; i < amount && spawned < count; i++)
                    {
                        StartCoroutine(SpawnEnemy(CreateScaledEnemy(enemyType, spawn, variables)));
                        spawned++;
                    }
                    seqIdx = (seqIdx + 1) % spawn.sequence.Count;
                    if (spawned < count) yield return new WaitForSeconds(delay);
                }
            }
            else
            {
                while (spawned < count)
                {
                    StartCoroutine(SpawnEnemy(CreateScaledEnemy(enemyType, spawn, variables)));
                    spawned++;
                    if (spawned < count) yield return new WaitForSeconds(delay);
                }
            }
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.waveTime = Time.time - startTime;
        GameManager.Instance.state    = GameManager.GameState.WAVEEND;

        GameManager.Instance.player.GetComponent<PlayerController>().OnWaveEnd(currentWave);
    }

    private Enemy CopyEnemy(Enemy og) => new Enemy
        { name = og.name, sprite = og.sprite, hp = og.hp, speed = og.speed, damage = og.damage };

    private Enemy CreateScaledEnemy(Enemy enemyType, Spawn spawn, Dictionary<string, int> variables)
    {
        Enemy scaled = CopyEnemy(enemyType);
        if (!string.IsNullOrEmpty(spawn.hp))
            scaled.hp = RPNEvaluator.RPNEvaluator.Evaluate(spawn.hp, variables);
        if (!string.IsNullOrEmpty(spawn.damage))
        {
            var dmgVars = new Dictionary<string, int>
                { { "wave", currentWave }, { "base", enemyType.damage } };
            scaled.damage = RPNEvaluator.RPNEvaluator.Evaluate(spawn.damage, dmgVars);
        }
        return scaled;
    }

    IEnumerator SpawnEnemy(Enemy enType)
    {
        SpawnPoint sp = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 pos = sp.transform.position + new Vector3(offset.x, offset.y, 0);

        GameObject go = Instantiate(enemy, pos, Quaternion.identity);
        go.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enType.sprite);

        EnemyController ec = go.GetComponent<EnemyController>();
        ec.hp     = new Hittable(enType.hp, Hittable.Team.MONSTERS, go);
        ec.damage = enType.damage;
        ec.speed  = enType.speed;
        GameManager.Instance.AddEnemy(go);
        yield return new WaitForSeconds(0.5f);
    }
}