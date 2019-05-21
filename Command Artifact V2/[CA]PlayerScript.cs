using MiniRpcLib.Action;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Command_Artifact_V2
{
    class CA_PlayerScript : MonoBehaviour
    {
        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom;
        public Canvas canvas = RoR2.RoR2Application.instance.mainCanvas;
        public bool isChestOpening = false;

        private Sprite BGTex;
        private GameObject BuyMenu;
        private bool showBuyMenu;
        private Color buyable = new Color(100f / 255f, 200f / 255f, 50f / 255f, 191f / 255f);
        private GameObject ElementContainer;

        public void AwakeManual()
        {
            SetupGUI();
            SetBuyMenu(false);
            HideCursor();
        }

        public void SetBuyMenu(bool state)
        {
            BuyMenu.SetActive(state);
            isChestOpening = state;
            ToggleCursor();

            double st = state ? 1.0 : 0.0;

            ExampleCommandHostCustom.Invoke(x =>
            {
                x.Write("Buymenu");
                x.Write(st);
            });
        }

        private void HideCursor()
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            pes.cursorOpenerCount = pes.cursorOpenerCount = 0;
        }

        public int ToggleCursor()
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            pes.cursorOpenerCount = pes.cursorOpenerCount > 0 ? 0 : 1;
            return pes.cursorOpenerCount;
        }

        List<GameObject> items = new List<GameObject>();


        /*
        *  1 = Tier 1
        *  2 = Tier 2
        *  3 = Tier 3
        *  5 = Lunar - ToDo
        *  6 = Equipment
        */
        public void AddTierToGUI(int Tier, Transform transform)
        {
            //Remove any old Items
            ClearGUI();

            string type = "Item";

            //items.Add(CreateElement(new ItemClass("Test", new PickupIndex(ItemIndex.Behemoth)), "Item", transform));
            List<PickupIndex> itemsToAdd = new List<PickupIndex>();

            switch (Tier)
            {
                case 1:
                    if (false)
                    {
                        itemsToAdd = Run.instance.availableTier1DropList;
                    }
                    else
                    {
                        foreach (ItemIndex item in RoR2.ItemCatalog.tier1ItemList)
                        {
                            PickupIndex pickupIndex = new PickupIndex(item);
                            itemsToAdd.Add(pickupIndex);
                        }
                    }
                    break;
                case 2:
                    if (false)
                    {
                        itemsToAdd = Run.instance.availableTier2DropList;
                    }
                    else
                    {
                        foreach (ItemIndex item in RoR2.ItemCatalog.tier2ItemList)
                        {
                            PickupIndex pickupIndex = new PickupIndex(item);
                            itemsToAdd.Add(pickupIndex);
                        }
                    }
                    break;
                case 3:
                    if (false)
                    {
                        itemsToAdd = Run.instance.availableTier3DropList;
                    }
                    else
                    {
                        foreach (ItemIndex item in RoR2.ItemCatalog.tier3ItemList)
                        {
                            PickupIndex pickupIndex = new PickupIndex(item);
                            itemsToAdd.Add(pickupIndex);
                        }
                    }
                    break;

                case 5:
                    break;

                case 6:
                    type = "Equipment";

                    if (false)
                    {
                        itemsToAdd = Run.instance.availableEquipmentDropList;
                        List<PickupIndex> lunarItems = itemsToAdd.FindAll(x => x.IsLunar());
                        for (int i = 0; i < lunarItems.Count; i++)
                        {
                            itemsToAdd.Remove(lunarItems[i]);
                        }
                    }
                    else
                    {
                        foreach (EquipmentIndex item in RoR2.EquipmentCatalog.equipmentList)
                        {
                            PickupIndex pickupIndex = new PickupIndex(item);
                            //Remove Lunar Equipment, because that doesnt make sense...
                            if (!pickupIndex.IsLunar())
                            {
                                itemsToAdd.Add(pickupIndex);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }

            for (int i = 0; i < itemsToAdd.Count; i++)
            {
                string name = Language.GetString(itemsToAdd[i].GetPickupNameToken());
                PickupIndex pickupIndex = itemsToAdd[i];
                if(type == "Item")
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(pickupIndex.itemIndex);
                    Sprite icon = Resources.Load<Sprite>(itemDef.pickupIconPath);

                    items.Add(CreateElement(new ItemClass(name, pickupIndex, icon), type, transform));
                }
                else
                {
                    EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupIndex.equipmentIndex);
                    Sprite icon = Resources.Load<Sprite>(equipmentDef.pickupIconPath);

                    items.Add(CreateElement(new ItemClass(name, pickupIndex, icon), type, transform));
                }
            }
        }

        public void ClearGUI()
        {
            foreach (GameObject go in items)
            {
                Destroy(go);
            }
        }

        public void SetupGUI()
        {
            HUD Hud = FindObjectOfType<HUD>();
            BGTex = Instantiate<Sprite>(Hud.lunarCoinContainer.transform.parent.GetComponent<Image>().sprite);
            DontDestroyOnLoad(BGTex);

            BuyMenu = CreateBGImage();
            GameObject Panel = CreateMainPanel(BuyMenu);
            GameObject Title = CreateTitle(Panel);

            GameObject ListContainer = CreateContainer(Panel, "Element Container", 0);

            this.ElementContainer = CreateEleContainer(ListContainer);
            this.ElementContainer.transform.SetAsFirstSibling();

            ListContainer.GetComponent<ScrollRect>().content = this.ElementContainer.GetComponent<RectTransform>();
        }

        private GameObject CreateElement(ItemClass item, string type, Transform transform)
        {
            GameObject Panel = new GameObject("Element"); //Create the GameObject
            Image img = Panel.AddComponent<Image>();
            Button btn = Panel.AddComponent<Button>();

            img.color = buyable;

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

            GameObject icon = new GameObject("Icon");
            icon.AddComponent<Image>();
            icon.GetComponent<Image>().sprite = item.Icon;
            icon.GetComponent<RectTransform>().SetParent(btn.GetComponent<RectTransform>());
            icon.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0f);
            icon.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1f);
            icon.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
            //Left Bottom
            icon.GetComponent<RectTransform>().offsetMin = new Vector2(25, 0);
            //-Right -Top
            icon.GetComponent<RectTransform>().offsetMax = new Vector2(-50, 0);
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 0);

            btn.GetComponentInChildren<Text>().text = string.Format("{0}", item.Name);

            if (type == "Item")
            {
                btn.onClick.AddListener(delegate ()
                {
                    SetBuyMenu(false);
                    ExampleCommandHostCustom.Invoke(x =>
                    {
                        ItemIndex itemindex = item.PickupIndex.itemIndex;

                        x.Write("CreatePickupDroplet_Item");
                        x.Write(itemindex);
                        x.Write(transform);
                    });
                });
            }
            else
            {
                btn.onClick.AddListener(delegate ()
                {
                    SetBuyMenu(false);
                    ExampleCommandHostCustom.Invoke(x =>
                    {
                        EquipmentIndex itemindex = item.PickupIndex.equipmentIndex;

                        x.Write("CreatePickupDroplet_Equipment");
                        x.Write(itemindex);
                        x.Write(transform);
                    });
                });
            }

            Panel.GetComponent<RectTransform>().SetParent(ElementContainer.transform);
            Panel.SetActive(true); //Activate the GameObject

            Panel.AddComponent<LayoutElement>();
            Panel.GetComponent<LayoutElement>().preferredHeight = 50;
            Panel.GetComponent<LayoutElement>().flexibleWidth = 1;

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

            TextObj.GetComponent<Text>().text = "Select Menu. Please select the Item you want!";

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
