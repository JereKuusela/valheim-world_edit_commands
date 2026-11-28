// Shared code: keep identical in EWD and EWP (common/). Sync changes to both.
using System.Collections.Generic;
using System.IO;

namespace Data;

/// <summary>
/// Loads blueprints on demand and keeps them until removed. Failed loads are cached too, so errors are not repeated.
/// </summary>
public static partial class BlueprintCache
{
  private static readonly Dictionary<string, Blueprint?> Loaded = [];
  private static Dictionary<int, string>? Names;

  // Called before centering, so hosts can still modify the raw blueprint.
  static partial void OnParsed(Blueprint blueprint);

  private static string Normalize(string name) => name.Replace(" ", "_");

  public static void Clear()
  {
    Loaded.Clear();
    Names = null;
  }

  public static void Remove(string name)
  {
    Loaded.Remove(Normalize(name));
    Names = null;
  }

  public static bool IsLoaded(string name) => Loaded.TryGetValue(Normalize(name), out var blueprint) && blueprint != null;

  public static bool HasName(string name) => GetHashes().ContainsKey(Normalize(name).GetStableHashCode());

  public static IEnumerable<string> GetNames() => GetHashes().Values;

  public static bool TryGet(int hash, out Blueprint blueprint)
  {
    blueprint = null!;
    return GetHashes().TryGetValue(hash, out var name) && TryGet(name, out blueprint);
  }

  public static bool TryGet(string name, out Blueprint blueprint)
  {
    name = Normalize(name);
    if (!Loaded.TryGetValue(name, out var cached))
    {
      cached = Blueprints.GetBluePrint(name);
      if (cached != null)
      {
        OnParsed(cached);
        cached.Center();
      }
      Loaded[name] = cached;
    }
    blueprint = cached!;
    return cached != null;
  }

  private static Dictionary<int, string> GetHashes()
  {
    if (Names != null) return Names;
    Names = [];
    foreach (var path in Blueprints.Files())
    {
      var name = Normalize(Path.GetFileNameWithoutExtension(path));
      Names[name.GetStableHashCode()] = name;
    }
    return Names;
  }
}
