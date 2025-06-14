using System.Collections;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.ImpostorRoles.SwooperMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKillSwooper
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Swooper);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Swooper>(PlayerControl.LocalPlayer);
            if (__instance == role.SwoopButton)
            {
                if (__instance.isCoolingDown) return false;
                if (!__instance.isActiveAndEnabled) return false;
                if (role.Cooldown > 0) return false;
                var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                if (!abilityUsed) return false;

                Utils.Rpc(CustomRPC.Swoop, PlayerControl.LocalPlayer.PlayerId);
                Coroutines.Start(Swoop(role));
                return false;
            }

            return true;
        }

        public static IEnumerator Swoop(Swooper role)
        {
            Utils.Swoop(role.Player, true);
            role.Cooldown = 2f;
            yield return new WaitForSeconds(2);
            role.Swoop();
            role.TimeRemaining = CustomGameOptions.SwoopDuration;
        }
    }
}