using System;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakSpawnerCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "globalkey")
      return TweakActions.GlobalKey(view, value);
    if (operation == "faction")
      return TweakActions.Faction(view, value);
    throw new NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    if (operation == "spawnhealth")
      return TweakActions.SpawnHealth(view, value);
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

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    if (operation == "minlevel")
      return TweakActions.MinLevel(view, value);
    if (operation == "maxlevel")
      return TweakActions.MaxLevel(view, value);
    if (operation == "maxnear")
      return TweakActions.MaxNear(view, value);
    if (operation == "maxtotal")
      return TweakActions.MaxTotal(view, value);
    if (operation == "spawncondition")
      return TweakActions.SpawnCondition(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "spawn")
      return TweakActions.Spawns(view, value);
    if (operation == "spawneffect")
      return TweakActions.SpawnEffect(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    throw new NotImplementedException();
  }

  public TweakSpawnerCommand()
  {
    Component = typeof(SpawnArea);
    ComponentName = "spawner";
    SupportedOperations.Add("globalkey", typeof(string));
    SupportedOperations.Add("maxnear", typeof(int));
    SupportedOperations.Add("maxtotal", typeof(int));
    SupportedOperations.Add("minlevel", typeof(int));
    SupportedOperations.Add("maxlevel", typeof(int));
    SupportedOperations.Add("spawncondition", typeof(int));
    SupportedOperations.Add("triggerdistance", typeof(float));
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("spawnhealth", typeof(float));
    SupportedOperations.Add("levelchance", typeof(float));
    SupportedOperations.Add("spawnradius", typeof(float));
    SupportedOperations.Add("nearradius", typeof(float));
    SupportedOperations.Add("farradius", typeof(float));
    SupportedOperations.Add("spawn", typeof(string[]));
    SupportedOperations.Add("spawneffect", typeof(string[]));
    SupportedOperations.Add("faction", typeof(string));

    AutoComplete.Add("minlevel", (int index) => index == 0 ? ParameterInfo.Create("minlevel=<color=yellow>number</color>", "Minimum level (level 1 = no star). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxlevel", (int index) => index == 0 ? ParameterInfo.Create("maxlevel=<color=yellow>number</color>", "Maximum level (level 1 = no star). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("globalkey", (int index) => index == 0 ? ParameterInfo.Create("text=<color=yellow>key</color>", "Required global keys to work. Start with - to remove the key. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxnear", (int index) => index == 0 ? ParameterInfo.Create("maxnear=<color=yellow>number</color>", "Maximum amount of spawns within the <color=yellow>nearradius</color>. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxtotal", (int index) => index == 0 ? ParameterInfo.Create("maxtotal=<color=yellow>number</color>", "Maximum amount of spawns within the <color=yellow>farradius</color>. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawncondition", (int index) => index == 0 ? ParameterInfo.Create("spawncondition=<color=yellow>flag</color>", "Sum up: 1 = day only, 2 = night only, 4 = ground only.") : ParameterInfo.None);
    AutoComplete.Add("spawnhealth", (int index) => index == 0 ? ParameterInfo.Create("spawnhealth=<color=yellow>number</color>", "Overrides the creature health. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => ParameterInfo.Create("respawn=<color=yellow>seconds</color>", "Sets the respawn time. No value to reset."));
    AutoComplete.Add("levelchance", (int index) => index == 0 ? ParameterInfo.Create("levelchance=<color=yellow>percent</color>", "Level up chance (from 0 to 100). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("faction", (int index) => index == 0 ? Enum.GetNames(typeof(Character.Faction)).ToList() : ParameterInfo.None);
    AutoComplete.Add("triggerdistance", (int index) => index == 0 ? ParameterInfo.Create("triggerdistance=<color=yellow>meters</color>", "Player distance to activate the spawner. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnradius", (int index) => index == 0 ? ParameterInfo.Create("spawnradius=<color=yellow>meters</color>", "Maximum spawn radius. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("nearradius", (int index) => index == 0 ? ParameterInfo.Create("nearradius=<color=yellow>meters</color>", "Radius for <color=yellow>maxnear</color>. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("farradius", (int index) => index == 0 ? ParameterInfo.Create("farradius=<color=yellow>meters</color>", "Radius for <color=yellow>maxtotal</color>. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) =>
    {
      if (index == 0) return ParameterInfo.Ids;
      if (index == 1) return ParameterInfo.Create("spawn=id,<color=yellow>weight</color>,minlevel,maxlevel,health", "Spawn chance relative to other spawns.");
      if (index == 2) return ParameterInfo.Create("spawn=id,weight,<color=yellow>minlevel</color>,maxlevel,health", "Minimum level (level 1 = 0 star).");
      if (index == 3) return ParameterInfo.Create("spawn=id,weight,minlevel,<color=yellow>maxlevel</color>,health", "Maximum level (level 1 = 0 star).");
      if (index == 4) return ParameterInfo.Create("spawn=id,weight,minlevel,maxlevel,<color=yellow>health</color>", "Health.");
      return ParameterInfo.Create("For additional entries, add more <color>spawn=...</color> parameters.");
    });
    AutoComplete.Add("spawneffect", (int index) => TweakAutoComplete.Effect("spawneffect", index));
    Init("tweak_spawner", "Modify spawners");
  }
}

