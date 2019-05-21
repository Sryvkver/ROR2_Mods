using BepInEx;
using MiniRpcLib;
using MiniRpcLib.Action;
using MiniRpcLib.Func;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;
using EntityStates.Barrel;

namespace Command_Artifact_V2
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency(MiniRpcPlugin.Dependency)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class Main : BaseUnityPlugin
    {
        private const string ModVer = "1.0.0";
        private const string ModName = "Command Artifact";
        private const string ModGuid = "dev.felixire.CommandArtifact";

        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom { get; set; }
        public IRpcAction<Action<NetworkWriter>> ExampleCommandClientCustom { get; set; }
        public IRpcFunc<GameObject, int> ExampleFuncClient { get; set; }

        private PlayerCharacterMasterController playerCharacterMaster = null;
        System.Random random;

        private void Awake()
        {
            On.RoR2.RoR2Application.UnitySystemConsoleRedirector.Redirect += orig => { };
            var miniRpc = MiniRpc.CreateInstance(ModGuid);
            RegisterMiniRpcCMDs(miniRpc);

            var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            int seed = (int)((System.DateTime.UtcNow - epochStart).TotalSeconds / 2);
            Debug.Log("Seed: " + seed);
            random = new System.Random(seed);

            On.RoR2.Run.Awake += Run_Awake;
        }

        #region Hooks
        private void Run_Awake(On.RoR2.Run.orig_Awake orig, RoR2.Run self)
        {
            orig.Invoke(self);

            RoR2.Chat.AddMessage("<color=blue>Command Artifact Loaded</color>");

            //Unhook non host related stuff
            //Had issues when everyone hooked them ...
            On.EntityStates.Barrel.Opening.OnEnter -= Opening_OnEnter;
            On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin;
            //Makes sure the Update Function is hooked at the start!
            On.RoR2.Run.Update += Run_Update;
        }

        private void Run_Update(On.RoR2.Run.orig_Update orig, RoR2.Run self)
        {
            orig.Invoke(self);

            //Wait till all players loaded
            //Dont know if there is a better option for this
            if (RoR2.Run.instance.time < 5)
                return;

            //Compare this user with the host user
            bool isHost = LocalUserManager.GetFirstLocalUser().cachedMasterController == PlayerCharacterMasterController.instances[0];

            //If Not host unhook the update function and quit this function
            if (!isHost)
            {
                On.RoR2.Run.Update -= Run_Update;
                return;
            }

            //If Host send a Reset, so everything is setup correctly
            ExampleCommandClientCustom.Invoke(x =>
            {
                x.Write("Reset");
            });

            //Hook host specific stuff
            On.EntityStates.Barrel.Opening.OnEnter += Opening_OnEnter;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            //Unhook the Update function
            On.RoR2.Run.Update -= Run_Update;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, RoR2.PurchaseInteraction self, RoR2.Interactor activator)
        {
            //Get the lowercase name of the Interacted Object
            string objName = self.gameObject.name.ToLower();
            //Check if its a supported Chest
            if (!objName.Contains("chest1") && !objName.Contains("chest2") && !objName.Contains("goldchest") && !objName.Contains("equipmentbarrel") && !objName.Contains("isclockbox"))
            {
                orig.Invoke(self, activator);
                return;
            }

            //Get the chest type
            //Too lazy and tired to make it prettier
            int chestType = objName.Contains("chest1") ? 0 : objName.Contains("chest2") ? 1 : objName.Contains("goldchest") ? 2 : objName.Contains("isclockbox") ? 3 : -1;

            //Randomly Select Tier
            double tier = GetRandomTier(chestType);

            //Send every client a message that a chest has been opened
            ExampleFuncClient.Invoke(activator.gameObject, check =>
            {
                //Check which client opened the chest and wheter or not he is idling
                Debug.Log("Check: " + check);
                if (check == 0)
                {
                    orig.Invoke(self, activator);
                }
                if (check != 0)
                {
                    return;
                }


                if (objName.Contains("equipmentbarrel"))
                {
                    //Signal the server that a chest has been opened
                    //Give the player that opend it and the chest position
                    ExampleCommandClientCustom.Invoke(x =>
                    {
                        x.Write("Chest");
                        x.Write(activator.gameObject);
                        x.Write(self.gameObject.transform);
                        //Specify tier as Equipment
                        x.Write((double)6);
                    });
                }
                else
                {
                    // RNG Value was above specified percantage values
                    // Cancel the drop
                    if (tier == 0)
                        return;

                    //Signal the server that a chest has been opened
                    //Give the player that opend it and the chest position
                    ExampleCommandClientCustom.Invoke(x =>
                    {
                        x.Write("Chest");
                        x.Write(activator.gameObject);
                        x.Write(self.gameObject.transform);
                        x.Write(tier);
                    });
                }
            });


        }

        private void Opening_OnEnter(On.EntityStates.Barrel.Opening.orig_OnEnter orig, EntityStates.Barrel.Opening self)
        {
            GameObject obj = self.outer.gameObject;
            string name = obj.name.ToLower();

            Debug.Log(name);
            if (!name.Contains("chest1") && !name.Contains("chest2") && !name.Contains("goldchest") && !name.Contains("equipmentbarrel") && !name.Contains("isclockbox"))
            {
                orig.Invoke(self);
                return;
            }
            //Debug.Log("FORCE OPEN THE FRICKING CHEST!");
            //Force the Chest Open
            //self.GetComponent<EntityStateMachine>().SetState(new Opened());
        }
        #endregion


        // Parameter: 0 = Normal, 1 = Large, 2 = Golden, 3 = Rusty
        // Returns int: 1 | 2 | 3
        public int GetRandomTier(int ChestType)
        {
            int tier = 0;
            double rng = (random.NextDouble() * 100);

            float[] percantages = { 80, 20, 1 };

            // !TODO add config here
            switch (ChestType)
            {
                case 0:
                    percantages = new float[3] { 100, 0, 0 };
                    break;
                case 1:
                    percantages = new float[3] { 0, 100, 0 };
                    break;
                case 2:
                    percantages = new float[3] { 0, 0, 100 };
                    break;
                case 3:
                    percantages = new float[3] { 80, 20, 1 };
                    break;

                default:
                    break;
            }

            // Normalize the Values
            // for example { 100, 50, 50 } => { 50, 25, 25 }
            percantages = NormalizeValues(percantages);

            if (rng < percantages[0])
            {
                tier = 1;
            }
            else if (rng < percantages[0] + percantages[1])
            {
                tier = 2;
            }
            else if (rng < percantages[0] + percantages[1] + percantages[2])
            {
                tier = 3;
            }

            return tier;
        }

        private float[] NormalizeValues(float[] floats)
        {
            float[] normalizedValues = floats;
            float sum = floats[0] + floats[1] + floats[2];
            for (int i = 0; i < 3; i++)
            {
                if (sum <= 100)
                {
                    break;
                }
                else
                {
                    float unNormalized = floats[i];
                    float normalized = (100 / sum * unNormalized);
                    floats[i] = normalized;
                }
            }


            return normalizedValues;
        }

        List<bool> AllBuyMenuStates = new List<bool>();
        private void RegisterMiniRpcCMDs(MiniRpcInstance miniRpc)
        {
            // This command will be called by a client (including the host), and executed on the server (host)
            ExampleCommandHostCustom = miniRpc.RegisterAction(Target.Server, (user, x) =>
            {
                // This is what the server will execute when a client invokes the IRpcAction

                var str = x.ReadString();

                Debug.Log($"[Host] {user?.userName} sent us: {str}");

                if (str == "Buymenu")
                {
                    //Get a user id, starts from 0

                    List<NetworkUser> instancesList = typeof(NetworkUser).GetFieldValue<List<NetworkUser>>("instancesList");
                    int id = instancesList.IndexOf(user);
                    if (id < 0)
                        return;

                    //When the id is bigger than the count add items, with a default value of false
                    if (AllBuyMenuStates.Count - 1 < id)
                    {
                        for (int i = AllBuyMenuStates.Count - 1; i < id; i++)
                        {
                            AllBuyMenuStates.Add(false);
                        }
                    }

                    var doubleVal = x.ReadDouble();

                    bool state = doubleVal == 0.0 ? false : true;

                    AllBuyMenuStates[id] = state;

                    int check = AllBuyMenuStates.FindAll(a => a == true).Count;
                    Debug.Log(check);
                    if (check > 0)
                    {
                        ExampleCommandClientCustom.Invoke(y =>
                        {
                            y.Write("Timescale");
                            y.Write(.25);
                        });
                    }
                    else
                    {
                        ExampleCommandClientCustom.Invoke(y =>
                        {
                            y.Write("Timescale");
                            y.Write(1.0);
                        });
                    }
                }

                if (str == "CreatePickupDroplet_Item")
                {
                    ItemIndex item = x.ReadItemIndex();
                    PickupIndex pickupIndex = new PickupIndex(item);
                    Transform chestTransform = x.ReadTransform();

                    Vector3 pos = chestTransform.position;
                    Vector3 forward = chestTransform.position;

                    RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos + Vector3.up * 1.5f, Vector3.up * 20f + forward * 2f);
                }

                if (str == "CreatePickupDroplet_Equipment")
                {
                    EquipmentIndex item = x.ReadEquipmentIndex();
                    PickupIndex pickupIndex = new PickupIndex(item);
                    Transform chestTransform = x.ReadTransform();

                    Vector3 pos = chestTransform.position;
                    Vector3 forward = chestTransform.position;

                    RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos + Vector3.up * 1.5f, Vector3.up * 20f + forward * 2f);
                }
            });

            // This command will be called by the host, and executed on all clients
            ExampleCommandClientCustom = miniRpc.RegisterAction(Target.Client, (user, x) =>
            {
                // This is what all clients will execute when the server invokes the IRpcAction

                var str = x.ReadString();

                Debug.Log($"[Client] Host sent us: {str}");

                if (str == "Reset")
                {
                    playerCharacterMaster = null;
                    //kills = 0;
                    if (this.playerCharacterMaster == null)
                        this.playerCharacterMaster = LocalUserManager.GetFirstLocalUser().cachedMasterController;

                    if (this.playerCharacterMaster.GetComponent<CA_PlayerScript>() == null)
                    {
                        this.playerCharacterMaster.gameObject.AddComponent<CA_PlayerScript>();
                        this.playerCharacterMaster.gameObject.GetComponent<CA_PlayerScript>().ExampleCommandHostCustom = this.ExampleCommandHostCustom;
                        //this.playerCharacterMaster.gameObject.GetComponent<PlayerScript>().config = config;
                        this.playerCharacterMaster.gameObject.GetComponent<CA_PlayerScript>().AwakeManual();
                    }
                }

                if (str == "Chest")
                {
                    GameObject interactor = x.ReadGameObject();
                    Transform transform = x.ReadTransform();
                    double tier = x.ReadDouble();

                    CA_PlayerScript playerScript = this.playerCharacterMaster.gameObject.GetComponent<CA_PlayerScript>();

                    Debug.Log(playerScript != null);
                    if (!playerScript)
                        return;

                    GameObject activator = interactor.GetComponent<CharacterBody>().gameObject;
                    GameObject thisGO = this.playerCharacterMaster.master.GetBody().gameObject;


                    Debug.Log(activator == thisGO);
                    if (activator != thisGO)
                        return;

                    playerScript.AddTierToGUI((int)tier, transform);
                    playerScript.SetBuyMenu(true);

                    return;
                }

                if (str == "Timescale")
                {
                    var floatVal = (float)x.ReadDouble();
                    Time.timeScale = floatVal;
                }
            });

            // -1 = Error/Not Same  ||  1 = Same but not idling  ||  0 = Same and idling
            ExampleFuncClient = miniRpc.RegisterFunc<GameObject, int>(Target.Client, (user, activator) =>
            {
                CA_PlayerScript playerScript = this.playerCharacterMaster.gameObject.GetComponent<CA_PlayerScript>();

                Debug.Log(playerScript != null);
                if (!playerScript)
                    return -1;

                GameObject activatorGO = activator.GetComponent<CharacterBody>().gameObject;
                GameObject thisGO = this.playerCharacterMaster.master.GetBody().gameObject;

                Debug.Log(activatorGO == thisGO);
                if (activatorGO != thisGO)
                    return -1;

                Debug.Log(playerScript.isChestOpening);
                if (playerScript.isChestOpening)
                    return 1;

                return 0;
            });
        }
    }
}
