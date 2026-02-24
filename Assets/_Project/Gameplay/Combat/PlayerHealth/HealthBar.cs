using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class HealthBarSpawner : IComponentData
{
    public GameObject HealthBarPrefab;
    public float OpponentHeightOffset;
    public float PlayerTowardCameraOffset;
    public float PlayerHeightOffset;
}

public class HealthUI : IComponentData, IDisposable, ICloneable
{
    public Transform HealthBar;
    public Image HealthSlider;
    public float OpponentHeightOffset;
    public float PlayerHeightOffset;
    public float PlayerTowardCameraOffset;

    public object Clone()
    {
        if (HealthBar == null || HealthBar.gameObject == null)
        {
            return new HealthUI();
        }

        var newHealthBar = Object.Instantiate(HealthBar.gameObject);
        var images = HealthBar.GetComponentsInChildren<Image>();
        return new HealthUI
        {
            HealthBar = newHealthBar.GetComponent<Transform>(),
            HealthSlider = images[1]
        };
    }

    public void Dispose()
    {
        if (HealthBar != null)
            Object.Destroy(HealthBar.gameObject);
    }
}