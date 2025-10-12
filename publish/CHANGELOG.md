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

- v1.66
  - Improves the undo system (again).
