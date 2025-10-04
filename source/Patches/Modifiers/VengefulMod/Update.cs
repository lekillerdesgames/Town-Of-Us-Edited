using HarmonyLib;
using TownOfUsEdited.Roles.Modifiers;
using UnityEngine;
using System.Linq;
using TownOfUsEdited.Roles;
using TownOfUsEdited.Patches;

namespace TownOfUsEdited.Modifiers.VengefulMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class Update
    {

        public static void Postfix(HudManager __instance)
        {
            UpdateVengefulButton(__instance);
        }

        private static void UpdateVengefulButton(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(ModifierEnum.Vengeful)) return;
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Glitch)) return;

            var role = Modifier.GetModifier<Vengeful>(PlayerControl.LocalPlayer);
            var playerRole = Role.GetRole(PlayerControl.LocalPlayer);

            if (role.KillButton == null)
            {
                role.KillButton = Object.Instantiate(__instance.KillButton, __instance.transform.parent);
                role.KillButton.graphic.enabled = true;
                role.KillButton.graphic.sprite = DestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite;
                role.KillText = Object.Instantiate(__instance.KillButton.buttonLabelText, role.KillButton.transform);
                role.KillText.gameObject.SetActive(false);
                playerRole.ButtonLabels.Add(role.KillText);
            }

            role.KillButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            var renderer = role.KillButton.graphic;

            role.KillButton.SetCoolDown(0f, 1f);

            role.KillText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            role.KillText.text = "Kill";
            role.KillText.SetOutlineColor(Colors.Vengeful);

            float addY = 0f;
            bool hasButtonModif = PlayerControl.LocalPlayer.Is(ModifierEnum.ButtonBarry) ||
            PlayerControl.LocalPlayer.Is(ModifierEnum.Satellite);
            if (hasButtonModif) addY = 1f;

            if (__instance.UseButton != null)
            {
                var position1 = __instance.UseButton.transform.position;
                role.KillButton.transform.position = new Vector3(
                    Camera.main.ScreenToWorldPoint(new Vector3(0, 0)).x + 0.75f, position1.y + addY,
                    position1.z);
            }
            else
            {
                var position1 = __instance.PetButton.transform.position;
                role.KillButton.transform.position = new Vector3(
                    Camera.main.ScreenToWorldPoint(new Vector3(0, 0)).x + 0.75f, position1.y + addY,
                    position1.z);
            }

            var taskinfos = PlayerControl.LocalPlayer.Data.Tasks.ToArray().ToList();

            var tasksLeft = taskinfos.Count(x => !x.Complete);

            if (!role.UsedAbility && tasksLeft == 0)
            {
                Utils.SetTarget(ref role.ClosestPlayer, role.KillButton);
                if (role.ClosestPlayer != null)
                {
                    role.KillText.color = Palette.EnabledColor;
                    role.KillText.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    role.KillText.color = Palette.DisabledClear;
                    role.KillText.material.SetFloat("_Desat", 1f);
                }
                return;
            }

            renderer.color = Palette.DisabledClear;
            renderer.material.SetFloat("_Desat", 1f);
            role.KillText.color = Palette.DisabledClear;
            role.KillText.material.SetFloat("_Desat", 1f);
        }
    }
}