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
    class BeastBoardWindow : MonoBehaviour
    {
        private static GameObject beastContents;
        private static GameObject scrollView;
        public static Dictionary<ToggleIcon, string> toggles = new Dictionary<ToggleIcon, string>();
        public static Dictionary<string, bool> filterToggles = new Dictionary<string, bool>();
        private static Dictionary<string, int> talentPriority = new Dictionary<string, int>
        {
            {"Zeta", 15},
            {"Sigma", 14},
            {"Omega", 13},
            {"EXrank", 16},
            {"SSSrank", 12},
            {"SSrank", 11},
            {"Srank", 10},
            {"Arank", 9},
            {"Brank", 8},
            {"Crank", 7},
            {"Drank", 6},
            {"Erank", 5},
            {"Frank", 4},
            {"Lionrank", 3},
            {"Bearrank", 2},
            {"Wolfrank", 1},
        };

        public static void init()
        {
            UI.addNewWindow("beastBoardWindow", "Beast Board Window");
            beastContents = UI.windowContents["beastBoardWindow"];
            scrollView = UI.windowScrollView["beastBoardWindow"];

            Button levelButton = createLeaderBoardFilter(scrollView, "levels", 80, "Levels", "BeastLevelsFilter", "Search Beasts By Level");
            levelButton.onClick.AddListener(() => searchUnits("levels", MapBox.instance.units.getSimpleList()));
            Button talentButton = createLeaderBoardFilter(scrollView, "talents", 50, "Talents", "BeastTalentsFilter", "Search Beasts By Talent");
            talentButton.onClick.AddListener(() => searchUnits("talents", MapBox.instance.units.getSimpleList()));
            Button killsButton = createLeaderBoardFilter(scrollView, "kills", 20, "Kills", "BeastKillsFilter", "Search Beasts By Kills");
            killsButton.onClick.AddListener(() => searchUnits("kills", MapBox.instance.units.getSimpleList()));
            Button ageButton = createLeaderBoardFilter(scrollView, "age", -10, "Age", "BeastAgeFilter", "Search Beasts By Age");
            ageButton.onClick.AddListener(() => searchUnits("age", MapBox.instance.units.getSimpleList()));
            Button dmgButton = createLeaderBoardFilter(scrollView, "dmg", -40, "DMG", "BeastDMGFilter", "Search Beasts By Damage");
            dmgButton.onClick.AddListener(() => searchUnits("dmg", MapBox.instance.units.getSimpleList()));
        }

        public static void searchUnits(string sortName, List<Actor> unitList)
        {
            foreach(Transform child in beastContents.transform)
            {
                Destroy(child.gameObject);
            }
            List<Actor> filteredList = sortUnits(sortName, unitList);
            RectTransform contentRect = beastContents.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0, 207 + (filteredList.Count * 70));
            for(int i = 0; i < filteredList.Count; i++)
            {
                Actor actor = filteredList[i];
                if (actor == null)
                {
                    continue;
                }
                UI.addNewWindowElement(beastContents, i, actor, sortName);
            }
        }

        public static List<Actor> sortUnits(string sortName, List<Actor> unitList)
        {
            List<Actor> copiedList = unitList.ToList();
            List<Actor> filteredList = new List<Actor>();
            int listCount = unitList.Count;
            for(int i = 0; i < listCount; i++)
            {
                if (filteredList.Count >= 100)
                {
                    break;
                }
                Actor actor1 = null;
                int num = 0;
                foreach(Actor actor2 in copiedList)
                {
                    if (!actor2.stats.animal)
                    {
                        continue;
                    }
                    string talent = Main.hasTalent(actor2.data);
                    if (talent == null)
                    {
                        continue;
                    }
                    int num2;
                    switch(sortName)
                    {
                        case "talents":
                            num2 = talentPriority[talent];
                            break;
                        case "levels":
                            num2 = actor2.data.level;
                            break;
                        case "kills":
                            num2 = actor2.data.kills;
                            break;
                        case "nothing":
                            num2 = 0;
                            break;
                        case "age":
                            num2 = actor2.data.age;
                            break;
                        case "dmg":
                            num2 = actor2.curStats.damage;
                            break;
                        default:
                            return unitList;
                    }
                    if(actor1 != null && num2 == num)
                    {
                        int attribute1 = actor1.data.kills + actor1.curStats.damage + actor1.data.level;
                        int attribute2 = actor2.data.kills + actor2.curStats.damage + actor2.data.level;
                        if (attribute2 > attribute1)
                        {
                            actor1 = actor2;
                        }
                    }
                    else if (actor1 == null || num2 > num)
                    {
                        num = num2;
                        actor1 = actor2;
                    }
                }
                if (actor1 != null)
                {
                    filteredList.Add(actor1);
                    copiedList.Remove(actor1);
                }
            }
            return filteredList;
        }

        private static Button createLeaderBoardFilter(GameObject parent, string name, int posY, string iconName, string buttonName, string buttonDesc)
        {
            ToggleIcon toggleIcon = null;
            Button buttonButton = UI.createFilterBGButton(
                parent, 
                name, 
                posY, 
                iconName, 
                buttonName, 
                buttonDesc,
                ref toggleIcon
            );
            toggles.Add(toggleIcon, name);
            filterToggles.Add(name, false);

            buttonButton.onClick.AddListener(() => checkToggledIcon(toggleIcon, name));

            return buttonButton;
        }

        private static void checkToggledIcon(ToggleIcon toggleIcon, string name)
        {
            for(int i = 0; i < filterToggles.Count; i++)
            {
                KeyValuePair<string, bool> kv = filterToggles.ElementAt(i);
                filterToggles[kv.Key] = false;
            }
            filterToggles[name] = true;
            updateToggledIcons();
            toggleIcon.updateIcon(true);
        }

        private static void updateToggledIcons()
        {
            List<string> trueFilters = new List<string>();
            foreach(KeyValuePair<string, bool> kv in filterToggles)
            {
                if (filterToggles[kv.Key])
                {
                    trueFilters.Add(kv.Key);
                }
            }
            foreach(KeyValuePair<ToggleIcon, string> kv in toggles)
            {
                if (trueFilters.Contains(kv.Value))
                {
                    kv.Key.updateIcon(true);
                    continue;
                }
                kv.Key.updateIcon(false);
            }
        }
    }
}