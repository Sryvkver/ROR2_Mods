using BepInEx;
using RoR2;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using BepInEx.Configuration;
using System.Collections;
using System.Linq;
using System.IO;
using EntityStates;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Reflection;
using System;
using static RoR2.Chat;

namespace Command_Artifact
{
    /*
        OMG THIS SHIT DOESNT MAKE ANY SENSE ANYMORE
        I LITTERALY CANT FIND ANYTHING ANYMORE AHHHH

        [TODO]
        [ ] Fix Multiplayer - litteraly everyone
        [X] Add config for moving the selector - Kathlyn <-- Please check the name, I dont remember...
        [X] Fix bug where Command Artifact breaks after game restart - ...
        [ ] Fix bug where Item spawns wherever the last thing was opend (Money Barrel) - Jessica <-- I think...
            -- Possible fix - Create empty gameobject in CA_Manager to save the location temporary, and delete this once the selection key was pressed!
        [ ] Fix all class names, because they dont make any sense and are just confusing... - felixire


    */
    [BepInPlugin("dev.felixire.Command_Artifact", "Command_Artifact", "1.3.0")]
    class Command_Artifact : BaseUnityPlugin
    {
        NetworkHash128 TimeScalerID;

        ConfigStuff config = new ConfigStuff();

        bool _DEBUG = false;
        bool init = false;
        System.Random random;

        static Vector3 chestPos = Vector3.zero;
        static Vector3 chestForward = Vector3.zero;
        //Transform chestTransform = null;

        CharacterBody characterBody = null;
        Notification masterNotification = null;
        public void Awake()
        {
            TimeScalerID = NetworkHash128.Parse("e2656f");
            ClientScene.RegisterSpawnHandler(TimeScalerID, SpawnTimeScaler, UnSpawnTimeScaler);

            config.Init(Config);

            //Generating random seed based on unix timestamp
            var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            int seed = (int)((System.DateTime.UtcNow - epochStart).TotalSeconds / 2);
            Debug.Log("Seed: " + seed);
            random = new System.Random(seed);

            //Hook needed stuff
            On.EntityStates.Barrel.Opening.OnEnter += Opening_OnEnter;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.Run.Update += Run_Update;
            On.RoR2.Run.Awake += Run_Awake;
        }

        public delegate GameObject SpawnTimeScalerDelegate(Vector3 position, NetworkHash128 assetId);
        public delegate void UnSpawnTimeScalerDelegate(GameObject spawned);

        public GameObject SpawnTimeScaler(Vector3 position, NetworkHash128 assetId)
        {
            GameObject timescaler;
            timescaler = new GameObject("TimeScaler");
            timescaler.AddComponent<CA_TimeScaler>();
            timescaler.AddComponent<NetworkIdentity>();
            return timescaler;
        }

        public void UnSpawnTimeScaler(GameObject spawned)
        {
            Debug.Log("Re-pooling GameObject " + spawned.name);
            spawned.SetActive(false);
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            //0 = Call Orig; anything else = DO NOT CALL ORIG
            int check = FindObjectOfType<CA_TimeScaler>().PurchaseInteraction_Stuff(self, activator);

            Chat.AddMessage("CA_TimeScaler Check: " + check);
            Chat.SendBroadcastChat(new SimpleChatMessage { baseToken = "<color=#e5eefc>CA_TimeScaler Check: {0}</color>", paramTokens = new[] { check.ToString() } });

            if (check == 0)
            {
                orig.Invoke(self, activator);
            }

            /*CA_Manager[] allManagers = FindObjectsOfType<CA_Manager>();

            for (int i = 0; i < allManagers.Length; i++)
            {
                //-1 = Error; 0 = Same but not idling; 1 = Same and Idling 2 = Not same; 3 = not a chest
                int check = allManagers[i].PurchaseInteraction_Receiver(self, activator);
                Chat.AddMessage("Check: " + check);
                if (check == 1 || check == 3)
                {
                    orig.Invoke(self, activator);
                    break;
                }
                else if (check == 0)
                {
                    break;
                }
            }*/
        }

        private void Run_Awake(On.RoR2.Run.orig_Awake orig, Run self)
        {
            orig.Invoke(self);

            CleanUpLeftovers();

            //SetGlobalTimeScaleV2(config.TimeScaleDefault);
            Chat.AddMessage("<color=blue>Command Artifact Loaded</color>");
            init = false;
        }

        private void CleanUpLeftovers()
        {
            //Hopefully this runs only once
            //Try to remove leftovers
            try
            {
                CA_Manager[] allManagers = FindObjectsOfType<CA_Manager>();

                for (int i = 0; i < allManagers.Length; i++)
                {
                    Destroy(allManagers[i]);
                }

                Notification[] allSelectors = FindObjectsOfType<Notification>();

                for (int i = 0; i < allSelectors.Length; i++)
                {
                    Destroy(allSelectors[i]);
                }

                Destroy(masterNotification);
                Destroy(characterBody);
            }
            catch (System.Exception)
            {
            }

            masterNotification = null;
            characterBody = null;
        }

        private void SetupPlayer()
        {
            //PlayerCharacterMasterController player = LocalUserManager.GetFirstLocalUser().cachedMasterController;
            PlayerCharacterMasterController player = PlayerCharacterMasterController.instances[0];
            if (!RoR2Application.isInSinglePlayer && !player.isServer)
            {
                Chat.AddMessage("Not Host!");
                On.EntityStates.Barrel.Opening.OnEnter -= Opening_OnEnter;
                On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin;
                //On.RoR2.Run.Update -= Run_Update;
            }
            else
            {
                Chat.AddMessage("Host!");
                GameObject timeScaler = SpawnTimeScaler(Vector3.zero, TimeScalerID);
                NetworkServer.Spawn(timeScaler, TimeScalerID);
                //timeScaler.GetComponent<CA_TimeScaler>().SetTimeScale(config.TimeScaleDefault);
                //timeScaler.GetComponent<CA_TimeScaler>().SetupPlayers(config, random);
            }
            //FindObjectOfType<CA_TimeScaler>().SetupPlayers(config, random);
            FindObjectOfType<CA_TimeScaler>().SetupPlayer(config, random, player);

            Chat.SendBroadcastChat(new SimpleChatMessage { baseToken = "<color=#e5eefc>{0}: {1}</color>", paramTokens = new[] { "Amount of Managers: ", FindObjectOfType<CA_TimeScaler>().GetAmountOfManagers().ToString() } });

            SetGlobalTimeScale(config.TimeScaleDefault);
        }

        private void Run_Update(On.RoR2.Run.orig_Update orig, Run self)
        {
            orig.Invoke(self);

            //Debug.Log(PlayerCharacterMasterController.instances.Count != FindObjectsOfType<CA_Manager>().Length);
            //Debug.Log(string.Format("Player count: {0}; Manager Count: {1}", PlayerCharacterMasterController.instances.Count, FindObjectsOfType<CA_Manager>().Length));


            if (!Run.instance || Run.instance.time < 1)
                return;

            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser == null || localUser.cachedBody == null)
                return;

            characterBody = localUser.cachedBody;

            if (!init)
            {
                
                SetupPlayer();
                init = true;
            }

            //Check if every player has an CA_Manager and check master
            #region Not needed right now
            /* M
            if (masterNotification == null || PlayerCharacterMasterController.instances.Count != FindObjectsOfType<CA_Manager>().Length)
            {
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
                        Notification notification;
                        if (player.gameObject == localUser.cachedMasterController.gameObject)
                            masterNotification = notification = player.gameObject.AddComponent<Notification>();
                        else
                            notification = player.gameObject.AddComponent<Notification>();

                        notification.transform.SetParent(player.gameObject.transform);
                        notification.SetPosition(new Vector3((float)(Screen.width * 50) / 100f, (float)(Screen.height * 50) / 100f, 0f));
                        notification.SetSize(new Vector2(500, 250));

                        notification.GetTitle = (() => string.Format("Item Selector. Press {0} to continue", config.SelectButton.ToString()));

                        notification.GenericNotification.fadeTime = 1f;
                        notification.GenericNotification.duration = 10000f;
                        player.gameObject.GetComponent<CA_Manager>().HideSelectMenu();
                    }
                }
            }
            */
            #endregion

            //If player was destroyed, remove all leftovers
            if (characterBody == null )
            {
                //FindObjectOfType<CA_TimeScaler>().RemoveLeftovers();

                Debug.Log("Mhhh");
            }
        }

        public static void SetGlobalTimeScale(float timeScale)
        {
            CA_Manager[] allManagers = FindObjectsOfType<CA_Manager>();

            for (int i = 0; i < allManagers.Length; i++)
            {
                allManagers[i].SetTimeScale(timeScale);
            }

            FindObjectOfType<CA_TimeScaler>().SetTimeScale(timeScale);
        }

        public static void PopulateSelectMenu(Notification notification, System.Random random, float[] tierRates, bool allAvaiable)
        {
            notification.Clear();

            //int rng = random.Next(0, 101);
            double rng = (random.NextDouble() * 100);
            //Chat.AddMessage(rng.ToString() + " - " + tier1Rate.ToString());
            //Chat.AddMessage((rng < tier1Rate).ToString());
            if (rng < tierRates[0])
            {
                notification.PopulateTier(ItemTier.Tier1, allAvaiable);
            }
            else if (rng < tierRates[0] + tierRates[1])
            {
                notification.PopulateTier(ItemTier.Tier2, allAvaiable);
            }
            else if (rng < tierRates[0] + tierRates[1] + tierRates[2])
            {
                notification.PopulateTier(ItemTier.Tier3, allAvaiable);
            }
        }

        private void Opening_OnEnter(On.EntityStates.Barrel.Opening.orig_OnEnter orig, EntityStates.Barrel.Opening self)
        {
            GameObject obj = self.outer.gameObject;
            string name = obj.name.ToLower();

            chestPos = obj.transform.position;
            chestForward = obj.transform.forward;

            if (!name.Contains("chest1") && !name.Contains("chest2") && !name.Contains("goldchest") && !name.Contains("equipmentbarrel") && !name.Contains("isclockbox"))
            {
                orig.Invoke(self);
                return;
            }
        }

        public static void DropItems(PickupIndex item)
        {
            //PickupIndex pickupIndex = new PickupIndex(item);
            //localUser.cachedBody.inventory.GiveItem(item.itemIndex);
            PickupDropletController.CreatePickupDroplet(item, chestPos + Vector3.up * 1.5f, Vector3.up * 20f + chestForward * 2f);
            //PickupDropletController.CreatePickupDroplet(pickupIndex, Vector3.zero, Vector3.zero);
        }

        //Still Broken as hell
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
