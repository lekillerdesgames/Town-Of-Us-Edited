using System;
using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using TownOfUsEdited.Patches;
using System.Linq;
using System.Collections.Generic;

namespace TownOfUsEdited.CrewmateRoles.HaunterMod
{
    [HarmonyPatch(typeof(AirshipExileController._WrapUpAndSpawn_d__11), nameof(AirshipExileController._WrapUpAndSpawn_d__11.MoveNext))]
    public static class AirshipExileController_WrapUpAndSpawn
    {
        public static void Postfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance) => SetHaunter.ExileControllerPostfix(__instance.__4__this);
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class SetHaunter
    {
        public static PlayerControl WillBeHaunter;

        public static void ExileControllerPostfix(ExileController __instance)
        {
            if (WillBeHaunter == null) return;
            if (WillBeHaunter.Data.Disconnected) return;

            if (!WillBeHaunter.Is(RoleEnum.Haunter))
            {
                var oldRole = Role.GetRole(WillBeHaunter);
                var killsList = (oldRole.CorrectKills, oldRole.IncorrectKills, oldRole.CorrectAssassinKills, oldRole.IncorrectAssassinKills);
                Role.RoleDictionary.Remove(WillBeHaunter.PlayerId);
                if (PlayerControl.LocalPlayer == WillBeHaunter)
                {
                    var role = new Haunter(PlayerControl.LocalPlayer);
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
                    var role = new Haunter(WillBeHaunter);
                    role.formerRole = oldRole.RoleType;
                    role.CorrectKills = killsList.CorrectKills;
                    role.IncorrectKills = killsList.IncorrectKills;
                    role.CorrectAssassinKills = killsList.CorrectAssassinKills;
                    role.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                    role.DeathReason = oldRole.DeathReason;
                    // Not necessary to regen task here, I only do it so it looks good on mci lol
                    role.RegenTask();
                }

                Utils.RemoveTasks(WillBeHaunter);
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Phantom) && !PlayerControl.LocalPlayer.Is(RoleEnum.Spirit)) WillBeHaunter.MyPhysics.ResetMoveState();

                WillBeHaunter.gameObject.layer = LayerMask.NameToLayer("Players");
            }

            WillBeHaunter.gameObject.GetComponent<PassiveButton>().OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            WillBeHaunter.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => WillBeHaunter.OnClick()));
            WillBeHaunter.gameObject.GetComponent<BoxCollider2D>().enabled = true;

            if (PlayerControl.LocalPlayer != WillBeHaunter) return;

            if (Role.GetRole<Haunter>(PlayerControl.LocalPlayer).Caught) return;

            List<Vent> vents = new();
            var CleanVentTasks = PlayerControl.LocalPlayer.myTasks.ToArray().Where(x => x.TaskType == TaskTypes.VentCleaning).ToList();
            if (CleanVentTasks != null)
            {
                var ids = CleanVentTasks.Where(x => !x.IsComplete)
                                        .ToList()
                                        .ConvertAll(x => x.FindConsoles()[0].ConsoleId);

                vents = ShipStatus.Instance.AllVents.Where(x => !ids.Contains(x.Id)).ToList();
            }
            else vents = ShipStatus.Instance.AllVents.ToList();

            var startingVent = vents[Random.RandomRangeInt(0, vents.Count)];


            Utils.Rpc(CustomRPC.SetPos, PlayerControl.LocalPlayer.PlayerId, startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f);
            var pos = new Vector2(startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f);

            PlayerControl.LocalPlayer.transform.position = pos;
            PlayerControl.LocalPlayer.NetTransform.SnapTo(pos);
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(pos);
            PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(startingVent.Id);
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