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

- v1.53
  - Fixes data not always being synced on servers.

- v1.52
  - Adds a parameter `chance` to the command `terrain`.
  - Adds a parameter `components` to the command `object`.
  - Adds support for multiple values to the parameter `type`.
  - Changes the parameter `type` to use actual component names instead of some hardcoded values.
  - Fixes the `field` parameters being applied to all objects without checking the components.

- v1.51
  - Adds `id` support when selecting objects with `connect`.
  - Fixes `field` autocomplete not including child components.
  - Makes `tweak_creature faction=` work with the Expand World Factions mod.

- v1.50
  - Fixes compatibility issue with Drop That.

- v1.49
  - Improves compatibility with some mods.

- v1.48
  - Adds support for persisting false, zero or empty `field` values when installed on the server.
  - Adds connected ZDO info to the `object copy=all` command.
  - Fixes false not working for the `field` parameter.
