using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Utilities.Extensions;
using Reactor.Networking.Attributes;
using TownOfUsEdited.CustomOption;
using TownOfUsEdited.Patches;
using TownOfUsEdited.RainbowMod;
using TownOfUsEdited.Extensions;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using System.IO;
using TownOfUsEdited.CrewmateRoles.DetectiveMod;
using TownOfUsEdited.NeutralRoles.SoulCollectorMod;

namespace TownOfUsEdited
{
    [BepInPlugin(Id, "Town Of Us Edited", VersionString)]
    [BepInDependency(ReactorPlugin.Id)]
    [BepInDependency(SubmergedCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [ReactorModFlags(Reactor.Networking.ModFlags.RequireOnAllClients)]
    [BepInIncompatibility("MalumMenu")]

    public class TownOfUsEdited : BasePlugin
    {
        public const string Id = "com.lekillerdesgames.townofusedited";
        public const string VersionString = "1.1.4";
        public static System.Version Version = System.Version.Parse(VersionString);
        public const string VersionTag = "<color=#00F0FF></color>";

        public static AssetLoader bundledAssets;

        public static Sprite JanitorClean;
        public static Sprite SwitchRole;
        public static Sprite ReleaseSprite;
        public static Sprite PlaceHolder;
        public static Sprite SabotageCoven;
        public static Sprite Potion;
        public static Sprite Drink;
        public static Sprite Recruit;
        public static Sprite SpiritualistControl;
        public static Sprite Hex;
        public static Sprite HexBomb;
        public static Sprite Freeze;
        public static Sprite Blind;
        public static Sprite Rewind;
        public static Sprite Dissociate;
        public static Sprite Eat;
        public static Sprite Avenge;
        public static Sprite Spell;
        public static Sprite Bounty;
        public static Sprite Curse;
        public static Sprite Protect2Sprite;
        public static Sprite WerewolfConvertSprite;
        public static Sprite StoreSprite;
        public static Sprite ManipulateSprite;
        public static Sprite JailSprite;
        public static Sprite PoisonSprite;
        public static Sprite ShiftButton;
        public static Sprite DocReviveButton;
        public static Sprite CapZoomButton;
        public static Sprite EngineerFix;
        public static Sprite SwapperSwitch;
        public static Sprite SwapperSwitchDisabled;
        public static Sprite Footprint;
        public static Sprite NormalKill;
        public static Sprite MedicSprite;
        public static Sprite SeerSprite;
        public static Sprite SampleSprite;
        public static Sprite Arrow;
        public static Sprite MineSprite;
        public static Sprite DouseSprite;
        public static Sprite IgniteSprite;
        public static Sprite ReviveSprite;
        public static Sprite ButtonSprite;
        public static Sprite DisperseSprite;
        public static Sprite CycleBackSprite;
        public static Sprite CycleForwardSprite;
        public static Sprite GuessSprite;
        public static Sprite DragSprite;
        public static Sprite DropSprite;
        public static Sprite TransformSprite;
        public static Sprite UnTransformSprite;
        public static Sprite FlashSprite;
        public static Sprite AlertSprite;
        public static Sprite RememberSprite;
        public static Sprite PlantSprite;
        public static Sprite DetonateSprite;
        public static Sprite TransportSprite;
        public static Sprite MediateSprite;
        public static Sprite VestSprite;
        public static Sprite ProtectSprite;
        public static Sprite BlackmailSprite;
        public static Sprite BlackmailLetterSprite;
        public static Sprite BlackmailOverlaySprite;
        public static Sprite LighterSprite;
        public static Sprite DarkerSprite;
        public static Sprite InfectSprite;
        public static Sprite RampageSprite;
        public static Sprite UnRampageSprite;
        public static Sprite TrapSprite;
        public static Sprite TrackSprite;
        public static Sprite InspectSprite;
        public static Sprite ExamineSprite;
        public static Sprite EscapeSprite;
        public static Sprite MarkSprite;
        public static Sprite Revive2Sprite;
        public static Sprite ChameleonSwoop;
        public static Sprite WhisperSprite;
        public static Sprite ImitateSelectSprite;
        public static Sprite ImitateDeselectSprite;
        public static Sprite ObserveSprite;
        public static Sprite BiteSprite;
        public static Sprite StakeSprite;
        public static Sprite RevealSprite;
        public static Sprite RevealRoleSprite;
        public static Sprite ConfessSprite;
        public static Sprite NoAbilitySprite;
        public static Sprite CamouflageSprite;
        public static Sprite CamoSprintSprite;
        public static Sprite CamoSprintFreezeSprite;
        public static Sprite RadiateSprite;
        public static Sprite HackSprite;
        public static Sprite MimicSprite;
        public static Sprite LockSprite;
        public static Sprite SKConvertSprite;
        public static Sprite StalkSprite;
        public static Sprite CrimeSceneSprite;
        public static Sprite CampaignSprite;
        public static Sprite FortifySprite;
        public static Sprite HypnotiseSprite;
        public static Sprite HysteriaSprite;
        public static Sprite InJailSprite;
        public static Sprite ExecuteSprite;
        public static Sprite ReapSprite;
        public static Sprite SoulSprite;
        public static Sprite Voodoo;
        public static Sprite CampSprite;
        public static Sprite ShootSprite;
        public static Sprite WatchSprite;
        public static Sprite BlessSprite;
        public static Sprite FlushSprite;
        public static Sprite BlockSprite;
        public static Sprite BarricadeSprite;
        public static Sprite BlindSprite;
        public static Sprite GuardSprite;
        public static Sprite BribeSprite;
        public static Sprite BarrierSprite;
        public static Sprite CleanseSprite;
        public static Sprite DetectSprite;

        public static Sprite ToUBanner;
        public static Sprite UpdateTOUButton;
        public static Sprite UpdateSubmergedButton;

        public static Sprite ZoomPlusButton;
        public static Sprite ZoomMinusButton;
        public static Sprite ZoomPlusActiveButton;
        public static Sprite ZoomMinusActiveButton;
        public static Sprite NextButton;
        public static Sprite AnimationButton;
        public static Sprite AnimationButtonActive;
        public static Sprite heh;

        public static Vector3 ButtonPosition { get; private set; } = new Vector3(2.6f, 0.7f, -9f);

        private static DLoadImage _iCallLoadImage;


        private Harmony _harmony;
        public static ConfigEntry<bool> DeadSeeGhosts { get; set; }
        public static ConfigEntry<bool> DisableLobbyMusic { get; set; }
        public static ConfigEntry<bool> HideDevStatus { get; set; }
        public static string RuntimeLocation;
        public override void Load()
        {
            RuntimeLocation = Path.GetDirectoryName(Assembly.GetAssembly(typeof(TownOfUsEdited)).Location);

            System.Console.WriteLine("000.000.000.000/000000000000000000");

            _harmony = new Harmony("com.lekillerdesgames.townofusedited");

            Generate.GenerateAll();

            bundledAssets = new();

            PlaceHolder = CreateSprite("TownOfUsEdited.Resources.Placeholder.png");
            SwitchRole = CreateSprite("TownOfUsEdited.Resources.SwitchRole.png");
            heh = CreateSprite("TownOfUsEdited.Resources.heh.png");
            ReleaseSprite = CreateSprite("TownOfUsEdited.Resources.Release.png");
            WatchSprite = CreateSprite("TownOfUsEdited.Resources.Watch.png");
            BlessSprite = CreateSprite("TownOfUsEdited.Resources.Bless.png");
            FlushSprite = CreateSprite("TownOfUsEdited.Resources.Flush.png");
            BlockSprite = CreateSprite("TownOfUsEdited.Resources.Block.png");
            BarricadeSprite = CreateSprite("TownOfUsEdited.Resources.Barricade.png");
            BlindSprite = CreateSprite("TownOfUsEdited.Resources.Blind.png");
            GuardSprite = CreateSprite("TownOfUsEdited.Resources.Guard.png");
            BribeSprite = CreateSprite("TownOfUsEdited.Resources.Bribe.png");
            BarrierSprite = CreateSprite("TownOfUsEdited.Resources.Barrier.png");
            CleanseSprite = CreateSprite("TownOfUsEdited.Resources.Cleanse.png");
            DetectSprite = CreateSprite("TownOfUsEdited.Resources.Detect.png");
            ShootSprite = CreateSprite("TownOfUsEdited.Resources.Shoot.png");
            CampSprite = CreateSprite("TownOfUsEdited.Resources.Camp.png");
            ChameleonSwoop = CreateSprite("TownOfUsEdited.Resources.ChameleonSwoop.png");
            JanitorClean = CreateSprite("TownOfUsEdited.Resources.Janitor.png");
            Voodoo = CreateSprite("TownOfUsEdited.Resources.Voodoo.png");
            Potion = CreateSprite("TownOfUsEdited.Resources.Potion.png");
            Drink = CreateSprite("TownOfUsEdited.Resources.Drink.png");
            SabotageCoven = CreateSprite("TownOfUsEdited.Resources.SabotageCoven.png");
            Recruit = CreateSprite("TownOfUsEdited.Resources.Recruit.png");
            SpiritualistControl = CreateSprite("TownOfUsEdited.Resources.Control.png");
            Hex = CreateSprite("TownOfUsEdited.Resources.Hex.png");
            HexBomb = CreateSprite("TownOfUsEdited.Resources.HexBomb.png");
            Freeze = CreateSprite("TownOfUsEdited.Resources.Freeze.png");
            Blind = CreateSprite("TownOfUsEdited.Resources.Blind.png");
            Rewind = CreateSprite("TownOfUsEdited.Resources.Rewind.png");
            Eat = CreateSprite("TownOfUsEdited.Resources.Eat.png");
            Dissociate = CreateSprite("TownOfUsEdited.Resources.Dissociate.png");
            Avenge = CreateSprite("TownOfUsEdited.Resources.Avenge.png");
            TrackSprite = CreateSprite("TownOfUsEdited.Resources.Track.png");
            Spell = CreateSprite("TownOfUsEdited.Resources.Spell.png");
            Curse = CreateSprite("TownOfUsEdited.Resources.Curse.png");
            Bounty = CreateSprite("TownOfUsEdited.Resources.Bounty.png");
            Protect2Sprite = CreateSprite("TownOfUsEdited.Resources.Protect2.png");
            WerewolfConvertSprite = CreateSprite("TownOfUsEdited.Resources.WerewolfConvert.png");
            StoreSprite = CreateSprite("TownOfUsEdited.Resources.Store.png");
            ManipulateSprite = CreateSprite("TownOfUsEdited.Resources.ManipulateButton.png");
            JailSprite = CreateSprite("TownOfUsEdited.Resources.JailButton.png");
            PoisonSprite = CreateSprite("TownOfUsEdited.Resources.Poison.png");
            ShiftButton = CreateSprite("TownOfUsEdited.Resources.Shift.png");
            DocReviveButton = CreateSprite("TownOfUsEdited.Resources.DocReviveButton.png");
            CapZoomButton = CreateSprite("TownOfUsEdited.Resources.CapZoom.png");
            EngineerFix = CreateSprite("TownOfUsEdited.Resources.Engineer.png");
            SwapperSwitch = CreateSprite("TownOfUsEdited.Resources.SwapperSwitch.png");
            SwapperSwitchDisabled = CreateSprite("TownOfUsEdited.Resources.SwapperSwitchDisabled.png");
            Footprint = CreateSprite("TownOfUsEdited.Resources.Footprint.png");
            NormalKill = CreateSprite("TownOfUsEdited.Resources.NormalKill.png");
            MedicSprite = CreateSprite("TownOfUsEdited.Resources.Medic.png");
            SeerSprite = CreateSprite("TownOfUsEdited.Resources.Seer.png");
            SampleSprite = CreateSprite("TownOfUsEdited.Resources.Sample.png");
            Arrow = CreateSprite("TownOfUsEdited.Resources.Arrow.png");
            MineSprite = CreateSprite("TownOfUsEdited.Resources.Mine.png");
            DouseSprite = CreateSprite("TownOfUsEdited.Resources.Douse.png");
            IgniteSprite = CreateSprite("TownOfUsEdited.Resources.Ignite.png");
            ReviveSprite = CreateSprite("TownOfUsEdited.Resources.Revive.png");
            ButtonSprite = CreateSprite("TownOfUsEdited.Resources.Button.png");
            DisperseSprite = CreateSprite("TownOfUsEdited.Resources.Disperse.png");
            DragSprite = CreateSprite("TownOfUsEdited.Resources.Drag.png");
            DropSprite = CreateSprite("TownOfUsEdited.Resources.Drop.png");
            TransformSprite = CreateSprite("TownOfUsEdited.Resources.Transform.png");
            UnTransformSprite = CreateSprite("TownOfUsEdited.Resources.UnTransform.png");
            CycleBackSprite = CreateSprite("TownOfUsEdited.Resources.CycleBack.png");
            CycleForwardSprite = CreateSprite("TownOfUsEdited.Resources.CycleForward.png");
            GuessSprite = CreateSprite("TownOfUsEdited.Resources.Guess.png");
            FlashSprite = CreateSprite("TownOfUsEdited.Resources.Flash.png");
            AlertSprite = CreateSprite("TownOfUsEdited.Resources.Alert.png");
            RememberSprite = CreateSprite("TownOfUsEdited.Resources.Remember.png");
            PlantSprite = CreateSprite("TownOfUsEdited.Resources.Plant.png");
            DetonateSprite = CreateSprite("TownOfUsEdited.Resources.Detonate.png");
            TransportSprite = CreateSprite("TownOfUsEdited.Resources.Transport.png");
            MediateSprite = CreateSprite("TownOfUsEdited.Resources.Mediate.png");
            VestSprite = CreateSprite("TownOfUsEdited.Resources.Vest.png");
            ProtectSprite = CreateSprite("TownOfUsEdited.Resources.Protect.png");
            BlackmailSprite = CreateSprite("TownOfUsEdited.Resources.Blackmail.png");
            BlackmailLetterSprite = CreateSprite("TownOfUsEdited.Resources.BlackmailLetter.png");
            BlackmailOverlaySprite = CreateSprite("TownOfUsEdited.Resources.BlackmailOverlay.png");
            LighterSprite = CreateSprite("TownOfUsEdited.Resources.Lighter.png");
            DarkerSprite = CreateSprite("TownOfUsEdited.Resources.Darker.png");
            InfectSprite = CreateSprite("TownOfUsEdited.Resources.Infect.png");
            RampageSprite = CreateSprite("TownOfUsEdited.Resources.Rampage.png");
            TrapSprite = CreateSprite("TownOfUsEdited.Resources.Trap.png");
            InspectSprite = CreateSprite("TownOfUsEdited.Resources.Inspect.png");
            ExamineSprite = CreateSprite("TownOfUsEdited.Resources.Examine.png");
            EscapeSprite = CreateSprite("TownOfUsEdited.Resources.Recall.png");
            MarkSprite = CreateSprite("TownOfUsEdited.Resources.Mark.png");
            Revive2Sprite = CreateSprite("TownOfUsEdited.Resources.Revive2.png");
            WhisperSprite = CreateSprite("TownOfUsEdited.Resources.Whisper.png");
            ImitateSelectSprite = CreateSprite("TownOfUsEdited.Resources.ImitateSelect.png");
            ImitateDeselectSprite = CreateSprite("TownOfUsEdited.Resources.ImitateDeselect.png");
            ObserveSprite = CreateSprite("TownOfUsEdited.Resources.Observe.png");
            BiteSprite = CreateSprite("TownOfUsEdited.Resources.Bite.png");
            StakeSprite = CreateSprite("TownOfUsEdited.Resources.Stake.png");
            RevealSprite = CreateSprite("TownOfUsEdited.Resources.Reveal.png");
            RevealRoleSprite = CreateSprite("TownOfUsEdited.Resources.RevealRoles.png");
            ConfessSprite = CreateSprite("TownOfUsEdited.Resources.Confess.png");
            NoAbilitySprite = CreateSprite("TownOfUsEdited.Resources.NoAbility.png");
            CamouflageSprite = CreateSprite("TownOfUsEdited.Resources.Camouflage.png");
            CamoSprintSprite = CreateSprite("TownOfUsEdited.Resources.CamoSprint.png");
            CamoSprintFreezeSprite = CreateSprite("TownOfUsEdited.Resources.CamoSprintFreeze.png");
            RadiateSprite = CreateSprite("TownOfUsEdited.Resources.Radiate.png");
            HackSprite = CreateSprite("TownOfUsEdited.Resources.Hack.png");
            MimicSprite = CreateSprite("TownOfUsEdited.Resources.Mimic.png");
            LockSprite = CreateSprite("TownOfUsEdited.Resources.Lock.png");
            SKConvertSprite = CreateSprite("TownOfUsEdited.Resources.SKConvert.png");
            StalkSprite = CreateSprite("TownOfUsEdited.Resources.Stalk.png");
            CrimeSceneSprite = CreateSprite("TownOfUsEdited.Resources.CrimeScene.png");
            CampaignSprite = CreateSprite("TownOfUsEdited.Resources.Campaign.png");
            FortifySprite = CreateSprite("TownOfUsEdited.Resources.Fortify.png");
            HypnotiseSprite = CreateSprite("TownOfUsEdited.Resources.Hypnotise.png");
            HysteriaSprite = CreateSprite("TownOfUsEdited.Resources.Hysteria.png");
            InJailSprite = CreateSprite("TownOfUsEdited.Resources.InJail.png");
            ExecuteSprite = CreateSprite("TownOfUsEdited.Resources.Execute.png");
            ReapSprite = CreateSprite("TownOfUsEdited.Resources.Reap.png");
            SoulSprite = CreateSprite("TownOfUsEdited.Resources.Soul.png");

            ToUBanner = CreateSprite("TownOfUsEdited.Resources.TownOfUsEditedBanner.png");
            UpdateTOUButton = CreateSprite("TownOfUsEdited.Resources.UpdateToUButton.png");
            UpdateSubmergedButton = CreateSprite("TownOfUsEdited.Resources.UpdateSubmergedButton.png");

            ZoomPlusButton = CreateSprite("TownOfUsEdited.Resources.Plus.png");
            ZoomMinusButton = CreateSprite("TownOfUsEdited.Resources.Minus.png");
            ZoomPlusActiveButton = CreateSprite("TownOfUsEdited.Resources.PlusActive.png");
            ZoomMinusActiveButton = CreateSprite("TownOfUsEdited.Resources.MinusActive.png");
            NextButton = CreateSprite("TownOfUsEdited.Resources.NextButton.png");
            AnimationButton = CreateSprite("TownOfUsEdited.Resources.AnimationButton.png");
            AnimationButtonActive = CreateSprite("TownOfUsEdited.Resources.AnimationButtonActive.png");

            PalettePatch.Load();
            ClassInjector.RegisterTypeInIl2Cpp<RainbowBehaviour>();
            ClassInjector.RegisterTypeInIl2Cpp<CrimeScene>();
            ClassInjector.RegisterTypeInIl2Cpp<Soul>();

            // RegisterInIl2CppAttribute.Register();

            DeadSeeGhosts = Config.Bind("Settings", "Dead See Other Ghosts", true, "Whether you see other dead players' ghosts while your dead");

            DisableLobbyMusic = Config.Bind("Settings", "Disable Lobby Music", false, "Whether you want to disable the lobby Music in-game (can be changed in-game with settings)");

            HideDevStatus = Config.Bind("Settings", "Hide Special Status", false, "Toggle this to hide your special status when launching if you have one. You will still have access to your special perks.");

            _harmony.PatchAll();
            SubmergedCompatibility.Initialize();
            IL2CPPChainloader.Instance.Finished += LevelImpostorCompatibility.Initialize; // LI has a circular dependency on TOU, so we need to wait for LI to finish loading before we can initialize it

            ServerManager.DefaultRegions = new Il2CppReferenceArray<IRegionInfo>(new IRegionInfo[0]);
        }

        public static Sprite CreateSprite(string name)
        {
            var pixelsPerUnit = 100f;
            var pivot = new Vector2(0.5f, 0.5f);

            var assembly = Assembly.GetExecutingAssembly();
            var tex = AmongUsExtensions.CreateEmptyTexture();
            var imageStream = assembly.GetManifestResourceStream(name);
            var img = imageStream.ReadFully();
            LoadImage(tex, img, true);
            tex.DontDestroy();
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), pivot, pixelsPerUnit);
            sprite.DontDestroy();
            return sprite;
        }

        public static void LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            _iCallLoadImage ??= IL2CPP.ResolveICall<DLoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2CPPArray = (Il2CppStructArray<byte>) data;
            _iCallLoadImage.Invoke(tex.Pointer, il2CPPArray.Pointer, markNonReadable);
        }

        private delegate bool DLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
    }
}
