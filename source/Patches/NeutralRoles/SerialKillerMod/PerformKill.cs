﻿using HarmonyLib;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.Patches.NeutralRoles.SerialKillerMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class SerialKillerKillPatch
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.SerialKiller))
                return true;

            if (PlayerControl.LocalPlayer.Data.IsDead)
                return false;

            if (!PlayerControl.LocalPlayer.CanMove)
                return false;

            var sk = Role.GetRole<SerialKiller>(PlayerControl.LocalPlayer);
            var killbutton = DestroyableSingleton<HudManager>.Instance.KillButton;
            
            if (sk.ClosestPlayer == null)
                return false;

            if (PlayerControl.LocalPlayer.IsJailed()) return false;
                
            if (__instance == sk.skconvertButton) 
            {
                if (sk.ConvertCooldown > 0)
                    return false;

                var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                if (!abilityUsed) return false;

                var interact = Utils.Interact(PlayerControl.LocalPlayer, sk.ClosestPlayer);
                if (interact[4] == true)
                {
                    sk.SKConvertAbility(sk.ClosestPlayer);
                    return false;
                }
                else
                {
                    return false;
                }
            }
            
            if (__instance == killbutton)
            {
                if (sk.Cooldown > 0)
                return false;

                if (PlayerControl.LocalPlayer.IsControlled() && sk.ClosestPlayer.Is(Faction.Coven))
                {
                    Utils.Interact(sk.ClosestPlayer, PlayerControl.LocalPlayer, true);
                    return false;
                }
                else if (sk.ClosestPlayer.Is(RoleEnum.PotionMaster) && Role.GetRole<PotionMaster>(sk.ClosestPlayer).UsingPotion
                && Role.GetRole<PotionMaster>(sk.ClosestPlayer).Potion == "Shield")
                {
                    sk.Cooldown = CustomGameOptions.PotionKCDReset;
                    return false;
                }

                if (sk.ClosestPlayer.IsGuarded2())
                {
                    sk.Cooldown = CustomGameOptions.GuardKCReset;
                    return false; 
                }

                // Kill the closest player
                sk.Kill(sk.ClosestPlayer);
            }

            return false;
        }
    }
}