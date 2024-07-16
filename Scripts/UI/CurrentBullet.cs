using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrentBullet : MonoBehaviour
{
    public Entity entity;
    public TextMeshProUGUI textMeshProUGUI;

    private void Start()
    {
        entity.OnBulletChanged += OnValueUpdate;
        entity.OnMaxBulletChanged += OnValueUpdate;
    }

    public void OnValueUpdate(int bullet, int maxBullet)
    {
        textMeshProUGUI.text = $"{bullet} / {maxBullet}";
    }
}
