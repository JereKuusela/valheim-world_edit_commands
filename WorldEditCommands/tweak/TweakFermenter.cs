using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakFermenterCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
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
    throw new NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "conversion")
      return TweakActions.Conversions(view, value);
    if (operation == "inputeffect")
      return TweakActions.InputEffect(view, value);
    if (operation == "useeffect")
      return TweakActions.UseEffect(view, value);
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

  public TweakFermenterCommand()
  {
    Component = typeof(Fermenter);
    ComponentName = "fermenter";
    SupportedOperations.Add("speed", typeof(float));
    SupportedOperations.Add("conversion", typeof(string[]));
    SupportedOperations.Add("inputeffect", typeof(string[]));
    SupportedOperations.Add("useeffect", typeof(string[]));
    SupportedOperations.Add("outputeffect", typeof(string[]));

    AutoComplete.Add("speed", (int index) => index == 0 ? ParameterInfo.Create("speed=<color=yellow>number</color>", "Conversion speed in seconds. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("conversion", (int index) =>
   {
     if (index == 0) return ParameterInfo.ItemIds;
     if (index == 1) return ParameterInfo.ItemIds;
     if (index == 2) return ParameterInfo.Create("conversion=from,to,<color=yellow>amount</color>", "Amount of output.");
     return ParameterInfo.Create("For additional entries, add more <color>conversion=...</color> parameters.");
   });
    AutoComplete.Add("inputeffect", (int index) => TweakAutoComplete.Effect("inputeffect", index));
    AutoComplete.Add("useeffect", (int index) => TweakAutoComplete.Effect("useeffect", index));
    AutoComplete.Add("outputeffect", (int index) => TweakAutoComplete.Effect("outputeffect", index));
    Init("tweak_fermenter", "Modify fermenters");
  }
}

