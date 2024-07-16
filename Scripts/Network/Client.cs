using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
//#if UNITY_EDITOR
//using UnityEditor.PackageManager;
//#endif
using static Entity;
using System.Xml;
using System.Collections.Generic;
using UnityEditor;
using static UnityEngine.EventSystems.EventTrigger;
using PimDeWitte.UnityMainThreadDispatcher;
//using UnityEditor.Experimental.GraphView;

public class Client : MonoSingleton<Client>
{
    private TcpClient client;
    public NetworkStream stream;

    [HideInInspector]
    public string serverIp = "192.168.219.101";
    public int serverPort = 8888;

    // 현재 서버와 연결이 되어있는 상황이라면
    public bool Connected => client != null && client.Connected;

    public Dictionary<string, Entity> Units = new Dictionary<string, Entity>();

    public Entity Player;
    void Awake()
    {
        serverIp = GetLocalIPAddress();
        ConnectToServer();
    }

    private async void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIp, serverPort);
            stream = client.GetStream();
            Debug.Log("Connected to server");

            // 서버로부터 위치 업데이트를 비동기적으로 수신
            Task.Run(() => ReceiveUpdates());
        }
        catch (SocketException ex)
        {
            Debug.LogError("SocketException: " + ex.Message);
            Debug.LogError("SocketException StackTrace: " + ex.StackTrace);
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception: " + ex.Message);
            Debug.LogError("Exception StackTrace: " + ex.StackTrace);
        }
    }

    async Task ReceiveUpdates()
    {
        byte[] buffer = new byte[1024];

        while (true)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    if (message.StartsWith("MOVE:"))
                    {
                        string positionData = message.Substring(5);
                        string[] parts = positionData.Split(',');
                        if (parts.Length == 3)
                        {
                            float x = float.Parse(parts[0]);
                            float y = float.Parse(parts[1]);
                            float z = float.Parse(parts[2]);
                            Vector3 vector = new Vector3(x, y, z);
                        }
                    }
                    else if (message.StartsWith("CONNECTED:"))
                    {
                        string initData = message.Substring("CONNECTED:".Length);
                        string[] parts = initData.Split(',');
                        if (parts.Length == 4)
                        {
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                if (Player != null)
                                {
                                    Player.Initialized(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[3]), parts[2]);
                                }

                                if (Units.ContainsKey(parts[2]) == false)
                                {
                                    Units.Add(parts[2], Player);
                                }
                            });
                        }
                    }
                    else if (message.StartsWith("NEW_CLIENT:"))
                    {
                        string newClientData = message.Substring("NEW_CLIENT:".Length);
                        string[] parts = newClientData.Split(',');
                        if (parts.Length == 4)
                        {
                            int maxBullet = int.Parse(parts[0]);
                            int reloadBullet = int.Parse(parts[1]);
                            string uniqueId = parts[2];
                            int bullet = int.Parse(parts[3]);

                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                if(Units.ContainsKey(uniqueId) == false)
                                {
                                    GameObject go = PoolManager.Instance.SpawnObject(PoolManager.Instance.OtherPrefab);

                                    if(go.TryGetComponent(out Entity entity))
                                    {
                                        entity.Initialized(maxBullet, reloadBullet, bullet, uniqueId);

                                        Units.Add(uniqueId, entity);
                                    }
                                }
                                else
                                {
                                    Units[uniqueId].Initialized(maxBullet, reloadBullet, bullet, uniqueId);
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                Debug.LogError("Error: " + ex.Message);
                return;
            }
        }
    }

    // Server에 검증 요청
    public async void SendRequest(byte[] jsonData)
    {
        await stream.WriteAsync(jsonData, 0, jsonData.Length);

        byte[] responseData = new byte[256];
        int bytes = await stream.ReadAsync(responseData, 0, responseData.Length);
        string responseJson = Encoding.ASCII.GetString(responseData, 0, bytes);
        var response = JsonUtility.FromJson<ResponseData>(responseJson);


        if (response.IsValid)
        {
            ReceiveRequest();
        }
        else
        {
            Debug.LogError("Jump not validated by server.");
        }
    }

    public async void ReceiveRequest()
    {

    }

    private void PerformJump()
    {
        // 점프 동작을 수행하는 로직
        Debug.Log("Jumping!");
        // 실제 점프 로직 (예: Rigidbody를 사용하여 점프하기)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
        }
    }

    // 내 현재 IP 가져오기
    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    // 애플리케이션 종료 시 모두 Close
    private void OnApplicationQuit()
    {
        client?.Close();
        stream?.Close();
    }
}

[Serializable]
public class ResponseData
{
    public bool IsValid;
}
