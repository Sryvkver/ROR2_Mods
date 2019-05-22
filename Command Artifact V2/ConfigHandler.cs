using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Command_Artifact_V2
{
    class ConfigHandler
    {
        #region Wrappers

        private static ConfigWrapper<string> Normal_Chest_Percantages_Conf { get; set; }
        private static ConfigWrapper<string> Large_Chest_Percantages_Conf { get; set; }
        private static ConfigWrapper<string> Golden_Chest_Percantages_Conf { get; set; }
        private static ConfigWrapper<string> Rusty_Chest_Percantages_Conf { get; set; }

        private static ConfigWrapper<string> TimeScale_Conf { get; set; }

        private static ConfigWrapper<bool> Everything_Avaiable_Conf { get; set; }

        #endregion

        #region Public

        public float[] Normal_Chest_Percantages
        {
            get
            {
                string floatString = Normal_Chest_Percantages_Conf.Value;
                float[] floatVal = new float[3];
                bool sucess = false;

                sucess = float.TryParse(floatString.Split(',')[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[0]);
                if (!sucess)
                    floatVal[0] = 80;

                sucess = float.TryParse(floatString.Split(',')[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[1]);
                if (!sucess)
                    floatVal[1] = 20;

                sucess = float.TryParse(floatString.Split(',')[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[2]);
                if (!sucess)
                    floatVal[2] = 0.1f;

                return floatVal;
            }
            set
            {
                Normal_Chest_Percantages_Conf.Value = String.Format("{0},{1},{2}", value[0], value[1], value[2]);
            }
        }
        public float[] Large_Chest_Percantages
        {
            get
            {
                string floatString = Large_Chest_Percantages_Conf.Value;
                float[] floatVal = new float[3];
                bool sucess = false;

                sucess = float.TryParse(floatString.Split(',')[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[0]);
                if (!sucess)
                    floatVal[0] = 80;

                sucess = float.TryParse(floatString.Split(',')[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[1]);
                if (!sucess)
                    floatVal[1] = 20;

                sucess = float.TryParse(floatString.Split(',')[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[2]);
                if (!sucess)
                    floatVal[2] = 0.1f;

                return floatVal;
            }
            set
            {
                Large_Chest_Percantages_Conf.Value = String.Format("{0},{1},{2}", value[0], value[1], value[2]);
            }
        }
        public float[] Golden_Chest_Percantages
        {
            get
            {
                string floatString = Golden_Chest_Percantages_Conf.Value;
                float[] floatVal = new float[3];
                bool sucess = false;

                sucess = float.TryParse(floatString.Split(',')[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[0]);
                if (!sucess)
                    floatVal[0] = 80;

                sucess = float.TryParse(floatString.Split(',')[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[1]);
                if (!sucess)
                    floatVal[1] = 20;

                sucess = float.TryParse(floatString.Split(',')[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[2]);
                if (!sucess)
                    floatVal[2] = 0.1f;

                return floatVal;
            }
            set
            {
                Golden_Chest_Percantages_Conf.Value = String.Format("{0},{1},{2}", value[0], value[1], value[2]);
            }
        }
        public float[] Rusty_Chest_Percantages
        {
            get
            {
                string floatString = Rusty_Chest_Percantages_Conf.Value;
                float[] floatVal = new float[3];
                bool sucess = false;

                sucess = float.TryParse(floatString.Split(',')[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[0]);
                if (!sucess)
                    floatVal[0] = 80;

                sucess = float.TryParse(floatString.Split(',')[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[1]);
                if (!sucess)
                    floatVal[1] = 20;

                sucess = float.TryParse(floatString.Split(',')[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatVal[2]);
                if (!sucess)
                    floatVal[2] = 0.1f;

                return floatVal;
            }
            set
            {
                Rusty_Chest_Percantages_Conf.Value = String.Format("{0},{1},{2}", value[0], value[1], value[2]);
            }
        }

        public float TimeScale
        {
            get
            {
                float timescale = 0.25f;

                float.TryParse(TimeScale_Conf.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out timescale);

                return timescale;
            }
            set
            {
                TimeScale_Conf.Value = value.ToString();
            }
        }

        public bool Everything_Avaiable
        {
            get
            {
                return Everything_Avaiable_Conf.Value;
            }
            set
            {
                Everything_Avaiable_Conf.Value = value;
            }
        }

        #endregion

        public void Init(ConfigFile Config)
        {
            Normal_Chest_Percantages_Conf = Config.Wrap<string>("Percantages", "Normal_Chest", "How likely each tier is to appear in a Normal chest. (Default: \"80,20,0.1\") (Format: \"Tier 1,Tier 2,Tier 3\")", "80,20,0.1");
            Large_Chest_Percantages_Conf = Config.Wrap<string>("Percantages", "Large_Chest", "How likely each tier is to appear in a Large chest. (Default: \"0,80,20\") (Format: \"Tier 1,Tier 2,Tier 3\")", "0,80,2");
            Golden_Chest_Percantages_Conf = Config.Wrap<string>("Percantages", "Golden_Chest", "How likely each tier is to appear in a Golden chest. (Default: \"0,0,100\") (Format: \"Tier 1,Tier 2,Tier 3\")", "0,0,100");
            Rusty_Chest_Percantages_Conf = Config.Wrap<string>("Percantages", "Rusty_Chest", "How likely each tier is to appear in a Rusty chest. (Default: \"80,20,0.1\") (Format: \"Tier 1,Tier 2,Tier 3\")", "80,20,0.5");
            Everything_Avaiable_Conf = Config.Wrap<bool>("General", "Everything_Avaiable", "Should items that havent been unlocked yet be avaiable (Default: false)", false);
            TimeScale_Conf = Config.Wrap<string>("General", "Timescale", "How fast should time pass by when the select menu is open (Default \"0.25\")", "0.25");
        }
    }
}
