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
    Operation action = (compiler, index, node) =>
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] = delta * multiplier;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void RaiseTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float smooth, float amount)
  {
    Operation action = (compiler, index, node) =>
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] += multiplier * amount + compiler.m_smoothDelta[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void VoidTerrain(List<HeightNode> nodes, Vector3 pos, float radius)
  {
    Operation action = (compiler, index, node) =>
    {
      compiler.m_levelDelta[index] = float.NaN;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = true;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void LevelTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float smooth, float altitude)
  {
    Operation action = (compiler, index, node) =>
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] += multiplier * (altitude - compiler.m_hmap.m_heights[index]);
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoOperation(nodes, pos, radius, action);
  }

  public static void MaxTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float altitude)
  {
    Operation action = (compiler, index, node) =>
    {
      var capped = Mathf.Min(altitude, compiler.m_hmap.m_heights[index]);
      compiler.m_levelDelta[index] += capped - compiler.m_hmap.m_heights[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void MinTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float altitude)
  {
    Operation action = (compiler, index, node) =>
    {
      var capped = Mathf.Max(altitude, compiler.m_hmap.m_heights[index]);
      compiler.m_levelDelta[index] += capped - compiler.m_hmap.m_heights[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void SlopeTerrain(List<HeightNode> nodes, Vector3 pos, float radius, float angle, float smooth, float altitude, float amount)
  {
    Operation action = (compiler, index, node) =>
    {
      var multiplier = CalculateSlope(angle, node.DistanceWidth, node.DistanceDepth) * CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] += (altitude - compiler.m_hmap.m_heights[index]) + multiplier * amount / 2f;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void PaintTerrain(List<PaintNode> nodes, Vector3 pos, float radius, float smooth, Color color)
  {
    Operation action = (compiler, index, node) =>
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      var newColor = Color.Lerp(compiler.m_paintMask[index], color, multiplier);
      newColor.a = color.a;
      compiler.m_paintMask[index] = newColor;
      compiler.m_modifiedPaint[index] = true;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void ClearPaint(List<PaintNode> nodes, Vector3 pos, float radius)
  {
    Operation action = (compiler, index, node) =>
    {
      compiler.m_modifiedPaint[index] = false;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void ResetTerrain(List<HeightNode> heightNodes, List<PaintNode> paintNodes, Vector3 pos, float radius)
  {
    List<TerrainModifier> modifiers = new List<TerrainModifier>();
    TerrainModifier.GetModifiers(pos, radius + 1f, modifiers);
    foreach (TerrainModifier modifier in modifiers)
    {
      if (modifier.m_nview == null) continue;
      modifier.m_nview.ClaimOwnership();
      ZNetScene.instance.Destroy(modifier.gameObject);
    }
    Operation action = (compiler, index, node) =>
    {
      compiler.m_levelDelta[index] = 0f;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = false;
    };
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
