using System;
using Server;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Testing.Quests
{
    public class TheOldGrandfather : QuestItem
    {
        [Constructable]
        public TheOldGrandfather() : base(0x13B9)
        {
            Name = "The Grandfather";
            Hue = 2967;
        }

        public override bool CanDrop(PlayerMobile pm)
        {
            // Allow drop only if the target container is a HoradricCube
            return pm != null && pm.Target is HoradricCube;
        }

        public TheOldGrandfather(Serial serial) : base(serial) { }

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