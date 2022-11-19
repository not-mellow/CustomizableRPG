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
    class FilterWindow : MonoBehaviour
    {
        private static GameObject filterContents;
        private static GameObject scrollView;
        public static List<ToggleIcon> toggles = new List<ToggleIcon>();
        public static Kingdom kingdomFilter;
        public static City cityFilter;

        public static void init()
        {
            addFilterWindow("filterWindow", "Filters Window");
        }

        public static void openKingdomWindow()
        {
            // Delete all buttons in window
            foreach(Transform child in filterContents.transform)
            {
                Destroy(child.gameObject);
            }
            if (MapBox.instance.kingdoms.list_civs.Count > 16)
            {
                filterContents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, (MapBox.instance.kingdoms.list_civs.Count/4)*50);
            }
            toggles.Clear();
            int posY = 0;
            int posX = 0;
            // Loop Through Kingdoms List To Make Buttons
            for (int l = 0; l < MapBox.instance.kingdoms.list_civs.Count; l++)
            {
                Kingdom kingdom = MapBox.instance.kingdoms.list_civs[l];
                if (posX >= 4)
                {
                    posY++;
                    posX = 0;
                }
                // Adds PowerButtons To Window
                createFilterButton(filterContents, "kingdom", new Vector2(50+(posX*50),-50+(-posY*40)), kingdom.name, "Click on this kingdom as a filter", kingdom, null);
                posX++;
            }
            if (posX >= 4)
            {
                posY++;
                posX = 0;
            }
            createNoFilterButton(filterContents, new Vector2(50+(posX*50),-50+(-posY*40)), "No Filter", "Click on this kingdom as a filter");
            // Open Window
            Windows.ShowWindow("filterWindow");
        }

        public static void openCityWindow()
        {
            // Delete all buttons in window
            foreach(Transform child in filterContents.transform)
            {
                Destroy(child.gameObject);
            }
            if (MapBox.instance.citiesList.Count > 16)
            {
                filterContents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, (MapBox.instance.citiesList.Count/4)*50);
            }
            toggles.Clear();
            int posY = 0;
            int posX = 0;
            // Loop Through Kingdoms List To Make Buttons
            for (int l = 0; l < MapBox.instance.citiesList.Count; l++)
            {
                City city = MapBox.instance.citiesList[l];
                if (posX >= 4)
                {
                    posY++;
                    posX = 0;
                }
                // Adds PowerButtons To Window
                createFilterButton(filterContents, "city", new Vector2(50+(posX*50),-50+(-posY*40)), city.data.cityName, "Click on this city as a filter", city.kingdom, city);
                posX++;
            }
            if (posX >= 4)
            {
                posY++;
                posX = 0;
            }
            createNoFilterButton(filterContents, new Vector2(50+(posX*50),-50+(-posY*40)), "No Filter", "Click on this kingdom as a filter");
            // Open Window
            Windows.ShowWindow("filterWindow");
        }

        private static Button createFilterButton(GameObject parent, string name, Vector2 pos, string buttonName, string buttonDesc, Kingdom kingdom, City city)
        {
            // Check if the localization for button has already been added then remove it
            if (LocalizedTextManager.instance.localizedText.ContainsKey($"{buttonName}_dej_button"))
            {
                LocalizedTextManager.instance.localizedText.Remove($"{buttonName}_dej_button");
                LocalizedTextManager.instance.localizedText.Remove($"{buttonName}_dej_button Description");
                PowerButtons.CustomButtons.Remove($"{buttonName}_dej_button");
            }
            BannerContainer bannerContainer = BannerGenerator.dict[kingdom.race.banner_id];
            Sprite backgroundBanner = bannerContainer.backrounds[kingdom.banner_background_id];

            // Create button
            PowerButton button = PowerButtons.CreateButton(
                $"{buttonName}_dej_button",
                // Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.icon{iconName}.png"),
                backgroundBanner,
                buttonName,
                buttonDesc,
                pos,
                ButtonType.Click,
                parent.transform,
                null
            );
            Image bannerBG = button.gameObject.transform.Find("Icon").GetComponent<Image>();
            bannerBG.color = kingdom.kingdomColor.colorBorderOut;
            Button buttonButton = button.gameObject.GetComponent<Button>();

            GameObject bannerIcon = new GameObject("BannerIcon");
            bannerIcon.transform.SetParent(button.gameObject.transform);
            Image bannerIconImg = bannerIcon.AddComponent<Image>();
            bannerIconImg.sprite = bannerContainer.icons[kingdom.banner_icon_id];
            bannerIconImg.color = kingdom.kingdomColor.colorBorderBannerIcon;
            RectTransform bannerIconRect = bannerIcon.GetComponent<RectTransform>();
            bannerIconRect.localPosition = new Vector3(0, 0, 0);
            bannerIconRect.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            //Make a new GameObject that will hold the toggle icon of the button
            GameObject toggleHolder = new GameObject("ToggleIcon");
            toggleHolder.transform.SetParent(button.gameObject.transform);
            Image toggleImage = toggleHolder.AddComponent<Image>();
            ToggleIcon toggleIcon = toggleHolder.AddComponent<ToggleIcon>();
            toggleIcon.spriteON = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.buttonToggleIndicator0.png");
            toggleIcon.spriteOFF = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.buttonToggleIndicator1.png");
            toggleIcon.updateIcon(false);

            //Change the size of the toggle icon
            RectTransform toggleRect = toggleHolder.GetComponent<RectTransform>();
            toggleRect.localPosition = new Vector3(0, 15, 0);
            toggleRect.sizeDelta = new Vector2(8, 7);

            RectTransform buttonRect = button.gameObject.GetComponent<RectTransform>();
            buttonRect.localScale = Vector3.one;

            // Add listeners
            toggles.Add(toggleIcon);
            switch(name)
            {
                case "kingdom":
                    buttonButton.onClick.AddListener(() => checkKingdomToggledIcon(toggleIcon, kingdom));
                    break;
                case "city":
                    buttonButton.onClick.AddListener(() => checkCityToggledIcon(toggleIcon, city));
                    break;
            }

            return buttonButton;
        }

        private static void checkKingdomToggledIcon(ToggleIcon toggleIcon, Kingdom kingdom)
        {
            if (kingdomFilter == kingdom)
            {
                kingdomFilter = null;
                cityFilter = null;
                toggleIcon.updateIcon(false);
                return;
            }
            foreach(ToggleIcon toggle in toggles)
            {
                toggle.updateIcon(false);
            }
            toggleIcon.updateIcon(true);
            kingdomFilter = kingdom;
            cityFilter = null;
            string sortName = getSortName();
            LeaderBoard.updateToggledIcons();
            LeaderBoard.searchUnits(sortName, kingdom.units.getSimpleList());
        }

        private static Button createNoFilterButton(GameObject parent, Vector2 pos, string buttonName, string buttonDesc)
        {
            // Check if the localization for button has already been added then remove it
            if (LocalizedTextManager.instance.localizedText.ContainsKey($"{buttonName}_dej_button"))
            {
                LocalizedTextManager.instance.localizedText.Remove($"{buttonName}_dej_button");
                LocalizedTextManager.instance.localizedText.Remove($"{buttonName}_dej_button Description");
                PowerButtons.CustomButtons.Remove($"{buttonName}_dej_button");
            }

            // Create button
            PowerButton button = PowerButtons.CreateButton(
                $"{buttonName}_dej_button",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.iconDelete.png"),
                buttonName,
                buttonDesc,
                pos,
                ButtonType.Click,
                parent.transform,
                null
            );
            Button buttonButton = button.gameObject.GetComponent<Button>();

            //Make a new GameObject that will hold the toggle icon of the button
            GameObject toggleHolder = new GameObject("ToggleIcon");
            toggleHolder.transform.SetParent(button.gameObject.transform);
            Image toggleImage = toggleHolder.AddComponent<Image>();
            ToggleIcon toggleIcon = toggleHolder.AddComponent<ToggleIcon>();
            toggleIcon.spriteON = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.buttonToggleIndicator0.png");
            toggleIcon.spriteOFF = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.buttonToggleIndicator1.png");
            toggleIcon.updateIcon(false);

            //Change the size of the toggle icon
            RectTransform toggleRect = toggleHolder.GetComponent<RectTransform>();
            toggleRect.localPosition = new Vector3(0, 15, 0);
            toggleRect.sizeDelta = new Vector2(8, 7);

            RectTransform buttonRect = button.gameObject.GetComponent<RectTransform>();
            buttonRect.localScale = Vector3.one;

            // Add listeners
            toggles.Add(toggleIcon);
            buttonButton.onClick.AddListener(() => removeFilters(toggleIcon));

            return buttonButton;
        }

        private static void removeFilters(ToggleIcon toggleIcon)
        {
            foreach(ToggleIcon toggle in toggles)
            {
                toggle.updateIcon(false);
            }
            toggleIcon.updateIcon(true);
            kingdomFilter = null;
            cityFilter = null;
            string sortName = getSortName();
            LeaderBoard.updateToggledIcons();
            LeaderBoard.searchUnits(sortName, MapBox.instance.units.getSimpleList());
        }

        private static void checkCityToggledIcon(ToggleIcon toggleIcon, City city)
        {
            if (cityFilter == city)
            {
                kingdomFilter = null;
                cityFilter = null;
                toggleIcon.updateIcon(false);
                return;
            }
            foreach(ToggleIcon toggle in toggles)
            {
                toggle.updateIcon(false);
            }
            toggleIcon.updateIcon(true);
            cityFilter = city;
            kingdomFilter = null;
            string sortName = getSortName();
            LeaderBoard.updateToggledIcons();
            LeaderBoard.searchUnits(sortName, city.units.getSimpleList());
        }

        private static string getSortName()
        {
            string sortName = "nothing";
            for(int i = 0; i < LeaderBoard.filterToggles.Count; i++)
            {
                KeyValuePair<string, bool> kv = LeaderBoard.filterToggles.ElementAt(i);
                if (LeaderBoard.filterToggles[kv.Key])
                {
                    sortName = kv.Key;
                }
            }
            return sortName;
        }

        private static void addFilterWindow(string id, string title)
        {
            ScrollWindow window;
            window = Windows.CreateNewWindow(id, title);

            scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);

            filterContents = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
        }
    }
}