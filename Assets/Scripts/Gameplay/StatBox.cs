using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatBox : MonoBehaviour
{
    public TMP_Text strLabel;
    public TMP_Text hstLabel;
    public TMP_Text wisLabel;
    public TMP_Text scvLabel;

    public void SetCharacter(CharacterData data)
    {
        strLabel.text = data.runtimeStats.strength.ToString();
        hstLabel.text = data.runtimeStats.haste.ToString();
        wisLabel.text = data.runtimeStats.wisdom.ToString();
        scvLabel.text = data.runtimeStats.scavenge.ToString();
    }
}