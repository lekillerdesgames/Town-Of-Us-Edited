using System.Collections;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.ImpostorRoles.MorphlingMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKillMorphling
    {
        public static Sprite SampleSprite => TownOfUsEdited.SampleSprite;

        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Morphling);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Morphling>(PlayerControl.LocalPlayer);
            var target = role.ClosestPlayer;
            var shapeshifter = RoleManager.Instance.GetRole(AmongUs.GameOptions.RoleTypes.Shapeshifter).Cast<ShapeshifterRole>();
            Sprite MorphSprite = shapeshifter.Ability.Image;
            if (__instance == role.MorphButton)
            {
                if (!__instance.isActiveAndEnabled) return false;
                if (role.MorphButton.graphic.sprite == SampleSprite)
                {
                    if (target == null) return false;
                    var interact = Utils.Interact(PlayerControl.LocalPlayer, target);
                    if (interact[4] == true)
                    {
                        role.SampledPlayer = target;
                        role.MorphButton.graphic.sprite = MorphSprite;
                        role.Cooldown = role.KillCooldown;
                    }
                    if (interact[0] == true)
                    {
                        role.Cooldown = 1f;
                        return false;
                    }
                    else if (interact[1] == true)
                    {
                        role.Cooldown = CustomGameOptions.TempSaveCdReset;
                        return false;
                    }
                    else if (interact[3] == true) return false;
                }
                else
                {
                    if (__instance.isCoolingDown) return false;
                    if (role.Cooldown > 0) return false;
                    var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                    if (!abilityUsed) return false;
                    Utils.Rpc(CustomRPC.Morph, PlayerControl.LocalPlayer.PlayerId, role.SampledPlayer.PlayerId);
                    role.MorphedPlayer = role.SampledPlayer;
                    Coroutines.Start(Morph(role));
                }

                return false;
            }

            return true;
        }

        public static IEnumerator Morph(Morphling role)
        {
            Utils.Morph(role.Player, role.MorphedPlayer, true, true);
            role.Cooldown = 2f;
            yield return new WaitForSeconds(2f);
            role.TimeRemaining = CustomGameOptions.MorphlingDuration;
        }
    }
}
