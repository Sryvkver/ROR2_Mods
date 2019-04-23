using BepInEx;
using RoR2;
using UnityEngine;
using RoR2.UI;
using BepInEx.Configuration;
using System.Collections;
using System.Linq;
using System.IO;
using EntityStates;
using System.Collections.Generic;

namespace Chest_Percantage_Modifier
{
    [BepInPlugin("dev.felixire.ChestPercantage", "ChestPercantage", "1.0.0")]
    class Main : BaseUnityPlugin
    {
        public Config config;
        public System.Random random;
        public bool hasSet = false;

        public void Awake()
        {
            config = new Config();
            config.Init(Config);

            var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            int seed = (int)((System.DateTime.UtcNow - epochStart).TotalSeconds / 2);
            Debug.Log(seed);
            random = new System.Random(seed);

            On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
        }

        private void ChestBehavior_ItemDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            string goName = self.gameObject.name.ToLower();
            if (!goName.Contains("chest"))
                orig.Invoke(self);


            PickupIndex pickup;

            if (goName.Contains("chest1"))
            {
                pickup = GetRandomItem(Chest_Percantage_Modifier.Config.ChestType.Normal);
                if(pickup != PickupIndex.none)
                    self.SetFieldValue("dropPickup", pickup);
            }
            else if (goName.Contains("chest2"))
            {
                pickup = GetRandomItem(Chest_Percantage_Modifier.Config.ChestType.Large);
                if (pickup != PickupIndex.none)
                    self.SetFieldValue("dropPickup", pickup);
            }
            else if (goName.Contains("goldchest"))
            {
                pickup = GetRandomItem(Chest_Percantage_Modifier.Config.ChestType.Golden);
                if (pickup != PickupIndex.none)
                    self.SetFieldValue("dropPickup", pickup);
            }

            orig.Invoke(self);
        }

        private PickupIndex GetRandomItem(Config.ChestType chestType)
        {
            PickupIndex item = PickupIndex.none;

            int rng = random.Next(0, 101);
            float tier1Chance = 0;
            float tier2Chance = 0;
            float tier3Chance = 0;

            switch (chestType)
            {
                case Chest_Percantage_Modifier.Config.ChestType.Normal:
                    tier1Chance = config.GetValue(1, Chest_Percantage_Modifier.Config.ChestType.Normal);
                    tier2Chance = config.GetValue(2, Chest_Percantage_Modifier.Config.ChestType.Normal);
                    tier3Chance = config.GetValue(3, Chest_Percantage_Modifier.Config.ChestType.Normal);

                    if (rng < tier1Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier1);
                    }
                    else if (rng < tier1Chance + tier2Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier2);
                    }
                    else if (rng < tier1Chance + tier2Chance + tier3Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier3);
                    }
                    break;

                case Chest_Percantage_Modifier.Config.ChestType.Large:
                    tier1Chance = config.GetValue(1, Chest_Percantage_Modifier.Config.ChestType.Large);
                    tier2Chance = config.GetValue(2, Chest_Percantage_Modifier.Config.ChestType.Large);
                    tier3Chance = config.GetValue(3, Chest_Percantage_Modifier.Config.ChestType.Large);

                    if (rng < tier1Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier1);
                    }
                    else if (rng < tier1Chance + tier2Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier2);
                    }
                    else if (rng < tier1Chance + tier2Chance + tier3Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier3);
                    }
                    break;

                case Chest_Percantage_Modifier.Config.ChestType.Golden:
                    tier1Chance = config.GetValue(1, Chest_Percantage_Modifier.Config.ChestType.Golden);
                    tier2Chance = config.GetValue(2, Chest_Percantage_Modifier.Config.ChestType.Golden);
                    tier3Chance = config.GetValue(3, Chest_Percantage_Modifier.Config.ChestType.Golden);

                    if (rng < tier1Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier1);
                    }
                    else if (rng < tier1Chance + tier2Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier2);
                    }
                    else if (rng < tier1Chance + tier2Chance + tier3Chance)
                    {
                        item = GetRandomTierItem(ItemTier.Tier3);
                    }
                    break;

                default:
                    break;
            }

            return item;
        }

        private PickupIndex GetRandomTierItem(ItemTier tier)
        {
            List<PickupIndex> allItems;
            PickupIndex item;

            switch (tier)
            {
                case ItemTier.Tier1:
                    allItems = Run.instance.availableTier1DropList;
                    break;
                case ItemTier.Tier2:
                    allItems = Run.instance.availableTier2DropList;
                    break;
                case ItemTier.Tier3:
                    allItems = Run.instance.availableTier3DropList;
                    break;
                default:
                    return PickupIndex.none;
                    break;
            }

            int value = random.Next(0, allItems.Count);

            item = allItems[value];

            return item;
        }
    }
}
