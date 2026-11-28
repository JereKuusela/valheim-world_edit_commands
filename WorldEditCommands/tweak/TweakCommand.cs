using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace WorldEditCommands;

public abstract class TweakCommand
{
  protected Type Component = typeof(int);
  protected string ComponentName = "";
  protected virtual string DoOperation(ZNetView view, string operation, string? value) => throw new NotImplementedException();
  protected virtual string DoOperation(ZNetView view, string operation, float? value) => throw new NotImplementedException();
  protected virtual string DoOperation(ZNetView view, string operation, string[] value) => throw new NotImplementedException();
  protected virtual string DoOperation(ZNetView view, string operation, int? value) => throw new NotImplementedException();
  protected virtual string DoOperation(ZNetView view, string operation, bool? value) => throw new NotImplementedException();
  protected virtual string DoOperation(ZNetView view, string operation, long? value) => throw new NotImplementedException();
  protected virtual Dictionary<string, object?> Postprocess(ZNetView view, Dictionary<string, object?> operations) => operations;
  protected virtual void Postprocess(GameObject? obj) { }
  protected virtual ZNetView Preprocess(Terminal context, ZNetView view) => view;

  public bool AddComponentAutomatically = true;

  private void Execute(Terminal context, bool force, Dictionary<string, object?> operations, ZNetView[] views)
  {
    Dictionary<ZDOID, long> oldOwner = [];
    views = views.Select(view => Preprocess(context, view)).Where(view =>
    {
      // Preprocess can return null.
      if (!view) return false;
      if (ComponentName != "" && !view.GetComponentInChildren(Component))
      {
        if (force || (views.Length == 1 && AddComponentAutomatically)) return true;
        context.AddString($"Skipped: {view.name} doesn't have the component. Use <color=yellow>force</color> to add it.");
        return false;
      }
      return true;
    }).ToArray();
    UndoHelper.BeginAction();
    try
    {
      foreach (var view in views)
      {
        var zdo = view.GetZDO();
        UndoHelper.AddEditAction(zdo);
        oldOwner.Add(zdo.m_uid, zdo.GetOwner());
        view.ClaimOwnership();
      }
      foreach (var view in views)
        ApplyOperations(context, force, operations, view, views.Length);
    }
    finally
    {
      foreach (var view in views)
      {
        var zdo = view.GetZDO();
        if (oldOwner.TryGetValue(zdo.m_uid, out var owner))
          zdo.SetOwner(owner);
      }
      UndoHelper.EndAction();
    }
    foreach (var view in views)
      Postprocess(Actions.Refresh(view));
  }
  private void ApplyOperations(Terminal context, bool force, Dictionary<string, object?> operations, ZNetView view, int count)
  {
    if (ComponentName != "" && !view.GetComponentInChildren(Component))
    {
      if (force || (count == 1 && AddComponentAutomatically))
      {
        // Dungeons can have vegvisirs so this allows to edit them with the runestone feature.
        // However adding the runestone component wouldn't make any sense.
        if (!view.GetComponent<DungeonGenerator>())
          TweakActions.AddComponent(view, ComponentName);
      }
    }
    var name = Utils.GetPrefabName(view.gameObject);
    foreach (var operation in Postprocess(view, operations))
    {
      var type = SupportedOperations[operation.Key];
      string output;
      if (type == typeof(int))
        output = DoOperation(view, operation.Key, (int?)operation.Value);
      else if (type == typeof(long))
        output = DoOperation(view, operation.Key, (long?)operation.Value);
      else if (type == typeof(float))
        output = DoOperation(view, operation.Key, (float?)operation.Value);
      else if (type == typeof(string[]))
        output = DoOperation(view, operation.Key, (string[])operation.Value!);
      else if (type == typeof(bool))
        output = DoOperation(view, operation.Key, (bool?)operation.Value!);
      else
        output = DoOperation(view, operation.Key, (string?)operation.Value);
      // No operation.
      if (output == "") continue;
      var message = output.Replace("¤", name);
      if (count == 1)
        Helper.AddMessage(context, message);
      else
        context.AddString(message);
    }
  }
  public Dictionary<string, Type> SupportedOperations = [];
  public Dictionary<string, Func<int, List<string>>> AutoComplete = [];

  private TweakParameters Parameters = new([]);
  protected void Init(string name, string description)
  {
    var namedParameters = TweakAutoComplete.WithFilters(AutoComplete.Keys.ToList());
    ServerDevcommands.AutoComplete.Register(name, index => namedParameters, TweakAutoComplete.WithFilters(AutoComplete));
    Helper.Command(name, description, (args) =>
    {
      Parameters = new(SupportedOperations);
      Parameters.ParseCommand(args);
      var views = Parameters.GetObjects();
      Execute(args.Context, Parameters.Force, Parameters.Operations, views);

    });
  }
  public TweakCommand()
  {
  }
}
