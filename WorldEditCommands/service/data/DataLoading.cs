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
  public static readonly Dictionary<int, ZDOData> Data = [];
  public static readonly List<string> DataKeys = [];

  public static void Load(string data, ZDO zdo) => Get(data).Write(zdo);
  public static string Base64(string data)
  {
    if (!Data.TryGetValue(data.GetStableHashCode(), out var zdo))
      return data;
    return zdo.GetBase64();
  }

  public static ZDOData Get(string name)
  {
    var hash = name.GetStableHashCode();
    if (!Data.ContainsKey(hash))
    {
      try
      {
        Data[hash] = new ZDOData(name);
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

  public static void LoadEntries()
  {
    Data.Clear();
    DataKeys.Clear();
    var files = Directory.GetFiles(GamePath, "*.yaml").Concat(Directory.GetFiles(ProfilePath, "*.yml")).ToArray();
    foreach (var file in files)
    {
      var yaml = Yaml.LoadList<DataData>(file);
      foreach (var data in yaml)
      {
        if (data.name == null) continue;
        var hash = data.name.GetStableHashCode();
        if (Data.ContainsKey(hash))
          ServerDevcommands.ServerDevcommands.Log.LogWarning($"Duplicate data entry: {data.name} at {file}");
        DataKeys.Add(data.name);
        Data[hash] = new ZDOData(data);
      }
    }
    ServerDevcommands.ServerDevcommands.Log.LogInfo($"Loaded {Data.Count} data entries.");
  }
  public static void Save(ZDOData data, string name, bool profile)
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

  public static DataData ToData(ZDOData zdo, string name)
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
