//Copied directly from AlchemyPouch.cs and modified for ore/ingots  Pretty sure the IsReagent check isn't gonna find ore/gem/ingots 
//because they aren't reagents, but I can't test it.

using System;
using Server;

namespace Server.Items
{
	[Flipable( 0x2c7e, 0x1e3f )]
    public class CoinPouch : LargeSack
    {
		public override int MaxWeight{ get{return 800; } }

		[Constructable]
		public CoinPouch() : base()
		{
			Weight = 1.0;
			//MaxWeight = 800;
			MaxItems = 50;
			Name = "coin pouch";
			Hue = 0x31;
		}

        public override bool CanAdd( Mobile from, Item item)
		{
            if ( item is DDCopper || 
				 item is DDSilver || 
				 item is DDJewels ||
				 item is DDXormite ||
				 item is DDGemstones ||
				 item is Gold ||
				 item is DDGoldNuggets )
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
                from.SendMessage("You can't add a container in this pouch.");
			}
			else
            {
                from.SendMessage("This coin pouch is for currency only.");
            }

            return false;
        }

        public override bool OnDragDrop( Mobile from, Item dropped )
        {
			if (CanAdd(from, dropped)) return base.OnDragDrop(from, dropped);
				
			if ( dropped is Container )
			{
                from.SendMessage("You can't add a container in this pouch.");
			}
			else
            {
                from.SendMessage("This coin pouch is for currency only.");
            }

            return false;
        }

        public CoinPouch( Serial serial ) : base( serial )
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
			Name = "coin pouch";
		}

		public override int GetTotal(TotalType type)
        {
			if (type != TotalType.Weight)
				return base.GetTotal(type);
			else
			{
				return (int)(TotalItemWeights() * (0.5));
			}
        }

		public override void UpdateTotal(Item sender, TotalType type, int delta)
        {
            if (type != TotalType.Weight)
                base.UpdateTotal(sender, type, delta);
            else
                base.UpdateTotal(sender, type, (int)(delta * (0.5)));
        }

		private double TotalItemWeights()
        {
			double weight = 0.0;

			foreach (Item item in Items)
				weight += (item.Weight * (double)(item.Amount));

			return weight;
        }
	}
}