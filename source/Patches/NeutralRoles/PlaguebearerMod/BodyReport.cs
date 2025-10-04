using HarmonyLib;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.NeutralRoles.PlaguebearerMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    public class BodyReport
    {
        private static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo info)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (PlayerControl.LocalPlayer != __instance) return;
            if (info == null) return;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!info.Disconnected && player.PlayerId == info.PlayerId)
                {
                    if (PlayerControl.LocalPlayer.IsInfected() || player.IsInfected())
                    {
                        foreach (var pb in Role.GetRoles(RoleEnum.Plaguebearer)) ((Plaguebearer)pb).RpcSpreadInfection(PlayerControl.LocalPlayer, player);
                    }
                }
            }
        }
    }
}