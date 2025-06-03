using HarmonyLib;
using Spirefrost.Builders.Icons;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.Tribes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using WildfrostHopeMod.VFX;
using static Spirefrost.MainModFile;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class IconReplacementPatches
    {
        internal static readonly Dictionary<(KeywordData, string), KeywordData> spliced = new Dictionary<(KeywordData, string), KeywordData>();
        internal static bool screenOpen = false;
        internal static bool choseSpire = false;
        internal static SelectStartingPet currentPetSelect = null;

        static KeywordData GetOrMakeSplicedData(KeywordData original, string otherID)
        {
            if (!spliced.ContainsKey((original, otherID)))
            {
                spliced[(original, otherID)] = null;
            }
            KeywordData ret = spliced[(original, otherID)];
            if (ret)
            {
                return ret;
            }
            KeywordData other = instance.TryGet<KeywordData>(otherID);
            ret = ScriptableObject.CreateInstance<KeywordData>();
            ret.bodyColour = other.bodyColour;
            ret.canStack = original.canStack;
            ret.descKey = original.descKey;
            ret.iconName = other.iconName;
            ret.iconTintHex = other.iconTintHex;
            ret.ModAdded = other.ModAdded;
            ret.noteColour = other.noteColour;
            ret.panelColor = other.panelColor;
            ret.panelSprite = other.panelSprite;
            ret.show = original.show;
            ret.showIcon = original.showIcon;
            ret.showName = original.showName;
            ret.titleColour = other.titleColour;
            ret.titleKey = other.titleKey;
            ret.name = other.name + " over " + original.name;

            spliced[(original, otherID)] = ret;
            return ret;
        }

        internal static bool PlayingSTSLeader()
        {
            //Debug.Log($"Checking if STS Leader:");
            //Debug.Log($"Player exists? {References.Player != null}");
            //Debug.Log($"Class id ({References.PlayerData?.classData?.id}) matches? {References.PlayerData?.classData?.id == SpireTribe.ClassID}");
            if (References.Player != null && References.PlayerData?.classData?.id == SpireTribe.ClassID)
            {
                //Debug.Log($"Okay to replace!");
                return true;
            }
            //Debug.Log($"In select screen? {screenOpen}, Chose spire? {choseSpire}");
            if (screenOpen && choseSpire)
            {
                //Debug.Log($"Okay to replace!");
                return true;
            }
            //Debug.Log($"Do not replace");
            return false;
        }

        internal static bool CheckIconReplace(ReplaceType type)
        {
            return type == ReplaceType.On || (type == ReplaceType.If_StS_Leader && PlayingSTSLeader());
        }

        [HarmonyPatch]
        internal class TextPatches
        {
            [HarmonyPatch(typeof(Text), nameof(Text.ToKeyword))]
            internal static class ToKeywordPatch
            {
                static void Postfix(ref KeywordData __result)
                {
                    if (__result.name == "Demonize" && CheckIconReplace(instance.vulnReplace))
                    {
                        __result = GetOrMakeSplicedData(__result, VulnerableKeyword.FullID);
                    }
                    else if (__result.name == "Frost" && CheckIconReplace(instance.weakReplace))
                    {
                        __result = GetOrMakeSplicedData(__result, WeakKeyword.FullID);
                    }
                    else if (__result.name == "Frost" && CheckIconReplace(instance.shackledReplace))
                    {
                        __result = GetOrMakeSplicedData(__result, ShackledKeyword.FullID);
                    }
                    else if (__result.name == "Shroom" && CheckIconReplace(instance.poisonReplace))
                    {
                        __result = GetOrMakeSplicedData(__result, PoisonKeyword.FullID);
                    }
                    else if (__result.name == "Teeth" && CheckIconReplace(instance.thornsReplace))
                    {
                        __result = GetOrMakeSplicedData(__result, ThornsKeyword.FullID);
                    }
                    else if (__result.name == "Spice" && CheckIconReplace(instance.vigorReplace))
                    {
                        __result = GetOrMakeSplicedData(__result, VigorKeyword.FullID);
                    }
                    else if (__result.name == "Haze" && CheckIconReplace(instance.confusedReplace))
                    {
                        __result = GetOrMakeSplicedData(__result, ConfusedKeyword.FullID);
                    }
                }
            }
        }

        [HarmonyPatch]
        internal class CharacterSelectScreenPatches
        {
            [HarmonyPatch(typeof(CharacterSelectScreen), nameof(CharacterSelectScreen.ContinueRoutine))]
            internal static class ContinueRoutinePatch
            {
                static void Postfix()
                {
                    screenOpen = false;
                    choseSpire = false;
                    currentPetSelect = null;
                }
            }

            [HarmonyPatch(typeof(CharacterSelectScreen), nameof(CharacterSelectScreen.ReturnToMenu))]
            internal static class ReturnToMenuPatch
            {
                static void Postfix()
                {
                    screenOpen = false;
                    choseSpire = false;
                    currentPetSelect = null;
                }
            }

            [HarmonyPatch(typeof(CharacterSelectScreen), nameof(CharacterSelectScreen.Start))]
            internal static class StartPatch
            {
                static void Prefix(CharacterSelectScreen __instance)
                {
                    screenOpen = true;
                    currentPetSelect = __instance.petSelection;
                }
            }
        }

        [HarmonyPatch]
        internal class SelectTribePatches
        {
            [HarmonyPatch(typeof(SelectTribe), nameof(SelectTribe.SelectRoutine))]
            internal static class SelectRoutinePatch
            {
                static void Prefix(SelectTribe __instance, ClassData classData)
                {
                    bool hadChoseSpire = choseSpire;
                    choseSpire = classData.id == SpireTribe.ClassID;
                    if (hadChoseSpire != choseSpire && currentPetSelect != null)
                    {
                        Debug.Log($"Updating pet descriptions");
                        foreach (var item in currentPetSelect.pets)
                        {
                            if (item.display is Card card)
                            {
                                card.SetDescription();
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch]
        internal class IconPatches
        {
            [HarmonyPatch(typeof(CardManager), nameof(CardManager.NewStatusIcon))]
            internal static class NewStatusIconPatch
            {
                static StatusIcon MakeSplicedIcon(StatusIcon icon, string type, Transform iconParent, string toReplace, string spriteID, string keywordID)
                {
                    StatusIcon ret = icon;
                    if (CardManager.cardIcons.ContainsKey(spriteID))
                    {
                        // Get original keyword for splicing
                        KeywordData replace = null;
                        if (ret.gameObject.TryGetComponent<CardPopUpTarget>(out CardPopUpTarget originalTarget))
                        {
                            for (int i = 0; i < originalTarget.keywords.Length; i++)
                            {
                                if (originalTarget.keywords[i].name == toReplace)
                                {
                                    replace = originalTarget.keywords[i];
                                    break;
                                }
                            }
                        }

                        // Prevent null crash from checking null target before Destroy can actually happen
                        icon.Assign(instance.dummyEntity);
                        icon.Destroy();

                        // Create new icon and splice keyword
                        ret = UnityEngine.Object.Instantiate(CardManager.cardIcons[spriteID], iconParent).GetComponent<StatusIcon>();
                        ret.type = type;
                        if (replace != null && ret.gameObject.TryGetComponent<CardPopUpTarget>(out CardPopUpTarget newTarget))
                        {
                            for (int i = 0; i < newTarget.keywords.Length; i++)
                            {
                                if (newTarget.keywords[i].name == keywordID)
                                {
                                    newTarget.keywords[i] = GetOrMakeSplicedData(replace, keywordID);
                                }
                            }
                        }
                    }
                    return ret;
                }

                static void Postfix(CardManager __instance, string type, Transform iconParent, ref StatusIcon __result)
                {
                    if (type == "demonize" && CheckIconReplace(instance.vulnReplace))
                    {
                        __result = MakeSplicedIcon(__result, type, iconParent, "Demonize", VulnerableIcon.SpriteID, VulnerableKeyword.FullID);
                    }
                    else if (type == "frost" && CheckIconReplace(instance.weakReplace))
                    {
                        __result = MakeSplicedIcon(__result, type, iconParent, "Frost", WeakIcon.SpriteID, WeakKeyword.FullID);
                    }
                    else if (type == "frost" && CheckIconReplace(instance.shackledReplace))
                    {
                        __result = MakeSplicedIcon(__result, type, iconParent, "Frost", ShackledIcon.SpriteID, ShackledKeyword.FullID);
                    }
                    else if (type == "shroom" && CheckIconReplace(instance.poisonReplace))
                    {
                        __result = MakeSplicedIcon(__result, type, iconParent, "Shroom", PoisonIcon.SpriteID, PoisonKeyword.FullID);
                    }
                    else if (type == "teeth" && CheckIconReplace(instance.thornsReplace))
                    {
                        __result = MakeSplicedIcon(__result, type, iconParent, "Teeth", ThornsIcon.SpriteID, ThornsKeyword.FullID);
                    }
                    else if (type == "spice" && CheckIconReplace(instance.vigorReplace))
                    {
                        __result = MakeSplicedIcon(__result, type, iconParent, "Spice", VigorIcon.SpriteID, VigorKeyword.FullID);
                    }
                    else if (type == "haze" && CheckIconReplace(instance.confusedReplace))
                    {
                        __result = MakeSplicedIcon(__result, type, iconParent, "Haze", ConfusedIcon.SpriteID, ConfusedKeyword.FullID);
                    }
                }
            }
        }

        [HarmonyPatch]
        internal class FXPatches
        {
            static string CheckReplaceEffectType(string type)
            {
                if (type == "demonize" && CheckIconReplace(instance.vulnReplace))
                {
                    return VulnerableIcon.SpriteID;
                }
                else if (type == "frost" && CheckIconReplace(instance.weakReplace))
                {
                    return WeakIcon.SpriteID;
                }
                else if (type == "frost" && CheckIconReplace(instance.shackledReplace))
                {
                    return ShackledIcon.SpriteID;
                }
                else if (type == "shroom" && CheckIconReplace(instance.poisonReplace))
                {
                    return PoisonIcon.SpriteID;
                }
                else if ((type == "teeth" || type == "spikes") && CheckIconReplace(instance.thornsReplace))
                {
                    return ThornsIcon.SpriteID;
                }
                else if (type == "spice" && CheckIconReplace(instance.vigorReplace))
                {
                    return VigorIcon.SpriteID;
                }
                else if (type == "haze" && CheckIconReplace(instance.confusedReplace))
                {
                    return ConfusedIcon.SpriteID;
                }
                return type;
            }

            static string InverseCheckReplaceEffectType(string type)
            {
                if (CheckIconReplace(instance.vulnReplace))
                {
                    if (type == VulnerableIcon.SpriteID)
                    {
                        return "demonize";
                    }
                    else if (type == "demonize")
                    {
                        return "spirefrost.dummyeffecttype";
                    }
                }
                else if (CheckIconReplace(instance.weakReplace))
                {
                    if (type == WeakIcon.SpriteID)
                    {
                        return "frost";
                    }
                    else if (type == "frost")
                    {
                        return "spirefrost.dummyeffecttype";
                    }
                }
                else if (CheckIconReplace(instance.shackledReplace))
                {
                    if (type == ShackledIcon.SpriteID)
                    {
                        return "frost";
                    }
                    else if (type == "frost")
                    {
                        return "spirefrost.dummyeffecttype";
                    }
                }
                else if (CheckIconReplace(instance.poisonReplace))
                {
                    if (type == PoisonIcon.SpriteID)
                    {
                        return "shroom";
                    }
                    else if (type == "shroom")
                    {
                        return "spirefrost.dummyeffecttype";
                    }
                }
                else if (CheckIconReplace(instance.thornsReplace))
                {
                    if (type == ThornsIcon.SpriteID)
                    {
                        return "teeth";
                    }
                    else if (type == "teeth")
                    {
                        return "spirefrost.dummyeffecttype";
                    }
                }
                else if (CheckIconReplace(instance.vigorReplace))
                {
                    if (type == VigorIcon.SpriteID)
                    {
                        return "spice";
                    }
                    else if (type == "spice")
                    {
                        return "spirefrost.dummyeffecttype";
                    }
                }
                else if (CheckIconReplace(instance.confusedReplace))
                {
                    if (type == ConfusedIcon.SpriteID)
                    {
                        return "haze";
                    }
                    else if (type == "haze")
                    {
                        return "spirefrost.dummyeffecttype";
                    }
                }
                return type;
            }

            [HarmonyPatch]
            internal static class HitDamageTypePatch
            {
                static IEnumerable<MethodBase> TargetMethods()
                {
                    yield return AccessTools.Method(typeof(SfxSystem), nameof(SfxSystem.EntityHit));
                    yield return AccessTools.Method(typeof(VFXMod), nameof(VFXMod.OnEntityHitEffectDamageSFXCheckCooldown));
                    yield return AccessTools.Method(typeof(VfxStatusSystem), nameof(VfxStatusSystem.EntityHit));
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    FieldInfo damageType = AccessTools.Field(typeof(Hit), nameof(Hit.damageType));
                    MethodInfo check = AccessTools.Method(typeof(FXPatches), nameof(CheckReplaceEffectType));

                    for (int i = 0; i < codes.Count; i++)
                    {
                        yield return codes[i];
                        if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand as FieldInfo == damageType)
                        {
                            Debug.Log($"HitPatch - Wrapping damageType in replace check");
                            yield return new CodeInstruction(OpCodes.Call, check);
                        }
                    }
                }
            }

            [HarmonyPatch]
            internal static class StatusEffectDataTypePatch
            {
                static IEnumerable<MethodBase> TargetMethods()
                {
                    yield return AccessTools.Method(typeof(SfxSystem), nameof(SfxSystem.StatusApplied));
                    yield return AccessTools.Method(typeof(VFXMod), nameof(VFXMod.OnStatusAppliedCheckCooldown));
                    yield return AccessTools.Method(typeof(VfxStatusSystem), nameof(VfxStatusSystem.StatusApplied));
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    FieldInfo type = AccessTools.Field(typeof(StatusEffectData), nameof(StatusEffectData.type));
                    MethodInfo check = AccessTools.Method(typeof(FXPatches), nameof(CheckReplaceEffectType));

                    for (int i = 0; i < codes.Count; i ++)
                    {
                        yield return codes[i];
                        if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand as FieldInfo == type)
                        {
                            Debug.Log($"StatusAppliedPatch - Wrapping type in replace check");
                            yield return new CodeInstruction(OpCodes.Call, check);
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(VFXMod), nameof(VFXMod.OnEntityHitWhenHitSFXCheckCooldown))]
            internal static class OnHitSFXPatch
            {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    MethodInfo check = AccessTools.Method(typeof(FXPatches), nameof(InverseCheckReplaceEffectType));
                    bool checkInserted = false;
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (!checkInserted && codes[i].opcode == OpCodes.Stloc_2)
                        {
                            Debug.Log($"OnHitSFXPatch - Wrapping type in inverse replace check");
                            checkInserted = true;
                            yield return new CodeInstruction(OpCodes.Call, check);
                        }
                        yield return codes[i];
                    }
                }
            }

            [HarmonyPatch(typeof(VfxHitSystem), nameof(VfxHitSystem.TakeHit))]
            internal static class OnHitVFXPatch
            {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    FieldInfo type = AccessTools.Field(typeof(VfxHitSystem.WithStatusProfile), nameof(VfxHitSystem.WithStatusProfile.statusType));
                    MethodInfo check = AccessTools.Method(typeof(FXPatches), nameof(InverseCheckReplaceEffectType));

                    for (int i = 0; i < codes.Count; i++)
                    {
                        yield return codes[i];
                        if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand as FieldInfo == type)
                        {
                            Debug.Log($"OnHitVFXPatch - Wrapping type in inverse replace check");
                            yield return new CodeInstruction(OpCodes.Call, check);
                        }
                    }
                }
            }
        }
    }
}
