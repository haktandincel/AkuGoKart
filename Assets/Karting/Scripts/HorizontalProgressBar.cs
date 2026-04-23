using System;
using UnityEngine;


[Serializable]
public class MyHorizontalProgressBar : ProgressBar
{
    private float realProgress = 0f;

    public override void SetProgress(float progress)
    {
        realProgress = Mathf.Clamp01(progress);
        base.SetProgress(realProgress);
    }

    void Start()
    {
        realProgress = 0f;
        base.SetProgress(0f);
    }

    public float GetProgress()
    {
        Debug.Log("Current Progress: " + realProgress);
        return realProgress;
        
    }
}