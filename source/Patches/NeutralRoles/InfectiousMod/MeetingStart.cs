using System.Linq;
using AmongUs.Data.Player;
using HarmonyLib;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.NeutralRoles.InfectiousMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class MeetingStart
    {
        public static void Postfix()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                foreach (var infectious in Role.GetRoles(RoleEnum.Infectious))
                {
                    var infectiousRole = (Infectious)infectious;
                    var playerRole = Role.GetRole(player);
                    if (!player.Data.IsDead && playerRole != null && playerRole.InfectionState != 0 
                    && playerRole.InfectionState > 0 && playerRole.InfectionState != 4
                    && infectiousRole.Infected.Contains(player.PlayerId) && !infectiousRole.Player.Data.IsDead
                    && !player.Is(RoleEnum.Infectious))
                    {
                        playerRole.InfectionState += 1;
                    }
                }
            }
            foreach (var infectious in Role.GetRoles(RoleEnum.Infectious))
            {
                var infectiousRole = (Infectious)infectious;
                if (infectiousRole.Infected != null)
                {
                    foreach (var playerid in infectiousRole.Infected)
                    {
                        var player = Utils.PlayerById(playerid);
                        var playerRole = Role.GetRole(player);
                        if (playerRole.InfectionState == 0 && !player.Is(RoleEnum.Infectious)) playerRole.InfectionState = 1;
                    }
                }
            }
            if (PlayerControl.LocalPlayer.Data.IsDead) return;
            var role = Role.GetRole(PlayerControl.LocalPlayer);
            if (role == null) return;
            if (role.InfectionState == 0) return;
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Infectious)) return;
            if (Role.GetRoles(RoleEnum.Infectious).ToArray().ToList().Count <= 0) return;
            if (role.InfectionState == 1)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "You have been Infected by an <color=#bf9000>Infectious</color> and entered the stage 1 of the infection.\nEject the Infectious before it's too late!");
            }
            else if (role.InfectionState == 2)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "You have entered the stage 2 of your infection. You will start to become slower.");
            }
            else if (role.InfectionState == 3)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "You have entered the stage 3 of your infection. You vision will now decrease.");
            }
            else if (role.InfectionState == 4)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "You have entered the stage 4 of your infection. If you don't eject the Infectious this meeting, you will die!");
            }
        }
    }
}
