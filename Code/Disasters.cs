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
    class Disasters
    {
        public static void init()
        {
            Localization.AddOrSet("worldlog_boss_dej", "Boss Monster Have Been Spotted!");
            AssetManager.disasters.add(new DisasterAsset
            {
                id = "bossMonster",
                rate = 20,
                chance = 1f,
                min_world_cities = 1,
                world_log = "worldlog_boss_dej",
                world_log_icon = "iconBossRank",
                min_world_population = 10,
                type = DisasterType.Nature,
                action = new DisasterAction(spawnBoss)
            });
        }

        private static void spawnBoss(DisasterAsset pAsset)
        {
            TileIsland randomIslandGround = MapBox.instance.islandsCalculator.getRandomIslandGround(true);
            if (randomIslandGround == null)
            {
                return;
            }
            WorldTile randomTile = randomIslandGround.getRandomTile();
            GodPower godPower = AssetManager.powers.get("rhino");
            if (godPower.showSpawnEffect != string.Empty)
            {
                MapBox.instance.stackEffects.startSpawnEffect(randomTile, godPower.showSpawnEffect);
            }
            MapBox.instance.createNewUnit(godPower.actorStatsId, randomTile, "", godPower.actorSpawnHeight, null);
            int num = Toolbox.randomInt(2, 5);
            for (int i = 0; i < num; i++)
            {
                WorldTile random = randomTile.neighbours.GetRandom<WorldTile>();
                Actor beast = MapBox.instance.createNewUnit(godPower.actorStatsId, random, "", godPower.actorSpawnHeight, null);
                beast.data.addTrait("AnimalBossrank");
            }
            WorldLog.logDisaster(pAsset, randomTile, null, null, null);
        }
    }
}