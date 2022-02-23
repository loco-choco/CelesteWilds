using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;

namespace CelesteWilds.Collectibles
{
    //Fazer a UI bem parecida com o mario 64, e adicionar dicas quanto apertar o menu de pause como em https://youtu.be/-e5yqy4eNaQ?t=675
    public class MarshmellowCollectiblesUI : MonoBehaviour
    {
        IModHelper helper;
        PlayerAudioController playerAudioController;

        CollectibleManager<MarshmellowCollectibleData> collectibleManager;
        Dictionary<Sector.Name, List<MarshmellowCollectibleData>> collectibles;

        private PlayerSectorDetector playerSectorDetector;

        private const string textureFileName = "marsh.png";
        private static Sprite marshMelloSprite;
        private ScreenPrompt collectedPrompt;
        private NotificationData onCollectNotification;

        private Text pauseHintText;

        int totalAmountOfCollectiblesInSector;
        int amountOfCollectedInSector;


        private float waitToFadePrompt = 5f;
        public void Initialize(IModHelper helper, CollectibleManager<MarshmellowCollectibleData> collectibleManager)
        {

            this.helper = helper;
            this.collectibleManager = collectibleManager;

            collectibleManager.OnCollected += OnCollected;

            collectibles = new Dictionary<Sector.Name, List<MarshmellowCollectibleData>>();
            PopulateCollectiblesDict();

            if (marshMelloSprite == null) 
            {
                Texture2D texture = helper.Assets.GetTexture(textureFileName);
                marshMelloSprite = Sprite.Create(texture, Rect.MinMaxRect(0f, 0f, texture.width, texture.height), Vector2.zero);
            }
        }

        private void PopulateCollectiblesDict() 
        {
            foreach(var pair in collectibleManager.collectibles) 
            {
                if (!collectibles.ContainsKey(pair.Value.SectorWhereItBelongs))
                    collectibles.Add(pair.Value.SectorWhereItBelongs, new List<MarshmellowCollectibleData>());

                collectibles[pair.Value.SectorWhereItBelongs].Add(pair.Value);
            }
            //helper.Console.WriteLine(collectibles.Count.ToString());
        }
        public void Start()
        {
            playerAudioController = Locator.GetPlayerAudioController();
            playerSectorDetector = Locator.GetPlayerSectorDetector();

            collectedPrompt = new ScreenPrompt($"0 / {collectibleManager.collectibles.Count}", marshMelloSprite);
            collectedPrompt.SetDisplayState(ScreenPrompt.DisplayState.Attention);
            collectedPrompt.SetVisibility(false);
            Locator.GetPromptManager().AddScreenPrompt(collectedPrompt, PromptPosition.LowerLeft, false);

            onCollectNotification = new NotificationData(NotificationTarget.Player, "NAMENAMENAME");
            
            helper.Menus.PauseMenu.OnOpened += PauseMenu_OnOpened;
            GameObject pauseLabel = GameObject.Find("PauseMenu/PauseMenuCanvas/PauseMenuBlock/PauseMenuItems/PauseMenuItemsLayout/LabelPaused");
            GameObject hintLabel = Instantiate(pauseLabel, pauseLabel.transform.parent);
            hintLabel.transform.SetAsLastSibling();
            pauseHintText = hintLabel.GetComponentInChildren<Text>();
            pauseHintText.color = new Color(0.8860378f, 0.8867925f, 0.8078431f);
            pauseHintText.text = "Hint: ";
            pauseHintText.fontSize = 36;
        }

        private void PauseMenu_OnOpened()
        {
            if (!collectibles.ContainsKey(LastSectorEntered))
            {
                pauseHintText.text = TextTranslation.Translate_UI((int)UIPrompts.NO_COLLECTIBLE_HINT);
                return;
            }
            var nextHintAbout = collectibles[LastSectorEntered].FirstOrDefault((data) => !data.IsCollected);

            if (nextHintAbout != null)
                pauseHintText.text = TextTranslation.Translate_UI((int)UIPrompts.COLLECTIBLE_HINT) + TextTranslation.Translate(nextHintAbout.Hint);
            else
                pauseHintText.text = TextTranslation.Translate_UI((int)UIPrompts.NO_COLLECTIBLE_HINT);
        }

        public void OnDestroy()
        {
            collectibleManager.OnCollected -= OnCollected;
            helper.Menus.PauseMenu.OnOpened -= PauseMenu_OnOpened;
        }

        public Sector.Name LastSectorEntered { get; private set; }
        readonly Sector.Name[] ignoreList = new Sector.Name[] { Sector.Name.Unnamed };
        private void Update() 
        {
            Sector sector = playerSectorDetector.GetLastEnteredSector(ignoreList);
            if (sector == null)
                return;
            Sector.Name newSectorEntered = sector.GetName();
            
            if (newSectorEntered != LastSectorEntered)
                OnEnterNewSector(newSectorEntered);

            LastSectorEntered = newSectorEntered;
        }

        private void OnEnterNewSector(Sector.Name colletiblesSector) 
        {
            if (!collectibles.ContainsKey(colletiblesSector))
                return;

            totalAmountOfCollectiblesInSector = collectibles[colletiblesSector].Count();
            amountOfCollectedInSector = collectibles[colletiblesSector].Count((data) => data.IsCollected);
        }

        private void OnCollected(CollectibleData data)
        {
            MarshmellowCollectibleData marshmellowData = (MarshmellowCollectibleData)data;
            amountOfCollectedInSector++;
            onCollectNotification.displayMessage = TextTranslation.Translate(marshmellowData.DisplayName);
            NotificationManager.SharedInstance.PostNotification(onCollectNotification);
            DisplayCollectiblesCollected(LastSectorEntered);
            
            if (collectibleManager.collectibles.Count((pair) => pair.Value.IsCollected) == collectibleManager.collectibles.Count)
                OnCollectAllEvent();
        }

        private void DisplayCollectiblesCollected(Sector.Name colletiblesSector) 
        {
            if (!collectibles.ContainsKey(colletiblesSector))
                return;

            collectedPrompt.SetVisibility(true);
            collectedPrompt.SetText($"{amountOfCollectedInSector} / {totalAmountOfCollectiblesInSector}");
            StartCoroutine("WaitToStopDisplay");
        }


        private IEnumerator WaitToStopDisplay()
        {
            yield return new WaitForSeconds(waitToFadePrompt / 2f);
            Locator.GetPlayerAudioController().PlayMarshmallowCatchFire();
            yield return new WaitForSeconds(waitToFadePrompt / 2f);
            collectedPrompt.SetVisibility(false);
        }

        private void OnCollectAllEvent() 
        {
            helper.Console.WriteLine("ALL MARSHMELLOWS HAVE BEEN COLLECTED");
            StartCoroutine("WaitToDetonate");
        }

        private IEnumerator WaitToDetonate()
        {
            yield return new WaitForSeconds(waitToFadePrompt);
            playerAudioController.PlayOneShotInternal(AudioType.DBAnglerfishDetectTarget);
            yield return new WaitForSeconds(3f);
            playerAudioController.PlayOneShotInternal(AudioType.KazooTheme);
            yield return new WaitForSeconds(48f);
            SunController sunController = Locator.GetSunController();
            if (!sunController.HasSupernovaStarted())
                GlobalMessenger.FireEvent("TriggerSupernova");
        }
    }
}
