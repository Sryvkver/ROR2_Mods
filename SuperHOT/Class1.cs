using BepInEx;
using MiniRpcLib;
using MiniRpcLib.Action;
using MiniRpcLib.Func;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperHOT
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency(MiniRpcPlugin.Dependency)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class SuperHOT : BaseUnityPlugin
    {
        private const string ModVer = "1.0.0";
        private const string ModName = "SuperHot";
        private const string ModGuid = "dev.felixire.SuperHot";

        // Define two actions sending/receiving a single string
        public IRpcAction<string> ExampleCommandHost { get; set; }
        public IRpcAction<string> ExampleCommandClient { get; set; }

        // Define two actions that manages reading/writing messages themselves
        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom { get; set; }
        public IRpcAction<Action<NetworkWriter>> ExampleCommandClientCustom { get; set; }

        // Define two functions of type `string Function(bool);`
        public IRpcFunc<bool, string> ExampleFuncClient { get; set; }
        public IRpcFunc<bool, string> ExampleFuncHost { get; set; }


        public Rewired.Player inputPlayer;
        public CharacterMotor characterMotor;
        public CharacterBody characterBody;

        public float[] times;

        public void Awake()
        {
            Debug.Log("Loaded!");

            On.RoR2.RoR2Application.UnitySystemConsoleRedirector.Redirect += orig => { };
            var miniRpc = MiniRpc.CreateInstance(ModGuid);
            RegisterMiniRpcCommands(miniRpc);

            On.RoR2.Run.OnServerTeleporterPlaced += Run_OnServerTeleporterPlaced;
            On.RoR2.Run.BeginGameOver += Run_BeginGameOver;
        }

        private void Run_BeginGameOver(On.RoR2.Run.orig_BeginGameOver orig, Run self, GameResultType gameResultType)
        {
            On.RoR2.Run.Update -= Run_Update;

            Time.timeScale = 1.0f;

            orig.Invoke(self, gameResultType);

        }

        private void Run_OnServerTeleporterPlaced(On.RoR2.Run.orig_OnServerTeleporterPlaced orig, Run self, SceneDirector sceneDirector, GameObject teleporter)
        {
            orig.Invoke(self, sceneDirector, teleporter);

            ExampleCommandClientCustom.Invoke(y => { y.Write("Setup"); y.Write(0.0); });

            times = new float[PlayerCharacterMasterController.instances.Count];
        }

        public void Run_Update(On.RoR2.Run.orig_Update orig, Run self)
        {
            orig.Invoke(self);

            //Check if player is moving in any Axis
            float vert = inputPlayer.GetAxis("MoveVertical");
            float horz = inputPlayer.GetAxis("MoveHorizontal");

            //Rework so it works better with controller
            float move = vert != 0 ? .5f : horz != 0 ? .5f : 0;

            float sprint = move == 0 ? 0f : characterBody.isSprinting ? .4f : 0;

            //Check if player is litteraly pressing moving button (Jumping, Skills)
            float skill = inputPlayer.GetButton("PrimarySkill") ? .75f :
                        inputPlayer.GetButton("SecondarySkill") ? .75f :
                        inputPlayer.GetButton("UtilitySkill") ? .75f :
                        inputPlayer.GetButton("SpecialSkill") ? .75f :
                        inputPlayer.GetButton("Jump") ? .75f :
                        inputPlayer.GetButton("Equipment") ? .75f : 0;

            float onGround = characterMotor.isGrounded ? 0f : .25f;

            //Calculate Time
            //Gotta rework this sheit
            float time = onGround + skill + move + sprint;

            if (time > 1f)
                time = 1f;

            ExampleCommandHostCustom.Invoke(x => { x.Write("Time"); x.Write((double)time); });
        }

        private void RegisterMiniRpcCommands(MiniRpcInstance miniRpc)
        {
            ExampleCommandHost = miniRpc.RegisterAction(Target.Server, (NetworkUser user, string x) => Debug.Log($"[Host] {user?.userName} sent us: {x}"));
            ExampleCommandClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, string x) => Debug.Log($"[Client] Host sent us: {x}"));

            // This command will be called by a client (including the host), and executed on the server (host)
            ExampleCommandHostCustom = miniRpc.RegisterAction(Target.Server, (user, x) =>
            {
                // This is what the server will execute when a client invokes the IRpcAction

                var str = x.ReadString();
                var doubleVal = x.ReadDouble();

                Debug.Log($"[Host] {user?.userName} sent us: {str} {doubleVal}");

                if(str == "Time")
                {
                    List < NetworkUser > instancesList = typeof(NetworkUser).GetFieldValue<List<NetworkUser>>("instancesList");
                    int id = instancesList.IndexOf(user);
                    if (id < 0)
                        return;

                    //Debug.Log("ID: " + id);

                    times[id] = ((float)doubleVal / times.Length);

                    float timeScale = 0.0f;

                    for (int i = 0; i < times.Length; i++)
                    {
                        timeScale += times[i];
                    }

                    ExampleCommandClientCustom.Invoke(y => { y.Write("SetTimeScale"); y.Write((double)timeScale); });
                }
            });

            // This command will be called by the host, and executed on all clients
            ExampleCommandClientCustom = miniRpc.RegisterAction(Target.Client, (user, x) =>
            {
                // This is what all clients will execute when the server invokes the IRpcAction

                var str = x.ReadString();
                var doubleVal = x.ReadDouble();

                float floatVal = (float)doubleVal;

                Debug.Log($"[Client] Host sent us: {str} {floatVal}");

                if (str == "SetTimeScale")
                {
                    /*
                        Todo Add config
                    */
                    if (floatVal < .1f)
                        floatVal = .1f;
                    Time.timeScale = floatVal;
                }

                if(str == "Setup")
                {
                    characterBody = LocalUserManager.GetFirstLocalUser().cachedBody;
                    inputPlayer = LocalUserManager.GetFirstLocalUser().inputPlayer;
                    characterMotor = LocalUserManager.GetFirstLocalUser().cachedBodyObject.GetComponent<CharacterMotor>();
                    On.RoR2.Run.Update += Run_Update;
                }
            });
        }
    }
}
