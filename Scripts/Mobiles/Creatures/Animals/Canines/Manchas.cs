using System;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "a pug corpse" )]
	public class Manchas : BaseCreature
	{
		[Constructable]
		public Manchas() : base( AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Name = "Manchas";
			Body = 0xD9;
			Hue = 0x86e;
			BaseSoundID = 0x85;
            Female = false;

			SetStr( 401, 430 );
			SetDex( 133, 152 );
			SetInt( 101, 140 );

			SetHits( 241, 258 );

			SetDamage( 11, 17 );

			SetDamageType( ResistanceType.Physical, 80 );
			SetDamageType( ResistanceType.Fire, 20 );

			SetResistance( ResistanceType.Physical, 45, 50 );
			SetResistance( ResistanceType.Fire, 50, 60 );
			SetResistance( ResistanceType.Cold, 40, 50 );
			SetResistance( ResistanceType.Poison, 20, 30 );
			SetResistance( ResistanceType.Energy, 30, 40 );

			SetSkill( SkillName.MagicResist, 65.1, 80.0 );
			SetSkill( SkillName.Tactics, 65.1, 90.0 );
			SetSkill( SkillName.Wrestling, 65.1, 80.0 );

			Fame = 15000;
			Karma = 15000;
			VirtualArmor = 90;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 85.3;
		}

		public override int Meat{ get{ return 1; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Meat; } }
		public override PackInstinct PackInstinct{ get{ return PackInstinct.Canine; } }

		public override bool CanBeControlledBy(Mobile m)
		{
			PlayerMobile player = m as PlayerMobile;
			if (player != null)
			{
				if ( player.AllFollowers.Exists(f => f is Manchas) )
				{
					player.SendMessage("Manchas refuses to share food with another Manchas.");
					return false;
				}
			}

			return base.CanBeControlledBy(m);
		}

		public Manchas(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}