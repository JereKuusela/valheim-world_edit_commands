// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all.
using System;
using System.Globalization;
using UnityEngine;

namespace Data;

public partial class Blueprints
{
  private static BlueprintTerrain<T> GetPlanBuildTerrain<T>(string row, int separator) where T : struct
  {
    if (separator < 0) throw new FormatException("Terrain section requires center, rotation and node spacing.");
    var split = row.Substring(separator + 1).Split(';');
    if (split.Length < 3) throw new FormatException("Terrain section requires center, rotation and node spacing.");
    var center = split[0].Split(',');
    if (center.Length != 3) throw new FormatException("Terrain center requires X, Z and Y coordinates.");
    var centerX = RequiredInvariantFloat(center, 0);
    var centerZ = RequiredInvariantFloat(center, 1);
    var centerY = RequiredInvariantFloat(center, 2);
    var yaw = RequiredInvariantFloat(split, 1);
    var spacing = RequiredInvariantFloat(split, 2);
    if (!IsFinite(centerX) || !IsFinite(centerY) || !IsFinite(centerZ) || !IsFinite(yaw))
      throw new FormatException("Terrain center and rotation must contain finite numbers.");
    if (!IsFinite(spacing) || spacing <= 0f)
      throw new FormatException("Terrain node spacing must be a finite positive number.");
    return new()
    {
      CenterPosition = new(centerX, centerY, centerZ),
      CenterRotation = PlanBuildYaw(yaw),
      DistanceBetweenNodes = spacing
    };
  }

  private static Quaternion PlanBuildYaw(float degrees)
  {
    var radians = degrees * Math.PI / 180.0 * 0.5;
    return new(0f, (float)Math.Sin(radians), 0f, (float)Math.Cos(radians));
  }

  private static float?[] GetPlanBuildHeightRow(string row)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var values = new float?[split.Length];
    for (var i = 0; i < split.Length; ++i)
    {
      if (!string.IsNullOrEmpty(split[i]))
      {
        var value = InvariantFloat(split, i);
        if (!IsFinite(value)) throw new FormatException("Terrain height values must be finite numbers.");
        values[i] = value;
      }
    }
    return values;
  }

  private static Color?[] GetPlanBuildPaintRow(string row)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var values = new Color?[split.Length];
    for (var i = 0; i < split.Length; ++i)
    {
      if (string.IsNullOrEmpty(split[i])) continue;
      var color = split[i].Split(':');
      if (color.Length < 3) throw new FormatException($"Terrain paint value '{split[i]}' requires r:g:b[:a].");
      var r = RequiredInvariantFloat(color, 0);
      var g = RequiredInvariantFloat(color, 1);
      var b = RequiredInvariantFloat(color, 2);
      var a = color.Length > 3 ? RequiredInvariantFloat(color, 3) : 1f;
      if (!IsFinite(r) || !IsFinite(g) || !IsFinite(b) || !IsFinite(a))
        throw new FormatException("Terrain paint values must be finite numbers.");
      values[i] = new(r, g, b, a);
    }
    return values;
  }
}
