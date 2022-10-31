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
    class WorldLevelStats : MonoBehaviour
    {
        private static GameObject statsContents;
        private static Dictionary<string, int> levelStats = new Dictionary<string, int>();

        public static void init()
        {
            addWorldStatsWindow("levelStatsWindow", "World Level Stats");
        }

        public static void openWindow()
        {
            loadStats();
            Windows.ShowWindow("levelStatsWindow");
        }

        private static void loadStats()
        {
            foreach(Transform child in statsContents.transform)
            {
                Destroy(child.gameObject);
            }

            levelStats.Clear();
            foreach(Actor actor in MapBox.instance.units.getSimpleList())
            {
                int curlevel = (actor.data.level/10)*10;
                if (!levelStats.ContainsKey(curlevel.ToString()))
                {
                    levelStats.Add(curlevel.ToString(), 1);
                }
                else
                {
                    levelStats[curlevel.ToString()] += 1;
                }
            }

            if (levelStats.Count > 8)
            {
                int yIncrease = levelStats.Count - 8;
                RectTransform statsRect = statsContents.GetComponent<RectTransform>();
                statsRect.sizeDelta += new Vector2(0, yIncrease*30);
            }
            int index = 0;
            foreach(KeyValuePair<string, int> kv in levelStats)
            {
                UI.addText($"{kv.Key}-{int.Parse(kv.Key) + 9} Level", statsContents, 10, new Vector3(60, -25 + (index*-25)));
                UI.addText(kv.Value.ToString(), statsContents, 10, new Vector3(200, -25 + (index*-25)));
                index++;
            }
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