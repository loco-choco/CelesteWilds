using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CelesteWilds
{
    public static class Utils
    {
        static Utils()
        {
            TextTranslation.Get().OnLanguageChanged += Utils_OnLanguageChanged;
            translationTables = new Dictionary<TextTranslation.Language, List<TextTranslation.TranslationTable>>();
        }

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

        private static Dictionary<TextTranslation.Language, List<TextTranslation.TranslationTable>> translationTables;

        private static void Utils_OnLanguageChanged()
        {
            if (!translationTables.ContainsKey(TextTranslation.Get().m_language))
                return;

            var listToAdd = translationTables[TextTranslation.Get().m_language];

            foreach (var table in listToAdd)
            {
                foreach (var pair in table.theTable)
                    TextTranslation.Get().m_table.theTable[pair.Key] = pair.Value;

                foreach (var pair in table.theShipLogTable)
                    TextTranslation.Get().m_table.theShipLogTable[pair.Key] = pair.Value;

                foreach (var pair in table.theUITable)
                    TextTranslation.Get().m_table.theUITable[pair.Key] = pair.Value;
            }
        }
        public static void AddFolderOfTranslationTables(params string[] filesPath)
        {
            string pathOfFiles = Path.Combine(filesPath);
            for (int i = 0; i < TextTranslation.s_langFolder.Length; i++)
            {
                string endOfFileName = TextTranslation.s_langFolder[i] + ".xml";
                string completePossibleFilePath = pathOfFiles + endOfFileName;
                if (File.Exists(completePossibleFilePath))
                {
                    string xmlFile = File.ReadAllText(completePossibleFilePath);
                    var xmlInAsset = new TextAsset(xmlFile);
                    AddTranslationTable(xmlInAsset, (TextTranslation.Language)i);
                }
            }
        }

        public static void AddTranslationTable(TextAsset xmlInAsset, TextTranslation.Language language)
        {
            TextTranslation.TranslationTable_XML tableXml = StringToTranslationTableXML(xmlInAsset);
            var newTable = new TextTranslation.TranslationTable(tableXml);

            if (translationTables.ContainsKey(language))
                translationTables[language].Add(newTable);
            else
                translationTables[language] = new List<TextTranslation.TranslationTable> { newTable };
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
