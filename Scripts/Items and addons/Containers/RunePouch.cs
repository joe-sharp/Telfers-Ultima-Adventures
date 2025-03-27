using System;
using Server;

namespace Server.Items
{
	[Flipable( 0x1C10, 0x1CC6 )]
    public class RunePouch : LargeSack
    {
		[Constructable]
		public RunePouch() : base()
		{
			Weight = 1.0;
			MaxItems = 100;
			Name = "rune rucksack";
			Hue = 0x89F;
		}

        public override bool CanAdd( Mobile from, Item item)
		{
            if ( Server.Misc.MaterialInfo.IsReagent( item ) ||
						item is Key ||
						item is Pouch ||
						item is RecallRune ||
						item is Runebook ||
						item is RunePouch )
			{
				return true;
			}

			return false;
		}

		public override bool OnDragDropInto( Mobile from, Item dropped, Point3D p )
        {
			if (CanAdd(from, dropped)) return base.OnDragDropInto(from, dropped, p);

			if ( dropped is Container )
			{
                from.SendMessage("You can only use another rune rucksack within this sack.");
			}
			else
			{
				from.SendMessage("This rucksack is for runes and runebooks.");
			}

			return false;
        }

        public override bool OnDragDrop( Mobile from, Item dropped )
        {
			if (CanAdd(from, dropped)) return base.OnDragDrop(from, dropped);

			if ( dropped is Container)
			{
                from.SendMessage("You can only use another rune rucksack within this sack.");
			}
			else
			{
				from.SendMessage("This rucksack is for runes and runebooks.");
			}

			return false;
        }

        public RunePouch( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			Weight = 1.0;
			MaxItems = 50;
			Name = "alchemy rucksack";
		}

		public override int GetTotal(TotalType type)
        {
			switch ( type )
			{
				case TotalType.Items:
					return 0;

				case TotalType.Weight:
					return 0;
			}

			return base.GetTotal( type );
        }

		public override void UpdateTotal(Item sender, TotalType type, int delta)
		{
			switch ( type )
			{
				case TotalType.Items:
					base.UpdateTotal(sender, type, 0);
					break;

				case TotalType.Weight:
					base.UpdateTotal(sender, type, 0);
					break;
			}

			base.UpdateTotal( sender, type, delta );
		}
	}
}