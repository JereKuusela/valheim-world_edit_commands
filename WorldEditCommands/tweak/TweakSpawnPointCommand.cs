using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakSpawnPointCommand : TweakCommand {
  protected override string DoOperation(ZNetView view, string operation, string? value) {
    if (operation == "spawn")
      return TweakActions.Spawn(view, value);
    throw new NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value) {
    if (operation == "respawntime")
      return TweakActions.Respawn(view, value);
    if (operation == "spawnhealth")
      return TweakActions.SpawnHealth(view, value);
    if (operation == "triggerdistance")
      return TweakActions.TriggerDistance(view, value);
    if (operation == "triggernoise")
      return TweakActions.TriggerNoise(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value) {
    if (operation == "minlevel")
      return TweakActions.MinLevel(view, value);
    if (operation == "maxlevel")
      return TweakActions.MaxLevel(view, value);
    if (operation == "spawncondition")
      return TweakActions.SpawnCondition(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value) {
    if (operation == "spawneffect")
      return TweakActions.SpawnEffect(view, value);
    throw new System.NotImplementedException();
  }

  public TweakSpawnPointCommand() {
    Component = typeof(CreatureSpawner);
    SupportedOperations.Add("minlevel", typeof(int));
    SupportedOperations.Add("maxlevel", typeof(int));
    SupportedOperations.Add("spawncondition", typeof(int));
    SupportedOperations.Add("triggerdistance", typeof(float));
    SupportedOperations.Add("respawn", typeof(float));
    SupportedOperations.Add("spawnhealth", typeof(float));
    SupportedOperations.Add("spawn", typeof(string));
    SupportedOperations.Add("spawneffect", typeof(string[]));

    AutoComplete.Add("minlevel", (int index) => index == 0 ? ParameterInfo.Create("minlevel=<color=yellow>number</color>. No value to reset.", "Sets the minimum level.") : ParameterInfo.None);
    AutoComplete.Add("maxlevel", (int index) => index == 0 ? ParameterInfo.Create("maxlevel=<color=yellow>number</color>. No value to reset.", "Sets the maximum level.") : ParameterInfo.None);
    AutoComplete.Add("triggernoise", (int index) => index == 0 ? ParameterInfo.Create("triggernoise=<color=yellow>meters</color>", "Sets how loud noise activates the spawn point. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("triggerdistance", (int index) => index == 0 ? ParameterInfo.Create("triggerdistance=<color=yellow>meters</color>", "Sets how far distance activates the spawn point. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawncondition", (int index) => index == 0 ? ParameterInfo.Create("spawncondition=<color=yellow>flag</color>", "Sum up: 1 = day only, 2 = night only.") : ParameterInfo.None);
    AutoComplete.Add("respawn", (int index) => index == 0 ? ParameterInfo.Create("respawn=<color=yellow>minutes/false</color>. No value to reset.", "Sets the respawn time.") : ParameterInfo.None);
    AutoComplete.Add("spawnhealth", (int index) => index == 0 ? ParameterInfo.Create("spawnhealth=<color=yellow>number</color>. No value to reset.", "Sets the creature health.") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) => index == 0 ? ParameterInfo.ObjectIds : ParameterInfo.None);
    AutoComplete.Add("spawneffect", TweakAutoComplete.Effect);
    Init("tweak_spawnpoint", "Modify spawn points");
  }
}

