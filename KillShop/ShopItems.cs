using MiniRpcLib.Action;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static KillShop.ShopItems;

namespace KillShop
{
    class ShopItems
    {
        List<Item> allItems = new List<Item>();
        List<Item> categories = new List<Item>();

        public enum Categories
        {
            Tier1,
            Tier2,
            Tier3,
            Lunar,
            Equipment,
            Money,
            Experience,
            None
        }
        public List<Item> GetCategories()
        {
            return categories;
        }

        public List<Item> GetShopItems()
        {
            return allItems;
        }

        public void BuildItemList(IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom, ConfigHandler config)
        {
            #region Items

            #region Tier 1

            allItems.Add(new Item("Tier 1 Categorie", 0, Resources.Load<Sprite>("Textures/texSimpleTriangle"),  delegate (PlayerCharacterMasterController player)
            {
                return 0;
            },"", Categories.Tier1, true));

            int Tier1Price = config.Tier1_Price;

            List<ItemIndex> allTier1Items = ItemCatalog.tier1ItemList;

            for (int i = 0; i < allTier1Items.Count; i++)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(allTier1Items[i]);
                PickupIndex pickupIndex = new PickupIndex(allTier1Items[i]);
                ItemIndex item = allTier1Items[i];
                string name = Language.GetString(pickupIndex.GetPickupNameToken());
                string description = Language.GetString(itemDef.pickupToken);

                //Get all string between '<' '>' Character
                string[] testing = Regex.Matches(description, @"\<(.+?)\>")
                            .Cast<Match>()
                            .Select(s => s.Groups[1].Value).ToArray();

                //Remove all string from the description
                foreach (string str in testing)
                {
                    description = description.Replace(str, "");
                }
                //Remove '<' '>' Characters
                description = description.Replace("<", "").Replace(">", "");

                allItems.Add(new Item(name, description: description, price: Tier1Price, icon: Resources.Load<Sprite>(itemDef.pickupIconPath), func: delegate (PlayerCharacterMasterController player)
                {
                    Vector3 pos = player.master.GetBodyObject().transform.position;

                    //RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos, Vector3.up * 10f);
                    ExampleCommandHostCustom.Invoke(x =>
                    {
                        x.Write("CreatePickupDroplet_Item");
                        x.Write(item);
                        x.Write(pos);
                    });

                    int itemIndex = allItems.FindIndex(a => a.Name == name);
                    allItems[itemIndex].Price = (int)Math.Round((float)allItems[itemIndex].Price * config.Price_Increase);
                    return 0;
                }, categorie: Categories.Tier1));
            }
            #endregion

            #region Tier 2

            allItems.Add(new Item("Tier 2 Categorie", 0, delegate (PlayerCharacterMasterController player)
            {
                return 0;
            },"", Categories.Tier2, true));

            int Tier2Price = config.Tier2_Price;

            List<ItemIndex> allTier2Items = ItemCatalog.tier2ItemList;

            for (int i = 0; i < allTier2Items.Count; i++)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(allTier2Items[i]);
                PickupIndex pickupIndex = new PickupIndex(allTier2Items[i]);
                ItemIndex item = allTier2Items[i];
                string name = Language.GetString(pickupIndex.GetPickupNameToken());
                string description = Language.GetString(itemDef.pickupToken);

                //Get all string between '<' '>' Character
                string[] testing = Regex.Matches(description, @"\<(.+?)\>")
                            .Cast<Match>()
                            .Select(s => s.Groups[1].Value).ToArray();

                //Remove all string from the description
                foreach (string str in testing)
                {
                    description = description.Replace(str, "");
                }
                //Remove '<' '>' Characters
                description = description.Replace("<", "").Replace(">", "");

                allItems.Add(new Item(name, Tier2Price, Resources.Load<Sprite>(itemDef.pickupIconPath), delegate (PlayerCharacterMasterController player)
                {
                    Vector3 pos = player.master.GetBodyObject().transform.position;

                    //RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos, Vector3.up * 10f);
                    ExampleCommandHostCustom.Invoke(x =>
                    {
                        x.Write("CreatePickupDroplet_Item");
                        x.Write(item);
                        x.Write(pos);
                    });

                    int itemIndex = allItems.FindIndex(a => a.Name == name);
                    allItems[itemIndex].Price = (int)Math.Round(allItems[itemIndex].Price * config.Price_Increase);
                    return 0;
                },description, Categories.Tier2));
            }
            #endregion

            #region Tier 3

            allItems.Add(new Item("Tier 3 Categorie", 0, delegate (PlayerCharacterMasterController player)
            {
                return 0;
            },"", Categories.Tier3, true));

            int Tier3Price = config.Tier3_Price;

            List<ItemIndex> allTier3Items = ItemCatalog.tier3ItemList;

            for (int i = 0; i < allTier3Items.Count; i++)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(allTier3Items[i]);
                PickupIndex pickupIndex = new PickupIndex(allTier3Items[i]);
                ItemIndex item = allTier3Items[i];
                string name = Language.GetString(pickupIndex.GetPickupNameToken());
                string description = Language.GetString(itemDef.pickupToken);

                //Get all string between '<' '>' Character
                string[] testing = Regex.Matches(description, @"\<(.+?)\>")
                            .Cast<Match>()
                            .Select(s => s.Groups[1].Value).ToArray();

                //Remove all string from the description
                foreach (string str in testing)
                {
                    description = description.Replace(str, "");
                }
                //Remove '<' '>' Characters
                description = description.Replace("<", "").Replace(">", "");

                allItems.Add(new Item(name, Tier3Price, Resources.Load<Sprite>(itemDef.pickupIconPath), delegate (PlayerCharacterMasterController player)
                {
                    Vector3 pos = player.master.GetBodyObject().transform.position;

                    //RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos, Vector3.up * 10f);
                    ExampleCommandHostCustom.Invoke(x =>
                    {
                        x.Write("CreatePickupDroplet_Item");
                        x.Write(item);
                        x.Write(pos);
                    });

                    int itemIndex = allItems.FindIndex(a => a.Name == name);
                    allItems[itemIndex].Price = (int)Math.Round(allItems[itemIndex].Price * config.Price_Increase);
                    return 0;
                },description, Categories.Tier3));
            }
            #endregion

            #region Lunar

            allItems.Add(new Item("Lunar Categorie", 0, delegate (PlayerCharacterMasterController player)
            {
                return 0;
            },"", Categories.Lunar, true));

            int LunarItemPrice = config.Lunar_Price;

            List<ItemIndex> allLunarItems = ItemCatalog.lunarItemList;

            for (int i = 0; i < allLunarItems.Count; i++)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(allLunarItems[i]);
                PickupIndex pickupIndex = new PickupIndex(allLunarItems[i]);
                ItemIndex item = allLunarItems[i];
                string name = Language.GetString(pickupIndex.GetPickupNameToken());
                string description = Language.GetString(itemDef.pickupToken);

                //Get all string between '<' '>' Character
                string[] testing = Regex.Matches(description, @"\<(.+?)\>")
                            .Cast<Match>()
                            .Select(s => s.Groups[1].Value).ToArray();

                //Remove all string from the description
                foreach (string str in testing)
                {
                    description = description.Replace(str, "");
                }
                //Remove '<' '>' Characters
                description = description.Replace("<", "").Replace(">", "");

                allItems.Add(new Item(name, LunarItemPrice, Resources.Load<Sprite>(itemDef.pickupIconPath), delegate (PlayerCharacterMasterController player)
                {
                    Vector3 pos = player.master.GetBodyObject().transform.position;

                    //RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos, Vector3.up * 10f);
                    ExampleCommandHostCustom.Invoke(x =>
                    {
                        x.Write("CreatePickupDroplet_Item");
                        x.Write(item);
                        x.Write(pos);
                    });

                    int itemIndex = allItems.FindIndex(a => a.Name == name);
                    allItems[itemIndex].Price = (int)Math.Round(allItems[itemIndex].Price * config.Price_Increase);
                    return 0;
                },description, Categories.Lunar));
            }
            #endregion

            #endregion

            #region Equipment

            allItems.Add(new Item("Equipment Categorie", 0, Resources.Load<Sprite>("Textures/texSimpleTriangle"), delegate (PlayerCharacterMasterController player)
            {
                return 0;
            },"", Categories.Equipment, true));

            int EquipmentPrice = config.Equipment_Price;

            List<EquipmentIndex> allEquipment = EquipmentCatalog.equipmentList;

            for (int i = 0; i < allEquipment.Count; i++)
            {
                EquipmentDef itemDef = EquipmentCatalog.GetEquipmentDef(allEquipment[i]);
                PickupIndex pickupIndex = new PickupIndex(allEquipment[i]);
                EquipmentIndex equipmentIndex = allEquipment[i];
                string name = Language.GetString(pickupIndex.GetPickupNameToken());
                string description = Language.GetString(itemDef.pickupToken);

                //Get all string between '<' '>' Character
                string[] testing = Regex.Matches(description, @"\<(.+?)\>")
                            .Cast<Match>()
                            .Select(s => s.Groups[1].Value).ToArray();

                //Remove all string from the description
                foreach (string str in testing)
                {
                    description = description.Replace(str, "");
                }
                //Remove '<' '>' Characters
                description = description.Replace("<", "").Replace(">", "");

                allItems.Add(new Item(name, EquipmentPrice, Resources.Load<Sprite>(itemDef.pickupIconPath), delegate (PlayerCharacterMasterController player)
                {
                    Vector3 pos = player.master.GetBodyObject().transform.position;

                    //RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos, Vector3.up * 10f);
                    ExampleCommandHostCustom.Invoke(x =>
                    {
                        x.Write("CreatePickupDroplet_Equipment");
                        x.Write(equipmentIndex);
                        x.Write(pos);
                    });

                    int itemIndex = allItems.FindIndex(a => a.Name == name);
                    allItems[itemIndex].Price = (int)Math.Round(allItems[itemIndex].Price * config.Price_Increase);
                    return 0;
                },description, Categories.Equipment));
            }

            #endregion

            #region Exp

            allItems.Add(new Item("Exp Categorie", 0, delegate (PlayerCharacterMasterController player)
            {
                return 0;
            },"", Categories.Experience, true));

            allItems.Add(new Item("50 XP", 10, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GiveExperience(50);

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("AddExp");
                    x.Write(player.gameObject);
                    x.Write(50.0);
                });

                int i = allItems.FindIndex(a => a.Name == "50 XP");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.Experience));

            allItems.Add(new Item("250 XP", 25, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GiveExperience(250);

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("AddExp");
                    x.Write(player.gameObject);
                    x.Write(250.0);
                });

                int i = allItems.FindIndex(a => a.Name == "250 XP");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.Experience));

            allItems.Add(new Item("1K XP", 50, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GiveExperience(1000);

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("AddExp");
                    x.Write(player.gameObject);
                    x.Write(1000.0);
                });

                int i = allItems.FindIndex(a => a.Name == "1K XP");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.Experience));

            allItems.Add(new Item("10K XP", 400, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GiveExperience(10000);

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("AddExp");
                    x.Write(player.gameObject);
                    x.Write(10000.0);
                });

                int i = allItems.FindIndex(a => a.Name == "10K XP");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.Experience));
            #endregion

            #region Gold

            allItems.Add(new Item("Money Categorie", 0, delegate (PlayerCharacterMasterController player)
            {
                return 0;
            },"", Categories.Money, true));

            allItems.Add(new Item("150 Gold", 10, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GiveMoney(150);

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("AddMoney");
                    x.Write(player.gameObject);
                    x.Write(150.0);
                });

                int i = allItems.FindIndex(a => a.Name == "150 Gold");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.Money));

            allItems.Add(new Item("500 Gold", 25, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GiveMoney(500);

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("AddMoney");
                    x.Write(player.gameObject);
                    x.Write(500.0);
                });

                int i = allItems.FindIndex(a => a.Name == "500 Gold");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.Money));

            allItems.Add(new Item("1K Gold", 40, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GiveMoney(1000);

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("AddMoney");
                    x.Write(player.gameObject);
                    x.Write(1000.0);
                });

                int i = allItems.FindIndex(a => a.Name == "1K Gold");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.Money));

            allItems.Add(new Item("10K Gold", 380, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GiveMoney(10000);

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("AddMoney");
                    x.Write(player.gameObject);
                    x.Write(10000.0);
                });

                int i = allItems.FindIndex(a => a.Name == "10K Gold");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.Money));
            #endregion

            #region None
            allItems.Add(new Item("Undefined Categorie", 0, delegate (PlayerCharacterMasterController player)
            {
                return 0;
            },"", Categories.None, true));

            allItems.Add(new Item("Full HP", 10, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GetBody().healthComponent.health = player.master.GetBody().healthComponent.fullHealth;

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("FullHP");
                    x.Write(player.gameObject);
                });

                int i = allItems.FindIndex(a => a.Name == "Full HP");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            },"", Categories.None));

            allItems.Add(new Item("Full Shield", 10, delegate (PlayerCharacterMasterController player)
            {
                //player.body.healthComponent.health = player.body.healthComponent.fullHealth;
                //player.master.GetBody().healthComponent.health = player.master.GetBody().healthComponent.fullHealth;

                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("FullShield");
                    x.Write(player.gameObject);
                });

                int i = allItems.FindIndex(a => a.Name == "Full Shield");
                allItems[i].Price = (int)Math.Round(allItems[i].Price * config.Price_Increase);
                return 0;
            }, "", Categories.None));
            #endregion
        }
    }

    class Item
    {
        public string Name;
        public string Description;
        public int Price;
        public bool IsFoldOut;
        public Categories Categorie;
        public Sprite Icon; //Resources.Load<Sprite>(itemDef.pickupIconPath);
        public Func<PlayerCharacterMasterController, int> Function;
        public UnityAction OnBought;

        public Item(string name, int price, Sprite icon, Func<PlayerCharacterMasterController, int> func, string description, Categories categorie = Categories.None, bool isFoldout = false)
        {
            Name = name;
            Description = description;
            Price = price;
            Function = func;
            Icon = icon;
            Categorie = categorie;
            IsFoldOut = isFoldout;
        }

        public Item(string name, int price, Func<PlayerCharacterMasterController, int> func, string description, Categories categorie = Categories.None, bool isFoldout = false)
        {
            Name = name;
            Description = description;
            Price = price;
            Function = func;
            Categorie = categorie;
            IsFoldOut = isFoldout;
        }
    }
}
