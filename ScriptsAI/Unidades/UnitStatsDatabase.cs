using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatsEntry
{
    public string type;
    public float maxSpeed;
    public int maxHealth;
    public int attack;
    public bool magicAttack;
    public int physicalDefense;
    public int magicDefense;
    public float dodge;
    public float range;
    public float attackCooldown;
}

[Serializable]
public class StatsCollection
{
    public StatsEntry[] units;
}

public class UnitStatsDatabase
{
    private static UnitStatsDatabase _instance;
    private readonly Dictionary<string, StatsEntry> _statsEntries;

    private UnitStatsDatabase()
    {
        _statsEntries = new Dictionary<string, StatsEntry>(StringComparer.OrdinalIgnoreCase);
        TextAsset text = Resources.Load<TextAsset>("unit_stats");
        if (text == null)
        {
            Debug.LogError("UnitStatsDatabase: Could not find unit_stats.json in Resources folder.");
            return;
        }

        StatsCollection statsCollection = JsonUtility.FromJson<StatsCollection>(text.text);
        if (statsCollection == null || statsCollection.units == null)
        {
            Debug.LogError("UnitStatsDatabase: Failed to parse unit_stats.json or no units found.");
            return;
        }

        _statsEntries = new Dictionary<string, StatsEntry>();
        foreach (var entry in statsCollection.units)
        {
            if (string.IsNullOrEmpty(entry.type))
            {
                Debug.LogWarning("UnitStatsDatabase: Found StatsEntry with null or empty id, skipping.");
                continue;
            }
            _statsEntries[entry.type] = entry;
        }
    }

    public static UnitStatsDatabase getInstance => _instance ??= new UnitStatsDatabase();

    public StatsEntry GetStats(string id)
    {
        if (_statsEntries.TryGetValue(id, out var entry))
        {
            return entry;
        }
        Debug.LogWarning($"UnitStatsDatabase: No StatsEntry found for id '{id}'.");
        return null;
    }

}
