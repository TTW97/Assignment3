using System;
using System.Collections.Generic;

[Serializable]
public class SpawnData
{
    public string enemy;

    public string count; // RPN
    public List<int> sequence;

    // string if we want delay in RPN form
    public int delay;

    public string location;

    public string hp; // base
}
