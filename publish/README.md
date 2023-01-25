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

- v1.23
	- Fixes the parameter `scale` not forking on the `spawn_object` command.
	- Fixes the autocomplete of the `tweak_object show` command.

- v1.22
	- Adds a parameter `crafterId` to the `spawn_object` command.
	- Adds a parameter `ignore` to the `object` command.
	- Adds a new value `vegetation` to the `terrain paint=` parameter.

- v1.21
	- Adds a parameter `damage` to the `object`, `spawn_object` and `tweak_creature` commands.
	- Adds parameters `ammo` and `ammo_type` to the `object` and `spawn_object` commands.
	- Adds a parameter `status` to the `object` command.
	- Fixes a rare error when other mods added objects with the same name (but different casing).

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
