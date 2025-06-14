using System;
using System.Linq;
using TMPro;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles.Modifiers;
using UnityEngine;

namespace TownOfUsEdited.Roles
{
    public class Morphling : Role, IVisualAlteration

    {
        public KillButton _morphButton;
        public PlayerControl ClosestPlayer;
        public TextMeshPro MorphText;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public PlayerControl MorphedPlayer;

        public PlayerControl SampledPlayer;
        public float TimeRemaining;
        public bool Enabled;

        public Morphling(PlayerControl player) : base(player)
        {
            Name = "Morphling";
            ImpostorText = () => "Transform Into Crewmates";
            TaskText = () => "Morph into <color=#00FFFF>Crewmates</color> to disguise yourself\nFake Tasks:";
            Color = Patches.Colors.Impostor;
            Cooldown = CustomGameOptions.MorphlingCd;
            RoleType = RoleEnum.Morphling;
            AddToRoleHistory(RoleType);
            Faction = Faction.Impostors;
            Alignment = Alignment.ImpostorConcealing;
        }

        public KillButton MorphButton
        {
            get => _morphButton;
            set
            {
                _morphButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public bool Morphed => TimeRemaining > 0f;

        public void Morph()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
            if (Player.Data.IsDead)
            {
                TimeRemaining = 0f;
            }
        }

        public void Unmorph()
        {
            Enabled = false;
            MorphedPlayer = null;
            Utils.Unmorph(Player);
            Cooldown = CustomGameOptions.MorphlingCd;
        }

        public float MorphTimer()
        {
            if (!coolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                Cooldown -= Time.deltaTime;
                return Cooldown;
            }
            else return Cooldown;
        }

        public bool TryGetModifiedAppearance(out VisualAppearance appearance)
        {
            if (Morphed)
            {
                appearance = MorphedPlayer.GetDefaultAppearance();
                var modifiers = Modifier.GetModifiers(MorphedPlayer);
                var modifier = modifiers.FirstOrDefault(x => x is IVisualAlteration);
                if (modifier is IVisualAlteration alteration)
                    alteration.TryGetModifiedAppearance(out appearance);
                return true;
            }

            appearance = Player.GetDefaultAppearance();
            return false;
        }
    }
}
