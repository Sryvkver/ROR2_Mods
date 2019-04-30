using RoR2;
using RoR2.UI;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static Command_Artifact.Command_Artifact;

namespace Command_Artifact
{
    class CA_Manager : MonoBehaviour
    {
        private bool chestOpening = false;
        private bool chestOpeningS = false; //Started
        private bool chestOpeningE = false; //Ended

        private float tier1Rate = 80;
        private float tier2Rate = 19;
        private float tier3Rate = 1;

        public int selectedItem = 0;
        public float lastInput = 0;

        public ConfigStuff config;
        public System.Random random;

        public State currState = State.Idle;

        public void Awake()
        {
            //On.RoR2.InteractionDriver.FixedUpdate += InteractionDriver_FixedUpdate;
            //On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            Chat.AddMessage("Found me");
        }

        public void OnKill()
        {
            //On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin;
        }

        public int PurchaseInteraction_Receiver(PurchaseInteraction self, Interactor activator)
        {
            CharacterBody player = activator.GetComponent<CharacterBody>();
            if (!player)
                return -1;

            CharacterMaster characterMaster = this.gameObject.GetComponent<CharacterMaster>();
            if (!characterMaster)
                return -1;

            if (player.gameObject == characterMaster.GetBody().gameObject)
            {
                //Debug.Log("Same!");
                if (currState != State.Idle)
                    return 0;

                string objName = self.gameObject.name.ToLower();
                Debug.Log(objName);

                if (objName.Contains("chest1"))
                {
                    //Debug.Log("Default Chest");
                    //Default Chest
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Normal);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Normal);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Normal);
                    chestOpening = true;
                    currState = State.Opening;
                }
                else if (objName.Contains("chest2"))
                {
                    //Debug.Log("Large Chest");
                    //Large Chest
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Large);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Large);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Large);
                    chestOpening = true;
                    currState = State.Opening;
                }
                else if (objName.Contains("goldchest"))
                {
                    //Debug.Log("Golden Chest");
                    //Golden Chest
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Golden);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Golden);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Golden);
                    chestOpening = true;
                    currState = State.Opening;
                }
                else if (objName.Contains("isclockbox"))
                {
                    //Debug.Log("Rusty Chest");
                    //Rusty Chest
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Rusty);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Rusty);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Rusty);
                    chestOpening = true;
                    currState = State.Opening;
                }
                else if (objName.Contains("equipmentbarrel"))
                {
                    //Debug.Log("Equipment Barrel");
                    //Equipment Barrel

                    chestOpening = true;
                    currState = State.Equipment;
                }
                return -1;
            }
            return 2;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (currState != State.Idle)
                return;

            orig.Invoke(self, activator);

            CharacterBody player = activator.GetComponent<CharacterBody>();
            if (!player)
                return;

            CharacterMaster characterMaster = this.gameObject.GetComponent<CharacterMaster>();
            if (!characterMaster)
                return;

            if (player.gameObject == characterMaster.GetBody().gameObject)
            {
                //Debug.Log("Same!");

                string objName = self.gameObject.name.ToLower();
                Debug.Log(objName);

                if (objName.Contains("chest1"))
                {
                    //Debug.Log("Default Chest");
                    //Default Chest
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Normal);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Normal);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Normal);
                    chestOpening = true;
                    currState = State.Opening;
                }
                else if (objName.Contains("chest2"))
                {
                    //Debug.Log("Large Chest");
                    //Large Chest
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Large);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Large);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Large);
                    chestOpening = true;
                    currState = State.Opening;
                }
                else if (objName.Contains("goldchest"))
                {
                    //Debug.Log("Golden Chest");
                    //Golden Chest
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Golden);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Golden);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Golden);
                    chestOpening = true;
                    currState = State.Opening;
                }
                else if (objName.Contains("isclockbox"))
                {
                    //Debug.Log("Rusty Chest");
                    //Rusty Chest
                    tier1Rate = config.GetValue(1, ConfigStuff.ChestType.Rusty);
                    tier2Rate = config.GetValue(2, ConfigStuff.ChestType.Rusty);
                    tier3Rate = config.GetValue(3, ConfigStuff.ChestType.Rusty);
                    chestOpening = true;
                    currState = State.Opening;
                }
                else if (objName.Contains("equipmentbarrel"))
                {
                    //Debug.Log("Equipment Barrel");
                    //Equipment Barrel

                    chestOpening = true;
                    currState = State.Equipment;
                }
            }
        }

        public void Update()
        {
            if (chestOpening)
            {
                if (!chestOpeningS)
                {
                    SetGlobalTimeScale(config.TimeScale);

                    Notification notification = this.GetComponent<Notification>();

                    switch (currState)
                    {
                        case State.Idle:
                            break;
                        case State.Opening:
                            float[] tierRates = new float[] { tier1Rate, tier2Rate, tier3Rate };

                            PopulateSelectMenu(notification, random, tierRates, config.AllAvaiable);
                            break;
                        case State.Equipment:
                            notification.PopulateEquipment(config.AllAvaiable);
                            break;
                        default:
                            break;
                    }

                    ShowSelectMenu();

                    chestOpeningS = true;
                }

                //Check if a hotkey was pressed
                //TODO add mouse input
                ProcessInput();

                if (chestOpeningE)
                {
                    Notification notification = this.GetComponent<Notification>();
                    notification.Clear();
                    //Hide Menu and Reset all values
                    HideSelectMenu();

                    chestOpening = false;
                    chestOpeningS = false;
                    chestOpeningE = false;
                    selectedItem = 0;

                    currState = State.Idle;
                    if (CheckAllStates())
                        SetGlobalTimeScale(config.TimeScaleDefault);

                }
            }
        }

        public void SetTimeScale(float newTimeScale)
        {
            Time.timeScale = newTimeScale;
        }

        public void ShowSelectMenu()
        {
            Notification notification = this.GetComponent<Notification>();
            if (notification)
                notification.RootObject.SetActive(true);
        }

        public void HideSelectMenu()
        {
            Notification notification = this.GetComponent<Notification>();
            if (notification)
                notification.RootObject.SetActive(false);
        }

        //returns true if all are Idle
        private bool CheckAllStates()
        {
            CA_Manager[] allManagers = FindObjectsOfType<CA_Manager>();

            for (int i = 0; i < allManagers.Length; i++)
            {
                if (allManagers[i].currState != State.Idle)
                    return false;
            }
            return true;
        }

        private void ProcessInput()
        {
            Notification notification = this.GetComponent<Notification>();
            if (!notification)
                return;

            float inputDelay = (config.TimeScale / 0.1f * .015f);

            if (Input.GetKey(config.LeftButton) && Time.time > lastInput + inputDelay)
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
            else if (Input.GetKey(config.RightButton) && Time.time > lastInput + inputDelay)
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
            else if (Input.GetKey(config.UpButton) && Time.time > lastInput + inputDelay)
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
            else if (Input.GetKey(config.DownButton) && Time.time > lastInput + inputDelay)
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
            //Debug.Log(selectedItem);

            if (Input.GetKeyDown(config.SelectButton))
            {
                chestOpeningE = true;
                ItemDef item;
                EquipmentDef equipment;
                PickupIndex pickupIndex;
                switch (currState)
                {
                    case State.Idle:
                        break;
                    case State.Opening:
                        item = notification.iconsCA[selectedItem].ItemDef;
                        //Debug.Log(item.itemIndex.ToString());
                        pickupIndex = new PickupIndex(item.itemIndex);
                        DropItems(pickupIndex);
                        break;
                    case State.Equipment:
                        equipment = notification.iconsCA[selectedItem].EquipmentDef;
                        //Debug.Log(equipment.equipmentIndex.ToString());
                        pickupIndex = new PickupIndex(equipment.equipmentIndex);
                        DropItems(pickupIndex);
                        break;
                    default:
                        break;
                }
            }
        }

        public enum State
        {
            Idle,
            Opening,
            Equipment
        }

        /*Might be useful later
        private void InteractionDriver_FixedUpdate(On.RoR2.InteractionDriver.orig_FixedUpdate orig, RoR2.InteractionDriver self)
        {
            orig.Invoke(self);

            bool inputReceived = false;
            try
            {
                inputReceived = self.GetFieldValue<bool>("inputReceived");
            }
            catch (Exception)
            {
            }

            if (!inputReceived)
                return;

            Debug.Log("Input Received!");

            CharacterMaster characterMaster = this.gameObject.GetComponent<CharacterMaster>();
            if (!characterMaster)
                return;

            //Debug.Log("Self: " + self.gameObject.name);
            //Debug.Log("This: " + characterMaster.GetBody().gameObject.name);

            if (self.gameObject == characterMaster.GetBody().gameObject)
            {
                Debug.Log("Same!");

                GameObject gameObject = self.FindBestInteractableObject();
                if (gameObject)
                {
                    Debug.Log(gameObject.name);
                    if (gameObject.name.ToLower().Contains("chest"))
                    {
                        Debug.Log("Okay");
                    }
                }

            }
        }
        */
    }
}
