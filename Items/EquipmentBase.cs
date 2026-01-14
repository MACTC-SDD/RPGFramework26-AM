using RPGFramework;

public class EquipmentBase
{
    private object? slots;

    public void Unequip(EquipmentSlot slot)
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
            if (item is not Weapon weapon || !weapon.IsTwoHanded)
            {
                slots.Remove(slot);
            }
            else
            {
                slots.Remove(EquipmentSlot.MainHand);
                slots.Remove(EquipmentSlot.OffHand);
            }
        }
        //the below is the code equipment weapon/Armor
        public class Player
        {
            public Equipment Equipment { get; } = new Equipment();

            public int Strength { get; private set; }
            public int Defense { get; private set; }

            public void RecalculateStats()
            {
                Strength = 0;
                Defense = 0;

                foreach (Item item in Equipment.GetEquippedItems())
                {
                    Strength += item.StrengthBonus;
                    Defense += item.DefenseBonus;
                }
            }
        }

        private IEnumerable<Item> GetEquippedItems()
        {
            throw new NotImplementedException();
        }

        public abstract class Item
        {
            public EquipmentSlot? Slot { get; protected set; }

            public(EquipmentSlot? slot)
            {
                Slot = slot;
            }

            public int StrengthBonus { get; set; }
            public int DefenseBonus { get; set; }
            public object? Name { get; internal set; }
        }
        public class Weapon : Item
        {
            public bool IsTwoHanded { get; set; }

            public Weapon(bool twoHanded)
            {
                IsTwoHanded = twoHanded;
                Slot = EquipmentSlot.MainHand;
            }
        }
        public class Helmet : Item
        {
            public Helmet()
            {
                Slot = EquipmentSlot.Head;
                DefenseBonus = 5;
            }
        }
        public void EquipItem(Player player, Item item)
        {
            bool equipped = player.Equipment.Equip(item);

            if (equipped)
            {
                player.RecalculateStats();
                Console.WriteLine($"{item.Name} equipped!");
            }
        }

        private bool Equip(Item item)
        {
            throw new NotImplementedException();
        }

        public void UnequipHelmet(Player player)
        {
            player.Equipment.Unequip(EquipmentSlot.Head);
            player.RecalculateStats();
        }
    }
}