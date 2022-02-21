using System.Collections.Generic;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands {

  public class TerrainCommand {
    public TerrainCommand() {
      Operations.Sort();
      new TerrainAutoComplete();
      new Terminal.ConsoleCommand("terrain", "[raise/lower/reset/level/paint=value] [radius=0] [smooth=0] [blockcheck] [square] - Terrain manipulation.", delegate (Terminal.ConsoleEventArgs args) {
        if (Player.m_localPlayer == null) {
          Helper.AddMessage(args.Context, "Unable to find the player.");
          return;
        }
        var pos = Player.m_localPlayer.transform.position;
        var height = ZoneSystem.instance.GetGroundHeight(pos);
        var pars = new TerrainParameters();
        if (!pars.ParseArgs(args, args.Context, height)) return;
        var heightMaps = new List<Heightmap>();
        Heightmap.FindHeightmap(pos, pars.Radius, heightMaps);
        var compilerIndices = Terrain.GetCompilerIndices(heightMaps, pos, pars.Radius, pars.Square, pars.BlockCheck);
        var before = Terrain.GetData(compilerIndices);
        if (pars.Level.HasValue)
          Terrain.LevelTerrain(compilerIndices, pos, pars.Radius, pars.Smooth, pars.Level.Value);
        if (pars.Set.HasValue)
          Terrain.SetTerrain(compilerIndices, pos, pars.Radius, pars.Smooth, pars.Set.Value);
        if (pars.Delta.HasValue)
          Terrain.RaiseTerrain(compilerIndices, pos, pars.Radius, pars.Smooth, pars.Delta.Value);
        if (pars.Paint != "") {
          if (pars.Paint == "dirt")
            Terrain.PaintTerrain(compilerIndices, pos, pars.Radius, Color.red);
          if (pars.Paint == "paved")
            Terrain.PaintTerrain(compilerIndices, pos, pars.Radius, Color.blue);
          if (pars.Paint == "cultivated")
            Terrain.PaintTerrain(compilerIndices, pos, pars.Radius, Color.green);
          if (pars.Paint == "grass")
            Terrain.PaintTerrain(compilerIndices, pos, pars.Radius, Color.black);
        }

        var after = Terrain.GetData(compilerIndices);
        UndoManager.Add(new UndoTerrain(before, after, pos, pars.Radius));

      }, true, true, optionsFetcher: () => Operations);
    }

    public static List<string> Operations = new List<string>(){
      "lower",
      "level",
      "raise",
      "reset",
      "paint",
      "blockcheck",
      "square",
      "radius",
      "smooth"
    };

  }

}
