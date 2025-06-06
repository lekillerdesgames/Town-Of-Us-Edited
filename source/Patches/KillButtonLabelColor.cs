using HarmonyLib;
using TownOfUsEdited.Roles;

namespace KillButtonLabelColor
{
    public class ColorPatch
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class KillButtonLabelColor
        {
            public static void Postfix(HudManager __instance)
            {
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started &&
                AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) return;
                var role = Role.GetRole(PlayerControl.LocalPlayer);
                if (role == null) return;
                if (!__instance.KillButton.buttonLabelText.isActiveAndEnabled) return;
                __instance.KillButton.buttonLabelText.SetOutlineColor(role.Color);
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class VentButtonLabelColor
        {
            public static void Postfix(HudManager __instance)
            {
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started &&
                AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) return;
                var role = Role.GetRole(PlayerControl.LocalPlayer);
                if (role == null) return;
                if (!__instance.ImpostorVentButton.buttonLabelText.isActiveAndEnabled) return;
                __instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(role.Color);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.IsKillTimerEnabled), MethodType.Getter)]
        public static class SpawnMinigameKillTimer
        {
            public static void Postfix(ref bool __result)
            {
                __result = __result || (Minigame.Instance && Minigame.Instance.TryCast<SpawnInMinigame>() != null);
            }
        }
    }
}