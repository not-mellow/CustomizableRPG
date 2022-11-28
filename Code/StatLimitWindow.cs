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
    class StatLimitWindow : MonoBehaviour
    {
        private static GameObject limitContents;
        private static GameObject scrollView;

        public static void init()
        {
            addLimitWindow("statLimitWindow", "Stat Limits Window");

            createStatLimiter(
                "ArmorLimit",
                "Armor Limit",
                -40,
                limitContents,
                "50"
            );

            createStatLimiter(
                "SpeedLimit",
                "Speed Limit",
                -80,
                limitContents,
                "300"
            );

            createStatLimiter(
                "DMGLimit",
                "DMG Limit",
                -120,
                limitContents,
                "1000"
            );

            createStatLimiter(
                "HealthLimit",
                "Health Limit",
                -160,
                limitContents,
                "1000"
            );

            NCMS.Utils.PowerButtons.CreateButton(
                "saveLimitsButton",
                Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.iconSave.png"),
                "Save Limits",
                "Save The Limits To The Settings",
                new Vector2(130, -195),
                ButtonType.Click,
                limitContents.transform,
                Main.saveStats
            );
        }

        public static GameObject createStatLimiter(string objName, string title, int posY, GameObject parent, string textValue = "-1")
        {
            GameObject inputRef = NCMS.Utils.GameObjects.FindEvenInactive("NameInputElement");

            GameObject inputField = Instantiate(inputRef, parent.transform);
            NameInput nameInputComp = inputField.GetComponent<NameInput>();
            if (Main.hasSettings)
            {
                // textValue = Main.savedStats.inputOptions[objName];
                textValue = Main.getSavedOption(objName, textValue);
            }
            nameInputComp.setText(textValue);
            RectTransform inputRect = inputField.GetComponent<RectTransform>();
            inputRect.localPosition = new Vector3(130,posY,0);
            inputRect.sizeDelta = new Vector2(120, 20);

            GameObject inputChild = inputField.transform.Find("InputField").gameObject;
            RectTransform inputChildRect = inputChild.GetComponent<RectTransform>();
            // inputChildRect.sizeDelta *= 2;
            Text inputChildText = inputChild.GetComponent<Text>();
            inputChildText.resizeTextMaxSize = 20;
            InputField inputFieldComp = inputChild.GetComponent<InputField>();
            inputFieldComp.characterValidation = InputField.CharacterValidation.Integer;
            nameInputComp.inputField.onValueChanged.AddListener(delegate{
                changeInput(objName, inputFieldComp);
            });

            UI.inputOptions.Add(objName, textValue);

            Text statText = UI.addText(title, inputField, 10, new Vector3(0, 15, 0));
            RectTransform statTextRect = statText.gameObject.GetComponent<RectTransform>();
            statTextRect.sizeDelta = new Vector2(statTextRect.sizeDelta.x, 15);

            return inputField;
        }

        private static void changeInput(string inputName, InputField inputField)
        {
            if (float.TryParse(inputField.text, out _) || int.TryParse(inputField.text, out _))
            {
                UI.inputOptions[inputName] = inputField.text;
            }
        }

        public static void openWindow()
        {
            Windows.ShowWindow("statLimitWindow");
        }

        private static void addLimitWindow(string id, string title)
        {
            ScrollWindow window;
            window = Windows.CreateNewWindow(id, title);

            scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);

            limitContents = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
        }
    }
}