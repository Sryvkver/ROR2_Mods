using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BepInEx.Configuration;

namespace KillShop
{
    class ConfigHandler
    {
        #region Wrappers 
        private static ConfigWrapper<int> Tier1_Price_Conf { get; set; }
        private static ConfigWrapper<int> Tier2_Price_Conf { get; set; }
        private static ConfigWrapper<int> Tier3_Price_Conf { get; set; }
        private static ConfigWrapper<int> LunarItem_Price_Conf { get; set; }
        private static ConfigWrapper<int> Equipment_Price_Conf { get; set; }
        private static ConfigWrapper<string> Price_Increase_Conf { get; set; }
        #endregion

        #region publics
        public int Tier1_Price
        {
            get
            {
                return Tier1_Price_Conf.Value;
            }
            set
            {
                Tier1_Price_Conf.Value = value;
            }
        }

        public int Tier2_Price
        {
            get
            {
                return Tier2_Price_Conf.Value;
            }
            set
            {
                Tier2_Price_Conf.Value = value;
            }
        }

        public int Tier3_Price
        {
            get
            {
                return Tier3_Price_Conf.Value;
            }
            set
            {
                Tier3_Price_Conf.Value = value;
            }
        }

        public int Lunar_Price
        {
            get
            {
                return LunarItem_Price_Conf.Value;
            }
            set
            {
                LunarItem_Price_Conf.Value = value;
            }
        }

        public int Equipment_Price
        {
            get
            {
                return Equipment_Price_Conf.Value;
            }
            set
            {
                Equipment_Price_Conf.Value = value;
            }
        }

        public float Price_Increase
        {
            get
            {
                float priceIncrease;


                if (float.TryParse(Price_Increase_Conf.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out priceIncrease))
                {
                    return priceIncrease;
                }

                return 1.25f;
            }
            set
            {
                Price_Increase_Conf.Value = value.ToString();
            }
        }
        #endregion

        public void Init(ConfigFile Config)
        {
            Tier1_Price_Conf = Config.Wrap<int>("Prices", "Tier_1", "How much should a Tier 1 Item cost?", 5);
            Tier2_Price_Conf = Config.Wrap<int>("Prices", "Tier_2", "How much should a Tier 2 Item cost?", 20);
            Tier3_Price_Conf = Config.Wrap<int>("Prices", "Tier_3", "How much should a Tier 3 Item cost?", 50);
            LunarItem_Price_Conf = Config.Wrap<int>("Prices", "Lunar", "How much should a Lunar Item cost?", 50);
            Equipment_Price_Conf = Config.Wrap<int>("Prices", "Equipment", "How much should Equipment cost?", 100);
            Price_Increase_Conf = Config.Wrap<string>("Prices", "Price_Increase", "How much should the Price increase with each Purchase?", "1.25");
        }
    }
}
