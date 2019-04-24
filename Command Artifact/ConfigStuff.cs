using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Command_Artifact 
{
    class ConfigStuff
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

        private static ConfigWrapper<string> TimeScale_Conf { get; set; }
        private static ConfigWrapper<string> TimeScaleDefault_Conf { get; set; }
        private static ConfigWrapper<string> SelectKey_Conf { get; set; }
        #endregion

        public float TimeScale {
            get
            {
                float timeScale;

                
                if(float.TryParse(TimeScale_Conf.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out timeScale))
                {
                    return timeScale;
                }

                return 0.1f;
            }
            set
            {
                TimeScale_Conf.Value = value.ToString();
            }
        }
        public float TimeScaleDefault
        {
            get
            {
                float timeScale;


                if (float.TryParse(TimeScaleDefault_Conf.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out timeScale))
                {
                    return timeScale;
                }

                return 1.0f;
            }
            set
            {
                TimeScaleDefault_Conf.Value = value.ToString();
            }
        }

        public KeyCode SelectButton
        {
            get
            {
                KeyCode select;

                if(Enum.TryParse(SelectKey_Conf.Value, out select))
                {
                    return select;
                    
                }

                return KeyCode.F;
            }
            set
            {
                SelectKey_Conf.Value = value.ToString();
            }
        }

        List<ConfigWrapper<int>> configs = new List<ConfigWrapper<int>>();
        List<float> normalizedConfigs = new List<float>();

        public void Init(ConfigFile Config)
        {
            TimeScaleDefault_Conf = Config.Wrap<string>("Commander Artifact", "Time Scale D", "How fast the game is, when the selection window is not open (Default: 1.0)", "1.0");
            TimeScale_Conf = Config.Wrap<string>("Commander Artifact", "Time Scale", "How fast the game is, when the selection window is open (normal game speed: 1.0)", "0.1");
            SelectKey_Conf = Config.Wrap<string>("Commander Artifact", "Select Key", "Which key to use for selection (Check https://docs.unity3d.com/ScriptReference/KeyCode.html for the names)", "F");

            //Might have to reverse this, because its reversed in the actual file...
            Tier1Percantage_Normal_Conf = Config.Wrap<int>("Normal Chest", "Tier 1 Percantage", "How likely it is that tier 1 pops up on a Normal chest (0-100)", 80);
            Tier2Percantage_Normal_Conf = Config.Wrap<int>("Normal Chest", "Tier 2 Percantage", "How likely it is that tier 2 pops up on a Normal chest (0-100)", 20);
            Tier3Percantage_Normal_Conf = Config.Wrap<int>("Normal Chest", "Tier 3 Percantage", "How likely it is that tier 3 pops up on a Normal chest (0-100)", 01);

            Tier1Percantage_Large_Conf = Config.Wrap<int>("Large Chest", "Tier 1 Percantage", "How likely it is that tier 1 pops up on a Large chest (0-100)", 0);
            Tier2Percantage_Large_Conf = Config.Wrap<int>("Large Chest", "Tier 2 Percantage", "How likely it is that tier 2 pops up on a Large chest (0-100)", 80);
            Tier3Percantage_Large_Conf = Config.Wrap<int>("Large Chest", "Tier 3 Percantage", "How likely it is that tier 3 pops up on a Large chest (0-100)", 20);

            Tier1Percantage_Golden_Conf = Config.Wrap<int>("Golden Chest", "Tier 1 Percantage", "How likely it is that tier 1 pops up on a Golden chest (0-100)", 000);
            Tier2Percantage_Golden_Conf = Config.Wrap<int>("Golden Chest", "Tier 2 Percantage", "How likely it is that tier 2 pops up on a Golden chest (0-100)", 000);
            Tier3Percantage_Golden_Conf = Config.Wrap<int>("Golden Chest", "Tier 3 Percantage", "How likely it is that tier 3 pops up on a Golden chest (0-100)", 100);

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

        /*public int GetValue(int tier, ChestType chestType)
        {
            if (tier < 1 || tier > 3)
                return -1;

            int index = (int)chestType * 3 + tier - 1;

            if (index > 8)
            {
                Debug.Log("Invalid Index when getting Chest percantage...");
                return -1;
            }

            int value = configs[index].Value;
            //Debug.Log("Tier: " + tier);
            //Debug.Log("Chest: " + chestType);
            //Debug.Log("Value: " + value);
            return value;
        }*/

        public enum ChestType
        {
            Normal,
            Large,
            Golden
        }
    }
}
