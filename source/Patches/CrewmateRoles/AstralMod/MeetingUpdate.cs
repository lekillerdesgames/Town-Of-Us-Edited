using HarmonyLib;
using TownOfUsEdited.Roles;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace TownOfUsEdited.ImpostorRoles.AstralMod
{
    public class MeetingUpdate
    {
        private static bool UsedGhost = false;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
        class Revive
        {
            public static void Prefix()
            {
                var astrals = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(RoleEnum.Astral)).ToList();
                foreach (var astral in astrals)
                {
                    var role = Role.GetRole<Astral>(astral);
                    if (role.Enabled)
                    {
                        role.TimeRemaining = 0f;
                        role.Revive(role.Player);
                        role.ResetCD();
                        UsedGhost = true;
                    }
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CoIntro))]
        class UpdateDeads
        {
            public static void Prefix(ref Il2CppReferenceArray<NetworkedPlayerInfo> deadBodies)
            {
                var astrals = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(RoleEnum.Astral)).ToList();
                foreach (var astral in astrals)
                {
                    var role = Role.GetRole<Astral>(astral);
                    if (UsedGhost == true)
                    {
                        var playerId = role.Player.PlayerId;
                        deadBodies = new(deadBodies.Where(x => x.PlayerId != playerId).ToArray());
                        UsedGhost = false;
                        return;
                    }
                }
            }
        }
    }
}
