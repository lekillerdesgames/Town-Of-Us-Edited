using System.Collections.Generic;
using System.Linq;
using TMPro;
using TownOfUsEdited.Extensions;
using UnityEngine;

namespace TownOfUsEdited.Roles
{
    public class PotionMaster : Role
    {
        public PotionMaster(PlayerControl owner) : base(owner)
        {
            Name = "Potion Master";
            ImpostorText = () => "What A Good Drink";
            TaskText = () => "Create Potions to get special abilities\nCurrent Potion: " + PotionType + "\nFake Tasks:";
            Color = Patches.Colors.Coven;
            RoleType = RoleEnum.PotionMaster;
            AddToRoleHistory(RoleType);
            Faction = Faction.Coven;
            PotionCooldown = CustomGameOptions.PotionCD;
        }

        public KillButton _potionButton;
        public TextMeshPro PotionText;
        public float PotionCooldown;
        public bool PotioncoolingDown => PotionCooldown > 0f;
        public float TimeRemaining;
        public bool UsingPotion => TimeRemaining > 0f;
        public string Potion = "null";
        public string PotionType = "None";
        public bool Enabled;

        public void UsePotion()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
            if (Potion == "Invisibility")
            {
                Swoop();
            }
        }
        public void StopPotion()
        {
            Enabled = false;
            PotionCooldown = CustomGameOptions.PotionCD;
            if (Potion == "Invisibility")
            {
                UnSwoop();
            }
            Potion = "null";
        }
        public void GetPotion()
        {
            var random = new System.Random();
            var randomPotion = new List<string>{"Speed", "Strength", "Invisibility", "Shield"};
            int index = random.Next(randomPotion.Count);
            var chosenPotion = randomPotion[index];
            randomPotion.RemoveAt(index);
            Potion = chosenPotion;
        }
        public void Swoop()
        {
            if (Player.Data.IsDead)
            {
                TimeRemaining = 0f;
            }
        }


        public void UnSwoop()
        {
            Utils.Unmorph(Player);
            Player.myRend().color = Color.white;
        }
        public KillButton PotionButton
        {
            get => _potionButton;
            set
            {
                _potionButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }
        public float PotionTimer()
        {
            if (!PotioncoolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                PotionCooldown -= Time.deltaTime;
                return PotionCooldown;
            }
            else return PotionCooldown;
        }

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__38 __instance)
        {
            var covenTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            covenTeam.Add(PlayerControl.LocalPlayer);
            var toAdd = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Coven) && x != PlayerControl.LocalPlayer).ToList();
            foreach (var player in toAdd)
            {
                covenTeam.Add(player);
            }
            __instance.teamToShow = covenTeam;
        }
    }
}