using HarmonyLib;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.CrewmateRoles.OracleMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HighlightConfessor
    {
        public static void UpdateMeeting(Oracle role, MeetingHud __instance)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                int accuracy = (int)role.Accuracy;
                foreach (var state in __instance.playerStates)
                {
                    if (player.PlayerId != state.TargetPlayerId) continue;
                    if (player == role.Confessor)
                    {
                        if (role.RevealedFaction == Faction.Crewmates && !role.Player.Is(Faction.Madmates)) state.NameText.text += $"<color=#00FFFFFF> ({accuracy}% Crew)</color>";
                        else if (role.RevealedFaction == Faction.Impostors && !role.Player.Is(Faction.Madmates)) state.NameText.text += $"<color=#FF0000FF> ({accuracy}% Imp)</color>";
                        else if (role.RevealedFaction == Faction.Coven) state.NameText.text += $"<color=#bf5fff> ({accuracy}% Coven)</color>";
                        else if (role.RevealedFaction == Faction.Impostors && role.Player.Is(Faction.Madmates)) state.NameText.text += $"<color=#00FFFFFF>({accuracy}% Crew) </color>";
                        else if (role.RevealedFaction == Faction.Crewmates && role.Player.Is(Faction.Madmates)) state.NameText.text += $"<color=#FF0000FF>({accuracy}% Imp) </color>";
                        else if (role.RevealedFaction == Faction.Madmates && !role.Player.Is(Faction.Madmates)) state.NameText.text += $"<color=#FF0000FF>({accuracy}% Mad) </color>";
                        else if (role.RevealedFaction == Faction.Madmates && role.Player.Is(Faction.Madmates)) state.NameText.text += $"<color=#00FFFFFF> ({accuracy}% Crew)</color>";
                        else state.NameText.text += $"<color=#808080FF> ({accuracy}% Neut)</color>";
                    }
                }
            }
        }
        public static void Postfix(HudManager __instance)
        {
            if (!MeetingHud.Instance || PlayerControl.LocalPlayer.Data.IsDead) return;
            foreach (var oracle in Role.GetRoles(RoleEnum.Oracle))
            {
                var role = Role.GetRole<Oracle>(oracle.Player);
                if (role == null || role.Player == null || role.Player.Data == null || role.Player.Data.Disconnected || !role.Player.Data.IsDead || role.Confessor == null) return;
                UpdateMeeting(role, MeetingHud.Instance);
            }
        }
    }
}