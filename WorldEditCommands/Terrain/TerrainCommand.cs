using System;
using System.Collections.Generic;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class TerrainCommand {
  public const string Name = "terrain";

  private TerrainComp[] GetCompilers(TerrainParameters pars) {
    if (pars.Diameter.HasValue) return Terrain.GetCompilers(pars.Position, pars.Diameter.Value / 2f);
    if (pars.Width.HasValue && pars.Depth.HasValue) return Terrain.GetCompilers(pars.Position, pars.Width.Value, pars.Depth.Value, pars.Angle);
    throw new InvalidOperationException("Unable to select any terrain");
  }
  private Func<TerrainComp, Indices> GetIndexer(TerrainParameters pars) {
    if (pars.Diameter.HasValue) return Terrain.CreateIndexer(pars.Position, pars.Diameter.Value);
    if (pars.Width.HasValue && pars.Depth.HasValue) return Terrain.CreateIndexer(pars.Position, pars.Width.Value, pars.Depth.Value, pars.Angle);
    throw new InvalidOperationException("Unable to select any terrain");
  }
  private List<Func<BaseIndex, bool>> GetFilterers(TerrainParameters pars) {
    List<Func<BaseIndex, bool>> filterers = new();
    if (pars.BlockCheck != BlockCheck.Off) filterers.Add(Terrain.CreateBlockCheckFilter(pars.BlockCheck));
    if (pars.Within != null) filterers.Add(Terrain.CreateAltitudeFilter(pars.Within.Min, pars.Within.Max));
    return filterers;
  }
  public TerrainCommand() {
    TerrainAutoComplete autoComplete = new();
    var description = CommandInfo.Create("Manipulates the terrain.", null, autoComplete.NamedParameters);
    Helper.Command(Name, description, (args) => {
      TerrainParameters pars = new(args);
      if (pars.Guide) {
        Ruler.Create(pars.ToRuler());
        return;
      }
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
      if (pars.Paint != "") {
        if (pars.Paint == "dirt")
          Terrain.PaintTerrain(compilerIndices, pars.Position, pars.Size, Color.red);
        if (pars.Paint == "paved")
          Terrain.PaintTerrain(compilerIndices, pars.Position, pars.Size, Color.blue);
        if (pars.Paint == "cultivated")
          Terrain.PaintTerrain(compilerIndices, pars.Position, pars.Size, Color.green);
        if (pars.Paint == "grass")
          Terrain.PaintTerrain(compilerIndices, pars.Position, pars.Size, Color.black);
      }

      var after = Terrain.GetData(compilerIndices);
      UndoTerrain undo = new(before, after, pars.Position, pars.Size);
      UndoManager.Add(undo);

    }, () => autoComplete.NamedParameters);
  }
}
