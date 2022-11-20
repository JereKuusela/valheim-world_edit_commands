using System;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakSpawnPointCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "spawndata")
      return TweakActions.SpawnData(view, value);
    if (operation == "spawn")
      return TweakActions.Spawn(view, value);
    if (operation == "faction")
      return TweakActions.Faction(view, value);
    throw new NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    if (operation == "levelchance")
      return TweakActions.LevelChance(view, value);
    if (operation == "respawn")
      return TweakActions.Respawn(view, value);
    if (operation == "spawnhealth")
      return TweakActions.SpawnHealth(view, value);
    if (operation == "triggerdistance")
      return TweakActions.TriggerDistance(view, value);
    if (operation == "triggernoise")
      return TweakActions.TriggerNoise(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    if (operation == "minlevel")
      return TweakActions.MinLevel(view, value);
    if (operation == "maxlevel")
      return TweakActions.MaxLevel(view, value);
    if (operation == "spawncondition")
      return TweakActions.SpawnCondition(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "spawneffect")
      return TweakActions.SpawnEffect(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, long? value)
  {
    throw new NotImplementedException();
  }

  public TweakSpawnPointCommand()
  {
    Component = typeof(CreatureSpawner);
    ComponentName = "spawnpoint";
    SupportedOperations.Add("minlevel", typeof(int));
    SupportedOperations.Add("maxlevel", typeof(int));
    SupportedOperations.Add("spawncondition", typeof(int));
    SupportedOperations.Add("levelchance", typeof(float));
    SupportedOperations.Add("triggerdistance", typeof(float));
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("spawnhealth", typeof(float));
    SupportedOperations.Add("spawn", typeof(string));
    SupportedOperations.Add("spawneffect", typeof(string[]));
    SupportedOperations.Add("faction", typeof(string));
    SupportedOperations.Add("spawndata", typeof(string));

    AutoComplete.Add("faction", (int index) => index == 0 ? Enum.GetNames(typeof(Character.Faction)).ToList() : ParameterInfo.None);
    AutoComplete.Add("minlevel", (int index) => index == 0 ? ParameterInfo.Create("minlevel=<color=yellow>number</color>", "Minimum level (level 1 = no star). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxlevel", (int index) => index == 0 ? ParameterInfo.Create("maxlevel=<color=yellow>number</color>", "Maximum level (level 1 = no star). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("triggernoise", (int index) => index == 0 ? ParameterInfo.Create("triggernoise=<color=yellow>meters</color>", "Required noise to activate the spawn point. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("triggerdistance", (int index) => index == 0 ? ParameterInfo.Create("triggerdistance=<color=yellow>meters</color>", "Required distance to activate the spawn point. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawncondition", (int index) => index == 0 ? ParameterInfo.Create("spawncondition=<color=yellow>flag</color>", "1 = day only, 2 = night only.") : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => index == 0 ? ParameterInfo.Create("respawn=<color=yellow>minutes/false</color>", "Respawn time. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("levelchance", (int index) => index == 0 ? ParameterInfo.Create("levelchance=<color=yellow>percent</color>", "Level up chance (from 0 to 100). No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawnhealth", (int index) => index == 0 ? ParameterInfo.Create("spawnhealth=<color=yellow>number</color>", "Overrides the creature health. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) => index == 0 ? ParameterInfo.ObjectIds : ParameterInfo.None);
    AutoComplete.Add("spawneffect", (int index) => TweakAutoComplete.Effect("spawneffect", index));
    AutoComplete.Add("spawndata", (int index) => index == 0 ? ParameterInfo.Create("spawndata=<color=yellow>base64 encoded</color", "ZDO data.") : ParameterInfo.None);
    Init("tweak_spawnpoint", "Modify spawn points");
  }
}

