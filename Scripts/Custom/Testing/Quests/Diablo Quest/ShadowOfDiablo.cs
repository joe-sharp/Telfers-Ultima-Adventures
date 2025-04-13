using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Testing.Quests
{
    public class ShadowOfDiablo : Demon
    {
        [Constructable]
        public ShadowOfDiablo() : base()
        {
            Name = "Shadow of Terror";
            Body = 137;
            Hue = 2769;
            SetHits( 279, 304 );
            AddItem( new SoulstoneShard() );
        }

        public ShadowOfDiablo(Serial serial) : base(serial) { }

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