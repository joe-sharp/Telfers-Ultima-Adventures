using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Commands;
using Server.Misc;

namespace Server.Engines.Craft
{
	public enum ConsumeType
	{
		All, Half, None
	}

	public interface ICraftable
	{
		int OnCraft( int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue );
	}

	public class CraftItem
	{
		private CraftResCol m_arCraftRes;
		private CraftSkillCol m_arCraftSkill;
		private Type m_Type;

		private string m_GroupNameString;
		private int m_GroupNameNumber;

		private string m_NameString;
		private int m_NameNumber;
		
		private int m_Mana;
		private int m_Hits;
		private int m_Stam;

		private bool m_UseAllRes;

		private bool m_NeedHeat;
		private bool m_NeedOven;
		private bool m_NeedMill;

		private bool m_UseSubRes2;

		private bool m_ForceNonExceptional;

		public bool ForceNonExceptional
		{
			get { return m_ForceNonExceptional; }
			set { m_ForceNonExceptional = value; }
		}
	

		private Expansion m_RequiredExpansion;

		public Expansion RequiredExpansion
		{
			get { return m_RequiredExpansion; }
			set { m_RequiredExpansion = value; }
		}

		private Recipe m_Recipe;

		public Recipe Recipe
		{
			get { return m_Recipe; }
		}

		public void AddRecipe( int id, CraftSystem system )
		{
			if( m_Recipe != null )
			{
				Console.WriteLine( "Warning: Attempted add of recipe #{0} to the crafting of {1} in CraftSystem {2}.", id, this.m_Type.Name, system );
				return;
			}

			m_Recipe = new Recipe( id, system, this );
		}


		private static Dictionary<Type, int> _itemIds = new Dictionary<Type, int>();
		
		public static int ItemIDOf( Type type ) {
			int itemId;

			if ( !_itemIds.TryGetValue( type, out itemId ) ) {

				if ( itemId == 0 ) {
					object[] attrs = type.GetCustomAttributes( typeof( CraftItemIDAttribute ), false );

					if ( attrs.Length > 0 ) {
						CraftItemIDAttribute craftItemID = ( CraftItemIDAttribute ) attrs[0];
						itemId = craftItemID.ItemID;
					}
				}

				if ( itemId == 0 ) {
					Item item = null;

					try { item = Activator.CreateInstance( type ) as Item; } catch { }

					if ( item != null ) {
						itemId = item.ItemID;
						item.Delete();
					}
				}

				_itemIds[type] = itemId;
			}

			return itemId;
		}

		public CraftItem( Type type, TextDefinition groupName, TextDefinition name )
		{
			m_arCraftRes = new CraftResCol();
			m_arCraftSkill = new CraftSkillCol();

			m_Type = type;

			m_GroupNameString = groupName;
			m_NameString = name;

			m_GroupNameNumber = groupName;
			m_NameNumber = name;
		}

		public void AddRes( Type type, TextDefinition name, int amount )
		{
			AddRes( type, name, amount, "" );
		}

		public void AddRes( Type type, TextDefinition name, int amount, TextDefinition message )
		{
			CraftRes craftRes = new CraftRes( type, name, amount, message );
			m_arCraftRes.Add( craftRes );
		}

		public void AddSkill( SkillName skillToMake, double minSkill, double maxSkill )
		{
			CraftSkill craftSkill = new CraftSkill( skillToMake, minSkill, maxSkill );
			m_arCraftSkill.Add( craftSkill );
		}

		public int Mana
		{
			get { return m_Mana; }
			set { m_Mana = value; }
		}

		public int Hits
		{
			get { return m_Hits; }
			set { m_Hits = value; }
		}

		public int Stam
		{
			get { return m_Stam; }
			set { m_Stam = value; }
		}

		public bool UseSubRes2
		{
			get { return m_UseSubRes2; }
			set { m_UseSubRes2 = value; }
		}

		public bool UseAllRes
		{
			get { return m_UseAllRes; }
			set { m_UseAllRes = value; }
		}

		public bool NeedHeat
		{
			get { return m_NeedHeat; }
			set { m_NeedHeat = value; }
		}

		public bool NeedOven
		{
			get { return m_NeedOven; }
			set { m_NeedOven = value; }
		}

		public bool NeedMill
		{
			get { return m_NeedMill; }
			set { m_NeedMill = value; }
		}
		
		public Type ItemType
		{
			get { return m_Type; }
		}

		public string GroupNameString
		{
			get { return m_GroupNameString; }
		}

		public int GroupNameNumber
		{
			get { return m_GroupNameNumber; }
		}

		public string NameString
		{
			get { return m_NameString; }
		}

		public int NameNumber
		{
			get { return m_NameNumber; }
		}

		public CraftResCol Resources
		{
			get { return m_arCraftRes; }
		}

		public CraftSkillCol Skills
		{
			get { return m_arCraftSkill; }
		}

		public bool ConsumeAttributes( Mobile from, ref object message, bool consume )
		{
			bool consumMana = false;
			bool consumHits = false;
			bool consumStam = false;

			if ( Hits > 0 && from.Hits < Hits )
			{
				message = "You lack the required hit points to make that.";
				return false;
			}
			else
			{
				consumHits = consume;
			}

			if ( Mana > 0 && from.Mana < Mana )
			{
				message = "You lack the required mana to make that.";
				return false;
			}
			else
			{
				consumMana = consume;
			}

			if ( Stam > 0 && from.Stam < Stam )
			{
				message = "You lack the required stamina to make that.";
				return false;
			}
			else
			{
				consumStam = consume;
			}

			if ( consumMana )
				from.Mana -= Mana;

			if ( consumHits )
				from.Hits -= Hits;

			if ( consumStam )
				from.Stam -= Stam;

			return true;
		}

		#region Tables
		private static int[] m_HeatSources = new int[]
			{
				0x461, 0x48E, // Sandstone oven/fireplace
				0x92B, 0x96C, // Stone oven/fireplace
				0xDE3, 0xDE9, // Campfire
				0xFAC, 0xFAC, // Firepit
				0x184A, 0x184C, // Heating stand (left)
				0x184E, 0x1850, // Heating stand (right)
				0x398C, 0x399F,  // Fire field
				0x2DDB, 0x2DDC,	//Elven stove
				0x19AA, 0x19BB,	// Veteran Reward Brazier
				0x197A, 0x19A9, // Large Forge 
				0x0FB1, 0x0FB1, // Small Forge
				0x2DD8, 0x2DD8, // Elven Forge
				0x5321, 0x5322, 0x5323, 0x5324, 0x5325, 0x5326, 0x53A0, 0x53A1, 0x53A2, 0x53A3, 0x53A4, 0x53A5 // bonfire
			};

		private static int[] m_Ovens = new int[]
			{
				0x461, 0x46F, // Sandstone oven
				0x92B, 0x93F,  // Stone oven
				0x2DDB, 0x2DDC,	//Elven stove
				0x5363, 0x5364, 0x5365, 0x5366, 0x5367, 0x5368, 0x5369, 0x536A // stove
			};

		private static int[] m_Mills = new int[]
			{
				0x1920, 0x1921, 0x1922, 0x1923, 0x1924, 0x1295, 0x1926, 0x1928,
				0x192C, 0x192D, 0x192E, 0x129F, 0x1930, 0x1931, 0x1932, 0x1934
			};

		private static Type[][] m_TypesTable = new Type[][]
			{
				new Type[]{ typeof( Log ), typeof( Board ) },
				new Type[]{ typeof( AshLog ), typeof( AshBoard ) },
				new Type[]{ typeof( CherryLog ), typeof( CherryBoard ) },
				new Type[]{ typeof( EbonyLog ), typeof( EbonyBoard ) },
				new Type[]{ typeof( GoldenOakLog ), typeof( GoldenOakBoard ) },
				new Type[]{ typeof( HickoryLog ), typeof( HickoryBoard ) },
				new Type[]{ typeof( MahoganyLog ), typeof( MahoganyBoard ) },
				new Type[]{ typeof( DriftwoodLog ), typeof( DriftwoodBoard ) },
				new Type[]{ typeof( OakLog ), typeof( OakBoard ) },
				new Type[]{ typeof( PineLog ), typeof( PineBoard ) },
				new Type[]{ typeof( GhostLog ), typeof( GhostBoard ) },
				new Type[]{ typeof( RosewoodLog ), typeof( RosewoodBoard ) },
				new Type[]{ typeof( WalnutLog ), typeof( WalnutBoard ) },
				new Type[]{ typeof( PetrifiedLog ), typeof( PetrifiedBoard ) },
				new Type[]{ typeof( ElvenLog ), typeof( ElvenBoard ) },
				new Type[]{ typeof( Leather ), typeof( Hides ) },
				new Type[]{ typeof( SpinedLeather ), typeof( SpinedHides ) },
				new Type[]{ typeof( HornedLeather ), typeof( HornedHides ) },
				new Type[]{ typeof( BarbedLeather ), typeof( BarbedHides ) },
				new Type[]{ typeof( DinosaurLeather ), typeof( DinosaurHides ) },
				new Type[]{ typeof( AlienLeather ), typeof( AlienHides ) },
				new Type[]{ typeof( BlankMap ), typeof( BlankScroll ) },
				new Type[]{ typeof( Cloth ), typeof( UncutCloth ) },
				new Type[]{ typeof( CheeseWheel ), typeof( CheeseWedge ) },
				new Type[]{ typeof( Pumpkin ), typeof( SmallPumpkin ) },
				new Type[]{ typeof( WoodenBowlOfPeas ), typeof( PewterBowlOfPeas ) }
			};

		private static Type[] m_ColoredItemTable = new Type[]
			{
				typeof( BaseWeapon ), typeof( BaseArmor ), typeof( BaseClothing ),
				typeof( BaseJewel ), typeof( DragonBardingDeed )
			};

		private static Type[] m_ColoredResourceTable = new Type[]
			{
				typeof( BaseIngot ), typeof( BaseOre ),
				typeof( BaseLeather ), typeof( BaseHides ),
				typeof( UncutCloth ), typeof( Cloth ),
				typeof( BaseGranite ), typeof( BaseScales ),
				typeof( BaseLog ), typeof( BaseWoodBoard )
			};

		private static Type[] m_MarkableTable = new Type[]
				{
					typeof( BaseArmor ),
					typeof( BaseWeapon ),
					typeof( BaseClothing ),
					typeof( BaseJewel ),
					typeof( BaseInstrument ),
					typeof( DragonBardingDeed ),
					typeof( BaseTool ),
					typeof( BaseHarvestTool ),
					typeof( FukiyaDarts ), typeof( Shuriken ),
					typeof( Spellbook ), typeof( Runebook ),
					typeof( BaseQuiver )
				};
		#endregion

		public bool IsMarkable( Type type )
		{
			if ( type == typeof(BaseMagicStaff) )
				return false;

			if( m_ForceNonExceptional )	//Don't even display the stuff for marking if it can't ever be exceptional.
				return false;

			for ( int i = 0; i < m_MarkableTable.Length; ++i )
			{
				if ( type == m_MarkableTable[i] || type.IsSubclassOf( m_MarkableTable[i] ) )
					return true;
			}

			return false;
		}

		public bool RetainsColorFrom( CraftSystem system, Type type )
		{
			if (DefTailoring.IsNonColorable(m_Type))
			{
				return false;
			}

			if ( system is DefWands )
				return false;

			if ( system.RetainsColorFrom( this, type ) )
				return true;

			bool inItemTable = false, inResourceTable = false;

			for ( int i = 0; !inItemTable && i < m_ColoredItemTable.Length; ++i )
				inItemTable = ( m_Type == m_ColoredItemTable[i] || m_Type.IsSubclassOf( m_ColoredItemTable[i] ) );

			for ( int i = 0; inItemTable && !inResourceTable && i < m_ColoredResourceTable.Length; ++i )
				inResourceTable = ( type == m_ColoredResourceTable[i] || type.IsSubclassOf( m_ColoredResourceTable[i] ) );

			return ( inItemTable && inResourceTable );
		}

		public bool Find( Mobile from, int[] itemIDs )
		{
			Map map = from.Map;

			if ( map == null )
				return false;

			IPooledEnumerable eable = map.GetItemsInRange( from.Location, 2 );

			foreach ( Item item in eable )
			{
				if ( (item.Z + 16) > from.Z && (from.Z + 16) > item.Z && Find( item.ItemID, itemIDs ) )
				{
					eable.Free();
					return true;
				}
			}

			eable.Free();

			for ( int x = -2; x <= 2; ++x )
			{
				for ( int y = -2; y <= 2; ++y )
				{
					int vx = from.X + x;
					int vy = from.Y + y;

					StaticTile[] tiles = map.Tiles.GetStaticTiles( vx, vy, true );

					for ( int i = 0; i < tiles.Length; ++i )
					{
						int z = tiles[i].Z;
						int id = tiles[i].ID;

						if ( (z + 16) > from.Z && (from.Z + 16) > z && Find( id, itemIDs ) )
							return true;
					}
				}
			}

			return false;
		}

		public bool Find( int itemID, int[] itemIDs )
		{
			bool contains = false;

			for ( int i = 0; !contains && i < itemIDs.Length; i += 2 )
				contains = ( itemID >= itemIDs[i] && itemID <= itemIDs[i + 1] );

			return contains;
		}

		public bool IsQuantityType( Type[][] types )
		{
			for ( int i = 0; i < types.Length; ++i )
			{
				Type[] check = types[i];

				for ( int j = 0; j < check.Length; ++j )
				{
					if ( typeof( IHasQuantity ).IsAssignableFrom( check[j] ) )
						return true;
				}
			}

			return false;
		}

		public int ConsumeQuantity( Container cont, Type[][] types, int[] amounts )
		{
			if ( types.Length != amounts.Length )
				throw new ArgumentException();

			Item[][] items = new Item[types.Length][];
			int[] totals = new int[types.Length];

			for ( int i = 0; i < types.Length; ++i )
			{
				items[i] = cont.FindItemsByType( types[i], true );

				for ( int j = 0; j < items[i].Length; ++j )
				{
					IHasQuantity hq = items[i][j] as IHasQuantity;

					if ( hq == null )
					{
						totals[i] += items[i][j].Amount;
					}
					else
					{
						if ( hq is BaseBeverage && ((BaseBeverage)hq).Content != BeverageType.Water )
							continue;

						totals[i] += hq.Quantity;
					}
				}

				if ( totals[i] < amounts[i] )
					return i;
			}

			for ( int i = 0; i < types.Length; ++i )
			{
				int need = amounts[i];

				for ( int j = 0; j < items[i].Length; ++j )
				{
					Item item = items[i][j];
					IHasQuantity hq = item as IHasQuantity;

					if ( hq == null )
					{
						int theirAmount = item.Amount;

						if ( theirAmount < need )
						{
							item.Delete();
							need -= theirAmount;
						}
						else
						{
							item.Consume( need );
							break;
						}
					}
					else
					{
						if ( hq is BaseBeverage && ((BaseBeverage)hq).Content != BeverageType.Water )
							continue;

						int theirAmount = hq.Quantity;

						if ( theirAmount < need )
						{
							hq.Quantity -= theirAmount;
							need -= theirAmount;
						}
						else
						{
							hq.Quantity -= need;
							break;
						}
					}
				}
			}

			return -1;
		}

		public int GetQuantity( Container cont, Type[] types )
		{
			Item[] items = cont.FindItemsByType( types, true );

			int amount = 0;

			for ( int i = 0; i < items.Length; ++i )
			{
				IHasQuantity hq = items[i] as IHasQuantity;

				if ( hq == null )
				{
					amount += items[i].Amount;
				}
				else
				{
					if ( hq is BaseBeverage && ((BaseBeverage)hq).Content != BeverageType.Water )
						continue;

					amount += hq.Quantity;
				}
			}

			return amount;
		}

		public bool ConsumeRes( Mobile from, Type typeRes, CraftSystem craftSystem, ref int resHue, ref int maxAmount, ConsumeType consumeType, ref object message )
		{
			return ConsumeRes( from, typeRes, craftSystem, ref resHue, ref maxAmount, consumeType, ref message, false );
		}

		public bool ConsumeRes( Mobile from, Type typeRes, CraftSystem craftSystem, ref int resHue, ref int maxAmount, ConsumeType consumeType, ref object message, bool isFailure )
		{
			Container ourPack = from.Backpack;

			if ( ourPack == null )
				return false;

			if ( m_NeedHeat && !Find( from, m_HeatSources ) )
			{
				message = 1044487; // You must be near a fire source to cook.
				return false;
			}

			if ( m_NeedOven && !Find( from, m_Ovens ) )
			{
				message = 1044493; // You must be near an oven to bake that.
				return false;
			}

			if ( m_NeedMill && !Find( from, m_Mills ) )
			{
				message = 1044491; // You must be near a flour mill to do that.
				return false;
			}

			Type[][] types = new Type[m_arCraftRes.Count][];
			int[] amounts = new int[m_arCraftRes.Count];

			maxAmount = int.MaxValue;

			CraftSubResCol resCol = ( m_UseSubRes2 ? craftSystem.CraftSubRes2 : craftSystem.CraftSubRes );

			for ( int i = 0; i < types.Length; ++i )
			{
				CraftRes craftRes = m_arCraftRes.GetAt( i );
				Type baseType = craftRes.ItemType;

				// Resource Mutation
				if ( (baseType == resCol.ResType) && ( typeRes != null ) )
				{
					baseType = typeRes;

					CraftSubRes subResource = resCol.SearchFor( baseType );

					if ( subResource != null && from.Skills[craftSystem.MainSkill].Value < subResource.RequiredSkill )
					{
						message = subResource.Message;
						return false;
					}
				}
				// ******************

				for ( int j = 0; types[i] == null && j < m_TypesTable.Length; ++j )
				{
					if ( m_TypesTable[j][0] == baseType )
						types[i] = m_TypesTable[j];
				}

				if ( types[i] == null )
					types[i] = new Type[]{ baseType };

				amounts[i] = craftRes.Amount;

				// For stackable items that can be crafted more than one at a time
				if ( UseAllRes )
				{
					int tempAmount = ourPack.GetAmount( types[i] );
					tempAmount /= amounts[i];
					if ( tempAmount < maxAmount )
					{
						maxAmount = tempAmount;

						if ( maxAmount == 0 )
						{
							CraftRes res = m_arCraftRes.GetAt( i );

							if ( res.MessageNumber > 0 )
								message = res.MessageNumber;
							else if ( !String.IsNullOrEmpty( res.MessageString ) )
								message = res.MessageString;
							else
								message = 502925; // You don't have the resources required to make that item.

							return false;
						}
					}
				}
				// ****************************

				if ( isFailure && !craftSystem.ConsumeOnFailure( from, types[i][0], this ) )
					amounts[i] = 0;
			}

			// We adjust the amount of each resource to consume the max posible
			if ( UseAllRes )
			{
				for ( int i = 0; i < amounts.Length; ++i )
					amounts[i] *= maxAmount;
			}
			else
				maxAmount = -1;

			Item consumeExtra = null;

			int index = 0;

            // Consume ALL
            if ( consumeType == ConsumeType.All )
			{
				m_ResHue = 0; m_ResAmount = 0; m_System = craftSystem;

                if ( IsQuantityType( types ) )
					index = ConsumeQuantity( ourPack, types, amounts );
				else
					index = ourPack.ConsumeTotalGrouped( types, amounts, true, new OnItemConsumed( OnResourceConsumed ), new CheckItemGroup( CheckHueGrouping ) );

				resHue = m_ResHue;
			}

			// Consume Half ( for use all resource craft type )
			else if ( consumeType == ConsumeType.Half )
			{
				for ( int i = 0; i < amounts.Length; i++ )
				{
					amounts[i] /= 2;

					if ( amounts[i] < 1 )
						amounts[i] = 1;
				}

				m_ResHue = 0; m_ResAmount = 0; m_System = craftSystem;

				if ( IsQuantityType( types ) )
					index = ConsumeQuantity( ourPack, types, amounts );
				else
					index = ourPack.ConsumeTotalGrouped( types, amounts, true, new OnItemConsumed( OnResourceConsumed ), new CheckItemGroup( CheckHueGrouping ) );

				resHue = m_ResHue;
			}

			else // ConstumeType.None ( it's basicaly used to know if the crafter has enough resource before starting the process )
			{
				index = -1;

				if ( IsQuantityType( types ) )
				{
					for ( int i = 0; i < types.Length; i++ )
					{
						if ( GetQuantity( ourPack, types[i] ) < amounts[i] )
						{
							index = i;
							break;
						}
					}
				}
				else
				{
					for ( int i = 0; i < types.Length; i++ )
					{
						if ( ourPack.GetBestGroupAmount( types[i], true, new CheckItemGroup( CheckHueGrouping ) ) < amounts[i] )
						{
							index = i;
							break;
						}
					}
				}
			}

			if ( index == -1 )
			{
				if ( consumeType != ConsumeType.None )
					if ( consumeExtra != null )
						consumeExtra.Delete();

				return true;
			}
			else
			{
				CraftRes res = m_arCraftRes.GetAt( index );

				if ( res.MessageNumber > 0 )
					message = res.MessageNumber;
				else if ( res.MessageString != null && res.MessageString != String.Empty )
					message = res.MessageString;
				else
					message = 502925; // You don't have the resources required to make that item.

				return false;
			}
		}

		private int m_ResHue;
		private int m_ResAmount;
		private CraftSystem m_System;

		private void OnResourceConsumed( Item item, int amount )
		{
			if ( !RetainsColorFrom( m_System, item.GetType() ) )
				return;

			if ( amount >= m_ResAmount )
			{
				m_ResHue = item.Hue;
				m_ResAmount = amount;
			}
		}

		private int CheckHueGrouping( Item a, Item b )
		{
			return b.Hue.CompareTo( a.Hue );
		}

		public double GetExceptionalChance( CraftSystem system, double chance, Mobile from )
		{
			if( m_ForceNonExceptional )
				return 0.0;

			double bonus = 0.0;

			switch ( system.ECA )
			{
				default:
				case CraftECA.ChanceMinusSixty: chance -= 0.6; break;
				case CraftECA.FiftyPercentChanceMinusTenPercent: chance = chance * 0.5 - 0.1; break;
				case CraftECA.ChanceMinusSixtyToFourtyFive:
				{
					double offset = 0.60 - ((from.Skills[system.MainSkill].Value - 95.0) * 0.03);

					if ( offset < 0.45 )
						offset = 0.45;
					else if ( offset > 0.60 )
						offset = 0.60;

					chance -= offset;
					break;
				}
			}

			if ( chance > 0 )
				return chance + bonus;

			return chance;
		}

		public bool CheckSkills( Mobile from, Type typeRes, CraftSystem craftSystem, ref int quality, ref bool allRequiredSkills )
		{
			return CheckSkills( from, typeRes, craftSystem, ref quality, ref allRequiredSkills, true );
		}

		public bool CheckSkills( Mobile from, Type typeRes, CraftSystem craftSystem, ref int quality, ref bool allRequiredSkills, bool gainSkills )
		{
			double chance = GetSuccessChance( from, typeRes, craftSystem, gainSkills, ref allRequiredSkills );

			if ( GetExceptionalChance( craftSystem, chance, from ) > Utility.RandomDouble() )
				quality = 2;

			return ( chance > Utility.RandomDouble() );
		}

		public double GetSuccessChance( Mobile from, Type typeRes, CraftSystem craftSystem, bool gainSkills, ref bool allRequiredSkills )
		{
			double minMainSkill = 0.0;
			double maxMainSkill = 0.0;
			double valMainSkill = 0.0;

			allRequiredSkills = true;

			for ( int i = 0; i < m_arCraftSkill.Count; i++)
			{
				CraftSkill craftSkill = m_arCraftSkill.GetAt(i);

				double minSkill = craftSkill.MinSkill;
				double maxSkill = craftSkill.MaxSkill;
				double valSkill = from.Skills[craftSkill.SkillToMake].Value;

				if ( valSkill < minSkill )
					allRequiredSkills = false;

				if ( craftSkill.SkillToMake == craftSystem.MainSkill )
				{
					minMainSkill = minSkill;
					maxMainSkill = maxSkill;
					valMainSkill = valSkill;
				}

				if ( gainSkills ) // This is a passive check. Success chance is entirely dependent on the main skill
					from.CheckSkill( craftSkill.SkillToMake, minSkill, maxSkill );
			}

			double chance;

			if ( allRequiredSkills )
				chance = craftSystem.GetChanceAtMin( this ) + ((valMainSkill - minMainSkill) / (maxMainSkill - minMainSkill) * (1.0 - craftSystem.GetChanceAtMin( this )));
			else
				chance = 0.0;

			if ( allRequiredSkills && valMainSkill == maxMainSkill )
				chance = 1.0;

			return chance;
		}

		public void Craft( Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool )
		{
			if ( from.BeginAction( typeof( CraftSystem ) ) )
			{
				if( RequiredExpansion == Expansion.None || ( from.NetState != null && from.NetState.SupportsExpansion( RequiredExpansion ) ) )
				{
					bool allRequiredSkills = true;
					double chance = GetSuccessChance( from, typeRes, craftSystem, false, ref allRequiredSkills );

					if ( allRequiredSkills && chance >= 0.0 )
					{
						if( this.Recipe == null || !(from is PlayerMobile) || ((PlayerMobile)from).HasRecipe( this.Recipe ) )
						{
							int badCraft = craftSystem.CanCraft( from, tool, m_Type );

							if( badCraft <= 0 )
							{
								int resHue = 0;
								int maxAmount = 0;
								object message = null;

								if( ConsumeRes( from, typeRes, craftSystem, ref resHue, ref maxAmount, ConsumeType.None, ref message ) )
								{
									message = null;

									if( ConsumeAttributes( from, ref message, false ) )
									{
										CraftContext context = craftSystem.GetContext( from );

										if( context != null )
											context.OnMade( this );

										int iMin = craftSystem.MinCraftEffect;
										int iMax = (craftSystem.MaxCraftEffect - iMin) + 1;
										int iRandom = Utility.Random( iMax );
										iRandom += iMin + 1;
										new InternalTimer( from, craftSystem, this, typeRes, tool, iRandom ).Start();
									}
									else
									{
										from.EndAction( typeof( CraftSystem ) );
										from.SendGump( new CraftGump( from, craftSystem, tool, message ) );
									}
								}
								else
								{
									from.EndAction( typeof( CraftSystem ) );
									from.SendGump( new CraftGump( from, craftSystem, tool, message ) );
								}
							}
							else
							{
								from.EndAction( typeof( CraftSystem ) );
								from.SendGump( new CraftGump( from, craftSystem, tool, badCraft ) );
							}
						}
						else
						{
							from.EndAction( typeof( CraftSystem ) );
							from.SendGump( new CraftGump( from, craftSystem, tool, 1072847 ) ); // You must learn that recipe from a scroll.
						}
					}
					else
					{
						from.EndAction( typeof( CraftSystem ) );
						from.SendGump( new CraftGump( from, craftSystem, tool, 1044153 ) ); // You don't have the required skills to attempt this item.
					}
				}
				else
				{
					from.EndAction( typeof( CraftSystem ) );
					from.SendGump( new CraftGump( from, craftSystem, tool, RequiredExpansionMessage( RequiredExpansion ) ) ); //The {0} expansion is required to attempt this item.
				}
			}
			else
			{
				from.SendLocalizedMessage( 500119 ); // You must wait to perform another action
			}
		}

		private object RequiredExpansionMessage( Expansion expansion )	//Eventually convert to TextDefinition, but that requires that we convert all the gumps to ues it too.  Not that it wouldn't be a bad idea.
		{
			switch( expansion )
			{
				case Expansion.SE:
					return 1063307; // The "Samurai Empire" expansion is required to attempt this item.
				case Expansion.ML:
					return 1072650; // The "Mondain's Legacy" expansion is required to attempt this item.
				default:
					return String.Format( "The \"{0}\" expansion is required to attempt this item.", ExpansionInfo.GetInfo( expansion ).Name );
			}
		}

		public void CompleteCraft( int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CustomCraft customCraft )
		{
			int badCraft = craftSystem.CanCraft( from, tool, m_Type );

			if ( badCraft > 0 )
			{
				if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
					from.SendGump( new CraftGump( from, craftSystem, tool, badCraft ) );
				else
					from.SendLocalizedMessage( badCraft );

				return;
			}

			int checkResHue = 0, checkMaxAmount = 0;
			object checkMessage = null;

			// Not enough resource to craft it
			if ( !ConsumeRes( from, typeRes, craftSystem, ref checkResHue, ref checkMaxAmount, ConsumeType.None, ref checkMessage ) )
			{
				if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
					from.SendGump( new CraftGump( from, craftSystem, tool, checkMessage ) );
				else if ( checkMessage is int && (int)checkMessage > 0 )
					from.SendLocalizedMessage( (int)checkMessage );
				else if ( checkMessage is string )
					from.SendMessage( (string)checkMessage );

				return;
			}
			else if ( !ConsumeAttributes( from, ref checkMessage, false ) )
			{
				if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
					from.SendGump( new CraftGump( from, craftSystem, tool, checkMessage ) );
				else if ( checkMessage is int && (int)checkMessage > 0 )
					from.SendLocalizedMessage( (int)checkMessage );
				else if ( checkMessage is string )
					from.SendMessage( (string)checkMessage );

				return;
			}

			bool toolBroken = false;

			int ignored = 1;
			int endquality = 1;

			bool allRequiredSkills = true;

			if ( CheckSkills( from, typeRes, craftSystem, ref ignored, ref allRequiredSkills ) )
			{
				// Resource
				int resHue = 0;
				int maxAmount = 0;

				object message = null;

				// Not enough resource to craft it
				if ( !ConsumeRes( from, typeRes, craftSystem, ref resHue, ref maxAmount, ConsumeType.All, ref message ) )
				{
					if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
						from.SendGump( new CraftGump( from, craftSystem, tool, message ) );
					else if ( message is int && (int)message > 0 )
						from.SendLocalizedMessage( (int)message );
					else if ( message is string )
						from.SendMessage( (string)message );

					return;
				}
				else if ( !ConsumeAttributes( from, ref message, true ) )
				{
					if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
						from.SendGump( new CraftGump( from, craftSystem, tool, message ) );
					else if ( message is int && (int)message > 0 )
						from.SendLocalizedMessage( (int)message );
					else if ( message is string )
						from.SendMessage( (string)message );

					return;
				}

				tool.UsesRemaining--;

				if ( craftSystem is DefBlacksmithy )
				{
					AncientSmithyHammer hammer = from.FindItemOnLayer( Layer.OneHanded ) as AncientSmithyHammer;
					if ( hammer != null && hammer != tool )
					{
						hammer.UsesRemaining--;
						if ( hammer.UsesRemaining < 1 )
							hammer.Delete();
					}
                }

                AncientCraftingGloves gloves = from.FindItemOnLayer(Layer.OneHanded) as AncientCraftingGloves;
				if (gloves != null)
                    gloves.ConsumeCharge(from, craftSystem);

                if ( tool.UsesRemaining < 1 )
					toolBroken = true;

                Container outputContainer = tool.Parent is PlayerMobile ? ((Mobile)tool.Parent).Backpack : tool.Parent as Container;

                if ( toolBroken )
					tool.Delete();

				int num = 0;
                Item item;
				if ( customCraft != null )
				{
					item = customCraft.CompleteCraft( out num );
				}
				else if ( ItemType != typeof(BlankMap) && typeof( MapItem ).IsAssignableFrom( ItemType ) && Worlds.IsPlayerInTheLand( from.Map, from.Location, from.X, from.Y ) == false )
				{
					item = new IndecipherableMap();
					from.SendMessage( "You cannot seem to create a map of this area." );
				}
				else
				{
					item = Activator.CreateInstance( ItemType ) as Item;
				}

				if ( item != null )
				{
					if( item is ICraftable ) {
						item.IsCrafted = true;
						endquality = ((ICraftable)item).OnCraft( quality, makersMark, from, craftSystem, typeRes, tool, this, resHue );
					} else if ( item.Hue == 0 ) {
						item.Hue = resHue;
					}

					if ( maxAmount > 0 )
					{
						if ( !item.Stackable && item is IUsesRemaining )
							((IUsesRemaining)item).UsesRemaining *= maxAmount;
						else
							item.Amount = maxAmount;
					}

					if ( item is Kindling || item is BarkFragment || item is Shaft )
					{
						item.Amount = item.Amount * 2;
					}
					else if ( item is WoodenPlateLegs || 
							item is WoodenPlateGloves || 
							item is WoodenPlateGorget || 
							item is WoodenPlateArms || 
							item is WoodenPlateChest || 
							item is WoodenPlateHelm )
					{
						if ( item is BaseArmor )
						{
							BaseArmor woody = (BaseArmor)item;
							if ( woody.Resource == CraftResource.None )
							{
								woody.Resource = CraftResource.RegularWood;
								item.Hue = 0x840;
							}
						}
					}
					else if ( item is PlateArms )
					{
						item.ItemID = Utility.RandomList( 0x1410, 0x1417 );
					}
					else if ( item is LeatherCloak || item is LeatherRobe || item is LeatherSandals || item is LeatherShoes || item is LeatherBoots || item is LeatherThighBoots || item is LeatherSoftBoots )
					{
						if ( item is BaseArmor )
						{
							BaseArmor cloak = (BaseArmor)item;
							if ( cloak.Resource == CraftResource.None )
							{
								cloak.Resource = CraftResource.RegularLeather;
								item.Hue = 0x83E;
							}
						}
					}
					else if ( item is DragonArms || item is DragonChest || item is DragonGloves || item is DragonHelm || item is DragonLegs )
					{
						if ( item is BaseArmor )
						{
							BaseArmor dino = (BaseArmor)item;

							if ( dino.Resource == CraftResource.DinosaurScales )
							{
								if ( item is DragonArms ){ item.Name = "dinosaur sleeves"; }
								else if ( item is DragonChest ){ item.Name = "dinosaur breastplate"; }
								else if ( item is DragonGloves ){ item.Name = "dinosaur gloves"; }
								else if ( item is DragonHelm ){ item.Name = "dinosaur helm"; }
								else if ( item is DragonLegs ){ item.Name = "dinosaur leggings"; }
							}	
						}
					}
					else if ( item is SkinUnicornLegs || 
							item is SkinUnicornGloves || 
							item is SkinUnicornGorget || 
							item is SkinUnicornArms || 
							item is SkinUnicornChest || 
							item is SkinUnicornHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "unicorn skin", "", 0 ); if ( item is BaseArmor ){ BaseArmor woody = (BaseArmor)item; woody.Resource = CraftResource.RegularLeather; } }
					else if ( item is SkinDemonLegs || 
							item is SkinDemonGloves || 
							item is SkinDemonGorget || 
							item is SkinDemonArms || 
							item is SkinDemonChest || 
							item is SkinDemonHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "demon skin", "", 0 ); if ( item is BaseArmor ){ BaseArmor woody = (BaseArmor)item; woody.Resource = CraftResource.RegularLeather; } }
					else if ( item is SkinDragonLegs || 
							item is SkinDragonGloves || 
							item is SkinDragonGorget || 
							item is SkinDragonArms || 
							item is SkinDragonChest || 
							item is SkinDragonHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "dragon skin", "", 0 ); if ( item is BaseArmor ){ BaseArmor woody = (BaseArmor)item; woody.Resource = CraftResource.RegularLeather; } }
					else if ( item is SkinNightmareLegs || 
							item is SkinNightmareGloves || 
							item is SkinNightmareGorget || 
							item is SkinNightmareArms || 
							item is SkinNightmareChest || 
							item is SkinNightmareHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "nightmare skin", "", 0 ); if ( item is BaseArmor ){ BaseArmor woody = (BaseArmor)item; woody.Resource = CraftResource.RegularLeather; } }
					else if ( item is SkinSerpentLegs || 
							item is SkinSerpentGloves || 
							item is SkinSerpentGorget || 
							item is SkinSerpentArms || 
							item is SkinSerpentChest || 
							item is SkinSerpentHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "serpent skin", "", 0 ); if ( item is BaseArmor ){ BaseArmor woody = (BaseArmor)item; woody.Resource = CraftResource.RegularLeather; } }
					else if ( item is SkinTrollLegs || 
							item is SkinTrollGloves || 
							item is SkinTrollGorget || 
							item is SkinTrollArms || 
							item is SkinTrollChest || 
							item is SkinTrollHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "troll skin", "", 0 ); if ( item is BaseArmor ){ BaseArmor woody = (BaseArmor)item; woody.Resource = CraftResource.RegularLeather; } }
					else if ( item is AmethystPlateLegs || 
							item is AmethystPlateGloves || 
							item is AmethystPlateGorget || 
							item is AmethystPlateArms || 
							item is AmethystPlateChest || 
							item is AmethystFemalePlateChest || 
							item is AmethystShield || 
							item is AmethystPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "amethyst", "", 0 ); }
					else if ( item is CaddellitePlateLegs || 
							item is CaddellitePlateGloves || 
							item is CaddellitePlateGorget || 
							item is CaddellitePlateArms || 
							item is CaddellitePlateChest || 
							item is CaddelliteFemalePlateChest || 
							item is CaddelliteShield || 
							item is CaddellitePlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "caddellite", "", 0 ); }
					else if ( item is EmeraldPlateLegs || 
							item is EmeraldPlateGloves || 
							item is EmeraldPlateGorget || 
							item is EmeraldPlateArms || 
							item is EmeraldPlateChest || 
							item is EmeraldFemalePlateChest || 
							item is EmeraldShield || 
							item is EmeraldPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "emerald", "", 0 ); }
					else if ( item is GarnetPlateLegs || 
							item is GarnetPlateGloves || 
							item is GarnetPlateGorget || 
							item is GarnetPlateArms || 
							item is GarnetPlateChest || 
							item is GarnetFemalePlateChest || 
							item is GarnetShield || 
							item is GarnetPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "garnet", "", 0 ); }
					else if ( item is IcePlateLegs || 
							item is IcePlateGloves || 
							item is IcePlateGorget || 
							item is IcePlateArms || 
							item is IcePlateChest || 
							item is IceFemalePlateChest || 
							item is IceShield || 
							item is IcePlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "ice", "", 0 ); }
					else if ( item is JadePlateLegs || 
							item is JadePlateGloves || 
							item is JadePlateGorget || 
							item is JadePlateArms || 
							item is JadePlateChest || 
							item is JadeFemalePlateChest || 
							item is JadeShield || 
							item is JadePlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "jade", "", 0 ); }
					else if ( item is MarblePlateLegs || 
							item is MarblePlateGloves || 
							item is MarblePlateGorget || 
							item is MarblePlateArms || 
							item is MarblePlateChest || 
							item is MarbleFemalePlateChest || 
							item is MarbleShields || 
							item is MarblePlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "marble", "", 0 ); }
					else if ( item is OnyxPlateLegs || 
							item is OnyxPlateGloves || 
							item is OnyxPlateGorget || 
							item is OnyxPlateArms || 
							item is OnyxPlateChest || 
							item is OnyxFemalePlateChest || 
							item is OnyxShield || 
							item is OnyxPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "onyx", "", 0 ); }
					else if ( item is QuartzPlateLegs || 
							item is QuartzPlateGloves || 
							item is QuartzPlateGorget || 
							item is QuartzPlateArms || 
							item is QuartzPlateChest || 
							item is QuartzFemalePlateChest || 
							item is QuartzShield || 
							item is QuartzPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "quartz", "", 0 ); }
					else if ( item is RubyPlateLegs || 
							item is RubyPlateGloves || 
							item is RubyPlateGorget || 
							item is RubyPlateArms || 
							item is RubyPlateChest || 
							item is RubyFemalePlateChest || 
							item is RubyShield || 
							item is RubyPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "ruby", "", 0 ); }
					else if ( item is SapphirePlateLegs || 
							item is SapphirePlateGloves || 
							item is SapphirePlateGorget || 
							item is SapphirePlateArms || 
							item is SapphirePlateChest || 
							item is SapphireFemalePlateChest || 
							item is SapphireShield || 
							item is SapphirePlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "sapphire", "", 0 ); }
					else if ( item is SilverPlateLegs || 
							item is SilverPlateGloves || 
							item is SilverPlateGorget || 
							item is SilverPlateArms || 
							item is SilverPlateChest || 
							item is SilverFemalePlateChest || 
							item is SilverShield || 
							item is SilverPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "silver", "", 0 ); }
					else if ( item is SpinelPlateLegs || 
							item is SpinelPlateGloves || 
							item is SpinelPlateGorget || 
							item is SpinelPlateArms || 
							item is SpinelPlateChest || 
							item is SpinelFemalePlateChest || 
							item is SpinelShield || 
							item is SpinelPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "spinel", "", 0 ); }
					else if ( item is StarRubyPlateLegs || 
							item is StarRubyPlateGloves || 
							item is StarRubyPlateGorget || 
							item is StarRubyPlateArms || 
							item is StarRubyPlateChest || 
							item is StarRubyFemalePlateChest || 
							item is StarRubyShield || 
							item is StarRubyPlateHelm ){ item.Hue = MaterialInfo.GetMaterialColor( "star ruby", "", 0 ); }
					else if ( item is TopazPlateLegs || 
							item is TopazPlateGloves || 
							item is TopazPlateGorget || 
							item is TopazPlateArms || 
							item is TopazPlateChest || 
							item is TopazFemalePlateChest || 
							item is TopazPlateHelm || 
							item is TopazShield ){ item.Hue = MaterialInfo.GetMaterialColor( "topaz", "", 0 ); }
					else if ( item is WhiteFurRobe || 
							item is WhiteFurCape || 
							item is WhiteFurCap || 
							item is WhiteFurBoots || 
							item is WhiteFurSarong ){ item.Hue = 0x481; }
					else if ( item is FurRobe || 
							item is FurCape || 
							item is FurCap || 
							item is FurBoots || 
							item is FurSarong ){ item.Hue = 0x907; }

					else if ( item is BaseMagicStaff )
					{
						((BaseBashing)item).Resource = CraftResource.None;
						string CrafterName = from.Name;

						if ( CrafterName.EndsWith( "s" ) )
						{
							CrafterName = CrafterName + "'";
						}
						else
						{
							CrafterName = CrafterName + "'s";
						}

						if ( item is ClumsyMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 20 ); }
						else if ( item is CreateFoodMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 17 ); }
						else if ( item is FeebleMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 16 ); }
						else if ( item is HealMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 21 ); }
						else if ( item is MagicArrowMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 18 ); }
						else if ( item is NightSightMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 14 ); }
						else if ( item is ReactiveArmorMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 15 ); }
						else if ( item is WeaknessMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 19 ); }
						else if ( item is AgilityMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 21 ); }
						else if ( item is CunningMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 14 ); }
						else if ( item is CureMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 17 ); }
						else if ( item is HarmMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 15 ); }
						else if ( item is MagicTrapMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 19 ); }
						else if ( item is MagicUntrapMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 20 ); }
						else if ( item is ProtectionMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 16 ); }
						else if ( item is StrengthMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 21 ); }
						else if ( item is BlessMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 18 ); }
						else if ( item is FireballMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 14 ); }
						else if ( item is MagicLockMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 19 ); }
						else if ( item is MagicUnlockMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 17 ); }
						else if ( item is PoisonMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 16 ); }
						else if ( item is TelekinesisMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 20 ); }
						else if ( item is TeleportMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 15 ); }
						else if ( item is WallofStoneMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 18 ); }
						else if ( item is ArchCureMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 21 ); }
						else if ( item is ArchProtectionMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 15 ); }
						else if ( item is CurseMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 17 ); }
						else if ( item is FireFieldMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 14 ); }
						else if ( item is GreaterHealMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 18 ); }
						else if ( item is LightningMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 20 ); }
						else if ( item is ManaDrainMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 19 ); }
						else if ( item is RecallMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 20 ); }
						else if ( item is BladeSpiritsMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 1 ); }
						else if ( item is DispelFieldMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 7 ); }
						else if ( item is IncognitoMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 9 ); }
						else if ( item is MagicReflectionMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 24 ); }
						else if ( item is MindBlastMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 11 ); }
						else if ( item is ParalyzeMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 13 ); }
						else if ( item is PoisonFieldMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 10 ); }
						else if ( item is SummonCreatureMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 4 ); }
						else if ( item is DispelMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 29 ); }
						else if ( item is EnergyBoltMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 3 ); }
						else if ( item is ExplosionMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 2 ); }
						else if ( item is InvisibilityMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 5 ); }
						else if ( item is MarkMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 23 ); }
						else if ( item is MassCurseMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 22 ); }
						else if ( item is ParalyzeFieldMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 8 ); }
						else if ( item is RevealMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 37 ); }
						else if ( item is ChainLightningMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 6 ); }
						else if ( item is EnergyFieldMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 11 ); }
						else if ( item is FlameStrikeMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 22 ); }
						else if ( item is GateTravelMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 29 ); }
						else if ( item is ManaVampireMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 9 ); }
						else if ( item is MassDispelMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 1 ); }
						else if ( item is MeteorSwarmMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 2 ); }
						else if ( item is PolymorphMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 23 ); }
						else if ( item is AirElementalMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 6 ); }
						else if ( item is EarthElementalMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 10 ); }
						else if ( item is EarthquakeMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 4 ); }
						else if ( item is EnergyVortexMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 3 ); }
						else if ( item is FireElementalMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 5 ); }
						else if ( item is ResurrectionMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 24 ); }
						else if ( item is SummonDaemonMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 13 ); }
						else if ( item is WaterElementalMagicStaff ){ Server.Misc.MaterialInfo.ColorMetal( item, 8 ); }

						item.Name = CrafterName + " " + item.Name;
					}
					else if ( item is BaseInstrument )
					{
						int cHue = 0;
						int cUse = 0;
						Type resourceType = typeRes;
						if ( resourceType == null )
							resourceType = Resources.GetAt( 0 ).ItemType;

						CraftResource thisResource = CraftResources.GetFromType( resourceType );

						switch ( thisResource )
						{
							case CraftResource.AshTree: cHue = MaterialInfo.GetMaterialColor( "ash", "", 0 ); cUse = 20; break;
							case CraftResource.CherryTree: cHue = MaterialInfo.GetMaterialColor( "cherry", "", 0 ); cUse = 40; break;
							case CraftResource.EbonyTree: cHue = MaterialInfo.GetMaterialColor( "ebony", "", 0 ); cUse = 60; break;
							case CraftResource.GoldenOakTree: cHue = MaterialInfo.GetMaterialColor( "golden oak", "", 0 ); cUse = 80; break;
							case CraftResource.HickoryTree: cHue = MaterialInfo.GetMaterialColor( "hickory", "", 0 ); cUse = 100; break;
							case CraftResource.MahoganyTree: cHue = MaterialInfo.GetMaterialColor( "mahogany", "", 0 ); cUse = 120; break;
							case CraftResource.DriftwoodTree: cHue = MaterialInfo.GetMaterialColor( "driftwood", "", 0 ); cUse = 120; break;
							case CraftResource.OakTree: cHue = MaterialInfo.GetMaterialColor( "oak", "", 0 ); cUse = 140; break;
							case CraftResource.PineTree: cHue = MaterialInfo.GetMaterialColor( "pine", "", 0 ); cUse = 160; break;
							case CraftResource.GhostTree: cHue = MaterialInfo.GetMaterialColor( "ghostwood", "", 0 ); cUse = 160; break;
							case CraftResource.RosewoodTree: cHue = MaterialInfo.GetMaterialColor( "rosewood", "", 0 ); cUse = 180; break;
							case CraftResource.WalnutTree: cHue = MaterialInfo.GetMaterialColor( "walnut", "", 0 ); cUse = 200; break;
							case CraftResource.PetrifiedTree: cHue = MaterialInfo.GetMaterialColor( "petrified", "", 0 ); cUse = 250; break;
							case CraftResource.ElvenTree: cHue = MaterialInfo.GetMaterialColor( "elven", "", 0 ); cUse = 400; break;
						}

						((BaseInstrument)item).UsesRemaining = ((BaseInstrument)item).UsesRemaining + cUse;
						item.Hue = cHue;
					}
					else if ( item is PotionKeg )
					{
						item.Hue = 0x96D;
					}
					else if ( item is TenFootPole )
					{
						int cHue = 0;
						double cWeight = 40.0;
						string wood = "wooden";
						Type resourceType = typeRes;
						if ( resourceType == null )
							resourceType = Resources.GetAt( 0 ).ItemType;

						CraftResource thisResource = CraftResources.GetFromType( resourceType );

						switch ( thisResource )
						{
							case CraftResource.AshTree: cHue = MaterialInfo.GetMaterialColor( "ash", "", 0 );			cWeight = cWeight-1; 	wood = "ashen";				; break;
							case CraftResource.CherryTree: cHue = MaterialInfo.GetMaterialColor( "cherry", "", 0 ); 		cWeight = cWeight-2; 	wood = "cherry wood";		break;
							case CraftResource.EbonyTree: cHue = MaterialInfo.GetMaterialColor( "ebony", "", 0 );			cWeight = cWeight-3; 	wood = "ebony wood";		break;
							case CraftResource.GoldenOakTree: cHue = MaterialInfo.GetMaterialColor( "golden oak", "", 0 ); 	cWeight = cWeight-4; 	wood = "golden oak";		break;
							case CraftResource.HickoryTree: cHue = MaterialInfo.GetMaterialColor( "hickory", "", 0 ); 		cWeight = cWeight-5; 	wood = "hickory";			break;
							case CraftResource.MahoganyTree: cHue = MaterialInfo.GetMaterialColor( "mahogany", "", 0 );	cWeight = cWeight-6; 	wood = "mahogany";			break;
							case CraftResource.DriftwoodTree: cHue = MaterialInfo.GetMaterialColor( "driftwood", "", 0 );	 	cWeight = cWeight-7; 	wood = "driftwood";			; break;
							case CraftResource.OakTree: cHue = MaterialInfo.GetMaterialColor( "oak", "", 0 );			cWeight = cWeight-8; 	wood = "oaken";				break;
							case CraftResource.PineTree: cHue = MaterialInfo.GetMaterialColor( "pine", "", 0 );			cWeight = cWeight-9; 	wood = "pine wood";			break;
							case CraftResource.GhostTree: cHue = MaterialInfo.GetMaterialColor( "ghostwood", "", 0 );		cWeight = cWeight-10; 	wood = "ghostwood";			break;
							case CraftResource.RosewoodTree: cHue = MaterialInfo.GetMaterialColor( "rosewood", "", 0 );		cWeight = cWeight-11; 	wood = "rosewood";			break;
							case CraftResource.WalnutTree: cHue = MaterialInfo.GetMaterialColor( "walnut", "", 0 );		cWeight = cWeight-12; 	wood = "walnut wood";;break;
							case CraftResource.PetrifiedTree: cHue = MaterialInfo.GetMaterialColor( "petrified", "", 0 );		 	cWeight = cWeight-13; 	wood = "petrified wood";break;
							case CraftResource.ElvenTree: cHue = MaterialInfo.GetMaterialColor( "elven", "", 0 );			cWeight = cWeight-14; 	wood = "elven wood";break;
						}

						item.Name = "ten foot " + wood + " pole";
						item.Weight = cWeight;
						item.Hue = cHue;

					}
					else if ( item is HorseArmor )
					{
						int color = MaterialInfo.GetMaterialColor( "silver", "monster", 0 );
						string material = "Iron";

						Type resourceType = typeRes;
						if ( resourceType == null )
							resourceType = Resources.GetAt( 0 ).ItemType;

						CraftResource thisResource = CraftResources.GetFromType( resourceType );

						switch ( thisResource )
						{
							case CraftResource.DullCopper: color = MaterialInfo.GetMaterialColor( "dull copper", "classic", 0 ); material = "Dull Copper"; break;
							case CraftResource.ShadowIron: color = MaterialInfo.GetMaterialColor( "shadow iron", "classic", 0 ); material = "Shadow Iron"; break;
							case CraftResource.Copper: color = MaterialInfo.GetMaterialColor( "copper", "classic", 0 ); material = "Copper"; break;
							case CraftResource.Bronze: color = MaterialInfo.GetMaterialColor( "bronze", "classic", 0 ); material = "Bronze"; break;
							case CraftResource.Gold: color = MaterialInfo.GetMaterialColor( "gold", "classic", 0 ); material = "Gold"; break;
							case CraftResource.Agapite: color = MaterialInfo.GetMaterialColor( "agapite", "classic", 0 ); material = "Agapite"; break;
							case CraftResource.Verite: color = MaterialInfo.GetMaterialColor( "verite", "classic", 0 ); material = "Verite"; break;
							case CraftResource.Valorite: color = MaterialInfo.GetMaterialColor( "valorite", "classic", 0 ); material = "Valorite"; break;
							case CraftResource.Nepturite: color = MaterialInfo.GetMaterialColor( "nepturite", "classic", 0 ); material = "Nepturite"; break;
							case CraftResource.Obsidian: color = MaterialInfo.GetMaterialColor( "obsidian", "classic", 0 ); material = "Obsidian"; break;
							case CraftResource.Steel: color = MaterialInfo.GetMaterialColor( "steel", "classic", 0 ); material = "Steel"; break;
							case CraftResource.Brass: color = MaterialInfo.GetMaterialColor( "brass", "classic", 0 ); material = "Brass"; break;
							case CraftResource.Mithril: color = MaterialInfo.GetMaterialColor( "mithril", "classic", 0 ); material = "Mithril"; break;
							case CraftResource.Xormite: color = MaterialInfo.GetMaterialColor( "xormite", "classic", 0 ); material = "Xormite"; break;
							case CraftResource.Dwarven: color = MaterialInfo.GetMaterialColor( "dwarven", "classic", 0 ); material = "Dwarven"; break;
						}

						item.Hue = color;
					}
					else if ( item is BaseStatue )
					{
						int color = 0xB8E;
						string material = "Granite";
						string maker = from.Name;

						Type resourceType = typeRes;
						if ( resourceType == null )
							resourceType = Resources.GetAt( 0 ).ItemType;

						CraftResource thisResource = CraftResources.GetFromType( resourceType );

						switch ( thisResource )
						{
							case CraftResource.DullCopper: color = MaterialInfo.GetMaterialColor( "dull copper", "classic", 0 ); material = "Dull Copper Granite"; break;
							case CraftResource.ShadowIron: color = MaterialInfo.GetMaterialColor( "shadow iron", "classic", 0 ); material = "Shadow Iron Granite"; break;
							case CraftResource.Copper: color = MaterialInfo.GetMaterialColor( "copper", "classic", 0 ); material = "Copper Granite"; break;
							case CraftResource.Bronze: color = MaterialInfo.GetMaterialColor( "bronze", "classic", 0 ); material = "Bronze Granite"; break;
							case CraftResource.Gold: color = MaterialInfo.GetMaterialColor( "gold", "classic", 0 ); material = "Gold Granite"; break;
							case CraftResource.Agapite: color = MaterialInfo.GetMaterialColor( "agapite", "classic", 0 ); material = "Agapite Granite"; break;
							case CraftResource.Verite: color = MaterialInfo.GetMaterialColor( "verite", "classic", 0 ); material = "Verite Granite"; break;
							case CraftResource.Valorite: color = MaterialInfo.GetMaterialColor( "valorite", "classic", 0 ); material = "Valorite Granite"; break;
							case CraftResource.Nepturite: color = MaterialInfo.GetMaterialColor( "nepturite", "classic", 0 ); material = "Nepturite Granite"; break;
							case CraftResource.Obsidian: color = MaterialInfo.GetMaterialColor( "obsidian", "classic", 0 ); material = "Obsidian Granite"; break;
							case CraftResource.Mithril: color = MaterialInfo.GetMaterialColor( "mithril", "classic", 0 ); material = "Mithril Granite"; break;
							case CraftResource.Xormite: color = MaterialInfo.GetMaterialColor( "xormite", "classic", 0 ); material = "Xormite Granite"; break;
							case CraftResource.Dwarven: color = MaterialInfo.GetMaterialColor( "dwarven", "classic", 0 ); material = "Dwarven Granite"; break;
						}

						((BaseStatue)item).Crafter = maker;
						((BaseStatue)item).Resource = material;
						item.Hue = color;
					}
					else if ( item is BaseStatueDeed )
					{
						int color = 0xB8E;
						string material = "Granite";
						string maker = from.Name;

						Type resourceType = typeRes;
						if ( resourceType == null )
							resourceType = Resources.GetAt( 0 ).ItemType;

						CraftResource thisResource = CraftResources.GetFromType( resourceType );

						switch ( thisResource )
						{
							case CraftResource.DullCopper: color = MaterialInfo.GetMaterialColor( "dull copper", "classic", 0 ); material = "Dull Copper Granite"; break;
							case CraftResource.ShadowIron: color = MaterialInfo.GetMaterialColor( "shadow iron", "classic", 0 ); material = "Shadow Iron Granite"; break;
							case CraftResource.Copper: color = MaterialInfo.GetMaterialColor( "copper", "classic", 0 ); material = "Copper Granite"; break;
							case CraftResource.Bronze: color = MaterialInfo.GetMaterialColor( "bronze", "classic", 0 ); material = "Bronze Granite"; break;
							case CraftResource.Gold: color = MaterialInfo.GetMaterialColor( "gold", "classic", 0 ); material = "Gold Granite"; break;
							case CraftResource.Agapite: color = MaterialInfo.GetMaterialColor( "agapite", "classic", 0 ); material = "Agapite Granite"; break;
							case CraftResource.Verite: color = MaterialInfo.GetMaterialColor( "verite", "classic", 0 ); material = "Verite Granite"; break;
							case CraftResource.Valorite: color = MaterialInfo.GetMaterialColor( "valorite", "classic", 0 ); material = "Valorite Granite"; break;
							case CraftResource.Nepturite: color = MaterialInfo.GetMaterialColor( "nepturite", "classic", 0 ); material = "Nepturite Granite"; break;
							case CraftResource.Obsidian: color = MaterialInfo.GetMaterialColor( "obsidian", "classic", 0 ); material = "Obsidian Granite"; break;
							case CraftResource.Mithril: color = MaterialInfo.GetMaterialColor( "mithril", "classic", 0 ); material = "Mithril Granite"; break;
							case CraftResource.Xormite: color = MaterialInfo.GetMaterialColor( "xormite", "classic", 0 ); material = "Xormite Granite"; break;
							case CraftResource.Dwarven: color = MaterialInfo.GetMaterialColor( "dwarven", "classic", 0 ); material = "Dwarven Granite"; break;
						}

						Server.Items.Statues.SetStatue( (BaseStatueDeed)item, (int)item.Weight, color, material, maker, item.Name );
					}
					else if ( craftSystem is DefMasonry )
					{
						int color = 0;

						Type resourceType = typeRes;
						if ( resourceType == null )
							resourceType = Resources.GetAt( 0 ).ItemType;

						CraftResource thisResource = CraftResources.GetFromType( resourceType );

						switch ( thisResource )
						{
							case CraftResource.DullCopper: color = MaterialInfo.GetMaterialColor( "dull copper", "classic", 0 ); break;
							case CraftResource.ShadowIron: color = MaterialInfo.GetMaterialColor( "shadow iron", "classic", 0 ); break;
							case CraftResource.Copper: color = MaterialInfo.GetMaterialColor( "copper", "classic", 0 ); break;
							case CraftResource.Bronze: color = MaterialInfo.GetMaterialColor( "bronze", "classic", 0 ); break;
							case CraftResource.Gold: color = MaterialInfo.GetMaterialColor( "gold", "classic", 0 ); break;
							case CraftResource.Agapite: color = MaterialInfo.GetMaterialColor( "agapite", "classic", 0 ); break;
							case CraftResource.Verite: color = MaterialInfo.GetMaterialColor( "verite", "classic", 0 ); break;
							case CraftResource.Valorite: color = MaterialInfo.GetMaterialColor( "valorite", "classic", 0 ); break;
							case CraftResource.Nepturite: color = MaterialInfo.GetMaterialColor( "nepturite", "classic", 0 ); break;
							case CraftResource.Obsidian: color = MaterialInfo.GetMaterialColor( "obsidian", "classic", 0 ); break;
							case CraftResource.Mithril: color = MaterialInfo.GetMaterialColor( "mithril", "classic", 0 ); break;
							case CraftResource.Xormite: color = MaterialInfo.GetMaterialColor( "xormite", "classic", 0 ); break;
							case CraftResource.Dwarven: color = MaterialInfo.GetMaterialColor( "dwarven", "classic", 0 ); break;
						}

						item.Hue = color;
					}
					else if ( item is TrapKit )
					{
						Type resourceType = typeRes;
						if ( resourceType == null )
							resourceType = Resources.GetAt( 0 ).ItemType;

						CraftResource thisResource = CraftResources.GetFromType( resourceType );

						switch ( thisResource )
						{
							case CraftResource.DullCopper: 	item.Hue = MaterialInfo.GetMaterialColor( "dull copper", "classic", 0 ); ((TrapKit)item).m_Metal = "Dull Copper"; 	break;
							case CraftResource.ShadowIron: 	item.Hue = MaterialInfo.GetMaterialColor( "shadow iron", "classic", 0 ); ((TrapKit)item).m_Metal = "Shadow Iron"; 	break;
							case CraftResource.Copper: 		item.Hue = MaterialInfo.GetMaterialColor( "copper", "classic", 0 ); ((TrapKit)item).m_Metal = "Copper"; 			break;
							case CraftResource.Bronze: 		item.Hue = MaterialInfo.GetMaterialColor( "bronze", "classic", 0 ); ((TrapKit)item).m_Metal = "Bronze"; 			break;
							case CraftResource.Gold: 		item.Hue = MaterialInfo.GetMaterialColor( "gold", "classic", 0 ); ((TrapKit)item).m_Metal = "Gold"; 				break;
							case CraftResource.Agapite: 	item.Hue = MaterialInfo.GetMaterialColor( "agapite", "classic", 0 ); ((TrapKit)item).m_Metal = "Agapite"; 			break;
							case CraftResource.Verite: 		item.Hue = MaterialInfo.GetMaterialColor( "verite", "classic", 0 ); ((TrapKit)item).m_Metal = "Verite"; 			break;
							case CraftResource.Valorite: 	item.Hue = MaterialInfo.GetMaterialColor( "valorite", "classic", 0 ); ((TrapKit)item).m_Metal = "Valorite"; 		break;
							case CraftResource.Nepturite: 	item.Hue = MaterialInfo.GetMaterialColor( "nepturite", "classic", 0 ); ((TrapKit)item).m_Metal = "Nepturite"; 		break;
							case CraftResource.Obsidian: 	item.Hue = MaterialInfo.GetMaterialColor( "obsidian", "classic", 0 ); ((TrapKit)item).m_Metal = "Obsidian"; 		break;
							case CraftResource.Steel: 		item.Hue = MaterialInfo.GetMaterialColor( "steel", "classic", 0 ); ((TrapKit)item).m_Metal = "Steel"; 				break;
							case CraftResource.Brass: 		item.Hue = MaterialInfo.GetMaterialColor( "brass", "classic", 0 ); ((TrapKit)item).m_Metal = "Brass"; 				break;
							case CraftResource.Mithril: 	item.Hue = MaterialInfo.GetMaterialColor( "mithril", "classic", 0 ); ((TrapKit)item).m_Metal = "Mithril"; 			break;
							case CraftResource.Xormite: 	item.Hue = MaterialInfo.GetMaterialColor( "xormite", "classic", 0 ); ((TrapKit)item).m_Metal = "Xormite"; 			break;
							case CraftResource.Dwarven: 	item.Hue = MaterialInfo.GetMaterialColor( "dwarven", "classic", 0 ); ((TrapKit)item).m_Metal = "Dwarven"; 			break;
						}
					}
					else if ( item is ShortMusicStand || 
						item is Backpack || 
						item is Pouch || 
						item is Bag || 
						item is LargeBag || 
						item is GiantBag || 
						item is LargeSack || 
						item is Scales || 
						item is Key || 
						item is Globe || 
						item is WindChimes || 
						item is FancyWindChimes || 
						item is TallMusicStand || 
						item is Easle || 
						item is ShojiScreen || 
						item is BambooScreen || 
						item is FootStool || 
						item is Stool || 
						item is BambooChair || 
						item is WoodenChair || 
						item is WoodenCoffin || 
						item is WoodenCasket || 
						item is StoneCoffin || 
						item is StoneCasket || 
						item is RockUrn || 
						item is RockVase || 
						item is FancyWoodenChairCushion || 
						item is WoodenChairCushion || 
						item is WoodenBench || 
						item is WoodenThrone || 
						item is Throne || 
						item is Nightstand || 
						item is WritingTable || 
						item is YewWoodTable || 
						item is LargeTable || 
						item is ElegantLowTable || 
						item is PlainLowTable || 
						item is CandleLarge || 
						item is Candelabra || 
						item is CandelabraStand || 
						item is WoodenBox || 
						item is WoodenChest || 
						item is SmallCrate || 
						item is MediumCrate || 
						item is LargeCrate || 
						item is AdventurerCrate || 
						item is AlchemyCrate || 
						item is ArmsCrate || 
						item is BakerCrate || 
						item is BeekeeperCrate || 
						item is BlacksmithCrate || 
						item is BowyerCrate || 
						item is ButcherCrate || 
						item is CarpenterCrate || 
						item is FletcherCrate || 
						item is HealerCrate || 
						item is HugeCrate || 
						item is JewelerCrate || 
						item is LibrarianCrate || 
						item is MusicianCrate || 
						item is NecromancerCrate || 
						item is ProvisionerCrate || 
						item is SailorCrate || 
						item is StableCrate || 
						item is SupplyCrate || 
						item is TailorCrate || 
						item is TavernCrate || 
						item is TinkerCrate || 
						item is TreasureCrate || 
						item is WizardryCrate || 
						item is SailorShelf || 
						item is ColoredArmoireA || 
						item is ColoredArmoireB || 
						item is ColoredCabinetA || 
						item is ColoredCabinetB || 
						item is ColoredCabinetC || 
						item is ColoredCabinetD || 
						item is ColoredCabinetE || 
						item is ColoredCabinetF || 
						item is ColoredCabinetG || 
						item is ColoredCabinetH || 
						item is ColoredCabinetI || 
						item is ColoredCabinetJ || 
						item is ColoredCabinetK || 
						item is ColoredCabinetL || 
						item is ColoredCabinetM || 
						item is ColoredCabinetN || 
						item is ColoredDresserA || 
						item is ColoredDresserB || 
						item is ColoredDresserC || 
						item is ColoredDresserD || 
						item is ColoredDresserE || 
						item is ColoredDresserF || 
						item is ColoredDresserG || 
						item is ColoredDresserH || 
						item is ColoredDresserI || 
						item is ColoredDresserJ || 
						item is ColoredShelf1 || 
						item is ColoredShelf2 || 
						item is ColoredShelf3 || 
						item is ColoredShelf4 || 
						item is ColoredShelf5 || 
						item is ColoredShelf6 || 
						item is ColoredShelf7 || 
						item is ColoredShelf8 || 
						item is ColoredShelfA || 
						item is ColoredShelfB || 
						item is ColoredShelfC || 
						item is ColoredShelfD || 
						item is ColoredShelfE || 
						item is ColoredShelfF || 
						item is ColoredShelfG || 
						item is ColoredShelfH || 
						item is ColoredShelfI || 
						item is ColoredShelfJ || 
						item is ColoredShelfK || 
						item is ColoredShelfL || 
						item is ColoredShelfM || 
						item is ColoredShelfN || 
						item is ColoredShelfO || 
						item is ColoredShelfP || 
						item is ColoredShelfQ || 
						item is ColoredShelfR || 
						item is ColoredShelfS || 
						item is ColoredShelfT || 
						item is ColoredShelfU || 
						item is ColoredShelfV || 
						item is ColoredShelfW || 
						item is ColoredShelfX || 
						item is ColoredShelfY || 
						item is ColoredShelfZ || 
						item is EmptyBookcase || 
						item is FancyArmoire || 
						item is Armoire || 
						item is PlainWoodenChest || 
						item is OrnateWoodenChest || 
						item is GildedWoodenChest || 
						item is WoodenFootLocker || 
						item is FinishedWoodenChest || 
						item is TallCabinet || 
						item is ShortCabinet || 
						item is RedArmoire || 
						item is ElegantArmoire || 
						item is MapleArmoire || 
						item is CherryArmoire )
						{
							int cHue = 0;

							if ( item is WoodenChest ){ cHue = 0x724; }
							else if ( item is WoodenThrone ){ cHue = 0x840; }
							else if ( item is GiantBag ){ cHue = 0x83E; }
							else if ( item is LargeSack ){ cHue = 0x83F; }
							else if ( item is Bag ){ cHue = 0xABE; }
							else if ( item is AdventurerCrate ){ cHue = 0xABE; }
							else if ( item is AlchemyCrate ){ cHue = 0xABE; }
							else if ( item is ArmsCrate ){ cHue = 0xABE; }
							else if ( item is BakerCrate ){ cHue = 0xABE; }
							else if ( item is BeekeeperCrate ){ cHue = 0xABE; }
							else if ( item is BlacksmithCrate ){ cHue = 0xABE; }
							else if ( item is BowyerCrate ){ cHue = 0xABE; }
							else if ( item is ButcherCrate ){ cHue = 0xABE; }
							else if ( item is CarpenterCrate ){ cHue = 0xABE; }
							else if ( item is FletcherCrate ){ cHue = 0xABE; }
							else if ( item is HealerCrate ){ cHue = 0xABE; }
							else if ( item is HugeCrate ){ cHue = 0xABE; }
							else if ( item is JewelerCrate ){ cHue = 0xABE; }
							else if ( item is LibrarianCrate ){ cHue = 0xABE; }
							else if ( item is MusicianCrate ){ cHue = 0xABE; }
							else if ( item is NecromancerCrate ){ cHue = 0xABE; }
							else if ( item is ProvisionerCrate ){ cHue = 0xABE; }
							else if ( item is SailorCrate ){ cHue = 0xABE; }
							else if ( item is StableCrate ){ cHue = 0xABE; }
							else if ( item is SupplyCrate ){ cHue = 0xABE; }
							else if ( item is TailorCrate ){ cHue = 0xABE; }
							else if ( item is TavernCrate ){ cHue = 0xABE; }
							else if ( item is TinkerCrate ){ cHue = 0xABE; }
							else if ( item is TreasureCrate ){ cHue = 0xABE; }
							else if ( item is WizardryCrate ){ cHue = 0xABE; }
							else if ( item is ColoredArmoireA ){ cHue = 0xABE; }
							else if ( item is ColoredArmoireB ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetA ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetB ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetC ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetD ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetE ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetF ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetG ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetH ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetI ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetJ ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetK ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetL ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetM ){ cHue = 0xABE; }
							else if ( item is ColoredCabinetN ){ cHue = 0xABE; }
							else if ( item is ColoredDresserA ){ cHue = 0xABE; }
							else if ( item is ColoredDresserB ){ cHue = 0xABE; }
							else if ( item is ColoredDresserC ){ cHue = 0xABE; }
							else if ( item is ColoredDresserD ){ cHue = 0xABE; }
							else if ( item is ColoredDresserE ){ cHue = 0xABE; }
							else if ( item is ColoredDresserF ){ cHue = 0xABE; }
							else if ( item is ColoredDresserG ){ cHue = 0xABE; }
							else if ( item is ColoredDresserH ){ cHue = 0xABE; }
							else if ( item is ColoredDresserI ){ cHue = 0xABE; }
							else if ( item is ColoredDresserJ ){ cHue = 0xABE; }
							else if ( item is ColoredShelf1 ){ cHue = 0xABE; }
							else if ( item is ColoredShelf2 ){ cHue = 0xABE; }
							else if ( item is ColoredShelf3 ){ cHue = 0xABE; }
							else if ( item is ColoredShelf4 ){ cHue = 0xABE; }
							else if ( item is ColoredShelf5 ){ cHue = 0xABE; }
							else if ( item is ColoredShelf6 ){ cHue = 0xABE; }
							else if ( item is ColoredShelf7 ){ cHue = 0xABE; }
							else if ( item is ColoredShelf8 ){ cHue = 0xABE; }
							else if ( item is ColoredShelfA ){ cHue = 0xABE; }
							else if ( item is ColoredShelfB ){ cHue = 0xABE; }
							else if ( item is ColoredShelfC ){ cHue = 0xABE; }
							else if ( item is ColoredShelfD ){ cHue = 0xABE; }
							else if ( item is ColoredShelfE ){ cHue = 0xABE; }
							else if ( item is ColoredShelfF ){ cHue = 0xABE; }
							else if ( item is ColoredShelfG ){ cHue = 0xABE; }
							else if ( item is ColoredShelfH ){ cHue = 0xABE; }
							else if ( item is ColoredShelfI ){ cHue = 0xABE; }
							else if ( item is ColoredShelfJ ){ cHue = 0xABE; }
							else if ( item is ColoredShelfK ){ cHue = 0xABE; }
							else if ( item is ColoredShelfL ){ cHue = 0xABE; }
							else if ( item is ColoredShelfM ){ cHue = 0xABE; }
							else if ( item is ColoredShelfN ){ cHue = 0xABE; }
							else if ( item is ColoredShelfO ){ cHue = 0xABE; }
							else if ( item is ColoredShelfP ){ cHue = 0xABE; }
							else if ( item is ColoredShelfQ ){ cHue = 0xABE; }
							else if ( item is ColoredShelfR ){ cHue = 0xABE; }
							else if ( item is ColoredShelfS ){ cHue = 0xABE; }
							else if ( item is ColoredShelfT ){ cHue = 0xABE; }
							else if ( item is ColoredShelfU ){ cHue = 0xABE; }
							else if ( item is ColoredShelfV ){ cHue = 0xABE; }
							else if ( item is ColoredShelfW ){ cHue = 0xABE; }
							else if ( item is ColoredShelfX ){ cHue = 0xABE; }
							else if ( item is ColoredShelfY ){ cHue = 0xABE; }
							else if ( item is ColoredShelfZ ){ cHue = 0xABE; }

							Type resourceType = typeRes;
							if ( resourceType == null )
								resourceType = Resources.GetAt( 0 ).ItemType;

							CraftResource thisResource = CraftResources.GetFromType( resourceType );

							switch ( thisResource )
							{
								case CraftResource.DullCopper: cHue = MaterialInfo.GetMaterialColor( "dull copper", "classic", 0 ); break;
								case CraftResource.ShadowIron: cHue = MaterialInfo.GetMaterialColor( "shadow iron", "classic", 0 ); break;
								case CraftResource.Copper: cHue = MaterialInfo.GetMaterialColor( "copper", "classic", 0 ); break;
								case CraftResource.Bronze: cHue = MaterialInfo.GetMaterialColor( "bronze", "classic", 0 ); break;
								case CraftResource.Gold: cHue = MaterialInfo.GetMaterialColor( "gold", "classic", 0 ); break;
								case CraftResource.Agapite: cHue = MaterialInfo.GetMaterialColor( "agapite", "classic", 0 ); break;
								case CraftResource.Verite: cHue = MaterialInfo.GetMaterialColor( "verite", "classic", 0 ); break;
								case CraftResource.Valorite: cHue = MaterialInfo.GetMaterialColor( "valorite", "classic", 0 ); break;
								case CraftResource.Obsidian: cHue = MaterialInfo.GetMaterialColor( "obsidian", "classic", 0 ); break;
								case CraftResource.Steel: cHue = MaterialInfo.GetMaterialColor( "steel", "classic", 0 ); break;
								case CraftResource.Brass: cHue = MaterialInfo.GetMaterialColor( "brass", "classic", 0 ); break;
								case CraftResource.Mithril: cHue = MaterialInfo.GetMaterialColor( "mithril", "classic", 0 ); break;
								case CraftResource.Xormite: cHue = MaterialInfo.GetMaterialColor( "xormite", "classic", 0 ); break;
								case CraftResource.Dwarven: cHue = MaterialInfo.GetMaterialColor( "dwarven", "classic", 0 ); break;
								case CraftResource.Nepturite: cHue = MaterialInfo.GetMaterialColor( "nepturite", "classic", 0 ); break;
								case CraftResource.AshTree: cHue = MaterialInfo.GetMaterialColor( "ash", "", 0 ); break;
								case CraftResource.CherryTree: cHue = MaterialInfo.GetMaterialColor( "cherry", "", 0 ); break;
								case CraftResource.EbonyTree: cHue = MaterialInfo.GetMaterialColor( "ebony", "", 0 ); break;
								case CraftResource.GoldenOakTree: cHue = MaterialInfo.GetMaterialColor( "golden oak", "", 0 ); break;
								case CraftResource.HickoryTree: cHue = MaterialInfo.GetMaterialColor( "hickory", "", 0 ); break;
								case CraftResource.MahoganyTree: cHue = MaterialInfo.GetMaterialColor( "mahogany", "", 0 ); break;
								case CraftResource.DriftwoodTree: cHue = MaterialInfo.GetMaterialColor( "driftwood", "", 0 ); break;
								case CraftResource.OakTree: cHue = MaterialInfo.GetMaterialColor( "oak", "", 0 ); break;
								case CraftResource.PineTree: cHue = MaterialInfo.GetMaterialColor( "pine", "", 0 ); break;
								case CraftResource.GhostTree: cHue = MaterialInfo.GetMaterialColor( "ghostwood", "", 0 ); break;
								case CraftResource.RosewoodTree: cHue = MaterialInfo.GetMaterialColor( "rosewood", "", 0 ); break;
								case CraftResource.WalnutTree: cHue = MaterialInfo.GetMaterialColor( "walnut", "", 0 ); break;
								case CraftResource.PetrifiedTree: cHue = MaterialInfo.GetMaterialColor( "petrified", "", 0 ); break;
								case CraftResource.ElvenTree: cHue = MaterialInfo.GetMaterialColor( "elven", "", 0 ); break;
								case CraftResource.SpinedLeather: cHue = MaterialInfo.GetMaterialColor( "deep sea", "", 0 ); break;
								case CraftResource.HornedLeather: cHue = MaterialInfo.GetMaterialColor( "lizard", "", 0 ); break;
								case CraftResource.BarbedLeather: cHue = MaterialInfo.GetMaterialColor( "serpent", "", 0 ); break;
								case CraftResource.NecroticLeather: cHue = MaterialInfo.GetMaterialColor( "necrotic", "", 0 ); break;
								case CraftResource.VolcanicLeather: cHue = MaterialInfo.GetMaterialColor( "volcanic", "", 0 ); break;
								case CraftResource.FrozenLeather: cHue = MaterialInfo.GetMaterialColor( "frozen", "", 0 ); break;
								case CraftResource.GoliathLeather: cHue = MaterialInfo.GetMaterialColor( "goliath", "", 0 ); break;
								case CraftResource.DraconicLeather: cHue = MaterialInfo.GetMaterialColor( "draconic", "", 0 ); break;
								case CraftResource.HellishLeather: cHue = MaterialInfo.GetMaterialColor( "hellish", "", 0 ); break;
								case CraftResource.DinosaurLeather: cHue = MaterialInfo.GetMaterialColor( "dinosaur", "", 0 ); break;
								case CraftResource.AlienLeather: cHue = MaterialInfo.GetMaterialColor( "alien", "", 0 ); break;
							}

							item.Hue = cHue;
					}

					if ( item is BaseArmor || item is BaseWeapon ) // ELVEN WOOD
					{
						Type resourceType = typeRes;
						if ( resourceType == null )
							resourceType = Resources.GetAt( 0 ).ItemType;

						CraftResource thisResource = CraftResources.GetFromType( resourceType );

						if ( thisResource == CraftResource.ElvenTree )
						{
							if ( item is BaseWeapon ){ ((BaseWeapon)item).Attributes.SpellChanneling = 1; ((BaseWeapon)item).Attributes.NightSight = 1; BaseRunicTool.ApplyAttributesTo( (BaseWeapon)item, false, 0, 1, 5, 10 ); }
							else if ( item is BaseShield ){ ((BaseShield)item).Attributes.SpellChanneling = 1; ((BaseShield)item).Attributes.NightSight = 1; BaseRunicTool.ApplyAttributesTo( (BaseShield)item, false, 0, 1, 5, 10 ); }
							else if ( item is BaseArmor ){ ((BaseArmor)item).ArmorAttributes.MageArmor = 1; ((BaseArmor)item).Attributes.NightSight = 1; BaseRunicTool.ApplyAttributesTo( (BaseArmor)item, false, 0, 1, 5, 10 ); }
						}
						else if ( thisResource == CraftResource.Dwarven )
						{
							if ( item is BaseWeapon ){ ((BaseWeapon)item).Attributes.RegenHits = 5; ((BaseWeapon)item).Attributes.AttackChance = 50; BaseRunicTool.ApplyAttributesTo( (BaseWeapon)item, false, 0, 1, 5, 10 ); }
							else if ( item is BaseShield ){ ((BaseShield)item).Attributes.RegenHits = 5; ((BaseShield)item).Attributes.DefendChance = 10; BaseRunicTool.ApplyAttributesTo( (BaseShield)item, false, 0, 1, 5, 10 ); }
							else if ( item is BaseArmor ){ ((BaseArmor)item).Attributes.RegenHits = 5; ((BaseArmor)item).Attributes.DefendChance = 10; BaseRunicTool.ApplyAttributesTo( (BaseArmor)item, false, 0, 1, 5, 10 ); }
						}
						else if ( thisResource == CraftResource.AlienLeather )
						{
							if ( item is BaseWeapon ){ ((BaseWeapon)item).Attributes.RegenMana = 5; ((BaseWeapon)item).Attributes.WeaponSpeed = 30; BaseRunicTool.ApplyAttributesTo( (BaseWeapon)item, false, 0, 1, 5, 10 ); }
							else if ( item is BaseArmor ){ ((BaseArmor)item).Attributes.RegenMana = 5; ((BaseArmor)item).Attributes.WeaponSpeed = 10; BaseRunicTool.ApplyAttributesTo( (BaseArmor)item, false, 0, 1, 5, 10 ); }
						}

                        else if (thisResource == CraftResource.RedScales)
                        {
                            if (item is BaseArmor) { ((BaseArmor)item).Attributes.RegenHits = 3; ((BaseArmor)item).Attributes.BonusHits = 5; BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, 0, 1, 5, 10); }
                        }
                        else if (thisResource == CraftResource.YellowScales)
                        {
                            if (item is BaseArmor) { ((BaseArmor)item).Attributes.RegenStam = 3; ((BaseArmor)item).Attributes.BonusStam = 5; ((BaseArmor)item).Attributes.Luck = 55; BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, 0, 1, 5, 10); }
                        }
                        else if (thisResource == CraftResource.BlackScales)
                        {
                            if (item is BaseArmor) { ((BaseArmor)item).Attributes.ReflectPhysical = 7; ((BaseArmor)item).Attributes.DefendChance = 5; ((BaseArmor)item).Attributes.BonusStr = 3; BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, 0, 1, 5, 10); }
                        }
                        else if (thisResource == CraftResource.GreenScales)
                        {
                            if (item is BaseArmor) { ((BaseArmor)item).Attributes.EnhancePotions = 5; ((BaseArmor)item).Attributes.WeaponSpeed = 5; ((BaseArmor)item).Attributes.BonusDex = 5; BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, 0, 1, 5, 10); }
                        }
                        else if (thisResource == CraftResource.WhiteScales)
                        {
                            if (item is BaseArmor) { ((BaseArmor)item).Attributes.SpellDamage = 7; ((BaseArmor)item).Attributes.LowerManaCost = 7; ((BaseArmor)item).ArmorAttributes.MageArmor = 1; BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, 0, 1, 5, 10); }
                        }
                        else if (thisResource == CraftResource.WhiteScales)
                        {
                            if (item is BaseArmor) { ((BaseArmor)item).Attributes.LowerRegCost = 10; ((BaseArmor)item).Attributes.RegenMana = 3; ((BaseArmor)item).ArmorAttributes.MageArmor = 1; BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, 0, 1, 5, 10); }
                        }
                        else if (thisResource == CraftResource.DinosaurScales)
                        {
                            if (item is BaseArmor) { ((BaseArmor)item).Attributes.AttackChance = 5; ((BaseArmor)item).Attributes.WeaponDamage = 7; ((BaseArmor)item).Attributes.WeaponSpeed = 3; BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, 0, 1, 5, 10); }
                        }
                    }

					if (item is Food)
					{
						Food fd = (Food)item;

						string title = GetPlayerInfo.GetSkillTitle( from );
						fd.Cook = from.Name + " " + title;
						fd.CookMobile = from;

						double minMainSkill = 0.0;
						double maxMainSkill = 0.0;
						int bn = 0;

						for ( int i = 0; i < m_arCraftSkill.Count; i++)
						{
							CraftSkill craftSkill = m_arCraftSkill.GetAt(i);

							double MakeCookinGreatAgain = (craftSkill.MinSkill + craftSkill.MaxSkill) /2;
							
							int bnn = 0;

							if (MakeCookinGreatAgain < 50)
								bnn = 1;
							else if (MakeCookinGreatAgain < 60)
								bnn = 2;
							else if (MakeCookinGreatAgain < 70)
								bnn = 4;
							else if (MakeCookinGreatAgain < 80)
								bnn = 6;
							else if (MakeCookinGreatAgain < 90)
								bnn = 8;
							else if (MakeCookinGreatAgain < 100)
								bnn = 12;
							else if (MakeCookinGreatAgain >= 100)
								bnn = 16;
	
							if (bn < (int)(( (from.Skills[craftSkill.SkillToMake].Value + MakeCookinGreatAgain) / 200 ) * 5) ); 
								bn = (int)(( (from.Skills[craftSkill.SkillToMake].Value + MakeCookinGreatAgain) / 200 ) * 5); 
							
							if (bnn > fd.Benefit)
								fd.Benefit = bnn;
						}
						
						fd.Benefit += bn;
					}

                    if (outputContainer != null && (
							!(outputContainer is LargeSack) // Not a large sack
							|| ((LargeSack)outputContainer).CanAdd(from, item)) // Is a large sack AND can add ... try to suppress message
						)
					{
						if (!outputContainer.OnDragDrop(from, item)) // Needs to respect coinpouches, etc
						{
							// Fallback to backpack on failure
							from.AddToBackpack(item);
						}
                    }
					else
					{
                        from.AddToBackpack(item);
                    }

                    if( from.AccessLevel > AccessLevel.Player )
						CommandLogging.WriteLine( from, "Crafting {0} with craft system {1}", CommandLogging.Format( item ), craftSystem.GetType().Name );

					//from.PlaySound( 0x57 );
				}

				if ( num == 0 )
					num = craftSystem.PlayEndingEffect( from, false, true, toolBroken, endquality, makersMark, this );


				// TODO: Scroll imbuing

				if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
					from.SendGump( new CraftGump( from, craftSystem, tool, num ) );
				else if ( num > 0 )
					from.SendLocalizedMessage( num );
			}
			else if ( !allRequiredSkills )
			{
				if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
					from.SendGump( new CraftGump( from, craftSystem, tool, 1044153 ) );
				else
					from.SendLocalizedMessage( 1044153 ); // You don't have the required skills to attempt this item.
			}
			else
			{
				ConsumeType consumeType = ( UseAllRes ? ConsumeType.Half : ConsumeType.All );
				int resHue = 0;
				int maxAmount = 0;

                object message = null;

                // Not enough resource to craft it
                if ( !ConsumeRes( from, typeRes, craftSystem, ref resHue, ref maxAmount, consumeType, ref message, true ) )
				{
					if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
						from.SendGump( new CraftGump( from, craftSystem, tool, message ) );
					else if ( message is int && (int)message > 0 )
						from.SendLocalizedMessage( (int)message );
					else if ( message is string )
						from.SendMessage( (string)message );

					return;
				}

				tool.UsesRemaining--;

				if ( tool.UsesRemaining < 1 )
					toolBroken = true;

				if ( toolBroken )
					tool.Delete();

				// SkillCheck failed.
				int num = craftSystem.PlayEndingEffect( from, true, true, toolBroken, endquality, false, this );

				if ( tool != null && !tool.Deleted && tool.UsesRemaining > 0 )
					from.SendGump( new CraftGump( from, craftSystem, tool, num ) );
				else if ( num > 0 )
					from.SendLocalizedMessage( num );
			}
		}

		private class InternalTimer : Timer
		{
			private Mobile m_From;
			private int m_iCount;
			private int m_iCountMax;
			private CraftItem m_CraftItem;
			private CraftSystem m_CraftSystem;
			private Type m_TypeRes;
			private BaseTool m_Tool;

			public InternalTimer( Mobile from, CraftSystem craftSystem, CraftItem craftItem, Type typeRes, BaseTool tool, int iCountMax ) : base( TimeSpan.Zero, TimeSpan.FromSeconds( craftSystem.Delay ), iCountMax )
			{
				m_From = from;
				m_CraftItem = craftItem;
				m_iCount = 0;
				m_iCountMax = iCountMax;
				m_CraftSystem = craftSystem;
				m_TypeRes = typeRes;
				m_Tool = tool;
			}

			protected override void OnTick()
			{
				m_iCount++;

				m_From.DisruptiveAction();

				if ( m_iCount < m_iCountMax )
				{
					m_CraftSystem.PlayCraftEffect( m_From );
				}
				else
				{
					m_From.EndAction( typeof( CraftSystem ) );

					int badCraft = m_CraftSystem.CanCraft( m_From, m_Tool, m_CraftItem.m_Type );

					if ( badCraft > 0 )
					{
						if ( m_Tool != null && !m_Tool.Deleted && m_Tool.UsesRemaining > 0 )
							m_From.SendGump( new CraftGump( m_From, m_CraftSystem, m_Tool, badCraft ) );
						else
							m_From.SendLocalizedMessage( badCraft );

						return;
					}

					int quality = 1;
					bool allRequiredSkills = true;

					m_CraftItem.CheckSkills( m_From, m_TypeRes, m_CraftSystem, ref quality, ref allRequiredSkills, false );

					CraftContext context = m_CraftSystem.GetContext( m_From );

					if ( context == null )
						return;

					if ( typeof( CustomCraft ).IsAssignableFrom( m_CraftItem.ItemType ) )
					{
						CustomCraft cc = null;

						try{ cc = Activator.CreateInstance( m_CraftItem.ItemType, new object[] { m_From, m_CraftItem, m_CraftSystem, m_TypeRes, m_Tool, quality } ) as CustomCraft; }
						catch{}

						if ( cc != null )
							cc.EndCraftAction();

						return;
					}

					bool makersMark = false;

					if ( quality == 2 && m_From.Skills[m_CraftSystem.MainSkill].Value >= 100.0 )
						makersMark = m_CraftItem.IsMarkable( m_CraftItem.ItemType );

					if ( makersMark && context.MarkOption == CraftMarkOption.PromptForMark )
					{
						m_From.SendGump( new QueryMakersMarkGump( quality, m_From, m_CraftItem, m_CraftSystem, m_TypeRes, m_Tool ) );
					}
					else
					{
						if ( context.MarkOption == CraftMarkOption.DoNotMark )
							makersMark = false;

						m_CraftItem.CompleteCraft( quality, makersMark, m_From, m_CraftSystem, m_TypeRes, m_Tool, null );
					}
				}
			}
		}
	}
}
