- v1.85
  - Fixed for the new update. Thanks Maxime!

- v1.84
  - Fixes blueprint loading not always recognizing commented out lines.
  - Fixes `spawn_object` applying field edits post spawn (too late for some changes).
  - Fixes `object` field edits not fully refreshing the object (missing some changes).
  - Fixes `spawn_object` snapping sometimes placing objects below the terrain.
  - Fixes `object model=` not being undoable.
  - Fixes `object` ignoring the `type` parameter when targeting the hovered object.
  - Fixes undo not reverting object changes in reverse order.
  - Fixes `tweak_altar` parameter `itemstandrange` not working.
  - Fixes `tweak_beehive` parameter `spawneffect` setting the output effect.
  - Fixes `tweak_chest` automatic item list missing the max amount.
  - Fixes `tweak_*` commands applying the `chance` parameter twice.
  - Fixes undo/redo of `data merge/set` removing or duplicating the object.
  - Fixes `tweak_fireplace` parameter `smoke` not resetting without a value.
  - Fixes `data set` not applying "HasFields" automatically.
  - Fixes some autocomplete texts.

- v1.83
  - Adds shared Blueprint implementation with Expand World Data and Expand World Prefabs mods.
  - Adds `spawn_blueprint` to place saved blueprint objects with undo/redo.
  - Fixes unnecessary warning logging when writing data to ZDO.

- v1.82
  - Fixes custom item data not working when spawning items.
