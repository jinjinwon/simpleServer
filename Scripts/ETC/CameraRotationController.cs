using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    private float yRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 화면 중앙에 고정
    }

    void Update()
    {
        if (playerBody == null)
            return;

        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        // 카메라의 좌우 회전 적용
        yRotation += mouseX;

        // 플레이어의 Y축 회전만 적용
        playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        // 카메라 회전도 동일하게 적용
        virtualCamera.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
