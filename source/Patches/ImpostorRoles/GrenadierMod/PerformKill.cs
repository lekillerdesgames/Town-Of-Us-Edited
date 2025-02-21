using HarmonyLib;
using Hazel;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.ImpostorRoles.GrenadierMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Grenadier);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Grenadier>(PlayerControl.LocalPlayer);
            if (__instance == role.FlashButton)
            {
                if (__instance.isCoolingDown) return false;
                if (!__instance.isActiveAndEnabled) return false;
                var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
                var sabActive = system.AnyActive;
                if (sabActive) return false;
                if (role.Cooldown > 0) return false;
                var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
                if (!abilityUsed) return false;

                role.TimeRemaining = CustomGameOptions.GrenadeDuration;

                role.StartFlash();

                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                254, SendOption.Reliable, -1);
                writer.Write((int)CustomRPC.FlashGrenade);
                writer.Write((byte)role.Player.PlayerId);
                writer.Write((byte)role.flashedPlayers.Count);
                foreach (var player in role.flashedPlayers)
                {
                    writer.Write(player.PlayerId);
                }
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                return false;
            }

            return true;
        }
    }
}