using HarmonyLib;
using TownOfUsEdited.Roles;
using AmongUs.GameOptions;

namespace TownOfUsEdited.CrewmateRoles.PoliticianMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Politician)) return true;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (__instance != HudManager.Instance.KillButton) return true;
            if (!__instance.enabled) return false;
            var role = Role.GetRole<Politician>(PlayerControl.LocalPlayer);
            if (role.Cooldown > 0 || !role.CanCampaign) return false;
            if (role.ClosestPlayer == null) return false;
            if (role.CampaignedPlayers.Contains(role.ClosestPlayer.PlayerId)) return false;
            var distBetweenPlayers = Utils.GetDistBetweenPlayers(PlayerControl.LocalPlayer, role.ClosestPlayer);
            var flag3 = distBetweenPlayers <
                        LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            if (!flag3) return false;
            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
            if (interact[4] == true)
            {
                role.CampaignedPlayers.Add(role.ClosestPlayer.PlayerId);
            }
            if (interact[0] == true)
            {
                role.Cooldown = CustomGameOptions.CampaignCd;
                return false;
            }
            else if (interact[3] == true) return false;
            return false;
        }
    }
}