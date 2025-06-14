using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUsEdited.Roles
{
    public class Vulture : Role
    {
        public Vulture(PlayerControl player) : base(player)
        {
            Name = "Vulture";
            ImpostorText = () => "Eat All Bodies";
            TaskText = () => $"Eat {CustomGameOptions.VultureBodies} Dead Bodies to win!\nFake Tasks:";
            Color = Patches.Colors.Vulture;
            Cooldown = CustomGameOptions.VultureCD;
            RoleType = RoleEnum.Vulture;
            AddToRoleHistory(RoleType);
            Faction = Faction.NeutralEvil;
        }

        public Dictionary<byte, ArrowBehaviour> BodyArrows = new Dictionary<byte, ArrowBehaviour>();
        public TextMeshPro BodiesText;
        public DeadBody CurrentTarget;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public int BodiesEaten = 0;
        public bool VultureWins = false;

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__38 __instance)
        {
            var vultureTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            vultureTeam.Add(PlayerControl.LocalPlayer);
            __instance.teamToShow = vultureTeam;
        }

        public float EatTimer()
        {
            if (!coolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                Cooldown -= Time.deltaTime;
                return Cooldown;
            }
            else return Cooldown;
        }

        public void Wins()
        {
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
            {
                VultureWins = true;
                if (AmongUsClient.Instance.AmHost && CustomGameOptions.NeutralEvilWinEndsGame)
                {
                    Coroutines.Start(WaitForEnd());
                    PluginSingleton<TownOfUsEdited>.Instance.Log.LogMessage("GAME OVER REASON: Vulture Win");
                }
            }
            else
            {
                HudManager.Instance.ShowPopUp("Normally, the game would've ended and the Vulture would've won. In Freeplay, we just reset the bodies eaten count.");
                BodiesEaten = 0;
            }
        }

        public void DestroyArrow(byte targetPlayerId)
        {
            var arrow = BodyArrows.FirstOrDefault(x => x.Key == targetPlayerId);
            if (arrow.Value != null)
                Object.Destroy(arrow.Value);
            if (arrow.Value.gameObject != null)
                Object.Destroy(arrow.Value.gameObject);
            BodyArrows.Remove(arrow.Key);
        }
    }
}