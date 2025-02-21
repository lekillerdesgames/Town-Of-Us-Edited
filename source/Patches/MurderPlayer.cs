using HarmonyLib;

namespace TownOfUsEdited.Patches
{
    [HarmonyPatch]
    public class MurderPlayer
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public class MurderPlayerPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (LobbyBehaviour.Instance) return false;
                Utils.MurderPlayer(__instance, target, true);
                return false;
            }
        }

        [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
        [HarmonyPriority(Priority.Last)]
        public class DoClickPatch
        {
            public static bool Prefix(KillButton __instance, ref bool __runOriginal)
            {
                if (!__runOriginal) return false;
                if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove)
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        PlayerControl.LocalPlayer.CheckMurder(__instance.currentTarget);
                    }
                    __instance.SetTarget(null);
                }
                return false;
            }
        }
    }
}