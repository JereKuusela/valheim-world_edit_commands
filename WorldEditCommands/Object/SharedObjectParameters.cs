using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class Item
{
  public string Name;
  public int Variant;
  public Item(string value)
  {
    var values = Parse.Split(value);
    Name = Parse.String(values, 0);
    Variant = Parse.Int(values, 1, 0);
  }
}
public class SharedObjectParameters
{
  public Range<Vector3> Scale = new(Vector3.one);
  public Range<int>? Level;
  public Range<float>? Health;
  public bool isHealthPercentage = false;
  public Range<float>? Damage;
  public Range<int>? Ammo;
  public string? AmmoType;
  public bool? Baby;
  public Item? Helmet;
  public Item? LeftHand;
  public Item? RightHand;
  public Item? Chest;
  public Item? Shoulders;
  public Item? Legs;
  public Item? Utility;
  public Range<float>? Radius;
  public Range<int>? Model;
  public Dictionary<string, object> Fields = [];

  protected virtual void ParseArgs(string[] args)
  {
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (name == "baby") Baby = true;
      if (split.Length < 2) continue;
      var value = split[1];
      if (name == "health" || name == "durability")
      {
        if (value.EndsWith("%"))
        {
          isHealthPercentage = true;
          value = value.Substring(0, value.Length - 1);
        }
        Health = Parse.FloatRange(value, 0);
        if (isHealthPercentage)
        {
          Health.Max /= 100f;
          Health.Min /= 100f;
        }
      }
      if (name == "damage")
        Damage = Parse.FloatRange(value, 0);
      if (name == "ammo")
        Ammo = Parse.IntRange(value, 0);
      if (name == "ammotype")
        AmmoType = value;
      if (name == "stars" || name == "star")
      {
        Level = Parse.IntRange(value, 0);
        Level.Max++;
        Level.Min++;
      }
      if (name == "model")
        Model = Parse.IntRange(value, 0);
      if (name == "level" || name == "levels")
        Level = Parse.IntRange(value);
      if (name == "radius" || name == "range" || name == "circle")
        Radius = Parse.FloatRange(value);
      if (name == "sc" || name == "scale")
        Scale = Parse.ScaleRange(value);
      if (name == "helmet") Helmet = new(value);
      if (name == "left_hand") LeftHand = new(value);
      if (name == "right_hand") RightHand = new(value);
      if (name == "chest") Chest = new(value);
      if (name == "shoulders") Shoulders = new(value);
      if (name == "legs") Legs = new(value);
      if (name == "utility") Utility = new(value);
      if (name == "field" || name == "f")
      {
        var values = value.Split(',');
        if (values.Length < 3) continue;
        var prefab = DataAutoComplete.PrefabFromCommand(string.Join(" ", args));
        var component = DataAutoComplete.RealComponent(prefab, values[0]);
        var field = DataAutoComplete.RealField(component, values[1]);
        var fieldValue = string.Join(",", values.Skip(2));
        var type = DataAutoComplete.GetType(component, field);
        var key = $"{component}.{field}";
        if (type == typeof(int))
          Fields.Add(key, Parse.Int(fieldValue));
        else if (type == typeof(float))
          Fields.Add(key, Parse.Float(fieldValue));
        else if (type == typeof(string))
          Fields.Add(key, fieldValue);
        else if (type == typeof(bool))
          Fields.Add(key, bool.Parse(fieldValue) ? 1 : 0);
        else if (type == typeof(Vector3))
          Fields.Add(key, Parse.VectorXZY(values, 2));
        else if (type == typeof(GameObject) || type == typeof(ItemDrop) || type == typeof(EffectList))
          Fields.Add(key, fieldValue);
        else if (type == typeof(Character.Faction))
          Fields.Add(key, (int)ToEnum<Character.Faction>(fieldValue));
        else
          throw new Exception($"Unhandled type for field {key}");
      }
    }
  }

  public static T ToEnum<T>(string str) where T : struct, Enum => ToEnum<T>(ToList(str));
  public static T ToEnum<T>(List<string> list) where T : struct, Enum
  {
    int value = 0;
    foreach (var item in list)
    {
      if (Enum.TryParse<T>(item, true, out var parsed))
        value += (int)(object)parsed;
      else
        throw new Exception($"Failed to parse value {item} as {nameof(T)}.");
    }
    return (T)(object)value;
  }
  public static List<string> ToList(string str, bool removeEmpty = true) => Split(str, removeEmpty).ToList();

  public static string[] Split(string arg, bool removeEmpty = true, char split = ',') => arg.Split(split).Select(s => s.Trim()).Where(s => !removeEmpty || s != "").ToArray();
}
