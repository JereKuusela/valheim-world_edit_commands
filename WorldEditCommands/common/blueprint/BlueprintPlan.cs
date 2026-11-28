// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all.
using System;
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;

namespace Data;

public class PlannedObject(string prefab, Vector3 pos, Quaternion rot, Vector3 scale, DataEntry? data, bool snapToGround)
{
  public readonly string Prefab = prefab;
  public readonly Vector3 Pos = pos;
  public readonly Quaternion Rot = rot;
  public readonly Vector3 Scale = scale;
  public readonly DataEntry? Data = data;
  public readonly bool SnapToGround = snapToGround;
}

public class PlannedTerrain(Blueprint source, Vector3 pos, Quaternion rot)
{
  public readonly Blueprint Source = source;
  public readonly Vector3 Pos = pos;
  public readonly Quaternion Rot = rot;
}

/// <summary>
/// The outcome of placing a blueprint: nested blueprints are expanded and chance, swaps and data overrides are resolved.
/// Random rolls happen here in a fixed order, so hosts only have to spawn the result.
/// </summary>
public class BlueprintPlan
{
  private const int MaxDepth = 8;
  private static readonly HashSet<string> WarnedPrefabs = [];

  public readonly List<PlannedObject> Objects = [];
  public readonly List<PlannedTerrain> Terrains = [];

  public static BlueprintPlan Create(Blueprint bp, Vector3 pos, Quaternion rot, Vector3 scale,
    Func<DataEntry?, string, DataEntry?> dataOverride, Func<string, string> prefabOverride, Func<string, Blueprint?> getBlueprint)
  {
    BlueprintPlan plan = new();
    plan.Add(bp, pos, rot, scale, dataOverride, prefabOverride, getBlueprint, 0);
    return plan;
  }

  private void Add(Blueprint bp, Vector3 pos, Quaternion rot, Vector3 scale,
    Func<DataEntry?, string, DataEntry?> dataOverride, Func<string, string> prefabOverride, Func<string, Blueprint?> getBlueprint, int depth)
  {
    if (bp.TerrainHeight?.HasValues == true || bp.TerrainPaint?.HasValues == true)
      Terrains.Add(new(bp, pos, rot));
    var picked = PickGroups(bp.Objects, () => UnityEngine.Random.value);
    foreach (var obj in bp.Objects)
    {
      if (obj.Group != "")
      {
        if (picked[obj.Group] != obj) continue;
      }
      else if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      var objPos = pos + rot * obj.Pos;
      var objRot = rot * obj.Rot;
      var objScale = obj.Scale.HasValue ? Vector3.Scale(scale, obj.Scale.Value) : scale;
      var prefabName = prefabOverride(obj.Prefab);
      // Empty is valid for removing objects.
      if (prefabName == "") continue;
      if (!ZNetScene.instance.GetPrefab(prefabName))
      {
        var nested = depth < MaxDepth ? getBlueprint(prefabName) : null;
        if (nested != null)
          Add(nested, objPos, objRot, objScale, dataOverride, prefabOverride, getBlueprint, depth + 1);
        else if (WarnedPrefabs.Add(prefabName))
          Log.Warning($"Blueprint {bp.Name}: prefab / blueprint {prefabName} not found.");
        continue;
      }
      Objects.Add(new(prefabName, objPos, objRot, objScale, dataOverride(obj.Data, prefabName), obj.SnapToGround));
    }
  }

  // One roll per group. Chances are probabilities, so a remainder under 1 means none of the group is picked.
  // Chances over 1 in total are scaled down.
  public static Dictionary<string, BlueprintObject?> PickGroups(IEnumerable<BlueprintObject> objects, Func<float> roll)
  {
    Dictionary<string, BlueprintObject?> picked = [];
    foreach (var group in objects.Where(obj => obj.Group != "").GroupBy(obj => obj.Group))
    {
      var members = group.ToList();
      var total = Math.Max(1f, members.Sum(obj => obj.Chance));
      var value = roll() * total;
      BlueprintObject? selected = null;
      foreach (var member in members)
      {
        value -= member.Chance;
        if (value >= 0f) continue;
        selected = member;
        break;
      }
      picked[group.Key] = selected;
    }
    return picked;
  }

  // Terrains of nested blueprints are merged to the placement of the main blueprint, so they can be applied at once.
  public (BlueprintTerrain<float>? Height, BlueprintTerrain<Color>? Paint) GetTerrain(Vector3 pos, Quaternion rot) => GetTerrain(Terrains, pos, rot);

  // Terrains of multiple plans can be merged too, for example to apply all terrain of a zone at once.
  public static (BlueprintTerrain<float>? Height, BlueprintTerrain<Color>? Paint) GetTerrain(IEnumerable<PlannedTerrain> terrains, Vector3 pos, Quaternion rot)
  {
    var list = terrains.ToList();
    return (
      Merge(list.Where(t => t.Source.TerrainHeight?.HasValues == true).Select(t => (t.Source.TerrainHeight!, t.Pos, t.Rot)).ToList(), pos, rot, (value, y) => value + y),
      Merge(list.Where(t => t.Source.TerrainPaint?.HasValues == true).Select(t => (t.Source.TerrainPaint!, t.Pos, t.Rot)).ToList(), pos, rot, null)
    );
  }

  private static BlueprintTerrain<T>? Merge<T>(List<(BlueprintTerrain<T> Terrain, Vector3 Pos, Quaternion Rot)> sources, Vector3 pos, Quaternion rot, Func<T, float, T>? shiftHeight) where T : struct
  {
    if (sources.Count == 0) return null;
    // Usual case of a single terrain at the placement needs no merging.
    if (sources.Count == 1 && sources[0].Pos == pos && sources[0].Rot == rot) return sources[0].Terrain;
    var merged = BlueprintTerrain<T>.Merge(sources, pos, rot, shiftHeight);
    if (merged == null)
      Log.Warning("Terrains of nested blueprints cover too large area and were ignored.");
    return merged;
  }
}
