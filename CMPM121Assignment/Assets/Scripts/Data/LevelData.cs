using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public string name;
    public int? waves; // Endless does not have waves
    public List<SpawnData> spawns;
}
