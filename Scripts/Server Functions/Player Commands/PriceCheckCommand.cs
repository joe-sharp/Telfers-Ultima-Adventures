using System;
using System.Collections.Generic;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;
using System.Diagnostics;
using System.Linq;

namespace Server.Commands
{
    public class PVPriceCheck
    {
        public static void Initialize()
        {
            CommandSystem.Register("pvpc", AccessLevel.Player, new CommandEventHandler(PVPC_OnCommand));
        }

        [Usage("pvpc")]
        [Description("Runs a Price Estimate on the targetted item")]

        public static void PVPC_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Select an item to view price estimate");
            e.Mobile.Target = new InternalTarget(e.Mobile);
        }

        private class InternalTarget : Target
        {
            private Mobile m_From;

            private static bool LowPriceBoost = false; // may make it easier to game the system on low sell price (1 gp) items
            private static bool HarderBagSale = true; // if enabled, makes it harder to get a decent price on items sold en masse in bags
            private static int BarterValue = 33;
            private static int RichSuckerChance = 950;
            private static int ImprovedPriceModChance = 90;
            private static int MaxImprovedPriceMod = 4;
            private static int MinImprovedPriceMod = 2;
            private static int MinAttrsMultiplier = 50; // %
            private static int MaxAttrsMultiplier = 200; // %
            private static int RichSuckerMinPrice = 100;
            private static int RichSuckerMinPriceMultiplier = 3;
            private static int RichSuckerMaxPriceMultiplier = 10;
            private static int MinPriceModifier = 2;
            private static int MaxPriceModifier = 10;
            private static int MinimalPriceMaxBoost = 4;
            private static int SBListMaxRandom = 10;
            private static int SBListMaxFixed = 20;
            private static int PriceThresholdForAttributeCheck = 50; // set to a low value (100-200) to only do this to cheap items
            private static bool IncreasePriceBasedOnNumberOfProps = true; // if true, items with many beneficial props will sell for more money
            private static int AttrsMod1Or2Props = 1; // price multiplier if the item has 1-2 beneficial props
            private static int AttrsMod3Or4Props = 2;
            private static int AttrsMod5Or6Props = 5;
            private static int AttrsMod7Or8Props = 10;
            private static int AttrsMod9OrMoreProps = 20; // price multiplier if the item has 9+ beneficial props
            private static int AttrsIntensityThreshold = 10; // threshold for attribute intensity % to count toward the number of beneficial props (0 = any intensity, otherwise needs to be greater than the percentage specified)
            private static int IntensityPercentile = 20; // for each N% intensity, give a payout bonus equal to intensity multiplied by the multiplier below
            private static int IntensityMultiplier = 2; // for each N% intensity, give an additional intensity multiplier
            private static int PriceCutOnMaxDurability25 = 90; // %
            private static int PriceCutOnMaxDurability20 = 75; // %
            private static int PriceCutOnMaxDurability15 = 50; // %
            private static int PriceCutOnMaxDurability10 = 25; // %
            private static int PriceCutOnMaxDurability5 = 5; // %
            private static int PriceCutOnMaxDurability3 = 1; // %
            private static int FinalPriceModifier = 200; // % - the final price after all bonuses will be modified to this percentage (e.g. the final price of 1000 will be set to 800 if the 80% modifier is applied); use this to fine tune the prices without affecting the overall balance above
            private void SetupSBList(ref List<SBInfo> sbList)
            {
                sbList.Clear();
                sbList.Add(new SBBuyArtifacts());
                for (int i = 0; i < Utility.RandomMinMax(1, SBListMaxRandom) + SBListMaxFixed; i++)
                {
                    int sbListID = Utility.Random(127);
                    switch (sbListID)
                    {
                        case 0: { sbList.Add(new SBElfRares()); break; }
                        case 1: { sbList.Add(new SBChainmailArmor()); break; }
                        case 2: { sbList.Add(new SBHelmetArmor()); break; }
                        case 3: { sbList.Add(new SBLeatherArmor()); break; }
                        case 4: { sbList.Add(new SBMetalShields()); break; }
                        case 5: { sbList.Add(new SBPlateArmor()); break; }
                        case 6: { sbList.Add(new SBLotsOfArrows()); break; }
                        case 7: { sbList.Add(new SBRingmailArmor()); break; }
                        case 8: { sbList.Add(new SBStuddedArmor()); break; }
                        case 9: { sbList.Add(new SBWoodenShields()); break; }
                        case 10: { sbList.Add(new SBSEArmor()); break; }
                        case 11: { sbList.Add(new SBSEBowyer()); break; }
                        case 12: { sbList.Add(new SBSECarpenter()); break; }
                        case 13: { sbList.Add(new SBSEFood()); break; }
                        case 14: { sbList.Add(new SBSELeatherArmor()); break; }
                        case 15: { sbList.Add(new SBSEWeapons()); break; }
                        case 16: { sbList.Add(new SBAxeWeapon()); break; }
                        case 17: { sbList.Add(new SBKnifeWeapon()); break; }
                        case 18: { sbList.Add(new SBMaceWeapon()); break; }
                        case 19: { sbList.Add(new SBPoleArmWeapon()); break; }
                        case 20: { sbList.Add(new SBRangedWeapon()); break; }
                        case 21: { sbList.Add(new SBSpearForkWeapon()); break; }
                        case 22: { sbList.Add(new SBStavesWeapon()); break; }
                        case 23: { sbList.Add(new SBSwordWeapon()); break; }
                        case 24: { sbList.Add(new SBElfWizard()); break; }
                        case 25: { sbList.Add(new SBElfHealer()); break; }
                        case 26: { sbList.Add(new SBUndertaker()); break; }
                        case 27: { sbList.Add(new SBAlchemist()); break; }
                        case 28: { sbList.Add(new SBMixologist()); break; }
                        case 29: { sbList.Add(new SBAnimalTrainer()); break; }
                        case 30: { sbList.Add(new SBHumanAnimalTrainer()); break; }
                        case 31: { sbList.Add(new SBGargoyleAnimalTrainer()); break; }
                        case 32: { sbList.Add(new SBElfAnimalTrainer()); break; }
                        case 33: { sbList.Add(new SBBarbarianAnimalTrainer()); break; }
                        case 34: { sbList.Add(new SBOrkAnimalTrainer()); break; }
                        case 35: { sbList.Add(new SBArchitect()); break; }
                        case 36: { sbList.Add(new SBSailor()); break; }
                        case 37: { sbList.Add(new SBKungFu()); break; }
                        case 38: { sbList.Add(new SBBaker()); break; }
                        case 39: { sbList.Add(new SBBanker()); break; }
                        case 40: { sbList.Add(new SBBard()); break; }
                        case 41: { sbList.Add(new SBBarkeeper()); break; }
                        case 42: { sbList.Add(new SBBeekeeper()); break; }
                        case 43: { sbList.Add(new SBBlacksmith()); break; }
                        case 44: { sbList.Add(new SBBowyer()); break; }
                        case 45: { sbList.Add(new SBButcher()); break; }
                        case 46: { sbList.Add(new SBCarpenter()); break; }
                        case 47: { sbList.Add(new SBCobbler()); break; }
                        case 48: { sbList.Add(new SBCook()); break; }
                        case 49: { sbList.Add(new SBFarmer()); break; }
                        case 50: { sbList.Add(new SBFisherman()); break; }
                        case 51: { sbList.Add(new SBFortuneTeller()); break; }
                        case 52: { sbList.Add(new SBFurtrader()); break; }
                        case 53: { sbList.Add(new SBGlassblower()); break; }
                        case 54: { sbList.Add(new SBHairStylist()); break; }
                        case 55: { sbList.Add(new SBHealer()); break; }
                        case 56: { sbList.Add(new SBDruid()); break; }
                        case 57: { sbList.Add(new SBDruidTree()); break; }
                        case 58: { sbList.Add(new SBHerbalist()); break; }
                        case 59: { sbList.Add(new SBHolyMage()); break; }
                        case 60: { sbList.Add(new SBRuneCasting()); break; }
                        case 61: { sbList.Add(new SBEnchanter()); break; }
                        case 62: { sbList.Add(new SBHouseDeed()); break; }
                        case 63: { sbList.Add(new SBInnKeeper()); break; }
                        case 64: { sbList.Add(new SBJewel()); break; }
                        case 65: { sbList.Add(new SBKeeperOfChivalry()); break; }
                        case 66: { sbList.Add(new SBLeatherWorker()); break; }
                        case 67: { sbList.Add(new SBMapmaker()); break; }
                        case 68: { sbList.Add(new SBMiller()); break; }
                        case 69: { sbList.Add(new SBMiner()); break; }
                        case 70: { sbList.Add(new SBMonk()); break; }
                        case 71: { sbList.Add(new SBPlayerBarkeeper()); break; }
                        case 72: { sbList.Add(new SBProvisioner()); break; }
                        case 73: { sbList.Add(new SBRancher()); break; }
                        case 74: { sbList.Add(new SBRanger()); break; }
                        case 75: { sbList.Add(new SBRealEstateBroker()); break; }
                        case 76: { sbList.Add(new SBScribe()); break; }
                        case 77: { sbList.Add(new SBSage()); break; }
                        case 78: { sbList.Add(new SBSECook()); break; }
                        case 79: { sbList.Add(new SBSEHats()); break; }
                        case 80: { sbList.Add(new SBShipwright()); break; }
                        case 81: { sbList.Add(new SBDevon()); break; }
                        case 82: { sbList.Add(new SBSmithTools()); break; }
                        case 83: { sbList.Add(new SBStoneCrafter()); break; }
                        case 84: { sbList.Add(new SBTailor()); break; }
                        case 85: { sbList.Add(new SBJester()); break; }
                        case 86: { sbList.Add(new SBTanner()); break; }
                        case 87: { sbList.Add(new SBTavernKeeper()); break; }
                        case 88: { sbList.Add(new SBThief()); break; }
                        case 89: { sbList.Add(new SBTinker()); break; }
                        case 90: { sbList.Add(new SBVagabond()); break; }
                        case 91: { sbList.Add(new SBVarietyDealer()); break; }
                        case 92: { sbList.Add(new SBVeterinarian()); break; }
                        case 93: { sbList.Add(new SBWaiter()); break; }
                        case 94: { sbList.Add(new SBWeaponSmith()); break; }
                        case 95: { sbList.Add(new SBWeaver()); break; }
                        case 96: { sbList.Add(new SBNecroMage()); break; }
                        case 97: { sbList.Add(new SBNecromancer()); break; }
                        case 98: { sbList.Add(new SBWitches()); break; }
                        case 99: { sbList.Add(new SBMortician()); break; }
                        case 100: { sbList.Add(new SBMage()); break; }
                        case 101: { sbList.Add(new SBGodlySewing()); break; }
                        case 102: { sbList.Add(new SBGodlySmithing()); break; }
                        case 103: { sbList.Add(new SBGodlyBrewing()); break; }
                        case 104: { sbList.Add(new SBMazeStore()); break; }
                        case 105: { sbList.Add(new SBBuyArtifacts()); break; }
                        case 106: { sbList.Add(new SBGemArmor()); break; }
                        case 107: { sbList.Add(new SBRoscoe()); break; }
                        case 108: { sbList.Add(new SBTinkerGuild()); break; }
                        case 109: { sbList.Add(new SBThiefGuild()); break; }
                        case 110: { sbList.Add(new SBTailorGuild()); break; }
                        case 111: { sbList.Add(new SBMinerGuild()); break; }
                        case 112: { sbList.Add(new SBMageGuild()); break; }
                        case 113: { sbList.Add(new SBHealerGuild()); break; }
                        case 114: { sbList.Add(new SBSailorGuild()); break; }
                        case 115: { sbList.Add(new SBBlacksmithGuild()); break; }
                        case 116: { sbList.Add(new SBBardGuild()); break; }
                        case 117: { sbList.Add(new SBHolidayXmas()); break; }
                        case 118: { sbList.Add(new SBHolidayHalloween()); break; }
                        case 119: { sbList.Add(new SBNecroGuild()); break; }
                        case 120: { sbList.Add(new SBArcherGuild()); break; }
                        case 121: { sbList.Add(new SBAlchemistGuild()); break; }
                        case 122: { sbList.Add(new SBLibraryGuild()); break; }
                        case 123: { sbList.Add(new SBDruidGuild()); break; }
                        case 124: { sbList.Add(new SBCarpenterGuild()); break; }
                        case 125: { sbList.Add(new SBAssassin()); break; }
                        case 126: { sbList.Add(new SBCartographer()); break; }
                        case 127: { sbList.Add(new SBBuyArtifacts()); break; }
                        default: break;
                    }
                }
            }

            // The lists below must correspond to the enum definitions in AOS.cs. The number of elements
            // must strictly correspond to the number of elements in the AOS enums, or the game will crash.
            private int[] AosAttributeIntensities = {
            10, // RegenHits
			10, // RegenStam
			10, // RegenMana
			25, // DefendChance
			25, // AttackChance
			25, // BonusStr
			25, // BonusDex
			25, // BonusInt
			25, // BonusHits
			25, // BonusStam
			25, // BonusMana
			50, // WeaponDamage
			50, // WeaponSpeed
			50, // SpellDamage
			3, // CastRecovery
			3, // CastSpeed
			25, // LowerManaCost
			25, // LowerRegCost
			50, // ReflectPhysical
			50, // EnhancePotions,
			150, // Luck
			1, // SpellChanneling
			1 // NightSight
		    };

            private int[] AosWeaponAttributeIntensities = {
            50, // LowerStatReq
			5, // SelfRepair
			50, // HitLeechHits
			50, // HitLeechStam
			50, // HitLeechMana
			50, // HitLowerAttack
			50, // HitLowerDefend
			50, // HitMagicArrow
			50, // HitHarm
			50, // HitFireball
			50, // HitLightning
			50, // HitDispel
			50, // HitColdArea
			50, // HitFireArea
			50, // HitPoisonArea
			50, // HitEnergyArea
			50, // HitPhysicalArea
			15, // ResistPhysicalBonus
			15, // ResistFireBonus
			15, // ResistColdBonus
			15, // ResistPoisonBonus
			15, // ResistEnergyBonus
			1, // UseBestSkill
			1, // MageWeapon
			100 // DurabilityBonus
		    };

            private int[] AosArmorAttributeIntensities = {
            50, // LowerStatReq
			5, // SelfRepair
			1, // MageArmor
			100 // DurabilityBonus
		    };

            private int[] AosElementAttributeIntensities = {
            1100, // Physical - avoid overvaluing it since most stuff is at 100% physical
			200, // Fire
			200, // Cold
			200, // Poison
			200, // Energy
			25, // Chaos
			25, // Direct
		    };

            private int MaxSkillIntensity = 15; // FIXME: 12?
            private int MaxResistanceIntensity = 25;
            private int ResistanceIntensityCountsAsProp = 80; // %

            private enum IntensityMode
            {
                AosAttribute,
                AosWeaponAttribute,
                AosArmorAttribute,
                AosElementAttribute,
                SkillBonus,
                ResistanceBonus,
                RunicToolProperties
            }

            private void AddSkillBonuses(Item ii, double skill1, double skill2, double skill3, double skill4, double skill5, ref int attrsMod, ref int props)
            {
                int MaxRealIntensity = ii is BaseMagicStaff ? 50 : MaxSkillIntensity;

                int NormalizedSkillBonus1 = (int)skill1 * 100 / MaxRealIntensity;
                int NormalizedSkillBonus2 = (int)skill2 * 100 / MaxRealIntensity;
                int NormalizedSkillBonus3 = (int)skill3 * 100 / MaxRealIntensity;
                int NormalizedSkillBonus4 = (int)skill4 * 100 / MaxRealIntensity;
                int NormalizedSkillBonus5 = (int)skill5 * 100 / MaxRealIntensity;

                if (NormalizedSkillBonus1 > 0 && NormalizedSkillBonus1 >= AttrsIntensityThreshold) ++props;
                if (NormalizedSkillBonus2 > 0 && NormalizedSkillBonus2 >= AttrsIntensityThreshold) ++props;
                if (NormalizedSkillBonus3 > 0 && NormalizedSkillBonus3 >= AttrsIntensityThreshold) ++props;
                if (NormalizedSkillBonus4 > 0 && NormalizedSkillBonus4 >= AttrsIntensityThreshold) ++props;
                if (NormalizedSkillBonus5 > 0 && NormalizedSkillBonus5 >= AttrsIntensityThreshold) ++props;

                attrsMod += (int)skill1 * (NormalizedSkillBonus1 / 2);
                attrsMod += (int)skill2 * (NormalizedSkillBonus2 / 2);
                attrsMod += (int)skill3 * (NormalizedSkillBonus3 / 2);
                attrsMod += (int)skill4 * (NormalizedSkillBonus4 / 2);
                attrsMod += (int)skill5 * (NormalizedSkillBonus5 / 2);
            }

            private void AddResistanceBonuses(int physical, int fire, int cold, int poison, int energy, ref int attrsMod, ref int props)
            {
                int NormalizedPhysicalResistance = physical * 100 / MaxResistanceIntensity;
                int NormalizedFireResistance = fire * 100 / MaxResistanceIntensity;
                int NormalizedColdResistance = cold * 100 / MaxResistanceIntensity;
                int NormalizedPoisonResistance = poison * 100 / MaxResistanceIntensity;
                int NormalizedEnergyResistance = energy * 100 / MaxResistanceIntensity;

                if (NormalizedPhysicalResistance >= ResistanceIntensityCountsAsProp) ++props;
                if (NormalizedFireResistance >= ResistanceIntensityCountsAsProp) ++props;
                if (NormalizedColdResistance >= ResistanceIntensityCountsAsProp) ++props;
                if (NormalizedPoisonResistance >= ResistanceIntensityCountsAsProp) ++props;
                if (NormalizedEnergyResistance >= ResistanceIntensityCountsAsProp) ++props;

                attrsMod += physical * (NormalizedPhysicalResistance / 10);
                attrsMod += fire * (NormalizedFireResistance / 10);
                attrsMod += cold * (NormalizedColdResistance / 10);
                attrsMod += poison * (NormalizedPoisonResistance / 10);
                attrsMod += energy * (NormalizedEnergyResistance / 10);
            }

            private void ScalePriceOnDurability(Item item, ref int price)
            {
                int cur_dur = 0;
                int max_dur = 0;

                if (item is BaseWeapon)
                {
                    cur_dur = ((BaseWeapon)item).HitPoints;
                    max_dur = ((BaseWeapon)item).MaxHitPoints;
                }
                else if (item is BaseArmor)
                {
                    cur_dur = ((BaseArmor)item).HitPoints;
                    max_dur = ((BaseArmor)item).MaxHitPoints;
                }
                else if (item is BaseClothing)
                {
                    cur_dur = ((BaseClothing)item).HitPoints;
                    max_dur = ((BaseClothing)item).MaxHitPoints;
                }
                else if (item is BaseShield)
                {
                    cur_dur = ((BaseShield)item).HitPoints;
                    max_dur = ((BaseShield)item).MaxHitPoints;
                }
                else if (item is BaseJewel)
                {
                    cur_dur = ((BaseJewel)item).HitPoints;
                    max_dur = ((BaseJewel)item).MaxHitPoints;
                }

                if (cur_dur > 0 && max_dur > 0)
                {
                    if (max_dur <= 3)
                        price = price * PriceCutOnMaxDurability3 / 100;
                    if (max_dur <= 5)
                        price = price * PriceCutOnMaxDurability5 / 100;
                    if (max_dur <= 10)
                        price = price * PriceCutOnMaxDurability10 / 100;
                    if (max_dur <= 15)
                        price = price * PriceCutOnMaxDurability15 / 100;
                    if (max_dur <= 20)
                        price = price * PriceCutOnMaxDurability20 / 100;
                    else if (max_dur <= 25)
                        price = price * PriceCutOnMaxDurability25 / 100;
                }
            }

            // BaseWeapon
            private void AddNormalizedBonuses(BaseWeapon bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                int id = 0;

                if (mode == IntensityMode.AosAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                    {
                        int MaxIntensity = AosAttributeIntensities[id++];
                        int NormalizedAttribute = bw.Attributes[(AosAttribute)i] * 100 / MaxIntensity;
                        if (NormalizedAttribute > 0 && NormalizedAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxIntensity > 1)
                            attrsMod += (int)(NormalizedAttribute * ((double)NormalizedAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.AosWeaponAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosWeaponAttribute)))
                    {
                        int MaxWeaponIntensity = AosWeaponAttributeIntensities[id++];
                        int NormalizedWeaponAttribute = bw.WeaponAttributes[(AosWeaponAttribute)i] * 100 / MaxWeaponIntensity;
                        if (NormalizedWeaponAttribute > 0 && NormalizedWeaponAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxWeaponIntensity > 1)
                            attrsMod += (int)(NormalizedWeaponAttribute * ((double)NormalizedWeaponAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedWeaponAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.AosElementAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosElementAttribute)))
                    {
                        int MaxElemIntensity = AosElementAttributeIntensities[id++];
                        int NormalizedElementalAttribute = bw.AosElementDamages[(AosElementAttribute)i] * 100 / MaxElemIntensity;
                        if (NormalizedElementalAttribute > 0 && NormalizedElementalAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxElemIntensity > 1)
                            attrsMod += (int)(NormalizedElementalAttribute * ((double)NormalizedElementalAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedElementalAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.SkillBonus)
                {
                    AddSkillBonuses(bw, bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
                        bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
                }
                else
                {
                    Console.WriteLine("Unexpected mode for weapon: " + mode);
                }
            }

            // BaseArmor
            private void AddNormalizedBonuses(BaseArmor bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                int id = 0;

                if (mode == IntensityMode.AosAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                    {
                        int MaxIntensity = AosAttributeIntensities[id++];
                        int NormalizedAttribute = bw.Attributes[(AosAttribute)i] * 100 / MaxIntensity;
                        if (NormalizedAttribute > 0 && NormalizedAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxIntensity > 1)
                            attrsMod += (int)(NormalizedAttribute * ((double)NormalizedAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.AosArmorAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosArmorAttribute)))
                    {
                        int MaxArmorIntensity = AosArmorAttributeIntensities[id++];
                        int NormalizedArmorAttribute = bw.ArmorAttributes[(AosArmorAttribute)i] * 100 / MaxArmorIntensity;
                        if (NormalizedArmorAttribute > 0 && NormalizedArmorAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxArmorIntensity > 1)
                            attrsMod += (int)(NormalizedArmorAttribute * ((double)NormalizedArmorAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedArmorAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.SkillBonus)
                {
                    AddSkillBonuses(bw, bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
                        bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
                }
                else if (mode == IntensityMode.ResistanceBonus)
                {
                    AddResistanceBonuses(bw.PhysicalBonus, bw.FireBonus, bw.ColdBonus, bw.PoisonBonus, bw.EnergyBonus,
                        ref attrsMod, ref props);
                }
                else
                {
                    Console.WriteLine("Unexpected mode for armor: " + mode);
                }
            }

            // BaseShield
            private void AddNormalizedBonuses(BaseShield bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                int id = 0;

                if (mode == IntensityMode.AosAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                    {
                        int MaxIntensity = AosAttributeIntensities[id++];
                        int NormalizedAttribute = bw.Attributes[(AosAttribute)i] * 100 / MaxIntensity;
                        if (NormalizedAttribute > 0 && NormalizedAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxIntensity > 1)
                            attrsMod += (int)(NormalizedAttribute * ((double)NormalizedAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.AosArmorAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosArmorAttribute)))
                    {
                        int MaxArmorIntensity = AosArmorAttributeIntensities[id++];
                        int NormalizedArmorAttribute = bw.ArmorAttributes[(AosArmorAttribute)i] * 100 / MaxArmorIntensity;
                        if (NormalizedArmorAttribute > 0 && NormalizedArmorAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxArmorIntensity > 1)
                            attrsMod += (int)(NormalizedArmorAttribute * ((double)NormalizedArmorAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedArmorAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.SkillBonus)
                {
                    AddSkillBonuses(bw, bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
                        bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
                }
                else if (mode == IntensityMode.ResistanceBonus)
                {
                    AddResistanceBonuses(bw.PhysicalBonus, bw.FireBonus, bw.ColdBonus, bw.PoisonBonus, bw.EnergyBonus,
                        ref attrsMod, ref props);
                }
                else
                {
                    Console.WriteLine("Unexpected mode for shield: " + mode);
                }
            }

            // BaseClothing
            private void AddNormalizedBonuses(BaseClothing bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                int id = 0;

                if (mode == IntensityMode.AosAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                    {
                        int MaxIntensity = AosAttributeIntensities[id++];
                        int NormalizedAttribute = bw.Attributes[(AosAttribute)i] * 100 / MaxIntensity;
                        if (NormalizedAttribute > 0 && NormalizedAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxIntensity > 1)
                            attrsMod += (int)(NormalizedAttribute * ((double)NormalizedAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.AosArmorAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosArmorAttribute)))
                    {
                        int MaxArmorIntensity = AosArmorAttributeIntensities[id++];
                        int NormalizedArmorAttribute = bw.ClothingAttributes[(AosArmorAttribute)i] * 100 / MaxArmorIntensity;
                        if (NormalizedArmorAttribute > 0 && NormalizedArmorAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxArmorIntensity > 1)
                            attrsMod += (int)(NormalizedArmorAttribute * ((double)NormalizedArmorAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedArmorAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.SkillBonus)
                {
                    AddSkillBonuses(bw, bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
                        bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
                }
                else
                {
                    Console.WriteLine("Unexpected mode for clothing: " + mode);
                }
            }

            // BaseJewel
            private void AddNormalizedBonuses(BaseJewel bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                int id = 0;

                if (mode == IntensityMode.AosAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                    {
                        int MaxIntensity = AosAttributeIntensities[id++];
                        int NormalizedAttribute = bw.Attributes[(AosAttribute)i] * 100 / MaxIntensity;
                        if (NormalizedAttribute > 0 && NormalizedAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxIntensity > 1)
                            attrsMod += (int)(NormalizedAttribute * ((double)NormalizedAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.AosElementAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosElementAttribute)))
                    {
                        int MaxElemIntensity = AosElementAttributeIntensities[id++];
                        int NormalizedElementalAttribute = bw.Resistances[(AosElementAttribute)i] * 100 / MaxElemIntensity;
                        if (NormalizedElementalAttribute > 0 && NormalizedElementalAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxElemIntensity > 1)
                            attrsMod += (int)(NormalizedElementalAttribute * ((double)NormalizedElementalAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedElementalAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.SkillBonus)
                {
                    AddSkillBonuses(bw, bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
                        bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
                }
                else if (mode == IntensityMode.ResistanceBonus)
                {
                    AddResistanceBonuses(bw.PhysicalResistance, bw.FireResistance, bw.ColdResistance, bw.PoisonResistance, bw.EnergyResistance,
                        ref attrsMod, ref props);
                }
                else
                {
                    Console.WriteLine("Unexpected mode for jewel: " + mode);
                }
            }

            // BaseQuiver
            private void AddNormalizedBonuses(BaseQuiver bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                int id = 0;

                if (mode == IntensityMode.AosAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                    {
                        int MaxIntensity = AosAttributeIntensities[id++];
                        int NormalizedAttribute = bw.Attributes[(AosAttribute)i] * 100 / MaxIntensity;
                        if (NormalizedAttribute > 0 && NormalizedAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxIntensity > 1)
                            attrsMod += (int)(NormalizedAttribute * ((double)NormalizedAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else
                {
                    Console.WriteLine("Unexpected mode for quiver: " + mode);
                }
            }

            // BaseInstrument
            private void AddNormalizedBonuses(BaseInstrument bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                int id = 0;

                if (mode == IntensityMode.AosAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                    {
                        int MaxIntensity = AosAttributeIntensities[id++];
                        int NormalizedAttribute = bw.Attributes[(AosAttribute)i] * 100 / MaxIntensity;
                        if (NormalizedAttribute > 0 && NormalizedAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxIntensity > 1)
                            attrsMod += (int)(NormalizedAttribute * ((double)NormalizedAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.SkillBonus)
                {
                    AddSkillBonuses(bw, bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
                        bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
                }
                else if (mode == IntensityMode.ResistanceBonus)
                {
                    AddResistanceBonuses(bw.PhysicalResistance, bw.FireResistance, bw.ColdResistance, bw.PoisonResistance, bw.EnergyResistance,
                        ref attrsMod, ref props);
                }
                else
                {
                    Console.WriteLine("Unexpected mode for instrument: " + mode);
                }
            }

            // Spellbook
            private void AddNormalizedBonuses(Spellbook bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                int id = 0;

                if (mode == IntensityMode.AosAttribute)
                {
                    foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                    {
                        int MaxIntensity = AosAttributeIntensities[id++];
                        int NormalizedAttribute = bw.Attributes[(AosAttribute)i] * 100 / MaxIntensity;
                        if (NormalizedAttribute > 0 && NormalizedAttribute >= AttrsIntensityThreshold) ++props;

                        if (MaxIntensity > 1)
                            attrsMod += (int)(NormalizedAttribute * ((double)NormalizedAttribute / IntensityPercentile * IntensityMultiplier));
                        else if (NormalizedAttribute > 0)
                            attrsMod += Utility.RandomMinMax(50, 100);
                    }
                }
                else if (mode == IntensityMode.SkillBonus)
                {
                    AddSkillBonuses(bw, bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
                        bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
                }
                else if (mode == IntensityMode.ResistanceBonus)
                {
                    AddResistanceBonuses(bw.PhysicalResistance, bw.FireResistance, bw.ColdResistance, bw.PoisonResistance, bw.EnergyResistance,
                        ref attrsMod, ref props);
                }
                else
                {
                    Console.WriteLine("Unexpected mode for spellbook: " + mode);
                }
            }

            // BaseRunicTool
            private void AddNormalizedBonuses(BaseRunicTool bw, IntensityMode mode, ref int attrsMod, ref int props)
            {
                if (mode == IntensityMode.RunicToolProperties)
                {
                    attrsMod += 1000;

                    switch (bw.Resource)
                    {
                        case CraftResource.DullCopper: attrsMod = (int)(attrsMod * 1.25); break;
                        case CraftResource.ShadowIron: attrsMod = (int)(attrsMod * 1.5); break;
                        case CraftResource.Copper: attrsMod = (int)(attrsMod * 1.75); break;
                        case CraftResource.Bronze: attrsMod = (int)(attrsMod * 2); break;
                        case CraftResource.Gold: attrsMod = (int)(attrsMod * 2.25); break;
                        case CraftResource.Agapite: attrsMod = (int)(attrsMod * 2.50); break;
                        case CraftResource.Verite: attrsMod = (int)(attrsMod * 2.75); break;
                        case CraftResource.Valorite: attrsMod = (int)(attrsMod * 3); break;
                        case CraftResource.Nepturite: attrsMod = (int)(attrsMod * 3.10); break;
                        case CraftResource.Obsidian: attrsMod = (int)(attrsMod * 3.10); break;
                        case CraftResource.Steel: attrsMod = (int)(attrsMod * 3.25); break;
                        case CraftResource.Brass: attrsMod = (int)(attrsMod * 3.5); break;
                        case CraftResource.Mithril: attrsMod = (int)(attrsMod * 3.75); break;
                        case CraftResource.Xormite: attrsMod = (int)(attrsMod * 3.75); break;
                        case CraftResource.Dwarven: attrsMod = (int)(attrsMod * 7.50); break;
                        case CraftResource.SpinedLeather: attrsMod = (int)(attrsMod * 1.5); break;
                        case CraftResource.HornedLeather: attrsMod = (int)(attrsMod * 1.75); break;
                        case CraftResource.BarbedLeather: attrsMod = (int)(attrsMod * 2.0); break;
                        case CraftResource.NecroticLeather: attrsMod = (int)(attrsMod * 2.25); break;
                        case CraftResource.VolcanicLeather: attrsMod = (int)(attrsMod * 2.5); break;
                        case CraftResource.FrozenLeather: attrsMod = (int)(attrsMod * 2.75); break;
                        case CraftResource.GoliathLeather: attrsMod = (int)(attrsMod * 3.0); break;
                        case CraftResource.DraconicLeather: attrsMod = (int)(attrsMod * 3.25); break;
                        case CraftResource.HellishLeather: attrsMod = (int)(attrsMod * 3.5); break;
                        case CraftResource.DinosaurLeather: attrsMod = (int)(attrsMod * 3.75); break;
                        case CraftResource.AlienLeather: attrsMod = (int)(attrsMod * 3.75); break;
                        case CraftResource.RedScales: attrsMod = (int)(attrsMod * 1.25); break;
                        case CraftResource.YellowScales: attrsMod = (int)(attrsMod * 1.25); break;
                        case CraftResource.BlackScales: attrsMod = (int)(attrsMod * 1.5); break;
                        case CraftResource.GreenScales: attrsMod = (int)(attrsMod * 1.5); break;
                        case CraftResource.WhiteScales: attrsMod = (int)(attrsMod * 1.5); break;
                        case CraftResource.BlueScales: attrsMod = (int)(attrsMod * 1.5); break;
                        case CraftResource.AshTree: attrsMod = (int)(attrsMod * 1.25); break;
                        case CraftResource.CherryTree: attrsMod = (int)(attrsMod * 1.45); break;
                        case CraftResource.EbonyTree: attrsMod = (int)(attrsMod * 1.65); break;
                        case CraftResource.GoldenOakTree: attrsMod = (int)(attrsMod * 1.85); break;
                        case CraftResource.HickoryTree: attrsMod = (int)(attrsMod * 2.05); break;
                        case CraftResource.MahoganyTree: attrsMod = (int)(attrsMod * 2.25); break;
                        case CraftResource.DriftwoodTree: attrsMod = (int)(attrsMod * 2.25); break;
                        case CraftResource.OakTree: attrsMod = (int)(attrsMod * 2.45); break;
                        case CraftResource.PineTree: attrsMod = (int)(attrsMod * 2.65); break;
                        case CraftResource.GhostTree: attrsMod = (int)(attrsMod * 2.65); break;
                        case CraftResource.RosewoodTree: attrsMod = (int)(attrsMod * 2.85); break;
                        case CraftResource.WalnutTree: attrsMod = (int)(attrsMod * 3); break;
                        case CraftResource.ElvenTree: attrsMod = (int)(attrsMod * 6); break;
                        case CraftResource.PetrifiedTree: attrsMod = (int)(attrsMod * 3.25); break;
                    }

                    attrsMod -= (50 - bw.UsesRemaining) * 30;
                    if (attrsMod < 0)
                        attrsMod = 0;
                }
                else
                {
                    Console.WriteLine("Unexpected mode for runic tool: " + mode);
                }
            }

            private int GetAttrsMod(Item ii)
            {
                if (ii == null) { return 0; }

                int attrsMod = 0;
                int props = 0;

                if (ii is BaseWeapon)
                {
                    BaseWeapon bw = ii as BaseWeapon;

                    AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.AosWeaponAttribute, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.AosElementAttribute, ref attrsMod, ref props);

                    AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);

                    if (bw.Slayer != SlayerName.None) ++props;
                    if (bw.Slayer2 != SlayerName.None) ++props;

                    if (bw.Slayer != SlayerName.None)
                    {
                        attrsMod += 100;
                        props++;
                    }
                    if (bw.Slayer2 != SlayerName.None)
                    {
                        attrsMod += 100;
                        props++;
                    }

                    if (props >= 3 && (bw.WeaponAttributes.MageWeapon > 0 || bw.Attributes.SpellChanneling > 0))
                        attrsMod = (int)((double)attrsMod * 1.3);
                }
                else if (ii is BaseArmor)
                {
                    BaseArmor bw = ii as BaseArmor;

                    AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.AosArmorAttribute, ref attrsMod, ref props);

                    AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

                    if (props >= 3 && bw.ArmorAttributes.MageArmor > 0 || bw.Attributes.SpellChanneling > 0)
                        attrsMod = (int)((double)attrsMod * 1.3);

                }
                else if (ii is BaseClothing)
                {
                    BaseClothing bw = ii as BaseClothing;

                    AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.AosArmorAttribute, ref attrsMod, ref props);

                    AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);

                    if (props >= 3 && bw.ClothingAttributes.MageArmor > 0 || bw.Attributes.SpellChanneling > 0)
                        attrsMod = (int)((double)attrsMod * 1.3);
                }
                else if (ii is BaseJewel)
                {
                    BaseJewel bw = ii as BaseJewel;

                    AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.AosElementAttribute, ref attrsMod, ref props);

                    AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);
                }
                else if (ii is BaseShield)
                {
                    BaseShield bw = ii as BaseShield;

                    AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.AosArmorAttribute, ref attrsMod, ref props);

                    AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

                    if (props >= 3 && bw.ArmorAttributes.MageArmor > 0 || bw.Attributes.SpellChanneling > 0)
                        attrsMod = (int)((double)attrsMod * 1.3);
                }
                else if (ii is BaseQuiver)
                {
                    BaseQuiver bw = ii as BaseQuiver;

                    AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
                }
                else if (ii is BaseInstrument)
                {
                    BaseInstrument bw = ii as BaseInstrument;

                    AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);

                    AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);
                }
                else if (ii is BaseRunicTool)
                {
                    BaseRunicTool bw = ii as BaseRunicTool;

                    AddNormalizedBonuses(bw, IntensityMode.RunicToolProperties, ref attrsMod, ref props);
                }
                else if (ii is Spellbook)
                {
                    Spellbook bw = ii as Spellbook;

                    AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);

                    AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
                    AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

                    if (bw.Slayer != SlayerName.None) ++props;
                    if (bw.Slayer2 != SlayerName.None) ++props;

                    if (bw.SpellCount > 0)
                    {
                        attrsMod += bw.SpellCount * 20; // TODO: make the higher circle spells cost more
                    }
                    if (bw.Slayer != SlayerName.None)
                    {
                        attrsMod += 100;
                    }
                    if (bw.Slayer2 != SlayerName.None)
                    {
                        attrsMod += 100;
                    }
                }

                if (IncreasePriceBasedOnNumberOfProps)
                {
                    if (props == 1 || props == 2) { attrsMod *= AttrsMod1Or2Props; }
                    else if (props == 3 || props == 4) { attrsMod *= AttrsMod3Or4Props; }
                    else if (props == 5 || props == 6) { attrsMod *= AttrsMod5Or6Props; }
                    else if (props == 7 || props == 8) { attrsMod *= AttrsMod7Or8Props; }
                    else if (props >= 9) { attrsMod *= AttrsMod9OrMoreProps; }
                }

                return attrsMod;
            }


            private int PredictPrice(Item item)
            {
                int price = 0;
                List<SBInfo> sbList = new List<SBInfo>();
                List<Item> items = new List<Item>();

                SetupSBList(ref sbList);

                items.Add(item);
                if (item is Container)
                {
                    Container c = item as Container;
                    foreach (Item it in c.Items)
                    {
                        bool banned = it is BankCheck || it is Gold || it is DDCopper || it is DDSilver || it is DDJewels || it is DDXormite || it is DDGemstones || it is DDGoldNuggets;

                        if (!banned)
                            items.Add(it);
                    }
                }

                int barterValue = Utility.Random(BarterValue);
                int attrsMultiplier = Utility.RandomMinMax(MinAttrsMultiplier, MaxAttrsMultiplier);
                bool isRichSucker = Utility.RandomMinMax(0, 1000) > RichSuckerChance;
                float modifier = (float)Utility.RandomMinMax(MinPriceModifier, MaxPriceModifier);
                if (Utility.Random(100) > ImprovedPriceModChance && Utility.RandomBool())
                    modifier *= Utility.RandomMinMax(MinImprovedPriceMod, MaxImprovedPriceMod); // big luck, improved mod

                foreach (Item ii in items)
                {
                    if (HarderBagSale)
                    {
                        // reroll for each item in the bag, makes it progressively harder to get a decent price
                        // SetupSBList(ref sbList); // enable to make things even harder for bag sales
                        barterValue = Utility.Random(BarterValue);
                        attrsMultiplier = Utility.RandomMinMax(MinAttrsMultiplier, MaxAttrsMultiplier);
                        modifier = (float)Utility.RandomMinMax(MinPriceModifier, MaxPriceModifier);
                        if (Utility.Random(100) > ImprovedPriceModChance && Utility.RandomBool())
                            modifier *= Utility.RandomMinMax(MinImprovedPriceMod, MaxImprovedPriceMod);
                    }

                    int itemPrice = 0;
                    foreach (SBInfo priceInfo in sbList)
                    {
                        int estimate = priceInfo.SellInfo.GetSellPriceFor(ii, barterValue);
                        if (itemPrice < estimate)
                            itemPrice = estimate;
                    }

                    if (itemPrice < PriceThresholdForAttributeCheck)
                    {
                        int attrsMod = GetAttrsMod(ii);
                        attrsMod *= (int)((float)attrsMultiplier / 100);

                        ScalePriceOnDurability(ii, ref attrsMod);

                        itemPrice += attrsMod;
                    }
                    ScalePriceOnDurability(ii, ref price);

                    if (itemPrice == 1 && LowPriceBoost)
                    {
                        itemPrice = Utility.RandomMinMax( 1, MinimalPriceMaxBoost );
                    }
                    else if (itemPrice > RichSuckerMinPrice && isRichSucker)
                    {
                        itemPrice *= Utility.RandomMinMax( RichSuckerMinPriceMultiplier, RichSuckerMaxPriceMultiplier ); // rich sucker
                    }

                    itemPrice += (int)((float)itemPrice / modifier);
                    price += itemPrice;
                }

                return price;
            }
            private string IteratePC(Item item)
            {
                const int NUM_ITERATIONS = 20;
                List<Item> items = new List<Item>();

                items.Add(item);
                if (item is Container)
                {
                    Container c = item as Container;
                    foreach (Item it in c.Items)
                    {
                        bool banned = it is BankCheck || it is Gold || it is DDCopper || it is DDSilver || it is DDJewels || it is DDXormite || it is DDGemstones || it is DDGoldNuggets;

                        if (!banned)
                            items.Add(it);
                    }
                }
                if (items.Count > 0)
                {
                    foreach (Item item2 in items)
                    {
                        List<int> Prices = new List<int>();
                        int min = int.MaxValue;
                        int max = 1;
                        int avg = 0;

                        for (int i = 0; i < NUM_ITERATIONS; i++)
                        {
                            int ProjPrice = PredictPrice(item2) * item2.Amount;
                            if (ProjPrice > 1)
                                ProjPrice = ProjPrice * FinalPriceModifier / 100; // +++
                            if (ProjPrice > 1 && ProjPrice < min)
                                min = ProjPrice;
                            if (ProjPrice > max)
                                max = ProjPrice;
                            if (ProjPrice > 1)
                                Prices.Add(ProjPrice);
                        }

                        if (min == int.MaxValue) { min = 1; }
                        if (Prices.Count > 0)
                            avg = (int)Prices.Average();

                        return "Prices for Target: min = " + min + ", max = " + max + ", avg = " + avg;
                    }
                    return "";
                }
                return "";
            }
                public InternalTarget(Mobile from) : base(2, false, TargetFlags.None)
            {
                m_From = from;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Item)
                {
                    Item item = (Item)targeted;

                    if (item.Parent != from.Backpack)
                    {
                        from.SendMessage("The item must be in your backpack!");
                        return;
                    }
                    string PVPriceEstimate = this.IteratePC(item);
                    from.SendMessage(PVPriceEstimate);
                }
                else
                {
                    from.SendMessage("That is not a valid item!");
                }

            }
        }


    }
}