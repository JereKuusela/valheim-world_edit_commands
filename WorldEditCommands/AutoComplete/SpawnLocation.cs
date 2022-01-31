using System.Collections.Generic;
using DEV;

namespace WorldEditCommands {
  public class SpawnLocationAutoComplete {
    public SpawnLocationAutoComplete() {
      var namedParameters = new List<string>() {
        "seed",
        "pos",
        "rot",
        "refPos",
        "refRot"
      };
      namedParameters.Sort();
      AutoComplete.Register("spawn_location", (int index) => {
        if (index == 0) return ParameterInfo.LocationIds;
        return namedParameters;
      }, new Dictionary<string, System.Func<int, List<string>>>() {
        {
          "seed", (int index) => index == 0 ? ParameterInfo.Create("Location seed", "an integer") : null
        },
        {
          "pos", ParameterInfo.XZY
        },
        {
          "refPos", ParameterInfo.XZY
        },
        {
          "rot", (int index) => index == 0 ? ParameterInfo.Create("Rotation", "a number") : null
        },
        {
          "refRot", (int index) => index == 0 ? ParameterInfo.Create("Reference rotation", "a number") : null
        }
      });
    }
  }
}
