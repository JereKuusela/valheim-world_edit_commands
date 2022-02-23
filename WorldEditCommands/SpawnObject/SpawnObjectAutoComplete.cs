using System;
using System.Collections.Generic;
using ServerDevcommands;

namespace WorldEditCommands {
  using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
  public class SpawnObjectAutoComplete : SharedObjectAutoComplete {
    public List<string> NamedParameters;
    public SpawnObjectAutoComplete() {
      var NamedParameters = MergeDefault(new List<string>() {
        "hunt",
        "durability",
        "name",
        "crafter",
        "variant",
        "amount",
        "pos",
        "rot",
        "refPlayer",
        "refPos",
        "refRot"
      });
      AutoComplete.Register("spawn_object", (int index) => {
        if (index == 0) return ParameterInfo.Ids;
        return NamedParameters;
      }, MergeDefault(new NamedOptionsFetchers() {
        {
          "hunt", (int index) => ParameterInfo.Flag("Hunt")
        },
        {
          "name", (int index) => index == 0 ? ParameterInfo.Create("Name", "a string") : null
        },
        {
          "crafter", (int index) => index == 0 ? ParameterInfo.Create("Name", "a crafter") : null
        },
        {
          "variant", (int index) => index == 0 ? ParameterInfo.Create("Variant", "an integer range") : null
        },
        {
          "amount", (int index) => index == 0 ? ParameterInfo.Create("Amount", "an integer range") : null
        },
        {
          "pos", (int index) => ParameterInfo.XZY("pos", "Offset from the player / reference position", index)
        },
        {
          "refPos", (int index) => ParameterInfo.XZY("refPos", "Overrides the reference position (player's position)", index)
        },
        {
          "refPlayer", (int index) => index == 0 ? ParameterInfo.PlayerNames : null
        },
        {
          "rot", (int index) => ParameterInfo.YXZ("rot", "Rotation from the player / reference rotation", index)
        },
        {
          "refRot", (int index) => ParameterInfo.YXZ("refRot", "Overrides the reference rotation (player's rotation)", index)
        }
      }));
    }
  }
}
