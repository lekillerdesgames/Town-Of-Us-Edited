using HarmonyLib;

namespace TownOfUsEdited
{
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public class AmBanned
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
}