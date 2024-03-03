using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using ServerDevcommands;
namespace Data;

public class DataLoading
{
  private static readonly string GamePath = Path.Combine("BepInEx", "config", "data");
  private static readonly string ProfilePath = Path.Combine(Paths.ConfigPath, "config", "data");

  // Each file can have multiple data entries so we need to load them all.
  public static readonly Dictionary<int, DataEntry> Data = [];
  public static readonly Dictionary<int, List<string>> ValueGroups = [];
  public static readonly List<string> DataKeys = [];

  public static void Load(string data, Dictionary<string, string> pars, ZDO zdo) => Get(data).Write(pars, zdo);
  public static string Base64(Dictionary<string, string> pars, string data)
  {
    if (!Data.TryGetValue(data.GetStableHashCode(), out var zdo))
      return data;
    return zdo.GetBase64(pars);
  }

  public static DataEntry Get(string name)
  {
    var hash = name.GetStableHashCode();
    if (!Data.ContainsKey(hash))
    {
      try
      {
        Data[hash] = new DataEntry(name);
      }
      catch (Exception e)
      {
        if (name.Contains("=") || name.Length > 32)
          throw new InvalidOperationException($"Can't load data value: {name}", e);
        else
          throw new InvalidOperationException($"Can't find data entry: {name}", e);
      }
    }
    return Data[hash];
  }
  public static bool TryGetValueFromGroup(string group, out string value)
  {
    var hash = group.GetStableHashCode();
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
    Data.Clear();
    DataKeys.Clear();
    ValueGroups.Clear();
    var files = Directory.GetFiles(GamePath, "*.yaml").Concat(Directory.GetFiles(ProfilePath, "*.yml")).ToArray();
    foreach (var file in files)
    {
      var yaml = Yaml.LoadList<DataData>(file);
      foreach (var data in yaml)
      {
        if (data.value != null)
        {
          var kvp = Parse.Kvp(data.value);
          var hash = kvp.Key.GetStableHashCode();
          if (ValueGroups.ContainsKey(hash))
            ServerDevcommands.ServerDevcommands.Log.LogWarning($"Duplicate value group entry: {kvp.Key} at {file}");
          if (!ValueGroups.ContainsKey(hash))
            ValueGroups[hash] = [];
          ValueGroups[hash].Add(kvp.Value);
        }
        if (data.valueGroup != null && data.values != null)
        {
          var hash = data.valueGroup.GetStableHashCode();
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

  }
  public static void Save(PlainDataEntry data, string name, bool profile)
  {
    var path = Path.Combine(profile ? ProfilePath : GamePath, "data.yaml");
    if (!File.Exists(path))
    {
      Directory.CreateDirectory(Path.GetDirectoryName(path));
      File.Create(path).Close();
    }
    var yaml = File.ReadAllText(path);
    yaml += "\n" + Yaml.Serializer().Serialize(new[] { ToData(data, name) });
    File.WriteAllText(path, yaml);
  }

  public static DataData ToData(PlainDataEntry zdo, string name)
  {
    DataData data = new() { name = name };
    zdo.Write(data);
    return data;
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
