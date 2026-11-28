using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Service;

namespace Data;

public partial class Blueprints
{
  public static Func<IEnumerable<string>> FolderProvider = () => [];

  static Blueprints() => RemoveCreatorData = () => false;

  private static partial IEnumerable<string> Folders() => [Path.Combine(Paths.ConfigPath, "blueprints"), .. FolderProvider()];

  private static partial DataEntry? ResolveData(string data, string fileName)
  {
    if (data == "") return null;
    try
    {
      return DataHelper.Get(data);
    }
    catch (Exception e)
    {
      Log.Warning($"{fileName}: {e.Message}");
      return null;
    }
  }
}
