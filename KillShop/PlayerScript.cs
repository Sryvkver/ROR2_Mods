using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.Events;
using UnityEngine.UI;
using MiniRpcLib.Action;
using UnityEngine.Networking;
using RoR2.UI;
using UnityEngine.EventSystems;

namespace KillShop
{
    class PlayerScript : MonoBehaviour
    {
        public Canvas canvas = RoR2.RoR2Application.instance.mainCanvas;
        public ConfigHandler config;

        public int _kills = 0;
        private UnityAction OnKillsChanged;
        public int kills
        {
            get
            {
                return _kills;
            }

            set
            {
                _kills = value;
                OnKillsChanged();
            }
        }

        //Currently to true for testing
        private bool showBuyMenu = false;


        private int widthMenu = 500;
        private int heightMenu = 750;

        public GameObject KillCounter;
        private GameObject BuyMenu;
        private ShopItems shopItems;
        private Sprite BGTex;
        private Color buyable = new Color(100f / 255f, 200f / 255f, 50f / 255f, 191f / 255f);
        private Color notBuyable = new Color(200f / 255f, 50f / 255f, 100f / 255f, 191f / 255f);

        private List<GameObject> allBuyMenuItems = new List<GameObject>();
        private List<GameObject> allBuyMenuCategories = new List<GameObject>();
        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom;


        public void AwakeManual()
        {
            shopItems = new ShopItems();
            shopItems.BuildItemList(ExampleCommandHostCustom, config);
            SetupGUI();
            SetBuyMenu(false);
        }

        public void SetBuyMenu(bool state)
        {
            BuyMenu.SetActive(state);

            double st = state ? 1.0 : 0.0;

            ExampleCommandHostCustom.Invoke(x =>
            {
                x.Write("Buymenu");
                x.Write(st);
            });
        }

        public void AddKillCounter()
        {
            KillCounter = CreateKillCounter();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                showBuyMenu = !showBuyMenu;
                SetBuyMenu(showBuyMenu);
                ToggleCursor();
            }

            /*if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                kills += 100;
            }*/
        }

        public int ToggleCursor()
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            pes.cursorOpenerCount = pes.cursorOpenerCount > 0 ? 0 : 1;
            return pes.cursorOpenerCount;
        }

        public void SetupGUI()
        {
            AddKillCounter();

            BuyMenu = CreateBGImage();
            GameObject Panel = CreateMainPanel(BuyMenu);
            GameObject DescriptionPanel = CreateDescriptionPanel(BuyMenu);
            GameObject DescriptionImage = CreateDescriptionImage(DescriptionPanel);
            GameObject DescriptionText = CreateDescriptionText(DescriptionImage);
            DescriptionPanel.SetActive(false);
            GameObject Title = CreateTitle(Panel);

            GameObject ListContainer = CreateContainer(Panel, "Element Container", 0);
            GameObject TabContainer = CreateContainer(Panel, "Tab Container", 1);

            GameObject ElementContainer = CreateEleContainer(ListContainer);
            ElementContainer.transform.SetAsFirstSibling();

            GameObject TabEleContainer = CreateTabContainer(TabContainer);
            TabEleContainer.transform.SetAsFirstSibling();

            ListContainer.GetComponent<ScrollRect>().content = ElementContainer.GetComponent<RectTransform>();
            TabContainer.GetComponent<ScrollRect>().content = TabEleContainer.GetComponent<RectTransform>();

            //Name, Icon, function, price, 
            List<Item> allItems = shopItems.GetShopItems();
            List<Item> categories = shopItems.GetCategories();

            //Hide all Tier1 Items
            //allBuyMenuItems.FindAll(x => x.name == ShopItems.Categories.Tier1);

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].IsFoldOut)
                {
                    allBuyMenuCategories.Add(CreateCategory(TabEleContainer, allItems[i].Categorie));
                }
                else
                {
                    allBuyMenuItems.Add(CreateElement(ElementContainer, DescriptionPanel, allItems[i].Name, allItems[i].Price, allItems[i].Function, i, allItems[i].Categorie, allItems[i].IsFoldOut, allItems[i].Icon, allItems[i].Description));
                    //Disable all items at start
                    allBuyMenuItems[allBuyMenuItems.Count-1].SetActive(false);
                }

            }


            OnKillsChanged += delegate ()
            {
                Title.GetComponent<Text>().text = "Buy Menu - Souls: " + kills;
                KillCounter.GetComponent<MoneyText>().targetValue = kills;
            };
        }

        private GameObject CreateElement(GameObject container, GameObject descriptionOBJ, string name, int price, Func<PlayerCharacterMasterController, int> func, int index, ShopItems.Categories categorie, bool isFoldout, Sprite itemIcon = null, string description = null)
        {
            GameObject Empty = new GameObject("ElementHolder");
            Empty.AddComponent<RectTransform>();
            Empty.name = categorie.ToString();

            Empty.AddComponent<ItemOnHover>();
            Empty.GetComponent<ItemOnHover>().descriptionOBJ = descriptionOBJ;
            Empty.GetComponent<ItemOnHover>().description = description;

            GameObject Panel = new GameObject("Element"); //Create the GameObject
            Image img = Panel.AddComponent<Image>();
            Button btn = Panel.AddComponent<Button>();

            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1f);

            Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);

            img.color = notBuyable;

            GameObject textObj = new GameObject("Text");
            textObj.AddComponent<Text>();
            textObj.GetComponent<RectTransform>().SetParent(btn.GetComponent<RectTransform>());
            textObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0f);
            textObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1f);
            textObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            //Left Bottom
            textObj.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            //-Right -Top
            textObj.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            textObj.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            btn.GetComponentInChildren<Text>().color = Color.white;
            btn.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;

            if(isFoldout)
            {
                btn.GetComponentInChildren<Text>().text = string.Format("{0}", name);

                btn.onClick.AddListener(delegate ()
                {
                    List<GameObject> gameObjects = allBuyMenuItems.FindAll(x => x.name == categorie.ToString());

                    foreach (GameObject item in gameObjects)
                    {
                        if (item == Panel)
                            continue;

                        item.SetActive(!item.activeSelf);
                    }
                });
            }
            else
            {
                GameObject icon = new GameObject("Icon");
                icon.AddComponent<Image>();
                icon.GetComponent<Image>().sprite = itemIcon;
                icon.GetComponent<RectTransform>().SetParent(btn.GetComponent<RectTransform>());
                icon.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0f);
                icon.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1f);
                icon.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
                //Left Bottom
                icon.GetComponent<RectTransform>().offsetMin = new Vector2(25, 0);
                //-Right -Top
                icon.GetComponent<RectTransform>().offsetMax = new Vector2(-50, 0);
                icon.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 0);

                btn.GetComponentInChildren<Text>().text = string.Format("{0} ({1} Souls)", name, price);

                shopItems.GetShopItems()[index].OnBought += delegate ()
                {
                    btn.GetComponentInChildren<Text>().text = string.Format("{0} ({1} Souls)", name, shopItems.GetShopItems()[index].Price);
                };

                btn.onClick.AddListener(delegate ()
                {
                    if (shopItems.GetShopItems()[index].Price > kills)
                        return;

                    //Save the Price
                    int oldPrice = shopItems.GetShopItems()[index].Price;
                    //Call the function (Updates the price)
                    func(LocalUserManager.GetFirstLocalUser().cachedMasterController);
                    //Deduct the souls
                    kills -= oldPrice;
                    //Update the text
                    //btn.GetComponentInChildren<Text>().text = string.Format("{0} ({1} Souls)", name, shopItems.GetShopItems()[index].Price);
                    shopItems.GetShopItems()[index].OnBought();
                });

                //btn.OnPointerEnter

                OnKillsChanged += delegate ()
                {
                    if(shopItems.GetShopItems()[index].Price > kills)
                    {
                        img.color = notBuyable;
                    }
                    else
                    {
                        img.color = buyable;
                    }
                };
            }


            Empty.GetComponent<RectTransform>().SetParent(container.transform);
            Panel.GetComponent<RectTransform>().SetParent(Empty.transform);
            Panel.SetActive(true); //Activate the GameObject

            Empty.AddComponent<LayoutElement>();
            Empty.GetComponent<LayoutElement>().preferredHeight = 50;
            Empty.GetComponent<LayoutElement>().flexibleWidth = 1;

            AddXButtons(Empty, price, func, index);

            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            return Empty;
        }

        private void AddXButtons(GameObject elementHolder, int price, Func<PlayerCharacterMasterController, int> func, int index)
        {
            GameObject Panel = new GameObject("X5 Button"); //Create the GameObject
            Image img = Panel.AddComponent<Image>();

            img.color = notBuyable;

            //Panel.AddComponent<RectTransform>();

            Button btn = Panel.AddComponent<Button>();

            Panel.GetComponent<RectTransform>().SetParent(elementHolder.transform);

            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1f);

            Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(45.5f, 50);
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(402f, 0);

            GameObject textObj = new GameObject("Text");
            textObj.AddComponent<Text>();
            textObj.GetComponent<RectTransform>().SetParent(btn.GetComponent<RectTransform>());
            textObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0f);
            textObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1f);
            textObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            //Left Bottom
            textObj.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            //-Right -Top
            textObj.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);


            textObj.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            btn.GetComponentInChildren<Text>().color = Color.white;
            btn.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
            btn.GetComponentInChildren<Text>().text = "x5 \r\n<size=8>(" + GetPriceXTime(5, (int)Math.Round((float)shopItems.GetShopItems()[index].Price), config.Price_Increase) + ")</size>";

            btn.onClick.AddListener(delegate ()
            {
                //Wow this is too acurate so it deducts more/less sometime because we round the Price above :/
                //int price5X = (int)Math.Round((float)shopItems.GetShopItems()[index].Price * Math.Pow(config.Price_Increase, 5));

                int price5X = GetPriceXTime(5, (int)Math.Round((float)shopItems.GetShopItems()[index].Price), config.Price_Increase);

                if (price5X > kills)
                    return;

                //Save the Price
                int oldPrice = shopItems.GetShopItems()[index].Price;
                //Call the function 5 Times
                for (int i = 0; i < 5; i++)
                {
                    //Call the function (Updates the price)
                    func(LocalUserManager.GetFirstLocalUser().cachedMasterController);
                }
                //Deduct the souls
                kills -= price5X;

                //Update the text
                //btn.GetComponentInChildren<Text>().text = string.Format("{0} \r\n<size=8>({1} Souls)</size>", "x5", price5X);
                shopItems.GetShopItems()[index].OnBought();
            });



            GameObject x10Button = Instantiate(Panel, elementHolder.transform);
            x10Button.name = "X10Button";
            x10Button.GetComponentInChildren<Text>().text = "x10 \r\n<size=8>(" + GetPriceXTime(10, (int)Math.Round((float)shopItems.GetShopItems()[index].Price), config.Price_Increase) + ")</size>"; ;
            x10Button.GetComponent<RectTransform>().anchoredPosition = new Vector2(449.5f, 0);

            Button x10BTN = x10Button.GetComponent<Button>();
            Image x10img = x10Button.GetComponent<Image>();
            x10BTN.onClick.AddListener(delegate ()
            {
                //Wow this is too acurate so it deducts more/less sometime because we round the Price above :/
                //int price10X = (int)Math.Round((float)shopItems.GetShopItems()[index].Price * Math.Pow(config.Price_Increase, 10));

                int price10X = GetPriceXTime(10, (int)Math.Round((float)shopItems.GetShopItems()[index].Price), config.Price_Increase);

                if (price10X > kills)
                    return;

                //Save the Price
                int oldPrice = shopItems.GetShopItems()[index].Price;
                //Call the function 5 Times
                for (int i = 0; i < 10; i++)
                {
                    //Call the function (Updates the price)
                    func(LocalUserManager.GetFirstLocalUser().cachedMasterController);
                }
                //Deduct the souls
                kills -= price10X;

                //Update the text
                //x10BTN.GetComponentInChildren<Text>().text = string.Format("{0} \r\n<size=8>({1} Souls)</size>", "x10", price10X);
                shopItems.GetShopItems()[index].OnBought();
            });

            shopItems.GetShopItems()[index].OnBought += delegate ()
            {
                btn.GetComponentInChildren<Text>().text = string.Format("{0} \r\n<size=8>({1})</size>", "x5", GetPriceXTime(5, (int)Math.Round((float)shopItems.GetShopItems()[index].Price), config.Price_Increase));
                x10BTN.GetComponentInChildren<Text>().text = string.Format("{0} \r\n<size=8>({1})</size>", "x10", GetPriceXTime(10, (int)Math.Round((float)shopItems.GetShopItems()[index].Price), config.Price_Increase));
            };

            OnKillsChanged += delegate ()
            {
                int price5X = GetPriceXTime(5, (int)Math.Round((float)shopItems.GetShopItems()[index].Price), config.Price_Increase);
                int price10X = GetPriceXTime(10, (int)Math.Round((float)shopItems.GetShopItems()[index].Price), config.Price_Increase);
                if (price5X > kills)
                {
                    img.color = notBuyable;
                    x10img.color = notBuyable;
                }
                else
                {
                    img.color = buyable;
                    if(price10X > kills)
                    {
                        x10img.color = notBuyable;
                    }
                    else
                    {
                        x10img.color = buyable;
                    }
                }
            };

            Panel.SetActive(true); //Activate the GameObject
        }

        private int GetPriceXTime(int times, int price, float increase)
        {
            int newPrice = price;
            int toBuyPrice = price;

            //This feels so shitty, but it accounts for the rounding issue, so thats nice I guess
            for (int i = 0; i < times-1; i++)
            {
                newPrice = (int)Math.Round((float)newPrice * increase);
                toBuyPrice += newPrice;
            }


            return toBuyPrice;
        }

        private GameObject CreateDescriptionPanel(GameObject bgImage)
        {
            GameObject Panel = new GameObject("Main Panel"); //Create the GameObject
                                                             //PanelIMG.sprite = currentSprite; //Set the Sprite of the Image Component on the new GameObject
                                                             //Panel.AddComponent<RectTransform>();
            Panel.AddComponent<Image>();
            Panel.GetComponent<Image>().color = new Color(38f / 255f, 37f / 255f, 42f / 255f, 32f / 255f);
            Panel.GetComponent<RectTransform>().SetParent(bgImage.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject
            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(.5f, .5f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(.5f, .5f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
            Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 100);
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(425f, 325f);

            Panel.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            return Panel;
        }

        private GameObject CreateDescriptionImage(GameObject descriptionPanel)
        {
            GameObject Panel = new GameObject("Killcounter Description BG Image"); //Create the GameObject
            Image PanelIMG = Panel.AddComponent<Image>(); //Add the Image Component script
            PanelIMG.sprite = BGTex;
            PanelIMG.type = Image.Type.Sliced;
            PanelIMG.fillCenter = true;
            //PanelIMG.sprite = currentSprite; //Set the Sprite of the Image Component on the new GameObject
            Panel.GetComponent<RectTransform>().SetParent(descriptionPanel.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject
            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            //Left Bottom
            Panel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            //-Right -Top
            Panel.GetComponent<RectTransform>().offsetMax = new Vector2(-0, -0);

            Panel.GetComponent<RectTransform>().localScale = new Vector3(.989f, .994f, 1);

            return Panel;
        }

        private GameObject CreateDescriptionText(GameObject descriptionImage)
        {
            GameObject Panel = new GameObject("Killcounter Description Text"); //Create the GameObject
            Text Text = Panel.AddComponent<Text>();

            Panel.GetComponent<RectTransform>().SetParent(descriptionImage.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject
            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);

            Panel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            Panel.GetComponent<RectTransform>().offsetMax = new Vector2(-0, 0);

            Text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            //Text.fontSize = 14;
            Text.color = Color.white;

            Text.text = "Just a Sample Text!";

            Text.alignment = TextAnchor.MiddleCenter;

            Panel.GetComponent<RectTransform>().localScale = new Vector3(.989f, .994f, 1);

            return Panel;
        }

        private GameObject CreateCategory(GameObject container, ShopItems.Categories categorie)
        {
            GameObject Panel = new GameObject("Element"); //Create the GameObject#

            Image img = Panel.AddComponent<Image>();
            Button btn = Panel.AddComponent<Button>();

            img.color = notBuyable;

            GameObject textObj = new GameObject("Text");
            textObj.AddComponent<Text>();
            textObj.GetComponent<RectTransform>().SetParent(btn.GetComponent<RectTransform>());
            textObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0f);
            textObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1f);
            textObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            //Left Bottom
            textObj.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            //-Right -Top
            textObj.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            textObj.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            btn.GetComponentInChildren<Text>().color = Color.white;
            btn.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
            btn.GetComponentInChildren<Text>().text = categorie.ToString();

            /*EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener( (eventdata) => { Debug.Log("TEST"); } );

            btn.triggers.Add(entry);*/
            btn.onClick.AddListener(delegate() 
            {
                List<GameObject> gameObjects = allBuyMenuItems.FindAll(x => x.name != categorie.ToString());
                List<GameObject> gameObjectsOfCategorie = allBuyMenuItems.FindAll(x => x.name == categorie.ToString());

                foreach (GameObject item in gameObjects)
                {
                    item.SetActive(false);
                }

                foreach (GameObject item in gameObjectsOfCategorie)
                {
                    item.SetActive(true);
                }

                foreach (GameObject item in allBuyMenuCategories)
                {
                    item.GetComponent<Image>().color = notBuyable;
                }
                img.color = buyable;
            });

            Panel.GetComponent<RectTransform>().SetParent(container.transform);
            Panel.SetActive(true); //Activate the GameObject

            Panel.AddComponent<LayoutElement>();
            Panel.GetComponent<LayoutElement>().preferredWidth = 80;
            Panel.GetComponent<LayoutElement>().flexibleHeight = 1;


            Panel.transform.localScale = Vector3.one;

            return Panel;
        }

        private GameObject CreateEleContainer(GameObject listContainer)
        {
            GameObject Panel = new GameObject("ElementContainer"); //Create the GameObject
            Image PanelIMG = Panel.AddComponent<Image>(); //Add the Image Component script
            PanelIMG.color = Color.clear;
            //PanelIMG.sprite = currentSprite; //Set the Sprite of the Image Component on the new GameObject
            Panel.GetComponent<RectTransform>().SetParent(listContainer.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject
            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
            Panel.GetComponent<RectTransform>().offsetMin = new Vector2(0, Panel.GetComponent<RectTransform>().offsetMin.y);
            Panel.GetComponent<RectTransform>().offsetMax = new Vector2(-0, Panel.GetComponent<RectTransform>().offsetMax.y);
            //Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

            Panel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Panel.AddComponent<VerticalLayoutGroup>();
            Panel.GetComponent<VerticalLayoutGroup>().padding.right = 8;
            Panel.GetComponent<VerticalLayoutGroup>().spacing = 5;
            Panel.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            Panel.GetComponent<VerticalLayoutGroup>().childControlWidth = true;
            Panel.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
            Panel.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;

            Panel.GetComponent<RectTransform>().localScale = Vector3.one;

            return Panel;
        }

        private GameObject CreateTabContainer(GameObject listContainer)
        {
            GameObject Panel = new GameObject("Tab Element Container"); //Create the GameObject
            Image PanelIMG = Panel.AddComponent<Image>(); //Add the Image Component script
            PanelIMG.color = Color.clear;
            //PanelIMG.sprite = currentSprite; //Set the Sprite of the Image Component on the new GameObject
            Panel.GetComponent<RectTransform>().SetParent(listContainer.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject
            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(0f, .5f);

            Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 655);

            //Left Bottom
            Panel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            //-Right -Top
            Panel.GetComponent<RectTransform>().offsetMax = new Vector2(-0, -0);
            //Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

            Panel.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            Panel.AddComponent<HorizontalLayoutGroup>();
            Panel.GetComponent<HorizontalLayoutGroup>().padding.top = 5;
            Panel.GetComponent<HorizontalLayoutGroup>().spacing = 5;
            Panel.GetComponent<HorizontalLayoutGroup>().childControlHeight = true;
            Panel.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;
            Panel.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = false;
            Panel.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;

            Panel.GetComponent<RectTransform>().localScale = Vector3.one;

            return Panel;
        }

        private GameObject CreateContainer(GameObject panel, string name, int type)
        {
            GameObject Panel = new GameObject(name); //Create the GameObject
            Image PanelIMG = Panel.AddComponent<Image>(); //Add the Image Component script
                                                          //PanelIMG.sprite = currentSprite; //Set the Sprite of the Image Component on the new GameObject
            Panel.GetComponent<RectTransform>().SetParent(panel.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject

            Panel.AddComponent<Mask>();
            Panel.GetComponent<Mask>().showMaskGraphic = false;

            if (type == 0)
            {
                Panel.GetComponent<RectTransform>().anchorMax = new Vector2(.5f, .5f);
                Panel.GetComponent<RectTransform>().anchorMin = new Vector2(.5f, .5f);
                Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 687f);
                Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -31f);

                GameObject scrollbar = CreateScrollbarVert(Panel);
                Panel.AddComponent<ScrollRect>().verticalScrollbar = scrollbar.GetComponent<Scrollbar>();
                Panel.GetComponent<ScrollRect>().horizontal = false;
                Panel.GetComponent<ScrollRect>().vertical = true;
            }
            else
            {
                Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
                Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
                Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, 1f);
                Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25f);
                Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -37f);

                GameObject scrollbar = CreateScrollbarHorz(Panel);
                Panel.AddComponent<ScrollRect>().horizontalScrollbar = scrollbar.GetComponent<Scrollbar>();
                Panel.GetComponent<ScrollRect>().horizontal = true;
                Panel.GetComponent<ScrollRect>().vertical = false;
            }

            Panel.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
            Panel.GetComponent<ScrollRect>().scrollSensitivity = 10;

            Panel.GetComponent<RectTransform>().localScale = Vector3.one;

            return Panel;
        }

        private GameObject CreateScrollbarHorz(GameObject Container)
        {
            GameObject Panel = new GameObject("Scrollbar"); //Create the GameObject
            Scrollbar scrollbar = Panel.AddComponent<Scrollbar>();
            Panel.GetComponent<RectTransform>().SetParent(Container.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject

            GameObject handle = CreateSlidingArea(Panel);
            handle.AddComponent<RectTransform>();

            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 5);
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            scrollbar.direction = Scrollbar.Direction.LeftToRight;
            scrollbar.targetGraphic = handle.GetComponent<Image>();
            scrollbar.handleRect = handle.GetComponent<RectTransform>();

            ColorBlock colors = new ColorBlock();
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(209, 209, 209);
            colors.pressedColor = new Color(209, 209, 209);
            colors.colorMultiplier = 1;

            scrollbar.colors = colors;

            Panel.GetComponent<RectTransform>().localScale = Vector3.one;

            return Panel;
        }

        private GameObject CreateScrollbarVert(GameObject Container)
        {
            GameObject Panel = new GameObject("Scrollbar"); //Create the GameObject
            Scrollbar scrollbar = Panel.AddComponent<Scrollbar>();
            Panel.GetComponent<RectTransform>().SetParent(Container.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject

            GameObject handle = CreateSlidingArea(Panel);
            handle.AddComponent<RectTransform>();

            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 0);
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2.5f, 0);

            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = handle.GetComponent<Image>();
            scrollbar.handleRect = handle.GetComponent<RectTransform>();

            ColorBlock colors = new ColorBlock();
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(209, 209, 209);
            colors.pressedColor = new Color(209, 209, 209);
            colors.colorMultiplier = 1;

            scrollbar.colors = colors;

            Panel.GetComponent<RectTransform>().localScale = Vector3.one;

            return Panel;
        }

        private GameObject CreateSlidingArea(GameObject scrollbar)
        {
            GameObject Panel = new GameObject("Sliding Area"); //Create the GameObject
            Panel.AddComponent<RectTransform>();
            Panel.GetComponent<RectTransform>().SetParent(scrollbar.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject

            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
            //Left Bottom
            Panel.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            //-Right -Top
            Panel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);

            GameObject Handle = CreateHandle(Panel);

            Panel.GetComponent<RectTransform>().localScale = Vector3.one;

            return Handle;
        }

        private GameObject CreateHandle(GameObject SlidingArea)
        {
            GameObject Panel = new GameObject("Handle"); //Create the GameObject
            Panel.AddComponent<RectTransform>();
            Panel.GetComponent<RectTransform>().SetParent(SlidingArea.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject

            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
            //Left Bottom
            Panel.GetComponent<RectTransform>().offsetMin = new Vector2(-10, -10);
            //-Right -Top
            Panel.GetComponent<RectTransform>().offsetMax = new Vector2(10, 10);

            Panel.AddComponent<Image>();
            Panel.GetComponent<Image>().sprite = null;
            Panel.GetComponent<Image>().color = Color.white;

            Panel.GetComponent<RectTransform>().localScale = Vector3.one;

            return Panel;
        }

        private GameObject CreateKillCounter()
        {
            HUD Hud = FindObjectOfType<HUD>();
            Vector2 ogSize = Hud.lunarCoinContainer.transform.parent.GetComponent<RectTransform>().sizeDelta;

            //Increase size to bypass the squishing
            Hud.lunarCoinContainer.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(ogSize.x, ogSize.y + 20);

            //Duplicate the Lunarcoin Display
            GameObject KillCounterContainer = Instantiate(Hud.lunarCoinContainer, Hud.lunarCoinContainer.transform.parent);
            KillCounterContainer.name = "KillCounterContainer";
            MoneyText KillCounterText = KillCounterContainer.GetComponent<MoneyText>();
            KillCounterText.targetValue = kills;
            FlashPanel KillCounterFlash = KillCounterContainer.GetComponent<FlashPanel>();
            KillCounterFlash.flashRectTransform.GetComponent<Image>().color = Color.cyan;

            BGTex = Instantiate<Sprite>(Hud.lunarCoinContainer.transform.parent.GetComponent<Image>().sprite);
            DontDestroyOnLoad(BGTex);

            return KillCounterText.gameObject;
        }

        private GameObject CreateTitle(GameObject panel)
        {
            GameObject TextObj = new GameObject("Title");
            Text Text = TextObj.AddComponent<Text>();

            TextObj.GetComponent<RectTransform>().SetParent(panel.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            TextObj.SetActive(true); //Activate the GameObject
            TextObj.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
            TextObj.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            TextObj.GetComponent<RectTransform>().pivot = new Vector2(.5f, 1f);

            TextObj.GetComponent<RectTransform>().offsetMin = new Vector2(0, TextObj.GetComponent<RectTransform>().offsetMin.y);
            TextObj.GetComponent<RectTransform>().offsetMax = new Vector2(-0, TextObj.GetComponent<RectTransform>().offsetMax.y);
            TextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);

            TextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

            TextObj.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            TextObj.GetComponent<Text>().fontSize = 20;
            TextObj.GetComponent<Text>().color = Color.white;

            TextObj.GetComponent<Text>().text = "Buy Menu - Souls: " + kills;

            TextObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            TextObj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            return TextObj;
        }

        private GameObject CreateMainPanel(GameObject BG)
        {
            GameObject Panel = new GameObject("Main Panel"); //Create the GameObject
                                                             //PanelIMG.sprite = currentSprite; //Set the Sprite of the Image Component on the new GameObject
                                                             //Panel.AddComponent<RectTransform>();
            Panel.AddComponent<Image>();
            Panel.GetComponent<Image>().color = new Color(38f / 255f, 37f / 255f, 42f / 255f, 32f / 255f);
            Panel.GetComponent<RectTransform>().SetParent(BG.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject
            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            //Left Bottom
            Panel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            //-Right -Top
            Panel.GetComponent<RectTransform>().offsetMax = new Vector2(-0, -0);

            Panel.GetComponent<RectTransform>().localScale = new Vector3(.989f, .994f, 1);

            return Panel;
        }

        private GameObject CreateBGImage()
        {
            GameObject Panel = new GameObject("Killcounter BG Image"); //Create the GameObject
            Image PanelIMG = Panel.AddComponent<Image>(); //Add the Image Component script
            PanelIMG.sprite = BGTex;
            PanelIMG.type = Image.Type.Sliced;
            PanelIMG.fillCenter = true;
            //PanelIMG.sprite = currentSprite; //Set the Sprite of the Image Component on the new GameObject
            Panel.GetComponent<RectTransform>().SetParent(canvas.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
            Panel.SetActive(true); //Activate the GameObject
            Panel.GetComponent<RectTransform>().anchorMax = new Vector2(.5f, .5f);
            Panel.GetComponent<RectTransform>().anchorMin = new Vector2(.5f, .5f);
            Panel.GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
            Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 750);
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

            return Panel;
        }

    }
}
