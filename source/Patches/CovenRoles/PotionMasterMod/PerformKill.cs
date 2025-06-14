using System.Collections;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.Patches.CovenRoles.PotionMasterMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKillPotionMaster
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.PotionMaster))
                return true;

            if (!PlayerControl.LocalPlayer.CanMove)
                return false;

            var role = Role.GetRole<PotionMaster>(PlayerControl.LocalPlayer);

            if (PlayerControl.LocalPlayer.Data.IsDead)
                return false;

            if (__instance == role.PotionButton)
            {
                if (role.PotionCooldown > 0)
                    return false;

                if (__instance.graphic.sprite == TownOfUsEdited.Potion)
                {
                    role.GetPotion();
                    role.PotionCooldown = 5f;
                    __instance.graphic.sprite = TownOfUsEdited.Drink;
                }
                else
                {
                    role.UsePotion();
                    if (role.Potion != "Invisibility")
                    {
                        role.TimeRemaining = CustomGameOptions.PotionDuration;
                        role.Enabled = true;
                        Utils.Rpc(CustomRPC.UsePotion, PlayerControl.LocalPlayer.PlayerId, role.Potion);
                    }
                    else
                    {
                        Coroutines.Start(Swoop(role));
                        Utils.Rpc(CustomRPC.UsePotion, PlayerControl.LocalPlayer.PlayerId, role.Potion);
                    }
                }
            }

            return false;
        }

        public static IEnumerator Swoop(PotionMaster role)
        {
            Utils.Swoop(role.Player, true);
            role.PotionCooldown = 2f;
            yield return new WaitForSeconds(2);
            role.Swoop();
            role.TimeRemaining = CustomGameOptions.ChamSwoopDuration;
            role.Enabled = true;
        }
    }
}