using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakSmelterCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "fuel")
      return TweakActions.Fuel(view, value);
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
    if (operation == "maxfuel")
      return TweakActions.MaxFuel(view, value);
    if (operation == "fuelusage")
      return TweakActions.FuelUsage(view, value);
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "conversion")
      return TweakActions.Conversions(view, value);
    if (operation == "inputeffect")
      return TweakActions.InputEffect(view, value);
    if (operation == "fueleffect")
      return TweakActions.FuelEffect(view, value);
    if (operation == "outputeffect")
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

  public TweakSmelterCommand()
  {
    Component = typeof(Smelter);
    ComponentName = "smelter";
    SupportedOperations.Add("maxamount", typeof(int));
    SupportedOperations.Add("maxfuel", typeof(int));
    SupportedOperations.Add("fuel", typeof(string));
    SupportedOperations.Add("fuelusage", typeof(int));
    SupportedOperations.Add("speed", typeof(float));
    SupportedOperations.Add("conversion", typeof(string[]));
    SupportedOperations.Add("inputeffect", typeof(string[]));
    SupportedOperations.Add("fueleffect", typeof(string[]));
    SupportedOperations.Add("outputeffect", typeof(string[]));

    AutoComplete.Add("maxamount", (int index) => index == 0 ? ParameterInfo.Create("maxamount=<color=yellow>number</color>", "Maximum amount of queued items. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("maxfuel", (int index) => index == 0 ? ParameterInfo.Create("maxfuel=<color=yellow>number</color>", "Maximum amount of fuel. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("fuelusage", (int index) => index == 0 ? ParameterInfo.Create("fuelusage=<color=yellow>number</color>", "Required fuel per conversion. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("speed", (int index) => index == 0 ? ParameterInfo.Create("speed=<color=yellow>number</color>", "Conversion speed in seconds. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("fuel", (int index) => index == 0 ? ParameterInfo.ItemIds : ParameterInfo.None);
    AutoComplete.Add("conversion", (int index) =>
   {
     if (index == 0) return ParameterInfo.ItemIds;
     if (index == 1) return ParameterInfo.ItemIds;
     return ParameterInfo.Create("For additional entries, add more <color>conversion=...</color> parameters.");
   });
    AutoComplete.Add("inputeffect", (int index) => TweakAutoComplete.Effect("inputeffect", index));
    AutoComplete.Add("fueleffect", (int index) => TweakAutoComplete.Effect("fueleffect", index));
    AutoComplete.Add("outputeffect", (int index) => TweakAutoComplete.Effect("outputeffect", index));
    Init("tweak_smelter", "Modify smelters");
  }
}

