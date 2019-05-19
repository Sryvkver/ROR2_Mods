using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DioWho
{
    class GO_Script : MonoBehaviour
    {
        public int ogItem { get; set; }

        public void setOGItem(int index)
        {
            ogItem = index;
        }

        public int getOGItem()
        {
            return ogItem;
        }
    }
}
