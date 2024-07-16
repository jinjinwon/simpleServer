using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootPointController : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        // ���콺 ��ġ�� ĵ���� ��ǥ�� ��ȯ
        Vector2 mousePosition = Input.mousePosition;
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, mousePosition, canvas.worldCamera, out canvasPosition);

        // ShootPoint ��ġ ����
        rectTransform.anchoredPosition = canvasPosition;
    }
}
