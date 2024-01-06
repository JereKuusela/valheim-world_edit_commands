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
