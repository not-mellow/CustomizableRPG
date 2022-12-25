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
    class UI : MonoBehaviour
    {
        public static Dictionary<string, string> inputOptions = new Dictionary<string, string>();
        public static Dictionary<string, bool> boolOptions = new Dictionary<string, bool>();
        public static Dictionary<string, GameObject> windowContents = new Dictionary<string, GameObject>();
        public static Dictionary<string, GameObject> windowScrollView = new Dictionary<string, GameObject>();

        public static void init()
        {
            ScrollWindow.checkWindowExist("inspect_unit");
            addButtons();
        }

        private static void addButtons()
        {
            PowerButton limitButton = NCMS.Utils.PowerButtons.CreateButton(
                "statLimitsButton",
                NCMS.Utils.Sprites.LoadSprite($"{Mod.Info.Path}/icon.png"),
                "Stat Limiter",
                "Click Here To Limit Stats",
                new Vector2(0, 0),
                ButtonType.Click,
                null,
                () => Windows.ShowWindow("statLimitWindow")
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(limitButton, NCMS.Utils.PowerTab.Main, new Vector2(674, 18));

            PowerButton beastBoardButton = NCMS.Utils.PowerButtons.CreateButton(
                "beastBoardButton",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.iconLeaderBoardBeast.png"),
                "Beast Leader Board",
                "Click Here To See 100 Of The Most Talented Beasts In The World!",
                new Vector2(0, 0),
                ButtonType.Click,
                null,
                () => Windows.ShowWindow("beastBoardWindow")
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(beastBoardButton, NCMS.Utils.PowerTab.Main, new Vector2(674, -18));

            PowerButton settingsButton = NCMS.Utils.PowerButtons.CreateButton(
                "commissionModSettingsButton",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.iconSettings.png"),
                "Mod Settings",
                "Click Here To Change Settings Of The Commission Mod",
                new Vector2(0, 0),
                ButtonType.Click,
                null,
                () => Windows.ShowWindow("commissionSettings")
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(settingsButton, NCMS.Utils.PowerTab.Main, new Vector2(632, 18));

            PowerButton levelButton = NCMS.Utils.PowerButtons.CreateButton(
                "levelUp",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.iconLevels.png"),
                "Level Up Rain",
                "Create Rain That Raises The Level Of Any Unit",
                new Vector2(0, 0),
                ButtonType.GodPower,
                null,
                null
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(levelButton, NCMS.Utils.PowerTab.Main, new Vector2(590, -18));

            PowerButton leaderBoardButton = NCMS.Utils.PowerButtons.CreateButton(
                "leaderBoardButton",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.iconLeaderBoardCivs.png"),
                "Civ Leader Board",
                "Click Here To See 100 Of The Most Talented People In The World!",
                new Vector2(0, 0),
                ButtonType.Click,
                null,
                LeaderBoard.openWindow
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(leaderBoardButton, NCMS.Utils.PowerTab.Main, new Vector2(632, -18));

            PowerButton talentStatsButton = NCMS.Utils.PowerButtons.CreateButton(
                "talentStatsButton",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.iconTalentsButton.png"),
                "World Talent Stats",
                "Click Here To See The Current Stats Of Talents In The World!",
                new Vector2(0, 0),
                ButtonType.Click,
                null,
                WorldTalentStats.openWindow
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(talentStatsButton, NCMS.Utils.PowerTab.Main, new Vector2(590, 18));

            PowerButton levelStatsButton = NCMS.Utils.PowerButtons.CreateButton(
                "levelStatsButton",
                NCMS.Utils.Sprites.LoadSprite($"{Mod.Info.Path}/icon.png"),
                "World Level Stats",
                "Click Here To See The Current Stats of Levels In The World!",
                new Vector2(0, 0),
                ButtonType.Click,
                null,
                WorldLevelStats.openWindow
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(levelStatsButton, NCMS.Utils.PowerTab.Main, new Vector2(548, 18));

            PowerButton spLevelButton = NCMS.Utils.PowerButtons.CreateButton(
                "spLevelUp",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.iconLevels.png"),
                "Specific Level Up Rain",
                "Create Rain That Would Set Unit's Levels To A Specific Value!",
                new Vector2(0, 0),
                ButtonType.GodPower,
                null,
                null
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(spLevelButton, NCMS.Utils.PowerTab.Main, new Vector2(548, -18));
        }
        
        public static void addNewWindow(string id, string title)
        {
            ScrollWindow window = Windows.CreateNewWindow(id, title);

            GameObject pScrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            pScrollView.gameObject.SetActive(true);

            GameObject pContents = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
            windowContents.Add(id, pContents);
            windowScrollView.Add(id, pScrollView);
        }

        public static GameObject createInputOption(string objName, string title, string desc, int posY, GameObject parent, string textValue = "-1")
        {
            GameObject statHolder = new GameObject("OptionHolder");
            statHolder.transform.SetParent(parent.transform);
            Image statImage = statHolder.AddComponent<Image>();
            statImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowInnerSliced.png");
            RectTransform statHolderRect = statHolder.GetComponent<RectTransform>();
            statHolderRect.localPosition = new Vector3(130, posY, 0);
            statHolderRect.sizeDelta = new Vector2(400, 150);

            Text statText = addText(title, statHolder, 20, new Vector3(0, 20, 0));
            RectTransform statTextRect = statText.gameObject.GetComponent<RectTransform>();
            statTextRect.sizeDelta = new Vector2(statTextRect.sizeDelta.x, 80);

            Text descText = addText(desc, statHolder, 20, new Vector3(0, -60, 0));
            RectTransform descTextRect = descText.gameObject.GetComponent<RectTransform>();
            descTextRect.sizeDelta = new Vector2(descTextRect.sizeDelta.x, 80);

            GameObject inputRef = NCMS.Utils.GameObjects.FindEvenInactive("NameInputElement");

            GameObject inputField = Instantiate(inputRef, statHolder.transform);
            NameInput nameInputComp = inputField.GetComponent<NameInput>();
            if (Main.hasSettings)
            {
                // textValue = Main.savedStats.inputOptions[objName];
                textValue = Main.getSavedOption(objName, textValue);
            }
            nameInputComp.setText(textValue);
            RectTransform inputRect = inputField.GetComponent<RectTransform>();
            inputRect.localPosition = new Vector3(0,10,0);
            inputRect.sizeDelta += new Vector2(120, 40);

            GameObject inputChild = inputField.transform.Find("InputField").gameObject;
            RectTransform inputChildRect = inputChild.GetComponent<RectTransform>();
            inputChildRect.sizeDelta *= 2;
            Text inputChildText = inputChild.GetComponent<Text>();
            inputChildText.resizeTextMaxSize = 20;
            InputField inputFieldComp = inputChild.GetComponent<InputField>();
            inputFieldComp.characterValidation = InputField.CharacterValidation.Integer;
            nameInputComp.inputField.onValueChanged.AddListener(delegate{
                changeInput(objName, inputFieldComp);
            });

            inputOptions.Add(objName, textValue);
            return statHolder;
        }

        public static void createStatOption(string objName, List<string> statNames, int posY, List<string> textValues, GameObject parent)
        {
            GameObject statHolder = new GameObject("OptionHolder");
            statHolder.transform.SetParent(parent.transform);
            Image statImage = statHolder.AddComponent<Image>();
            statImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowInnerSliced.png");
            RectTransform statHolderRect = statHolder.GetComponent<RectTransform>();
            statHolderRect.localPosition = new Vector3(130, posY, 0);
            statHolderRect.sizeDelta = new Vector2(600, 150 + (statNames.Count*50));

            Text nameText = addText(objName, statHolder, 30, new Vector3(0, 105, 0));
            RectTransform nameTextRect = nameText.gameObject.GetComponent<RectTransform>();
            nameTextRect.sizeDelta = new Vector2(80, 50);

            GameObject inputRef = NCMS.Utils.GameObjects.FindEvenInactive("NameInputElement");
            int index = 0;
            foreach (string stat in statNames)
            {

                Text statNameText = addText(stat, statHolder, 15, new Vector3(-140, 45 + (-50*index), 0));
                statNameText.alignment = TextAnchor.MiddleLeft;
                RectTransform statNameRect = statNameText.gameObject.GetComponent<RectTransform>();
                statNameRect.sizeDelta = new Vector2(0, 50);

                GameObject inputField = Instantiate(inputRef, statHolder.transform);
                NameInput nameInputComp = inputField.GetComponent<NameInput>();
                string trueTextValue = null;
                if (Main.hasSettings)
                {
                    // trueTextValue = Main.savedStats.inputOptions[stat];
                    trueTextValue = Main.getSavedOption(stat, textValues[index]);
                }
                else
                {
                    trueTextValue = textValues[index];
                }
                nameInputComp.setText(trueTextValue);
                RectTransform inputRect = inputField.GetComponent<RectTransform>();
                inputRect.localPosition = new Vector3(0, 45 + (-50*index),0);
                inputRect.sizeDelta += new Vector2(120, 40);

                GameObject inputChild = inputField.transform.Find("InputField").gameObject;
                RectTransform inputChildRect = inputChild.GetComponent<RectTransform>();
                inputChildRect.sizeDelta *= 2;
                Text inputChildText = inputChild.GetComponent<Text>();
                inputChildText.resizeTextMaxSize = 20;
                InputField inputFieldComp = inputChild.GetComponent<InputField>();
                if (!int.TryParse(textValues[index], out _))
                {
                    inputFieldComp.characterValidation = InputField.CharacterValidation.Decimal;
                }
                else
                {
                    inputFieldComp.characterValidation = InputField.CharacterValidation.Integer;
                }
                nameInputComp.inputField.onValueChanged.AddListener(delegate{
                    changeInput(stat, inputFieldComp);
                });

                inputOptions.Add(stat, trueTextValue);
                index++;
            }
        }

        public static void createTraitOption(string objName, SavedTrait trait, int posY, GameObject parent, string textValue = "-1")
        {
            GameObject statHolder = new GameObject("OptionHolder");
            statHolder.transform.SetParent(parent.transform);
            Image statImage = statHolder.AddComponent<Image>();
            statImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowInnerSliced.png");
            RectTransform statHolderRect = statHolder.GetComponent<RectTransform>();
            statHolderRect.localPosition = new Vector3(130, posY, 0);
            statHolderRect.sizeDelta = new Vector2(600, 400);

            Text nameText = addText(objName, statHolder, 30, new Vector3(0, 145, 0));
            RectTransform nameTextRect = nameText.gameObject.GetComponent<RectTransform>();
            nameTextRect.sizeDelta = new Vector2(0, 50);

            GameObject inputRef = NCMS.Utils.GameObjects.FindEvenInactive("NameInputElement");
            int index = 0;
            foreach (FieldInfo field in trait.GetType().GetFields())
            {
                bool isInt = true;
                if (field.FieldType != typeof(int) && field.FieldType != typeof(float))
                {
                    continue;
                }
                else if (field.FieldType != typeof(int))
                {
                    isInt = false;
                }

                Text fieldNameText = addText(field.Name, statHolder, 15, new Vector3(-140, 100 + (-55*index), 0));
                fieldNameText.alignment = TextAnchor.MiddleLeft;
                RectTransform fieldNameRect = fieldNameText.gameObject.GetComponent<RectTransform>();
                fieldNameRect.sizeDelta = new Vector2(0, 50);

                GameObject inputField = Instantiate(inputRef, statHolder.transform);
                NameInput nameInputComp = inputField.GetComponent<NameInput>();
                textValue = (string)(field.GetValue(trait).ToString());
                // Debug.Log($"{field.Name}: {textValue}");
                // if (Main.hasSettings)
                // {
                //     textValue = Main.savedStats.inputOptions[objName];
                // }
                nameInputComp.setText(textValue);
                RectTransform inputRect = inputField.GetComponent<RectTransform>();
                inputRect.localPosition = new Vector3(0, 105 + (-54*index),0);
                inputRect.sizeDelta += new Vector2(120, 40);

                GameObject inputChild = inputField.transform.Find("InputField").gameObject;
                RectTransform inputChildRect = inputChild.GetComponent<RectTransform>();
                inputChildRect.sizeDelta *= 2;
                Text inputChildText = inputChild.GetComponent<Text>();
                inputChildText.resizeTextMaxSize = 20;
                InputField inputFieldComp = inputChild.GetComponent<InputField>();
                if (!isInt)
                {
                    inputFieldComp.characterValidation = InputField.CharacterValidation.Decimal;
                }
                else
                {
                    inputFieldComp.characterValidation = InputField.CharacterValidation.Integer;
                }
                nameInputComp.inputField.onValueChanged.AddListener(delegate{
                    changeTraitInput(objName, field, inputFieldComp, isInt);
                });
                index++;
            }
        }

        private static void changeInput(string inputName, InputField inputField)
        {
            int value = -1;
            float fValue = -1f;
            if (float.TryParse(inputField.text, out fValue) || int.TryParse(inputField.text, out value))
            {
                inputOptions[inputName] = inputField.text;
            }
        }

        private static void changeTraitInput(string inputName, FieldInfo field, InputField inputField, bool isInt)
        {
            string value = inputField.text;
            if(string.IsNullOrEmpty(value))
            {
                return;
            }
            if (isInt)
            {
                int intValue = int.Parse(value);
                field.SetValue(Traits.talentIDs[inputName], intValue);
            }
            else
            {
                float floatValue = float.Parse(value);
                field.SetValue(Traits.talentIDs[inputName], floatValue);
            }
        }

        public static Text addText(string textString, GameObject parent, int sizeFont, Vector3 pos, int increaseWidth = 0)
        {
            GameObject textRef = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/commissionSettings/Background/Name");
            GameObject textGo = Instantiate(textRef, parent.transform);
            textGo.SetActive(true);

            var textComp = textGo.GetComponent<Text>();
            textComp.text = textString;
            textComp.fontSize = sizeFont;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.position = new Vector3(0,0,0);
            textRect.localPosition = pos;
            textRect.sizeDelta = new Vector2(textComp.preferredWidth + increaseWidth, textComp.preferredHeight);
        
            return textComp;
        }

        public static string getOption(string option, string value = "1")
        {
            if (inputOptions.ContainsKey(option))
            {
                return inputOptions[option];
            }
            inputOptions.Add(option, value);
            return value;
        }

        public static Button createBGButton(GameObject parent, int posY, string iconName, string buttonName, string buttonDesc)
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

            return buttonButton;
        }

        public static Button createFilterBGButton(GameObject parent, string name, int posY, string iconName, string buttonName, string buttonDesc, ref ToggleIcon tIcon)
        {
            Button buttonButton = UI.createBGButton(
                parent, 
                posY, 
                iconName, 
                buttonName, 
                buttonDesc
            );

            GameObject toggleHolder = new GameObject("ToggleIcon");
            toggleHolder.transform.SetParent(buttonButton.gameObject.transform);
            Image toggleImage = toggleHolder.AddComponent<Image>();
            ToggleIcon toggleIcon = toggleHolder.AddComponent<ToggleIcon>();
            toggleIcon.spriteON = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.buttonToggleIndicator0.png");
            toggleIcon.spriteOFF = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.buttonToggleIndicator1.png");
            toggleIcon.updateIcon(false);

            RectTransform toggleRect = toggleHolder.GetComponent<RectTransform>();
            toggleRect.localPosition = new Vector3(0, 12, 0);
            toggleRect.sizeDelta = new Vector2(8, 7);

            tIcon = toggleIcon;

            return buttonButton;
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

        public static void addNewWindowElement(GameObject parent, int i, Actor actor, string sorting)
        {
            GameObject bgElement = new GameObject("WindowElement");
            bgElement.transform.SetParent(parent.transform);
            Image bgImage = bgElement.AddComponent<Image>();
            bgImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowInnerSliced.png");
            RectTransform bgRect = bgElement.GetComponent<RectTransform>();
            bgRect.localPosition = new Vector3(130, -40 + (i*-60), 0);
            bgRect.sizeDelta = new Vector2(450, 140);

            GameObject avatar = UI.createAvatar(actor, bgElement, 30,  new Vector3(-110, -70, 0));

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
                    string talentName = Main.hasTalent(actor.data);
                    string[] talent = talentName.Split('r');
                    if (talentName == "Bearrank")
                    {
                        talent[0] = "Bear";
                    }
                    GameObject talentText = UI.addText($"{talent[0]}", bgElement, 50, new Vector3(140, 5, 0),20).gameObject;
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
                    string[] actortalent = Main.hasTalent(actor.data).Split('r');
                    GameObject defaultText = UI.addText($"{actortalent[0]}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
                    break;
            }
            createBanner(bgElement, actor.city, new Vector2(80, 80), new Vector3(0, 0, 0));
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
    }
}