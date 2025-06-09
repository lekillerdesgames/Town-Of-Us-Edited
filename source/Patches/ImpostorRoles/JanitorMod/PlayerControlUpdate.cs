using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using AmongUs.GameOptions;

namespace TownOfUsEdited.ImpostorRoles.JanitorMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class PlayerControlUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Janitor)) return;

            var role = Role.GetRole<Janitor>(PlayerControl.LocalPlayer);
            if (role.CleanButton == null)
            {
                role.CleanButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.CleanButton.graphic.enabled = true;
                role.CleanButton.gameObject.SetActive(false);
            }

            role.CleanButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ||
                    AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay));
            role.CleanButton.graphic.sprite = TownOfUsEdited.JanitorClean;
            role.CleanButton.transform.localPosition = new Vector3(-2f, 1f, 0f);


            var data = PlayerControl.LocalPlayer.Data;
            var isDead = data.IsDead;
            var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            var maxDistance = LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            var flag = (GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks || !data.IsDead) &&
                       (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver) &&
                       PlayerControl.LocalPlayer.CanMove;
            var allocs = Physics2D.OverlapCircleAll(truePosition, maxDistance,
                LayerMask.GetMask(new[] { "Players", "Ghost" }));
            var killButton = role.CleanButton;
            DeadBody closestBody = null;
            var closestDistance = float.MaxValue;

            foreach (var collider2D in allocs)
            {
                if (!flag || isDead || collider2D.tag != "DeadBody") continue;
                var component = collider2D.GetComponent<DeadBody>();
                if (!(Vector2.Distance(truePosition, component.TruePosition) <=
                      maxDistance)) continue;

                var distance = Vector2.Distance(truePosition, component.TruePosition);
                if (!(distance < closestDistance)) continue;
                closestBody = component;
                closestDistance = distance;
            }

            KillButtonTarget.SetTarget(killButton, closestBody, role);
            role.CleanButton.SetCoolDown(role.KillCooldown, GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
            role.CleanButton.graphic.SetCooldownNormalizedUvs();
        }
    }
}