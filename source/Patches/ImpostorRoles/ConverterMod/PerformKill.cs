﻿using AmongUs.GameOptions;
using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.Patches.ImpostorRoles.ConverterMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Converter))
                return true;

            if (PlayerControl.LocalPlayer.Data.IsDead)
                return false;

            if (!PlayerControl.LocalPlayer.CanMove)
                return false;

            var role = Role.GetRole<Converter>(PlayerControl.LocalPlayer);
            
            if (role.CurrentTarget == null)
                return false;
                
            if (__instance == role.ConvertButton) 
            {
                if (role.Cooldown > 0)
                    return false;

                if (!__instance.isActiveAndEnabled) return false;
                var maxDistance = LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];

                if (Vector2.Distance(role.CurrentTarget.TruePosition,
                    PlayerControl.LocalPlayer.GetTruePosition()) > maxDistance) return false;
                
                var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                if (!abilityUsed) return false;

                role.ReviveCount++;
                role.ConvertAbility(role.CurrentTarget);
                return false;
            }

            return false;
        }
    }
}