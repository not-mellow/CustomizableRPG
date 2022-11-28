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
    public enum RankType
    {
        Human,
        Beast,
        Nothing
    }

    class Traits
    {
        private static List<ActorTrait> addedTalents = new List<ActorTrait>();
        public static Dictionary<string, SavedTrait> talentIDs = new Dictionary<string, SavedTrait>();
        public static void init()
        {
            traitInit();
        }

        private static void traitInit()
        {
            BaseStats newStats = new BaseStats{
                knockbackReduction = 100f
            };
            createTalentTrait(
                "Frank",
                "ui/Icons/iconFrank",
                "F Rank",
                "The Lowest Tier",
                0f,
                newStats,
                100,
                0.05f,
                250,
                10,
                5,
                RankType.Human
            );
            createTalentTrait(
                "Erank",
                "ui/Icons/iconErank",
                "E Rank",
                "The Second Lowest Tier",
                50f,
                newStats,
                70,
                0.08f,
                300,
                20,
                10,
                RankType.Human
            );
            createTalentTrait(
                "Drank",
                "ui/Icons/iconDrank",
                "D Rank",
                "The Third Lowest Tier",
                45f,
                newStats,
                75,
                0.10f,
                320,
                30,
                15,
                RankType.Human
            );
            createTalentTrait(
                "Crank",
                "ui/Icons/iconCrank",
                "C Rank",
                "The Fourth Lowest Tier",
                15f,
                newStats,
                77,
                0.15f,
                350,
                40,
                20,
                RankType.Human
            );

            createTalentTrait(
                "Brank",
                "ui/Icons/iconBrank",
                "B Rank",
                "The Sixth Highest Tier",
                5f,
                newStats,
                80,
                0.2f,
                370,
                50,
                25,
                RankType.Human
            );
            createTalentTrait(
                "Arank",
                "ui/Icons/iconArank",
                "A Rank",
                "The Fifth Highest Tier",
                1.5f,
                newStats,
                90,
                0.30f,
                400,
                60,
                30,
                RankType.Human
            );
            createTalentTrait(
                "Srank",
                "ui/Icons/iconSrank",
                "S Rank",
                "The Fourth Highest Tier",
                0.03f,
                newStats,
                110,
                0.3f,
                450,
                70,
                50,
                RankType.Human
            );
            createTalentTrait(
                "SSrank",
                "ui/Icons/iconSSrank",
                "SS Rank",
                "The Third Highest Tier",
                0.005f,
                newStats,
                130,
                0.35f,
                480,
                80,
                55,
                RankType.Human
            );
            createTalentTrait(
                "SSSrank",
                "ui/Icons/iconSSSrank",
                "SSS Rank",
                "The Second Highest Tier",
                0.001f,
                newStats,
                150,
                0.4f,
                510,
                90,
                60,
                RankType.Human
            );
            createTalentTrait(
                "EXrank",
                "ui/Icons/iconEXrank",
                "EX Rank",
                "The Highest Tier",
                0.0003f,
                newStats,
                200,
                0.5f,
                550,
                100,
                70,
                RankType.Human
            );
            createTalentTrait(
                "Sigma",
                "ui/Icons/iconSigmarank",
                "Sigma Rank",
                "The Highest Tier",
                0f,
                newStats,
                1,
                0f,
                1,
                10,
                1,
                RankType.Human
            );
            createTalentTrait(
                "Omega",
                "ui/Icons/iconOmegarank",
                "Omega Rank",
                "The Highest Tier",
                0f,
                newStats,
                1,
                0f,
                1,
                10,
                1,
                RankType.Human
            );
            createTalentTrait(
                "Zeta",
                "ui/Icons/iconZetarank",
                "Zeta Rank",
                "The Highest Tier",
                0f,
                newStats,
                1,
                0f,
                1,
                10,
                1,
                RankType.Human
            );
            createTalentTrait(
                "WolfRank",
                "ui/Icons/iconWolfRank",
                "Lion Rank",
                "The Lowest Beast Rank",
                0f,
                newStats,
                100,
                0.1f,
                100,
                50,
                25,
                RankType.Beast
            );
            createTalentTrait(
                "BearRank",
                "ui/Icons/iconBearRank",
                "Bear Rank",
                "The Middle Beast Rank",
                10f,
                newStats,
                150,
                0.3f,
                200,
                70,
                50,
                RankType.Beast
            );
            createTalentTrait(
                "LionRank",
                "ui/Icons/iconLionRank",
                "Lion Rank",
                "The Highest Beast Rank",
                0.05f,
                newStats,
                200,
                0.5f,
                500,
                100,
                100,
                RankType.Beast
            );

            addTalentList();
        }

        private static void createTalentTrait(string newID, string icon, string title, string desc, float newBirth, 
        BaseStats newStats, int expGainKillMod, float percentageMod, int passive, int levelCap, int expGainHitMod, RankType rankType)
        {
            Localization.AddOrSet($"trait_{newID}", title);
            Localization.AddOrSet($"trait_{newID}_info", desc);
            ActorTrait talent = new ActorTrait
            {
                id = newID,
                path_icon = icon,
                birth = newBirth,
                // opposite = "weak",
                // oppositeTraitMod = -10,
                // sameTraitMod = 5,
                group = TraitGroup.Genetic,
                type = TraitType.Positive
            };
            if (Main.hasSettings && Main.savedStats.traits.ContainsKey(talent.id))
            {
                talent.birth = Main.savedStats.traits[talent.id].spawnRate;
            }
            if (newStats != null)
            {
		        talent.baseStats.addStats(newStats);
            }
            addedTalents.Add(talent);
            SavedTrait newTrait = new SavedTrait{
                expGainKill = expGainKillMod,
                expGainHit = expGainHitMod,
                spawnRate = newBirth,
                decreaseEXPRequirement = percentageMod,
                passiveExpGain = passive,
                talentLevelCap = levelCap,
                type = rankType
            };
            if (Main.hasSettings && Main.savedStats.traits.ContainsKey(talent.id))
            {
                SavedTrait currentSavedTrait = Main.savedStats.traits[talent.id];
                bool useSavedTrait = true;
                foreach (FieldInfo field in currentSavedTrait.GetType().GetFields())
                {
                    int ivalue = 0;
                    float fvalue = 0f;
                    if (field.FieldType != typeof(int) && field.FieldType != typeof(float))
                    {
                        if (field.FieldType == typeof(RankType))
                        {
                            if ((RankType)(field.GetValue(currentSavedTrait)) == RankType.Nothing)
                            {
                                currentSavedTrait.type = rankType;
                            }
                        }
                        continue;
                    }
                    else if (field.FieldType != typeof(int))
                    {
                        fvalue = (float)(field.GetValue(currentSavedTrait));
                    }
                    else
                    {
                        ivalue = (int)(field.GetValue(currentSavedTrait));
                    }
                    if (ivalue < 0 || fvalue < 0)
                    {
                        useSavedTrait = false;
                        break;
                    }
                }
                if (useSavedTrait)
                {
                    newTrait = currentSavedTrait;
                }
            }
            talentIDs.Add(talent.id, newTrait);
        }

        private static void addTalentList()
        {
            foreach (ActorTrait kv in addedTalents)
            {
                foreach (ActorTrait kvTwo in addedTalents)
                {
                    if (kv.id == kvTwo.id)
                    {
                        continue;
                    }
                    kv.opposite += $"{kvTwo.id},";
                }
                AssetManager.traits.add(kv);
                PlayerConfig.unlockTrait(kv.id);
            }
        }
    }

    public class SavedTrait
    {
        public int expGainKill = -1;
        public int expGainHit = -1;
        public float spawnRate = -1f;
        public float decreaseEXPRequirement = -1f;
        public int passiveExpGain = -1;
        public int talentLevelCap = -1;
        public RankType type = RankType.Nothing;
    }
}