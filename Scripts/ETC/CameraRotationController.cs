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
        Cursor.lockState = CursorLockMode.Locked; // ���콺 Ŀ���� ȭ�� �߾ӿ� ����
    }

    void Update()
    {
        if (playerBody == null)
            return;

        // ���콺 �Է� �ޱ�
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        // ī�޶��� �¿� ȸ�� ����
        yRotation += mouseX;

        // �÷��̾��� Y�� ȸ���� ����
        playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        // ī�޶� ȸ���� �����ϰ� ����
        virtualCamera.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
