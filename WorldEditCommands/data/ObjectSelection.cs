using System;
using System.Collections.Generic;
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

  public ZNetView[] GetObjects(Terminal terminal)
  {
    ZNetView[] views;
    if (Connect)
    {
      var view = Selector.GetHovered(50f, IncludedIds, Components, ExcludedIds);
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
      var view = Selector.GetHovered(50f, IncludedIds, Components, ExcludedIds);
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
