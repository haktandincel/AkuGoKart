using UnityEngine;
using Cinemachine;

public class SplitScreenManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera player1Camera;
    public Camera player2Camera;

    [Header("Split Settings")]
    public bool isSplitActive = true;
    public bool isHorizontalSplit = false; // false = dikey (Blur/Split Fiction stili)

    void Start()
    {
        ApplySplit();
    }

    public void ApplySplit()
    {
        if (!isSplitActive)
        {
            // Tek ekran
            player1Camera.rect = new Rect(0, 0, 1, 1);
            player2Camera.gameObject.SetActive(false);
            return;
        }

        player2Camera.gameObject.SetActive(true);

        if (isHorizontalSplit)
        {
            // Üst / Alt
            player1Camera.rect = new Rect(0, 0.5f, 1, 0.5f);
            player2Camera.rect = new Rect(0, 0,    1, 0.5f);
        }
        else
        {
            // Sol / Sağ (Split Fiction / It Takes Two stili)
            player1Camera.rect = new Rect(0,    0, 0.5f, 1);
            player2Camera.rect = new Rect(0.5f, 0, 0.5f, 1);
        }
    }

    // Çizgi çizmek istersen (ortaya UI çizgisi)
    public void ToggleSplit()
    {
        isSplitActive = !isSplitActive;
        ApplySplit();
    }
}