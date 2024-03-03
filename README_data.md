# World Edit Commands Data

This document explains how to save object data to a file and then load it back to the game.

## How to get started

1. Use command `data save=Test` on any object.
2. Open the Valheim\BepInEx\config\data\data.yaml file to see the saved data.
3. Modify the file to change values of the saved data.
4. Use command `data load=Test` on any object to load the data to it.

## Data command

The `data` command has following operations:

- `clear`: Removes all data values.
- `keep=key1,key2,...`: Removes data values except the given ones.
- `load=data`: Replaces all data from a data entry or base64 encoded string.
- `merge=data1,data2,...`: Adds data from data entries or base64 encoded strings.
- `print`: Prints the data values.
- `save=name`: Saves the data of an object to a data entry.
- `set=type,key,value`: Sets a single value. Multiple `set=` can be used in the same command.
- `remove=key1,key2,...`: Removes data values.
- `par=key,value`: Sets a parameter for the data entry. Multiple `par=` can be used in the same command.

For changing multiple objects, check the `object` command documentation for the filtering parameters.

## Data entries

Data entries are the way to save and load object data.

Data is stored in the \BepInEx\config\data folder. When using a mod manager, the data is also read from the profile folder.

Data is always saved to the data.yaml file in the game directory. But you can organize the data by manually adding new files.

### What is data?

Information of each object is saved as key-value pairs.

The game supports the following data types:

- strings: Text values. For example "Hello".
- floats: Decimal values. For example 1.0 or 1.5.
- ints: Integer values. For example 1 or 5.
- longs: Long integer values. Used for timestamps which are too large for regular integers.
- vecs: 3d vector values. Used for positions and scales.
- quats: Quaternion values. Used for rotations.
- bytes: Raw array of bytes. These are not human readable.

Each data type has its own values. It's important to use the correct type, otherwise the game won't find the data value.

### Data properties

- name: Name of the data entry. Must be unique across all files.
- ints: List of integer values.
- floats: List of decimal values.
- strings: List of text values.
- longs: List of long integer values.
- vecs: List of vector values (x,z,y).
  - Pay attention to the order of the coordinates.
  - Most of my mods use x,z,y order because y is height.
  - Other mod authors probably use x,y,z order.
- quats: List of quaternion values as vector (y,x,z).
  - Pay attention to the order of the coordinates.
  - Most of my mods use y,x,z order because y is rotation around height axis.
- bytes: List of byte values as base64 encoded strings.
- bools: List of boolean values (true or false).
  - Internally these are stored as integers (0 = false, 1 = true).
- hashes: List of hashed values. These are strings that are saved as integers.
  - Internally these are stored as integers.
- items: List of items.
  - Internally the list is converted to base64 encoded string and saved as "items".
  - If you use this, remove the "items" from strings.
  - See section "Loot generation" for more information.

## Dynamic data entries

This section is for advanced users and is not required for basic usage.

Data entries that you get with `object save` are static. Loading them back to the game always gives the same result.

However you can create data entries that have a random result or are based on parameters.

### Parameters

Data entries can be parametrized with the `par=` parameter.

This can be useful when you want to change a few values of a big data entry.

```yaml
- data: leveler
  ints:
  - level, <level>
```

`data load=leveler par=level,2` sets the level to 2.

### Target object

Parameters `x`, `y` and `z` are automatically read from the target object.

```yaml
- data: someData
  vecs:
  - spawnpoint, "<x>,<z>,<y>"
```

Parameters containing `datatype_` are automatically read from the target object

```yaml
- data: someData
  floats:
  - RandomSkillFactor, <int_level>
```

### Expressions

For numerical values, simple calculations can be used.

```yaml
- data: leveler
  ints:
  - level, <level>
  floats:
  - RandomSkillFactor, 0.5 * <level>
```

### Lists

Multiple values can be used to randomize the result.

```yaml
- data: randomLeveler
  ints:
  - level, 1,2,3
  hashes:
  - ShoulderItem, CapeDeerHide,CapeLox
```

Special parameter `<none>` can be used to set no value.

No value is also set if the numerical value is not valid.

```yaml
- data: someData
  hashes:
  - ShoulderItem, CapeDeerHide,CapeLox,<none>
```

### Ranges

Ranges can be used to randomize numerical values.

```yaml
- data: randomLeveler
  ints:
  - level, 1;3
  health:
  - health, 100;1000;50
  
- data: leveler
  ints:
  - level, 1;<level>
  health:
  - health, 100;1000;50;<value>*<level>
```

The format is `min;max;step;expression`.

For example:

- `1;2`: Random number between 1 and 2.
- `1;2;0.5`: Random number of 1, 1.5 or 2.
- `1;2;0.5;2*<value>`: Random number of 2, 3 or 4.s

## Value entries

Value entries can be added to the same file as the data entries. Separate files can also be used to organize the values.

### Default parameters

```yaml
- value: level, 3

- data: leveler
  ints:
  - level, <level>
```

`data load=leveler` sets the level to 3.

`data load=leveler par=level,2` sets the level to 2.

### Values can be used in parameters

```yaml
- value: mag, <color=#FF00FF>
- value: cyan, <color=#00FFFF>

- data: texter
  strings:
  - RuneStone.m_text, <text>
```

`data load=texter par=text,"<mag>some text</color>"` colors the text without typing the color code.

### Parameters can be used in values

```yaml
- value: textMag, <color=#FF00FF><text></color>

- data: texter
  strings:
  - RuneStone.m_text, <textMag>
```

`data load=texter par=text,"some text"` colors the text automatically.

### Multiple parameter values

Value groups randomly select one of the values. Otherwise they work like normal values.

```yaml
- valueGroup: randomLevel
  values:
  - 1
  - 2
  - 3

- data: leveler
  ints:
  - level, <randomLevel>
```

## Loot generation

Chest items have special support because the base64 encoded string is impossible to edit.

For example:

```yaml
- name: Chest
  items:
  - pos: 2, 1
    prefab: MagicallyStuffedShroom
    stack: 1-20
    durability: 100
    crafterName: Thor
  - pos: 4, 1
    prefab: SilverOre, CopperOre
    durability: 100
  - pos: 0, 0
    prefab: SwordCheat
    durability: 50-100
```

Item properties:

- pos: Position of the item in the chest (x,y).
- chance: Chance for the item to appear (from 0.0 to 1.0).
- prefab: Name of the item.
- stack: Amount of the item.
- quality: Level of the item.
- variant: Variant of the item (for example shield styles).
- durability: Durability of the item.
- crafterID: Character ID of the crafter.
- crafterName: Name of the crafter.
- worldLevel: World level of the item.
  - World level is like New Game+, but it's not used in the game.
  - It's always 0 unless manually changed with a global key.
- equipped: true/false, if the item is currently equipped.
  - This doesn't make sense for chest items, but player inventory uses the same system.
- pickedUp: true/false, if the item has been picked up.
  - This probably just affects auto pickup, so is not useful for chest items.
- customData: Custom data of the item.
  - This is a list of key-value pairs.
  - These are only used by modded items.

### Random loot

The items can be randomly generated by leaving out the "pos" property.

For example:

```yaml
- name: Chest
  itemAmount: 2
  items:
  - prefab: CopperOre
    chance: 0.5
  - prefab: TinOre
    chance: 0.5
  - prefab: IronOre
    chance: 0.1
    
- name: BigChest
  itemAmount: 5;10
  containerSize: 6,3
  items:
  - prefab: CopperOre
  # ...
```

For this there are two properties:

- containerSize: Optional. Size of the chest (x,y).
- itemAmount: Optional. Amount of items to generate.

Container size is only used for the item placement and doesn't have to match the actual chest size.

Default size is 4,2 which is enough to generate up to 8 items.

If item amount is set, X amount of items are selected from the item list based on their chances. Each item can only appear once.

If item amount is not set, each item is rolled separately in the same order as they appear on the list.

## Compatiblity with old data

Some mods like Spawner Tweaks require the data as base64 encoded strings.

For this purpose, there is `data_raw [name]` command that prints the data as base64 encoded string.

## Pattern matching

`data` and `object` commands have parameters `match` and `unmatch`.

These parameters can be used to filter the affected objects based on their data.

### Default values

If the object is missing the data value, its default value is used.

For numerical values, the default value is 0. For strings, the default value is an empty string.

This mostly works as expected but there are special cases like creature level.

For creatures, both levels 0 and 1 are considered as level 1. So simply matching with either of the value may not work correctly.

### Invalid values

For dynamic data entries, invalid numerical values or the value `<none>` are ignored.

Ignored values are always a match for `match` and never a match for `unmatch`, as if they never existed on the data entry.

### Lists and ranges

For lists and ranges, the object must match at least one of the values.

If all list values are invalid, the list is ignored. If anyone of the range parmaters is invalid, the range is ignored.
