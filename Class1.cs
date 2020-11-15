﻿using BepInEx;
using RoR2;
using UnityEngine.Networking;



namespace BossVengenceRev
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Melting-Cube.BossVengenceRevive", "Boss and Vengence Revive", "1.0.1")]
    public class BossVengenceRevive : BaseUnityPlugin
    {
        public void Awake()
        {
            Chat.AddMessage("Loaded testmod");
            On.RoR2.BossGroup.OnDefeatedServer += (orig, self) =>
            {
                orig(self);
                //see if they are playing with others
                bool solo = RoR2.RoR2Application.isInSinglePlayer || !NetworkServer.active;
                if (!solo)
                {
                    foreach (RoR2.PlayerCharacterMasterController playerCharacterMasterController in RoR2.PlayerCharacterMasterController.instances)
                    {
                        bool playerConnected = playerCharacterMasterController.isConnected;
                        bool isDead = !playerCharacterMasterController.master.GetBody() || playerCharacterMasterController.master.IsDeadAndOutOfLivesServer() || !playerCharacterMasterController.master.GetBody().healthComponent.alive;
                        if (playerConnected && isDead)
                        {
                            playerCharacterMasterController.master.RespawnExtraLife();
                        }
                    }
                }
            };
        }
    }
}