using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data;
using ServerDevcommands;
using UnityEngine;
using Selector = Service.Selector;
namespace WorldEditCommands;

// Shared parameters for selecting and filtering existing objects.
public class ObjectSelection
{
  public Vector3 From = Player.m_localPlayer ? Player.m_localPlayer.transform.position : Vector3.zero;
  public Vector3? Center;
  public string[] ConnectionIds = [];
  public string[] IncludedIds = [];
  public string[] ExcludedIds = [];
  public float Angle = 0f;
  public Range<float>? Radius;
  public Range<float>? Width;
  public Range<float>? Depth;
  public float Height = 0f;
  public float Chance = 1f;
  public string Match = "";
  public string Unmatch = "";
  public List<string[]> Filters = [];
  public bool Connect;
  public HashSet<string> Components = [];
  public Dictionary<string, string> DataParameters = [];

  public void ParseSelectionArg(string name)
  {
    if (name == "connect") Connect = true;
    if (name == "center") Center = From;
  }
  public void ParseSelectionArg(string name, string value)
  {
    var values = Parse.Split(value);
    if (name == "par")
    {
      var kvp = Parse.Kvp(value);
      if (kvp.Key == "") throw new InvalidOperationException($"Invalid data parameter {value}.");
      DataParameters[$"<{kvp.Key}>"] = kvp.Value;
    }
    if (name == "connect") ConnectionIds = values;
    if (name == "center" || name == "from") Center = Parse.VectorXZY(values);
    if (name == "id") IncludedIds = values;
    if (name == "ignore") ExcludedIds = values;
    if (name == "chance") Chance = Parse.Float(value, 1f);
    if (name == "type") AddComponents(values);
    if (name == "match") Match = value;
    if (name == "unmatch") Unmatch = value;
    if (name == "filter") Filters.Add(ParseFilter(value));
    if (name == "rect")
    {
      var size = Parse.ScaleRange(value);
      Width = new(size.Min.x, size.Max.x);
      Depth = new(size.Min.z, size.Max.z);
    }
    if (name == "radius" || name == "range" || name == "circle")
      Radius = Parse.FloatRange(value);
    if (name == "height")
      Height = Parse.Float(value, 0f);
    if (name == "angle")
      Angle = Parse.Float(value, 0f) * Mathf.PI / 180f;
  }
  private static string[] ParseFilter(string value)
  {
    var split = value.Split([','], 3);
    if (split.Length < 3) return split;
    var type = split[0].ToLowerInvariant();
    if (type == "int" || type == "float" || type == "long")
      split[2] = string.Join(",", split[2].Split(',').Select(v => IsRange(v) ? ToRange(type, v) : v));
    return split;
  }
  private static bool IsRange(string value) => value.Length > 1 && value.IndexOf('-', 1) > 0 && !value.Contains("<");
  private static string ToRange(string type, string value) => type switch
  {
    "int" => ToRange(Parse.IntRange(value)),
    "long" => ToRange(Parse.LongRange(value)),
    _ => ToRange(Parse.FloatRange(value)),
  };
  private static string ToRange<T>(Range<T> range) where T : IFormattable =>
    range.Min.ToString(null, CultureInfo.InvariantCulture) + ";" + range.Max.ToString(null, CultureInfo.InvariantCulture);
  public void ValidateSelection()
  {
    if (Radius != null && Depth != null)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>rect</color> parameters can't be used together.");
    if (Radius != null && Connect)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>connect</color> parameters can't be used together.");
    if (Depth != null && Connect)
      throw new InvalidOperationException($"<color=yellow>connect</color> and <color=yellow>rect</color> parameters can't be used together.");
  }
  private void AddComponents(string[] values)
  {
    foreach (var value in values)
    {
      var lower = value.ToLowerInvariant();
      if (lower == "structure")
        Components.Add("WearNTear");
      else if (lower == "creature")
        Components.Add("Humanoid");
      else
        Components.Add(value);
    }
  }

  public ZNetView? GetHovered() => Selector.GetHovered(50f, IncludedIds, Components, ExcludedIds);

  public ZNetView[] GetObjects(Terminal terminal)
  {
    ZNetView[] views;
    if (Connect)
    {
      var view = GetHovered();
      if (view == null) return [];
      views = Selector.GetConnected(view, ConnectionIds, IncludedIds, ExcludedIds);
    }
    else if (Radius != null)
    {
      views = Selector.GetNearby(IncludedIds, Components, ExcludedIds, Center ?? From, Radius, Height);
    }
    else if (Width != null && Depth != null)
    {
      views = Selector.GetNearby(IncludedIds, Components, ExcludedIds, Center ?? From, Angle, Width, Depth, Height);
    }
    else
    {
      var view = GetHovered();
      if (view == null) return [];
      if (!Selector.GetPrefabs(IncludedIds).Contains(view.GetZDO().GetPrefab()))
      {
        Helper.AddMessage(terminal, $"Skipped: {view.name} has invalid id.");
        return [];
      }
      views = [view];
    }
    DataEntry? matchData = Match == "" ? null : DataHelper.Get(Match);
    DataEntry? unmatchData = Unmatch == "" ? null : DataHelper.Get(Unmatch);
    DataEntry? filterData = null;
    foreach (var filter in Filters)
      (filterData ??= new()).Load(filter);
    return [.. views.Where(view =>
    {
      if (!view) return false;
      if (!view.GetZDO().IsValid())
      {
        terminal.AddString($"Skipped: {view.name} is not loaded.");
        return false;
      }
      var zdo = view.GetZDO();
      if (matchData != null && !matchData.Match(new ParFunctions(DataParameters, zdo), zdo))
      {
        terminal.AddString($"Skipped: {view.name} not matching filter.");
        return false;
      }
      if (filterData != null && !filterData.Match(new ParFunctions(DataParameters, zdo), zdo))
      {
        terminal.AddString($"Skipped: {view.name} not matching filter.");
        return false;
      }
      if (unmatchData != null && !unmatchData.MatchNone(new ParFunctions(DataParameters, zdo), zdo))
      {
        terminal.AddString($"Skipped: {view.name} matching filter.");
        return false;
      }
      if (!Roll(Chance))
      {
        terminal.AddString($"Skipped: {view.name} (chance).");
        return false;
      }
      return true;
    })];
  }

  private static readonly System.Random Random = new();
  private static bool Roll(float value)
  {
    if (value >= 1f) return true;
    return Random.NextDouble() < value;
  }
}
