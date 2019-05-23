using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KillShop
{
    class ItemOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject descriptionOBJ;
        public string description;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (description == null || description == "")
                return;

            descriptionOBJ.GetComponentInChildren<Text>().text = description;
            descriptionOBJ.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            descriptionOBJ.SetActive(false);
        }
    }
}
