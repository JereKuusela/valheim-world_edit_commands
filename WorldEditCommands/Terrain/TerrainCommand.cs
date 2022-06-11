using System.Collections.Generic;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class TerrainCommand {
  public const string Name = "terrain";
  public TerrainCommand() {
    TerrainAutoComplete autoComplete = new();
    var description = CommandInfo.Create("Manipulates the terrain.", null, autoComplete.NamedParameters);
    new Terminal.ConsoleCommand(Name, description, (args) => {
      var player = Player.m_localPlayer;
      if (!player) {
        Helper.AddMessage(args.Context, "Unable to find the player.");
        return;
      }
      var precision = Mathf.PI / 4f;
      var angle = precision * Mathf.Round(player.transform.rotation.eulerAngles.y / 45f);
      TerrainParameters pars = new() { Position = player.transform.position, Angle = angle };
      if (!pars.ParseArgs(args, args.Context)) return;
      if (pars.Guide) {
        TerrainRuler.Create(pars);
        return;
      }
      Dictionary<TerrainComp, Indices> compilerIndices = new();
      if (pars.Diameter.HasValue)
        compilerIndices = Terrain.GetCompilerIndicesWithCircle(pars.Position, pars.Diameter.Value, pars.BlockCheck);
      if (pars.Width.HasValue && pars.Depth.HasValue)
        compilerIndices = Terrain.GetCompilerIndicesWithRect(pars.Position, pars.Width.Value, pars.Depth.Value, pars.Angle, pars.BlockCheck);
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

    }, true, true, optionsFetcher: () => autoComplete.NamedParameters);
  }
}
