using System;
using System.Collections.Generic;
using UnityEngine;
namespace WorldEditCommands;

using Operation = Action<TerrainComp, int, TerrainNode>;
public partial class Terrain
{
  private static float CalculateSmooth(float smooth, float distance) => (1f - distance) >= smooth ? 1f : (1f - distance) / smooth;
  private static float CalculateSlope(float angle, float distanceWidth, float distanceDepth) => Mathf.Sin(angle) * distanceWidth + Mathf.Cos(angle) * distanceDepth;
  public static void SetTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float smooth, float delta)
  {
    void action(TerrainComp compiler, int index, TerrainNode node)
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] = delta * multiplier;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    }
    DoOperation(nodes, pos, radius, action);
  }
  public static void RaiseTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float smooth, float amount)
  {
    void action(TerrainComp compiler, int index, TerrainNode node)
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] += multiplier * amount + compiler.m_smoothDelta[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    }
    DoOperation(nodes, pos, radius, action);
  }
  public static void VoidTerrain(List<HeightNode> nodes, Vector3 pos, float radius)
  {
    static void action(TerrainComp compiler, int index, TerrainNode node)
    {
      compiler.m_levelDelta[index] = float.NaN;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = true;
    }
    DoOperation(nodes, pos, radius, action);
  }
  public static void LevelTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float smooth, float altitude)
  {
    void action(TerrainComp compiler, int index, TerrainNode node)
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] += multiplier * (altitude - compiler.m_hmap.m_heights[index]);
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    }
    DoOperation(nodes, pos, radius, action);
  }

  public static void MaxTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float altitude)
  {
    void action(TerrainComp compiler, int index, TerrainNode node)
    {
      var capped = Mathf.Min(altitude, compiler.m_hmap.m_heights[index]);
      compiler.m_levelDelta[index] += capped - compiler.m_hmap.m_heights[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    }
    DoOperation(nodes, pos, radius, action);
  }
  public static void MinTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float altitude)
  {
    void action(TerrainComp compiler, int index, TerrainNode node)
    {
      var capped = Mathf.Max(altitude, compiler.m_hmap.m_heights[index]);
      compiler.m_levelDelta[index] += capped - compiler.m_hmap.m_heights[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    }
    DoOperation(nodes, pos, radius, action);
  }
  public static void SlopeTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float angle, float smooth, float altitude, float amount)
  {
    void action(TerrainComp compiler, int index, TerrainNode node)
    {
      var multiplier = CalculateSlope(angle, node.DistanceWidth, node.DistanceDepth) * CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] += altitude - compiler.m_hmap.m_heights[index] + multiplier * amount / 2f;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    }
    DoOperation(nodes, pos, radius, action);
  }
  public static void PaintTerrain(List<PaintNode> nodes, Vector3 pos, float radius, float smooth, Color color)
  {
    void action(TerrainComp compiler, int index, TerrainNode node)
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      var sourceColor = compiler.m_paintMask[index];
      var hm = compiler.m_hmap;
      // Lava is implemented with alpha 1, which unfortunately is same alpha as on regular terrain.
      // 1. Unmodified state has alpha 1 which doesn't match how Ashlands biome works (unmodified state is considered as alpha 0).
      // 2. Regular paint colors would apply lava as they have alpha 1.

      // This should be most compatible way with custom biomes.
      // IsLava always returns false for non-lava biomes, regarless of the value.
      var isAshlands = hm != null && hm.IsLava(pos, -1f);
      // Fix for issue 1.
      if (isAshlands && !compiler.m_modifiedPaint[index])
        sourceColor.a = 0f;

      var targetColor = color;
      // Support for only changing specific channel.
      if (float.IsNaN(color.r)) targetColor.r = sourceColor.r;
      if (float.IsNaN(color.g)) targetColor.g = sourceColor.g;
      if (float.IsNaN(color.b)) targetColor.b = sourceColor.b;

      if (float.IsNaN(color.a))
      {
        targetColor.a = sourceColor.a;
      }
      else if (color.a >= 0f)
      {
        // Fix for issue 2.
        if (isAshlands)
          targetColor.a = 1 - color.a;
        else
          targetColor.a = color.a;
      }
      else
      {
        // Negative values skip biome checks for precise control.
        targetColor.a = -color.a;
      }

      compiler.m_paintMask[index] = Color.Lerp(sourceColor, targetColor, multiplier);
      compiler.m_modifiedPaint[index] = true;
    }
    DoOperation(nodes, pos, radius, action);
  }
  public static void ClearPaint(List<PaintNode> nodes, Vector3 pos, float radius)
  {
    static void action(TerrainComp compiler, int index, TerrainNode node)
    {
      compiler.m_modifiedPaint[index] = false;
    }
    DoOperation(nodes, pos, radius, action);
  }
  public static void ResetTerrain(List<HeightNode> heightNodes, List<PaintNode> paintNodes, Vector3 pos, float radius)
  {
    List<TerrainModifier> modifiers = [];
    TerrainModifier.GetModifiers(pos, radius + 1f, modifiers);
    foreach (TerrainModifier modifier in modifiers)
    {
      if (modifier.m_nview == null) continue;
      modifier.m_nview.ClaimOwnership();
      ZNetScene.instance.Destroy(modifier.gameObject);
    }
    static void action(TerrainComp compiler, int index, TerrainNode node)
    {
      compiler.m_levelDelta[index] = 0f;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = false;
    }
    DoOperation(heightNodes, pos, radius, action);
    ClearPaint(paintNodes, pos, radius);
  }
  private static void DoOperation(List<PaintNode> nodes, Vector3 pos, float radius, Operation action)
  {
    foreach (var node in nodes)
      action(node.Compiler!, node.Index, node);
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
  private static void DoOperation(List<HeightNode> nodes, Vector3 pos, float radius, Operation action)
  {
    foreach (var node in nodes)
      action(node.Compiler!, node.Index, node);
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
}
