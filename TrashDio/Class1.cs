using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DioWho
{
    [BepInPlugin("dev.felixire.TrashDio", "TrashDio", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {

        System.Random random;

        public void Awake()
        {
            var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            int seed = (int)((System.DateTime.UtcNow - epochStart).TotalSeconds / 2);
            Debug.Log("Seed: " + seed);
            random = new System.Random(seed);

            On.RoR2.GenericPickupController.GrantItem += GenericPickupController_GrantItem;
            //On.RoR2.Run.Update += Run_Update;
        }

        private void Run_Update(On.RoR2.Run.orig_Update orig, Run self)
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                LocalUser localUser = LocalUserManager.GetFirstLocalUser();
                Transform transform = localUser.cachedBodyObject.transform;

                PickupIndex dio = new PickupIndex(ItemIndex.Bear);
                PickupDropletController.CreatePickupDroplet(dio, transform.position, Vector3.up * 5f);
            }
        }

        private void GenericPickupController_GrantItem(On.RoR2.GenericPickupController.orig_GrantItem orig, RoR2.GenericPickupController self, RoR2.CharacterBody body, RoR2.Inventory inventory)
        {
            orig.Invoke(self, body, inventory);

            //Debug.Log(self.pickupIndex);

            PickupIndex trashDio = new PickupIndex(ItemIndex.Bear);

            if (self.pickupIndex == trashDio)
            {
                List<PickupIndex> tier1Items = Run.instance.availableTier1DropList;

                int rng = random.Next(0, tier1Items.Count);
                if (tier1Items[rng] == trashDio)
                    rng++;

                //ItemIndex rngItem = tier3Items[rng].itemIndex;
                //tier3Items[rng].itemIndex.ToString();
                //tier3Items[rng].GetPickupNameToken();

                string rngItemName = Language.GetString(tier1Items[rng].GetPickupNameToken());
                Color32 color = tier1Items[rng].GetPickupColor();
                Color32 colorDark = tier1Items[rng].GetPickupColorDark();



                //<color=#307FFF>Lunar Coin</color><style=cEvent> Dropped</style>
                Chat.AddMessage(String.Format("<style=cEvent>You were expecting </style>{0} {1}", Util.GenerateColoredString(rngItemName, color), Util.GenerateColoredString("BUT IT WAS ME TRASH DIO", colorDark)));
            }
        }
    }
}
