using System;
using System.Collections.Generic;
using UnityEngine;
namespace WorldEditCommands;
using CompilerIndices = Dictionary<TerrainComp, Indices>;

public partial class Terrain
{
  private static float CalculateSmooth(float smooth, float distance) => (1f - distance) >= smooth ? 1f : (1f - distance) / smooth;
  private static float CalculateSlope(float angle, float distanceWidth, float distanceDepth) => Mathf.Sin(angle) * distanceWidth + Mathf.Cos(angle) * distanceDepth;
  public static void SetTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float smooth, float delta)
  {
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var multiplier = CalculateSmooth(smooth, heightIndex.Distance);
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] = delta * multiplier;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
  }
  public static void RaiseTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float smooth, float amount)
  {
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var multiplier = CalculateSmooth(smooth, heightIndex.Distance);
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] += multiplier * amount + compiler.m_smoothDelta[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
  }
  public static void VoidTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius)
  {
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] = float.NaN;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = true;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
  }
  public static void LevelTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float smooth, float altitude)
  {
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var multiplier = CalculateSmooth(smooth, heightIndex.Distance);
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] += multiplier * (altitude - compiler.m_hmap.m_heights[index]);
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
  }

  public static void MaxTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float altitude)
  {
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var index = heightIndex.Index;
      var capped = Mathf.Min(altitude, compiler.m_hmap.m_heights[index]);
      compiler.m_levelDelta[index] += capped - compiler.m_hmap.m_heights[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
  }
  public static void MinTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float altitude)
  {
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var index = heightIndex.Index;
      var capped = Mathf.Max(altitude, compiler.m_hmap.m_heights[index]);
      compiler.m_levelDelta[index] += capped - compiler.m_hmap.m_heights[index];
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
  }
  public static void SlopeTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float angle, float smooth, float altitude, float amount)
  {
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var multiplier = CalculateSlope(angle, heightIndex.DistanceWidth, heightIndex.DistanceDepth) * CalculateSmooth(smooth, heightIndex.Distance);
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] += (altitude - compiler.m_hmap.m_heights[index]) + multiplier * amount / 2f;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
  }
  public static void PaintTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, Color color)
  {
    Action<TerrainComp, int> action = (compiler, index) =>
    {
      compiler.m_paintMask[index] = color;
      compiler.m_modifiedPaint[index] = true;
    };
    DoPaintOperation(compilerIndices, pos, radius, action);
  }
  public static void ClearPaint(CompilerIndices compilerIndices, Vector3 pos, float radius)
  {
    Action<TerrainComp, int> action = (compiler, index) =>
    {
      compiler.m_modifiedPaint[index] = false;
    };
    DoPaintOperation(compilerIndices, pos, radius, action);
  }
  public static void ResetTerrain(Dictionary<TerrainComp, Indices> compilerIndices, Vector3 pos, float radius)
  {
    List<TerrainModifier> modifiers = new List<TerrainModifier>();
    TerrainModifier.GetModifiers(pos, radius + 1f, modifiers);
    foreach (TerrainModifier modifier in modifiers)
    {
      if (modifier.m_nview == null) continue;
      modifier.m_nview.ClaimOwnership();
      ZNetScene.instance.Destroy(modifier.gameObject);
    }
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] = 0f;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = false;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
    ClearPaint(compilerIndices, pos, radius);
  }
  private static void DoHeightOperation(CompilerIndices compilerIndices, Vector3 pos, float radius, Action<TerrainComp, HeightIndex> action)
  {
    foreach (var kvp in compilerIndices)
    {
      var compiler = kvp.Key;
      var indices = kvp.Value.HeightIndices;
      foreach (var heightIndex in indices) action(compiler, heightIndex);
      Save(compiler);
    }
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
  private static void DoPaintOperation(CompilerIndices compilerIndices, Vector3 pos, float radius, Action<TerrainComp, int> action)
  {
    foreach (var kvp in compilerIndices)
    {
      var compiler = kvp.Key;
      var indices = kvp.Value.PaintIndices;
      foreach (var index in indices) action(compiler, index.Index);
      Save(compiler);
    }
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
}
