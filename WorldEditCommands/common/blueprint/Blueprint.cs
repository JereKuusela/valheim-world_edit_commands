// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all.
using System;
using System.Collections.Generic;
using Service;
using UnityEngine;
using Common;

namespace Data;

public class Blueprint
{
  // A square with this half-extent can touch at most 8 x 8 terrain zones.
  // Keeping the parser-side bound aligned with Terrain.ApplyBlueprint avoids
  // letting blueprint radius calculations reach ZoneSystem with huge values.
  private const float MaxTerrainApplicationRadius = ZoneSystem.c_ZoneSize * 3.5f;
  public string Name;
  // Optional metadata from the file. Name is always the file name.
  public string Title = "";
  public string Description = "";
  public string Creator = "";
  public string Category = "";
  public Vector3 Coordinates = Vector3.zero;
  public Vector3 Rotation = Vector3.zero;
  public List<BlueprintObject> Objects = [];
  public const string DefaultCenterPiece = "piece_bpcenterpoint";
  public string CenterPiece = DefaultCenterPiece;
  public float Radius = 0f;
  public List<SnapPoint> SnapPoints = [];
  public Vector3 Size = Vector3.one;
  public BlueprintTerrain<float>? TerrainHeight;
  public BlueprintTerrain<Color>? TerrainPaint;

  public Blueprint(string name)
  {
    Name = name;
  }
  private void AddSnapPoint(Vector3 pos, Quaternion rot, int index)
  {
    if (SnapPoints.Count <= index)
      SnapPoints.Add(new(pos, rot));
    else
      SnapPoints[index] = new(pos, rot);
  }
  // Provides a way to override or load snap points from pieces or coordinates.
  // Snap point system is used for dungeon room connections.
  public void LoadSnapPoints(string[] snapPieces)
  {
    for (var i = 0; i < snapPieces.Length; ++i)
    {
      var piece = snapPieces[i];
      if (piece.Split(',').Length == 3)
      {
        var pos = Parse.VectorXZY(piece);
        AddSnapPoint(pos, Quaternion.identity, i);
        continue;
      }
      var success = false;
      foreach (var obj in Objects)
      {
        if (obj.Prefab != piece) continue;
        if (obj.Chance == 0f) continue;
        obj.Chance = 0f;
        AddSnapPoint(obj.Pos, obj.Rot, i);
        success = true;
        break;
      }
      if (!success)
        Log.Warning($"Snap point piece {piece} not found in blueprint {Name}.");
    }
  }
  // Editors use exact mode: the center piece stays visible, the height isn't lifted and snap points move with the objects.
  public Vector3 Center(string centerPiece = "", bool exact = false)
  {
    if (centerPiece != "") CenterPiece = centerPiece;
    Bounds bounds = new();
    var hasObjects = Objects.Count > 0;
    var y = hasObjects ? float.MaxValue : 0f;
    Quaternion rot = Quaternion.identity;
    foreach (var obj in Objects)
    {
      y = Mathf.Min(y, obj.Pos.y);
      bounds.Encapsulate(obj.Pos);
    }
    // Slightly towards the ground to prevent gaps.
    if (hasObjects && !exact) y += 0.05f;
    Size = hasObjects ? bounds.size : Vector3.zero;
    Vector3 center = hasObjects ? new(bounds.center.x, y, bounds.center.z) : Vector3.zero;
    foreach (var obj in Objects)
    {
      if (obj.Prefab == CenterPiece)
      {
        center = obj.Pos;
        rot = Quaternion.Inverse(obj.Rot);
        // Bit hacky way to prevent it from being spawned.
        if (!exact) obj.Chance = 0f;
        break;
      }
    }
    Radius = Utils.LengthXZ(bounds.extents);
    foreach (var obj in Objects)
      obj.Pos -= center;
    if (exact)
    {
      foreach (var snap in SnapPoints)
        snap.Pos -= center;
    }
    if (TerrainHeight != null)
    {
      TerrainHeight.MapValues(value => value - center.y);
      TerrainHeight.Center(center, rot);
    }
    if (TerrainPaint != null)
      TerrainPaint.Center(center, rot);
    if (rot != Quaternion.identity)
    {
      foreach (var obj in Objects)
      {
        obj.Pos = rot * obj.Pos;
        obj.Rot = rot * obj.Rot;
      }
      if (exact)
      {
        foreach (var snap in SnapPoints)
        {
          snap.Pos = rot * snap.Pos;
          snap.Rot = rot * snap.Rot;
        }
      }
    }
    if (exact)
    {
      var exactRadius = Mathf.Max(TerrainHeight?.GetRadius() ?? 0f, TerrainPaint?.GetRadius() ?? 0f);
      Radius = Mathf.Max(Radius, exactRadius);
      return center;
    }
    if (TerrainHeight != null && !IsValidTerrain(TerrainHeight, IsFinite))
    {
      Log.Warning($"Blueprint {Name}: Ignoring an invalid or oversized terrain height snapshot.");
      TerrainHeight = null;
    }
    if (TerrainPaint != null && !IsValidTerrain(TerrainPaint, IsFinite))
    {
      Log.Warning($"Blueprint {Name}: Ignoring an invalid or oversized terrain paint snapshot.");
      TerrainPaint = null;
    }
    var terrainRadius = Mathf.Max(TerrainHeight?.GetRadius() ?? 0f, TerrainPaint?.GetRadius() ?? 0f);
    var terrainMargin = Mathf.Max(1f,
      TerrainHeight?.HasValues == true ? TerrainHeight.DistanceBetweenNodes : 0f,
      TerrainPaint?.HasValues == true ? TerrainPaint.DistanceBetweenNodes : 0f);
    if (TerrainHeight?.HasValues == true && TerrainPaint?.HasValues == true &&
        (!IsFinite(terrainRadius + terrainMargin) || terrainRadius + terrainMargin > MaxTerrainApplicationRadius))
    {
      Log.Warning($"Blueprint {Name}: Ignoring terrain snapshots whose combined bounds cover too many zones.");
      TerrainHeight = null;
      TerrainPaint = null;
      terrainRadius = 0f;
    }
    Radius = Mathf.Max(Radius, terrainRadius);
    return center;
  }

  private static bool IsValidTerrain<T>(BlueprintTerrain<T> terrain, Func<T, bool> valueIsFinite) where T : struct
  {
    var rotationMagnitude = terrain.CenterRotation.x * terrain.CenterRotation.x +
      terrain.CenterRotation.y * terrain.CenterRotation.y +
      terrain.CenterRotation.z * terrain.CenterRotation.z +
      terrain.CenterRotation.w * terrain.CenterRotation.w;
    if (!IsFinite(terrain.CenterPosition) || !IsFinite(terrain.CenterRotation) ||
        !IsFinite(rotationMagnitude) || rotationMagnitude <= 0.0001f ||
        !IsFinite(terrain.DistanceBetweenNodes) || terrain.DistanceBetweenNodes <= 0f ||
        !terrain.AllValues(valueIsFinite))
      return false;
    var radius = terrain.GetRadius();
    var applicationRadius = radius + Mathf.Max(1f, terrain.DistanceBetweenNodes);
    return IsFinite(radius) && radius >= 0f && IsFinite(applicationRadius) &&
      applicationRadius <= MaxTerrainApplicationRadius;
  }

  private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
  private static bool IsFinite(Vector3 value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
  private static bool IsFinite(Quaternion value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z) && IsFinite(value.w);
  private static bool IsFinite(Color value) => IsFinite(value.r) && IsFinite(value.g) && IsFinite(value.b) && IsFinite(value.a);
}
