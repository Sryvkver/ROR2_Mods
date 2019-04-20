using BepInEx;
using RoR2;
using UnityEngine;
using RoR2.UI;
using BepInEx.Configuration;
using System.Collections;

namespace BepInExMods
{
    [BepInPlugin("dev.felixire.LunarNotifier", "LunarModifier", "1.1.0")]
    class LunarNotifier : BaseUnityPlugin
    {
        private static ConfigWrapper<bool> confPing { get; set; }
        private static ConfigWrapper<int> confPingDuration { get; set; }
        PingIndicator pingIndicator;
        Coroutine clearCoroutine;
        private static bool shouldPing
        {
            get
            {
                return confPing.Value;
            }
            set
            {
                confPing.Value = value;
            }
        }
        private static int pingDuration
        {
            get
            {
                return confPingDuration.Value;
            }
            set
            {
                confPingDuration.Value = value;
            }
        }

        public void Awake()
        {
            //Setup config
            confPing = base.Config.Wrap<bool>("LunarNotifier", "Autoping?", "Weather or not to automatically lunar coins. (true or false)", true);
            confPingDuration = base.Config.Wrap<int>("LunarNotifier", "Autoping Duration", "How long the ping should be visible. (Seconds)", 5);
            //Check for some invalid Config settings
            //Got to add more than that...
            if (pingDuration < 0)
                pingDuration = 5;
            On.RoR2.PickupDropletController.CreatePickupDroplet += PickupDropletController_CreatePickupDroplet;

            //On.RoR2.DeathRewards.OnKilled += DeathRewards_OnKilled; //Always drop lunar coins. good for testing
        }

        private void DeathRewards_OnKilled(On.RoR2.DeathRewards.orig_OnKilled orig, DeathRewards self, DamageInfo damageInfo)
        {
            PickupDropletController.CreatePickupDroplet(PickupIndex.lunarCoin1, damageInfo.position, Vector3.zero);

            orig.Invoke(self, damageInfo);
        }

        private void PickupDropletController_CreatePickupDroplet(On.RoR2.PickupDropletController.orig_CreatePickupDroplet orig, PickupIndex pickupIndex, Vector3 position, Vector3 velocity)
        {
            orig.Invoke(pickupIndex, position, velocity);
            //Check if a lunar coin dropped
            if (pickupIndex != PickupIndex.lunarCoin1)
                return;

            //Send message
            Chat.AddMessage("<color=#307FFF>Lunar Coin</color><style=cEvent> Dropped</style>");

            if (!shouldPing)
                return;
            
            //Create a new Ping
            PingerController.PingInfo pingInfo = new PingerController.PingInfo
            {
                active = true
            };

            //Remove old Ping Target
            try
            {
                Destroy(GameObject.Find("FakeLunarCoin"));
            }
            catch (System.Exception)
            {

            }

            //Create Ping Target
            //Had weird glitches when I used the real Lunar coin
            GameObject fakeLunarCoin = new GameObject("FakeLunarCoin");

            #region OLD
            /*try
            {
                //<style=cIsHealing>💝 Der Felix 💝 wants to move here.</style>
                if(GameObject.Find("PickupLunarCoin") != null)
                {
                    fakeLunarCoin = GameObject.Find("PickupLunarCoin");
                    if (fakeLunarCoin.GetComponentInParent<IDisplayNameProvider>() != null)
                        Chat.AddMessage("1 Not Null");
                    else
                        Chat.AddMessage("1 Null");

                    if (fakeLunarCoin.GetComponent<IDisplayNameProvider>() != null)
                        Chat.AddMessage("2 Not Null");
                    else
                        Chat.AddMessage("2 Null");

                    if (fakeLunarCoin.GetComponentInChildren<IDisplayNameProvider>() != null)
                        Chat.AddMessage("3 Not Null");
                    else
                        Chat.AddMessage("3 Null");
                }

            }
            catch (System.Exception)
            {

                Chat.AddMessage("Not Found!");
            }*/
            #endregion

            //Position it to the real Lunar Coin
            fakeLunarCoin.transform.position = position;
            //Add neccesary Component
            fakeLunarCoin.AddComponent<ModelLocator>();

            pingInfo.origin = position;

            //Check if another ping is active
            if (!pingInfo.active && this.pingIndicator != null)
            {
                UnityEngine.Object.Destroy(this.pingIndicator.gameObject);
                this.pingIndicator = null;
                return;
            }
            //Creat new Ping Object
            if (!this.pingIndicator)
            {
                GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/PingIndicator"));
                this.pingIndicator = gameObject.GetComponent<PingIndicator>();
                this.pingIndicator.pingOwner = base.gameObject;
            }
            //Set some stuff
            this.pingIndicator.pingOrigin = pingInfo.origin;
            this.pingIndicator.pingNormal = pingInfo.normal;
            this.pingIndicator.pingTarget = fakeLunarCoin;
            this.pingIndicator.pingOwner = LocalUserManager.GetFirstLocalUser().cachedMasterObject;
            this.pingIndicator.RebuildPing();

            //Clear old Coroutine
            //Dont wanna have it removed because of the old one
            try
            {
                StopCoroutine(clearCoroutine);
            }
            catch (System.Exception)
            {
                Debug.Log("No Coroutine Running!");
            }
            clearCoroutine = StartCoroutine(ClearPing());
        }

        IEnumerator ClearPing()
        {
            yield return new WaitForSeconds(pingDuration);
            try
            {
                UnityEngine.Object.Destroy(this.pingIndicator.gameObject);
                this.pingIndicator = null;
            }
            catch (System.Exception)
            {
                Debug.Log("Gameobject not found. Problaby already destroyed!");
                throw;
            }
        }
    }
}
