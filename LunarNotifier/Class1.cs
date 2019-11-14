using BepInEx;
using RoR2;
using UnityEngine;
using RoR2.UI;
using BepInEx.Configuration;
using System.Collections;

namespace BepInExMods
{
    [BepInPlugin("dev.felixire.LunarNotifier", "LunarModifier", "1.1.9")]
    class LunarNotifier : BaseUnityPlugin
    {
        private static ConfigEntry<bool> ConfPing { get; set; }
        private static ConfigEntry<int> ConfPingDuration { get; set; }
        PingIndicator pingIndicator;
        Coroutine clearCoroutine;
        private static bool ShouldPing
        {
            get
            {
                return ConfPing.Value;
            }
            set
            {
                ConfPing.Value = value;
            }
        }
        private static int PingDuration
        {
            get
            {
                return ConfPingDuration.Value;
            }
            set
            {
                ConfPingDuration.Value = value;
            }
        }

        public void Awake()
        {
            //Setup config
            ConfPing = Config.Bind("LunarNotifier", "Autoping?", true, "Wether or not to automatically lunar coins. (true or false)");
            ConfPingDuration = Config.Bind("LunarNotifier", "Autoping Duration", 5, "How long the ping should be visible. (Seconds)");
            //Check for some invalid Config settings
            //Got to add more than that...
            if (PingDuration < 0)
                PingDuration = 5;
            On.RoR2.PickupDropletController.CreatePickupDroplet += PickupDropletController_CreatePickupDroplet;
            
        }

        private void PickupDropletController_CreatePickupDroplet(On.RoR2.PickupDropletController.orig_CreatePickupDroplet orig, PickupIndex pickupIndex, Vector3 position, Vector3 velocity)
        {
            orig(pickupIndex, position, velocity);
            //Check if a lunar coin dropped
            if (pickupIndex != PickupCatalog.FindPickupIndex("LunarCoin.Coin0"))
                return;

            //Send message
            Chat.AddMessage("<color=#307FFF>Lunar Coin</color><style=cEvent> Dropped</style>");

            if (!ShouldPing)
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
            catch 
            {

            }

            //Create Ping Target
            //Had weird glitches when I used the real Lunar coin
            GameObject fakeLunarCoin = new GameObject("FakeLunarCoin");

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
            yield return new WaitForSeconds(PingDuration);
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
