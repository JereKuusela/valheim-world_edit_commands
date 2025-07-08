using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using ServerDevcommands;
namespace Data;

public class DataLoading
{
  private static readonly string GamePath = Path.GetFullPath(Path.Combine("BepInEx", "config", "data"));
  private static readonly string ProfilePath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, "data"));

  // Each file can have multiple data entries so we need to load them all.
  public static readonly Dictionary<int, DataEntry> Data = [];
  public static readonly Dictionary<int, List<string>> ValueGroups = [];
  public static readonly List<string> DataKeys = [];


  public static bool TryGetValueFromGroup(string group, out string value)
  {
    var hash = group.ToLowerInvariant().GetStableHashCode();
    if (!ValueGroups.ContainsKey(hash))
    {
      value = group;
      return false;
    }
    var roll = UnityEngine.Random.Range(0, ValueGroups[hash].Count);
    value = ValueGroups[hash][roll];
    return true;
  }
  public static void LoadEntries()
  {
    if (!ZNet.instance)
      return;
    Data.Clear();
    DataKeys.Clear();
    ValueGroups.Clear();
    var files = Directory.GetFiles(GamePath, "*.yaml").Concat(Directory.GetFiles(ProfilePath, "*.yaml")).
      Select(Path.GetFullPath).Distinct().ToArray();
    foreach (var file in files)
    {
      var yaml = Yaml.LoadList<DataData>(file);
      foreach (var data in yaml)
      {
        if (data.value != null)
        {
          var kvp = Parse.Kvp(data.value);
          var hash = kvp.Key.ToLowerInvariant().GetStableHashCode();
          if (ValueGroups.ContainsKey(hash))
            ServerDevcommands.ServerDevcommands.Log.LogWarning($"Duplicate value group entry: {kvp.Key} at {file}");
          if (!ValueGroups.ContainsKey(hash))
            ValueGroups[hash] = [];
          ValueGroups[hash].Add(kvp.Value);
        }
        if (data.valueGroup != null && data.values != null)
        {
          var hash = data.valueGroup.ToLowerInvariant().GetStableHashCode();
          if (ValueGroups.ContainsKey(hash))
            ServerDevcommands.ServerDevcommands.Log.LogWarning($"Duplicate value group entry: {data.valueGroup} at {file}");
          if (!ValueGroups.ContainsKey(hash))
            ValueGroups[hash] = [];
          foreach (var value in data.values)
            ValueGroups[hash].Add(value);
        }
        if (data.name != null)
        {
          var hash = data.name.GetStableHashCode();
          if (Data.ContainsKey(hash))
            ServerDevcommands.ServerDevcommands.Log.LogWarning($"Duplicate data entry: {data.name} at {file}");
          DataKeys.Add(data.name);
          Data[hash] = new DataEntry(data);
        }
      }
    }
    ServerDevcommands.ServerDevcommands.Log.LogInfo($"Loaded {Data.Count} data entries.");
    if (ValueGroups.Count > 0)
      ServerDevcommands.ServerDevcommands.Log.LogInfo($"Loaded {ValueGroups.Count} value groups.");
    LoadDefaultValueGroups();

  }
  public static void Save(PlainDataEntry data, string name, bool profile, bool dump)
  {
    var path = Path.Combine(profile ? ProfilePath : GamePath, "data.yaml");
    if (!File.Exists(path))
    {
      Directory.CreateDirectory(Path.GetDirectoryName(path));
      File.Create(path).Close();
    }
    var yaml = File.ReadAllText(path);
    yaml += "\n" + Yaml.Serializer().Serialize(new[] { ToData(data, name, dump) });
    File.WriteAllText(path, yaml);
  }

  public static DataData ToData(PlainDataEntry zdo, string name, bool dump)
  {
    DataData data = new() { name = name };
    zdo.Write(data, dump);
    return data;
  }

  private static readonly Dictionary<int, List<string>> DefaultValueGroups = [];
  private static readonly int WearNTearHash = "wearntear".GetStableHashCode();
  private static readonly int HumanoidHash = "humanoid".GetStableHashCode();
  private static readonly int CreatureHash = "creature".GetStableHashCode();
  private static readonly int StructureHash = "structure".GetStableHashCode();
  private static void LoadDefaultValueGroups()
  {
    if (DefaultValueGroups.Count == 0)
    {
      foreach (var type in ComponentInfo.Types)
      {
        var hash = type.Name.ToLowerInvariant().GetStableHashCode();
        DefaultValueGroups[hash] = [.. ComponentInfo.PrefabsByComponent(type.Name)];
      }
      // Some key codes are hardcoded for legacy reasons.
      if (DefaultValueGroups.ContainsKey(HumanoidHash))
        DefaultValueGroups[CreatureHash] = DefaultValueGroups[HumanoidHash];
      if (DefaultValueGroups.ContainsKey(WearNTearHash))
        DefaultValueGroups[StructureHash] = DefaultValueGroups[WearNTearHash];
    }
    foreach (var kvp in DefaultValueGroups)
    {
      if (!ValueGroups.ContainsKey(kvp.Key))
        ValueGroups[kvp.Key] = kvp.Value;
    }
  }

  public static void SetupWatcher()
  {
    if (!Directory.Exists(GamePath))
      Directory.CreateDirectory(GamePath);
    if (!Directory.Exists(ProfilePath))
      Directory.CreateDirectory(ProfilePath);
    Yaml.SetupWatcher(GamePath, "*", LoadEntries);
    if (GamePath != ProfilePath)
      Yaml.SetupWatcher(ProfilePath, "*", LoadEntries);
  }
}
