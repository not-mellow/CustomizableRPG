using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;

namespace CommissionMod
{
    class SettingsWindow : MonoBehaviour
    {
        private static GameObject settingContents;
        private static GameObject scrollView;

        public static void init()
        {
            UI.addNewWindow("commissionSettings", "Settings Window");
            settingContents = UI.windowContents["commissionSettings"];
            scrollView = UI.windowScrollView["commissionSettings"];

            addSettingsOptions();
        }

        private static void addSettingsOptions()
        {
            RectTransform settingRect = settingContents.GetComponent<RectTransform>();
            settingRect.sizeDelta += new Vector2(0, Traits.talentIDs.Count*220);

            // GameObject mainTab = GameObjects.FindEvenInactive("Tab_Main");
            GameObject levelRateHolder = UI.createInputOption(
                "levelRate",
                "Level Rain Value",
                "Modify The Value Of Each Level Rain",
                -60,
                settingContents,
                "10"
            );
            // RectTransform levelRateRect = levelRateHolder.GetComponent<RectTransform>();
            // levelRateRect.localPosition = new Vector3(750, 80);
            UI.createInputOption(
                "ageIncreaseOption",
                "Age Level Increase",
                "Modify How Many Years Are Added\nTo Units That Gained A New Level In The 10s",
                -120,
                settingContents,
                "250"
            );
            
            UI.createInputOption(
                "levelCapOption",
                "Unit Level Cap",
                "Modify The Max Level A Unit Can Reach.\nIf The Value is -1, The Max Level Will Be Infinite!",
                -180,
                settingContents,
                "120"
            );

            UI.createInputOption(
                "expGapOption",
                "Unit Experience Gap",
                "Modify How The Experience Gap Between Levels!\nIf the is -1, it will revert back to the normal exp gap.",
                -240,
                settingContents,
                "250"
            );

            UI.createStatOption(
                "Stat Boost Per Level Leap",
                new List<string>{"Damage", "Armor", "Critical", "Attack Speed", "Health", "Multiplier"},
                -340,
                new List<string>{"75", "0", "5", "5", "600", "0.0"},
                settingContents
            );

            UI.createStatOption(
                "DMG Reduct Per LVL Leap",
                new List<string>{"InitialLevel", "DMGReductionPercent"},
                -460,
                new List<string>{"10", "0.5"},
                settingContents
            );

            UI.createInputOption(
                "getHitOption",
                "Exp Gained From Getting Hit",
                "Modify How Much Exp A Unit Gets When Hit By Someone.",
                -540,
                settingContents,
                "1"
            );

            int index = 0;
            foreach(KeyValuePair<string, SavedTrait> kv in Traits.talentIDs)
            {
                UI.createTraitOption(
                    kv.Key,
                    kv.Value,
                    -630 + (-index*160),
                    settingContents
                );
                index++;
            }

            Button saveButton = UI.createBGButton(
                scrollView,
                50,
                "Save",
                "Save Changes",
                "Save The Changes To The Settings File"
            );
            saveButton.onClick.AddListener(Main.saveStats);
        }
    }
}