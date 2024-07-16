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
    // �̺�Ʈ
    public delegate void HpChanged(int currentHp, int prevHp);                          // HP ���� �̺�Ʈ
    public delegate void BulletChanged(int bullet, int maxBullet);                      // �Ѿ� ���� �̺�Ʈ
    public delegate void ReloadBullet();                                                // �Ѿ� ���� �̺�Ʈ
    public delegate void MaxBulletChanged(int bullet, int MaxBullet);                   // źâ ���� �̺�Ʈ
    public delegate void ConnectedChanged(int count);                                   // ���� ���� �̺�Ʈ

    // Unique
    private string unique;

    // Navmesh
    private NavMeshAgent agent;

    // HP
    private int hp;

    // �� ���� �Ѿ� ����
    private int bullet;

    // �ִ� �Ѿ� ����
    private int currentMaxBullets;

    // ���� �� ����
    private int reloadBullet;

    // Entity ����
    private PlayerState state;

    // �̺�Ʈ
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

            // HP Text�� ����ֱ� ���� ���� ���� �޾ƿ�
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
            // -�� ���
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

    // ������ ������ �������� üũ
    private bool IsReload(int reload)
    {
        // �� źâ�� �Ѿ��� ���� ���
        if (CurrentMaxBullets <= 0)
        {
            return false;
        }

        // �� źâ�� �Ѿ��� ���� �� �Ѿ��� �������� ���� ���
        if (CurrentMaxBullets >= reload)
        {
            return true;
        }

        // �� źâ�� �Ѿ��� ���� �� �Ѿ��� �������� ������ 0���� �ƴ� ���
        if (CurrentMaxBullets <= reload)
        {
            return true;
        }

        return false;
    }

    private bool IsShooting => Bullets > 0;
    // ����
    public void Reload(int bullet, int maxbullet)
    {
        CurrentMaxBullets = maxbullet;
        Bullets = bullet;
    }

    // �̵�
    public void Movement(UnityEngine.Vector3 direction)
    {
        agent.SetDestination(direction);
    }

    // �߻�
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


    // ���� ���� ��û �Լ�
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
