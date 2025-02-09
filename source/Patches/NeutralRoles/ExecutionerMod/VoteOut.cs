using HarmonyLib;
using Reactor.Utilities;
using System;
using System.Linq;
using TownOfUs.Patches;
using TownOfUs.Patches.NeutralRoles;
using TownOfUs.Roles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.NeutralRoles.ExecutionerMod
{
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public static class AirshipExileController_WrapUpAndSpawn
    {
        public static void Postfix(AirshipExileController __instance) => MeetingExiledEnd.ExileControllerPostfix(__instance);
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    internal class MeetingExiledEnd
    {
        public static void ExileControllerPostfix(ExileController __instance)
        {
            var exiled = __instance.initData.networkedPlayer;
            if (exiled == null) return;
            var player = exiled.Object;

            foreach (var role in Role.GetRoles(RoleEnum.Troll))
            {
                if (((Troll)role).TrolledPlayer != null && player.PlayerId == ((Troll)role).TrolledPlayer.PlayerId && ((Troll)role).UsedTroll)
                {
                    return;
                }
            }

            foreach (var role in Role.GetRoles(RoleEnum.Executioner))
                if (player.PlayerId == ((Executioner)role).target.PlayerId)
                {
                    ((Executioner)role).Wins();

                    if (CustomGameOptions.NeutralEvilWinEndsGame || !CustomGameOptions.ExecutionerTorment) return;
                    if (PlayerControl.LocalPlayer != ((Executioner)role).Player) return;
                    role.PauseEndCrit = true;

                    byte[] toKill = MeetingHud.Instance.playerStates.Where(x => !Utils.PlayerById(x.TargetPlayerId).Is(RoleEnum.Pestilence) && x.VotedFor == ((Executioner)role).target.PlayerId).Select(x => x.TargetPlayerId).ToArray();
                    var pk = new PlayerMenu((x) => {
                        Utils.RpcMultiMurderPlayer(((Executioner)role).Player, x);
                        role.PauseEndCrit = false;
                    }, (y) => {
                        return toKill.Contains(y.PlayerId);
                    });
                    Coroutines.Start(pk.Open(3f));
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