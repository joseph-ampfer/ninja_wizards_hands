using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GraphicsSettingsManager : MonoBehaviour
{
    public Volume globalVolume;
    private UniversalRenderPipelineAsset urpAsset;
    private Bloom bloom;
    private MotionBlur motionBlur;

    public static GraphicsSettingsManager Instance; // singleton


    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (Instance == null)
        {
            //if not, set instance to this
            Instance = this;
        }
        //If instance already exists and it's not this:
        else if (Instance != this)
        {
            Destroy(gameObject);   
        }
        
        //Sets this to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out bloom);
            globalVolume.profile.TryGet(out motionBlur);
        }
        else
        {
            Debug.LogWarning("[GraphicsSettingsManager] globalVolume or its profile is not assigned; bloom/motion blur toggles will be ignored.");
        }

        // Re-apply saved preferences so settings persist across scene loads
        // without requiring the player to reopen the settings panel.
        SetRenderScale(PlayerPrefs.GetFloat("RenderScale", 1f));
        SetBloom(PlayerPrefs.GetInt("Bloom", 1) == 1);
        SetMotionBlur(PlayerPrefs.GetInt("MotionBlur", 1) == 1);
    }

    // Call these from your UI sliders/toggles
    public void SetRenderScale(float value)  // 0.5 - 1.0
    {
        Debug.Log("[GraphicsSettingsManager] Setting render scale to " + value);
        if (urpAsset != null) urpAsset.renderScale = value;
        PlayerPrefs.SetFloat("RenderScale", value);
    }

    public void SetBloom(bool enabled)
    {
        if (bloom != null) bloom.active = enabled;
        PlayerPrefs.SetInt("Bloom", enabled ? 1 : 0);
    }

    public void SetMotionBlur(bool enabled)
    {
        if (motionBlur != null) motionBlur.active = enabled;
        PlayerPrefs.SetInt("MotionBlur", enabled ? 1 : 0);
    }
}