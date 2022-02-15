using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands {
  class SpawnObjectParameters : SharedObjectParameters {
    public Quaternion BaseRotation = Quaternion.identity;
    public Range<Vector3> Rotation = new Range<Vector3>(Vector3.zero);
    public Vector3 RelativePosition = Vector3.zero;
    public Vector3 BasePosition = Vector3.zero;
    public Range<int> Amount = new Range<int>(1);
    public string Name = "";
    public Range<int> Variant = new Range<int>(0);
    public bool Snap = true;
    public bool Tamed = false;
    public bool Hunt = false;
    public bool UseDefaultRelativePosition = true;

    public override bool ParseArgs(string[] args, Terminal terminal) {
      if (!base.ParseArgs(args, terminal)) return false;
      foreach (var arg in args) {
        var split = arg.Split('=');
        var name = split[0];
        if (name == "tame" || name == "tamed")
          Tamed = true;
        if (name == "hunt")
          Hunt = true;
        if (split.Length < 2) continue;
        var value = split[1];
        if (split[0] == "name" || split[0] == "crafter")
          Name = value;
        if (split[0] == "variant")
          Variant = Parse.TryIntRange(value, 0);
        if (split[0] == "amount")
          Amount = Parse.TryIntRange(value);
        if (split[0] == "refRot" || split[0] == "refRotation") {
          BaseRotation = Parse.TryAngleYXZ(value, BaseRotation);
        }
        if (split[0] == "pos" || split[0] == "position") {
          UseDefaultRelativePosition = false;
          RelativePosition = Parse.TryVectorXZY(value.Split(','));
          Snap = value.Split(',').Length < 3;
        }
        if (name == "rot" || name == "rotation") {
          Rotation = Parse.TryVectorYXZRange(value, Vector3.zero);
        }
        if (split[0] == "refPos" || split[0] == "refPosition") {
          UseDefaultRelativePosition = false;
          BasePosition = Parse.TryVectorXZY(value.Split(','), BasePosition);
        }
        if (split[0] == "refPlayer") {
          UseDefaultRelativePosition = false;
          var player = Helper.FindPlayer(value);
          if (player.m_characterID.IsNone()) {
            terminal.AddString("Error: Unable to find the player.");
            return false;
          } else if (!player.m_publicPosition) {
            terminal.AddString("Error: Player doesn't have a public position.");
            return false;
          } else {
            BasePosition = player.m_position;
          }
        }
      }
      Radius = Radius == 0f ? 0.5f : Radius;
      return true;
    }
  }
}
