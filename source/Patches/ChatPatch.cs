using HarmonyLib;
using System.Linq;
using TownOfUsEdited.WerewolfRoles.TalkativeWolfMod.ChatPatch;

namespace TownOfUsEdited.Patches
{
    public class PatchChat
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendFreeChat))]
        public class SendChat
        {
            public static void Postfix(ChatController __instance)
            {
                if (__instance != HudManager.Instance.Chat) return;
                if (PlayerControl.LocalPlayer.Is(RoleEnum.TalkativeWolf) && ChatPatch.WordSaid != true && __instance.freeChatField.Text.ToLower().Contains(ChatPatch.Word) && MeetingHud.Instance
                && !PlayerControl.LocalPlayer.Data.IsDead && !UrlFinder.TryFindUrl(__instance.freeChatField.Text.ToCharArray(), out var num, out var num2))
                {
                    var impos = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.Disconnected && x.Is(Faction.Impostors)).ToArray();
                    foreach (var player in impos)
                    {
                        var playerResults = "The Talkative Wolf has said his word! He can continue his day without problems...";
                        if (!string.IsNullOrWhiteSpace(playerResults) && player == PlayerControl.LocalPlayer)
                        {
                            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, playerResults);
                        }
                    }
                    ChatPatch.WordSaid = true;
                }
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
        public class FixEmojis
        {
            public static void Postfix(ChatBubble __instance, ref string chatText)
            {
                if (asset == null) asset = AssetLoader.LoadSpriteAsset(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "hehEmojiAsset.asset");
                __instance.TextArea.spriteAsset = asset;
                __instance.TextArea.text = EmojiFormat(chatText);
            }
        }

        public static TMPro.TMP_SpriteAsset asset;
        public static string EmojiFormat(string text)
        {
            string finalText = text;
            if (finalText.ToLower().Contains(":heh:")) finalText = text.Replace(":heh:", "<sprite=0>");
            return finalText;
        }
    }
}