// Shared code: keep identical in EWD and EWP (common/). Sync changes to both.
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Service;

namespace Data;

/// <summary>
/// Watches blueprint files and drops changed blueprints from the cache.
/// Events are debounced because editors write files in several steps.
/// </summary>
public static partial class BlueprintWatcher
{
  private const double QuietMilliseconds = 500;
  private static readonly List<FileSystemWatcher> Watchers = [];
  private static readonly HashSet<string> Pending = [];
  private static System.Timers.Timer? Debounce;

  // Called after the blueprint was removed from the cache. listChanged is true when a file was added or removed.
  static partial void OnChanged(string name, bool wasLoaded, bool listChanged);

  public static void Start()
  {
    Stop();
    Debounce = new(QuietMilliseconds) { AutoReset = false, SynchronizingObject = ThreadingHelper.SynchronizingObject };
    Debounce.Elapsed += (_, _) => Flush();
    foreach (var folder in Blueprints.GetFolders())
    {
      Directory.CreateDirectory(folder);
      Watch(folder, "*.blueprint");
      Watch(folder, "*.vbuild");
    }
  }

  public static void Stop()
  {
    foreach (var watcher in Watchers)
      watcher.Dispose();
    Watchers.Clear();
    Debounce?.Dispose();
    Debounce = null;
    Pending.Clear();
  }

  private static void Watch(string folder, string pattern)
  {
    var watcher = new FileSystemWatcher(folder, pattern)
    {
      IncludeSubdirectories = true,
      SynchronizingObject = ThreadingHelper.SynchronizingObject
    };
    watcher.Created += (_, e) => Queue(e.FullPath);
    watcher.Changed += (_, e) => Queue(e.FullPath);
    watcher.Deleted += (_, e) => Queue(e.FullPath);
    watcher.Renamed += (_, e) =>
    {
      Queue(e.OldFullPath);
      Queue(e.FullPath);
    };
    watcher.EnableRaisingEvents = true;
    Watchers.Add(watcher);
  }

  private static void Queue(string path)
  {
    Pending.Add(Path.GetFileNameWithoutExtension(path));
    Debounce?.Stop();
    Debounce?.Start();
  }

  private static void Flush()
  {
    var names = new List<string>(Pending);
    Pending.Clear();
    foreach (var name in names)
    {
      try
      {
        var existed = BlueprintCache.HasName(name);
        var wasLoaded = BlueprintCache.IsLoaded(name);
        BlueprintCache.Remove(name);
        OnChanged(name, wasLoaded, existed != BlueprintCache.HasName(name));
      }
      catch (Exception e)
      {
        Log.Error($"Failed to reload blueprint {name}: {e.Message}");
      }
    }
  }
}
