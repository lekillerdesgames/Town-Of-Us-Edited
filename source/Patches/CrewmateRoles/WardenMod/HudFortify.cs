﻿using HarmonyLib;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.CrewmateRoles.WardenMod
{
    [HarmonyPatch(typeof(HudManager))]
    public class HudFortify
    {
        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Warden)) return;
            var fortifyButton = __instance.KillButton;

            var role = Role.GetRole<Warden>(PlayerControl.LocalPlayer);

            fortifyButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ||
                    AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay));
            fortifyButton.SetCoolDown(0f, 1f);

            if (role.Fortified == null) Utils.SetTarget(ref role.ClosestPlayer, fortifyButton, float.NaN);
            else fortifyButton.SetTarget(null);
        }
    }
}
