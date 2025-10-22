using MelonLoader;
using UnityEngine;
using System.Collections;
using MasterFaceEditor;
using HarmonyLib;
using Il2CppYgomGame;
using Il2CppYgomGame.Card;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(CardMod), "Card Patch", "1.0.0", "Eglen")]
[assembly: MelonGame("Konami Digital Entertainment Co., Ltd.", "masterduel")]

namespace MasterFaceEditor
{
    public class CardMod : MelonMod
    {

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            
            // Move and scale Art
            MelonCoroutines.Start(WaitAndApplyMoveAndScale(
                "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/MainArea/Illust",
                new Vector3(0f, 23f, 0f),
                new Vector3(1.2f, 1.2f, 1f)
                )
            );
            
            // Move Level Stars
            MelonCoroutines.Start(WaitAndApplyMove(
                "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/CardPictureTop/Root/Level",
                new Vector3(0f, -720f, 0f)
                )
            );
            
            // Move Name
            MelonCoroutines.Start(WaitAndApplyMove(
                "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/CardPictureTop/Root/Name/BaseText",
                new Vector3(-15f, 20f, 0f)
                )
            );
            
            // Move Effect
            MelonCoroutines.Start(WaitAndApplyMove(
                "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/TextArea",
                new Vector3(0f, -280f, 0f)
                )
            );
            
            // Adapt Atk/Def
            MelonCoroutines.Start(WaitAndApplyMove(
                    "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/TextArea/NonEffectArea/AtkDefArea",
                    new Vector3(-300f, 190f, 0f)
                )
            );
            MelonCoroutines.Start(WaitAndApplyAtkDef());
            
            // Hide Spell/Trap Type
            MelonCoroutines.Start(WaitAndApplyMoveAndScale(
                "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/SpellTrap",
                new Vector3(-200f, -230f, 0f),
                new Vector3(0f, 0f, 0f)
                )
            );
            
            // Move Spell/Trap Type Icon
            MelonCoroutines.Start(WaitAndApplyMove(
                "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/SpellTrapIcon",
                new Vector3(-300f, -250f, 0f)
                )
            );
            
            // Move and scale Attribute Icon
            MelonCoroutines.Start(WaitAndApplyMoveAndScale(
                "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/AttrIcon",
                new Vector3(281f, 441.4f, 0f),
                new Vector3(1.3f, 1.3f, 1f)
                )
            );
            
            // Move Pendulum assets
            MelonCoroutines.Start(WaitAndApplyMove(
                    "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/PendulumText",
                    new Vector3(0, 10, 0)
                )
            );
            MelonCoroutines.Start(WaitAndApplyMove(
                    "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/PendulumScaleL",
                    new Vector3(-295, -30, 0)
                )
            );
            MelonCoroutines.Start(WaitAndApplyMove(
                    "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/PendulumScaleR",
                    new Vector3(295, -30, 0)
                )
            );
            
            // Move and scale Link assets
            MelonCoroutines.Start(WaitAndApplyMoveAndScale(
                    "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/LinkMarker",
                    new Vector3(0f, 65f, 0f),
                    new Vector3(1.6f, 1.6f, 1f)
                )
            );
            MelonCoroutines.Start(WaitAndApplyMoveAndScale(
                    "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/LinkMarkerDark",
                    new Vector3(0f, 65f, 0f),
                    new Vector3(1.6f, 1.6f, 1f)
                )
            );
            MelonCoroutines.Start(WaitAndApplyMove(
                    "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/TextArea/NonEffectArea/AtkDefArea/LinkNum",
                    new Vector3(55f, 14f, 0f)
                )
            );
            
            HarmonyInstance.PatchAll();
            
        }
        
        private IEnumerator WaitAndApplyAtkDef()
        {
            const string atkDefArea = "CardPictureCreator/CardPictureCanvas/CardPicture(Clone)/Root/BottomArea/TextArea/NonEffectArea/AtkDefArea";
            
            GameObject target = null;

            while (target == null)
            {
                target = GameObject.Find($"{atkDefArea}/AtkRoot/AtkText");
                yield return new WaitForSeconds(1f);
            }
            
            ApplyAtkDef(target, new Vector3(245, 54, 0));
            
            target = null;
            
            while (target == null)
            {
                target = GameObject.Find($"{atkDefArea}/DefRoot/DefText");
                yield return new WaitForSeconds(1f);
            }
            
            ApplyAtkDef(target, new Vector3(325, 54, 0));
            
            target = null;
            
            while (target == null)
            {
                target = GameObject.Find($"{atkDefArea}/DefRoot");
                yield return new WaitForSeconds(1f);
            }
            
            var text = target.GetComponent<Text>();
            if (text != null)
            {
                text.text = "";
            }
            
            target = null;
            
            while (target == null)
            {
                target = GameObject.Find($"{atkDefArea}/AtkRoot");
                yield return new WaitForSeconds(1f);
            }
            
            text = target.GetComponent<Text>();
            if (text != null)
            {
                text.text = "";
            }
            
        }

        private void ApplyAtkDef(GameObject target, Vector3 position)
        {
            var rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localPosition = position;
                rect.localScale = new Vector3(1.4f, 1.4f, 1f);
            }

            var text = target.GetComponent<Text>();
            if (text != null)
            {
                text.color = Color.white;
                text.fontSize = 50;
                text.fontStyle = FontStyle.Bold;
                
                var outline = text.gameObject.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1f, -1f);
            }
        }
        
        private IEnumerator WaitAndApplyMoveAndScale(string path, Vector3 position, Vector3 scale)
        {
            GameObject target = null;

            while (target == null)
            {
                target = GameObject.Find(path);
                yield return new WaitForSeconds(1f);
            }
            
            var rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = scale;
                rect.localPosition = position;
                MelonLogger.Msg("Scale and position applied to " + target.name);
            }
            else
            {
                MelonLogger.Msg("RectTransform not found on target.");
            }
        }
        
        private IEnumerator WaitAndApplyMove(string path, Vector3 position)
        {
            GameObject target = null;

            while (target == null)
            {
                target = GameObject.Find(path);
                yield return new WaitForSeconds(1f);
            }

            var rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localPosition = position;
                MelonLogger.Msg("Position applied to " + path);
            }
            else
            {
                MelonLogger.Msg("RectTransform not found on target " + path);
            }
        }
        
        // TODO: Create new shine effect instead of removing
        [HarmonyPatch(typeof(CardPictureSetting), nameof(CardPictureSetting.GetCardMaterial))]
        private class PatchCardPictureSettingGetCardMaterial
        {
            static void Prefix(ref CardFinishSetting.FinishType finishType)
            {
                // Make every card have normal finish effect
                finishType = CardFinishSetting.FinishType.Normal;
            }
        }
        
        [HarmonyPatch(typeof(CardPictureTop), nameof(CardPictureTop.SetCardTopArea))]
        private class PatchCardPictureTopSetCardTopArea
        {
            static void Postfix(ref CardPictureTop __instance)
            {
                // Hide Level Stars
                __instance.LevelRoot.SetActive(false);
                
                // Add level number to card
                var go = (__instance as MonoBehaviour)?.gameObject;
                if (go == null) return;
                
                Transform t = go.transform;
                
                // Climb up until Card Picture found by name
                while (t != null && t.name != "CardPicture(Clone)")
                    t = t.parent;

                var level = "";
                if (t != null)
                {
                    var pictureComponent = t.GetComponent<CardPicture>();
                    if (pictureComponent != null)
                    {
                        // SURELY there is a better way to do this?
                        // Look for Level or Rank
                        level = Content.DLL_CardGetLevel(pictureComponent.cardId).ToString();
                        if (level.Equals("0"))
                        {
                            level = Content.DLL_CardGetRank(pictureComponent.cardId).ToString();
                            if (level.Equals("0"))
                            {
                                // No level or rank, so don't add text
                                level = "";
                            }
                        }
                    }
                }

                var existing = go.transform.Find("AddedText");
                if (existing != null)
                {
                    var txt = existing.GetComponent<Text>();
                    if (txt != null)
                    {
                        txt.text = level;
                        return;
                    }
                }

                // create new if missing
                GameObject textObj = new GameObject("AddedText");
                textObj.transform.SetParent(go.transform, false);

                var text = textObj.AddComponent<Text>();
                text.text = level;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontStyle = FontStyle.Bold;
                text.fontSize = 56;
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleCenter;
                
                var outline = textObj.AddComponent<Outline>();
                outline.effectColor = Color.red;
                outline.effectDistance = new Vector2(1f, -1f);

                var rect = textObj.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(-270, -180, 0);
            }
        }
    }
}