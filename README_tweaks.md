# World Edit Commands Spawner Tweaks / Structure Tweaks

Some new commands are added if these mods are installed. They all start with the `tweak_` prefix. All values can be reseted by providing no value to the parameter.

Most commands can be used on any object. When editing a single object, the script component is automatically added if missing. When editing multiple objects, `force` parameter needs to be used to add missing components.

## Structure Tweaks

`tweak_object`:

- `component=name`: Adds behavior / script component.
- `creator=player id`: Sets the piece creator. Use 0 for no creator.
- `effect=radius,id1,id2,...,playeronly`: Adds forced effect area (heat, fire, player base, burning, no monsters, warm cozy area). Putting 1 to playeronly makes the effect only affect players.
- `event=radius,id`: Adds forced random event area.
- `collision=true/false`: Sets whether the object has collision. No value to toggle.
- `destoy=seconds`: Adds timed destroy after given seconds.
- `fall=off/solid/terrain`: Overrides the fall behavior.
- `growth=big/big_bad/small/small_bad`: Overrides the plant visual wear (and prevents growth).
- `interact=true/false`: Sets whether the object can be interacted with. No value to toggle.
- `show=true/false`: Sets whether the object is visible. No value to toggle.
- `status=radius,id,playeronly`: Adds forced status effect area. Putting 1 to playeronly makes the effect only affect players.
- `wear=broken/damaged/healthy`: Overrides the object visual wear.
- `weather=radius,id,instant,dungeon`: Adds forced weather area. If third parameter is given, the weather changes instantly. Fourth parameter can be used to change dungeon weather.
  - `weather=5,AshRain,true` adds a 5 meter radius Ashrain area that changes the weather instantly.
  - `weather=0,,,AshRain` changes the dungeon weather to Ashrain when used on the entrance.
- `water=name,x,z,y`: Adds a water surface.

`tweak_chest`:

- `name=text`: Display name. Put " around the value if you use spaces.
- `unlock=true/false`: Allows ignoring wards. No value to toggle.

Note: Spawner Tweaks adds more parameters for `tweak_chest`.

`tweak_door`:

- `consume=true/false`: Consumes the key item on use. No value to toggle.
- `closeeffect=id,flag`: Effects when closing. For random rotation, use 1 as the flag.
- `key=item`: Required key item to open the door. If set, the door can't be closed.
- `lockedeffect=id,flag`: Effects when locked. For random rotation, use 1 as the flag.
- `noclose=true/false`: If set, the door can't be closed. No value to toggle.
- `openeffect=id,flag`: Effects when opening. For random rotation, use 1 as the flag.
- `unlock=true/false`: Allows ignoring wards. No value to toggle.

`tweak_dungeon`:

- `enter_hover="text"`: Changes the hover text of the dungeon entrance. Put " around the value if you use spaces.
- `enter_text="text"`: Changes the text after entering the dungeon. Put " around the value if you use spaces.
- `exit_hover="text"`: Changes the hover text of the dungeon exut. Put " around the value if you use spaces.
- `exit_text="text"`: Changes the text after exiting the dungeon. Put " around the value if you use spaces.
- `weather=id`: Replaced the dungeon environment.

`tweak_fireplace`:

- `smoke=off/on/ignore`: Smoke behavior.

`tweak_portal`:

- `restrict=false`: Allows teleporting with any items.

`tweak_runestone`:

- `discover=id,pintext,pintype,showmap`: Location discovery. If showmap=1, then opens the map automatically.
  - This works for vegvisirs in dungeons too.
- `name="text"`: Display name. Put " around the value if you use spaces.
- `text="text"`: Use text. Put " around the value if you use spaces.
- `topic="text"`: Topic shown with the text. Put " around the value if you use spaces.
- `compendium="text"`: Label added to the compendium. Put " around the value if you use spaces. Start with - to remove the label.

### Examples: Structure Tweaks

- `tweak_runestone compendium="Hello world!" compendium=-Hi text=Stuff`: Adds Hello world! entry to the compendium while removing the Hi entry.

## Spawner Tweaks

Four new commands that share many parameters with the `object` command.

`tweak_altar`:

- `amount=number`: Amount of needed items to interact.
- `command="text"`: Command to run when interacted. Put " around the value if you use spaces. Use following special values to get altar coordinates:
  - $$x: x coordinate
  - $$y: y coordinate
  - $$z: z coordinate
  - $$i: zone x
  - $$j: zone y
  - $$a: angle
- `delay=seconds`: Duration of the spawning.
- `faction=text`: Determines which creatures are considered hostile.
- `itemstandprefix=text`: Prefix for included item stands.
- `itemstandrange=meters`: Radius for included item stands.
- `itemoffset=x,z,y`: Offset when spawning items. Also sets the `useeffect` position.
- `levelchance=percent`: Level up chance (from 0 to 100).
- `maxlevel=number`: Maximum level (level 1 = no star).
- `minlevel=number`: Minimum level (level 1 = no star).
- `name=text`: Display name. Put " around the value if you use spaces.
- `respawn=minutes`: How often the altar can be used.
- `spawn=id`: Spawned item or object.
- `spawndata=base64 encoded`: Sets the ZDO data, for example when using `object copy`.
- `spawneffect=id,flag`: Effects when the spawn happens. For random rotation, use 1 as the flag.
- `spawnhealth=number`: Overrides the creature health.
- `spawnitem=id`: Required item.
- `spawnmaxy=meters`: Maximum height difference from the altar.
- `spawnoffset=meters`: Spawn distance from the ground.
- `spawnradius=meters`: Maximum spawn radius.
- `starteffect=id,flag`: Effects when the spawn is started. For random rotation, use 1 as the flag.
- `text="text"`: Use text. Put " around the value if you use spaces.
- `useeffect=id,flag`: Effects when used. For random rotation, use 1 as the flag.

`tweak_beehive`:

- `biome=biome1,biome2,...`: List of active biomes.
- `coveroffset=x,z,y`: Offset for the cover calculation.
- `maxamount=number`: Maximum amount of production.
- `maxcover=number`: Coverage limit (from 0.0 to 1.0).
- `name="text"`: Display name. Put " around the value if you use spaces.
- `spawn=id`: Produced item.
- `spawncondition=flag`: 1 = produces also during the night.
- `spawneffect=id,flag`: Effects when taking the items. For random rotation, use 1 as the flag.
- `spawnoffset=x,z,y`: Offset for the produced items.
- `speed=seconds`: Interval for production.
- `textbiome="text"`: Text for wrong biome. Put " around the value if you use spaces.
- `textcheck="text"`: Text for checking the amount. Put " around the value if you use spaces.
- `textextract="text"`: Text for taking the items. Put " around the value if you use spaces.
- `texthappy="text"`: Text when being happy. Put " around the value if you use spaces.
- `textsleep="text"`: Text when sleeping. Put " around the value if you use spaces.
- `textspace="text"`: Text when covered. Put " around the value if you use spaces.

`tweak_chest`:

- `name="text"`: Display name. Put " around the value if you use spaces.
- `item=id,weight,minamount,maxamount`: Loot item. Weight is the relative chance compared to other items.
- `maxamount=number`: Maximum amount of items.
- `minamount=number`: Minimum amount of items.
- `respawn=minutes`: Respawn time for items. Automatically sets `item`, `maxamount` and `minamount` based on chest contents if not given in the command.

`tweak_creature`:

- `affix=name`: Sets the CLLC affix (if CLLC is installed).
- `attacks=id1,id2,...`: Sets the available attacks. If `attacks` is given multiple times, the attack set is randomly selected.
- `boss=true/false`: Sets the boss health bar. No value to toggle.
- `damage=number`: Sets the damage multiplier.
- `faction=text`:  Determines which creatures are considered hostile.
- `health=number`: The health.
- `hunt=true/false`: Sets the extra aggressiveness mode.
- `item=id,chance,minamount,maxamount,flags`: Item drop. Sum up: 1 = star multiplier, 2 = one per player.
- `level=number`: Level (level 1 = no star).
- `name="text"`: Display name. Put " around the value if you use spaces.
- `resistance=type,modifier`: Sets a damage resistance.

`tweak_fermenter`:

- `conversion=from,to,amount`: Conversion recipes.
- `inputeffect=id,flag`: Effects when adding a new item. For random rotation, use 1 as the flag.
- `outputeffect=id,flag`: Effects when producing a new item. For random rotation, use 1 as the flag.
- `speed=number`: Conversion speed in seconds.
- `useeffect=id,flag`: Effects when using the tap. For random rotation, use 1 as the flag.

`tweak_itemstand`:

- `name="text"`: Display name. Put " around the value if you use spaces.
- `item=id,variant`: Attached item. Variant is a number.
- `respawn=minutes`: Respawn time for items. Automatically sets the `item` based on the current item.

`tweak_pickable`:

- `amount=number`: Amount of dropped items.
- `name="text"`: Display name. Put " around the value if you use spaces.
- `respawn=minutes`: Respawn time. Must be an integer (no decimals).
- `spawn=id`: Spawned item or object.
- `spawnoffset=meters`: Spawn distance from the ground.
- `useeffect=id,flag`: Effects when used. For random rotation, use 1 as the flag.

`tweak_smelter`:

- `conversion=from,to`: Conversion recipes.
- `fuel=id`: Fuel item.
- `fuelusage=number`: Required fuel per conversion.
- `fueleffect=id,flag`: Effects when adding a new fuel. For random rotation, use 1 as the flag.
- `inputeffect=id,flag`: Effects when adding a new item. For random rotation, use 1 as the flag.
- `maxamount=number`: Maximum amount of items in the queue.
- `maxfuel=number`: Maximum amount of fuel.
- `outputeffect=id,flag`: Effects when producing a new item. For random rotation, use 1 as the flag.
- `speed=number`: Conversion speed in seconds.

`tweak_spawner`:

- `faction=text`: Determines which creatures are considered hostile.
- `farradius=meters`: Radius for `maxtotal`.
- `respawn=seconds`: Respawn time.
  - Pickable: Timer is stopped while the item is picked up.
- `levelchance=percent`: Level up chance (from 0 to 100).
- `maxlevel=number`: Maximum level (level 1 = no star).
- `minlevel=number`: Minimum level (level 1 = no star).
- `maxnear=number`: Maximum amount of spawns within the `nearradius`.
- `maxtotal=number`: Maximum amount of spawns within the `farradius`.
- `nearradius=meters`: Radius for `maxnear`.
- `respawn=seconds`: Respawn time.
- `spawn=id,weight,minlevel,maxlevel,data/health`: Spawned item or object. Weight is the relative chance compared to other spawns. Data is the ZDO data, for example when using `object copy`.
- `spawncondition=flag`: Sum up: 1 = day only, 2 = night only, 4 = ground only.
- `spawneffect=id,flag`: Effects when the spawn happens. For random rotation, use 1 as the flag.
- `spawnhealth=number`: Overrides the creature health.
- `spawnradius=meters`: Maximum spawn radius.
- `triggerdistance=meters`: Required distance to activate the spawner.

`tweak_spawnpoint`:

- `faction=text`: Determines which creatures are considered hostile.
- `levelchance=percent`: Level up chance (from 0 to 100).
- `maxlevel=number`: Maximum level (level 1 = no star).
- `minlevel=number`: Minimum level (level 1 = no star).
- `respawn=minutes`: Respawn time.
- `spawn=id`: Spawned item or object.
- `spawndata=base64 encoded`: Sets the ZDO data, for example when using `object copy`.
- `spawncondition=flag`: 1 = day only, 2 = night only.
- `spawneffect=id,flag`: Effects when the spawn happens. For random rotation, use 1 as the flag.
- `spawnhealth=number`: Overrides the creature health.
- `triggerdistance=meters`: Required distance to activate the spawn point.
- `triggernoise=meters`: Required noise to activate the spawn point.

### Examples: Spawner Tweaks

- `tweak_pickable use_effect=[sfx/vfx] use_effect=[sfx/vfx]`: Makes a pickable to cause two effects when picked.
- `tweak_spawner spawn=Boar spawn=Deer`: Makes any object spawn both boars and deer.
- `tweak_spawner spawn=lightningaoe respawn=1 spawnradius=10`: Makes any object to spawn lightning every second.
