using UnityEngine;

public class SpellBookController : MonoBehaviour
{
    [Header("Data")]
    public SpellBookDatabase database;

    [Header("Page UI")]
    public SpellPageUI spellPage;        // right page (glyph + text)
    public SpellCastPageUI castPage;     // left page (cast drawing)

    [Header("State")]
    [SerializeField]
    private int currentSpellIndex = 0;

    private void Start()
    {
        RefreshPage();
    }

    // Next button
    public void OnNextButtonClicked()
    {
        if (database == null || database.Count == 0) return;
        if (currentSpellIndex >= database.Count - 1) return;

        currentSpellIndex++;
        RefreshPage();
    }

    // Previous button
    public void OnPrevButtonClicked()
    {
        if (database == null || database.Count == 0) return;
        if (currentSpellIndex <= 0) return;

        currentSpellIndex--;
        RefreshPage();
    }

    private void RefreshPage()
    {
        if (database == null || database.Count == 0)
        {
            spellPage?.ShowSpell(null);
            castPage?.ShowSpell(null);
            return;
        }

        var spell = database.Get(currentSpellIndex);

        spellPage?.ShowSpell(spell);    // right page
        castPage?.ShowSpell(spell);     // left page
    }
}
