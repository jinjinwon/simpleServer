using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 30f; // �̵� �ӵ� ����

    private Vector3 moveDirection;
    private Entity entity;

    [SerializeField]
    private ShootPointController shootPointController;

    public void Start()
    {
        entity = GetComponent<Entity>();
    }

    public void Update()
    {
        HandleMovement();

        if (Input.GetMouseButtonDown(0))
        {
            // UI ��ǥ�� ���� ��ǥ�� ��ȯ
            RectTransform rectTransform = shootPointController.GetComponent<RectTransform>();
            Vector3 shootPointWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(rectTransform.position.x, rectTransform.position.y, Camera.main.nearClipPlane));

            // �߻� ���� ����
            Vector3 direction = (shootPointWorldPosition - Camera.main.transform.position).normalized;

            entity.RequestShoot(direction);
        }
        if (Input.GetMouseButtonDown(1))
        {
            entity.RequestReload();
        }
    }

    private void HandleMovement()
    {
        moveDirection = Vector3.zero;

        // WASD Ű �Է� ó��
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right;
        }

        // ���� ���͸� ����ȭ�Ͽ� �ϰ��� �ӵ� ����
        moveDirection = moveDirection.normalized * moveSpeed * Time.deltaTime;

        // ���ο� ��ǥ ��ġ ����
        Vector3 targetPosition = transform.position + moveDirection;

        // NavMeshAgent�� ����Ͽ� ��ǥ ��ġ�� �̵�
        entity.Movement(targetPosition);
    }
}
