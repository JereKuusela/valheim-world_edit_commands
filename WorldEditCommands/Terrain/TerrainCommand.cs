using System;
using System.Collections.Generic;
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
  };

  private TerrainComp[] GetCompilers(TerrainParameters pars)
  {
    if (pars.Radius != null) return Terrain.GetCompilers(pars.Position, pars.Radius);
    if (pars.Width != null && pars.Depth != null) return Terrain.GetCompilers(pars.Position, pars.Width, pars.Depth, pars.Angle);
    throw new InvalidOperationException("Unable to select any terrain");
  }
  private Func<TerrainComp, Indices> GetIndexer(TerrainParameters pars)
  {
    if (pars.Radius != null) return Terrain.CreateIndexer(pars.Position, pars.Radius);
    if (pars.Width != null && pars.Depth != null) return Terrain.CreateIndexer(pars.Position, pars.Width, pars.Depth, pars.Angle);
    throw new InvalidOperationException("Unable to select any terrain");
  }
  private List<Func<BaseIndex, bool>> GetFilterers(TerrainParameters pars)
  {
    List<Func<BaseIndex, bool>> filterers = new();
    if (pars.BlockCheck != BlockCheck.Off) filterers.Add(Terrain.CreateBlockCheckFilter(pars.BlockCheck));
    if (pars.Within != null) filterers.Add(Terrain.CreateAltitudeFilter(pars.Within.Min, pars.Within.Max));
    return filterers;
  }
  public TerrainCommand()
  {
    TerrainAutoComplete autoComplete = new();
    var description = CommandInfo.Create("Manipulates the terrain.", null, autoComplete.NamedParameters);
    Helper.Command(Name, description, (args) =>
    {
      TerrainParameters pars = new(args);
      var compilers = GetCompilers(pars);
      var indicer = GetIndexer(pars);
      var filterers = GetFilterers(pars);
      var compilerIndices = Terrain.GetIndices(compilers, indicer, filterers);
      var before = Terrain.GetData(compilerIndices);
      if (pars.Reset)
        Terrain.ResetTerrain(compilerIndices, pars.Position, pars.Size);
      if (pars.Set.HasValue)
        Terrain.SetTerrain(compilerIndices, pars.Position, pars.Size, pars.Smooth, pars.Set.Value);
      // Level would override the slope which can lead to weird results when operating near the dig limit.
      if (pars.Slope.HasValue && !pars.Level.HasValue)
        Terrain.SlopeTerrain(compilerIndices, pars.Position, pars.Size, pars.SlopeAngle, pars.Smooth, pars.Position.y, pars.Slope.Value);
      if (pars.Level.HasValue)
        Terrain.LevelTerrain(compilerIndices, pars.Position, pars.Size, pars.Smooth, pars.Level.Value);
      if (pars.Delta.HasValue)
        Terrain.RaiseTerrain(compilerIndices, pars.Position, pars.Size, pars.Smooth, pars.Delta.Value);
      if (pars.Min.HasValue)
        Terrain.MinTerrain(compilerIndices, pars.Position, pars.Size, pars.Min.Value);
      if (pars.Max.HasValue)
        Terrain.MaxTerrain(compilerIndices, pars.Position, pars.Size, pars.Max.Value);
      if (pars.Paint != "")
      {
        var split = pars.Paint.Split(',');
        if (split.Length > 2)
        {
          Color color = new(Parse.Float(split, 0), Parse.Float(split, 1), Parse.Float(split, 2), Parse.Float(split, 3, 1f));
          Terrain.PaintTerrain(compilerIndices, pars.Position, pars.Size, color);
        }
        else if (Paints.TryGetValue(pars.Paint, out var color))
        {
          Terrain.PaintTerrain(compilerIndices, pars.Position, pars.Size, color);
        }
      }

      var after = Terrain.GetData(compilerIndices);
      UndoTerrain undo = new(before, after, pars.Position, pars.Size);
      UndoManager.Add(undo);

    }, () => autoComplete.NamedParameters);
  }
}
