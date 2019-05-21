using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Command_Artifact_V2
{
    class ItemClass
    {
        public string Name;
        public Sprite Icon;
        public PickupIndex PickupIndex;

        public ItemClass(string Name, PickupIndex pickupIndex, Sprite icon)
        {
            this.Name = Name;
            this.PickupIndex = pickupIndex;
            this.Icon = icon;
        }

        public ItemClass(string Name, PickupIndex pickupIndex)
        {
            this.Name = Name;
            this.PickupIndex = pickupIndex;
        }
    }
}
