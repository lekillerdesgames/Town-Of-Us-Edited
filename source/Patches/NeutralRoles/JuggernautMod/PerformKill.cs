using System;
using HarmonyLib;
using TownOfUsEdited.Roles;
using AmongUs.GameOptions;

namespace TownOfUsEdited.NeutralRoles.JuggernautMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Juggernaut);
            if (!flag) return true;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            var role = Role.GetRole<Juggernaut>(PlayerControl.LocalPlayer);
            if (role.Player.inVent) return false;
            if (role.Cooldown > 0) return false;

            if (role.ClosestPlayer == null) return false;
            var distBetweenPlayers = Utils.GetDistBetweenPlayers(PlayerControl.LocalPlayer, role.ClosestPlayer);
            var flag3 = distBetweenPlayers <
                        GameOptionsData.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            if (!flag3) return false;
            if (PlayerControl.LocalPlayer.IsJailed()) return false;
            if (PlayerControl.LocalPlayer.IsControlled() && role.ClosestPlayer.Is(Faction.Coven))
            {
                Utils.Interact(role.ClosestPlayer, PlayerControl.LocalPlayer, true);
                return false;
            }
            else if (role.ClosestPlayer.Is(RoleEnum.PotionMaster) && Role.GetRole<PotionMaster>(role.ClosestPlayer).UsingPotion
            && Role.GetRole<PotionMaster>(role.ClosestPlayer).Potion == "Shield")
            {
                role.Cooldown = CustomGameOptions.PotionKCDReset;
                return false;
            }
            if (role.ClosestPlayer.IsGuarded2())
            {
                role.Cooldown = CustomGameOptions.GuardKCReset;
                return false; 
            }
            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer, true);
            if (interact[4] == true) return false;
            else if (interact[0] == true)
            {
                role.Cooldown = CustomGameOptions.JuggKCd - CustomGameOptions.ReducedKCdPerKill * role.JuggKills;
                return false;
            }
            else if (interact[1] == true)
            {
                role.Cooldown = CustomGameOptions.ProtectKCReset;
                return false;
            }
            else if (interact[2] == true)
            {
                role.Cooldown = CustomGameOptions.VestKCReset;
                return false;
            }
            else if (interact[3] == true) return false;
            return false;
        }
    }
}