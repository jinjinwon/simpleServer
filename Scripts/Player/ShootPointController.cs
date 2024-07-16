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
        // 마우스 위치를 캔버스 좌표로 변환
        Vector2 mousePosition = Input.mousePosition;
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, mousePosition, canvas.worldCamera, out canvasPosition);

        // ShootPoint 위치 설정
        rectTransform.anchoredPosition = canvasPosition;
    }
}
