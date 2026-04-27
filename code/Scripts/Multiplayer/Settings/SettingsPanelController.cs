using UnityEngine;
using UnityEngine.UIElements;

public class SettingsPanelController
{
    private const string ActivePresetClass = "preset-btn--active";

    // Preset definitions (slider percent, bloom, motion blur)
    private const int LowScale = 60;
    private const bool LowBloom = false;
    private const bool LowMotionBlur = false;

    private const int MediumScale = 80;
    private const bool MediumBloom = true;
    private const bool MediumMotionBlur = false;

    private const int HighScale = 100;
    private const bool HighBloom = true;
    private const bool HighMotionBlur = true;

    private VisualElement root;

    // State is stored in the same integer space as the slider (50..100)
    // and converted to the 0.5..1.0 render-scale float only at apply time.
    private int renderScalePercent;
    private bool bloomValue;
    private bool motionBlurValue;

    private SliderInt renderScaleSlider;
    private Label renderScaleValueLabel;
    private Toggle bloomToggle;
    private Toggle motionBlurToggle;
    private Button settingsCloseBtn;
    private Button settingsApplyBtn;
    private Button presetLowBtn;
    private Button presetMediumBtn;
    private Button presetHighBtn;

    public SettingsPanelController(VisualElement settingsPanelRoot)
    {
        root = settingsPanelRoot;

        renderScaleSlider = root.Q<SliderInt>("render-scale-slider");
        bloomToggle = root.Q<Toggle>("bloom-toggle");
        motionBlurToggle = root.Q<Toggle>("motion-blur-toggle");
        settingsCloseBtn = root.Q<Button>("settings-close-btn");
        settingsApplyBtn = root.Q<Button>("settings-apply-btn");
        renderScaleValueLabel = root.Q<Label>("render-scale-value");
        presetLowBtn = root.Q<Button>("preset-low");
        presetMediumBtn = root.Q<Button>("preset-medium");
        presetHighBtn = root.Q<Button>("preset-high");

        RegisterCallbacks();
    }

    public void RegisterCallbacks()
    {
        renderScaleSlider.RegisterValueChangedCallback(evt =>
        {
            renderScalePercent = evt.newValue;
            renderScaleValueLabel.text = evt.newValue + "%";
            RefreshPresetHighlight();
        });

        bloomToggle.RegisterValueChangedCallback(evt =>
        {
            bloomValue = evt.newValue;
            RefreshPresetHighlight();
        });

        motionBlurToggle.RegisterValueChangedCallback(evt =>
        {
            motionBlurValue = evt.newValue;
            RefreshPresetHighlight();
        });

        settingsCloseBtn.RegisterCallback<ClickEvent>(evt =>
        {
            root.style.display = DisplayStyle.None;
        });

        settingsApplyBtn.RegisterCallback<ClickEvent>(evt => ApplySettings());

        if (presetLowBtn != null)
            presetLowBtn.RegisterCallback<ClickEvent>(evt =>
                ApplyPreset(LowScale, LowBloom, LowMotionBlur));

        if (presetMediumBtn != null)
            presetMediumBtn.RegisterCallback<ClickEvent>(evt =>
                ApplyPreset(MediumScale, MediumBloom, MediumMotionBlur));

        if (presetHighBtn != null)
            presetHighBtn.RegisterCallback<ClickEvent>(evt =>
                ApplyPreset(HighScale, HighBloom, HighMotionBlur));
    }

    public void LoadSavedValues()
    {
        renderScalePercent = Mathf.RoundToInt(PlayerPrefs.GetFloat("RenderScale", 1f) * 100f);
        bloomValue = PlayerPrefs.GetInt("Bloom", 1) == 1;
        motionBlurValue = PlayerPrefs.GetInt("MotionBlur", 1) == 1;

        // SetValueWithoutNotify avoids firing the change callbacks, which
        // means we must also push the derived UI state (label, highlight)
        // ourselves below.
        renderScaleSlider.SetValueWithoutNotify(renderScalePercent);
        bloomToggle.SetValueWithoutNotify(bloomValue);
        motionBlurToggle.SetValueWithoutNotify(motionBlurValue);

        renderScaleValueLabel.text = renderScalePercent + "%";
        RefreshPresetHighlight();
    }

    public void ApplySettings()
    {
        GraphicsSettingsManager.Instance.SetRenderScale(renderScalePercent / 100f);
        GraphicsSettingsManager.Instance.SetBloom(bloomValue);
        GraphicsSettingsManager.Instance.SetMotionBlur(motionBlurValue);
        root.style.display = DisplayStyle.None;
    }

    private void ApplyPreset(int scalePercent, bool bloom, bool motionBlur)
    {
        renderScalePercent = scalePercent;
        bloomValue = bloom;
        motionBlurValue = motionBlur;

        renderScaleSlider.SetValueWithoutNotify(scalePercent);
        bloomToggle.SetValueWithoutNotify(bloom);
        motionBlurToggle.SetValueWithoutNotify(motionBlur);

        renderScaleValueLabel.text = scalePercent + "%";
        RefreshPresetHighlight();
    }

    private void RefreshPresetHighlight()
    {
        SetPresetActive(presetLowBtn, Matches(LowScale, LowBloom, LowMotionBlur));
        SetPresetActive(presetMediumBtn, Matches(MediumScale, MediumBloom, MediumMotionBlur));
        SetPresetActive(presetHighBtn, Matches(HighScale, HighBloom, HighMotionBlur));
    }

    private bool Matches(int scalePercent, bool bloom, bool motionBlur)
    {
        return renderScalePercent == scalePercent
            && bloomValue == bloom
            && motionBlurValue == motionBlur;
    }

    private static void SetPresetActive(Button btn, bool active)
    {
        if (btn == null) return;
        if (active) btn.AddToClassList(ActivePresetClass);
        else btn.RemoveFromClassList(ActivePresetClass);
    }
}
