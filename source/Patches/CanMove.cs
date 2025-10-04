using HarmonyLib;

namespace TownOfUsEdited.Patches
{
    public static class CanMove
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
        internal static class CanMovePatch
        {
            public static void Postfix(ref bool __result)
            {
                // Allows players to move when intro scene is played cuz apparently donners like it...
                // Tho I have to say its quite useful which is why I kept the patch (:
                __result = (__result || HudManager.Instance.IsIntroDisplayed) && !Minigame.Instance;
            }
        }
    }
}