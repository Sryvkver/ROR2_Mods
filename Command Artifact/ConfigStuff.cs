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

        private static ConfigWrapper<int> Tier1Percantage_Rusty_Conf { get; set; }
        private static ConfigWrapper<int> Tier2Percantage_Rusty_Conf { get; set; }
        private static ConfigWrapper<int> Tier3Percantage_Rusty_Conf { get; set; }

        private static ConfigWrapper<int> NotiSizeY_Conf { get; set; }
        private static ConfigWrapper<int> NotiSizeX_Conf { get; set; }

        private static ConfigWrapper<int> NotiIconSize_Conf { get; set; }

        private static ConfigWrapper<int> NotiPosY_Conf { get; set; }
        private static ConfigWrapper<int> NotiPosX_Conf { get; set; }

        private static ConfigWrapper<int> NotiItemsInLine_Conf { get; set; }

        private static ConfigWrapper<string> TimeScale_Conf { get; set; }
        private static ConfigWrapper<string> TimeScaleDefault_Conf { get; set; }
        private static ConfigWrapper<string> SelectKey_Conf { get; set; }
        private static ConfigWrapper<string> MoveUpKey_Conf { get; set; }
        private static ConfigWrapper<string> MoveLeftKey_Conf { get; set; }
        private static ConfigWrapper<string> MoveDownKey_Conf { get; set; }
        private static ConfigWrapper<string> MoveRightKey_Conf { get; set; }
        private static ConfigWrapper<bool> AllStuff_Conf { get; set; }
        #endregion

        #region public Stuff
        public int NotiIconSize
        {
            get
            {
                return NotiIconSize_Conf.Value;
            }
            set
            {
                NotiIconSize_Conf.Value = value;
            }
        }

        public int NotiSizeY
        {
            get
            {
                return NotiSizeY_Conf.Value;
            }
            set
            {
                NotiSizeY_Conf.Value = value;
            }
        }
        public int NotiSizeX
        {
            get
            {
                return NotiSizeX_Conf.Value;
            }
            set
            {
                NotiSizeX_Conf.Value = value;
            }
        }
        public int NotiItemsInLine
        {
            get
            {
                return NotiItemsInLine_Conf.Value;
            }
            set
            {
                NotiItemsInLine_Conf.Value = value;
            }
        }
        public float TimeScale
        {
            get
            {
                float timeScale;


                if (float.TryParse(TimeScale_Conf.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out timeScale))
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
        public bool AllAvaiable
        {
            get
            {
                bool allAvaiable = AllStuff_Conf.Value;
                return allAvaiable;
            }
            set
            {
                AllStuff_Conf.Value = value;
            }
        }

        public KeyCode SelectButton
        {
            get
            {
                KeyCode select;

                if (Enum.TryParse(SelectKey_Conf.Value, out select))
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

        public KeyCode UpButton
        {
            get
            {
                KeyCode up;

                if (Enum.TryParse(MoveUpKey_Conf.Value, out up))
                {
                    return up;

                }

                return KeyCode.UpArrow;
            }
            set
            {
                MoveUpKey_Conf.Value = value.ToString();
            }
        }
        public KeyCode LeftButton
        {
            get
            {
                KeyCode left;

                if (Enum.TryParse(MoveLeftKey_Conf.Value, out left))
                {
                    return left;

                }

                return KeyCode.LeftArrow;
            }
            set
            {
                MoveLeftKey_Conf.Value = value.ToString();
            }
        }
        public KeyCode DownButton
        {
            get
            {
                KeyCode down;

                if (Enum.TryParse(MoveDownKey_Conf.Value, out down))
                {
                    return down;

                }

                return KeyCode.DownArrow;
            }
            set
            {
                MoveDownKey_Conf.Value = value.ToString();
            }
        }
        public KeyCode RightButton
        {
            get
            {
                KeyCode right;

                if (Enum.TryParse(MoveRightKey_Conf.Value, out right))
                {
                    return right;

                }

                return KeyCode.RightArrow;
            }
            set
            {
                MoveRightKey_Conf.Value = value.ToString();
            }
        }
        #endregion


        List<ConfigWrapper<int>> configs = new List<ConfigWrapper<int>>();
        List<float> normalizedConfigs = new List<float>();

        public void Init(ConfigFile Config)
        {
            TimeScaleDefault_Conf = Config.Wrap<string>("Commander Artifact", "Time Scale D", "How fast the game is, when the selection window is not open (Default: 1.0)", "1.0");
            TimeScale_Conf = Config.Wrap<string>("Commander Artifact", "Time Scale", "How fast the game is, when the selection window is open (normal game speed: 1.0)", "0.1");

            SelectKey_Conf = Config.Wrap<string>("Commander Artifact", "Select Key", "Which key to use for selection (Check https://docs.unity3d.com/ScriptReference/KeyCode.html for the names)", "F");
            MoveUpKey_Conf = Config.Wrap<string>("Commander Artifact", "Up Key", "Which key to use for moving the selector up (Check https://docs.unity3d.com/ScriptReference/KeyCode.html for the names)", "UpArrow");
            MoveLeftKey_Conf = Config.Wrap<string>("Commander Artifact", "Left Key", "Which key to use for moving the selector left (Check https://docs.unity3d.com/ScriptReference/KeyCode.html for the names)", "LeftArrow");
            MoveDownKey_Conf = Config.Wrap<string>("Commander Artifact", "Down Key", "Which key to use for moving the selector down (Check https://docs.unity3d.com/ScriptReference/KeyCode.html for the names)", "DownArrow");
            MoveRightKey_Conf = Config.Wrap<string>("Commander Artifact", "Right Key", "Which key to use for moving the selector right (Check https://docs.unity3d.com/ScriptReference/KeyCode.html for the names)", "RightArrow");

            AllStuff_Conf = Config.Wrap<bool>("Commander Artifact", "Everything avaiable", "Should everything be avaiable, without having it unlocked", false);


            NotiIconSize_Conf = Config.Wrap<int>("Notification", "Size", "How big the item icon should be", 50);
            NotiSizeY_Conf = Config.Wrap<int>("Notification", "Y", "How big the notification is in the Y axis", 250);
            NotiSizeX_Conf = Config.Wrap<int>("Notification", "X", "How big the notification is in the X axis", 500);
            NotiItemsInLine_Conf = Config.Wrap<int>("Notification", "Count", "How many Items should be in one line", 10);

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

            Tier1Percantage_Rusty_Conf = Config.Wrap<int>("Rusty Chest", "Tier 1 Percantage", "How likely it is that tier 1 pops up on a Rusty chest (0-100) (DOES NOT SCALE WITH KEYS)", 000);
            Tier2Percantage_Rusty_Conf = Config.Wrap<int>("Rusty Chest", "Tier 2 Percantage", "How likely it is that tier 2 pops up on a Rusty chest (0-100) (DOES NOT SCALE WITH KEYS)", 000);
            Tier3Percantage_Rusty_Conf = Config.Wrap<int>("Rusty Chest", "Tier 3 Percantage", "How likely it is that tier 3 pops up on a Rusty chest (0-100) (DOES NOT SCALE WITH KEYS)", 100);

            configs.Add(Tier1Percantage_Normal_Conf);
            configs.Add(Tier2Percantage_Normal_Conf);
            configs.Add(Tier3Percantage_Normal_Conf);

            configs.Add(Tier1Percantage_Large_Conf);
            configs.Add(Tier2Percantage_Large_Conf);
            configs.Add(Tier3Percantage_Large_Conf);

            configs.Add(Tier1Percantage_Golden_Conf);
            configs.Add(Tier2Percantage_Golden_Conf);
            configs.Add(Tier3Percantage_Golden_Conf);

            configs.Add(Tier1Percantage_Rusty_Conf);
            configs.Add(Tier2Percantage_Rusty_Conf);
            configs.Add(Tier3Percantage_Rusty_Conf);

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

            if (index > 11)
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
            Golden,
            Rusty
        }
    }
}
