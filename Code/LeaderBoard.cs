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
        private static Dictionary<string, int> talentPriority = new Dictionary<string, int>
        {
            {"EXrank", 10},
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
        public static void init()
        {
            addLeaderBoardWindow("leaderBoardWindow", "Talent Leader Board");
        }

        public static void openWindow()
        {
            searchUnits("talents");
            Windows.ShowWindow("leaderBoardWindow");
        }

        public static void searchUnits(string sortName)
        {
            foreach(Transform child in boardContents.transform)
            {
                Destroy(child.gameObject);
            }
            List<Actor> filteredList = sortUnits(sortName);
            RectTransform contentRect = boardContents.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0, 207 + (filteredList.Count * 70));
            for(int i = 0; i < filteredList.Count; i++)
            {
                Actor actor = filteredList[i];
                addNewWindowElement(i, actor);
            }

        }

        public static List<Actor> sortUnits(string sortName)
        {
            List<Actor> unitList = MapBox.instance.units.getSimpleList();
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

        public static void addNewWindowElement(int i, Actor actor)
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
            string[] talent = hasTalent(actor.data).Split('r');
            GameObject talentText = UI.addText($"{talent[0]}", bgElement, 50, new Vector3(140, 5, 0)).gameObject;
            // GameObject dmgText = UI.addText($"DMG: {Toolbox.formatNumber(actor.curStats.damage)}", bgElement, 25, new Vector3(70, 20, 0)).gameObject;
            // GameObject killsText = UI.addText($"KILLS: {Toolbox.formatNumber(actor.data.kills)}", bgElement, 25, new Vector3(180, 20, 0)).gameObject;
            // GameObject lvlText = UI.addText($"LVL: {actor.data.level.ToString()}", bgElement, 25, new Vector3(70, -25, 0)).gameObject;
            // GameObject ageText = UI.addText($"AGE: {actor.data.age.ToString()}", bgElement, 25, new Vector3(180, -25, 0)).gameObject;

            // GameObject inspectButton = new GameObject("inspectButton");
            // inspectButton.transform.SetParent(boardContents.transform);
            // Image inspectImage = inspectButton.AddComponent<Image>();
            // inspectImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.iconSpectate.png");
            // RectTransform inspectRect = inspectButton.GetComponent<RectTransform>();
            // inspectRect.sizeDelta = new Vector2(80, 80);
            // inspectRect.localPosition = new Vector3(180, -40 + (i*-50) );
            // Button inspectButtonComp = inspectButton.AddComponent<Button>();
            // inspectButtonComp.onClick.AddListener(() => avatarOnClick(actor));
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

            var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);

            boardContents = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
        }
    }
}