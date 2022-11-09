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
                250
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
                300
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
                320
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
                350
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
                370
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
                400
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
                450
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
                480
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
                510
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
                550
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
                1
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
                1
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
                1
            );

            addTalentList();
        }

        private static void createTalentTrait(string newID, string icon, string title, string desc, float newBirth, BaseStats newStats, int expGainMod, float percentageMod, int passive)
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
                expGain = expGainMod,
                spawnRate = newBirth,
                decreaseEXPRequirement = percentageMod,
                passiveExpGain = passive
            };
            if (Main.hasSettings)
            {
                newTrait = Main.savedStats.traits[talent.id];
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
        public int expGain = 0;
        public float spawnRate = 0f;
        public float decreaseEXPRequirement = 0f;
        public int passiveExpGain = 0;
    }
}