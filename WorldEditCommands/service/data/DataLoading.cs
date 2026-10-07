using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerDevcommands;
using Service;

namespace Data;

public class DataLoading
{
  private static readonly string GamePath = Path.GetFullPath(Path.Combine("BepInEx", "config", "data"));
  private static readonly string ProfilePath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, "data"));

  // Each file can have multiple data entries so we need to load them all.
  public static readonly Dictionary<int, DataEntry> Data = [];
  public static readonly List<string> DataKeys = [];

  public static void LoadEntries()
  {
    if (!ZNet.instance)
      return;
    Data.Clear();
    DataKeys.Clear();
    ValueGroups.Clear();
    List<(string File, DataYaml Data)> loaded = [];
    void Collect(string file, DataYaml data) => loaded.Add((file, data));
    Yaml.LoadListsFromDirectory<DataYaml>(GamePath, "*.yaml", Collect);
    if (ProfilePath != GamePath)
      Yaml.LoadListsFromDirectory<DataYaml>(ProfilePath, "*.yaml", Collect);

    foreach (var entry in loaded)
      ValueGroups.Add(entry.Data, entry.File);
    if (ValueGroups.Count > 0)
      Log.Info($"Loaded {ValueGroups.Count} value groups.");
    // Entries need fully resolved value groups, so two passes are needed.
    ValueGroups.Resolve();
    foreach (var entry in loaded)
      LoadEntry(entry.File, entry.Data);
    PrefabHelper.ClearCache();
    Log.Info($"Loaded {Data.Count} data entries.");
  }

  private static void LoadEntry(string file, DataYaml data)
  {
    if (data.name == null) return;
    var hash = data.name.GetStableHashCode();
    if (Data.ContainsKey(hash))
      Log.Warning($"Duplicate data entry: {data.name} at {file}");
    DataKeys.Add(data.name);
    Data[hash] = new DataEntry(data);
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

  public static DataYaml ToData(PlainDataEntry zdo, string name, bool dump)
  {
    DataYaml data = new() { name = name };
    zdo.Write(data, dump);
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
