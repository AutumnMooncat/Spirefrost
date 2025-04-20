using HarmonyLib;


namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(References), nameof(References.Classes), MethodType.Getter)]
    internal static class FixClassesGetter
    {
        static void Postfix(ref ClassData[] __result) => __result = AddressableLoader.GetGroup<ClassData>("ClassData").ToArray();
    }
}
