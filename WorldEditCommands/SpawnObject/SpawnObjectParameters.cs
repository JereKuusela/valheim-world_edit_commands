using System;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
class SpawnObjectParameters : SharedObjectParameters {
  public Quaternion BaseRotation;
  public Range<Vector3> Rotation = new(Vector3.zero);
  public Range<Vector3> RelativePosition = new(Vector3.zero);
  public Vector3 From;
  public Vector3? To = null;
  public Range<int> Amount = new(1);
  public string Name = "";
  public Range<int> Variant = new(0);
  public bool Snap = true;
  public bool Tamed = false;
  public bool Hunt = false;
  public bool UseDefaultRelativePosition = true;

  public SpawnObjectParameters(Terminal.ConsoleEventArgs args) {
    if (Player.m_localPlayer) {
      From = Player.m_localPlayer.transform.position;
      BaseRotation = Player.m_localPlayer.transform.rotation;
    }
    ParseArgs(args.Args);
  }

  protected override void ParseArgs(string[] args) {
    base.ParseArgs(args);
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (name == "tame" || name == "tamed")
        Tamed = true;
      if (name == "hunt")
        Hunt = true;
      if (split.Length < 2) continue;
      var value = split[1];
      if (name == "name" || name == "crafter")
        Name = value.Replace("_", " ");
      if (name == "variant")
        Variant = Parse.IntRange(value, 0);
      if (name == "amount")
        Amount = Parse.IntRange(value);
      if (name == "refrot" || name == "refrotation") {
        BaseRotation = Parse.AngleYXZ(value, BaseRotation);
      }
      if (name == "pos" || name == "position") {
        UseDefaultRelativePosition = false;
        RelativePosition = Parse.VectorXZYRange(value, Vector3.zero);
        Snap = value.Split(',').Length < 3;
      }
      if (name == "rot" || name == "rotation") {
        Rotation = Parse.VectorYXZRange(value, Vector3.zero);
      }
      if (name == "from" || name == "refpos") {
        UseDefaultRelativePosition = false;
        From = Parse.VectorXZY(value.Split(','), From);
      }
      if (name == "to") {
        UseDefaultRelativePosition = false;
        To = Parse.VectorXZY(value.Split(','), From);
      }
      if (name == "refplayer") {
        UseDefaultRelativePosition = false;
        var player = Helper.FindPlayer(value);
        if (player.m_characterID.IsNone()) {
          throw new InvalidOperationException("Unable to find the player.");
        } else if (!player.m_publicPosition) {
          throw new InvalidOperationException("Player doesn't have a public position.");
        } else {
          From = player.m_position;
        }
      }
    }
    if (To.HasValue && Radius.HasValue)
      throw new InvalidOperationException("<color=yellow>radius</color> can't be used with <color=yellow>to</color>.");
  }
}
