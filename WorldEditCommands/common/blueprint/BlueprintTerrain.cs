// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data;

public class BlueprintTerrain<T> where T : struct
{
  private const int MaxAxis = 2048;
  private const int MaxCells = 1024 * 1024;
  private readonly List<T?[]> Rows = [];
  private int ValueCount;

  public Vector3 CenterPosition;
  public Quaternion CenterRotation = Quaternion.identity;
  public float DistanceBetweenNodes = 1f;
  // 0 applies the terrain exactly, 1 fades it out over the whole radius.
  public float Smooth = 0f;
  public int Width { get; private set; }
  public int Height => Rows.Count;
  public bool HasValues => ValueCount > 0;

  public void Initialize(int width, int height, Vector3 centerPosition)
  {
    Rows.Clear();
    ValueCount = 0;
    Width = width;
    CenterPosition = centerPosition;
    for (var z = 0; z < height; ++z)
      Rows.Add(new T?[width]);
  }

  public void Set(int x, int z, T value)
  {
    if (x < 0 || x >= Width || z < 0 || z >= Height) return;
    var row = Rows[z];
    if (!row[x].HasValue) ValueCount++;
    row[x] = value;
  }

  public BlueprintTerrain<T> Clone()
  {
    var clone = new BlueprintTerrain<T>
    {
      CenterPosition = CenterPosition,
      CenterRotation = CenterRotation,
      DistanceBetweenNodes = DistanceBetweenNodes,
      Smooth = Smooth,
      Width = Width,
      ValueCount = ValueCount
    };
    foreach (var row in Rows)
      clone.Rows.Add((T?[])row.Clone());
    return clone;
  }

  // Used when capturing: moves the terrain to be relative to the blueprint center.
  public void SetReference(Vector3 center, Quaternion rotation)
  {
    CenterPosition -= center;
    CenterRotation = Yaw(rotation);
  }

  public float GetSmoothWeight(Vector3 nodePosition, Vector3 placementPosition, Quaternion relativeRotation)
  {
    var smooth = Mathf.Clamp01(Smooth);
    if (smooth <= 0f) return 1f;
    var localPosition = Quaternion.Inverse(relativeRotation) * (nodePosition - placementPosition);
    var localX = localPosition.x - CenterPosition.x;
    var localZ = localPosition.z - CenterPosition.z;
    var radius = GetRadius();
    if (radius <= 0f) return 1f;
    var distance = Mathf.Sqrt(localX * localX + localZ * localZ);
    var innerRadius = radius * (1f - smooth);
    if (distance <= innerRadius) return 1f;
    if (distance >= radius) return 0f;
    var falloffRange = Mathf.Max(0.001f, radius - innerRadius);
    return Mathf.Clamp01(1f - (distance - innerRadius) / falloffRange);
  }

  public void AddRow(T?[] values)
  {
    if (values.Length == 0 || values.Length > MaxAxis)
      throw new FormatException($"Terrain row width must be between 1 and {MaxAxis}.");
    if (Rows.Count >= MaxAxis || (long)values.Length * (Rows.Count + 1) > MaxCells)
      throw new FormatException($"Terrain grid can contain at most {MaxCells} cells.");
    if (Width == 0)
      Width = values.Length;
    else if (values.Length != Width)
      throw new FormatException($"Terrain row width {values.Length} does not match the first row width {Width}.");
    Rows.Add(values);
    ValueCount += values.Count(value => value.HasValue);
  }

  public T? Get(int x, int z)
  {
    if (x < 0 || x >= Width || z < 0 || z >= Height) return null;
    return Rows[z][x];
  }

  public void MapValues(Func<T, T> map)
  {
    foreach (var row in Rows)
    {
      for (var i = 0; i < row.Length; ++i)
      {
        var value = row[i];
        if (value.HasValue)
          row[i] = map(value.Value);
      }
    }
  }

  public bool AllValues(Func<T, bool> predicate)
  {
    foreach (var row in Rows)
    {
      foreach (var value in row)
      {
        if (value.HasValue && !predicate(value.Value)) return false;
      }
    }
    return true;
  }

  public void Center(Vector3 center, Quaternion rotation)
  {
    var referenceRotation = CenterRotation;
    CenterPosition -= referenceRotation * center;
    CenterRotation = Yaw(referenceRotation * Quaternion.Inverse(rotation));
  }

  public float GetRadius()
  {
    if (Width == 0 || Height == 0) return 0f;
    var spacing = Mathf.Max(0.001f, DistanceBetweenNodes);
    var firstNode = CenterPosition - new Vector3((Width - 1) * spacing * 0.5f, 0f, (Height - 1) * spacing * 0.5f);
    var radius = 0f;
    for (var z = 0; z < Height; ++z)
    {
      for (var x = 0; x < Width; ++x)
      {
        if (!Get(x, z).HasValue) continue;
        var node = firstNode + new Vector3(x * spacing, 0f, z * spacing);
        radius = Mathf.Max(radius, Utils.LengthXZ(node));
      }
    }
    return radius;
  }

  public Quaternion GetRelativeRotation(Quaternion placementRotation) => Yaw(placementRotation * Quaternion.Inverse(CenterRotation));

  public T? FindNearest(Vector3 nodePosition, Vector3 placementPosition, Quaternion placementRotation)
    => FindNearestRelative(nodePosition, placementPosition, GetRelativeRotation(placementRotation));

  public T? FindNearestRelative(Vector3 nodePosition, Vector3 placementPosition, Quaternion relativeRotation)
  {
    if (Width == 0 || Height == 0) return null;
    var localPosition = Quaternion.Inverse(relativeRotation) * (nodePosition - placementPosition);
    var spacing = Mathf.Max(0.001f, DistanceBetweenNodes);
    var firstNode = CenterPosition - new Vector3((Width - 1) * spacing * 0.5f, 0f, (Height - 1) * spacing * 0.5f);
    var x = Mathf.RoundToInt((localPosition.x - firstNode.x) / spacing);
    var z = Mathf.RoundToInt((localPosition.z - firstNode.z) / spacing);
    return Get(x, z);
  }

  // Combines terrains placed at different positions into one terrain for the placement pos / rot. Later sources win.
  // shiftHeight moves values by the height difference between the source and the placement (heights only).
  // Returns null if there is nothing to merge or the result would be too large.
  public static BlueprintTerrain<T>? Merge(IReadOnlyList<(BlueprintTerrain<T> Terrain, Vector3 Pos, Quaternion Rot)> sources, Vector3 pos, Quaternion rot, Func<T, float, T>? shiftHeight)
  {
    var frame = Yaw(rot);
    var inverseFrame = Quaternion.Inverse(frame);
    var relatives = sources.Select(source => source.Terrain.GetRelativeRotation(source.Rot)).ToArray();
    var spacing = float.MaxValue;
    float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
    for (var i = 0; i < sources.Count; ++i)
    {
      var terrain = sources[i].Terrain;
      if (terrain.Width == 0 || terrain.Height == 0) continue;
      var step = Mathf.Max(0.001f, terrain.DistanceBetweenNodes);
      spacing = Mathf.Min(spacing, step);
      var first = terrain.CenterPosition - new Vector3((terrain.Width - 1) * step * 0.5f, 0f, (terrain.Height - 1) * step * 0.5f);
      foreach (var x in new[] { 0, terrain.Width - 1 })
      {
        foreach (var z in new[] { 0, terrain.Height - 1 })
        {
          var world = sources[i].Pos + relatives[i] * (first + new Vector3(x * step, 0f, z * step));
          var local = inverseFrame * (world - pos);
          minX = Mathf.Min(minX, local.x);
          maxX = Mathf.Max(maxX, local.x);
          minZ = Mathf.Min(minZ, local.z);
          maxZ = Mathf.Max(maxZ, local.z);
        }
      }
    }
    if (minX > maxX) return null;
    var width = Mathf.RoundToInt((maxX - minX) / spacing) + 1;
    var height = Mathf.RoundToInt((maxZ - minZ) / spacing) + 1;
    if (width > MaxAxis || height > MaxAxis || (long)width * height > MaxCells) return null;
    BlueprintTerrain<T> merged = new() { DistanceBetweenNodes = spacing };
    merged.Initialize(width, height, new Vector3(minX + (width - 1) * spacing * 0.5f, 0f, minZ + (height - 1) * spacing * 0.5f));
    for (var z = 0; z < height; ++z)
    {
      for (var x = 0; x < width; ++x)
      {
        var world = pos + frame * new Vector3(minX + x * spacing, 0f, minZ + z * spacing);
        for (var i = 0; i < sources.Count; ++i)
        {
          var value = sources[i].Terrain.FindNearestRelative(world, sources[i].Pos, relatives[i]);
          if (!value.HasValue) continue;
          merged.Set(x, z, shiftHeight == null ? value.Value : shiftHeight(value.Value, sources[i].Pos.y - pos.y));
        }
      }
    }
    return merged;
  }

  private static Quaternion Yaw(Quaternion rotation)
  {
    var forwardX = 2.0 * (rotation.x * rotation.z + rotation.w * rotation.y);
    var forwardZ = 1.0 - 2.0 * (rotation.x * rotation.x + rotation.y * rotation.y);
    if (forwardX * forwardX + forwardZ * forwardZ <= 0.0001) return Quaternion.identity;
    var halfYaw = Math.Atan2(forwardX, forwardZ) * 0.5;
    return new(0f, (float)Math.Sin(halfYaw), 0f, (float)Math.Cos(halfYaw));
  }
}

