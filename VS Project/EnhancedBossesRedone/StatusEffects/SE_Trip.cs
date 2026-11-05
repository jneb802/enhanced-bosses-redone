using EnhancedBossesRedone.Data;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace EnhancedBossesRedone.StatusEffects
{
    public class SE_Trip : StatusEffect
    {
        private PostProcessingBehaviour? component;
        private VignetteModel.Settings originalVignette;
        private ChromaticAberrationModel.Settings originalChromaticAberration;
        private ColorGradingModel.Settings originalColorGrading;
        private bool originalVignetteEnabled;
        private bool captured;
        private float elapsed;

        public SE_Trip()
        {
            name = "eb_trip";
            m_name = "Hallucinations";
            m_ttl = 1f;
            m_icon = Main.Bundle!.LoadAsset<Sprite>("trip");
            m_attributes = StatusAttribute.None;
        }

        public override bool CanAdd(Character character)
        {
            return character == Player.m_localPlayer && ConfigManager.BonemassTripEffect!.Value;
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            if (!captured && GameCamera.instance != null)
            {
                component = GameCamera.instance.GetComponent<PostProcessingBehaviour>();
                if (component == null)
                {
                    return;
                }

                originalVignette = component.m_Vignette.model.settings;
                originalChromaticAberration = component.m_ChromaticAberration.model.settings;
                originalColorGrading = component.m_ColorGrading.model.settings;
                originalVignetteEnabled = component.m_Vignette.model.enabled;
                captured = true;
            }

            if (component == null || !captured)
            {
                return;
            }

            elapsed += dt;
            VignetteModel.Settings vignette = originalVignette;
            vignette.color = Color.HSVToRGB(Mathf.Repeat(elapsed * 0.2f, 1f), 1f, 1f);
            vignette.intensity = Mathf.Lerp(originalVignette.intensity, 0.2f, Mathf.Clamp01(elapsed));
            component.m_Vignette.model.settings = vignette;
            component.m_Vignette.model.enabled = true;

            ChromaticAberrationModel.Settings chromatic = originalChromaticAberration;
            float intensity = Mathf.Clamp01(ConfigManager.BonemassTripIntensity!.Value * 0.1f);
            chromatic.intensity = Mathf.Lerp(originalChromaticAberration.intensity, intensity, Mathf.Clamp01(elapsed));
            component.m_ChromaticAberration.model.settings = chromatic;

            ColorGradingModel.Settings grading = originalColorGrading;
            grading.basic.saturation = Mathf.Lerp(originalColorGrading.basic.saturation, 2f, Mathf.Clamp01(elapsed));
            component.m_ColorGrading.model.settings = grading;
        }

        private void RestoreCamera()
        {
            if (captured && component != null)
            {
                component.m_Vignette.model.settings = originalVignette;
                component.m_Vignette.model.enabled = originalVignetteEnabled;
                component.m_ChromaticAberration.model.settings = originalChromaticAberration;
                component.m_ColorGrading.model.settings = originalColorGrading;
            }
            captured = false;
            component = null;
        }

        public override void Stop()
        {
            RestoreCamera();
            base.Stop();
        }

        public override void OnDestroy()
        {
            RestoreCamera();
            base.OnDestroy();
        }
    }
}
