using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class Item {
  public string Name;
  public int Variant;
  public Item(string value) {
    var values = Parse.Split(value);
    Name = Parse.String(values, 0);
    Variant = Parse.Int(values, 1, 0);
  }
}
public class SharedObjectParameters {
  public Range<Vector3> Scale = new(Vector3.one);
  public Range<int> Level = new(1);
  public Range<float> Health = new(0f);
  public Item? Helmet = null;
  public Item? LeftHand = null;
  public Item? RightHand = null;
  public Item? Chest = null;
  public Item? Shoulders = null;
  public Item? Legs = null;
  public Item? Utility = null;
  public float? Radius = null;
  public Range<int> Model = new(0);

  protected virtual void ParseArgs(string[] args) {
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1];
      if (name == "health" || name == "durability")
        Health = Parse.FloatRange(value, 0);
      if (name == "stars" || name == "star") {
        Level = Parse.IntRange(value, 0);
        Level.Max++;
        Level.Min++;
      }
      if (name == "model")
        Model = Parse.IntRange(value, 0);
      if (name == "level" || name == "levels")
        Level = Parse.IntRange(value);
      if (name == "radius" || name == "range" || name == "circle")
        Radius = Parse.Float(value);
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
