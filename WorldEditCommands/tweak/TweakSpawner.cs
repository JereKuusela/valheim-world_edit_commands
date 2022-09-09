using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakSpawnerCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    if (operation == "globalkey")
      return TweakActions.GlobalKey(view, value);
    throw new NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value) {
    if (operation == "levelchance")
      return TweakActions.LevelChance(view, value);
    if (operation == "spawnradius")
      return TweakActions.SpawnRadius(view, value);
    if (operation == "nearradius")
      return TweakActions.NearRadius(view, value);
    if (operation == "farradius")
      return TweakActions.FarRadius(view, value);
    if (operation == "triggerdistance")
      return TweakActions.TriggerDistance(view, value);
    if (operation == "respawn")
      return TweakActions.RespawnSeconds(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value) {
    if (operation == "maxnear")
      return TweakActions.MaxNear(view, value);
    if (operation == "maxtotal")
      return TweakActions.MaxTotal(view, value);
    if (operation == "spawncondition")
      return TweakActions.SpawnCondition(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value) {
    if (operation == "spawn")
      return TweakActions.Spawns(view, value);
    if (operation == "spawneffect")
      return TweakActions.SpawnEffect(view, value);
    throw new NotImplementedException();
  }

  public TweakSpawnerCommand() {
    Component = typeof(SpawnArea);
    SupportedOperations.Add("globalkey", typeof(string));
    SupportedOperations.Add("maxnear", typeof(int));
    SupportedOperations.Add("maxtotal", typeof(int));
    SupportedOperations.Add("spawncondition", typeof(int));
    SupportedOperations.Add("triggerdistance", typeof(float));
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("levelchance", typeof(float));
    SupportedOperations.Add("spawnradius", typeof(float));
    SupportedOperations.Add("nearradius", typeof(float));
    SupportedOperations.Add("farradius", typeof(float));
    SupportedOperations.Add("spawn", typeof(string[]));
    SupportedOperations.Add("spawneffect", typeof(string[]));

    AutoComplete.Add("globalkey", (int index) => index == 0 ? ParameterInfo.Create("text=<color=yellow>key</color>", "Required global keys to work. Start with - to remove the key. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxnear", (int index) => index == 0 ? ParameterInfo.Create("maxnear=<color=yellow>amount</color>", "Sets the max amount in <color=yellow>nearradius</color>. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxtotal", (int index) => index == 0 ? ParameterInfo.Create("maxtotal=<color=yellow>amount</color>", "Sets the max amount in <color=yellow>farradius</color>. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawncondition", (int index) => index == 0 ? ParameterInfo.Create("spawncondition=<color=yellow>flag</color>", "Sum up: 1 = day only, 2 = night only, 4 = ground only.") : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => ParameterInfo.Create("respawn=<color=yellow>seconds</color>", "Sets the respawn time. No value to reset."));
    AutoComplete.Add("levelchance", (int index) => index == 0 ? ParameterInfo.Create("levelchance=<color=yellow>percent</color>", "Sets the level up chance. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("triggerdistance", (int index) => index == 0 ? ParameterInfo.Create("triggerdistance=<color=yellow>meters</color>", "Sets how far the spawner is active. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnradius", (int index) => index == 0 ? ParameterInfo.Create("spawnradius=<color=yellow>meters</color>", "Sets the spawn radius. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("nearradius", (int index) => index == 0 ? ParameterInfo.Create("nearradius=<color=yellow>meters</color>", "Sets the near radius for <color=yellow>maxnear</color>. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("farradius", (int index) => index == 0 ? ParameterInfo.Create("farradius=<color=yellow>meters</color>", "Sets the far radius for <color=yellow>maxtotal</color>. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) => {
      if (index == 0) return ParameterInfo.Ids;
      if (index == 1) return ParameterInfo.Create("spawn=id,<color=yellow>weight</color>,minLevel,maxLevel", "Spawn chance relative to other spawns.");
      if (index == 2) return ParameterInfo.Create("spawn=id,weight,<color=yellow>minLevel</color>,maxLevel", "Minimum level.");
      if (index == 3) return ParameterInfo.Create("spawn=id,weight,minLevel,<color=yellow>maxLevel</color>", "Maximum level.");
      return ParameterInfo.Create("For additional entries, add more <color>spawn=...</color> parameters.");
    });
    AutoComplete.Add("spawneffect", TweakAutoComplete.Effect);
    Init("tweak_spawner", "Modify spawners");
  }
}

