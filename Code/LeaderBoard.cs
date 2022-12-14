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
            {"LionRank", 3},
            {"BearRank", 2},
            {"WolfRank", 1},
        };
        public static Dictionary<ToggleIcon, string> toggles = new Dictionary<ToggleIcon, string>();
        public static Dictionary<string, bool> filterToggles = new Dictionary<string, bool>();

        public static void init()
        {
            addLeaderBoardWindow("leaderBoardWindow", "Talent Leader Board");
            Button levelButton = createBGButton(scrollView, "levels", 80, "Levels", "LevelsFilter", "Search Units By Level");
            levelButton.onClick.AddListener(() => searchUnits("levels", getUnitList()));
            Button talentButton = createBGButton(scrollView, "talents", 50, "Talents", "TalentsFilter", "Search Units By Talent");
            talentButton.onClick.AddListener(() => searchUnits("talents", getUnitList()));
            Button killsButton = createBGButton(scrollView, "kills", 20, "Kills", "KillsFilter", "Search Units By Kills");
            killsButton.onClick.AddListener(() => searchUnits("kills", getUnitList()));
            Button ageButton = createBGButton(scrollView, "age", -10, "Age", "AgeFilter", "Search Units By Age");
            ageButton.onClick.AddListener(() => searchUnits("age", getUnitList()));
            Button dmgButton = createBGButton(scrollView, "dmg", -40, "DMG", "DMGFilter", "Search Units By Damage");
            dmgButton.onClick.AddListener(() => searchUnits("dmg", getUnitList()));
            Button kingdomFilterButton = createBGButton(scrollView, "kingdomFilter", -70, "GoldCrown", "KingdomFilter", "Search Units By Kingdom Filter");
            kingdomFilterButton.onClick.AddListener(FilterWindow.openKingdomWindow);
            Button cityFilterButton = createBGButton(scrollView, "cityFilter", -100, "SilverCrown", "CityFilter", "Search Units By City Filter");
            cityFilterButton.onClick.AddListener(FilterWindow.openCityWindow);
        }

        private static List<Actor> getUnitList()
        {
            if (FilterWindow.kingdomFilter != null)
            {
                return FilterWindow.kingdomFilter.units.getSimpleList();
            }
            else if (FilterWindow.cityFilter != null)
            {
                return FilterWindow.cityFilter.units.getSimpleList();
            }
            return MapBox.instance.units.getSimpleList();
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

            GameObject avatar = createAvatar(actor, bgElement, 30,  new Vector3(-110, -70, 0));

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
                case "age":
                    GameObject ageText = UI.addText($"{actor.data.age}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
                    break;
                case "dmg":
                    GameObject dmgText = UI.addText($"{actor.curStats.damage}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
                    break;
                default:
                    string[] actortalent = hasTalent(actor.data).Split('r');
                    GameObject defaultText = UI.addText($"{actortalent[0]}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
                    break;
            }
            createBanner(bgElement, actor.city, new Vector2(80, 80), new Vector3(0, 0, 0));
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

        public static GameObject createBanner(GameObject parent, City city, Vector2 size, Vector3 pos)
        {
            if (city == null)
            {
                return null;
            }
            Kingdom kingdom = city.kingdom;
            GameObject bannerGO = new GameObject("bannerHolder");
            bannerGO.transform.SetParent(parent.transform);
            bannerGO.AddComponent<CanvasRenderer>();
            bannerGO.transform.localPosition = pos;

            if (kingdom == null)
            {
                return null;
            }
            else if (kingdom.id == "nomads_human" || kingdom.id == "nomads_elf" || kingdom.id == "nomads_orc" || kingdom.id == "nomads_dwarf" || kingdom.id == "mad")
            {
                return null;
            }

            GameObject backgroundGO = new GameObject("background");
            backgroundGO.transform.SetParent(bannerGO.transform);
            Image backgroundImage = backgroundGO.AddComponent<Image>();
            RectTransform bgRect = backgroundGO.GetComponent<RectTransform>();
            bgRect.localPosition = new Vector3(0, 0, 0);
            bgRect.sizeDelta = size;

            GameObject iconGO = new GameObject("icon");
            iconGO.transform.SetParent(bannerGO.transform);
            Image iconImage = iconGO.AddComponent<Image>();
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.localPosition = new Vector3(0, 0, 0);
            iconRect.sizeDelta = size;

            BannerLoader bannerLoader = bannerGO.AddComponent<BannerLoader>();
            bannerLoader.partIcon = iconImage;
            bannerLoader.partBackround = backgroundImage;
            bannerLoader.load(kingdom);

            Button bannerButton = bannerGO.AddComponent<Button>();
            bannerButton.onClick.AddListener(() => bannerOnClick(city));

            UI.addText(city.data.cityName, bannerGO, 20, new Vector3(0, -50, 0));

            return bannerGO;
        }

        private static void bannerOnClick(City city)
        {
            Config.selectedCity = city;
            Windows.ShowWindow("village");
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

            toggles.Add(toggleIcon, name);
            filterToggles.Add(name, false);
            buttonButton.onClick.AddListener(() => checkToggledIcon(toggleIcon, name));

            return buttonButton;
        }

        public static void checkToggledIcon(ToggleIcon toggleIcon, string name)
        {
            for(int i = 0; i < filterToggles.Count; i++)
            {
                KeyValuePair<string, bool> kv = filterToggles.ElementAt(i);
                filterToggles[kv.Key] = false;
            }
            if (name == "kingdomFilter" || name == "cityFilter")
            {
                return;
            }
            filterToggles[name] = true;
            updateToggledIcons();
            toggleIcon.updateIcon(true);
        }

        public static void updateToggledIcons()
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
                if ((kv.Value == "kingdomFilter" && FilterWindow.kingdomFilter != null) || (kv.Value == "cityFilter" && FilterWindow.cityFilter != null) || trueFilters.Contains(kv.Value))
                {
                    kv.Key.updateIcon(true);
                    continue;
                }
                kv.Key.updateIcon(false);
            }
        }
    }
}