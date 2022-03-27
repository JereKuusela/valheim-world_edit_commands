using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands {

  public class TerrainCommand {
    public TerrainCommand() {
      var autoComplete = new TerrainAutoComplete();
      var description = CommandInfo.Create("Manipulates the terrain.", null, autoComplete.NamedParameters);
      new Terminal.ConsoleCommand("terrain", description, delegate (Terminal.ConsoleEventArgs args) {
        if (Player.m_localPlayer == null) {
          Helper.AddMessage(args.Context, "Unable to find the player.");
          return;
        }
        var pars = new TerrainParameters { Position = Player.m_localPlayer.transform.position };
        if (!pars.ParseArgs(args, args.Context)) return;
        var compilerIndices = pars.Diameter.HasValue ?
          Terrain.GetCompilerIndicesWithCircle(pars.Position, pars.Diameter.Value, pars.BlockCheck) :
          Terrain.GetCompilerIndicesWithRect(pars.Position, pars.Width.Value, pars.Depth.Value, pars.Angle, pars.BlockCheck);
        var before = Terrain.GetData(compilerIndices);
        if (pars.Level.HasValue)
          Terrain.LevelTerrain(compilerIndices, pars.Position, pars.Size, pars.Smooth, pars.Level.Value);
        if (pars.Set.HasValue)
          Terrain.SetTerrain(compilerIndices, pars.Position, pars.Size, pars.Smooth, pars.Set.Value);
        if (pars.Slope.HasValue)
          Terrain.SlopeTerrain(compilerIndices, pars.Position, pars.Size, pars.SlopeAngle, pars.Smooth, pars.Position.y, pars.Slope.Value);
        if (pars.Delta.HasValue)
          Terrain.RaiseTerrain(compilerIndices, pars.Position, pars.Size, pars.Smooth, pars.Delta.Value);
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
        UndoManager.Add(new UndoTerrain(before, after, pars.Position, pars.Size));

      }, true, true, optionsFetcher: () => autoComplete.NamedParameters);
    }
  }
}
