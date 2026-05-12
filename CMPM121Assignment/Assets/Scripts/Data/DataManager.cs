using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
    public List<EnemyData> enemies;
    public List<LevelData> levels;

    public Dictionary<string, EnemyData> enemyDict;

    void Awake()
    {
        LoadEnemies();
        LoadLevels();
        BuildEnemyDictionary();

        Debug.Log($"Loaded {enemies.Count} enemies.");
        Debug.Log($"Loaded {levels.Count} levels.");
    }

    void LoadEnemies()
    {
        TextAsset file = Resources.Load<TextAsset>("enemies");
        enemies = JsonConvert.DeserializeObject<List<EnemyData>>(file.text);
    }

    void LoadLevels()
    {
        TextAsset file = Resources.Load<TextAsset>("levels");
        levels = JsonConvert.DeserializeObject<List<LevelData>>(file.text);
    }

    void BuildEnemyDictionary()
    {
        enemyDict = new Dictionary<string, EnemyData>();

        foreach (EnemyData enemy in enemies)
        {
            enemyDict[enemy.name] = enemy;
        }
    }
}
