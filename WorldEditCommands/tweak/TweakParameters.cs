using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;
public class TweakParameters
{
  public Vector3 From;
  public Vector3? Center;
  public string[] IncludedIds = new string[0];
  public string[] ExcludedIds = new string[0];
  public float Angle = 0f;
  public long Creator = 0;
  public Range<float>? Radius;
  public Range<float>? Width;
  public Range<float>? Depth;
  public float Height = 0f;
  public float Chance = 1f;
  public bool Force;
  public bool Connect;
  public ObjectType ObjectType = ObjectType.All;

  public Dictionary<string, Type> SupportedOperations = new();

  public TweakParameters(Dictionary<string, Type> supportedOperations, Terminal.ConsoleEventArgs args)
  {
    SupportedOperations = supportedOperations;
    if (Player.m_localPlayer)
    {
      From = Player.m_localPlayer.transform.position;
    }
    ParseArgs(args.Args);
  }

  public Dictionary<string, object?> Operations = new();

  protected virtual void ParseArgs(string[] args)
  {
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      var name = split[0].ToLower();
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
      if (name == "force") Force = true;
      if (split.Length < 2) continue;
      var value = string.Join("=", split.Skip(1));
      if (SupportedOperations.TryGetValue(name, out type))
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
      if (name == "type" && value == "creature") ObjectType = ObjectType.Character;
      if (name == "type" && value == "chest") ObjectType = ObjectType.Chest;
      if (name == "type" && value == "fireplace") ObjectType = ObjectType.Fireplace;
      if (name == "type" && value == "item") ObjectType = ObjectType.Item;
      if (name == "type" && value == "spawner") ObjectType = ObjectType.Spawner;
      if (name == "type" && value == "spawnpoint") ObjectType = ObjectType.SpawnPoint;
      if (name == "type" && value == "structure") ObjectType = ObjectType.Structure;
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
      if (name == "creator")
        Creator = Parse.Long(value, 0L);
      if (name == "angle")
        Angle = Parse.Float(value, 0f) * Mathf.PI / 180f;
    }
    if (Operations.Count == 0 && !Force)
      throw new InvalidOperationException("Missing the operation.");
    if (Radius != null && Depth != null)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>rect</color> parameters can't be used together.");
    if (Radius != null && Connect)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>connect</color> parameters can't be used together.");
    if (Depth != null && Connect)
      throw new InvalidOperationException($"<color=yellow>connect</color> and <color=yellow>rect</color> parameters can't be used together.");
  }
}
