using OWML.Common;
using OWML.ModHelper;
using System.IO;
using System.Text;
using UnityEngine;

namespace CelesteWilds.TutorialText
{
    public class TutorialTextManager
    {
        //1 - ler o xml como um TranslationTable_XML
        //2 - Pegar seus dados e adiconar ao dicionario desejado ou trocar pelo novo valor
        //3 - ter no nome o tipo de tradução e todos na mesma pasta, ex: climbingTutorialTranslationTable_POR ,climbingTutorialTranslationTable_ENG, ... 
        TextAsset climbingTutorialText;
        TextAsset collectiblesTutorialText;
        TextAsset dashTutorialText;
        private IModHelper helper;

        public TutorialTextManager(ModBehaviour mod, ILocalizationAPI api, params string[] tutorialTextFolderName)
        {
            helper = mod.ModHelper;
            string tutorialFolderPath = Path.Combine(tutorialTextFolderName);
            string climbingTutorialString = File.ReadAllText(Path.Combine(helper.Manifest.ModFolderPath, tutorialFolderPath, "climbingTutorial.xml"));
            climbingTutorialText = new TextAsset(climbingTutorialString);

            string collectiblesTutorialString = File.ReadAllText(Path.Combine(helper.Manifest.ModFolderPath, tutorialFolderPath, "collectiblesTutorial.xml"));
            collectiblesTutorialText = new TextAsset(collectiblesTutorialString);

            string dashTutorialString = File.ReadAllText(Path.Combine(helper.Manifest.ModFolderPath, tutorialFolderPath, "dashTutorial.xml"));
            dashTutorialText = new TextAsset(dashTutorialString);

            Utils.AddFolderOfTranslationTables(api, mod, tutorialFolderPath, "celesteWildsTranslationTable_");
        }
        public void GenerateJournals()
        {
            var climbingTutorialJournal = GenerateJournal(climbingTutorialText);
            climbingTutorialJournal.parent = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
            climbingTutorialJournal.localPosition = new Vector3(41.7724f, 33.62963f, 70.57231f);
            climbingTutorialJournal.localEulerAngles = new Vector3(63.3553f, 143.7291f, 16.3578f);
            climbingTutorialJournal.Find("plaque_paper_1 (3)").gameObject.SetActive(false);

            var collectiblesTutorialJournal = GenerateJournal(collectiblesTutorialText);
            collectiblesTutorialJournal.parent = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
            collectiblesTutorialJournal.localPosition = new Vector3(10.9837f, -44.96f, 186.797f);
            collectiblesTutorialJournal.localEulerAngles = new Vector3(12.1138f, 334.5703f, 261.9001f);
            collectiblesTutorialJournal.Find("plaque_paper_1 (2)").gameObject.SetActive(false);

            var dashTutorialJournal = GenerateJournal(dashTutorialText);
            dashTutorialJournal.parent = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
            dashTutorialJournal.localPosition = new Vector3(3.0033f, -49.3382f, 227.0203f);
            dashTutorialJournal.localEulerAngles = new Vector3(341.9106f, 88.2874f, 198.555f);

            dashTutorialJournal.Find("plaque_paper_1 (1)").gameObject.SetActive(false);
            dashTutorialJournal.Find("plaque_paper_1 (2)").gameObject.SetActive(true);
            dashTutorialJournal.Find("plaque_paper_1 (3)").gameObject.SetActive(false);
        }

        private static GameObject jornalReference;
        public static Transform GenerateJournal(TextAsset textAsset)
        {
            if (jornalReference == null)
                jornalReference = GameObject.Find("Prefab_HEA_Journal");

            GameObject journal = Object.Instantiate(jornalReference);
            journal.GetComponentInChildren<CharacterDialogueTree>()._xmlCharacterDialogueAsset = textAsset;
            journal.GetComponentInChildren<SingleInteractionVolume>().EnableInteraction();
            return journal.transform;
        }
    }
}
