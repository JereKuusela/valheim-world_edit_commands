- v1.71
  - Adds new in-built paint value `lava` to create lava on Ashlands biome.
  - Adds support for changing only specific paint layers by using `*` as the layer value.
  - Fixes `object` command not able to set long data value. Thanks Haloa!
  - Fixes wrong autocomplete for `persist` operation. Thanks Haloa!
  - Fixes `data` command to support values with commas. Thanks Haloa!
  - Fixes `terrain paint` command automatically applying lava on Ashlands biome.
  
- v1.70
  - Improves `object connect` to support specifying returned ids and connection ids separately.

- v1.69
  - Fixes wrong base64 encoding of the priority data (very minor bug).

- v1.68
  - Adds new parameters `distant` and `persist` to the commands `spawn_object` and `object`.
  - Adds new fields `distant`, `persistent` and `priority` to the data file.
  - Improves the reverse hashing of commands `data save` and `data dump`.

- v1.67
  - Fixes the command `tweak_object weather=` not working properly for environment names with spacebars/underscores.
