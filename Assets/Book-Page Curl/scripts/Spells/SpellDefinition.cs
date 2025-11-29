using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Spell Definition")]
public class SpellDefinition : ScriptableObject
{
    [Header("Identity")]
    public string spellId;
    public string displayName;

    [Header("Visuals")]
    public Sprite glyph;                 // right page glyph
    public Sprite castIllustration;      // left page drawing of the spell
    public Color accentColor = Color.white;

    [Header("Description")]
    [TextArea(3, 8)]
    public string description;

    [Header("Optional Stats")]
    public int manaCost;
    public float cooldown;
    public string incantation;
}
