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

namespace KillShop
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency(MiniRpcPlugin.Dependency)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class KillShop : BaseUnityPlugin
    {
        private const string ModVer = "1.0.0";
        private const string ModName = "KillShop";
        private const string ModGuid = "dev.felixire.KillShop";

        #region Networkstuff

        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom { get; set; }
        public IRpcAction<Action<NetworkWriter>> ExampleCommandClientCustom { get; set; }

        #endregion

        //private int kills = 0;
        private PlayerCharacterMasterController playerCharacterMaster = null;

        private void Awake()
        {
            On.RoR2.RoR2Application.UnitySystemConsoleRedirector.Redirect += orig => { };
            var miniRpc = MiniRpc.CreateInstance(ModGuid);
            RegisterMiniRpcCMDs(miniRpc);

            On.RoR2.Run.Awake += Run_Awake;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.UI.HUD.Start += HUD_Start;
        }

        private void HUD_Start(On.RoR2.UI.HUD.orig_Start orig, HUD self)
        {
            orig.Invoke(self);

            PlayerScript playerScript = LocalUserManager.GetFirstLocalUser().cachedMasterController.GetComponent<PlayerScript>();
            if (playerScript)
            {
                playerScript.AddKillCounter();
            }
        }

        private void Run_Awake(On.RoR2.Run.orig_Awake orig, Run self)
        {
            orig.Invoke(self);

            //Readd the hook
            On.RoR2.Run.Update += Run_Update;
        }

        private void Run_Update(On.RoR2.Run.orig_Update orig, Run self)
        {
            orig.Invoke(self);

            //Wait till all players loaded
            //Dont know if there is a better option for this
            if (Run.instance.time < 5)
                return;

            bool isHost = LocalUserManager.GetFirstLocalUser().cachedMasterController == PlayerCharacterMasterController.instances[0];

            if (!isHost)
            {
                On.RoR2.Run.Update -= Run_Update;
                return;
            }

            ExampleCommandClientCustom.Invoke(x =>
            {
                x.Write("Reset");
            });

            On.RoR2.Run.Update -= Run_Update;
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig.Invoke(self, damageReport);

            GameObject attacker = damageReport.damageInfo.attacker;
            if (attacker)
            {
                CharacterBody component = attacker.GetComponent<CharacterBody>();
                if (component)
                {
                    GameObject masterObject = component.masterObject;
                    if (masterObject)
                    {
                        PlayerCharacterMasterController component2;
                        //Engineer Turrret Fix
                        if (masterObject.name == "EngiTurretMaster(Clone)")
                        {
                            component2 = masterObject.GetComponent<Deployable>()?.ownerMaster?.GetComponent<PlayerCharacterMasterController>();
                        }
                        else
                        {
                            component2 = masterObject.GetComponent<PlayerCharacterMasterController>();
                        }

                        Debug.Log("Killed");

                        ExampleCommandClientCustom.Invoke(x =>
                        {
                            x.Write("Kill");
                            x.Write(component2.gameObject);
                        });
                    }
                }
            }

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
                    Vector3 pos = x.ReadVector3();

                    RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos, Vector3.up * 10f);
                }

                if (str == "CreatePickupDroplet_Equipment")
                {
                    EquipmentIndex item = x.ReadEquipmentIndex();
                    PickupIndex pickupIndex = new PickupIndex(item);
                    Vector3 pos = x.ReadVector3();

                    RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, pos, Vector3.up * 10f);
                }

                if (str == "AddMoney")
                {
                    GameObject go = x.ReadGameObject();
                    int amount = (int)x.ReadDouble();
                    PlayerCharacterMasterController playerCharacterMaster = go.GetComponent<PlayerCharacterMasterController>();

                    playerCharacterMaster.master.GiveMoney((uint)amount);
                }

                if (str == "AddExp")
                {
                    GameObject go = x.ReadGameObject();
                    int amount = (int)x.ReadDouble();
                    PlayerCharacterMasterController playerCharacterMaster = go.GetComponent<PlayerCharacterMasterController>();

                    playerCharacterMaster.master.GiveExperience((uint)amount);
                }

                if (str == "FullHP")
                {
                    GameObject go = x.ReadGameObject();
                    PlayerCharacterMasterController playerCharacterMaster = go.GetComponent<PlayerCharacterMasterController>();

                    playerCharacterMaster.master.GetBody().healthComponent.health = playerCharacterMaster.master.GetBody().healthComponent.fullHealth;
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

                    if (this.playerCharacterMaster.GetComponent<PlayerScript>() == null)
                    {
                        this.playerCharacterMaster.gameObject.AddComponent<PlayerScript>();
                        this.playerCharacterMaster.gameObject.GetComponent<PlayerScript>().ExampleCommandHostCustom = this.ExampleCommandHostCustom;
                        this.playerCharacterMaster.gameObject.GetComponent<PlayerScript>().AwakeManual();
                    }
                }

                if(str == "Timescale")
                {
                    var floatVal = (float)x.ReadDouble();
                    Time.timeScale = floatVal;
                }


                if (str == "Kill")
                {
                    GameObject go = x.ReadGameObject();
                    PlayerCharacterMasterController playerCharacterMaster = go.GetComponent<PlayerCharacterMasterController>();
                    /*
                    if (this.playerCharacterMaster == null)
                        this.playerCharacterMaster = LocalUserManager.GetFirstLocalUser().cachedMasterController;
                    */


                    if (playerCharacterMaster == this.playerCharacterMaster)
                    {
                        ++playerCharacterMaster.GetComponent<PlayerScript>().kills;
                        //Chat.AddMessage("Added Kill!\n\rNew Kill count is: " + ++playerCharacterMaster.GetComponent<PlayerScript>().kills);
                    }
                }
            });
        }
    }
}
