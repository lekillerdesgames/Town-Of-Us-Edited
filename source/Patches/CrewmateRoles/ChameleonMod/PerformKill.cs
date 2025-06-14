using System.Collections;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.CrewmateRoles.ChameleonMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKillChameleon
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Chameleon);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Chameleon>(PlayerControl.LocalPlayer);
            var swoopButton = DestroyableSingleton<HudManager>.Instance.KillButton;
            if (__instance == swoopButton)
            {
                if (__instance.isCoolingDown) return false;
                if (!__instance.isActiveAndEnabled) return false;
                var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                if (!abilityUsed) return false;
                if (role.Cooldown > 0) return false;
                Coroutines.Start(Swoop(role));
                Utils.Rpc(CustomRPC.ChameleonSwoop, PlayerControl.LocalPlayer.PlayerId);
                return false;
            }

            return true;
        }

        public static IEnumerator Swoop(Chameleon role)
        {
            Utils.Swoop(role.Player, true);
            role.Cooldown = 2f;
            yield return new WaitForSeconds(2);
            role.Swoop();
            role.TimeRemaining = CustomGameOptions.ChamSwoopDuration;
        }
    }
}