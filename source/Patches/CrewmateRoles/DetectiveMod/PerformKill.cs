using System;
using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using Reactor.Utilities;
using AmongUs.GameOptions;
using TownOfUsEdited.Extensions;

namespace TownOfUsEdited.CrewmateRoles.DetectiveMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Detective)) return true;
            var role = Role.GetRole<Detective>(PlayerControl.LocalPlayer);
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (!__instance.enabled) return false;
            var maxDistance = LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];

            if (__instance == role.ExamineButton)
            {
                if (role.Cooldown > 0) return false;
                if (role.InvestigatingScene == null) return false;
                if (role.ClosestPlayer == null) return false;
                if (Vector2.Distance(role.ClosestPlayer.GetTruePosition(),
                    PlayerControl.LocalPlayer.GetTruePosition()) > maxDistance) return false;
                var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
                if (interact[4] == true)
                {
                    role.Cooldown = CustomGameOptions.ExamineCd;
                    if (role.InvestigatedPlayers.Contains(role.ClosestPlayer.PlayerId))
                    {
                        Coroutines.Start(Utils.FlashCoroutine(Color.red));
                        var deadPlayer = role.InvestigatingScene.DeadPlayer;
                        if (DestroyableSingleton<HudManager>.Instance)
                            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{role.ClosestPlayer.GetDefaultOutfit().PlayerName} was at the scene of {deadPlayer.GetDefaultOutfit().PlayerName}'s death!");
                    }
                    else Coroutines.Start(Utils.FlashCoroutine(Color.green));
                }
                if (interact[0] == true)
                {
                    role.Cooldown = CustomGameOptions.ExamineCd;
                    return false;
                }
                else if (interact[1] == true)
                {
                    role.Cooldown = CustomGameOptions.TempSaveCdReset;
                    return false;
                }
                else if (interact[3] == true) return false;
                return false;
            }
            else
            {
                if (__instance != HudManager.Instance.KillButton) return true;
                if (role.CurrentTarget == null)
                    return false;
                if (Vector2.Distance(role.CurrentTarget.gameObject.transform.position,
                    PlayerControl.LocalPlayer.GetTruePosition()) > maxDistance) return false;
                var player = role.CurrentTarget.DeadPlayer;
                var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                if (!abilityUsed) return false;
                if (player.IsInfected() || role.Player.IsInfected())
                {
                    foreach (var pb in Role.GetRoles(RoleEnum.Plaguebearer)) ((Plaguebearer)pb).RpcSpreadInfection(player, role.Player);
                }
                role.InvestigatingScene = role.CurrentTarget;
                role.InvestigatedPlayers.AddRange(role.CurrentTarget.ScenePlayers);
                return false;
            }
        }
    }
}
