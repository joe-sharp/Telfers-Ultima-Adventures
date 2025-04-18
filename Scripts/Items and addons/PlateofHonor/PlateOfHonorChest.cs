using System;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Items
{
	[FlipableAttribute( 11016, 11017 )]
	public class PlateOfHonorChest : BaseArmor, IBlacksmithRepairable
    {
		public override int LabelNumber{ get{ return 1074303; } }
		public override int BasePhysicalResistance{ get{ return 8; } }
		public override int BaseFireResistance{ get{ return 5; } }
		public override int BaseColdResistance{ get{ return 5; } }
		public override int BasePoisonResistance{ get{ return 7; } }
		public override int BaseEnergyResistance{ get{ return 5; } }

		public override int InitMinHits{ get{ return 50; } }
		public override int InitMaxHits{ get{ return 65; } }

		public override int AosStrReq{ get{ return 95; } }
		public override int OldStrReq{ get{ return 60; } }

		public override int OldDexBonus{ get{ return -8; } }

		public override int ArmorBase{ get{ return 40; } }

		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Plate; } }

		[Constructable]
		public PlateOfHonorChest() : base( 11016 )
		{
			Weight = 10.0;
			Attributes.RegenHits = 1;
			Attributes.AttackChance = 5;
		}
		
		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );
			
			list.Add( 1072376, "6" );
			
			if ( this.Parent is Mobile )
			{
				if ( this.Hue == 0x47E )
				{
					list.Add( 1072377 );
					list.Add( 1072513, "25" );
				}
			}
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( this.Hue == 0x0 )
			{
				list.Add( 1072378 );
				list.Add( 1072382, "2" );
				list.Add( 1072383, "5" );
				list.Add( 1072384, "5" );
				list.Add( 1072385, "3" );
				list.Add( 1072386, "5" );
				list.Add( 1060450, "3" );
				list.Add( 1072513, "25" );
				list.Add( "Chivalry 10 (total)" );
				list.Add( 1060441 );
			}
		}

		public override bool OnEquip( Mobile from )
		{
			
			Item glove = from.FindItemOnLayer( Layer.Gloves );
			Item pants = from.FindItemOnLayer( Layer.Pants );
			Item neck = from.FindItemOnLayer( Layer.Neck );
			Item helm = from.FindItemOnLayer( Layer.Helm );
			Item arms = from.FindItemOnLayer( Layer.Arms );
			
			if ( glove != null && glove.GetType() == typeof( PlateOfHonorGloves ) && pants != null && pants.GetType() == typeof( PlateOfHonorLegs ) && neck != null && neck.GetType() == typeof( PlateOfHonorGorget ) && helm != null && helm.GetType() == typeof( PlateOfHonorHelm ) && arms != null && arms.GetType() == typeof( PlateOfHonorArms ) )
			{
				Effects.PlaySound( from.Location, from.Map, 503 );
				from.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );

				Hue = 0x47E;
				ArmorAttributes.SelfRepair = 3;
				Attributes.NightSight = 1;
				Attributes.ReflectPhysical = 25;
				SkillBonuses.SetValues( 0, SkillName.Chivalry, 10.0 );
				PhysicalBonus = 2;
				FireBonus = 5;
				ColdBonus = 5;
				PoisonBonus = 3;
				EnergyBonus = 5;


				PlateOfHonorGloves gloves = from.FindItemOnLayer( Layer.Gloves ) as PlateOfHonorGloves;
				PlateOfHonorLegs legs = from.FindItemOnLayer( Layer.Pants ) as PlateOfHonorLegs;
				PlateOfHonorGorget gorget = from.FindItemOnLayer( Layer.Neck ) as PlateOfHonorGorget;
				PlateOfHonorHelm helmet = from.FindItemOnLayer( Layer.Helm ) as PlateOfHonorHelm;
				PlateOfHonorArms arm = from.FindItemOnLayer( Layer.Arms ) as PlateOfHonorArms;

				gloves.Hue = 0x47E;
				gloves.Attributes.NightSight = 1;
				gloves.ArmorAttributes.SelfRepair = 3;
				gloves.PhysicalBonus = 2;
				gloves.FireBonus = 5;
				gloves.ColdBonus = 5;
				gloves.PoisonBonus = 3;
				gloves.EnergyBonus = 5;

				legs.Hue = 0x47E;
				legs.Attributes.NightSight = 1;
				legs.ArmorAttributes.SelfRepair = 3;
				legs.PhysicalBonus = 2;
				legs.FireBonus = 5;
				legs.ColdBonus = 5;
				legs.PoisonBonus = 3;
				legs.EnergyBonus = 5;

				gorget.Hue = 0x47E;
				gorget.Attributes.NightSight = 1;
				gorget.ArmorAttributes.SelfRepair = 3;
				gorget.PhysicalBonus = 2;
				gorget.FireBonus = 5;
				gorget.ColdBonus = 5;
				gorget.PoisonBonus = 3;
				gorget.EnergyBonus = 5;

				helmet.Hue = 0x47E;
				helmet.Attributes.NightSight = 1;
				helmet.ArmorAttributes.SelfRepair = 3;
				helmet.PhysicalBonus = 2;
				helmet.FireBonus = 5;
				helmet.ColdBonus = 5;
				helmet.PoisonBonus = 3;
				helmet.EnergyBonus = 5;
				
				arm.Hue = 0x47E;
				arm.Attributes.NightSight = 1;
				arm.ArmorAttributes.SelfRepair = 3;
				arm.PhysicalBonus = 2;
				arm.FireBonus = 5;
				arm.ColdBonus = 5;
				arm.PoisonBonus = 3;
				arm.EnergyBonus = 5;
				
						
				from.SendLocalizedMessage( 1072391 );
			}
			this.InvalidateProperties();
			return base.OnEquip( from );							
		}

		public override void OnRemoved(IEntity parent )
		{
			if ( parent is Mobile )
			{
				Mobile m = ( Mobile )parent;
				Hue = 0x0;
				Attributes.NightSight = 0;
				ArmorAttributes.SelfRepair = 0;
				Attributes.ReflectPhysical = 0;
				PhysicalBonus = 0;
				FireBonus = 0;
				ColdBonus = 0;
				PoisonBonus = 0;
				EnergyBonus = 0;
				SkillBonuses.SetValues( 0, SkillName.Chivalry, 0.0 );

				if ( m.FindItemOnLayer( Layer.Gloves ) is PlateOfHonorGloves && m.FindItemOnLayer( Layer.Pants ) is PlateOfHonorLegs && m.FindItemOnLayer( Layer.Arms ) is PlateOfHonorArms && m.FindItemOnLayer( Layer.Neck ) is PlateOfHonorGorget && m.FindItemOnLayer( Layer.Helm ) is PlateOfHonorHelm )
				{
					PlateOfHonorGloves gloves = m.FindItemOnLayer( Layer.Gloves ) as PlateOfHonorGloves;
					gloves.Hue = 0x0;
					gloves.Attributes.NightSight = 0;
					gloves.ArmorAttributes.SelfRepair = 0;
					gloves.PhysicalBonus = 0;
					gloves.FireBonus = 0;
					gloves.ColdBonus = 0;
					gloves.PoisonBonus = 0;
					gloves.EnergyBonus = 0;

					PlateOfHonorLegs legs = m.FindItemOnLayer( Layer.Pants ) as PlateOfHonorLegs;
					legs.Hue = 0x0;
					legs.Attributes.NightSight = 0;
					legs.ArmorAttributes.SelfRepair = 0;
					legs.PhysicalBonus = 0;
					legs.FireBonus = 0;
					legs.ColdBonus = 0;
					legs.PoisonBonus = 0;
					legs.EnergyBonus = 0;
					
					PlateOfHonorArms arm = m.FindItemOnLayer( Layer.Arms ) as PlateOfHonorArms;
					arm.Hue = 0x0;
					arm.Attributes.NightSight = 0;
					arm.ArmorAttributes.SelfRepair = 0;
					arm.PhysicalBonus = 0;
					arm.FireBonus = 0;
					arm.ColdBonus = 0;
					arm.PoisonBonus = 0;
					arm.EnergyBonus = 0;

					PlateOfHonorGorget gorget = m.FindItemOnLayer( Layer.Neck ) as PlateOfHonorGorget;
					gorget.Hue = 0x0;
					gorget.Attributes.NightSight = 0;
					gorget.ArmorAttributes.SelfRepair = 0;
					gorget.PhysicalBonus = 0;
					gorget.FireBonus = 0;
					gorget.ColdBonus = 0;
					gorget.PoisonBonus = 0;
					gorget.EnergyBonus = 0;

					PlateOfHonorHelm helmet = m.FindItemOnLayer( Layer.Helm ) as PlateOfHonorHelm;
					helmet.Hue = 0x0;
					helmet.Attributes.NightSight = 0;
					helmet.ArmorAttributes.SelfRepair = 0;
					helmet.PhysicalBonus = 0;
					helmet.FireBonus = 0;
					helmet.ColdBonus = 0;
					helmet.PoisonBonus = 0;
					helmet.EnergyBonus = 0;
				}
				this.InvalidateProperties();
			}
			base.OnRemoved( parent );
		}

		public PlateOfHonorChest( Serial serial ) : base( serial )
		{
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( Weight == 1.0 )
				Weight = 10.0;
		}
	}
}