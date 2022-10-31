using ReflectionUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;

namespace CommissionMod
{
    class GodPowers
    {
        public static void init()
        {
            initPowers();
        }

        private static void initPowers()
        {
            DropAsset levelDrop = AssetManager.drops.clone("levelDrop", "blessing");
            levelDrop.action_landed = new DropsAction(action_levelup);

            GodPower levelPower = AssetManager.powers.clone("levelUp", "_drops");
            levelPower.name = "levelUp";
            levelPower.dropID = "levelDrop";
            levelPower.fallingChance = 0.01f;
            levelPower.click_power_action = new PowerAction(AssetManager.powers.spawnDrops);
            levelPower.click_power_brush_action = new PowerAction(AssetManager.powers.loopWithCurrentBrushPower);

            DropAsset spLevelDrop = AssetManager.drops.clone("specificLevelDrop", "blessing");
            spLevelDrop.action_landed = new DropsAction(action_specific_levelup);

            GodPower spLevelPower = AssetManager.powers.clone("spLevelUp", "_drops");
            spLevelPower.name = "spLevelUp";
            spLevelPower.dropID = "specificLevelDrop";
            spLevelPower.fallingChance = 0.01f;
            spLevelPower.click_power_action = new PowerAction(AssetManager.powers.spawnDrops);
            spLevelPower.click_power_brush_action = new PowerAction(AssetManager.powers.loopWithCurrentBrushPower);
        }

        private static void action_levelup(WorldTile pTile = null, string pDropID = null)
        {
            MapBox.instance.getObjectsInChunks(pTile, 3, MapObjectType.Actor);
            for (int i = 0; i < MapBox.instance.temp_map_objects.Count; i++)
            {
                Actor pActor = (Actor)MapBox.instance.temp_map_objects[i];
                // int capValue = (int.Parse(UI.inputOptions["levelCapOption"]) > -1)? int.Parse(UI.inputOptions["levelCapOption"]) : 10;
                // if (pActor.data.level < capValue)
                // {
                    int prevLevel = pActor.data.level;
                    pActor.data.level += int.Parse(Main.savedStats.inputOptions["levelRate"]);
                    if ((int)(pActor.data.level % 10) == 0 || pActor.data.level - prevLevel > 10)
                    {
                         pActor.event_full_heal = true;
                    }
                // }
                pActor.setStatsDirty();
                pActor.startShake(0.3f, 0.1f, true, true);
                pActor.startColorEffect("white");
            }
        }

        private static void action_specific_levelup(WorldTile pTile = null, string pDropID = null)
        {
            MapBox.instance.getObjectsInChunks(pTile, 3, MapObjectType.Actor);
            for (int i = 0; i < MapBox.instance.temp_map_objects.Count; i++)
            {
                Actor pActor = (Actor)MapBox.instance.temp_map_objects[i];
                pActor.data.level = int.Parse(Main.savedStats.inputOptions["levelRate"]);
                if ((int)(pActor.data.level / 10) != 0)
                {
                        pActor.event_full_heal = true;
                }
                pActor.setStatsDirty();
                pActor.startShake(0.3f, 0.1f, true, true);
                pActor.startColorEffect("white");
            }
        }
    }
}