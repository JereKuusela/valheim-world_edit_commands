# World Edit Commands

Adds new commands for advanced world editing.

Install on the admin client (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install also [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/).

## Tutorials

- Basic usage + commands: <https://youtu.be/Bwkb3XadSl0> (28 minutes, created by StonedProphet)
- Structure Tweaks commands: <https://youtu.be/OaGO9Vis6uE> (16 minutes, created by StonedProphet)
- Spawner Tweaks commands: <https://youtu.be/mS59BMvR5vY> (22 minutes, created by StonedProphet)

## Commands

This mods add 4 new commands. Most parameters are given as named parameters that have a "key=value" format (or just "key" if no value is needed). All of them are optional and can be put in any order. All keys and some values are case insensitive too.

Some parameters accept ranges with "key=min-max" format. This causes each affected object to randomly get a value within the range which allows creating some random variation.

Use the `alias` command to create new simpler commands. This should significantly reduce the amount of typing.

Remember to bind `undo` and `redo` commands for easier undoing. Recommended also to read the manual of Server Devcommands mod.

## Object

The `object [...args]` alters the hovered object or objects within a given radius. The `undo`/`redo` system is supported by saving snapshots of the object.

Following parameters are available:

- `ammo=number`: Sets the amount of ammo for turrets.
- `ammoType=item`: Sets the ammo type for turrets. Throwables, magic weapons and enemy weapons ranged weapons seem to work. Arrows work too but instantly fall to the ground.
- `angle=degrees`: Direction of the rectangle when used with `rect`.
- `baby`: Prevents offspring from growing up.
- `copy`: Copies most of the ZDO data to the clipboard. Excludes data:
  - Object scale (redundant because scale usually comes from the spawn system).
  - Creature spawn coordinates (harmful because creatures try returning to the spawn coordinates when idle).
  - LocationProxy data (redundant because the location spawn system sets this).
  - Connected ZDO (usually not needed).
- `copy=all`: Copies all of the ZDO data to the clipboard.
- `copy=key1,key2,...`: Copies give data keys to the clipboard. See bottom of the file for available keys.
- `center`: Sets `rotate` center point at player position.
- `center=x,z,y`: Overrides the player position and sets `rotate` center point.
- `chance=number`: Randomly filters included objects. For example 0.5 includes about half of the objects (50%).
- `creator=player id`: Sets the piece creator. Use 0 for no creator.
- `circle=number` or `circle=min-max`: Radius for included objects. If not given, the hovered object is only affected.
- `connect`: Includes entire structures.
- `damage=number`: Sets damage multiplier.
- `data=[key]`: Prints ZDO data.
- `data=[key,value]`: Sets ZDO data. The key must already exist in the data.
- `durability=number` or `health=number`: Sets the current durability for items, the current health for structures and the maximum health for creatures. Very high values like 1E30 turn the target invulnerable (including gravity for structures).
  - Note: Invulnerable objects without structure support take continuous damage which causes network traffic.
- `durability=number%` or `health=number%`: Sets the durabilty/health based on the max amount.
- `field=component,field,value`: Sets arbitrary component field. When using `id=` or hovering an object, the autocompletion shows only components for that object.
  - Multiple `field=` can be used in the same command.
  - Component name doesn't have to be fully written (best match is used).
  - Use `f=` as a shortcut for `field=`.
  - Create alias `alias sf spawn_object $$ f=$$ f=$$ f=$$ f=$$ f=$$` to quickly set multiple fields.
  - Note: This is vanilla compatible.
  - False, zero and empty values are removed on world load, unless World Edit Commands is also installed on the server.
- `fuel=number`: Sets the fuel amount. Without number, prints the fuel amount.
- `from=x,z,y`: Same as the `center`.
- `height`: Maximum height from the `center` point (default is 1000 meters).
- `id=id1,id2,...`: List of included ids. Supports starts with, ends with or contains by using "*". Default is `*` that allows all objects which don't start with "_".
- `ignore=id1,id2,..`: List of ignored ids. Supports starts with, ends with or contains by using "*"
- `info`: Prints information of objects.
- `level=integer`: Sets levels for creatures (level = stars + 1).
- `mirror`: Mirrors the position and rotation based on the player position. Always mirrors the x-axis.
- `move=forward,right,up`: Moves objects (meters). Static objects only update their position for other players when they leave the area.
- `origin=player|object|world`: Base direction for `move` and `rotate`. Default value `player` uses the player's rotation, `object` uses the objects rotation and `world` uses the global coordinate system (x=north/south,y=up/down,z=west/east).
- `prefab=id`: Replaces the object with the given id.
- `radius=number` or `radius=min-max`: Radius for included objects. If not given, the hovered object is only affected.
- `rect=width,depth` or `rect=min-max,min-max`: Area for included objects. If not given, the hovered object is only affected.
- `remove`: Removes objects. Must use the `id` parameter (`id=*` is ok). Can't be used with other operations.
- `respawn`: Resets loot chests, pickables and spawn points.
- `rotate=yaw,roll,pitch`: Rotates objects (degrees). Static objects only update their rotation for other players when they leave the area.
- `rotate=reset`: Resets object rotation. Static objects only update their rotation for other players when they leave the area.
- `scale=x,z,y`: Scales objects (which support it). A single value sets all of the scales.
- `sleep`: Makes creatures fall asleep (that support it).
- `stars=integer`: Sets stars for creatures (stars = level - 1).
- `status=name,duration,intensity`: Adds a status effect. Note: These are not stored to the save file.
- `type=creature/chest/fireplace/item/spawner/spawnpoint/structure`: Only selects creatures or structures.
- `wild`: Untames creatures.

Additional style parameters:

- `chest=item id,variant`: Sets creatures to have this item as their chest (if supported).
- `helmet=item id,variant`: Sets creatures to have this item as their helmet (if supported).
- `left_hand=item id,variant`: Sets creatures to have this item on their left hand (if supported).
- `legs=item id,variant`: Sets creatures to have this item as their legs (if supported).
- `right_hand=item id,variant`: Sets creatures to have this item on their right hand (if supported).
- `shoulders=item id,variant`: Sets creatures to have this item as their shoulders (if supported).
- `utility=item id,variant`: Sets creatures to have this item as their utility (if supported).
- `visual=item id,variant`: Sets item stands to have this item (including enemy weapons). Not all items work without Item Stand All Items mod.

Note: Creatures reset their style when attacking.

### Examples: Object

- `object radius=50 move=-5-5,-5,5`: Randomly moves all objects within 50 meters.
- `object move=5`: Moves the targeted object 5 meters away from you.
- `object move=0,5 origin=world`: Moves the targeted object 5 meters towards east.
- `object prefab=Wolf`: Changes the targeted object to a wolf.
- `object rotate=90 center radius=10`: Rotates nearby objects around the player.
- `alias remove object remove id=$$`: Adds a command `remove [object id]` that removes the targeted object if it matches the given object id.
- `alias remove object remove id=*`: Adds a command `remove` that removes the targeted object without having to specify the id.
- `alias remove50 object remove radius=50 id=$$`: Adds a command `remove50 [object id]` that removes the given objects within 50 meters.
- `alias essential object tame health=1E30 radius=$$ id=$$`: Adds a command `essential [radius] [object id]` that tames and turns objects invulnerable within a given radius and with a given id. If radius and id is not given, then affects the hovered object.

## Spawner Tweaks / Structure Tweaks mods

Some new commands are added if these mods are installed. They all start with the `tweak_` prefix. All values can be reseted by providing no value to the parameter.

Most commands can be used on any object. When editing a single object, the script component is automatically added if missing. When editing multiple objects, `force` parameter needs to be used to add missing components.

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

### Spawner Tweaks mod

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

## Spawn object

The `spawn_object [object id] [...args]` spawns objects to the world. The `undo`/`redo` system is supported by saving snapshots of the created objects.

Following parameters are available:

- `ammo=number`: Sets the amount of ammo for turrets.
- `ammoType=item`: Sets the ammo type for turrets. Throwables, magic weapons and enemy weapons ranged weapons seem to work. Arrows work too but instantly fall to the ground.
- `amount=integer`: Amount of spawned objects within a random radius. Items are autostacked.
- `baby`: Prevents offspring from growing up.
- `crafter=value` or `name=value`: Name of the crafter for items or name for tamed creatures (that support naming). Character _ is replaced with a space bar.
- `damage=number`: Sets the damage multiplier.
- `data=base64 encoded`: Sets the ZDO data, for example when using `object copy`.
- `durability=number` or `health=number`: Overrides the current durability for items, the current health for structures and the maximum health for creatures. Very high values like 1E30 turn the target invulnerable (including gravity for structures).
- `field=component,field,value`: Sets arbitrary component field. The autocompletion shows only components for the spawned object.
  - Multiple `field=` can be used in the same command.
  - Component name doesn't have to be fully written (best match is used).
  - Use `f=` as a shortcut for `field=`.
  - Create alias `alias sf spawn_object $$ f=$$ f=$$ f=$$ f=$$ f=$$` to quickly set multiple fields.
  - Note: This is vanilla compatible.
  - False, zero and empty values are removed on world load, unless World Edit Commands is also installed on the server.
- `from=x,z,y`: Allows overriding the player's position for the command. Used by `redo` and can be useful for some advanced usage.
- `hunt=true/false`: Spawned creatures are in the hunt mode.
- `level=integer`: Spawned creatures have this amount of level (level = stars + 1).
- `pos=forward,right,up`: Relative position (meters) from the player. If not given, the objects are spawned 2 meters front of the player. If y coordinate is not given, the objects snaps to the ground.
- `radius=number`: Maximum spawn distance when spawning multiple objects. Default is 0.5 meters.
- `refPlayer=name`: Allows overriding the player's position with another player's position.
- `refRot=yaw,roll,pick`: Allows overriding the player's rotation for the command. Used by `redo` and can be useful for some advanced usage.
- `rot=yaw,roll,pick`: Relative rotation (degrees) from the player's rotation.
- `scale=x,z,y`: Scale for the spawned objects (which support it). A single value sets all of the scales.
- `stars=integer`: Spawned creatures have this amount of stars (stars = level - 1).
- `tame=true/false`: Spawned creatures are tamed.
- `to=x,z,y`: Distributes the objects between the player and target position.
- `variant=integer`: Style/variant for some spawned items.

Additional style parameters:

- `chest=item id,variant`: Spawned creatures have this item as their chest (if supported).
- `helmet=item id,variant`: Spawned creatures have this item as their helmet (if supported).
- `left_hand=item id,variant`: Spawned creatures have this item on their left hand (if supported).
- `legs=item id,variant`: Spawned creatures have this item as their legs (if supported).
- `right_hand=item id,variant`: Spawned creatures have this item on their right hand (if supported).
- `shoulders=item id,variant`: Spawned creatures have this item as their shoulders (if supported).
- `utility=item id,variant`: Spawned creatures have this item as their utility (if supported).

Note: Creatures reset their style when attacking.

### Examples: Spawn Object

- `spawn_object StatueCorqi rot=0-360 amount=10 radius=10`: Spawns 10 corqi status within 10 meters that have a random rotation. Undo/redo can be used to reroll the positions and rotations.
- `spawn_object Rock_4 scale=0.5-5 rot=0-360 amount=200 radius=100`: Spawns 200 rocks with a random rotation and a random scale.
- `spawn_object Wolf star=0-2 amount=5 radius=20`: Spawns 5 wolves with random stars from 0 to 2.
- `spawn_object PickaxeIron refPlayer=Jay`: Spawns an iron pickaxe at the position of a player called Jay.
- `alias spawn spawn_object $$ amount=$$ level=$$`: Upgrades the default `spawn` command with undo/redo.

## Spawn location

The `spawn_location [location id] [...args]` spawns Point of Interests to the world. The main difference to the `location` command is that the game saving doesn't get disabled and all parameters can be set. The `undo`/`redo` system is supported by saving snapshots of the created objects.

Following parameters are available:

- `seed=number`: Sets the result of randomized locations. If not given, the result is random.
- `dungeonSeed=number`: Sets the result of next dungeon generation. If not given, the result is random. If the location is not a dungeon, this will carry over to the next dungeon generation. Forcing dungeon seed randomize the dungeon room seeds (instead of being based on the room position).
- `from=x,z,y`: Allows overriding the player's position for the command. Used by `redo` and can be useful for some advanced usage.
- `pos=forward,right,up`: Relative position (meters) from the player. If not given, the location will be placed at the player. If y coordinate is not given, the location snaps to the ground.
- `rot=degrees`: Relative rotation (degrees) from the player's rotation.
- `refRot=degrees`: Allows overriding the player's rotation for the command. Used by `redo` and can be useful for some advanced usage.

### Examples

- `spawn_location DevHouse1`: Spawns a location at your position (snaps to the ground).
- `spawn_location DevHouse1 pos=10 seed=1`: Spawns a location 10 meters in front of you with a specific random outcome.
- `alias location spawn_location`: Upgrades the default command with undo/redo, no save breaking and support for extra parameters.

## Terrain

The `terrain [...args]` command can create different shapes on top of the usual flattening and resetting. The `undo`/`redo` system is supported by saving snapshots of the terrain.

Note: The granularity of the terrain system is 0.5 meters.

Note: Terrain is only affected in loaded areas. You can use Render Limits mod to increase this area.

Following parameters are available:

- `angle=degrees`: Determines the direction. Cardinal directions like n, ne, e, se, s, sw, w and nw work as a value too. Uses the player's direction if not given (45 degrees precision).
- `blockcheck`: Excludes terrain that is under structures or other objects. This can be used to create specific shapes.
- `blockcheck=on/off/inverse`:
  - on: Excludes terrain that is under structures or other objects.
  - off: All terrain is affected (default).
  - inverse: Only includes terrain that is under structures or other objects.
- `circle=number` or `circle=min-max`: Determines the radius of the affected terrain.
- `delta=meters`: Sets the difference from the original elevation. Without the parameter, resets terrain altitude changes.
- `from=x,z,y`: Overwrites the player's position. Allows fixing the current position for more precise editing. The y coordinate can be used to override the current ground altitude.
- `id=id1,id2,...`: List of included ids. Supports starts with, ends with or contains by using "*". Default is `*` that allows all objects which don't start with "_".
- `ignore=id1,id2,..`: List of ignored ids. Supports starts with, ends with or contains by using "*"
- `level=altitude`: Sets terrain height to the given altitude. If not given, uses the ground altitude below the player.
- `lower=meters`: Lowers terrain by X meters. Same as `raise` when a negative value is used.
- `max=altitude`: Lowers terrain above the given altitude to the altitude.
- `min=altitude`: Raises terrain below the given altitude to the altitude.
- `offset=forward,right,up`: Moves the targeted position while still using the altitude of the player's position.
- `smooth=number`: Determines how gradually the changes are applied (from 0.0 to 1.0).
  - 1.0: All of the terrain gets reduced changes (except the very center).
  - 0.5: Half of the terrain gets reduced changes.
  - 0.0: No reduction (default).
- `paint=value`: Sets the terrain material (cultivated, grass, grass_dark,dirt, patches, paved, paved_dark, paved_dirt or paved_moss).
- `paint=dirt,cultivated,paved,vegetation`: Sets custom terrain material (values from 0.0 to 1.0).
- `raise=meters`: Raises terrain by X meters. Same as `lower` when a negative value is used.
- `rect=width,depth` or `rect=min-max,min-max`: Determines the size of the affected terrain.
- `reset`: Resets terrain height and paint changes. Ignores `smooth` parameter.
- `slope=meters,angle`: Creates a slope centered at current position with a given height.
- `step=forward,right,up`: Calculates offset based on the radius (and slope height if given).
- `to=x,z,y`: Moves the affected terrain between the current position and this position. Determines angle, slope and circle/rect size automatically.
- `void`: Removes the terrain surface. Ignores `smooth` parameter.
- `within=min-max`: Only includes terrain that is within the given altitude range.

### Examples: Terrain

- `terrain level circle=10`: Sets terrain height within 10 meters (20 meters diameter) to the same level as below you.
- `terrain level terrain raise=4 rect=4 smooth=1 paint=paved`: Creates a pyramid.
- `terrain level raise=4 rect=4;terrain lower=4 rect=4 smooth=1 paint=paved`: Creates a pyramid shaped hole.
- `terrain slope rect=5 from=10,10 to=10,20`: Creates a slope between two positions (10 meters width).
- `terrain slope=5 rect=5 from=10,10 to=10,20`: Creates a 5 meter slope between two positions (10 meters width).
- `terrain level from=300,40,-500 rect=5,10000`: Creates a long leveled path. Walk along the path to load new areas and then use the command again to extend the path.
- `terrain from=-23,23 angle=e rect=5 slope=4`: Creates a slope rising towards east.
- `terrain from=-23,23 angle=e rect=5 slope=4 step=1 level`: Creates a level at the end of the slope.
- `terrain from=-23,23 angle=e rect=5 slope=4,e step=1,1`: Creates a slope at right side of the level rising towards south (east of east).
- `terrain from=-23,23 angle=e rect=5 slope=4 step=1,2,1 level`: Creates a level at the end of the slope. The last parameter of step is needed because the slope is not going to the original direction so it won't be raised automatically.
- `alias level terrain level circle=$$`: New command `level [value]` for easier leveling.
- `alias level terrain level rect=$$`: New command `level_sq [value]` for easier leveling.

## Mechanics

Any changes made by the mod are compatible with unmodded clients which adds some significant restrictions to some commands.

### Creature style

Creatures overwrite their style when they try to attack. So altering their style has mainly three uses:

- Decorating the training dummy since it never attacks.
- Decorating tamed creatures while ensuring they are safe.
- Initially decorating aggresive creatures (as long as it's ok that their style resets during the fight).

### Creature health

Creatures reset their maximum health if their current health equals the maximum health. To prevent this, the mod sets creature health slightly higer than the max health.

However this means that if the creature is damaged and let to heal back to full, its health may reset when the area is loaded.

- When creating combat scenarios, manually set enemy health if the scenario is not finished.
- When creating stronger tamed creatures, manually set their health after the combat is over (recommended to bind a key for this).

### Creature / structure invulnerability

Setting a very high health like 1E30 will make objects invulnerable. This is because the float variable type has a limited precision so small amounts of damage is rounded down to nothing. This may cause following:

- Lack of support doesn't break structures so build limits can be ignored (since lack of support only causes damage equal to the default max health).
- `killall` command doesn't kill some creatures (since it only deals very high damage to them).

### Dig limit

This mod doesn't unlock the terrain dig limit so all changes will still be capped by the limit (default is +- 8 meters). However the `terrain` command should be compatible with any mods affecting the dig limit.

The command however supports going over the limit which can lead to unexpected results. For example if you lower terrain by 20 meters and then raise by 10 meters, the second command probably won't do anything because the result is still below the dig limit.

### Moving objects

Static objects only synchronize their position and rotation when loaded. This means that `object move` and `object rotate` commands won't instantly show for other clients. Instead they have to leave the area.

This shouldn't cause any issues unless objects are moved long distances (which might cause issues anyways).

## ZDO data keys

Most should be self-explanatory. More explanation will be added later.

### Vanilla keys

- `addedDefaultItems`:
- `ammo`: object ammo
- `alive_time`:
- `AmmoType`:
- `crafterID`:
- `crafterName`:
- `creator`:
- `durability`:
- `fuel`:
- `health`:
- `huntplayer`:
- `item`:
- `items`:
- `level`:
- `location`:
- `PlantTime`:
- `pose`:
- `quality`:
- `RandomSkillFactor`
- `scale`:
- `seed`:
- `sleeping`:
- `spawntime`:
- `spawnpoint`:
- `stack`:
- `tag`:
- `tamed`:
- `TamedName`:
- `text`:
- `variant`:

### Field keys

The field system adds `HasFields` to any object with custom fields.

The field system adds `HasFieldsXXX` for each component with custom fields. The XXX is the component name.

The field system adds `XXX.m_YYY` for each field where XXX is the component name and YYY is the field name.

### Modded keys

- `CL&LC effect`:
- `override_amount`:
- `override_attacks`:
- `override_biome`:
- `override_boss`:
- `override_collision`:
- `override_compendium`:
- `override_component`:
- `override_conversion`:
- `override_cover_offset`:
- `override_data`:
- `override_delay`:
- `override_destroy`:
- `override_discover`:
- `override_dungeon_enter_hover`:
- `override_dungeon_enter_text`:
- `override_dungeon_exit_hover`:
- `override_dungeon_exit_text`:
- `override_dungeon_weather`:
- `override_effect`:
- `override_event`:
- `override_faction`:
- `override_fall`:
- `override_fuel`:
- `override_fall`:
- `override_fuel_effect`:
- `override_globalkey`:
- `override_growth`:
- `override_health`:
- `override_input_effect`:
- `override_interact`:
- `override_item`:
- `override_item_offset`:
- `override_item_stand_prefix`:
- `override_item_stand_range`:
- `override_items`:
- `override_level_chance`:
- `override_maximum_amount`:
- `override_maximum_cover`:
- `override_maximum_fuel`:
- `override_maximum_level`:
- `override_max_near`:
- `override_max_total`:
- `override_minimum_amount`:
- `override_minimum_level`:
- `override_name`:
- `override_near_radius`:
- `override_output_effect`:
- `override_pickable_spawn`:
- `override_pickable_respawn`:
- `override_render`:
- `override_resistances`:
- `override_respawn`:
- `override_restrict`:
- `override_smoke`:
- `override_spawn`:
- `override_spawn_condition`:
- `override_spawn_effect`:
- `override_spawn_max_y`:
- `override_spawn_offset`:
- `override_spawn_radius`:
- `override_spawnarea_spawn`:
- `override_spawnarea_respawn`:
- `override_spawn_item`:
- `override_start_effect`:
- `override_text`:
- `override_text_biome`:
- `override_text_check`:
- `override_text_extract`:
- `override_text_happy`:
- `override_text_sleep`:
- `override_text_space`:
- `override_topic`:
- `override_trigger_distance`:
- `override_trigger_distance`:
- `override_trigger_noise`:
- `override_unlock`:
- `override_use_effect`:
- `override_water`:
- `override_wear`:
- `override_weather`:

## Credits

Thanks for Azumatt for creating the mod icon!

Sources: [GitHub](https://github.com/JereKuusela/valheim-world_edit_commands)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
