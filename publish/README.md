# World Edit Commands

Adds new client side commands for advanced world editing.

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim).
2. Install the [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) mod.
3. Download the latest zip.
4. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.

# Commands

This mods add 4 new commands. Most parameters are given as named parameters that have a "key=value" format (or just "key" if no value is needed). All of them are optional and can be put in any order. All keys and some values are case insensitive too.

Some parameters accept ranges with "key=min-max" format. This causes each affected object to randomly get a value within the range which allows creating some random variation.

Use the `alias` command to create new simpler commands. This should significantly reduce the amount of typing.

Remember to bind `undo` and `redo` commands for easier undoing. Recommended also to read the manual of Server Devcommands mod.

## Object

The `object` alters the hovered object or objects within a given radius. This command doesn't support the `undo`/`redo` system.

Following parameters are available:

- `baby`: Prevents offspring from growing up.
- `durability=number` or `health=number`: Sets the current durability for items, the current health for structures and the maximum health for creatures. Very high values like 1E30 turn the target invulnerable (including gravity for structures).
- `id`: Filters objects by id. Supports starts with, ends with or contains by using "*". Default is `*` that allows all objects which don't start with "_". 
- `info`: Prints information of objects.
- `level=integer`: Sets levels for creatures (level = stars + 1).
- `move=x,z,y`: Moves objects (meters). Static objects only update their position for other players when they leave the area.
- `origin=player|object|world`: Base direction for `move` and `rotate`. Default value `player` uses the player's rotation, `object` uses the objects rotation and `world` uses the global coordinate system (x=north/south,y=up/down,z=west/east).
- `radius=number`: Radius for included objects. Capped at 100 meters. If not given, the hovered object is only affected.
- `remove`: Removes objects. Must use the `id` parameter (`id=*` is ok). Can't be used with other operations.
- `rotate=y,x,z`: Rotates objects (degrees). Static objects only update their rotation for other players when they leave the area.
- `rotate=reset`: Resets object rotation. Static objects only update their rotation for other players when they leave the area.
- `scale=x,z,y`: Scales objects (which support it). A single value sets all of the scales.
- `sleep`: Makes creatures fall asleep (that support it).
- `stars=integer`: Sets stars for creatures (stars = level - 1).
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

### Examples

- `object radius=50 move=-5-5,-5,5`: Randomly moves all objects within 50 meters.
- `object move=5`: Moves the targeted object 5 meters away from you.
- `object move=0,5 origin=world`: Moves the targeted object 5 meters towards east.
- `alias remove object remove id=$`: Adds a command `remove [object id]` that removes the targeted object if it matches the given object id.
- `alias remove object remove id=*`: Adds a command `remove` that removes the targeted object without having to specify the id.
- `alias remove50 object remove radius=50 id=$`: Adds a command `remove50 [object id]` that removes the given objects within 50 meters.
- `alias essential object tame health=1E30 radius=$ id=$`: Adds a command `essential [radius] [object id]` that tames and turns objects invulnerable within a given radius and with a given id. If radius and id is not given, then affects the hovered object.

## Spawn object

The `spawn_object [object id]` spawns object to the world. This is similar to the `spawn` command but has more parameters and `undo`/`redo` system.

Following parameters are available:

- `amount=integer`: Amount of spawned objects within a random radius. Items are autostacked.
- `crafter=value` or `name=value`: Name of the crafter for items or name for tamed creatures (that support naming).
- `durability=number` or `health=number`: Overrides the current durability for items, the current health for structures and the maximum health for creatures. Very high values like 1E30 turn the target invulnerable (including gravity for structures).
- `hunt`: Spawned creatures are in the hunt mode.
- `level=integer`: Spawned creatures have this amount of level (level = stars + 1).
- `pos=x,z,y`: Relative position (meters) from the player. If not given, the objects are spawned 2 meters front of the player. If y coordinate is not given, the objects snaps to the ground.
- `radius=number`: Maximum spawn distance when spawning multiple objects. Default is 0.5 meters.
- `refPlayer=name`: Allows overriding the player's position with another player's position.
- `refPos=x,z,y`: Allows overriding the player's position for the command. Used by `redo` and can be useful for some advanced usage.
- `refRot=y,x,z`: Allows overriding the player's rotation for the command. Used by `redo` and can be useful for some advanced usage.
- `rot=y,x,z`: Relative rotation (degrees) from the player's rotation.
- `scale=x,z,y`: Scale for the spawned objects (which support it). A single value sets all of the scales.
- `stars=integer`: Spawned creatures have this amount of stars (stars = level - 1).
- `tame`: Spawned creatures are tamed.
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

### Examples

- `spawn_object StatueCorqi rot=0-360 amount=10 radius=10`: Spawns 10 corqi status within 10 meters that have a random rotation. Undo/redo can be used to reroll the positions and rotations.
- `spawn_object Rock_4 scale=0.5-5 rot=0-360 amount=200 radius=100`: Spawns 200 rocks with a random rotation and a random scale.
- `spawn_object Wolf star=0-2 amount=5 radius=20`: Spawns 5 wolves with random stars from 0 to 2.
- `spawn_object PickaxeIron refPlayer=Jay`: Spawns an iron pickaxe at the position of a player called Jay.
- `alias spawn spawn_object $ amount=$ level=$`: Upgrades the default `spawn` command with undo/redo.

## Spawn location

The `spawn_location [location id]` spawns Point of Interests to the world. The main difference to the `test_location` command is that the game saving doesn't get disabled and all parameters can be set. It also supports the `undo`/`redo` system.

Following parameters are available:

- `seed=number`: Sets the result of randomized locations. If not given, the result is random.
- `dungeonSeed=number`: Sets the result of next dungeon generation. If not given, the result is random. If the location is not a dungeon, this will carry over to the next dungeon generation. Forcing dungeon seed randomize the dungeon room seeds (instead of being based on the room position).
- `pos=x,z,y`: Relative position (meters) from the player. If not given, the location will be placed at the player. If y coordinate is not given, the location snaps to the ground.
- `refPos=x,z,y`: Allows overriding the player's position for the command. Used by `redo` and can be useful for some advanced usage.
- `rot=y`: Relative rotation (degrees) from the player's rotation.
- `refRot=y`: Allows overriding the player's rotation for the command. Used by `redo` and can be useful for some advanced usage.

### Examples

- `spawn_location DevHouse1`: Spawns a location at your position (snaps to the ground).
- `spawn_location DevHouse1 pos=10 seed=1`: Spawns a location 10 meters in front of you with a specific random outcome.
- `alias location spawn_location`: Upgrades the default command with undo/redo, no save breaking and support for extra parameters.

## Terrain

The `terrain` command can create different shapes on top of the usual flattening and resetting. It also supports the `undo`/`redo` system.

Note: The granularity of the terrain system is 0.5 meters.

Following parameters are available:

- `raise=number`: Raises terrain by X meters. Same as `lower` when a negative value is used.
- `lower=number`: Lowers terrain by X meters. Same as `raise` when a negative value is used.
- `reset`: Resets terrain height. Ground material is not affected.
- `level=number`: Sets terrain height to the given altitude. If not given, uses the ground altitude below the player.
- `paint=value`: Sets the terrain material (dirt, paved, cultivated or grass to reset).
- `radius=number`: Determines the size of the affected terrain. Capped at 64 meters to prevent changes outside the active play area (causes technical issues).
- `blockcheck`: Exclude terrain that is under structures or other objects. This can be used to create specific shapes.
- `blockcheck=on/off/inverse`:
	- on: Exclude terrain that is under structures or other objects.
	- off: All terrain is affected (default).
	- inverse: Only include terrain that is under structures or other objects.
- `square`: If given, the shape of the affected terrain is a square instead of a circle. Radius becomes half of the edge length.
- `smooth=number`: Determines how gradually the changes are applied (from 0.0 to 1.0).
	- 1.0: All of the terrain gets reduced changes (except the very center).
	- 0.5: Half of the terrain gets reduced changes.
	- 0.0: No reduction (default).

### Examples

- `terrain level radius=10`: Sets terrain height within 10 meters to the same level as below you. 
- `terrain level terrain raise=4 radius=4 square smooth=1 paint=paved`: Creates a pyramid.
- `terrain level raise=4 square;terrain lower=4 radius=4 square smooth=1 paint=paved`: Creates a pyramid shaped hole.
- `alias level terrain level radius=$`: New command to `level [value]` for easier leveling.
- `alias level terrain level square radius=$`: New command to `level_sq [value]` for easier leveling.

# Mechanics

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

# Changelog

- v1.0: 
	- Initial release.

Thanks for Azumatt for creating the mod icon!