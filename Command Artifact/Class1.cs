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

namespace Command_Artifact
{
    [BepInPlugin("dev.felixire.Command_Artifact", "Command_Artifact", "1.1.1")]
    class Command_Artifact : BaseUnityPlugin
    {
        bool _DEBUG = false;
        bool init = false;
        System.Random random;

        bool chestOpening = false;
        bool chestOpeningS = false; //Started
        bool chestOpeningE = false; //Ended
        Vector3 chestPos = Vector3.zero;
        Transform chestTransform = null;

        float oldTimeScale = 1f;
        CharacterBody characterBody = null;
        Notification notification = null;

        int tier1Rate = 80;
        int tier2Rate = 19;
        int tier3Rate = 1;

        int selectedItem = 0;
        float lastInput = 0;

        public void Awake()
        {
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
                notification = characterBody.gameObject.AddComponent<Notification>();
                notification.transform.SetParent(characterBody.gameObject.transform);
                notification.SetPosition(new Vector3((float)(Screen.width * 50) / 100f, (float)(Screen.height * 50) / 100f, 0f));
                notification.SetSize(new Vector2(500, 250));

                notification.GetTitle = (() => "Item Selector. Press F to confirm");
                //notification.GetDescription = (() => "Test");

                notification.GenericNotification.fadeTime = 1f;
                notification.GenericNotification.duration = 10000f;
                HideSelectMenu();

            }

            if (characterBody == null && notification != null)
            {
                Destroy(notification);
                notification = null;
            }

            if (chestOpening)
            {
                if (!chestOpeningS)
                {
                    float oldTimeScale = Time.timeScale;
                    Time.timeScale = 0.1f;
                    ShowSelectMenu();

                    chestOpeningS = true;
                }

                //Check if something was Selected
                ProcessInput();

                if (chestOpeningE)
                {
                    HideSelectMenu();
                    Time.timeScale = oldTimeScale;

                    chestOpening = false;
                    chestOpeningS = false;
                    chestOpeningE = false;
                    selectedItem = 0;

                    tier1Rate = 80;
                    tier2Rate = 19;
                    tier3Rate = 1;
                }
            }
        }

        private void ProcessInput()
        {
            if (Input.GetKey(KeyCode.LeftArrow) && Time.time > lastInput + .015f)
            {
                --selectedItem;
                if (selectedItem < 0)
                    selectedItem = notification.iconsCA.Count - 1;
                if (selectedItem > notification.iconsCA.Count - 1)
                    selectedItem = 0;
                notification.setSelectorPos(selectedItem);
                //Chat.AddMessage(selectedItem.ToString());
                lastInput = Time.time;
            }
            else if (Input.GetKey(KeyCode.RightArrow) && Time.time > lastInput + .015f)
            {
                ++selectedItem;
                if (selectedItem < 0)
                    selectedItem = notification.iconsCA.Count - 1;
                if (selectedItem > notification.iconsCA.Count - 1)
                    selectedItem = 0;
                notification.setSelectorPos(selectedItem);
                //Chat.AddMessage(selectedItem.ToString());
                lastInput = Time.time;
            }
            else if (Input.GetKey(KeyCode.UpArrow) && Time.time > lastInput + .015f)
            {
                selectedItem -= notification.ItemsInLine;
                //int offset = -selectedItem;
                if (selectedItem < 0)
                    selectedItem = notification.iconsCA.Count - 1;
                if (selectedItem > notification.iconsCA.Count - 1)
                    selectedItem = 0;
                notification.setSelectorPos(selectedItem);
                //Chat.AddMessage(selectedItem.ToString());
                lastInput = Time.time;
            }
            else if (Input.GetKey(KeyCode.DownArrow) && Time.time > lastInput + .015f)
            {
                selectedItem += notification.ItemsInLine;
                //int offset = selectedItem;
                if (selectedItem < 0)
                    selectedItem = notification.iconsCA.Count - 1;
                if (selectedItem > notification.iconsCA.Count - 1)
                    selectedItem = 0;
                notification.setSelectorPos(selectedItem);
                //Chat.AddMessage(selectedItem.ToString());
                lastInput = Time.time;
            }
            Debug.Log(selectedItem);


            if (Input.GetKeyDown(KeyCode.F))
            {
                chestOpeningE = true;
                ItemDef item = notification.iconsCA[selectedItem].ItemDef;
                PickupIndex pickupIndex = new PickupIndex(item.itemIndex);
                //localUser.cachedBody.inventory.GiveItem(item.itemIndex);
                PickupDropletController.CreatePickupDroplet(pickupIndex, chestPos + Vector3.up * 1.5f, Vector3.up * 20f + chestTransform.forward * 2f);
            }
        }

        private void ShowSelectMenu()
        {
            notification.Clear();

            int rng = random.Next(1, 101);
            //Chat.AddMessage(rng.ToString() + " - " + tier1Rate.ToString());
            //Chat.AddMessage((rng < tier1Rate).ToString());
            if (rng < tier1Rate)
            {
                notification.PopulateTier1();
            }
            else if (rng < tier1Rate + tier2Rate)
            {
                notification.PopulateTier2();
            }
            else
            {
                notification.PopulateTier3();
            }
            
            notification.RootObject.SetActive(true);
        }
        private void HideSelectMenu()
        {
            notification.RootObject.SetActive(false);
        }

        private void Opening_OnEnter(On.EntityStates.Barrel.Opening.orig_OnEnter orig, EntityStates.Barrel.Opening self)
        {
            //Chat.AddMessage(self.gameObject.name);
            string name = self.outer.gameObject.name;
            chestPos = self.outer.gameObject.transform.position;
            chestTransform = self.outer.gameObject.transform;

            //string name = self.characterBody.baseNameToken;
            //Chat.AddMessage(name + " - " + chestPos);
            if (name.ToLower().Contains("chest"))
            {
                if (name.ToLower().Contains("chest2")) //If large chest increase Odds of better Tier
                {
                    tier1Rate = 60;
                    tier2Rate = 35;
                    tier3Rate = 5;
                }
                else if (name.ToLower().Contains("goldchest")) //If golden chest make it 100% red item
                {
                    tier1Rate = 0;
                    tier2Rate = 0;
                    tier3Rate = 100;
                }
                chestOpening = true;

                if (_DEBUG)
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
            else
            {
                orig.Invoke(self);
            }
            //chestOpening = true;

            //throw new System.NotImplementedException();
        }
    }
}
