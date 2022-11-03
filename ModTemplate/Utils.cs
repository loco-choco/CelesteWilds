using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CelesteWilds.TutorialText;
using OWML.ModHelper;

namespace CelesteWilds
{
    public static class Utils
    {
        public static Vector3 CalculateMatchVelocity(OWRigidbody owRigidbody, OWRigidbody bodyToMatch, bool ignoreAngularVelocity = false)
        {
            if (bodyToMatch == null)
                return Vector3.zero;

            if (ignoreAngularVelocity)
                return bodyToMatch.GetVelocity();

            return bodyToMatch.GetPointVelocity(owRigidbody.GetCenterOfMass());

        }

        public static Transform PlayerAttachedPoint()
        {
            Transform playerParent = Locator.GetPlayerTransform().parent;
            if (playerParent == null)
                return null;

            if (playerParent.GetComponent<PlayerAttachPoint>() != null)
                return playerParent;

            return null;
        }

        public static void AddFolderOfTranslationTables(ILocalizationAPI api, ModBehaviour mod, params string[] filesPath)
        {
            string pathOfFiles = Path.Combine(filesPath);
            for (int i = 0; i < TextTranslation.s_langFolder.Length; i++)
            {
                string endOfFileName = TextTranslation.s_langFolder[i] + ".xml";
                string completePossibleFilePath = pathOfFiles + endOfFileName;
                if (File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, completePossibleFilePath)))
                {
                    api.AddTranslation(mod, ((TextTranslation.Language)i).ToString(), completePossibleFilePath);
                }
            }
        }
        public static TextTranslation.TranslationTable_XML StringToTranslationTableXML(TextAsset xmlInAsset)
        {
            string xml = OWUtilities.RemoveByteOrderMark(xmlInAsset);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("TranslationTable_XML");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("entry");
            TextTranslation.TranslationTable_XML translationTable_XML = new TextTranslation.TranslationTable_XML();
            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode2 = (XmlNode)obj;
                translationTable_XML.table.Add(new TextTranslation.TranslationTableEntry(xmlNode2.SelectSingleNode("key").InnerText, xmlNode2.SelectSingleNode("value").InnerText));
            }
            foreach (object obj2 in xmlNode.SelectSingleNode("table_shipLog").SelectNodes("TranslationTableEntry"))
            {
                XmlNode xmlNode3 = (XmlNode)obj2;
                translationTable_XML.table_shipLog.Add(new TextTranslation.TranslationTableEntry(xmlNode3.SelectSingleNode("key").InnerText, xmlNode3.SelectSingleNode("value").InnerText));
            }
            foreach (object obj3 in xmlNode.SelectSingleNode("table_ui").SelectNodes("TranslationTableEntryUI"))
            {
                XmlNode xmlNode4 = (XmlNode)obj3;
                translationTable_XML.table_ui.Add(new TextTranslation.TranslationTableEntryUI(int.Parse(xmlNode4.SelectSingleNode("key").InnerText), xmlNode4.SelectSingleNode("value").InnerText));
            }
            return translationTable_XML;
        }
    }
}
