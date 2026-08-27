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
        strLabel.text = $"STR:\t{data.runtimeStats.strength}";
        hstLabel.text = $"HST:\t{data.runtimeStats.haste}";
        wisLabel.text = $"WIS:\t{data.runtimeStats.wisdom}";
        scvLabel.text = $"SCV:\t{data.runtimeStats.scavenge}";
    }
}