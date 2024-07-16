using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectedUser : MonoBehaviour
{
    public Entity entity;
    public TextMeshProUGUI textMeshProUGUI;

    private void Start()
    {
        entity.OnConnectedChanged += OnUpdateUser;
    }

    public void OnUpdateUser(int count)
    {
        textMeshProUGUI.text = $"{count} User";
    }
}
