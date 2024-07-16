using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    // 이벤트
    public delegate void HpChanged(int currentHp, int prevHp);                          // HP 변동 이벤트
    public delegate void BulletChanged(int bullet, int maxBullet);                      // 총알 변동 이벤트
    public delegate void ReloadBullet();                                                // 총알 장전 이벤트
    public delegate void MaxBulletChanged(int bullet, int MaxBullet);                   // 탄창 변동 이벤트
    public delegate void ConnectedChanged(int count);                                   // 서버 접속 이벤트

    // Unique
    private string unique;

    // Navmesh
    private NavMeshAgent agent;

    // HP
    private int hp;

    // 내 현재 총알 개수
    private int bullet;

    // 최대 총알 개수
    private int currentMaxBullets;

    // 장전 시 개수
    private int reloadBullet;

    // Entity 상태
    private PlayerState state;

    // 이벤트
    public event HpChanged OnHPChanged;
    public event BulletChanged OnBulletChanged;
    public event ReloadBullet OnReloadBullet;
    public event MaxBulletChanged OnMaxBulletChanged;
    public event ConnectedChanged OnConnectedChanged;

    public string Unique => unique;

    public PlayerState State => hp != 0? PlayerState.Alive : PlayerState.Die;

    public int HP
    {
        get => hp;
        set 
        {
            if (hp == value)
                return;

            int prevHp = hp;
            hp = value;

            // HP Text를 띄워주기 위한 이전 값을 받아옴
            OnHPChanged?.Invoke(hp, prevHp);
        }
    }

    public int Bullets
    {
        get => bullet;
        set
        {
            if (bullet == value)
                return;

            bullet = value;
            OnBulletChanged?.Invoke(bullet,currentMaxBullets);
        }
    }

    public int CurrentMaxBullets
    {
        get => currentMaxBullets;
        set
        {
            // -인 경우
            if (value < 0)
                value = 0;

            if (currentMaxBullets == value)
                return;

            currentMaxBullets = value; 
            OnMaxBulletChanged?.Invoke(bullet, currentMaxBullets);
        }
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialized(int maxbullet, int reloadbullet, int bullet, string guid)
    {
        Bullets = bullet;
        CurrentMaxBullets = maxbullet;
        reloadBullet = reloadbullet;
        unique = guid;
    }

    // 장전이 가능한 상태인지 체크
    private bool IsReload(int reload)
    {
        // 내 탄창에 총알이 없는 경우
        if (CurrentMaxBullets <= 0)
        {
            return false;
        }

        // 내 탄창에 총알이 장전 할 총알의 개수보다 많은 경우
        if (CurrentMaxBullets >= reload)
        {
            return true;
        }

        // 내 탄창에 총알이 장전 할 총알의 개수보다 적지만 0개는 아닌 경우
        if (CurrentMaxBullets <= reload)
        {
            return true;
        }

        return false;
    }

    private bool IsShooting => Bullets > 0;
    // 장전
    public void Reload(int bullet, int maxbullet)
    {
        CurrentMaxBullets = maxbullet;
        Bullets = bullet;
    }

    // 이동
    public void Movement(UnityEngine.Vector3 direction)
    {
        agent.SetDestination(direction);
    }

    // 발사
    public void Shoot(UnityEngine.Vector3 direction,int bullet, int maxbullet)
    {
        if (IsShooting == false)
            return;

        UnityEngine.Vector3 position = new UnityEngine.Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + 0.35f);
        GameObject bulletObject = PoolManager.Instance.SpawnObject(PoolManager.Instance.BulletPrefab,null, position);

        Bullets = bullet;
        CurrentMaxBullets = maxbullet;

        Bullet tbullet = bulletObject.GetComponent<Bullet>();
        if (tbullet != null)
        {
            tbullet.SetDirection(direction);
        }
    }


    public void TakeDamage(int damage)
    {
        HP -= damage;
        PoolManager.Instance.Spawn(PoolManager.Instance.HitPrefab, this.transform);
    }


    // 서버 검증 요청 함수
    public async Task RequestMoveToServer(UnityEngine.Vector3 targetPos)
    {
        try
        {
            string request = $"MOVE:{targetPos.x},{targetPos.y},{targetPos.z}";
            byte[] requestBytes = Encoding.ASCII.GetBytes(request);
            await Client.Instance.stream.WriteAsync(requestBytes, 0, requestBytes.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await Client.Instance.stream.ReadAsync(buffer, 0, buffer.Length);

            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            if (response == "Approved")
            {
                transform.position = targetPos;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error: " + ex.Message);
        }
    }

    public async Task RequestReload()
    {
        try
        {
            string request = "RELOAD:";
            byte[] requestBytes = Encoding.ASCII.GetBytes(request);
            await Client.Instance.stream.WriteAsync(requestBytes, 0, requestBytes.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await Client.Instance.stream.ReadAsync(buffer, 0, buffer.Length);

            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            if (response.Contains("Approved"))
            {
                string initData = response.Substring("ApprovedRELOAD:".Length);
                string[] parts = initData.Split(',');

                if (parts.Length == 3)
                {
                    Reload(int.Parse(parts[0]), int.Parse(parts[1]));
                }

                Debug.Log("Reload successful");
            }
            else
            {
                Debug.Log("Reload denied");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error: " + ex.Message);
        }
    }

    public async Task RequestShoot(UnityEngine.Vector3 targetPos)
    {
        try
        {
            string request = $"SHOOT:{targetPos.x},{targetPos.y},{targetPos.z}";
            byte[] requestBytes = Encoding.ASCII.GetBytes(request);
            await Client.Instance.stream.WriteAsync(requestBytes, 0, requestBytes.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await Client.Instance.stream.ReadAsync(buffer, 0, buffer.Length);

            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            if (response.Contains("Approved"))
            {
                string initData = response.Substring("ApprovedSHOOT:".Length);
                string[] parts = initData.Split(',');

                float x = float.Parse(parts[0]);
                float y = float.Parse(parts[1]);
                float z = float.Parse(parts[2]);
                UnityEngine.Vector3 vector = new UnityEngine.Vector3(x, y, z);

                Shoot(vector, int.Parse(parts[3]), int.Parse(parts[4]));

                Debug.Log("Shoot successful");
            }
            else
            {
                Debug.Log("Shoot denied");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error: " + ex.Message);
        }
    }

    public async Task RequestConnected()
    {
        try
        {
            string request = "CONNECTED";
            byte[] requestBytes = Encoding.ASCII.GetBytes(request);
            await Client.Instance.stream.WriteAsync(requestBytes, 0, requestBytes.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error: " + ex.Message);
        }
    }
}
