- v1.32
  - Adds a new command `tweak_dungeon`.

- v1.31
  - Removes the leviathan test code that should have never been released. Oopsie!

- v1.30
  - Adds support the command `object copy` to allow copying only specific data keys.

- v1.29
  - Adds support for Pickable and Spawner combo.
  - Fixes chained commands (multiple commands per line) sometimes causing ghost objects.

- v1.28
  - Adds a new parameter `attacks` to the `tweak_creature` command.
  - Fixes `object copy` also copying creature spawn coordinates.

- v1.27
  - Adds a new command `tweak_beehive`.
  - Adds a new command `tweak_fermenter`.
  - Adds a new command `tweak_smelter`.
  - Fixes the `object move` not refreshing structure colliders.

- v1.26
  - Adds a new parameter `void` to the `terrain` command (for removing the terrain).
  - Adds smooth support to the `terrain paint` command.
  - Fixes 0 radius / rectangle in the `terrain` command causing void.

- v1.25
  - Adds support for multiple values to the parameter `id`.
  - Adds more values to the parameter `type`.
  - Adds parameters `id` and `ignore` to the `terrain` command (for block check).
