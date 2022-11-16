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
    class LeaderBoard : MonoBehaviour
    {
        private static GameObject boardContents;
        private static GameObject scrollView;
        private static Dictionary<string, int> talentPriority = new Dictionary<string, int>
        {
            {"Zeta", 12},
            {"Sigma", 11},
            {"Omega", 10},
            {"EXrank", 13},
            {"SSSrank", 9},
            {"SSrank", 8},
            {"Srank", 7},
            {"Arank", 6},
            {"Brank", 5},
            {"Crank", 4},
            {"Drank", 3},
            {"Erank", 2},
            {"Frank", 1},
        };
        public static List<ToggleIcon> toggles = new List<ToggleIcon>();
        public static Dictionary<string, bool> filterToggles = new Dictionary<string, bool>();

        public static void init()
        {
            addLeaderBoardWindow("leaderBoardWindow", "Talent Leader Board");
            Button levelButton = createBGButton(scrollView, "levels", 50, "Levels", "LevelsFilter", "Search Units By Level");
            levelButton.onClick.AddListener(() => searchUnits("levels", getUnitList()));
            Button talentButton = createBGButton(scrollView, "talents", 20, "Talents", "TalentsFilter", "Search Units By Talent");
            talentButton.onClick.AddListener(() => searchUnits("talents", getUnitList()));
            Button killsButton = createBGButton(scrollView, "kills", -10, "Kills", "KillsFilter", "Search Units By Kills");
            killsButton.onClick.AddListener(() => searchUnits("kills", getUnitList()));
            Button filterButton = createBGButton(scrollView, "kingdomFilter", -40, "Talents", "KingdomFilter", "Search Units By Filter");
            filterButton.onClick.AddListener(FilterWindow.openKingdomWindow);
        }

        private static List<Actor> getUnitList()
        {
            if (FilterWindow.kingdomFilter == null)
            {
                return MapBox.instance.units.getSimpleList();
            }
            return FilterWindow.kingdomFilter.units.getSimpleList();
        }

        public static void openWindow()
        {
            Windows.ShowWindow("leaderBoardWindow");
        }

        public static void searchUnits(string sortName, List<Actor> unitList)
        {
            foreach(Transform child in boardContents.transform)
            {
                Destroy(child.gameObject);
            }
            List<Actor> filteredList = sortUnits(sortName, unitList);
            RectTransform contentRect = boardContents.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0, 207 + (filteredList.Count * 70));
            for(int i = 0; i < filteredList.Count; i++)
            {
                Actor actor = filteredList[i];
                if (actor == null)
                {
                    continue;
                }
                addNewWindowElement(i, actor, sortName);
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
                    string talent = hasTalent(actor2.data);
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

        private static string hasTalent(ActorStatus status)
        {
            foreach(KeyValuePair<string, SavedTrait> kv in Traits.talentIDs)
            {
                if (status.haveTrait(kv.Key))
                {
                    return kv.Key;
                }
            }
            return null;
        }

        public static void addNewWindowElement(int i, Actor actor, string sorting)
        {
            GameObject bgElement = new GameObject("WindowElement");
            bgElement.transform.SetParent(boardContents.transform);
            Image bgImage = bgElement.AddComponent<Image>();
            bgImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowInnerSliced.png");
            RectTransform bgRect = bgElement.GetComponent<RectTransform>();
            bgRect.localPosition = new Vector3(130, -40 + (i*-60), 0);
            bgRect.sizeDelta = new Vector2(450, 140);

            GameObject avatar = createAvatar(actor, bgElement, 30,  new Vector3(-50, -70, 0));

            int nameSize = 30;
            if (actor.getName().Length > 6)
            {
                nameSize = 15;
            }
            GameObject nameText = UI.addText($"{actor.getName()}", bgElement, 30, new Vector3(-80, 40, 0)).gameObject;

            int posY = 5;
            GameObject numText = UI.addText($"{(i+1).ToString()}.", bgElement, 50, new Vector3(-200, -5, 0)).gameObject;
            switch(sorting)
            {
                case "talents":
                    string[] talent = hasTalent(actor.data).Split('r');
                    GameObject talentText = UI.addText($"{talent[0]}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
                    break;
                case "levels":
                    GameObject levelsText = UI.addText($"{actor.data.level}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
                    break;
                case "kills":
                    GameObject killsText = UI.addText($"{actor.data.kills}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
                    break;
                default:
                    string[] actortalent = hasTalent(actor.data).Split('r');
                    GameObject defaultText = UI.addText($"{actortalent[0]}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
                    break;
            }
        }

        public static GameObject createAvatar(Actor actor, GameObject parent, int size, Vector3 pos)
        {
            if (actor == null)
            {
                var ghostObject = new GameObject("ghostAvatar");
                ghostObject.transform.SetParent(parent.transform);
                RectTransform ghostRectTransform = ghostObject.AddComponent<RectTransform>();
                ghostRectTransform.localPosition = pos + new Vector3(0, 30, 0);
                ghostRectTransform.sizeDelta = new Vector2(size*2, size*2);
                Image img = ghostObject.AddComponent<Image>();
                img.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.ghost_icon.png");
                return ghostObject;
            }
            var avatarObject = new GameObject("avatar");
            avatarObject.transform.SetParent(parent.transform);
            RectTransform rectTransform = avatarObject.AddComponent<RectTransform>();
            rectTransform.localPosition = pos;
            rectTransform.sizeDelta = new Vector2(100, 100);
            var avatarLoader = avatarObject.AddComponent<UnitAvatarLoader>() as UnitAvatarLoader;
            var avatarButton = avatarObject.AddComponent<Button>() as Button;
            avatarButton.onClick.AddListener(() => avatarOnClick(actor));
            avatarLoader.load(actor);
            GameObject avatarImage = avatarObject.transform.GetChild(0).gameObject;
            GameObject avatarItem = null;
            if (avatarObject.transform.GetChildCount() == 2)
            {
                avatarItem = avatarObject.transform.GetChild(1).gameObject;
            }
            RectTransform avatarImageRect = avatarImage.GetComponent<RectTransform>();
            avatarImageRect.localPosition = new Vector3(0, 0, 0);
            avatarImageRect.sizeDelta = new Vector2(size, size);
            if (avatarItem != null)
            {
                RectTransform avatarItemRect = avatarItem.GetComponent<RectTransform>();
                avatarItemRect.localPosition = new Vector3(-size/3, size/2, 0);
                avatarItemRect.sizeDelta = new Vector2(size/3, size/2);
            }
            return avatarObject;
        }

        private static void avatarOnClick(Actor actor)
        {
            Config.selectedUnit = actor;
            Windows.ShowWindow("inspect_unit");
        }

        private static void addLeaderBoardWindow(string id, string title)
        {
            ScrollWindow window;
            window = Windows.CreateNewWindow(id, title);

            scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);

            boardContents = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
        }

        public static Button createBGButton(GameObject parent, string name, int posY, string iconName, string buttonName, string buttonDesc)
        {
            PowerButton button = PowerButtons.CreateButton(
                buttonName,
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.icon{iconName}.png"),
                buttonName,
                buttonDesc,
                new Vector2(118, posY),
                ButtonType.Click,
                parent.transform,
                null
            );

            Image buttonBG = button.gameObject.GetComponent<Image>();
            buttonBG.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.backgroundTabButton.png");
            Button buttonButton = button.gameObject.GetComponent<Button>();

            GameObject toggleHolder = new GameObject("ToggleIcon");
            toggleHolder.transform.SetParent(button.gameObject.transform);
            Image toggleImage = toggleHolder.AddComponent<Image>();
            ToggleIcon toggleIcon = toggleHolder.AddComponent<ToggleIcon>();
            toggleIcon.spriteON = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.buttonToggleIndicator0.png");
            toggleIcon.spriteOFF = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.buttonToggleIndicator1.png");
            toggleIcon.updateIcon(false);

            RectTransform toggleRect = toggleHolder.GetComponent<RectTransform>();
            toggleRect.localPosition = new Vector3(0, 12, 0);
            toggleRect.sizeDelta = new Vector2(8, 7);

            toggles.Add(toggleIcon);
            filterToggles.Add(name, false);
            buttonButton.onClick.AddListener(() => checkToggledIcon(toggleIcon, name));

            return buttonButton;
        }

        public static void checkToggledIcon(ToggleIcon toggleIcon, string name)
        {
            foreach(ToggleIcon toggle in toggles)
            {
                toggle.updateIcon(false);
            }
            toggleIcon.updateIcon(true);
            for(int i = 0; i < filterToggles.Count; i++)
            {
                KeyValuePair<string, bool> kv = filterToggles.ElementAt(i);
                filterToggles[kv.Key] = false;
            }
            filterToggles[name] = true;
        }
    }
}