using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;


namespace BossVengenceRev
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Melting-Cube.BossVengenceRevive", "Boss and Vengence Revive", "1.1.0")]
    public class BossVengenceRevive : BaseUnityPlugin
    {
        //add config entries
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ConfigEntry<bool> Doppelganger { get; set; }
        public static ConfigEntry<bool> TeleporterBoss { get; set; }
        public static ConfigEntry<bool> MiscBoss { get; set; }

    //respawn method to call
    public void RespawnChar()
        {
            //see if they are playing with others
            bool solo = RoR2.RoR2Application.isInSinglePlayer || !NetworkServer.active;
            if (!solo)
            {
                //loop through every player and res the ones that are dead
                foreach (RoR2.PlayerCharacterMasterController playerCharacterMasterController
                         in RoR2.PlayerCharacterMasterController.instances)
                {
                    bool playerConnected = playerCharacterMasterController.isConnected;
                    bool isDead = !playerCharacterMasterController.master.GetBody()
                                || playerCharacterMasterController.master.IsDeadAndOutOfLivesServer()
                                || !playerCharacterMasterController.master.GetBody().healthComponent.alive;
                    if (playerConnected && isDead)
                        playerCharacterMasterController.master.RespawnExtraLife();
                }
            }
            return;
        }

        public void Awake()
        {
            //make config entries
            Enabled = Config.Bind<bool>(
                "Mod Status",
                "Enabled",
                true,
                "Turn the mod on or off."
                );
            TeleporterBoss = Config.Bind<bool>(
                "Boss Types",
                "Teleporter Boss",
                true,
                "Revive dead players after you defeat a teleporter boss."
                );
            Doppelganger = Config.Bind<bool>(
                "Boss Types",
                "Doppelganger",
                true,
                "Revive dead players after you defeat a Doppelganger." +
                "\nDoppelgangers are enabled by the Vengence Artifact"
                );
            MiscBoss = Config.Bind<bool>(
                "Boss Types",
                "Special Bosses",
                false,
                "Revive dead players after you defeat a Special Boss." +
                "\nSpecial bosses are ones such as Aurelionite or Alloy Worship Unit."
                );

            //On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
            Logger.LogInfo("Loaded BossVengenceRevive");

            //is mod enabled
            if (!Enabled.Value)
                return;

            //everytime a character is hit
            On.RoR2.CharacterBody.OnTakeDamageServer += (orig, self, damageReport) =>
            {
                //call original method
                orig(self, damageReport);

                //exit if the entity is alive
                if (self.healthComponent.alive)
                return;

                //see if there is a boss and if character has model
                if (BossGroup.FindBossGroup(self) == null
                    || self.GetComponent<ModelLocator>()?.modelTransform?.GetComponent<CharacterModel>() == null)
                    return;

                //is character last member
                if (BossGroup.FindBossGroup(self).combatSquad.memberCount <= 1)
                {
                    //is defeated bossgroup doppelganger
                    if (self.GetComponent<ModelLocator>()?.modelTransform?.GetComponent<CharacterModel>().isDoppelganger == true
                        && Doppelganger.Value)
                    {
                        RespawnChar();
                        Logger.LogInfo("Doppelganger killed, revived dead players");
                        return;
                    }

                    //is defeated bossgroup a teleporter boss
                    if (BossGroup.FindBossGroup(self).GetComponent<TeleporterInteraction>()
                        && TeleporterBoss.Value)
                    {
                        RespawnChar();
                        Logger.LogInfo("Teleporter Boss killed, revived dead players");
                        return;
                    }

                    //is defeated boss even a boss (misc.)
                    if (self.isBoss
                        && MiscBoss.Value)
                    {
                        RespawnChar();
                        Logger.LogInfo("Special Boss killed, revived dead players");
                        return;
                    }
                }
            };
        }
    }
}