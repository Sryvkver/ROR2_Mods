using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2.Chat;

namespace Command_Artifact
{
    class CA_TimeScaler : MonoBehaviour
    {
        public void Awake()
        {
            On.RoR2.Chat.AddMessage_string += Chat_AddMessage_string;
        }

        private void Chat_AddMessage_string(On.RoR2.Chat.orig_AddMessage_string orig, string message)
        {
            
        }

        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;

            CA_Manager[] allManagers = FindObjectsOfType<CA_Manager>();

            for (int i = 0; i < allManagers.Length; i++)
            {
                allManagers[i].SetTimeScale(timeScale);
            }
        }

        public void SetupPlayers(ConfigStuff config, System.Random random)
        {
            RemoveLeftovers();

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {

                if (player.gameObject.GetComponent<CA_Manager>() == null)
                {
                    CA_Manager manager = player.gameObject.AddComponent<CA_Manager>();
                    manager.config = config;
                    manager.random = random;
                    Chat.AddMessage("Added");
                }

                if (player.gameObject.GetComponent<Notification>() == null)
                {
                    Notification notification;
                    notification = player.gameObject.AddComponent<Notification>();

                    notification.transform.SetParent(player.gameObject.transform);
                    notification.SetPosition(new Vector3((float)(Screen.width * 50) / 100f, (float)(Screen.height * 50) / 100f, 0f));
                    notification.SetSize(new Vector2(500, 250));

                    notification.GetTitle = (() => string.Format("Item Selector. Press {0} to continue", config.SelectButton.ToString()));

                    notification.GenericNotification.fadeTime = 1f;
                    notification.GenericNotification.duration = 10000f;
                    player.gameObject.GetComponent<CA_Manager>().HideSelectMenu();
                    Chat.AddMessage("Added");
                }
            }
        }

        public void SetupPlayer(ConfigStuff config, System.Random random, PlayerCharacterMasterController player)
        {
            if (player.gameObject.GetComponent<CA_Manager>() == null)
            {
                CA_Manager manager = player.gameObject.AddComponent<CA_Manager>();
                manager.config = config;
                manager.random = random;
                Chat.AddMessage("Added");
            }
            else
            {
                Chat.AddMessage("Failed to add");
            }

            if (player.gameObject.GetComponent<Notification>() == null)
            {
                Notification notification;
                notification = player.gameObject.AddComponent<Notification>();

                notification.transform.SetParent(player.gameObject.transform);
                notification.SetPosition(new Vector3((float)(Screen.width * 50) / 100f, (float)(Screen.height * 50) / 100f, 0f));
                notification.SetSize(new Vector2(500, 250));

                notification.GetTitle = (() => string.Format("Item Selector. Press {0} to continue", config.SelectButton.ToString()));

                notification.GenericNotification.fadeTime = 1f;
                notification.GenericNotification.duration = 10000f;
                player.gameObject.GetComponent<CA_Manager>().HideSelectMenu();
                Chat.AddMessage("Added");
            }
            else
            {
                Chat.AddMessage("Failed to add");
            }
        }

        public int PurchaseInteraction_Stuff(PurchaseInteraction self, Interactor activator)
        {
            CA_Manager[] allManagers = FindObjectsOfType<CA_Manager>();

            Chat.SendBroadcastChat(new SimpleChatMessage { baseToken = "<color=#e5eefc>{0}: {1}</color>", paramTokens = new[] { "Thingimayig", allManagers.Length.ToString() } });

            for (int i = 0; i < allManagers.Length; i++)
            {
                //-1 = Error; 0 = Same but not idling; 1 = Same and Idling 2 = Not same; 3 = not a chest
                int check = allManagers[i].PurchaseInteraction_Receiver(self, activator);
                Chat.AddMessage("Check: " + check);
                if (check == 1 || check == 3)
                {
                    return 0;
                    //orig.Invoke(self, activator);
                    break;
                }
                else if (check == 0)
                {
                    break;
                }
            }
            return 1;
        }

        public int GetAmountOfManagers()
        {
            return FindObjectsOfType<CA_Manager>().Length;
        }

        public void RemoveLeftovers()
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
        }
    }
}
