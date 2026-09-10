- v1.75
  - Fixes for the new game update.

- v1.74
  - Fixes `from` parameter not turning off snapping even when y coordinate is specified.
  - Internal changes to match Server Devcommands code.

- v1.73
  - Fixes `spawn_object pos` not being "forward, right, up" relative to the player.

- v1.72
  - Fixes terrain paint operations with Expand World Data mod.

- v1.71
  - Adds new in-built paint value `lava` to create lava on Ashlands biome.
  - Adds support for changing only specific paint layers by using `*` as the layer value.
  - Fixes `object` command not able to set long data value. Thanks Haloa!
  - Fixes wrong autocomplete for `persist` operation. Thanks Haloa!
  - Fixes `data` command to support values with commas. Thanks Haloa!
  - Fixes `terrain paint` command automatically applying lava on Ashlands biome.
  
- v1.70
  - Improves `object connect` to support specifying returned ids and connection ids separately.
