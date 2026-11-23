using System;
using ServerDevcommands;

namespace WorldEditCommands;

public class TweakPortalCommand : TweakCommand
{
  protected override string DoOperation(ZNetView view, string operation, bool? value)
  {
    if (operation == "restrict")
      return TweakActions.Restrict(view, value);
    throw new NotImplementedException();
  }

  public TweakPortalCommand()
  {
    Component = typeof(TeleportWorld);
    ComponentName = "portal";
    SupportedOperations.Add("restrict", typeof(bool));

    AutoComplete.Add("restrict", index => index == 0 ? ParameterInfo.Create("restrict=<color=yellow>false</color>", "Allows teleporting with any items. No value to reset.") : ParameterInfo.None);
    Init("tweak_portal", "Modify portals");
  }
}
