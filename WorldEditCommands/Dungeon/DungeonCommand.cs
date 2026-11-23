using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands;

public class DungeonCommand
{
  public const string Name = "dungeon";


  public DungeonCommand()
  {
    ObjectAutoComplete autoComplete = new();
    Helper.Command(Name, "Modifies the nearest dungeon.", (args) =>
    {
      var player = Helper.GetPlayer();
      var dungeon = ZNetScene.instance.m_instances.Values.Where(view => view.GetComponent<DungeonGenerator>()).OrderBy(view => Vector3.Distance(player.transform.position, view.transform.position)).FirstOrDefault();
      if (dungeon == null)
      {
        Helper.AddError(args.Context, "No dungeon found.");
        return;
      }
      if (args.Length < 2) PrintRooms(dungeon);

    });
  }
  private static string PrintRooms(ZNetView obj)
  {
    List<string> info = [];
    var zdo = obj.GetZDO();
    var dg = obj.GetComponent<DungeonGenerator>();
    info.Add($"Dungeon {obj.GetPrefabName()} {dg.m_generatedSeed} {Helper.PrintVectorXZY(obj.transform.position)} {Helper.PrintAngleYXZ(obj.transform.rotation)}");
    var rooms = zdo.GetInt("rooms", 0);
    for (int i = 0; i < rooms; i++)
    {
      var id = "room" + i.ToString();
      var type = zdo.GetInt(id, 0);
      var pos = zdo.GetVec3(id + "_pos", Vector3.zero);
      var rot = zdo.GetQuaternion(id + "_rot", Quaternion.identity);
      info.Add($"Room {i}: {DungeonDB.instance.GetRoom(type).RoomInPrefab.name} {Helper.PrintVectorXZY(pos)} {Helper.PrintAngleYXZ(rot)}");
    }
    return string.Join(", ", info);
  }
}
