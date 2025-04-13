using System;
using Server;
using Server.ContextMenus;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Custom.Testing.Quests
{
    public class DeckardCain : Sage
    {

        [Constructable]
        public DeckardCain()
        {
            Name = "Deckard Cain";
            Title = "the Elder";
            Blessed = true;
            Body = 0x190;
            Female = false;
            Hue = 0x3FF;
            HairItemID = 0x203E;
            HairHue = 2040;
            FacialHairItemID = 0x2041;
            FacialHairHue = 2040;
            SetSkill(SkillName.Camping, 100.0, 100.0);
            SetSkill(SkillName.Forensics, 120.0, 120.0);
            SetSkill(SkillName.Healing, 120.0, 120.0);
            SetSkill(SkillName.ItemID, 120.0, 120.0);
            SetSkill(SkillName.SpiritSpeak, 120.0, 120.0);
        }

        public override VendorShoeType ShoeType{ get{ return VendorShoeType.Sandals; } }

		public override bool ClickTitle{ get{ return false; } } // Do not display title in OnSingleClick

		public virtual bool HealsYoungPlayers{ get{ return true; } }

        public override void InitOutfit()
        {
            AddItem(new Robe(696)); // Grey
            AddItem(new GnarledStaff());
			AddItem( new Sandals( GetShoeHue() ) );
            AddItem( new ShortPants( GetRandomHue() ) );

            int money1 = 30;
            int money2 = 120;

            double w1 = money1 * (MyServerSettings.GetGoldCutRate( this, null ) * .01);
            double w2 = money2 * (MyServerSettings.GetGoldCutRate( this, null ) * .01);

            money1 = (int)w1;
            money2 = (int)w2;

			PackGold( money1, money2 );
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Speech.ToLower().Contains("soulstone"))
            {
                Say("Long ago, the Soulstones were given to the Horadrim to contain the Prime Evils.");
                Say("I now know that even these holy artifacts were no match for Diablo's power.");
                Say("If you can recover the Soulstone Shard from the Shadow of Diablo, I will reward you with a device long passed down by the Horadrim.");
            }
            else if (e.Speech.ToLower().Contains("grandfather"))
            {
                Say("An unbroken lineage of unwavering strength.");
            }

            base.OnSpeech(e);
        }

        public DeckardCain(Serial serial) : base(serial) { }

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

        public class DiabloEntry : ContextMenuEntry
        {
            private Mobile m_Mobile;
            private Mobile m_Giver;
            
            public DiabloEntry(Mobile from, Mobile giver) : base(6146, 3)
            {
                m_Mobile = from;
                m_Giver = giver;
            }

            public override void OnClick()
            {
                if (!(m_Mobile is PlayerMobile))
                    return;
                
                PlayerMobile mobile = (PlayerMobile)m_Mobile;
                m_Mobile.SendMessage("Stay a while and listen.");
                // if (!mobile.HasGump(typeof(DiabloSpawnGump)))
                // {
                //     mobile.SendGump(new DiabloSpawnGump(mobile));
                    
                // }
            }
        }
        
        public override bool OnDragDrop(Mobile from, Item dropped)
        {                  
            Mobile m = from;
            PlayerMobile mobile = m as PlayerMobile;

            if (mobile != null)
            {
                if (dropped is SoulstoneShard)
                {
                    dropped.Delete();
                    Say("You've recovered the Soulstone Shard! My thanks, please accept this ancient artifact of the Horadrim");
                    mobile.AddToBackpack(new HoradricCube());
                    
                    return true;
                }
                else if (dropped is TheOldGrandfather)
                {
                    Say("You have defeated Diablo! Thank you, hero!");
                    return true;
                }
                else
                {
                    // Call Sage's implementation if the item isn't handled here
                    return base.OnDragDrop(from, dropped);
                }
            }
            return false;
        }
    }
}