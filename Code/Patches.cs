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
using HarmonyLib;
using ai;
using ai.behaviours;
using ai.behaviours.conditions;

namespace CommissionMod
{
    class Patches
    {
        public static Harmony harmony = new Harmony("dej.mymod.wb.commissionmod");

        public static void init()
        {
            harmony.Patch(AccessTools.Method(typeof(Actor), "addExperience"), 
            prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "addExperience_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(Actor), "getExpToLevelup"), 
            prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "getExpToLevelup_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(ActorStatus), "generateCivUnitTraits"), 
            prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "generateCivUnitTraits_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(ActorBase), "updateStats"), 
            prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "updateStats_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(ActorStatus), "updateAge"), 
            prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "updateAge_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(Actor), "updateAge"), 
            postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "updateAge_Postfix")));
            harmony.Patch(AccessTools.Method(typeof(Actor), "getHit"), 
            prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "getHit_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(StatusLibrary), "burningEffect"), 
            prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "burningEffect_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(WindowCreatureInfo), "OnEnable"), 
            postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "OnEnable_Postfix")));
            harmony.Patch(AccessTools.Method(typeof(ActorBase), "addTrait"), 
            prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "addTrait_Prefix")));
        }

        public static bool getExpToLevelup_Prefix(Actor __instance, ref int __result)
        {
            int initialexp = 0;
            int expGap = int.Parse(UI.getOption("expGapOption"));
            if (expGap <= -1)
            {
                expGap = 20;
                initialexp = 100;
            }
            int normalexp = initialexp + (__instance.data.level) * expGap;
            foreach (KeyValuePair<string, SavedTrait> kv in Traits.talentIDs)
            {
                if (__instance.haveTrait(kv.Key))
                {
                    __result = (int)(normalexp - (normalexp*kv.Value.decreaseEXPRequirement));
                    return false;
                }
            }
            __result = normalexp;
            return false;
        }

        public static bool addExperience_Prefix(int pValue, Actor __instance)
        {
            if (!__instance.stats.canLevelUp)
            {
                return false;
            }
            int capValue = int.Parse(UI.getOption("levelCapOption"));
            if (capValue > -1 && __instance.data.level >= capValue)
            {
                return false;
            }
            // Culture culture = base.getCulture();
            // if (culture != null)
            // {
            //     num += culture.getMaxLevelBonus();
            // }
            int expGain = 0;
            string talentID = hasTalent(__instance.data);
            if (talentID != null)
            {
                expGain = Traits.talentIDs[talentID].expGain;
            }
            if (pValue < 0)
            {
                expGain = 0;
                pValue = Math.Abs(pValue);
            }
            int expToLevelup = __instance.getExpToLevelup();
            __instance.data.experience += pValue + expGain;
            if (__instance.data.experience >= expToLevelup)
            {
                __instance.data.experience = 0;
                __instance.data.level++;
                if (__instance.data.level >= capValue)
                {
                    __instance.data.experience = expToLevelup;
                    if (__instance.stats.flag_turtle)
                    {
                        AchievementLibrary.achievementNinjaTurtle.check(null, null, null);
                    }
                }
                __instance.setStatsDirty();
                if ((int)(__instance.data.level % 10) == 0)
                {
                    __instance.event_full_heal = true;
                }
            }

            return false;
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

        public static bool generateCivUnitTraits_Prefix(ActorStatus __instance)
        {
            bool flag = false;
            Dictionary<string, SavedTrait> reversedTalentIDS = Traits.talentIDs.Reverse().ToDictionary(x=>x.Key,x=>x.Value);
            for (int i = 0; i < Traits.talentIDs.Count; i++)
            {
                string traitID = reversedTalentIDS.ElementAt(i).Key;
                ActorTrait actorTrait = AssetManager.traits.get(traitID);
                if (actorTrait.birth != 0f)
                {
                    float num = Toolbox.randomFloat(0f, 100f);
                    if (actorTrait.birth >= num && !__instance.traits.Contains(actorTrait.id) && !__instance.haveOppositeTrait(actorTrait))
                    {
                        __instance.addTrait(actorTrait.id);
                        flag = true;
                    }
                }
            }

            if (!flag)
            {
                __instance.addTrait("Frank");
            }
            return true;
        }

        public static bool updateStats_Prefix(ActorBase __instance)
        {
            // base.updateStats();
            __instance.statsDirty = false;
            if (!__instance.data.alive)
            {
                return false;
            }
            if (__instance.stats.useSkinColors && __instance.data.skin_set == -1 && __instance.stats.color_sets != null)
            {
                __instance.setSkinSet("default");
            }
            if (__instance.stats.useSkinColors && __instance.data.skin == -1)
            {
                __instance.data.skin = Toolbox.randomInt(0, __instance.stats.color_sets[__instance.data.skin_set].colors.Count);
            }
            if (string.IsNullOrEmpty(__instance.data.mood))
            {
                __instance.data.mood = "normal";
            }
            MoodAsset moodAsset = AssetManager.moods.get(__instance.data.mood);
            __instance.curStats.clear();
            __instance.curStats.addStats(__instance.stats.baseStats);
            __instance.curStats.addStats(moodAsset.baseStats);
            __instance.curStats.diplomacy += __instance.data.diplomacy;
            __instance.curStats.stewardship += __instance.data.stewardship;
            __instance.curStats.intelligence += __instance.data.intelligence;
            __instance.curStats.warfare += __instance.data.warfare;
            if (__instance.activeStatus_dict != null)
            {
                foreach (StatusEffectData statusEffectData in __instance.activeStatus_dict.Values)
                {
                    __instance.curStats.addStats(statusEffectData.asset.baseStats);
                }
            }
            ItemAsset itemAsset = AssetManager.items.get(__instance.stats.defaultAttack);
            if (itemAsset != null)
            {
                __instance.curStats.addStats(itemAsset.baseStats);
            }
            __instance.s_attackType = __instance.getWeaponAsset().attackType;
            __instance.s_slashType = __instance.getWeaponAsset().slash;
            __instance.item_sprite_dirty = true;
            if (__instance.stats.use_items && !__instance.equipment.weapon.isEmpty())
            {
                __instance.s_weapon_texture = __instance.getWeaponId();
            }
            else
            {
                __instance.s_weapon_texture = string.Empty;
            }
            for (int i = 0; i < __instance.data.traits.Count; i++)
            {
                string pID = __instance.data.traits[i];
                ActorTrait actorTrait = AssetManager.traits.get(pID);
                if (actorTrait != null)
                {
                    __instance.curStats.addStats(actorTrait.baseStats);
                }
            }
            if (__instance.stats.unit)
            {
                __instance.s_personality = null;
                if ((__instance.kingdom != null && __instance.kingdom.isCiv() && __instance.kingdom.king == __instance) || (__instance.city != null && __instance.city.leader == __instance))
                {
                    string pID2 = "balanced";
                    int num = __instance.curStats.diplomacy;
                    if (__instance.curStats.diplomacy > __instance.curStats.stewardship)
                    {
                        pID2 = "diplomat";
                        num = __instance.curStats.diplomacy;
                    }
                    else if (__instance.curStats.diplomacy < __instance.curStats.stewardship)
                    {
                        pID2 = "administrator";
                        num = __instance.curStats.stewardship;
                    }
                    if (__instance.curStats.warfare > num)
                    {
                        pID2 = "militarist";
                    }
                    __instance.s_personality = AssetManager.personalities.get(pID2);
                    __instance.curStats.addStats(__instance.s_personality.baseStats);
                }
            }
            __instance._trait_weightless = __instance.haveTrait("weightless");
            __instance._status_frozen = __instance.haveStatus("frozen");
            int statIncrease = (int)(__instance.data.level/10);
            __instance.curStats.damage += ((__instance.data.level - 1) / 2) + (statIncrease*int.Parse(Main.savedStats.inputOptions["Damage"]));
            __instance.curStats.armor += (__instance.data.level - 1) / 3;
            __instance.curStats.crit += (float)((__instance.data.level - 1) + (statIncrease*int.Parse(Main.savedStats.inputOptions["Critical"])));
            __instance.curStats.attackSpeed += (float)((__instance.data.level - 1) + (statIncrease*int.Parse(Main.savedStats.inputOptions["Attack Speed"])));
            __instance.curStats.health += ((__instance.data.level - 1) * 20) + (statIncrease*int.Parse(Main.savedStats.inputOptions["Health"]));
            bool flag = __instance.haveTrait("madness");
            __instance.data.s_traits_ids.Clear();
            List<ActorTrait> list = __instance.s_special_effect_traits;
            if (list != null)
            {
                list.Clear();
            }
            for (int j = 0; j < __instance.data.traits.Count; j++)
            {
                string text = __instance.data.traits[j];
                __instance.data.s_traits_ids.Add(text);
                ActorTrait actorTrait2 = AssetManager.traits.get(text);
                if (actorTrait2 != null && actorTrait2.action_special_effect != null)
                {
                    if (__instance.s_special_effect_traits == null)
                    {
                        __instance.s_special_effect_traits = new List<ActorTrait>();
                    }
                    __instance.s_special_effect_traits.Add(actorTrait2);
                }
            }
            __instance.findHeadSprite();
            bool flag2 = __instance.haveTrait("madness");
            List<ActorTrait> list2 = __instance.s_special_effect_traits;
            if (list2 != null && list2.Count == 0)
            {
                __instance.s_special_effect_traits = null;
            }
            if (flag2 != flag)
            {
                __instance.checkMadness(flag2);
            }
            __instance._trait_peaceful = __instance.haveTrait("peaceful");
            __instance._trait_fire_resistant = __instance.haveTrait("fire_proof");
            if (__instance.stats.use_items)
            {
                List<ActorEquipmentSlot> list3 = ActorEquipment.getList(__instance.equipment, false);
                for (int k = 0; k < list3.Count; k++)
                {
                    ActorEquipmentSlot actorEquipmentSlot = list3[k];
                    if (actorEquipmentSlot.data != null)
                    {
                        ItemTools.calcItemValues(actorEquipmentSlot.data);
                        __instance.curStats.addStats(ItemTools.s_stats);
                    }
                }
            }
            __instance.curStats.normalize();
            __instance.curStats.health += (int)((float)__instance.curStats.health * (__instance.curStats.mod_health / 100f));
            __instance.curStats.damage += (int)((float)__instance.curStats.damage * (__instance.curStats.mod_damage / 100f));
            __instance.curStats.armor += (int)((float)__instance.curStats.armor * (__instance.curStats.mod_armor / 100f));
            __instance.curStats.crit += (float)((int)(__instance.curStats.crit * (__instance.curStats.mod_crit / 100f)));
            __instance.curStats.diplomacy += (int)((float)__instance.curStats.diplomacy * (__instance.curStats.mod_diplomacy / 100f));
            __instance.curStats.speed += (float)((int)(__instance.curStats.speed * (__instance.curStats.mod_speed / 100f)));
            __instance.curStats.attackSpeed += (float)((int)(__instance.curStats.attackSpeed * (__instance.curStats.mod_attackSpeed / 100f)));
            if (__instance.event_full_heal)
            {
                __instance.event_full_heal = false;
                __instance.data.health = __instance.curStats.health;
            }
            Culture culture = __instance.getCulture();
            if (culture != null)
            {
                __instance.curStats.damage = (int)((float)__instance.curStats.damage + (float)__instance.curStats.damage * culture.stats.bonus_damage.value);
                __instance.curStats.armor = (int)((float)__instance.curStats.armor + (float)__instance.curStats.armor * culture.stats.bonus_armor.value);
            }
            if (__instance.curStats.health < 1)
            {
                __instance.curStats.health = 1;
            }
            if (__instance.data.health > __instance.curStats.health)
            {
                __instance.data.health = __instance.curStats.health;
            }
            if (__instance.data.health < 1)
            {
                __instance.data.health = 1;
            }
            if (__instance.curStats.damage < 1)
            {
                __instance.curStats.damage = 1;
            }
            if (__instance.curStats.speed < 1f)
            {
                __instance.curStats.speed = 1f;
            }
            if (__instance.curStats.armor < 0)
            {
                __instance.curStats.armor = 0;
            }
            if (__instance.curStats.diplomacy < 0)
            {
                __instance.curStats.diplomacy = 0;
            }
            if (__instance.curStats.dodge < 0f)
            {
                __instance.curStats.dodge = 0f;
            }
            if (__instance.curStats.accuracy < 10f)
            {
                __instance.curStats.accuracy = 10f;
            }
            if (__instance.curStats.crit < 0f)
            {
                __instance.curStats.crit = 0f;
            }
            if (__instance.curStats.attackSpeed < 0f)
            {
                __instance.curStats.attackSpeed = 1f;
            }
            if (__instance.curStats.attackSpeed >= 300f)
            {
                __instance.curStats.attackSpeed = 300f;
            }
            __instance.s_attackSpeed_seconds = (300f - __instance.curStats.attackSpeed) / (100f + __instance.curStats.attackSpeed);
            __instance.curStats.s_crit_chance = __instance.curStats.crit / 100f;
            __instance.curStats.zone_range += __instance.curStats.stewardship / 10;
            __instance.curStats.cities += __instance.curStats.stewardship / 6 + 1;
            __instance.curStats.army += __instance.curStats.warfare + 5;
            __instance.curStats.bonus_towers = __instance.curStats.warfare / 10;
            if (__instance.curStats.bonus_towers > 2)
            {
                __instance.curStats.bonus_towers = 2;
            }
            if (__instance.curStats.army < 0)
            {
                __instance.curStats.army = 5;
            }
            __instance.attackTimer = 0f;
            __instance.updateTargetScale();
            __instance.curStats.normalize();
            __instance.currentScale.x = __instance.curStats.scale;
            __instance.currentScale.y = __instance.curStats.scale;
            __instance.currentScale.z = __instance.curStats.scale;
            return false;
        }

        public static void updateAge_Postfix(Actor __instance)
        {
            if (__instance.stats.unit)
            {
                if (__instance.data.age % 2 == 0)
                {
                    __instance.restoreHealth((int)(__instance.curStats.health*0.1f));
                }
                foreach (KeyValuePair<string, SavedTrait> kv in Traits.talentIDs)
                {
                    if (__instance.haveTrait(kv.Key))
                    {
                        __instance.addExperience((int)(kv.Value.passiveExpGain*-1));
                    }
                }
            }
        }

        public static bool getHit_Prefix(Actor __instance, float pDamage, bool pFlash = true, AttackType pType = AttackType.Other, BaseSimObject pAttacker = null, bool pSkipIfShake = true)
        {
            __instance.attackedBy = null;
            if (pSkipIfShake && __instance.shakeTimer.isActive)
            {
                return false;
            }
            if (__instance.data.health <= 0)
            {
                return false;
            }
            if (pType == AttackType.Other)
            {
                float num = 1f - (float)__instance.curStats.armor / 100f;
                pDamage *= num;
                if (pAttacker != null && pAttacker != __instance && pAttacker.isActor())
                {
                    if ((int)(__instance.data.level/10) > (int)(pAttacker.a.data.level/10) && __instance.data.level >= int.Parse(UI.getOption("InitialLevel")))
                    {
                        pDamage *= 1f - float.Parse(UI.getOption("DMGReductionPercent"));
                    }
                }
            }
            if (pDamage < 1f)
            {
                pDamage = 1f;
            }
            __instance.data.health -= (int)pDamage;
            __instance.timer_action = 0.002f;
            if (pType == AttackType.Other && !__instance.stats.immune_to_injuries && !__instance.haveStatus("shield"))
            {
                if (Toolbox.randomChance(0.02f))
                {
                    __instance.addTrait("crippled", false);
                }
                if (Toolbox.randomChance(0.02f))
                {
                    __instance.addTrait("eyepatch", false);
                }
                __instance.addExperience(int.Parse(UI.getOption("getHitOption"))*-1f);
            }
            if (pFlash)
            {
                __instance.startColorEffect("red");
            }
            if (__instance.data.health <= 0)
            {
                if (pAttacker != __instance)
                {
                    __instance.attackedBy = pAttacker;
                }
                __instance.killHimself(false, pType, true, true);
                if (pAttacker != null && pAttacker != __instance && pAttacker.isActor())
                {
                    BattleKeeperManager.unitKilled(__instance);
                    pAttacker.a.newKillAction(__instance);
                    if (pAttacker.city != null)
                    {
                        bool flag = false;
                        if (__instance.stats.animal)
                        {
                            flag = true;
                            pAttacker.city.data.storage.change("meat", 1);
                        }
                        else if (__instance.stats.unit && pAttacker.a.haveTrait("savage"))
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            if (Toolbox.randomChance(0.5f))
                            {
                                pAttacker.city.data.storage.change("bones", 1);
                                return false;
                            }
                            if (Toolbox.randomChance(0.5f))
                            {
                                pAttacker.city.data.storage.change("leather", 1);
                                return false;
                            }
                            if (Toolbox.randomChance(0.5f))
                            {
                                pAttacker.city.data.storage.change("meat", 1);
                                return false;
                            }
                        }
                    }
                }
            }
            else
            {
                __instance.shakeTimer.startTimer(-1f);
                if (pAttacker != __instance)
                {
                    __instance.attackedBy = pAttacker;
                }
                if (__instance.attackTarget == null && __instance.attackedBy != null && !__instance.targetsToIgnore.Contains(__instance.attackedBy) && __instance.canAttackTarget(__instance.attackedBy))
                {
                    __instance.attackTarget = __instance.attackedBy;
                }
                if (__instance.activeStatus_dict != null)
                {
                    foreach (StatusEffectData statusEffectData in __instance.activeStatus_dict.Values)
                    {
                        if (statusEffectData.asset.actionOnHit != null)
                        {
                            statusEffectData.asset.actionOnHit(__instance, __instance.currentTile);
                        }
                    }
                }
                for (int i = 0; i < __instance.callbacks_get_hit.Count; i++)
                {
                    __instance.callbacks_get_hit[i](__instance);
                }
            }
            return false;
        }

        public static bool updateAge_Prefix(ActorStatus __instance, ref bool __result, Race pRace)
        {
            __instance.age++;
            ActorStats actorStats = AssetManager.unitStats.get(__instance.statsID);
            __instance.updateAttributes(actorStats, pRace, false);
            if (!MapBox.instance.worldLaws.world_law_old_age.boolVal)
            {
                __result = true;
                return false;
            }
            int ageIncrease = (int)(__instance.level/10)*int.Parse(Main.savedStats.inputOptions["ageIncreaseOption"]);
            int num = actorStats.maxAge + ageIncrease;
            Culture culture = MapBox.instance.cultures.get(__instance.culture);
            if (culture != null)
            {
                num += culture.getMaxAgeBonus();
            }
            __result = num == 0 || num > __instance.age || !Toolbox.randomChance(0.15f);
            return false;
        }

        public static bool burningEffect_Prefix(ref bool __result, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget.isActor() && pTarget.a.stats.have_skin && Toolbox.randomBool())
            {
                pTarget.a.addTrait("skin_burns", false);
            }
            int num = (int)((float)pTarget.curStats.health * 0.1f) + 1;
            if (pTarget.isActor())
            {
                int trueDMG = 0;
                if (pTarget.a.data.level > 9)
                {
                    int intFlag = (pTarget.a.data.health+1) - num;
                    trueDMG = (intFlag > 0)? 0:intFlag;
                }
                if (pTarget.a.data.level < 9 || pTarget.a.data.health > pTarget.a.data.level*2)
                {
                    pTarget.getHit((float)num + trueDMG, true, AttackType.Fire, null, true);
                }
            }
            if (!MapBox.instance.qualityChanger.lowRes && Toolbox.randomChance(0.1f))
            {
                MapBox.instance.particlesFire.spawn(pTarget.currentPosition.x, pTarget.currentPosition.y, true);
            }
            __result = true;
            return false;
        }

        public static void OnEnable_Postfix(WindowCreatureInfo __instance)
        {
            if (Config.selectedUnit == null)
            {
                return;
            }
            Localization.AddOrSet("dej_new_age", "Age With Life Span");
            ActorStats stats = AssetManager.unitStats.get(Config.selectedUnit.data.statsID);
            __instance.showStat("dej_new_age",
            Config.selectedUnit.data.age.ToString() + "/" + (stats.maxAge + (int)(Config.selectedUnit.data.level/10)*int.Parse(Main.savedStats.inputOptions["ageIncreaseOption"])).ToString()
            );
        }

        public static bool addTrait_Prefix(ActorBase __instance, string pTrait, bool pRemoveOpposites = false)
        {
            if (__instance.haveTrait(pTrait))
            {
                return false;
            }
            if (pRemoveOpposites)
            {
                __instance.removeOppositeTraits(pTrait);
            }
            if (__instance.haveOppositeTrait(pTrait))
            {
                return false;
            }
            __instance.data.traits.Add(pTrait);
            __instance.setStatsDirty();
            return true;
        }
    }
}