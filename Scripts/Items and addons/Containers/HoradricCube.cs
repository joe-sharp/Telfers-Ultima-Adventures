using System;
using System.Collections.Generic;
using Server;
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

        public override void OnSingleClick(Mobile from)
        {
            if (Items.Count == 0)
            {
                LabelTo(from, "This cube can transmute harvest tools. Double-click to learn more.");
            }
            else
            {
                Transmute(from);
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.PrivateOverheadMessage(0, 0xB08, false, "Place harvest tools into the cube and single-click to transmute them.", from.NetState);
            base.OnDoubleClick(from); // Opens the container
        }

        private void Transmute(Mobile from)
        {
            Dictionary<Type, BaseHarvestTool> toolMap = new Dictionary<Type, BaseHarvestTool>();
            Dictionary<Type, int> usesMap = new Dictionary<Type, int>();

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
                        tool.Delete();
                    }
                }
            }

            foreach (KeyValuePair<Type, BaseHarvestTool> kvp in toolMap)
            {
                kvp.Value.UsesRemaining = usesMap[kvp.Key];
            }

            if (toolMap.Count > 0)
            {
                from.PrivateOverheadMessage(0, 0xB08, false, "The transmutation is complete. The tools have been combined.", from.NetState);
            }
            else
            {
                from.PrivateOverheadMessage(0, 0xB08, false, "There are no valid harvest tools in the cube to transmute.", from.NetState);
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
}