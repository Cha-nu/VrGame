using UnityEngine;
using UnityEngine.UI;

public class SpellCastPageUI : MonoBehaviour
{
    public Image castImage;

    public void ShowSpell(SpellDefinition spell)
    {
        if (spell == null || spell.castIllustration == null)
        {
            if (castImage != null)
            {
                castImage.enabled = false;
            }
            return;
        }

        castImage.enabled = true;
        castImage.sprite = spell.castIllustration;
    }
}
