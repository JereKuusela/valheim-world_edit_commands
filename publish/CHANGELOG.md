- v1.63
  - Adds a new command `data dump` to save data with default fields.
  - Adds some missing zdo keys.
  - Changes `object f=DungeonGenerator,room0_pos` to use relative coordinates (converted automatically to absolute).
  - Fixes the `data keep` command not working. Thanks ddein!
  - Reworks the undo system.

- v1.62
  - Adds some missing zdo keys.
  - Adds normal ZDO data keys to fields.
  - Adds support for setting CLLC infusion with the command `tweak_creature infusion=`.
  - Fixes the command `data print` taking object ownership.
  - Fixes the command `data set` not showing "long" on the autocomplete.
  - Removes the id requirement when using the `object remove` command.

- v1.61
  - No real changes, just compatibility for Expand World Prefabs data changes.

- v1.60
  - Fixes for the new game version.

- v1.59
  - Fixes arithmetics not working for int and long values.

- v1.58
  - Fixes possible error when matching float, int or long values.
  - Fixes undo not reverting `object move` and `object rotate` commands.

- v1.57
  - Fixes data from profile folder not being loaded.
  - Fixes `tweak_runestone compendium` not working.
  - Fixed for the new game version.

- v1.56
  - Adds new default value groups containing all objects of a certain type.
  - Changes the `ignore` parameter to also prevent the connecting with the `connect` parameter.
  - Fixes value groups being case sensitive.
  - Fixes setting max health not updating the visual correctly.

- v1.55
  - Adds new paramater `data copy_raw` to make it easier to use the old system.
  - Fixes multiple data entries not working for `spawn_object data`.
  - Fixes multiple data entries not working for `data data`.
  - Fixes item generation not working from data.

- v1.54
  - Adds support for targeting players when `id=Player` is used.
  - Adds new command `data` for data related operations.
  - Adds support for the Expand World Data system (with much more features).
  - Adds keywords `creature` (Humanoid) and `structure` (WearNTear) back to the `type` parameter.
  - Adds new parameters `match` and `unmatch` to filter objects by data.
  - Fixes the `type` parameter being case sensitive.
  - Fixes the `type` parameter not working for the hovered object.
  - Fixes the `field` parameter not working with the `Piece` component.
  - Fixes the `object` command defaulting to scale 1 when no scale was provided.
  - Improves reliability of the undo feature.
  - Replaces `object copy` command, now it copies the object id.
  - Removes `object data` as obsolete.
