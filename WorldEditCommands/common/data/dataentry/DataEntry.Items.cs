// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.

namespace Data;

public partial class DataEntry
{
  public byte[]? CreateItemData(Functions f, ZDO? zdo) => zdo == null ? null : Item?.Create(f, zdo);

  public byte[]? CreateInventory(Functions f, ZDO? zdo)
  {
    if (Items == null || Items.Count == 0) return null;
    var size = ContainerSize ?? ZdoHelper.GetInventorySize(this, f, zdo);
    return ItemValue.LoadItemBytes(f, Items, size, ItemAmount?.Get(f) ?? 0);
  }
}
