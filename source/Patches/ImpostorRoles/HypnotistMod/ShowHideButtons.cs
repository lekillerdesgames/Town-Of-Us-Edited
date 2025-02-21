using HarmonyLib;
using TownOfUsEdited.Roles;
using Reactor.Utilities.Extensions;

namespace TownOfUsEdited.ImpostorRoles.HypnotistMod
{
    public class ShowHideButtonsHypnotist
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
        public static class Confirm
        {
            public static bool Prefix(MeetingHud __instance)
            {
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Hypnotist)) return true;
                var hypnotist = Role.GetRole<Hypnotist>(PlayerControl.LocalPlayer);
                hypnotist.HysteriaButton.Destroy();
                return true;
            }
        }
    }
}