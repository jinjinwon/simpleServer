using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace SimpleServer
{
    internal class Server
    {
        static List<TcpClient> clients = new List<TcpClient>();
        static TcpListener server = null;

        static void Main(string[] args)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 8888);
                server.Start();
                Console.WriteLine("Server started...");

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = server.AcceptTcpClient();

                    ClientState clientState = new ClientState
                    {
                        Client = client,
                        Unique = Guid.NewGuid(),
                        Bullet = 30, // 초기 탄약 수 설정
                        MaxBullet = 900,
                        ReloadBullet = 30,
                    };
                    clients.Add(client);
                    Console.WriteLine("New client connected!");

                    SendInitialState(clientState);

                    // 새로운 클라이언트 연결 알림을 보내기 전에 잠시 대기
                    Thread.Sleep(100); // 100ms 지연

                    // 새로운 클라이언트 연결 알림
                    BroadcastNewClientConnected(clientState);

                    Thread clientThread = new Thread(HandleClient);
                    clientThread.Start(clientState);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                }
            }
        }

        static void HandleClient(object obj)
        {
            ClientState clientState = (ClientState)obj;
            TcpClient client = clientState.Client;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + message);

                    // 메시지 형식에 따라 처리
                    if (message.StartsWith("MOVE:"))
                    {
                        bool isApproved = ValidateMove(message);
                        string response = isApproved ? "Approved" : "Denied";
                        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);

                        if (isApproved)
                        {
                            BroadcastMessage(message, client);
                        }
                    }
                    else if (message.StartsWith("RELOAD:"))
                    {
                        bool isApproved = ValidateReload(clientState);
                        string response = isApproved ? "Approved" : "Denied";
                        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);

                        if (isApproved)
                        {
                            // 장전이 승인된 경우 탄약 수를 갱신
                            clientState.Bullet += clientState.Reload(); // 예: 최대 탄약 수를 10으로 설정
                            BroadcastMessage($"RELOAD:{clientState.Bullet},{clientState.MaxBullet},{clientState.Unique}", client);
                        }
                    }
                    else if (message.StartsWith("SHOOT:"))
                    {
                        bool isApproved = ValidateShoot(clientState);
                        string response = isApproved ? "Approved" : "Denied";
                        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);

                        if (isApproved)
                        {
                            // 발사가 승인된 경우 탄약 수를 감소
                            clientState.Bullet--;
                            BroadcastMessage($"{message},{clientState.Bullet},{clientState.MaxBullet},{clientState.Unique}", client);
                        }
                    }
                    else if (message.StartsWith("CONNECTED:"))
                    {
                        // 클라이언트가 연결되었을 때 처리
                        SendInitialState(clientState);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                clients.Remove(client);
                client.Close();
            }
        }

        static void SendInitialState(ClientState clientState)
        {
            string initialStateMessage = $"CONNECTED:{clientState.MaxBullet},{clientState.ReloadBullet},{clientState.Unique},{clientState.Bullet}";
            byte[] buffer = Encoding.ASCII.GetBytes(initialStateMessage);
            NetworkStream stream = clientState.Client.GetStream();
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush(); // 추가
            Console.WriteLine("Sent initial state: " + initialStateMessage);
        }

        static void BroadcastMessage(string message, TcpClient sender)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);

            foreach (var client in clients)
            {
                // 조건없이 전부 방송
                NetworkStream stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush(); // 추가
                Console.WriteLine("Broadcast message: " + message);
            }
        }

        static void BroadcastNewClientConnected(ClientState clientState)
        {
            string message = $"NEW_CLIENT:{clientState.Unique},{clientState.MaxBullet},{clientState.ReloadBullet},{clientState.Bullet}";
            BroadcastMessage(message, clientState.Client);
        }

        static bool ValidateMove(string request)
        {
            // 이동 검증 로직 구현
            return true; // 임시로 모든 이동을 승인
        }

        static bool ValidateReload(ClientState request)
        {
            return request.IsReload();
        }

        static bool ValidateShoot(ClientState request)
        {
            return request.IsShoot();
        }
    }

    public class ClientState
    {
        public TcpClient Client { get; set; }
        public Guid Unique { get; set; }
        public int Bullet { get; set; }
        public int MaxBullet { get; set; }

        public int ReloadBullet { get; set; }

        public bool IsReload()
        {
            // 내 탄창에 총알이 없는 경우
            if (MaxBullet <= 0)
            {
                return false;
            }

            // 내 탄창에 총알이 장전 할 총알의 개수보다 많은 경우
            if (MaxBullet >= ReloadBullet)
            {
                return true;
            }

            // 내 탄창에 총알이 장전 할 총알의 개수보다 적지만 0개는 아닌 경우
            if (MaxBullet <= ReloadBullet)
            {
                return true;
            }
            return false;
        }

        public int Reload()
        {
            MaxBullet -= (ReloadBullet - Bullet);
            return ReloadBullet - Bullet;
        }

        public bool IsShoot()
        {
            return Bullet > 0;
        }
        // 추가로 필요한 상태 정보들을 여기에 추가
    }
}