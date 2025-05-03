using Deadpan.Enums.Engine.Components.Modding;
using FMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Localization.Tables;
using UnityEngine;

namespace Spirefrost
{
    internal class SpirefrostStrings
    {
        public static string TribeTitleKey => MainModFile.instance.GUID + ".TribeTitle";
        public static string TribeDescKey => MainModFile.instance.GUID + ".TribeDesc";

        public static string ToolboxTitle => MainModFile.instance.GUID + ".ToolboxTitle";
        public static string GoldenEyeTitle => MainModFile.instance.GUID + ".GoldenEyeTitle";

        //Call this method in Load()
        internal static void CreateLocalizedStrings()
        {
            StringTable uiText = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);
            uiText.SetString(TribeTitleKey, "The Ascenders"); //Create the title
            uiText.SetString(TribeDescKey, "Denizens of the spire have formed an unlikely team after finding themselves in an unknown place. " +
                "\n\n" +
                "Well versed in defending themselves, they whittle their enemies down to win the war of attrition."); //Create the description.
            uiText.SetString(ToolboxTitle, "Add an item into your hand");
            uiText.SetString(GoldenEyeTitle, "Draw a card");
        }
    }
}
