using System;
using System.Linq;
using Data;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands;

public class SpawnBlueprintCommand
{
  public const string Name = "spawn_blueprint";

  private static Blueprint? GetBlueprint(string name) => BlueprintCache.TryGet(name, out var blueprint) ? blueprint : null;

  private static Vector3 SnapToFloor(Vector3 position)
  {
    if (ZoneSystem.instance.FindFloor(position, out var height) || ZoneSystem.instance.FindFloor(position + Vector3.up * 100f, out height))
      position.y = height;
    return position;
  }

  private static bool Spawn(PlannedObject piece, SpawnObjectParameters pars)
  {
    var prefab = ZNetScene.instance.GetPrefab(piece.Prefab);
    if (!prefab || !prefab.GetComponent<ZNetView>()) return false;
    var position = piece.SnapToGround ? SnapToFloor(piece.Pos) : piece.Pos;
    try
    {
      DataHelper.Init(prefab, position, piece.Rot, piece.Scale == Vector3.one ? null : piece.Scale, piece.Data, pars.DataParameters);
      var obj = UnityEngine.Object.Instantiate(prefab, position, piece.Rot);
      var zdo = obj.GetComponent<ZNetView>().GetZDO();
      if (zdo == null || !zdo.IsValid())
      {
        UnityEngine.Object.Destroy(obj);
        return false;
      }
      if (!ZNet.instance.IsServer())
        ZDOMan.instance.ClientChanged(zdo.m_uid);
      return true;
    }
    finally
    {
      DataHelper.CleanUp();
    }
  }

  public SpawnBlueprintCommand()
  {
    AutoComplete.Register(Name, index => index == 0 ? BlueprintCache.GetNames().ToList() :
      ["amount", "data", "from", "pos", "radius", "refPlayer", "refRot", "rot", "scale", "to"], new()
      {
        { "amount", index => index == 0 ? ParameterInfo.CreateWithMinMax("amount", "integer", "Number of blueprint copies.") : ParameterInfo.None },
        { "data", index => DataLoading.DataKeys },
        { "from", index => ParameterInfo.XZY("from", "Overrides the player position", index) },
        { "pos", index => ParameterInfo.FRU("pos", "Offset from the reference position", index) },
        { "radius", index => index == 0 ? ParameterInfo.CreateWithMinMax("radius", "number", "Placement radius for blueprint copies.") : ParameterInfo.None },
        { "refPlayer", index => index == 0 ? ParameterInfo.PlayerNames : ParameterInfo.None },
        { "refRot", index => ParameterInfo.YawRollPitch("refRot", "Overrides the player rotation", index) },
        { "rot", index => ParameterInfo.YawRollPitch("rot", "Rotation from the reference rotation", index) },
        { "scale", index => ParameterInfo.Scale("scale", "Scale of the blueprint pieces", index) },
        { "to", index => ParameterInfo.XZY("to", "End position for multiple copies", index) }
      });
    Helper.Command(Name, "Spawns blueprint objects", args =>
    {
      Helper.ArgsCheck(args, 2, "Missing blueprint name.");
      var name = args[1];
      var blueprint = GetBlueprint(name) ?? throw new InvalidOperationException($"Unable to load blueprint: {name}.");
      SpawnObjectParameters pars = new(args);
      var count = pars.Amount == null ? 1 : Helper.RandomValue(pars.Amount);
      var spawned = 0;
      var skipped = 0;
      var hasTerrain = false;
      UndoHelper.Run(() =>
      {
        for (int index = 0; index < count; index++)
        {
          var position = pars.GetSpawnPosition(index, count);
          if (pars.Snap) position = SnapToFloor(position);
          var rotation = pars.BaseRotation * Quaternion.Euler(Helper.RandomValue(pars.Rotation));
          var scale = pars.Scale == null ? Vector3.one : Helper.RandomValue(pars.Scale);
          var plan = BlueprintPlan.Create(blueprint, position, rotation, scale,
            (data, _) => DataHelper.Merge(data, pars.Data), prefab => prefab, GetBlueprint);
          hasTerrain |= plan.Terrains.Count > 0;
          foreach (var piece in plan.Objects)
          {
            if (Spawn(piece, pars)) spawned++;
            else skipped++;
          }
        }
      });
      args.Context.AddString($"Spawned blueprint {name}: {spawned} objects.");
      if (skipped > 0) args.Context.AddString($"Skipped {skipped} objects that could not be tracked for undo.");
      if (hasTerrain) args.Context.AddString("Blueprint terrain was not applied.");
    });
  }
}