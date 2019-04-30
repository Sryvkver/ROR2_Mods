using BepInEx;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DioWho
{
    [BepInPlugin("dev.felixire.DioExpecting", "DioExpecting", "1.0.0")]
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
            On.RoR2.PickupIndex.GetPickupDisplayPrefab += PickupIndex_GetPickupDisplayPrefab;
        }

        private GameObject PickupIndex_GetPickupDisplayPrefab(On.RoR2.PickupIndex.orig_GetPickupDisplayPrefab orig, ref PickupIndex self)
        {
            PickupIndex dio = new PickupIndex(ItemIndex.ExtraLife);
            if(self != dio)
            {
                return orig.Invoke(ref self);
            }
            else
            {
                List<PickupIndex> tier3Items = Run.instance.availableTier3DropList;
                if (!tier3Items.Contains(self))
                {
                    return orig.Invoke(ref self);
                }


                int rng = random.Next(0, tier3Items.Count);
                if (tier3Items[rng] == dio)
                    rng++;

                ItemIndex rngItem = tier3Items[rng].itemIndex;

                GameObject gameObject = Resources.Load<GameObject>(ItemCatalog.GetItemDef(rngItem).pickupModelPath);
                return gameObject;
            }

        }

        private void Run_Update(On.RoR2.Run.orig_Update orig, Run self)
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("Spawn DIO");
                LocalUser localUser = LocalUserManager.GetFirstLocalUser();
                Transform transform = localUser.cachedBodyObject.transform;

                PickupIndex dio = new PickupIndex(ItemIndex.ExtraLife);
                PickupDropletController.CreatePickupDroplet(dio, transform.position, Vector3.up * 5f);
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                Debug.Log("Spawn Random");
                LocalUser localUser = LocalUserManager.GetFirstLocalUser();
                Transform transform = localUser.cachedBodyObject.transform;

                List<PickupIndex> tier3Items = Run.instance.availableTier3DropList;
                int rng = random.Next(0, tier3Items.Count);

                ItemIndex rngItem = tier3Items[rng].itemIndex;

                PickupIndex dio = new PickupIndex(rngItem);
                PickupDropletController.CreatePickupDroplet(dio, transform.position, Vector3.up * 5f);
            }
        }

        private void GenericPickupController_GrantItem(On.RoR2.GenericPickupController.orig_GrantItem orig, RoR2.GenericPickupController self, RoR2.CharacterBody body, RoR2.Inventory inventory)
        {
            orig.Invoke(self, body, inventory);

            //Debug.Log(self.pickupIndex);

            PickupIndex dio = new PickupIndex(ItemIndex.ExtraLife);

            if(self.pickupIndex == dio)
            {
                //List<PickupIndex> tier3Items = Run.instance.availableTier3DropList;

                //int rng = random.Next(0, tier3Items.Count);
                //if (tier3Items[rng] == dio)
                //    rng++;

                //ItemIndex rngItem = tier3Items[rng].itemIndex;
                //tier3Items[rng].itemIndex.ToString();
                //tier3Items[rng].GetPickupNameToken();

                string pickupName = self.GetComponentInChildren<PickupDisplay>().transform.GetChild(1).name.Replace("(Clone)", "");
                //Debug.Log(pickupName);


                ItemDef[] items = typeof(ItemCatalog).GetFieldValue<ItemDef[]>("itemDefs");
                ItemDef fakeItem = items[0];
                //Debug.Log(items.Length);
                //Debug.Log(items[0]);
                foreach (ItemDef item in items)
                {
                    if(item.pickupModelPath.Contains(pickupName))
                    {
                        Debug.Log("FOUND!!!!!!!!");
                        fakeItem = item;
                        break;
                    }
                }

                PickupIndex ogItem = new PickupIndex(fakeItem.itemIndex);
                //Debug.Log("----------");
                //Debug.Log(ogItem.GetPickupNameToken());
                //Debug.Log(Language.GetString(ogItem.GetPickupNameToken()));
                //Debug.Log(ogItem.itemIndex);
                //Debug.Log("----------");

                //PickupIndex ogItem = new PickupIndex(ItemIndex.Behemoth);

                //string rngItemName = Language.GetString(tier3Items[rng].GetPickupNameToken());
                //Color32 color = tier3Items[rng].GetPickupColor();
                //Color32 colorDark = tier3Items[rng].GetPickupColorDark();

                string rngItemName = Language.GetString(ogItem.GetPickupNameToken());
                Color32 color = ogItem.GetPickupColor();
                Color32 colorDark = ogItem.GetPickupColorDark();


                //<color=#307FFF>Lunar Coin</color><style=cEvent> Dropped</style>
                Chat.AddMessage(String.Format("<style=cEvent>You were expecting </style>{0} {1}", Util.GenerateColoredString(rngItemName, color), Util.GenerateColoredString("BUT IT WAS ME DIO", colorDark)));
            }
        }
    }
}
