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
        private static GameObject settingContents;

        public static void init()
        {
            ScrollWindow.checkWindowExist("inspect_unit");
            addSettingsWindow("commissionSettings", "Settings Window");
            addButtons();
            RectTransform settingRect = settingContents.GetComponent<RectTransform>();
            settingRect.sizeDelta += new Vector2(0, Traits.talentIDs.Count*140);

            // GameObject mainTab = GameObjects.FindEvenInactive("Tab_Main");
            GameObject levelRateHolder = createInputOption(
                "levelRate",
                "Level Rain Value",
                "Modify The Value Of Each Level Rain",
                -60,
                settingContents,
                "10"
            );
            // RectTransform levelRateRect = levelRateHolder.GetComponent<RectTransform>();
            // levelRateRect.localPosition = new Vector3(750, 80);
            createInputOption(
                "ageIncreaseOption",
                "Age Level Increase",
                "Modify How Many Years Are Added\nTo Units That Gained A New Level In The 10s",
                -120,
                settingContents,
                "250"
            );
            
            createInputOption(
                "levelCapOption",
                "Unit Level Cap",
                "Modify The Max Level A Unit Can Reach.\nIf The Value is -1, The Max Level Will Be Infinite!",
                -180,
                settingContents,
                "120"
            );

            createInputOption(
                "expGapOption",
                "Unit Experience Gap",
                "Modify How The Experience Gap Between Levels!\nIf the is -1, it will revert back to the normal exp gap.",
                -240,
                settingContents,
                "250"
            );

            createStatOption(
                "Stat Boost Per Level Leap",
                new List<string>{"Damage", "Critical", "Attack Speed", "Health"},
                -320,
                new List<string>{"75", "5", "5", "600"}
            );

            int index = 0;
            foreach(KeyValuePair<string, SavedTrait> kv in Traits.talentIDs)
            {
                createTraitOption(
                    kv.Key,
                    kv.Value,
                    -420 + (-index*100)
                );
                index++;
            }

            NCMS.Utils.PowerButtons.CreateButton(
                "saveSettingsButton",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.save_icon.png"),
                "Save Changes",
                "Save The Changes To The Settings",
                new Vector2(130, -1390),
                ButtonType.Click,
                settingContents.transform,
                Main.saveStats
            );
        }

        private static void addButtons()
        {
            PowerButton settingsButton = NCMS.Utils.PowerButtons.CreateButton(
                "commissionModSettingsButton",
                NCMS.Utils.Sprites.LoadSprite($"{Mod.Info.Path}/icon.png"),
                "Mod Settings",
                "Click Here To Change Settings Of The Commission Mod",
                new Vector2(0, 0),
                ButtonType.Click,
                null,
                () => Windows.ShowWindow("commissionSettings")
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(settingsButton, NCMS.Utils.PowerTab.Main, new Vector2(632, -18));

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

            NCMS.Utils.PowerButtons.AddButtonToTab(levelButton, NCMS.Utils.PowerTab.Main, new Vector2(632, 18));

            PowerButton leaderBoardButton = NCMS.Utils.PowerButtons.CreateButton(
                "leaderBoardButton",
                NCMS.Utils.Sprites.LoadSprite($"{Mod.Info.Path}/icon.png"),
                "Talent Leader Board",
                "Click Here To See 100 Of The Most Talented People In The World!",
                new Vector2(0, 0),
                ButtonType.Click,
                null,
                LeaderBoard.openWindow
            );

            NCMS.Utils.PowerButtons.AddButtonToTab(leaderBoardButton, NCMS.Utils.PowerTab.Main, new Vector2(590, -18));

            PowerButton talentStatsButton = NCMS.Utils.PowerButtons.CreateButton(
                "talentStatsButton",
                NCMS.Utils.Sprites.LoadSprite($"{Mod.Info.Path}/icon.png"),
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

            NCMS.Utils.PowerButtons.AddButtonToTab(levelStatsButton, NCMS.Utils.PowerTab.Main, new Vector2(548, -18));

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

            NCMS.Utils.PowerButtons.AddButtonToTab(spLevelButton, NCMS.Utils.PowerTab.Main, new Vector2(548, 18));
        }
        
        private static void addSettingsWindow(string id, string title)
        {
            ScrollWindow window;
            window = Windows.CreateNewWindow(id, title);

            var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);

            settingContents = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
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
                textValue = Main.savedStats.inputOptions[objName];
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

        public static void createStatOption(string objName, List<string> statNames, int posY, List<string> textValues)
        {
            GameObject statHolder = new GameObject("OptionHolder");
            statHolder.transform.SetParent(settingContents.transform);
            Image statImage = statHolder.AddComponent<Image>();
            statImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowInnerSliced.png");
            RectTransform statHolderRect = statHolder.GetComponent<RectTransform>();
            statHolderRect.localPosition = new Vector3(130, posY, 0);
            statHolderRect.sizeDelta = new Vector2(600, 280);

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
                    trueTextValue = Main.savedStats.inputOptions[stat];
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
                inputFieldComp.characterValidation = InputField.CharacterValidation.Decimal;
                nameInputComp.inputField.onValueChanged.AddListener(delegate{
                    changeInput(stat, inputFieldComp);
                });

                inputOptions.Add(stat, trueTextValue);
                index++;
            }
        }

        public static void createTraitOption(string objName, SavedTrait trait, int posY, string textValue = "-1")
        {
            GameObject statHolder = new GameObject("OptionHolder");
            statHolder.transform.SetParent(settingContents.transform);
            Image statImage = statHolder.AddComponent<Image>();
            statImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowInnerSliced.png");
            RectTransform statHolderRect = statHolder.GetComponent<RectTransform>();
            statHolderRect.localPosition = new Vector3(130, posY, 0);
            statHolderRect.sizeDelta = new Vector2(600, 280);

            Text nameText = addText(objName, statHolder, 30, new Vector3(0, 105, 0));
            RectTransform nameTextRect = nameText.gameObject.GetComponent<RectTransform>();
            nameTextRect.sizeDelta = new Vector2(0, 50);

            GameObject inputRef = NCMS.Utils.GameObjects.FindEvenInactive("NameInputElement");
            int index = 0;
            foreach (FieldInfo field in trait.GetType().GetFields())
            {

                Text fieldNameText = addText(field.Name, statHolder, 15, new Vector3(-140, 45 + (-50*index), 0));
                fieldNameText.alignment = TextAnchor.MiddleLeft;
                RectTransform fieldNameRect = fieldNameText.gameObject.GetComponent<RectTransform>();
                fieldNameRect.sizeDelta = new Vector2(0, 50);

                GameObject inputField = Instantiate(inputRef, statHolder.transform);
                NameInput nameInputComp = inputField.GetComponent<NameInput>();
                bool isInt = true;
                if (field.FieldType != typeof(int))
                {
                    isInt = false;
                }
                textValue = (string)(field.GetValue(trait).ToString());
                // if (Main.hasSettings)
                // {
                //     textValue = Main.savedStats.inputOptions[objName];
                // }
                nameInputComp.setText(textValue);
                RectTransform inputRect = inputField.GetComponent<RectTransform>();
                inputRect.localPosition = new Vector3(0, 45 + (-50*index),0);
                inputRect.sizeDelta += new Vector2(120, 40);

                GameObject inputChild = inputField.transform.Find("InputField").gameObject;
                RectTransform inputChildRect = inputChild.GetComponent<RectTransform>();
                inputChildRect.sizeDelta *= 2;
                Text inputChildText = inputChild.GetComponent<Text>();
                inputChildText.resizeTextMaxSize = 20;
                InputField inputFieldComp = inputChild.GetComponent<InputField>();
                inputFieldComp.characterValidation = InputField.CharacterValidation.Decimal;
                nameInputComp.inputField.onValueChanged.AddListener(delegate{
                    changeTraitInput(objName, field, inputFieldComp, isInt);
                });
                index++;
            }
        }

        private static void changeInput(string inputName, InputField inputField)
        {
            int value = -1;
            if (!int.TryParse(inputField.text, out value))
            {
                return;
            }
            inputOptions[inputName] = inputField.text;
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

        public static Text addText(string textString, GameObject parent, int sizeFont, Vector3 pos)
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
            textRect.sizeDelta = new Vector2(textComp.preferredWidth, textComp.preferredHeight);
        
            return textComp;
        }
    }
}