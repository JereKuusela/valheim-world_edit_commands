using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;

public class TerrainCommand
{
  public const string Name = "terrain";
  public static Dictionary<string, Color> Paints = new() {
    {"grass", Color.black},
    {"patches", new(0f, 0.75f, 0f)},
    {"grass_dark", new(0.6f, 0.5f, 0f)},
    {"dirt", Color.red},
    {"cultivated", Color.green},
    {"paved", Color.blue},
    {"paved_moss", new(0f, 0f, 0.5f)},
    {"paved_dirt", new(1f, 0f, 0.5f)},
    {"paved_dark", new(0f, 1f, 0.5f)},
    {"lava", new(float.NaN, float.NaN, float.NaN, -1f)},
    {"lava_shallow", new(float.NaN, float.NaN, float.NaN, -0.5f)},
    {"lava_light", new(float.NaN, float.NaN, float.NaN, -0.25f)},
  };

  private TerrainComp[] GetCompilers(TerrainParameters pars)
  {
    if (pars.Paths != null) return Terrain.GetCompilers(pars.Paths, pars.PathRadius!.Max);
    if (pars.Radius != null) return Terrain.GetCompilers(pars.Position, pars.Radius);
    if (pars.Width != null && pars.Depth != null) return Terrain.GetCompilers(pars.Position, pars.Width, pars.Depth, pars.Angle);
    throw new InvalidOperationException("Unable to select any terrain");
  }
  private List<T> GetNodes<T>(TerrainParameters pars, IEnumerable<TerrainComp> compilers, List<Func<TerrainNode, bool>> filterers) where T : TerrainNode, new()
  {
    List<T> nodes = [];
    foreach (var comp in compilers)
    {
      if (pars.Paths != null) Terrain.GetNodesWithPath(nodes, comp, pars.Paths, pars.PathRadius!, pars.Radius == null);
      else if (pars.Radius != null) Terrain.GetNodesWithCircle(nodes, comp, pars.Position, pars.Radius);
      else if (pars.Width != null && pars.Depth != null) Terrain.GetNodesWithRect(nodes, comp, pars.Position, pars.Width, pars.Depth, pars.Angle);
    }
    return [.. nodes.Where(n => (pars.Chance >= 1f || UnityEngine.Random.value < pars.Chance) && filterers.All(f => f(n)))];
  }
  private List<Func<TerrainNode, bool>> GetFilterers(TerrainParameters pars)
  {
    List<Func<TerrainNode, bool>> filterers = [];
    if (pars.BlockCheck != BlockCheck.Off) filterers.Add(Terrain.CreateBlockCheckFilter(pars.BlockCheck, pars.IncludedIds, pars.ExcludedIds));
    if (pars.Within != null) filterers.Add(Terrain.CreateHeightFilter(pars.Within.Min, pars.Within.Max));
    return filterers;
  }
  public TerrainCommand()
  {
    TerrainAutoComplete autoComplete = new();
    Helper.Command(Name, "Manipulates the terrain.", (args) =>
    {
      TerrainParameters pars = new(args);
      var compilers = GetCompilers(pars);
      var filterers = GetFilterers(pars);
      UndoHelper.Run(() => Execute(pars, compilers, filterers));
    });
  }
  private void Execute(TerrainParameters pars, TerrainComp[] compilers, List<Func<TerrainNode, bool>> filterers)
  {
    var heightNodes = GetNodes<HeightNode>(pars, compilers, filterers);
    var paintNodes = GetNodes<PaintNode>(pars, compilers, filterers);
    var before = Terrain.GetData(heightNodes, paintNodes);
    if (pars.Reset)
    {
      var paths = pars.Paths;
      var maxDistance = pars.PathRadius?.Max + 1f;
      Func<Vector3, bool>? filter = paths == null ? null : pos => TerrainPath.Distance(paths, pos) <= maxDistance;
      Terrain.ResetTerrain(heightNodes, paintNodes, pars.Position, pars.Size, filter);
    }
    if (pars.Set.HasValue)
      Terrain.SetTerrain(heightNodes, pars.Position, pars.Size, pars.Smooth, pars.Set.Value);
    // Level would override the slope which can lead to weird results when operating near the dig limit.
    if (pars.Slope.HasValue && !pars.Level.HasValue)
      Terrain.SlopeTerrain(heightNodes, pars.Position, pars.Size, pars.SlopeAngle, pars.Smooth, pars.Position.y, pars.Slope.Value);
    if (pars.Level.HasValue && pars.Paths != null && pars.LevelToPath)
      Terrain.LevelTerrainToPath(heightNodes, pars.Position, pars.Size, pars.Smooth);
    else if (pars.Level.HasValue)
      Terrain.LevelTerrain(heightNodes, pars.Position, pars.Size, pars.Smooth, pars.Level.Value);
    if (pars.Delta.HasValue)
      Terrain.RaiseTerrain(heightNodes, pars.Position, pars.Size, pars.Smooth, pars.Delta.Value);
    if (pars.Min.HasValue)
      Terrain.MinTerrain(heightNodes, pars.Position, pars.Size, pars.Min.Value);
    if (pars.Max.HasValue)
      Terrain.MaxTerrain(heightNodes, pars.Position, pars.Size, pars.Max.Value);
    if (pars.Void)
      Terrain.VoidTerrain(heightNodes, pars.Position, pars.Size);
    if (pars.Paint != "")
    {
      var split = pars.Paint.Split(',');
      if (split.Length > 2)
      {
        for (var i = 0; i < split.Length; i++)
        {
          if (split[i].StartsWith("*"))
            split[i] = "NaN";
        }
        Color color = new(Parse.Float(split, 0), Parse.Float(split, 1), Parse.Float(split, 2), Parse.Float(split, 3, 1f));
        Terrain.PaintTerrain(paintNodes, pars.Position, pars.Size, pars.Smooth, color);
      }
      else if (Paints.TryGetValue(pars.Paint, out var color))
      {
        Terrain.PaintTerrain(paintNodes, pars.Position, pars.Size, pars.Smooth, color);
      }
    }
    foreach (var compiler in compilers)
      Terrain.Save(compiler);
    var after = Terrain.GetData(heightNodes, paintNodes);
    UndoHelper.AddTerrainAction(before, after, pars.Position, pars.Size);
  }
}
