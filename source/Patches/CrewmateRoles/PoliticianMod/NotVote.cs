using HarmonyLib;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.CrewmateRoles.PoliticianMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))] // BBFDNCCEJHI
    public static class VotingComplete
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Politician))
            {
                var politician = Role.GetRole<Politician>(PlayerControl.LocalPlayer);
                politician.RevealButton.Destroy();
            }
        }
    }
}