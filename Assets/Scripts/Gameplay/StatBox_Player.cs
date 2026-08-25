using UnityEngine;
using TMPro;

public class StatBox_Player : MonoBehaviour
{
    public TMP_Text strLabel;
    public TMP_Text hstLabel;
    public TMP_Text wisLabel;
    public TMP_Text scvLabel;

    public void SetStats(StatBlock stats)
    {
        strLabel.text = stats.strength.ToString();
        hstLabel.text = stats.haste.ToString();
        wisLabel.text = stats.wisdom.ToString();
        scvLabel.text = stats.scavenge.ToString();
    }
}