using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using BepInEx.Configuration;
using System.Collections;
using System.Linq;
using System.IO;
using EntityStates;
using System.Collections.Generic;

namespace Command_Artifact
{
    [BepInPlugin("dev.felixire.Command_Artifact", "Command_Artifact", "1.2.2")]
    class Command_Artifact : BaseUnityPlugin
    {
        ConfigStuff config = new ConfigStuff();

        bool _DEBUG = false;
        bool init = false;
        System.Random random;

        bool chestOpening = false;
        bool chestOpeningS = false; //Started
        bool chestOpeningE = false; //Ended

        List<GameObject> opendChests = new List<GameObject>();
        static Vector3 chestPos = Vector3.zero;
        static Vector3 chestForward = Vector3.zero;
        //Transform chestTransform = null;

        float oldTimeScale = 1f;
        CharacterBody characterBody = null;
        Notification notification = null;

        public void Awake()
        {
            config.Init(Config);

            var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            int seed = (int)((System.DateTime.UtcNow - epochStart).TotalSeconds / 2);
            Debug.Log(seed);
            random = new System.Random(seed);
            On.EntityStates.Barrel.Opening.OnEnter += Opening_OnEnter;
        }

        public void Update()
        {
            if (!Run.instance || Run.instance.time < 1)
                return;

            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser == null || localUser.cachedBody == null)
                return;

            characterBody = localUser.cachedBody;

            if (notification == null)
            {
                //Check if message was already said
                if (!init)
                {
                    Chat.AddMessage("<color=blue>Command Artifact Loaded</color>");
                    init = true;
                }

                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                {
                    if (player.gameObject.GetComponent<CA_Manager>() == null)
                    {
                        CA_Manager manager = player.gameObject.AddComponent<CA_Manager>();
                        manager.config = this.config;
                        manager.random = this.random;
                    }

                    if (player.gameObject.GetComponent<Notification>() == null)
                    {
                        if (player.gameObject == localUser.cachedMasterController.gameObject)
                            notification = player.gameObject.AddComponent<Notification>();

                        notification.transform.SetParent(player.gameObject.transform);
                        notification.SetPosition(new Vector3((float)(Screen.width * 50) / 100f, (float)(Screen.height * 50) / 100f, 0f));
                        notification.SetSize(new Vector2(500, 250));

                        notification.GetTitle = (() => string.Format("Item Selector. Press {0} to continue", config.SelectButton.ToString()));

                        notification.GenericNotification.fadeTime = 1f;
                        notification.GenericNotification.duration = 10000f;
                        player.gameObject.GetComponent<CA_Manager>().HideSelectMenu();
                        opendChests.Clear();
                    }
                }
            }

            if (characterBody == null && notification != null)
            {
                Destroy(notification);
                notification = null;
            }
        }

        public static void SetGlobalTimeScale(float timeScale)
        {
            CA_Manager[] allManagers = FindObjectsOfType<CA_Manager>();

            for (int i = 0; i < allManagers.Length; i++)
            {
                allManagers[i].SetTimeScale(timeScale);
            }
        }

        public static void PopulateSelectMenu(Notification notification, System.Random random, float[] tierRates)
        {
            notification.Clear();

            //int rng = random.Next(0, 101);
            double rng = (random.NextDouble() * 100);
            //Chat.AddMessage(rng.ToString() + " - " + tier1Rate.ToString());
            //Chat.AddMessage((rng < tier1Rate).ToString());
            if (rng < tierRates[0])
            {
                notification.PopulateTier(ItemTier.Tier1);
            }
            else if (rng < tierRates[0] + tierRates[1])
            {
                notification.PopulateTier(ItemTier.Tier2);
            }
            else if (rng < tierRates[0] + tierRates[1] + tierRates[2])
            {
                notification.PopulateTier(ItemTier.Tier3);
            }
        }

        private void LockChests(bool shouldLock)
        {
            //get all Object with a PurchaseInteraction (Shrine, 3D Printer, Chests)
            PurchaseInteraction[] purchasables = UnityEngine.Object.FindObjectsOfType<PurchaseInteraction>();
            for (int i = 0; i < purchasables.Length; i++)
            {
                if (purchasables[i].gameObject.name.ToLower().Contains("chest"))
                {
                    PurchaseInteraction chest = purchasables[i];
                    //Debug.Log("Null Check: " + (chestOBJ != null).ToString());
                    //Debug.Log("Same Check: " + (chest.gameObject == chestOBJ).ToString());
                    if (opendChests.IndexOf(chest.gameObject) > -1)
                        chest.SetAvailable(false);
                    else
                        chest.SetAvailable(!shouldLock);
                }
            }
        }

        private void Opening_OnEnter(On.EntityStates.Barrel.Opening.orig_OnEnter orig, EntityStates.Barrel.Opening self)
        {
            GameObject obj = self.outer.gameObject;
            string name = obj.name.ToLower();

            opendChests.Add(obj);
            chestPos = obj.transform.position;
            chestForward = obj.transform.forward;

            if(!name.Contains("chest1") && !name.Contains("chest2") && !name.Contains("goldchest"))
            {
                orig.Invoke(self);
                return;
            }
        }

        /*
        private void Opening_OnEnter(On.EntityStates.Barrel.Opening.orig_OnEnter orig, EntityStates.Barrel.Opening self)
        {
            //Chat.AddMessage(self.gameObject.name);
            string name = self.outer.gameObject.name;

            opendChests.Add(self.outer.gameObject);
            //Set Position
            chestPos = self.outer.gameObject.transform.position;
            Transform chestTransform = self.outer.gameObject.transform;
            //Should make it compatible with emptyChestBeGone Mod
            chestForward = new Vector3(chestTransform.forward.x, chestTransform.forward.y, chestTransform.forward.z);

            if (name.ToLower().Contains("chest"))
            {
                if (name.ToLower().Contains("chest1")) //If Normal chest use Normal odds
                {
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Normal);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Normal);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Normal);
                }else if (name.ToLower().Contains("chest2")) //If large chest increase Odds of better Tier
                {
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Large);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Large);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Large);
                }
                else if (name.ToLower().Contains("goldchest")) //If golden chest make it 100% red item
                {
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Golden);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Golden);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Golden);
                }
                else
                {
                    orig.Invoke(self);
                    chestOpening = false;
                    return;
                }

                //Broken shit
                if (_DEBUG)
                {
                    RunChestAnimation(self);
                }
            }
            else
            {
                orig.Invoke(self);
            }
            //chestOpening = true;

            //throw new System.NotImplementedException();
        }
        */
        public static void DropItems(ItemIndex item)
        {
            PickupIndex pickupIndex = new PickupIndex(item);
            //localUser.cachedBody.inventory.GiveItem(item.itemIndex);
            PickupDropletController.CreatePickupDroplet(pickupIndex, chestPos + Vector3.up * 1.5f, Vector3.up * 20f + chestForward * 2f);
            //PickupDropletController.CreatePickupDroplet(pickupIndex, Vector3.zero, Vector3.zero);
        }

        public void RunChestAnimation(EntityStates.Barrel.Opening self)
        {
            EntityState chestState = self.outer.GetComponent<EntityState>();

            ModelLocator modelLocator = self.outer.GetComponent<ModelLocator>();

            //protected void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration)
            //"Body", "Opening", "Opening.playbackRate", Opening.duration

            //Get Animator of the chest
            Animator modelAnimator = modelLocator.modelTransform.GetComponent<Animator>();
            if (modelAnimator)
            {
                //Play Animation
                int layerIndex = modelAnimator.GetLayerIndex("Body");
                modelAnimator.SetFloat("Opening.playbackRate", 1f);
                modelAnimator.PlayInFixedTime("Opening", layerIndex, 0f);
                modelAnimator.Update(0f);
                float length = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).length;
                modelAnimator.SetFloat("Opening.playbackRate", length / 1f);
            }

            if (chestState.sfxLocator)
            {
                Util.PlaySound(chestState.sfxLocator.openSound, base.gameObject);
            }
        }
    }
}
