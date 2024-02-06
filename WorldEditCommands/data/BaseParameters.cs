using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;
public abstract class BaseParameters
{
  public Vector3 From;
  public Vector3? Center;
  public string[] IncludedIds = [];
  public string[] ExcludedIds = [];
  public float Angle = 0f;
  public Range<float>? Radius;
  public Range<float>? Width;
  public Range<float>? Depth;
  public float Height = 0f;
  public float Chance = 1f;
  public bool Connect;
  public HashSet<string> Components = [];

  private readonly Dictionary<string, Type> SupportedOperations;
  private readonly Terminal terminal;

  public BaseParameters(Dictionary<string, Type> supportedOperations, Terminal.ConsoleEventArgs args)
  {
    SupportedOperations = supportedOperations;
    if (Player.m_localPlayer)
      From = Player.m_localPlayer.transform.position;
    terminal = args.Context;
    ParseArgs(args.Args);
  }

  public Dictionary<string, object?> Operations = [];

  abstract protected void ParseArg(string arg);
  abstract protected void ParseArg(string arg, string value);
  protected virtual void ParseArgs(string[] args)
  {
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      ParseArgInternal(name);
      ParseArg(name);
      if (split.Length < 2) continue;
      var value = string.Join("=", split.Skip(1));
      ParseArgInternal(name, value);
      ParseArg(name, value);
    }
    if (Operations.Count == 0)
      throw new InvalidOperationException("Missing the operation.");
    if (Radius != null && Depth != null)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>rect</color> parameters can't be used together.");
    if (Radius != null && Connect)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>connect</color> parameters can't be used together.");
    if (Depth != null && Connect)
      throw new InvalidOperationException($"<color=yellow>connect</color> and <color=yellow>rect</color> parameters can't be used together.");
  }
  private void ParseArgInternal(string name)
  {
    if (SupportedOperations.TryGetValue(name, out var type))
    {
      if (Operations.ContainsKey(name) && type != typeof(string[]))
        throw new InvalidOperationException($"Operation {name} used multiple times.");
      if (type == typeof(string[]))
      {
        if (!Operations.ContainsKey(name))
          Operations.Add(name, new string[0]);
      }
      else
        Operations.Add(name, null);
    }
    if (name == "connect") Connect = true;
  }
  private void ParseArgInternal(string name, string value)
  {
    if (SupportedOperations.TryGetValue(name, out var type))
    {
      if (type == typeof(int))
        Operations[name] = Parse.Int(value);
      else if (type == typeof(float))
        Operations[name] = Parse.Float(value);
      else if (type == typeof(bool))
        Operations[name] = Parse.Boolean(value);
      else if (type == typeof(string[]))
        Operations[name] = (Operations[name] as string[]).Append(value).ToArray();
      else
        Operations[name] = value;
    }
    var values = Parse.Split(value);
    if (name == "center" || name == "from") Center = Parse.VectorXZY(values);
    if (name == "id") IncludedIds = values;
    if (name == "ignore") ExcludedIds = values;
    if (name == "chance") Chance = Parse.Float(value, 1f);
    if (name == "type") AddComponents(values);
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
  private void AddComponents(string[] values)
  {
    foreach (var value in values)
    {
      var lower = value.ToLowerInvariant();
      if (lower == "strucutre")
        Components.Add("WearNTear");
      else if (lower == "creature")
        Components.Add("Humanoid");
      else
        Components.Add(value);
    }
  }

  public ZNetView[] GetObjects()
  {
    ZNetView[] views;
    if (Connect)
    {
      var view = Selector.GetHovered(50f, IncludedIds, Components, ExcludedIds);
      if (view == null) return [];
      views = Selector.GetConnected(view, IncludedIds, ExcludedIds);
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
    return views.Where(view =>
    {
      if (!view || !view.GetZDO().IsValid())
      {
        terminal.AddString($"Skipped: {view.name} is not loaded.");
        return false;
      }
      if (!Roll(Chance))
      {
        terminal.AddString($"Skipped: {view.name} (chance).");
        return false;
      }
      return true;
    }).ToArray();
  }

  public static System.Random Random = new();
  public static bool Roll(float value)
  {
    if (value >= 1f) return true;
    return Random.NextDouble() < value;
  }
}
