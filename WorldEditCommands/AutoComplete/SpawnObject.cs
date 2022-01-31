using System.Collections.Generic;
using DEV;

namespace WorldEditCommands {
  public class SpawnObjectAutoComplete {
    public SpawnObjectAutoComplete() {
      var namedParameters = new List<string>() {
        "tame",
        "hunt",
        "health",
        "durability",
        "name",
        "crafter",
        "variant",
        "star",
        "level",
        "amount",
        "scale",
        "pos",
        "rot",
        "refPlayer",
        "refPos",
        "refRot"
      };
      namedParameters.Sort();
      AutoComplete.Register("spawn_object", (int index) => {
        if (index == 0) return ParameterInfo.Ids;
        return namedParameters;
      }, new Dictionary<string, System.Func<int, List<string>>>() {
        {
          "tame", (int index) => index == 0 ? ParameterInfo.Flag("Tame") : null
        },
        {
          "hunt", (int index) => index == 0 ? ParameterInfo.Flag("Hunt") : null
        },
        {
          "health", (int index) => index == 0 ? ParameterInfo.Create("Health", "a number") : null
        },
        {
          "durability", (int index) => index == 0 ? ParameterInfo.Create("Durability", "a number") : null
        },
        {
          "name", (int index) => index == 0 ? ParameterInfo.Create("Name", "a string") : null
        },
        {
          "crafter", (int index) => index == 0 ? ParameterInfo.Create("Name", "a crafter") : null
        },
        {
          "variant", (int index) => index == 0 ? ParameterInfo.Create("Variant", "an integer") : null
        },
        {
          "star", (int index) => index == 0 ? ParameterInfo.Create("Star", "an integer") : null
        },
        {
          "level", (int index) => index == 0 ? ParameterInfo.Create("Level", "an integer") : null
        },
        {
          "amount", (int index) => index == 0 ? ParameterInfo.Create("Amount", "an integer") : null
        },
        {
          "scale", ParameterInfo.Scale
        },
        {
          "pos", ParameterInfo.XZY
        },
        {
          "refPos", ParameterInfo.XZY
        },
        {
          "refPlayer", (int index) => index == 0 ? ParameterInfo.PlayerNames : null
        },
        {
          "rot", ParameterInfo.YXZ
        },
        {
          "refRot", ParameterInfo.YXZ
        }
      });
    }
  }
}
