// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Data;

public partial class Blueprints
{
  // Hosts provide the folders to read and how a data string is resolved.
  private static partial IEnumerable<string> Folders();
  private static partial DataEntry? ResolveData(string data, string fileName);

  private static IEnumerable<string> LoadFiles(string folder, IEnumerable<string> bps)
  {
    if (Directory.Exists(folder))
    {
      var blueprints = Directory.EnumerateFiles(folder, "*.blueprint", SearchOption.AllDirectories);
      var vbuilds = Directory.EnumerateFiles(folder, "*.vbuild", SearchOption.AllDirectories);
      return bps.Concat(blueprints).Concat(vbuilds);
    }
    return bps;
  }
  public static IEnumerable<string> GetFolders() => Folders().Select(Path.GetFullPath).Distinct();

  public static IEnumerable<string> Files()
  {
    IEnumerable<string> bps = [];
    foreach (var folder in GetFolders())
      bps = LoadFiles(folder, bps);
    return bps.Distinct().OrderBy(s => s);
  }
  public static bool TryGetBluePrint(string name, out Blueprint blueprint)
  {
    blueprint = new("Invalid");
    var bp = GetBluePrint(name);
    if (bp == null) return false;
    blueprint = bp;
    return true;
  }
  public static Blueprint? GetBluePrint(string name, bool loadData = true)
  {
    name = name.Replace(" ", "_");
    var path = Files().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_") == name);
    if (path == null) return null;
    var rows = File.ReadAllLines(path);
    var extension = Path.GetExtension(path);
    Blueprint bp = new(name);
    if (extension == ".vbuild") return GetBuildShare(bp, rows, loadData);
    if (extension == ".blueprint") return GetPlanBuild(bp, rows, loadData);
    throw new InvalidOperationException("Unknown file format.");
  }
}
