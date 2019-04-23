using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Chest_Percantage_Modifier
{
    class Config
    {
        #region Wrappers
        private static ConfigWrapper<int> Tier1Percantage_Normal_Conf { get; set; }
        private static ConfigWrapper<int> Tier2Percantage_Normal_Conf { get; set; }
        private static ConfigWrapper<int> Tier3Percantage_Normal_Conf { get; set; }

        private static ConfigWrapper<int> Tier1Percantage_Large_Conf { get; set; }
        private static ConfigWrapper<int> Tier2Percantage_Large_Conf { get; set; }
        private static ConfigWrapper<int> Tier3Percantage_Large_Conf { get; set; }

        private static ConfigWrapper<int> Tier1Percantage_Golden_Conf { get; set; }
        private static ConfigWrapper<int> Tier2Percantage_Golden_Conf { get; set; }
        private static ConfigWrapper<int> Tier3Percantage_Golden_Conf { get; set; }
        #endregion

        List<ConfigWrapper<int>> configs = new List<ConfigWrapper<int>>();
        List<float> normalizedConfigs = new List<float>();
        public void Init(ConfigFile Config)
        {
            Tier1Percantage_Normal_Conf = Config.Wrap<int>("Normal Chest", "Tier 1 Percantage", "How likely it is that tier 1 pops up on a Normal chest (0-100) Default: 80", 80);
            Tier2Percantage_Normal_Conf = Config.Wrap<int>("Normal Chest", "Tier 2 Percantage", "How likely it is that tier 2 pops up on a Normal chest (0-100) Default: 20", 20);
            Tier3Percantage_Normal_Conf = Config.Wrap<int>("Normal Chest", "Tier 3 Percantage", "How likely it is that tier 3 pops up on a Normal chest (0-100) Default: 01", 01);

            Tier1Percantage_Large_Conf = Config.Wrap<int>("Large Chest", "Tier 1 Percantage", "How likely it is that tier 1 pops up on a Large chest (0-100) Default: 00", 0);
            Tier2Percantage_Large_Conf = Config.Wrap<int>("Large Chest", "Tier 2 Percantage", "How likely it is that tier 2 pops up on a Large chest (0-100) Default: 80", 80);
            Tier3Percantage_Large_Conf = Config.Wrap<int>("Large Chest", "Tier 3 Percantage", "How likely it is that tier 3 pops up on a Large chest (0-100) Default: 20", 20);

            Tier1Percantage_Golden_Conf = Config.Wrap<int>("Golden Chest", "Tier 1 Percantage", "How likely it is that tier 1 pops up on a Golden chest (0-100) Default: 000", 000);
            Tier2Percantage_Golden_Conf = Config.Wrap<int>("Golden Chest", "Tier 2 Percantage", "How likely it is that tier 2 pops up on a Golden chest (0-100) Default: 000", 000);
            Tier3Percantage_Golden_Conf = Config.Wrap<int>("Golden Chest", "Tier 3 Percantage", "How likely it is that tier 3 pops up on a Golden chest (0-100) Default: 100", 100);

            configs.Add(Tier1Percantage_Normal_Conf);
            configs.Add(Tier2Percantage_Normal_Conf);
            configs.Add(Tier3Percantage_Normal_Conf);

            configs.Add(Tier1Percantage_Large_Conf);
            configs.Add(Tier2Percantage_Large_Conf);
            configs.Add(Tier3Percantage_Large_Conf);

            configs.Add(Tier1Percantage_Golden_Conf);
            configs.Add(Tier2Percantage_Golden_Conf);
            configs.Add(Tier3Percantage_Golden_Conf);

            NormalizeValues();
        }

        private void NormalizeValues()
        {
            normalizedConfigs.Clear();
            for (int i = 0; i < configs.Count; i++)
            {
                normalizedConfigs.Add(configs[i].Value);
            }
            for (int i = 0; i < 3; i++)
            {
                float sum = configs[i * 3 + 0].Value + configs[i * 3 + 1].Value + configs[i * 3 + 2].Value;
                for (int j = 0; j < 3; j++)
                {
                    if (sum <= 100)
                    {
                        float unNormalized = configs[i * 3 + j].Value;
                        normalizedConfigs[i * 3 + j] = unNormalized;
                    }
                    else
                    {
                        float unNormalized = configs[i * 3 + j].Value;
                        float normalized = (100 / sum * unNormalized);
                        normalizedConfigs[i * 3 + j] = normalized;
                    }
                }

            }
        }

        public float GetValue(int tier, ChestType chestType)
        {
            if (tier < 1 || tier > 3)
                return -1;

            int index = (int)chestType * 3 + tier - 1;

            if (index > 8)
            {
                Debug.Log("Invalid Index when getting Chest percantage...");
                return -1;
            }

            float value = normalizedConfigs[index];
            //Debug.Log("Unnormalized: " + configs[index].Value);
            //Debug.Log("Normalized: " + normalizedConfigs[index]);
            return value;
        }

        public enum ChestType
        {
            Normal,
            Large,
            Golden
        }
    }
}
