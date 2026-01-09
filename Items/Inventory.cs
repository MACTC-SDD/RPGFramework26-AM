using System;
using System.Collections.Generic;
using System.Text;



namespace RPGFramework.Items
{
    internal class Inventory
    {
        public List<string> InventorySlots { get; set; } = new List<string>();

        public void SetSlotValue(int index, string value)
        {
            if (index < 0 || index >= InventorySlots.Count)
                throw new IndexOutOfRangeException("Inventory Is Full");

            InventorySlots[index] = value;
        }
    }

}
public enum EquipmentSlot
{
    Head,
    Chest,
    Legs,
    Back,
    MainHand,
    OffHand
}


public class Equipment : EquipmentBase
{
    private Dictionary<EquipmentSlot, Item> slots =
        new Dictionary<EquipmentSlot, Item>();

    private bool EquipToSlot(Item item, EquipmentSlot slot)
    {
        if (slots.ContainsKey(slot))
            Unequip(slot);

        slots[slot] = item;
        return true;
    }

    public void Unequip(EquipmentSlot slot)
    {
        if (!slots.ContainsKey(slot))
            return;

        Item item = slots[slot];

        // If two-handed weapon, remove from both hands
        if (item is Weapon weapon && weapon.IsTwoHanded)
        {
            slots.Remove(EquipmentSlot.MainHand);
            slots.Remove(EquipmentSlot.OffHand);
        }
        else
        {
            slots.Remove(slot);
        }
    }

    public RPGFramework.Item GetItem(EquipmentSlot slot)
    {
        slots.TryGetValue(slot, out Item item);
        return item;
    }
}


