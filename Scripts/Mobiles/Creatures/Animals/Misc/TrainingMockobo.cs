using System;
using System.Collections;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a bird corpse" )]
	public class TrainingMockobo : BaseCreature
	{

		[Constructable]
		public TrainingMockobo() : this("a mockobo")
		{
		}

		[Constructable]
		public TrainingMockobo( string name ) : base( name, 25, 0x19, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a mockabo";
			BaseSoundID = 0x2EE;
            Hue = 0x36;

			SetStr( 1 );
			SetDex( 1 );
			SetInt( 1 );

			SetHits( 1 );
			SetMana( 0 );

			SetDamage( 1 );

			SetDamageType( ResistanceType.Physical, 1 );

			SetResistance( ResistanceType.Physical, 1, 2 );

			SetSkill( SkillName.MagicResist, 5.0 );
			SetSkill( SkillName.Tactics, 4.0 );
			SetSkill( SkillName.Wrestling, 5.0 );

			Fame = 0;
			Karma = 15000;

			VirtualArmor = 1;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = -0.9;
		}

		public override void OnAfterSpawn()
		{
			base.OnAfterSpawn();

			// Training Mockobos
            if (this.Map == Map.Trammel && this.Home.X == 3636 && this.Home.Y == 3490)
            {
				this.Direction = Direction.East;
				this.ControlSlots = 1
                this.Hue = 0x36;
                this.Name = "25";
				this.MinTameSkill = 25;
            }
            if (this.Map == Map.Trammel && this.Home.X == 3636 && this.Home.Y == 3488)
            {
				this.Direction = Direction.East;
				this.ControlSlots = 1
                this.Hue = 0xBB4;
                this.Name = "40";
				this.MinTameSkill = 40;
            }
            if (this.Map == Map.Trammel && this.Home.X == 3636 && this.Home.Y == 3486)
            {
				this.Direction = Direction.East;
				this.ControlSlots = 1
                this.Hue = 0xBA2;
                this.Name = "50";
				this.MinTameSkill = 50;
            }
            if (this.Map == Map.Trammel && this.Home.X == 3636 && this.Home.Y == 3484)
            {
				this.Direction = Direction.East;
				this.ControlSlots = 1
                this.Hue = 0x92C;
                this.Name = "60";
				this.MinTameSkill = 60;
            }
            if (this.Map == Map.Trammel && this.Home.X == 3636 && this.Home.Y == 3482)
            {
				this.Direction = Direction.East;
				this.ControlSlots = 1
                this.Hue = 0x94C;
                this.Name = "70";
				this.MinTameSkill = 70;
            }
            if (this.Map == Map.Trammel && this.Home.X == 3636 && this.Home.Y == 3480)
            {
				this.Direction = Direction.East;
				this.ControlSlots = 1
                this.Hue = 0x929;
                this.Name = "80";
				this.MinTameSkill = 80;
            }
            if (this.Map == Map.Trammel && this.Home.X == 3636 && this.Home.Y == 3478)
            {
				this.Direction = Direction.East;
				this.ControlSlots = 1
                this.Hue = 0xB6F;
                this.Name = "90";
				this.MinTameSkill = 90;
            }
            if (this.Map == Map.Trammel && this.Home.X == 3636 && this.Home.Y == 3476)
            {
				this.Direction = Direction.East;
				this.ControlSlots = 1
                this.Hue = 0xAD4;
                this.Name = "105";
				this.MinTameSkill = 105;
            }
        }

		public override void OnDoubleClick( Mobile from )
		{ 
			if ( this.BaseSoundID == null )
			{
				// This isn't your mount; it refuses to let you ride.
				PrivateOverheadMessage( Network.MessageType.Regular, 0x3B2, 501264, from.NetState );
				return;
			}
			
			base.OnDoubleClick( from );
		}

		public override int Meat { get { return 1; } }
		public override FoodType FavoriteFood { get { return FoodType.Meat; } }
		public override MeatType MeatType{ get{ return MeatType.Bird; } }
		public override int Feathers{ get{ return 1; } }

		public TrainingMockobo( Serial serial ): base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)2 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
