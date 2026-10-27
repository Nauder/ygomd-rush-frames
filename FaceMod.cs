using MelonLoader;
using UnityEngine;
using MasterFaceEditor;
using HarmonyLib;
using Il2CppYgomGame.Card;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(CardMod), "Card Patch", "1.0.0", "Eglen")]
[assembly: MelonGame("Konami Digital Entertainment Co., Ltd.", "masterduel")]

namespace MasterFaceEditor
{
    public class CardMod : MelonMod
    {
        private static readonly Dictionary<int, LayoutState> layouts = new();
        private const float AttributeIconScale = 1.3f;

        // CardPicture instances are reused. Restore our changes before the next card
        // loads so overframe cards retain the game's original layout.
        private class LayoutState
        {
            private readonly HashSet<int> transforms = new();
            private readonly HashSet<int> texts = new();
            private readonly HashSet<int> objects = new();
            private readonly HashSet<int> outlines = new();
            private readonly List<Action> restore = new();

            public void Move(Transform target, Vector3 position, Vector3? scale = null)
            {
                if (target == null) return;
                if (transforms.Add(target.GetInstanceID()))
                {
                    var originalPosition = target.localPosition;
                    var originalScale = target.localScale;
                    restore.Add(() =>
                    {
                        if (target == null) return;
                        target.localPosition = originalPosition;
                        target.localScale = originalScale;
                    });
                }
                target.localPosition = position;
                if (scale.HasValue) target.localScale = scale.Value;
            }

            public void RememberText(Text text, bool rememberContent = false)
            {
                if (text == null || !texts.Add(text.GetInstanceID())) return;
                var color = text.color;
                var size = text.fontSize;
                var style = text.fontStyle;
                var content = text.text;
                restore.Add(() =>
                {
                    if (text == null) return;
                    text.color = color;
                    text.fontSize = size;
                    text.fontStyle = style;
                    if (rememberContent) text.text = content;
                });
            }

            public void SetActive(GameObject target, bool active)
            {
                if (target == null) return;
                if (objects.Add(target.GetInstanceID()))
                {
                    var originalActive = target.activeSelf;
                    restore.Add(() => { if (target != null) target.SetActive(originalActive); });
                }
                target.SetActive(active);
            }

            public void AddOutline(Text text)
            {
                // Reapplying after asynchronous loading must not accumulate outlines.
                if (!outlines.Add(text.GetInstanceID())) return;
                var outline = text.GetComponent<Outline>();
                var existed = outline != null;
                if (!existed) outline = text.gameObject.AddComponent<Outline>();
                var enabled = existed && outline.enabled;
                var color = outline.effectColor;
                var distance = outline.effectDistance;
                outline.enabled = true;
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1f, -1f);
                restore.Add(() =>
                {
                    if (outline == null) return;
                    outline.enabled = enabled;
                    outline.effectColor = color;
                    outline.effectDistance = distance;
                });
            }

            public void Restore()
            {
                for (var i = restore.Count - 1; i >= 0; i--) restore[i]();
            }
        }

        private static LayoutState GetLayout(CardPicture picture)
        {
            var id = picture.GetInstanceID();
            if (!layouts.TryGetValue(id, out var state))
            {
                state = new LayoutState();
                layouts.Add(id, state);
            }
            return state;
        }

        private static void RestoreLayout(CardPicture picture)
        {
            if (layouts.Remove(picture.GetInstanceID(), out var state)) state.Restore();
        }

        private static void ApplyLayout(CardPicture picture, int cardId)
        {
            if (picture == null || cardId <= 0 || picture.IsOverFrame(cardId)) return;
            var state = GetLayout(picture);
            var root = picture.transform;
            var bottom = root.Find("CardCanvas/Root/BottomArea");

            // SetUpCardContent hides unused artwork with zero scale, not SetActive.
            // Expanding both layers exposes an untextured layer as a white rectangle.
            ApplyArtwork(state, picture.illust_normal);
            ApplyArtwork(state, picture.illust_sp);
            ApplyArtwork(state, picture.illust_pendulum);
            state.Move(root.Find("CardCanvas/Root/CardPictureTop/CardCanvas/Root/Level"), new Vector3(0f, -720f, 0f));
            state.Move(root.Find("CardNameCanvas/CardPictureTop/Root/Name/BaseText"), new Vector3(-15f, 20f, 0f));
            if (bottom == null) return;
            state.Move(bottom.Find("TextArea"), new Vector3(0f, -280f, 0f));
            state.Move(bottom.Find("TextArea/NonEffectArea/AtkDefArea"), new Vector3(-300f, 190f, 0f));
            ApplyAtkDef(state, picture.atkText, new Vector3(245f, 54f, 0f));
            ApplyAtkDef(state, picture.defText, new Vector3(325f, 54f, 0f));
            ClearLabel(state, picture.atkRoot);
            ClearLabel(state, picture.defRoot);
            state.Move(bottom.Find("SpellTrap"), new Vector3(-200f, -230f, 0f), Vector3.zero);
            state.Move(bottom.Find("SpellTrapIcon"), new Vector3(-300f, -250f, 0f));
            state.Move(bottom.Find("AttrIcon"), new Vector3(281f, 441.4f, 0f), new Vector3(AttributeIconScale, AttributeIconScale, 1f));
            state.Move(bottom.Find("PendulumText"), new Vector3(0f, 10f, 0f));
            state.Move(bottom.Find("PendulumScaleL"), new Vector3(-295f, -30f, 0f));
            state.Move(bottom.Find("PendulumScaleR"), new Vector3(295f, -30f, 0f));
            state.Move(bottom.Find("LinkMarker"), new Vector3(0f, 65f, 0f), new Vector3(1.6f, 1.6f, 1f));
            state.Move(bottom.Find("LinkMarkerDark"), new Vector3(0f, 65f, 0f), new Vector3(1.6f, 1.6f, 1f));
            state.Move(bottom.Find("TextArea/NonEffectArea/AtkDefArea/LinkNum"), new Vector3(55f, 14f, 0f));
        }

        private static void ApplyArtwork(LayoutState state, RawImage illustration)
        {
            if (illustration == null) return;
            var target = illustration.transform;
            var scale = target.localScale;
            if (scale.x == 0f || scale.y == 0f) return;
            // Move the Illust (or illust_sp) container, as the original mod did.
            // The image has its own layout offset (normal/special art starts at Y=59).
            // Replacing that position centers the art instead of lifting it with the frame.
            state.Move(target.parent, new Vector3(0f, 23f, 0f), new Vector3(1.2f, 1.2f, 1f));
        }

        private static void ClearLabel(LayoutState state, GameObject root)
        {
            if (root == null) return;
            var text = root.GetComponent<Text>();
            if (text == null) return;
            state.RememberText(text, true);
            text.text = "";
        }

        private static void ApplyAtkDef(LayoutState state, Text text, Vector3 position)
        {
            if (text == null) return;
            state.Move(text.transform, position, new Vector3(1.4f, 1.4f, 1f));
            state.RememberText(text);
            text.color = Color.white;
            text.fontSize = 50;
            text.fontStyle = FontStyle.Bold;
            state.AddOutline(text);
        }

        [HarmonyPatch(typeof(CardPicture), nameof(CardPicture.SetupCardAsync))]
        private class PatchSetupCardAsync
        {
            static void Prefix(CardPicture __instance)
            {
                RestoreLayout(__instance);
            }
        }

        [HarmonyPatch(typeof(CardPicture), nameof(CardPicture.SetUpCardContent))]
        private class PatchSetUpCardContent
        {
            static void Prefix(CardPicture __instance)
            {
                RestoreLayout(__instance);
            }

            static void Postfix(CardPicture __instance, int __0)
            {
                ApplyLayout(__instance, __0);
            }
        }

        [HarmonyPatch(typeof(CardPicture), nameof(CardPicture.OnDestroy))]
        private class PatchOnDestroy
        {
            static void Prefix(CardPicture __instance)
            {
                layouts.Remove(__instance.GetInstanceID());
            }
        }

        [HarmonyPatch(typeof(CardPictureTop), nameof(CardPictureTop.SetCardTopArea))]
        private class PatchCardPictureTopSetCardTopArea
        {
            static void Postfix(CardPictureTop __instance, int __0)
            {
                var picture = __instance.GetComponentInParent<CardPicture>();
                if (picture == null) return;
                var existing = __instance.transform.Find("AddedText");
                if (picture.IsOverFrame(__0))
                {
                    if (existing != null) existing.gameObject.SetActive(false);
                    return;
                }

                var state = GetLayout(picture);
                state.SetActive(__instance.LevelRoot, false);
                // The method argument is current even if picture.cardId is not yet updated.
                var level = Content.DLL_CardGetLevel(__0);
                if (level == 0) level = Content.DLL_CardGetRank(__0);
                var levelText = level == 0 ? "" : level.ToString();
                if (existing != null)
                {
                    state.SetActive(existing.gameObject, true);
                    var existingText = existing.GetComponent<Text>();
                    if (existingText != null) existingText.text = levelText;
                    return;
                }

                var textObj = new GameObject("AddedText");
                textObj.transform.SetParent(__instance.transform, false);
                // Remember a hidden state for the number when restoring native layout.
                textObj.SetActive(false);
                state.SetActive(textObj, true);
                var text = textObj.AddComponent<Text>();
                text.text = levelText;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontStyle = FontStyle.Bold;
                text.fontSize = 56;
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleCenter;
                var outline = textObj.AddComponent<Outline>();
                outline.effectColor = Color.red;
                outline.effectDistance = new Vector2(1f, -1f);
                textObj.GetComponent<RectTransform>().localPosition = new Vector3(-270f, -180f, 0f);
            }
        }
    }
}
