using RPGFramework;

public class EquipmentBase
{
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
    }
}