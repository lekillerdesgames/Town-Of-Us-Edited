using HarmonyLib;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.ImpostorRoles.HypnotistMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public static class VotingComplete
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Hypnotist))
            {
                var hypnotist = Role.GetRole<Hypnotist>(PlayerControl.LocalPlayer);
                hypnotist.HysteriaButton.Destroy();
            }
        }
    }
}