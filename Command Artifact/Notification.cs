using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Command_Artifact
{
    //Used some code from @kookehs sorry about it, but it was way to tempting
    class Notification : MonoBehaviour
    {

        public GameObject RootObject { get; set; }

        public GenericNotification GenericNotification { get; set; }

        public Func<string> GetTitle { get; set; }

        public Func<string> GetDescription { get; set; }

        public Transform Parent { get; set; }

        public int ItemsInLine = 10;

        public List<IconCA> iconsCA = new List<IconCA>();

        public GameObject selector = null;
        private Sprite shadow = null;

        private void Awake()
        {
            this.Parent = RoR2Application.instance.mainCanvas.transform;
            this.RootObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NotificationPanel2"));
            this.GenericNotification = this.RootObject.GetComponent<GenericNotification>();
            this.GenericNotification.transform.SetParent(this.Parent);
            this.GenericNotification.iconImage.enabled = false;
        }

        private void Update()
        {
            if (this.GenericNotification == null)
            {
                UnityEngine.Object.Destroy(this);
                return;
            }
            typeof(LanguageTextMeshController).GetField("resolvedString", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.GenericNotification.titleText, this.GetTitle());
            typeof(LanguageTextMeshController).GetMethod("UpdateLabel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this.GenericNotification.titleText, new object[0]);
        }

        public void SetPosition(Vector3 position)
        {
            this.RootObject.transform.position = position;
        }

        public void SetSize(Vector2 size)
        {
            this.GenericNotification.GetComponent<RectTransform>().sizeDelta = size;
        }

        private void OnDestroy()
        {
            UnityEngine.Object.Destroy(this.GenericNotification);
            UnityEngine.Object.Destroy(this.RootObject);
        }

        public void Clear()
        {
            for (int i = 0; i < iconsCA.Count; i++)
            {
                Destroy(iconsCA[i].image);
                Destroy(iconsCA[i]);
            }
            //Remove all Images
            //while(GameObject.Find("Commander_Image") != null)
            //{
            //    Destroy(GameObject.Find("Commander_Image"));
            //}
            iconsCA.Clear();
        }

        public void PopulateTier(ItemTier tier)
        {
            if (selector == null)
                selector = createSelector();
            setSelectorPos(0);
            //List<ItemIndex> tier1 = ItemCatalog.tier1ItemList;
            List<ItemIndex> tierItems = getAvaiableItems(tier);

            int line = 0;
            int itemIndex = 0;
            for (int i = 0; i < tierItems.Count; i++)
            {
                ItemDef item = ItemCatalog.GetItemDef(tierItems[i]);
                if (String.IsNullOrEmpty(item.pickupIconPath))
                    return;

                //GenericNotification.gameObject
                IconCA icon = new IconCA(item, GenericNotification);
                iconsCA.Add(icon);

                if (i % ItemsInLine == 0)
                {
                    line++;
                    itemIndex = 0;
                }
                int x = (int)(-GenericNotification.GetComponent<RectTransform>().sizeDelta.x / 2 + 20 + itemIndex++ * 50);
                int y = (int)(-25 + (-line + 3) * 50);

                icon.SetPos(x, y);
            }
        }
        /*
        public void PopulateTier1()
        {
            if(selector == null)
                selector = createSelector();
            setSelectorPos(0);
            //List<ItemIndex> tier1 = ItemCatalog.tier1ItemList;
            List<ItemIndex> tier1 = getAvaiableItems(ItemTier.Tier1);

            int line = 0;
            int itemIndex = 0;
            for (int i = 0; i < tier1.Count; i++)
            {
                ItemDef item = ItemCatalog.GetItemDef(tier1[i]);
                if (String.IsNullOrEmpty(item.pickupIconPath))
                    return;

                //GenericNotification.gameObject
                IconCA icon = new IconCA(item, GenericNotification);
                iconsCA.Add(icon);

                if (i % ItemsInLine == 0)
                {
                    line++;
                    itemIndex = 0;
                }
                int x = (int)(-GenericNotification.GetComponent<RectTransform>().sizeDelta.x / 2 + 20 + itemIndex++ * 50);
                int y = (int)(-25 + (-line+3) * 50);

                icon.SetPos(x, y);
            }
        }

        public void PopulateTier2()
        {
            if (selector == null)
                selector = createSelector();
            setSelectorPos(0);
            //List<ItemIndex> tier2 = ItemCatalog.tier2ItemList;
            List<ItemIndex> tier2 = getAvaiableItems(ItemTier.Tier2);

            int line = 0;
            int itemIndex = 0;
            for (int i = 0; i < tier2.Count; i++)
            {
                ItemDef item = ItemCatalog.GetItemDef(tier2[i]);
                if (String.IsNullOrEmpty(item.pickupIconPath))
                    return;

                //GenericNotification.gameObject
                IconCA icon = new IconCA(item, GenericNotification);
                iconsCA.Add(icon);

                if (i % ItemsInLine == 0)
                {
                    line++;
                    itemIndex = 0;
                }
                int x = (int)(-GenericNotification.GetComponent<RectTransform>().sizeDelta.x / 2 + 20 + itemIndex++ * 50);
                int y = (int)(-25 + (-line + 3) * 50);

                icon.SetPos(x, y);
            }
        }

        public void PopulateTier3()
        {
            if (selector == null)
                selector = createSelector();
            setSelectorPos(0);
            //List<ItemIndex> tier3 = ItemCatalog.tier3ItemList;
            List<ItemIndex> tier3 = getAvaiableItems(ItemTier.Tier3);

            int line = 0;
            int itemIndex = 0;
            for (int i = 0; i < tier3.Count; i++)
            {
                ItemDef item = ItemCatalog.GetItemDef(tier3[i]);
                if (String.IsNullOrEmpty(item.pickupIconPath))
                    return;

                //GenericNotification.gameObject
                IconCA icon = new IconCA(item, GenericNotification);
                iconsCA.Add(icon);

                if (i % ItemsInLine == 0)
                {
                    line++;
                    itemIndex = 0;
                }
                int x = (int)(-GenericNotification.GetComponent<RectTransform>().sizeDelta.x / 2 + 20 + itemIndex++ * 50);
                int y = (int)(-25 + (-line + 3) * 50);

                icon.SetPos(x, y);
            }
        }
        */
        private List<ItemIndex> getAvaiableItems(ItemTier itemTier)
        {
            List<ItemIndex> items = new List<ItemIndex>();
            switch (itemTier)
            {
                case ItemTier.Tier1:
                    List<PickupIndex> tier1 = Run.instance.availableTier1DropList;
                    for (int i = 0; i < tier1.Count; i++)
                    {
                        items.Add(tier1[i].itemIndex);
                    }
                    break;
                case ItemTier.Tier2:
                    List<PickupIndex> tier2 = Run.instance.availableTier2DropList;
                    for (int i = 0; i < tier2.Count; i++)
                    {
                        items.Add(tier2[i].itemIndex);
                    }
                    break;
                case ItemTier.Tier3:
                    List<PickupIndex> tier3 = Run.instance.availableTier3DropList;
                    for (int i = 0; i < tier3.Count; i++)
                    {
                        items.Add(tier3[i].itemIndex);
                    }
                    break;
                default:
                    break;
            }
            return items;
        }

        private GameObject createSelector()
        {
            try
            {
                Destroy(this.selector);
                Destroy(GameObject.Find("Selector_CA"));
            }
            catch (Exception)
            {
            }

            GameObject selector = IconCA.ImageOBJ(name: "Selector_CA");
            selector.transform.SetParent(GenericNotification.transform);

            if(shadow == null)
            {
                Texture2D texture2D = new Texture2D(1, 1);
                texture2D.SetPixel(0, 0, Color.gray);
                texture2D.wrapMode = TextureWrapMode.Repeat;
                texture2D.Apply();

                shadow = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
            }

            selector.GetComponent<Image>().sprite = shadow;
            selector.transform.position = Vector3.zero;
            selector.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);

            return selector;
        }

        public void setSelectorPos(int itemIndex)
        {
            if (this.selector == null)
                return;

            int line = (int)Mathf.Floor(itemIndex/ItemsInLine);
            int col = itemIndex % ItemsInLine;
            //Chat.AddMessage(line + " - " + col);

            int x = (int)(-GenericNotification.GetComponent<RectTransform>().sizeDelta.x / 2 + 20 + col * 50);
            int y = (int)(-25 + (-line+2) * 50);

            this.selector.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }
    }
}
