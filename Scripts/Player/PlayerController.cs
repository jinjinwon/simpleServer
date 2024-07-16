using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 30f; // 이동 속도 설정

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
            // UI 좌표를 월드 좌표로 변환
            RectTransform rectTransform = shootPointController.GetComponent<RectTransform>();
            Vector3 shootPointWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(rectTransform.position.x, rectTransform.position.y, Camera.main.nearClipPlane));

            // 발사 방향 설정
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

        // WASD 키 입력 처리
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

        // 방향 벡터를 정규화하여 일관된 속도 유지
        moveDirection = moveDirection.normalized * moveSpeed * Time.deltaTime;

        // 새로운 목표 위치 설정
        Vector3 targetPosition = transform.position + moveDirection;

        // NavMeshAgent를 사용하여 목표 위치로 이동
        entity.Movement(targetPosition);
    }
}
