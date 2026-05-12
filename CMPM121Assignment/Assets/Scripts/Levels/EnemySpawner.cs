using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    // dictionary that contains all enemy types
    // (key: enemy name, val: Enemy Object)
    public Dictionary<string, Enemy> EnemyTypes;
    // list of Level Objects
    public List<Level> difficulties;

    private Level currentLevel;
    private int currentWave = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemyTypes = new Dictionary<string, Enemy>();
        // pre-setup that loads and parses JSON tokens to be used for spawning
        var enemyText = Resources.Load<TextAsset>("enemies");
        JToken enemyTokens = JToken.Parse(enemyText.text);
        
        var levelText = Resources.Load<TextAsset>("levels");
        JToken levelTokens = JToken.Parse(levelText.text);
        
        // fill dictionary with all enemy types
        foreach (var enemy in enemyTokens)
        {
            Enemy en = enemy.ToObject<Enemy>();
            EnemyTypes[en.name] = en;
        }
        // contains all levels/difficulties
        difficulties = levelTokens.ToObject<List<Level>>();

        // dynamically create buttons for each level
        for (int i = 0; i < difficulties.Count; i++)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, 130 - i * 120);
            selector.transform.localScale = new Vector3(2, 2, 2);

            RectTransform rectTransform = selector.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(difficulties[i].name);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        // a Level object that contains all enemies and waves for a specific level/difficulty
        currentLevel = difficulties.FirstOrDefault(l => l.name == levelname);
        currentWave = 1;
        if (currentLevel == null)
        {
            Debug.LogError("Level not found");
            return;
        }
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.Instance.state = GameManager.GameState.PREGAME;

    }

    public void NextWave()
    {
        currentWave++;
        if (currentLevel.waves > 0 && currentWave > currentLevel.waves)
        {
            Debug.Log("Level complete!");
            GameManager.Instance.state = GameManager.GameState.GAMEFINISH;
            Debug.Log("GameState: " + GameManager.GameState.GAMEFINISH);
            return;
        }
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        float startTime = Time.time;
        GameManager.Instance.ResetWaveStats();
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        // countdown timer
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
            // Enemy type to be spawned in this instance of the wave
            Enemy enemyType = EnemyTypes[spawn.enemy];
            // wave information variables dictionary
            Dictionary<string, int> variables = new Dictionary<string, int>()
            {
                { "wave", currentWave },
                { "base", enemyType.hp }
            };
            
            // number of enemies to be spawned in a wave
            int count = RPNEvaluator.RPNEvaluator.Evaluate(spawn.count, variables);
            // delay value between spawns, in seconds
            float delay = 1;
            if (!string.IsNullOrEmpty(spawn.delay))
            {
                delay = RPNEvaluator.RPNEvaluator.Evaluate(spawn.delay, variables);
            }

            // counter of how many enemies actually spawned
            int spawned = 0;
            // if a sequence value exists & sequence list is not empty
            if (spawn.sequence != null && spawn.sequence.Count > 0)
            {
                // index of sequence list to iterate through for spawning
                int sequenceIndex = 0;
                // while enemies spawned less than max count
                while (spawned < count)
                {
                    // number of enemies to spawn in one instance
                    int amountToSpawn = spawn.sequence[sequenceIndex];
                    // loop to spawn said number of enemies.
                    for (int i = 0; i < amountToSpawn; ++i)
                    {
                        Enemy scaledEnemy = CreateScaledEnemy(enemyType, spawn, variables);
                        StartCoroutine(SpawnEnemy(scaledEnemy));
                        spawned++;
                    }

                    // increment sequence index, cycling back to 0 if necessary
                    sequenceIndex = (sequenceIndex + 1) % spawn.sequence.Count;
                    // if threshold not reached, induce delay
                    if (spawned < count)
                    {
                        yield return new WaitForSeconds(delay);
                    }
                }
            }
            // fallback if no sequence value exists: spawn 1 enemy at a time
            // similar logic to above
            else
            {
                while (spawned < count)
                {
                    Enemy scaledEnemy = CreateScaledEnemy(enemyType, spawn, variables);
                    StartCoroutine(SpawnEnemy(scaledEnemy));
                    spawned++;
                    if (spawned < count)
                    {
                        yield return new WaitForSeconds(delay);
                    }
                }
            }
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.waveTime = Time.time - startTime;
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    // helper function to copy enemyType properties to an Enemy Object
    private Enemy CopyEnemy(Enemy og)
    {
        return new Enemy
        {
            // copy all fields
            name = og.name,
            sprite = og.sprite,
            hp = og.hp,
            speed = og.speed,
            damage = og.damage
        };
    }
    
    // helper function to help repeatedly create scaled enemies
    private Enemy CreateScaledEnemy(Enemy enemyType, Spawn spawn, Dictionary<string, int> variables)
    {
        Enemy scaledEnemy = CopyEnemy(enemyType);
        if (!string.IsNullOrEmpty(spawn.hp))
        {
            scaledEnemy.hp = RPNEvaluator.RPNEvaluator.Evaluate(spawn.hp, variables);
        }

        if (!string.IsNullOrEmpty(spawn.damage))
        {
            Dictionary<string, int> damageVariables = new Dictionary<string, int>
            {
                { "wave", currentWave },
                { "base", enemyType.damage }
            };
            scaledEnemy.damage = RPNEvaluator.RPNEvaluator.Evaluate(spawn.damage, damageVariables);
        }

        return scaledEnemy;
    }
    
    IEnumerator SpawnEnemy(Enemy enType)
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enType.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(enType.hp, Hittable.Team.MONSTERS, new_enemy);
        en.damage = enType.damage;
        en.speed = enType.speed;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}
