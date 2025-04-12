using System;
using System.Collections.Generic;
using Server;
using Server.ContextMenus;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
    public class HoradricCube : Container
    {
        [Constructable]
        public HoradricCube() : base(0x5D5)
        {
            Name = "Horadric Cube";
            Weight = 1.0;
            GumpID = 0x976;
            Hue = 0xB08;
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.PrivateOverheadMessage(0, 0xAD4, false, "Place harvest tools into the cube and use the context menu to transmute them.", from.NetState);
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

            if (from == null || !from.CheckAlive())
            {
                return;
            }
            if (Items.Count == 0)
            {
                from.PrivateOverheadMessage(0, 0xAD4, false, "This cube can transmute harvest tools. Double-click to learn more.", from.NetState);
                return;
            }

            foreach (Item item in Items)
            {
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
                }
            }

            // Delete items after iteration
            foreach (Item item in itemsToDelete)
            {
                item.Delete();
            }

            foreach (KeyValuePair<Type, BaseHarvestTool> kvp in toolMap)
            {
                kvp.Value.UsesRemaining = usesMap[kvp.Key];
            }

            if (toolMap.Count > 0)
            {
                from.PrivateOverheadMessage(0, 0xAD4, false, "The transmutation is complete. The tools have been combined.", from.NetState);
            }
            else
            {
                from.PrivateOverheadMessage(0, 0xAD4, false, "There are no valid harvest tools in the cube to transmute.", from.NetState);
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

        public TransmuteContainer(HoradricCube cube, Mobile from) : base(6189, 2) // Use string literal
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