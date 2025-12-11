using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class ModularScaleObject : MonoBehaviour
{
    [Header("Target")]
    public List<Transform> targets;
    private Dictionary<Transform, Vector3> defaultScales = new();
    [Header("Settings")]
    public float scaleMulti;
    public float scaleDuration;
    public Ease easeType;
    [Space]
    public bool resetScaleOnAnimate;
    [Header("Type")]
    public ScaleType currentScaleType;
    public enum ScaleType { Punch, Once }

    void Start()
    {
        foreach (var t in targets)
        {
            if (t != null)
            {
                defaultScales[t] = t.localScale;
            }
        }
    }

    [Button]
    public void DoScale()
    {
        foreach (var t in targets)
        {
            if (t == null) continue;

            t.DOKill(); 

            if (resetScaleOnAnimate && defaultScales.ContainsKey(t))
            {
                t.localScale = defaultScales[t]; 
            }

            if (currentScaleType == ScaleType.Once)
            {
                t.DOScale(t.localScale * scaleMulti, scaleDuration).SetEase(easeType);
            }

            if (currentScaleType == ScaleType.Punch)
            {
                t.DOPunchScale(t.localScale * scaleMulti, scaleDuration, 1, 1).SetEase(easeType);
            }
        }
    }
}
