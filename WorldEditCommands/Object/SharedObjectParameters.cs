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
        Health = Parse.FloatRange(value, 0);
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
    }
  }
}
