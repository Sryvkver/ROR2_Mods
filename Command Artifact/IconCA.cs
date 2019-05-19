using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Command_Artifact
{
    class IconCA : MonoBehaviour
    {
        public GameObject image;
        public ItemDef ItemDef;
        public EquipmentDef EquipmentDef;

        public IconCA(ItemDef itemDef, GenericNotification genericNotification, int size)
        {
            Sprite sprite = Resources.Load<Sprite>(itemDef.pickupIconPath);

            GameObject image = ImageOBJ("Commander_Image");

            image.GetComponent<Image>().sprite = sprite;
            image.transform.SetParent(genericNotification.transform);
            image.transform.position = Vector3.zero;
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
            this.image = image;
            this.ItemDef = itemDef;
        }

        public IconCA(EquipmentDef itemDef, GenericNotification genericNotification, int size)
        {
            Sprite sprite = Resources.Load<Sprite>(itemDef.pickupIconPath);

            GameObject image = ImageOBJ("Commander_Image");

            image.GetComponent<Image>().sprite = sprite;
            image.transform.SetParent(genericNotification.transform);
            image.transform.position = Vector3.zero;
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
            this.image = image;
            this.EquipmentDef = itemDef;
        }

        public static GameObject ImageOBJ(string name = "Commander_Image")
        {
            GameObject image = new GameObject(name);
            image.AddComponent<Image>();
            if (image.GetComponent<CanvasRenderer>() == null || !image.GetComponent<CanvasRenderer>())
                image.AddComponent<CanvasRenderer>();
            if (image.GetComponent<RectTransform>() == null || !image.GetComponent<RectTransform>())
                image.AddComponent<RectTransform>();
            return image;
        }

        public void SetPos(int x, int y)
        {
            this.image.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }
    }
}
