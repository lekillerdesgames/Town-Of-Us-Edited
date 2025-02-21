using HarmonyLib;
using TownOfUsEdited.Roles;
using System.Linq;

namespace TownOfUsEdited.ImpostorRoles.PoisonerMod
{

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
    class StartMeetingPatch
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo meetingTarget)
        {
            if (__instance == null)
            {
                return;
            }
            var poisoners = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(RoleEnum.Poisoner)).ToList();
            foreach (var poisoner in poisoners)
            {
                var role = Role.GetRole<Poisoner>(poisoner);
                if (poisoner != role.PoisonedPlayer && role.PoisonedPlayer != null)
                {
                    if (!role.PoisonedPlayer.Data.IsDead && !role.PoisonedPlayer.Is(RoleEnum.Pestilence))
                        Utils.MurderPlayer(poisoner, role.PoisonedPlayer, false);
                }
                return;
            }
        }
    }
}
