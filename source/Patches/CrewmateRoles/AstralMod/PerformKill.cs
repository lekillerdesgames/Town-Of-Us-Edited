using HarmonyLib;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.CrewmateRoles.AstralMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Astral);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Astral>(PlayerControl.LocalPlayer);
            var ghostButton = DestroyableSingleton<HudManager>.Instance.KillButton;
            if (__instance == ghostButton)
            {
                if (__instance.isCoolingDown) return false;
                if (!__instance.isActiveAndEnabled) return false;
                if (role.Cooldown > 0) return false;
                var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                if (!abilityUsed) return false;
                role.TimeRemaining = CustomGameOptions.GhostDuration;
                role.Die(PlayerControl.LocalPlayer);
                Utils.Rpc(CustomRPC.TurnGhost, PlayerControl.LocalPlayer.PlayerId);
                return false;
            }

            return true;
        }
    }
}