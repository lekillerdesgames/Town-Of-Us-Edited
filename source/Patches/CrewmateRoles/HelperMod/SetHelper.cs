using System;
using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using Object = UnityEngine.Object;
using TownOfUsEdited.Patches;

namespace TownOfUsEdited.CrewmateRoles.HelperMod
{
    [HarmonyPatch(typeof(AirshipExileController._WrapUpAndSpawn_d__11), nameof(AirshipExileController._WrapUpAndSpawn_d__11.MoveNext))]
    public static class AirshipExileController_WrapUpAndSpawn
    {
        public static void Postfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance) => SetHelper.ExileControllerPostfix(__instance.__4__this);
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class SetHelper
    {
        public static PlayerControl WillBeHelper;

        public static void ExileControllerPostfix(ExileController __instance)
        {
            if (WillBeHelper == null) return;
            if (WillBeHelper.Data.Disconnected) return;

            if (!WillBeHelper.Is(RoleEnum.Helper))
            {
                var oldRole = Role.GetRole(WillBeHelper);
                var killsList = (oldRole.CorrectKills, oldRole.IncorrectKills, oldRole.CorrectAssassinKills, oldRole.IncorrectAssassinKills);
                Role.RoleDictionary.Remove(WillBeHelper.PlayerId);
                if (PlayerControl.LocalPlayer == WillBeHelper)
                {
                    var role = new Helper(PlayerControl.LocalPlayer);
                    role.formerRole = oldRole.RoleType;
                    role.CorrectKills = killsList.CorrectKills;
                    role.IncorrectKills = killsList.IncorrectKills;
                    role.CorrectAssassinKills = killsList.CorrectAssassinKills;
                    role.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                    role.DeathReason = oldRole.DeathReason;
                    role.RegenTask();
                }
                else
                {
                    var role = new Helper(WillBeHelper);
                    role.formerRole = oldRole.RoleType;
                    role.CorrectKills = killsList.CorrectKills;
                    role.IncorrectKills = killsList.IncorrectKills;
                    role.CorrectAssassinKills = killsList.CorrectAssassinKills;
                    role.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                    role.DeathReason = oldRole.DeathReason;
                    //Not necessary to regen task here, I only do it so it looks good on mci lol
                    role.RegenTask();
                }
            }
        }

        public static void Postfix(ExileController __instance) => ExileControllerPostfix(__instance);

        [HarmonyPatch(typeof(Object), nameof(Object.Destroy), new Type[] { typeof(GameObject) })]
        public static void Prefix(GameObject obj)
        {
            if (!SubmergedCompatibility.Loaded || GameOptionsManager.Instance?.currentNormalGameOptions?.MapId != 6) return;
            if (obj.name?.Contains("ExileCutscene") == true) ExileControllerPostfix(ExileControllerPatch.lastExiled);
        }
    }
}