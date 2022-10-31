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
    class WorldTalentStats : MonoBehaviour
    {
        private static GameObject statsContents;
        private static Dictionary<string, int> talentStats = new Dictionary<string, int>();

        public static void init()
        {
            addWorldStatsWindow("talentStatsWindow", "World Talent Stats");
        }

        public static void openWindow()
        {
            loadStats();
            Windows.ShowWindow("talentStatsWindow");
        }

        private static void loadStats()
        {
            foreach(Transform child in statsContents.transform)
            {
                Destroy(child.gameObject);
            }
            talentStats.Clear();
            List<Actor> unitList = MapBox.instance.units.getSimpleList();
            foreach(Actor actor in unitList)
            {
                string talent = hasTalent(actor.data);
                if (talent == null)
                {
                    continue;
                }
                if (talentStats.ContainsKey(talent))
                {
                    talentStats[talent] += 1;
                }
                else
                {
                    talentStats.Add(talent, 1);
                }
            }
            if (talentStats.Count > 8)
            {
                int yIncrease = talentStats.Count - 8;
                RectTransform statsRect = statsContents.GetComponent<RectTransform>();
                statsRect.sizeDelta += new Vector2(0, yIncrease*30);
            }
            int index = 0;
            foreach(KeyValuePair<string, int> kv in talentStats)
            {
                UI.addText(kv.Key, statsContents, 10, new Vector3(60, -25 + (index*-25)));
                UI.addText(kv.Value.ToString(), statsContents, 10, new Vector3(200, -25 + (index*-25)));
                index++;
            }
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

        private static void addWorldStatsWindow(string id, string title)
        {
            ScrollWindow window;
            window = Windows.CreateNewWindow(id, title);

            var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);

            statsContents = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
        }
    }
}