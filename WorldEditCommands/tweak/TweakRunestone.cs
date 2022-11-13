using System;
using System.Linq;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakRunestoneCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, string? value)
  {
    if (operation == "text")
      return TweakActions.Text(view, value);
    if (operation == "name")
      return TweakActions.Name(view, value);
    if (operation == "topic")
      return TweakActions.Topic(view, value);
    if (operation == "discover")
      return TweakActions.Discover(view, value);
    throw new System.NotImplementedException();
  }
  protected override string DoOperation(ZNetView view, string operation, float? value)
  {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, int? value)
  {
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, string[] value)
  {
    if (operation == "compendium")
      return TweakActions.Compendium(view, value);
    throw new System.NotImplementedException();
  }

  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    throw new NotImplementedException();
  }

  public TweakRunestoneCommand()
  {
    Component = typeof(RuneStone);
    ComponentName = "runestone";
    SupportedOperations.Add("name", typeof(string));
    SupportedOperations.Add("text", typeof(string));
    SupportedOperations.Add("compendium", typeof(string));
    SupportedOperations.Add("topic", typeof(string));
    SupportedOperations.Add("discover", typeof(string));

    AutoComplete.Add("name", (int index) => index == 0 ? ParameterInfo.Create("name=<color=yellow>text</color>", "Display name. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("text", (int index) => ParameterInfo.Create("text=<color=yellow>text</color>", "Shown text. Use _ as the space. No value to reset."));
    AutoComplete.Add("compendium", (int index) => ParameterInfo.Create("compendium=<color=yellow>text</color>", "Entry in the compendium. Use _ as the space. No value to reset. Add new parameter for more entries."));
    AutoComplete.Add("topic", (int index) => index == 0 ? ParameterInfo.Create("topic=<color=yellow>text</color>", "Show text topic. Use _ as the space. No value to reset.") : ParameterInfo.None);
    AutoComplete.Add("discover", (int index) =>
    {
      if (index == 0) return ParameterInfo.LocationIds;
      if (index == 1) return ParameterInfo.Create("discover=id,<color=yellow>pinName</color>,pinType,openMap", "Pin name on the map.");
      if (index == 2) return Enum.GetNames(typeof(Minimap.PinType)).ToList();
      if (index == 3) return ParameterInfo.Create("discover=id,pinName,pinType,<color=yellow>openMap</color>", "1 = automatically open the map.");
      return ParameterInfo.None;
    });
    Init("tweak_runestone", "Modify runestones");
  }
}
