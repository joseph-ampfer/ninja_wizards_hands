using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class CooldownsUI : MonoBehaviour
{
    public UIDocument uIDocument;
    public VisualElement cooldownContainer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var root = uIDocument.rootVisualElement;
        cooldownContainer = root.Q<VisualElement>("CooldownContainer");
        if (cooldownContainer == null) 
            Debug.LogError("[CooldownsUI] Cooldown container not found");
        cooldownContainer.Clear();

        NetworkSpellCaster.OnSpellCast += OnSpellCast; // Subscribe
        NetworkSpellCaster.OnSpellCastDuringCooldown += OnSpellCastDuringCooldown;
    }

    void OnDisable()
    {
        NetworkSpellCaster.OnSpellCast -= OnSpellCast; // Unsubscribe
        NetworkSpellCaster.OnSpellCastDuringCooldown -= OnSpellCastDuringCooldown;
    }

    private void OnSpellCast(NetworkSpellCaster caster, Spell spell)
    {
        Debug.Log("[CooldownsUI] OnSpellCast: " + spell.name);

        // if not local caster, return
        if (!caster.IsOwner)
        {
            Debug.Log("[CooldownsUI] OnSpellCast: Not local caster");
            return;
        }
        if (spell.icon == null)
        {
            Debug.Log("[CooldownsUI] OnSpellCast: Icon is null, skipping");
            return;
        }

        var cooldown = new VisualElement();
        cooldown.AddToClassList("cooldown");
        cooldown.name = spell.name;
        cooldownContainer.Add(cooldown);

        var spellIcon = new Image
        {
            image = spell.icon,
            scaleMode = ScaleMode.StretchToFill
        };
        spellIcon.AddToClassList("spellIcon");

        cooldown.Add(spellIcon);

        var overlay = new VisualElement();
        overlay.AddToClassList("overlay");

        cooldown.Add(overlay);

        StartCoroutine(CooldownCoroutine(cooldown, overlay, spell.cooldown));
    }

    private IEnumerator CooldownCoroutine(VisualElement cooldown, VisualElement overlay, float duration)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = (duration - elapsed) / duration; //0-1
            // width of overlay shrink to 0
            overlay.style.width = new Length(scale * 100, LengthUnit.Percent);


            yield return null; // onto next frame
        }

        // duration has elapsed, remove cooldown
        DOTween.Kill(cooldown);
        cooldown.RemoveFromHierarchy();
    }

    private void OnSpellCastDuringCooldown(NetworkSpellCaster caster, Spell spell)
    {
        // if not local caster, return
        if (!caster.IsOwner)
        {
            Debug.Log("[CooldownsUI] OnSpellCastDuringCooldown: Not local caster");
            return;
        }
        if (spell.icon == null)
        {
            Debug.Log("[CooldownsUI] OnSpellCastDuringCooldown: Icon is null, skipping");
            return;
        }
        var cooldown = cooldownContainer.Q<VisualElement>(spell.name);
        if (cooldown == null)
        {
            Debug.LogWarning("[CooldownsUI] OnSpellCastDuringCooldown cooldown is null for " + spell.name);
            return;
        }

        cooldown.AddToClassList("cooldown--flash");

        // // shake with DOTween
        // cooldown.DOShake(1f, 10, 10, 90, true, true);
        //cooldown gets a 2px red boarder (ease in 0.5s) and then back to normal (ease out 0.5s)

        DOVirtual.DelayedCall(0.5f, () =>
        {
            cooldown.RemoveFromClassList("cooldown--flash");
        });


    }

    // Update is called once per frame

}
