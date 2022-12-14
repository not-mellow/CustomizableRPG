using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace CommissionMod
{
    [ModEntry]
    class Main : MonoBehaviour
    {
        public static SavedStats savedStats = new SavedStats();
        public static bool hasSettings = false;
        private static bool modLoaded = false;
        // void Awake()
        // {
        //     Invoke("fixedInit", 1f);
        // }

        void Update()
        {
            if (Config.gameLoaded && !modLoaded)
            {
                Invoke("fixedInit", 1f);
                modLoaded = true;
            }
        }

        public void fixedInit()
        {
            if (File.Exists($"{ModDeclaration.Info.NCMSModsPath}/CommissionModSettings.json"))
            {
                loadStats();
                hasSettings = true;
            }
            Patches.init();
            GodPowers.init();
            Traits.init();
            UI.init();
            LeaderBoard.init();
            FilterWindow.init();
            WorldTalentStats.init();
            WorldLevelStats.init();
            StatLimitWindow.init();
            if (!hasSettings)
            {
                createStats();
            }
        }

        private static void loadStats()
        {
            string data = File.ReadAllText($"{ModDeclaration.Info.NCMSModsPath}/CommissionModSettings.json");
            SavedStats loadedData = JsonConvert.DeserializeObject<SavedStats>(data);
            savedStats = loadedData;
        }

        private static void createStats()
        {
            foreach (KeyValuePair<string, SavedTrait> kv in Traits.talentIDs)
            {
                savedStats.traits.Add(kv.Key, kv.Value);
            }
            
            savedStats.inputOptions = UI.inputOptions;

            string json = JsonConvert.SerializeObject(savedStats, Formatting.Indented);
            File.WriteAllText($"{ModDeclaration.Info.NCMSModsPath}/CommissionModSettings.json", json);
        }

        public static void saveStats()
        {
            File.Delete($"{ModDeclaration.Info.NCMSModsPath}/CommissionModSettings.json");
            savedStats = new SavedStats();
            foreach (KeyValuePair<string, SavedTrait> kv in Traits.talentIDs)
            {
                savedStats.traits.Add(kv.Key, kv.Value);
            }
            
            savedStats.inputOptions = UI.inputOptions;

            string json = JsonConvert.SerializeObject(savedStats, Formatting.Indented);
            File.WriteAllText($"{ModDeclaration.Info.NCMSModsPath}/CommissionModSettings.json", json);

            foreach (Actor actor in MapBox.instance.units.getSimpleList())
            {
                actor.setStatsDirty();
            }
        }

        public static string getSavedOption(string option, string value = "1")
        {
            if (savedStats.inputOptions.ContainsKey(option))
            {
                return savedStats.inputOptions[option];
            }
            savedStats.inputOptions.Add(option, value);
            return value;
        }
    }

    public class SavedStats
    {
        public Dictionary<string, SavedTrait> traits = new Dictionary<string, SavedTrait>();

        public Dictionary<string, string> inputOptions = new Dictionary<string, string>();
    }
}