using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	public class PerfectMiningHarvester : BaseHarvester
	{
		[Constructable]
		public PerfectMiningHarvester() : base()
		{
			Name = "Perfect Mining Harvester";
			type = 1;
			quality = Utility.RandomMinMax(9, 10);
			NestSpawnType = "StrongSentry";
			Movable = true;
			
			if (quality != 10) Name = "Nearly Perfect Mining Harvester";
		}

		public PerfectMiningHarvester( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			if (version == 0)
			{
				if (quality != 10) Name = "Nearly Perfect Mining Harvester";
			}
		}
	}
}