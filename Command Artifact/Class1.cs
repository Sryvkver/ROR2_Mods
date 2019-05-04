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
using UnityEngine.Networking;

namespace Command_Artifact
{
    /*
    OMG THIS SHIT DOESNT MAKE ANY SENSE ANYMORE
    I LITTERALY CANT FIND ANYTHING ANYMORE AHHHH
    [TODO]
    [ ] Fix Multiplayer - litteraly everyone
        -- maybe just make it so everyone needs the mod and runs the code themselves (this breaks timescale but meh)
        -- This fixes the issue of fucking trying to share a variable between everyone which is litteraly impossible to do for me
    [X] Add config for moving the selector - Kathlyn <-- Please check the name, I dont remember...
    [X] Fix bug where Command Artifact breaks after game restart - ...
    [X] Fix bug where Item spawns wherever the last thing was opend (Money Barrel) - Jessica <-- I think...
        -- Possible fix - Create empty gameobject in CA_Manager to save the location temporary, and delete this once the selection key was pressed!
        -- Didnt do the fix above. mhh
    */

    [BepInPlugin("dev.felixire.Command_Artifact", "Command_Artifact", "1.3.0")]
    class Command_Artifact : BaseUnityPlugin
    {
        ConfigStuff config = new ConfigStuff();

        bool _DEBUG = false;
        bool init = false;
        System.Random random;

        CharacterBody characterBody = null;
        Notification masterNotification = null;

        public void Awake()
        {
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

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            CA_Manager[] allManagers = FindObjectsOfType<CA_Manager>();

            for (int i = 0; i < allManagers.Length; i++)
            {
                //-1 = Error; 0 = Same but not idling; 1 = Same and Idling; 2 = Not same; 3 = Not a chest
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
            }
        }

        private void Run_Awake(On.RoR2.Run.orig_Awake orig, Run self)
        {
            CleanUpLeftovers();
            orig.Invoke(self);


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

            //Check if every player has an CA_Manager and check master
            if (masterNotification == null)
            {
                PlayerCharacterMasterController player = PlayerCharacterMasterController.instances[0];
                if (player.gameObject.GetComponent<CA_Manager>() == null)
                {
                    CA_Manager manager = player.gameObject.AddComponent<CA_Manager>();
                    manager.config = this.config;
                    manager.random = this.random;
                }

                if (player.gameObject.GetComponent<Notification>() == null)
                {
                    Notification notification;
                    masterNotification = notification = player.gameObject.AddComponent<Notification>();

                    notification.transform.SetParent(player.gameObject.transform);
                    notification.SetPosition(new Vector3((float)(Screen.width * 50) / 100f, (float)(Screen.height * 50) / 100f, 0f));
                    notification.SetSize(new Vector2(500, 250));

                    notification.GetTitle = (() => string.Format("Item Selector. Press {0} to continue", config.SelectButton.ToString()));

                    notification.GenericNotification.fadeTime = 1f;
                    notification.GenericNotification.duration = 10000f;
                    player.gameObject.GetComponent<CA_Manager>().HideSelectMenu();
                }
            }

            //If player was destroyed, remove all leftovers
            if (characterBody == null && masterNotification != null)
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

                //Destroy(masterNotification);
                masterNotification = null;
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

            if (!name.Contains("chest1") && !name.Contains("chest2") && !name.Contains("goldchest") && !name.Contains("equipmentbarrel") && !name.Contains("isclockbox"))
            {
                orig.Invoke(self);
                return;
            }
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
