using System.Linq;
using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles.Modifiers;

namespace TownOfUsEdited.NeutralRoles.PlaguebearerMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Plaguebearer)) return;
            var isDead = PlayerControl.LocalPlayer.Data.IsDead;
            var infectButton = __instance.KillButton;
            var role = Role.GetRole<Plaguebearer>(PlayerControl.LocalPlayer);

            if (!PlayerControl.LocalPlayer.IsHypnotised() && !Utils.CommsCamouflaged())
            {
                foreach (var playerId in role.InfectedPlayers)
                {
                    var player = Utils.PlayerById(playerId);
                    var data = player?.Data;
                    if (data == null || data.Disconnected || data.IsDead || PlayerControl.LocalPlayer.Data.IsDead || playerId == PlayerControl.LocalPlayer.PlayerId)
                        continue;

                    player.myRend().material.SetColor("_VisorColor", role.Color);

                    var colour = Color.black;
                    if (player.Is(ModifierEnum.Shy)) colour.a = Modifier.GetModifier<Shy>(player).Opacity;
                    player.nameText().color = colour;
                }
            }

            infectButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ||
                    AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay));
            infectButton.SetCoolDown(role.InfectTimer(), CustomGameOptions.InfectCd);

            var notInfected = PlayerControl.AllPlayerControls.ToArray().Where(
                player => !role.InfectedPlayers.Contains(player.PlayerId)
            ).ToList();

            Utils.SetTarget(ref role.ClosestPlayer, __instance.KillButton, float.NaN, notInfected);

            if (role.CanTransform && (PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected).ToList().Count > 1) && !isDead)
            {
                var transform = false;
                var alives = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected && x != PlayerControl.LocalPlayer).ToList();
                if (alives.Count <= 1)
                {
                    foreach (var player in alives)
                    {
                        if (player.Data.IsImpostor() || player.Is(Faction.NeutralKilling))
                        {
                            transform = true;
                        }
                    }
                }
                else transform = true;
                if (transform)
                {
                    role.TurnPestilence();
                    Utils.Rpc(CustomRPC.TurnPestilence, PlayerControl.LocalPlayer.PlayerId);
                }
            }
        }
    }
}