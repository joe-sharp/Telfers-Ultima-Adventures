using System;
using System.Collections.Generic;
using Server;
using Server.ContextMenus;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
    public class HoradricCube : BaseContainer
    {
        public override int ArtifactRarity{ get{ return 0xD2; } } // 210

        [Constructable]
        public HoradricCube() : base(0x5D5)
        {
            Name = "Horadric Cube";
            GumpID = 0x976;
            Hue = 0xB08;
			Light = LightType.Circle150;
            Weight = 1.0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from); // Opens the container
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            if (from.InRange(this.GetWorldLocation(), 2))
            {
                list.Add(new TransmuteContainer(this, from));
            }
        }

        internal void Transmute(Mobile from)
        {
            Dictionary<Type, BaseHarvestTool> toolMap = new Dictionary<Type, BaseHarvestTool>();
            Dictionary<Type, int> usesMap = new Dictionary<Type, int>();
            List<Item> itemsToDelete = new List<Item>(); // Collect items to delete
            Dictionary<Type, int> stackableItems = new Dictionary<Type, int>(); // Track stackable items

            if (from == null || !from.CheckAlive())
            {
                return;
            }
            if (Items.Count == 0)
            {
                from.PrivateOverheadMessage(0, 0xAD4, false, "I combine charges from many tools onto one, and make clean stacks", from.NetState);
                from.PrivateOverheadMessage(0, 0xAD4, false, "Place multiple items of the same type into the cube and use the context menu to transmute them.", from.NetState);
                return;
            }

            foreach (Item item in Items)
            {
                // Handle harvest tools
                BaseHarvestTool tool = item as BaseHarvestTool;
                if (tool != null)
                {
                    Type toolType = tool.GetType();

                    if (!toolMap.ContainsKey(toolType))
                    {
                        toolMap[toolType] = tool;
                        usesMap[toolType] = tool.UsesRemaining;
                    }
                    else
                    {
                        usesMap[toolType] += tool.UsesRemaining;
                        itemsToDelete.Add(tool); // Add to delete list
                    }
                    continue;
                }

                // Handle stackable items
                if (item.Stackable)
                {
                    Type itemType = item.GetType();

                    if (!stackableItems.ContainsKey(itemType))
                    {
                        stackableItems[itemType] = item.Amount;
                    }
                    else
                    {
                        stackableItems[itemType] += item.Amount;
                    }
                    itemsToDelete.Add(item); // Add to delete list
                }
            }

            // Delete items after iteration
            foreach (Item item in itemsToDelete)
            {
                item.Delete();
            }

            // Combine harvest tools
            foreach (KeyValuePair<Type, BaseHarvestTool> kvp in toolMap)
            {
                kvp.Value.UsesRemaining = usesMap[kvp.Key];
            }

            // Create even stacks of 60000 for stackable items
            foreach (KeyValuePair<Type, int> kvp in stackableItems)
            {
                int totalAmount = kvp.Value;
                Type itemType = kvp.Key;

                while (totalAmount > 0)
                {
                    int stackAmount = Math.Min(totalAmount, 60000);
                    totalAmount -= stackAmount;

                    Item newItem = (Item)Activator.CreateInstance(itemType);
                    newItem.Amount = stackAmount;
                    DropItem(newItem); // Add the new stack to the cube
                }
            }

            // Send feedback to the player
            if (toolMap.Count > 0 || stackableItems.Count > 0)
            {
                from.PrivateOverheadMessage(0, 0xAD4, false, "Transmutation Complete!", from.NetState);
                from.PlaySound( 0x1F5 );
            }
            else
            {
                from.PrivateOverheadMessage(0, 0xAD4, false, "There are no valid items in the cube to transmute.", from.NetState);
            }
        }

        public HoradricCube(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class TransmuteContainer : ContextMenuEntry
    {
        private HoradricCube m_Cube;
        private Mobile m_From;

        public TransmuteContainer(HoradricCube cube, Mobile from) : base(6190, 2) // Use string literal
        {
            m_Cube = cube;
            m_From = from;
        }
        public override void OnClick()
        {
            if (m_From.InRange(m_Cube.GetWorldLocation(), 2))
            {
                m_Cube.Transmute(m_From);
            }
            else
            {
                m_From.SendLocalizedMessage(500446); // That is too far away.
            }
        }
    }
}