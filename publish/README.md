# World Edit Commands

Adds new commands for advanced world editing.

Install on the admin client (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install also [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/).

# Usage

See [documentation](https://github.com/JereKuusela/valheim-world_edit_commands/blob/main/README.md).

# Credits

Thanks for Azumatt for creating the mod icon!

Sources: [GitHub](https://github.com/JereKuusela/valheim-world_edit_commands)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

- v1.21
	- Adds a parameter `damage` to the `object` and `tweak_creature` commands.

- v1.20
	- Improves server side support.
	- Adds a parameter `data` to the `spawn_object` command.
	- Adds a parameter `creator` to the `tweak_object` command.
	- Adds a parameter `spawndata` to the `tweak_altar` command.
	- Adds a parameter `spawndata` to the `tweak_spawnpoint` command.
	- Adds a value for data to the `tweak_spawner spawn` paramemter.
	- Adds parameters `boss`, `health`, `hunt`, `item`,`level`, `name` and `resistance` to the `tweak_creature` command.
	- Adds parameters `faction`, `maxlevel`, `minlevel` and `spawnhealth` to the `tweak_spawner` command.

- v1.19
	- Changes the `tweak_*` command to automatically use `force` parameter when editing a single object.

- v1.18
	- Adds a parameter `baby` to the `spawn_object` command.
	- Changes the `radius` and `rect` parameters to support ranges.
	- Removes the `guide` parameter as obsolete (Infinity Hammer provides this).

- v1.17
	- Adds a new command `tweak_object` to take Spawner / Structure Tweaks command from the `object` command.
	- Changes the `world_edit_aliases` command to use the substitution value from Server Devcommands mod (internal change).
	- Fixes `spawn_object` removing the hunt mode of the bosses.

- v1.16
	- Adds a new parameter `copy` to the `object` command to copy zdo data.
	- Adds a new parameter `data` to the `object` command to print and set zdo data.
	- Fixes the `discover` parameter of `tweak_runestone` not working.
