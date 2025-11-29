using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellPageUI : MonoBehaviour
{
    [Header("UI References")]
    public Image glyphImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [Header("Optional styling")]
    public Graphic[] accentGraphics;

    public void ShowSpell(SpellDefinition spell)
    {
        if (spell == null)
        {
            glyphImage.enabled = false;
            titleText.text = "";
            descriptionText.text = "";
            return;
        }

        glyphImage.enabled = spell.glyph != null;
        glyphImage.sprite = spell.glyph;

        titleText.text = spell.displayName;
        descriptionText.text = spell.description;

        foreach (var g in accentGraphics)
        {
            if (g != null) g.color = spell.accentColor;
        }
    }
}
