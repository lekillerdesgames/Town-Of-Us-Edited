using HarmonyLib;
using TownOfUsEdited.Roles.Modifiers;

namespace TownOfUsEdited.Modifiers.SleuthMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    public static class BodyReport
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo info)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(ModifierEnum.Sleuth)) return;
            if (PlayerControl.LocalPlayer != __instance) return;

            Modifier.GetModifier<Sleuth>(PlayerControl.LocalPlayer).Reported.Add(info.PlayerId);
        }
    }
}