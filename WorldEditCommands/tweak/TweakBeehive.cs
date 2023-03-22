using System;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakBeehiveCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "spawn")
      return TweakActions.Spawn(view, value);
    if (operation == "spawnoffset")
      return TweakActions.SpawnOffset(view, value);
    if (operation == "coveroffset")
      return TweakActions.CoverOffset(view, value);
    if (operation == "biome")
      return TweakActions.Biome(view, value);
    throw new NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    if (operation == "speed")
      return TweakActions.Speed(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    if (operation == "maxamount")
      return TweakActions.MaxAmount(view, value);
    if (operation == "spawncondition")
      return TweakActions.SpawnCondition(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "spawneffect")
      return TweakActions.OutputEffect(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, long? value)
  {
    throw new NotImplementedException();
  }

  public TweakBeehiveCommand()
  {
    Component = typeof(Beehive);
    ComponentName = "beehive";
    SupportedOperations.Add("maxamount", typeof(int));
    SupportedOperations.Add("spawncondition", typeof(int));
    SupportedOperations.Add("spawn", typeof(string));
    SupportedOperations.Add("speed", typeof(float));
    SupportedOperations.Add("biome", typeof(string));
    SupportedOperations.Add("spawnoffset", typeof(string));
    SupportedOperations.Add("coveroffset", typeof(string));
    SupportedOperations.Add("spawneffect", typeof(string[]));

    AutoComplete.Add("maxamount", (int index) => index == 0 ? ParameterInfo.Create("maxamount=<color=yellow>number</color>", "Maximum amount of stored items. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawn", (int index) => index == 0 ? ParameterInfo.ItemIds : ParameterInfo.None);
    AutoComplete.Add("biome", (int index) => Enum.GetNames(typeof(Heightmap.Biome)).ToList());
    AutoComplete.Add("speed", (int index) => index == 0 ? ParameterInfo.Create("speed=<color=yellow>number</color>", "Production speed in seconds. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("spawncondition", (int index) => index == 0 ? ParameterInfo.Create("spawncondition=<color=yellow>flag</color>", "Sum up: 1 = day only.") : ParameterInfo.None);
    AutoComplete.Add("coveroffset", (int index) => ParameterInfo.XZY("coveroffset", "Offset for calculating cover.", index));
    AutoComplete.Add("spawnoffset", (int index) => ParameterInfo.XZY("spawnoffset", "Offset for spawning items. Also sets the <color=yellow>spawneffect</color> position.", index));
    AutoComplete.Add("spawneffect", (int index) => TweakAutoComplete.Effect("spawneffect", index));
    Init("tweak_beehive", "Modify beehives");
  }
}

