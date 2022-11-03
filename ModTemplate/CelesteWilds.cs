using OWML.ModHelper;
using HarmonyLib;

using CelesteWilds.Collectibles;
using CelesteWilds.TutorialText;
using OWML.Common;

namespace CelesteWilds
{
    public class CelesteWilds : ModBehaviour
    {
        Climbing climbing;
        Dashing dashing;

        public static CollectibleManager<MarshmellowCollectibleData> collectibleManager;
        public static TutorialTextManager tutorialTextManager;
        MarshmellowCollectiblesUI collectiblesUI;
        private void Start()
        {
            ILocalizationAPI api = ModHelper.Interaction.GetModApi<ILocalizationAPI>("xen.LocalizationUtility");

            Harmony harmony = new Harmony("com.locochoco.CelesteWilds");
            harmony.PatchAll();

            ChangePlayerResources.ChangeValues(true);
            ChangePlayerResources.AllowJetpack(false);
            ChangePlayerResources.AlwaysAllowBooster(true);

            ChangePlayerResources.SetMaxFuel(Climbing_Old.MaxClimbingStamina);
            ChangePlayerResources.SetLowFuel(Climbing_Old.MaxClimbingStamina * 0.5f);
            ChangePlayerResources.SetCriticalFuel(Climbing_Old.MaxClimbingStamina * 0.25f);

            GlobalMessenger.AddListener("SuitUp", OnSuitUp);
            GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);

            collectibleManager = new CollectibleManager<MarshmellowCollectibleData>(ModHelper, "celesteWildsCollectibles.json");
            collectibleManager.LoadCollectibles();

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                collectibleManager.SaveCollectibles();

                if (loadScene != OWScene.SolarSystem && loadScene != OWScene.EyeOfTheUniverse) return;
                var playerBody = FindObjectOfType<PlayerBody>();

                climbing = playerBody.gameObject.AddComponent<Climbing>();
                playerBody.gameObject.AddComponent<MatchRigidbody>();
                climbing.c = ModHelper.Console;
                dashing = playerBody.gameObject.AddComponent<Dashing>();
                dashing.c = ModHelper.Console;


                climbing.enabled = false;
                dashing.enabled = false;

                if (loadScene == OWScene.SolarSystem)
                {
                    //Collectibles Generation
                    collectibleManager.GenerateCollectibles();
                    //Collectibles UI
                    collectiblesUI = playerBody.gameObject.AddComponent<MarshmellowCollectiblesUI>();
                    collectiblesUI.Initialize(ModHelper, collectibleManager);
                    //Tutorial Jornals Generation
                    tutorialTextManager.GenerateJournals();
                }

                playerBody.gameObject.AddComponent<CollectibleSpawner>().marshmellowCollectiblesUI = collectiblesUI;
            };

            tutorialTextManager = new TutorialTextManager(this, api, "TutorialText");
        }

        private void OnDestroy()
        {
            GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
            GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
            collectibleManager.SaveCollectibles();
        }

        private void OnRemoveSuit()
        {
            if(climbing.isClimbing)
                climbing.StopClimbing();

            climbing.enabled = false;
            dashing.enabled = false;
        }

        private void OnSuitUp()
        {
            climbing.enabled = true;
            dashing.enabled = true;
            dashing.allowNormalDashing = !Locator.GetPlayerSuit().IsTrainingSuit();
        }
        public override void Configure(IModConfig config)
        {
            bool allowPlayerToSpawnCollectibles = config.GetSettingsValue<bool>("allowPlayerToSpawnCollectibles");
            CollectibleSpawner.CanPlayerSpawnMoreCollectibles = allowPlayerToSpawnCollectibles;
            bool resetCollectibles = config.GetSettingsValue<bool>("resetCollectibles");

            if (resetCollectibles && collectibleManager != null) 
                collectibleManager.ResetCollectibles();

            config.SetSettingsValue("resetCollectibles", false);
        }
    }
    public enum UIPrompts
    {
        CLIMB = UITextType.Tooltip_TextAudio_SFXVol_Dupe + 1,
        STOP_CLIMB,
        STARTED_CLIMB,
        STOPED_CLIMB,
        COLLECTIBLE_HINT,
        NO_COLLECTIBLE_HINT,
    }
}
