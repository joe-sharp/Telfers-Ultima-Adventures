using System;
using Server;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Testing.Quests
{
    public class SoulstoneShard : QuestItem
    {
        [Constructable]
        public SoulstoneShard() : base(0xF26)
        {
            Name = "Soulstone Shard";
            Hue = 2848;
        }

        public override bool CanDrop(PlayerMobile pm)
        {
            // Allow drop only if the target container is a HoradricCube
            return pm != null && pm.Target is HoradricCube;
        }

        public SoulstoneShard(Serial serial) : base(serial) { }

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