using UnityEngine;
using UnityEngine.InputSystem;  

public class TrustDebug : MonoBehaviour
{
    public int loseAmount = 1;
    public int gainAmount = 1;

    void Update()
    {
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            GameMngr.Instance.AdjustTrust(-loseAmount);
            Debug.Log($"Trust -{loseAmount} -> {GameMngr.Instance.trust}");
        }

        if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            GameMngr.Instance.AdjustTrust(gainAmount);
            Debug.Log($"Trust +{gainAmount} -> {GameMngr.Instance.trust}");
        }
    }
}