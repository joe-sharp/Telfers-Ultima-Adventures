using System;
using Server;

namespace Server.Items
{
	[Flipable( 0x1C10, 0x1CC6 )]
    public class RunePouch : LargeSack
    {
		private int m_ContainedItems; // Attribute to track contained items.

		[CommandProperty(AccessLevel.GameMaster)]
		public int ContainedItems
		{
			get { return m_ContainedItems; }
			private set { m_ContainedItems = value; }
		}

		[Constructable]
		public RunePouch() : base()
		{
			Weight = 1.0;
			MaxItems = 110;
			Name = "rune rucksack";
			Hue = 0x883;
			m_ContainedItems = 0; // Initialize the count.
		}

		public override bool CanAdd(Mobile from, Item item)
		{
			// Check if we would exceed the MaxItems limit
			if (m_ContainedItems >= MaxItems)
			{
				from.SendMessage("This rucksack cannot hold any more items.");
				return false; // Do not send a message here to avoid duplication.
			}

			// Check if we would exceeed the Parent's MaxItems limit
			RunePouch parentPouch = this.Parent as RunePouch;
			if (parentPouch != null && m_ContainedItems + parentPouch.ContainedItems >= parentPouch.MaxItems)
			{
				from.SendMessage("The rucksack inside this one cannot hold any more items.");
				return false;
			}

			RunePouch runepouch = item as RunePouch;
			if (runepouch != null)
			{
				// Enforce single layer nesting of RunePouches
				if (IsInvalidRunePouchNesting(runepouch))
				{
					from.SendMessage("You may not nest rune rucksacks more than once.");
					return false;
				}

				// Check if adding the items from the RunePouch exceeds MaxItems
				if (runepouch.ContainedItems + m_ContainedItems >= MaxItems)
				{
					from.SendMessage("This rucksack cannot hold any more items.");
					return false;
				}
			}

			if (item is Key ||
				item is RecallRune ||
				item is Runebook ||
				item is RunePouch)
			{
				return true;
			}

			if (item is Container)
			{
				from.SendMessage("You can only use another rune rucksack within this sack.");
			}
			else
			{
				from.SendMessage("This rucksack is for runes and runebooks.");
			}

			return false;
		}

		private bool IsInvalidRunePouchNesting(RunePouch runepouch)
		{

			// Check if the item being added is a RunePouch
			if (runepouch.Parent is RunePouch)
			{
				return true;
			}

			// Check if this RunePouch is already inside a RunePouch
			if (this.Parent is RunePouch)
			{
				return true;
			}

			// Check if the RunePouch contains another RunePouch
			foreach (Item subItem in runepouch.Items)
			{
				if (subItem is RunePouch)
				{
					return true;
				}
			}

			return false;
		}

		public override bool OnDragDropInto(Mobile from, Item dropped, Point3D p)
		{
			if (CanAdd(from, dropped))
			{
				return base.OnDragDropInto(from, dropped, p);
			}

			return false;
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (CanAdd(from, dropped))
			{
				return base.OnDragDrop(from, dropped);
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
			writer.Write(m_ContainedItems); // Serialize ContainedItems
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_ContainedItems = reader.ReadInt(); // Deserialize ContainedItems

			// Recalculate the total items to ensure accuracy
			m_ContainedItems = 0;
			foreach (Item item in Items)
			{
				m_ContainedItems += GetTotalItems(item);
			}

			Weight = 1.0;
			MaxItems = 110;
			Name = "rune rucksack";
		}

		private int GetTotalItems(Item item)
		{
			Container container = item as Container;
			if (container != null)
			{
				int total = 0;
				foreach (Item subItem in container.Items)
				{
					total += GetTotalItems(subItem); // Recursively count items in nested containers
				}
				return total;
			}
			return 1; // Count the item itself
		}

		public override int GetTotal(TotalType type)
        {
			switch (type)
			{
				case TotalType.Items:
					return 0; // Always return 0 for TotalType.Items.

				case TotalType.Weight:
					return 0;
			}

			return base.GetTotal(type);
        }

		public override void UpdateTotal(Item sender, TotalType type, int delta)
		{
			switch (type)
			{
				case TotalType.Items:
					// If the sender is a RunePouch, account for its ContainedItems
					RunePouch runepouch = sender as RunePouch;
					if (runepouch != null)
					{
						if (delta > 0)
						{
							m_ContainedItems = Math.Max(0, Math.Min(MaxItems, m_ContainedItems + runepouch.ContainedItems + delta));
						}
						else
						{
							m_ContainedItems = Math.Max(0, Math.Min(MaxItems, m_ContainedItems - runepouch.ContainedItems + delta));
						}
					}
					else
					{
						m_ContainedItems = Math.Max(0, Math.Min(MaxItems, m_ContainedItems + delta));
					}

					base.UpdateTotal(sender, type, 0); // Prevent affecting the player's total items.
					break;

				case TotalType.Weight:
					base.UpdateTotal(sender, type, 0); // Prevent affecting the player's total weight.
					break;
			}

			base.UpdateTotal(sender, type, delta);
		}
	}
}