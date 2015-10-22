using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace elf
{
    public class StateObject
    {
        public Socket workSocket = null;

        public const int BufferSize = 1024;

        public byte[] buffer = new byte[BufferSize];
    }

    public class SocketServer
    {
        private IPEndPoint localEndPoint;
        private Socket mySocket;
        private Socket handler;
        private static ManualResetEvent myReset = new ManualResetEvent(false);

        public delegate void SocketServerReceiveHandler(byte[] ReceiveData, String IPS);
        public delegate void SocketServerErrorEventHandler(string ErrCode, string ErrorDiscription);
        public delegate void ActiveClients(string[] IPS, int ClientCount);

        private string mIP;
        private int mPort;

        Dictionary<string, Socket> ClientSockets = new Dictionary<string, Socket>();

        Dictionary<string, System.Timers.Timer> ClientTimers = new Dictionary<string, System.Timers.Timer>();
        System.Timers.Timer ClientHeartBeatTimer;

        /// <summary>
        /// 返回活动的客户端地址、客户端数量的事件
        /// </summary>
        public event ActiveClients OnActiveClient;

        /// <summary>
        /// 接收数据事件
        /// </summary>
        public event SocketServerReceiveHandler OnReiceive;

        /// <summary>
        /// 弹出错误事件
        /// </summary>
        public event SocketServerErrorEventHandler OnError;

        /// <summary>
        /// </summary>
        public SocketServer(Int16 Port)
        {
            try
            {
                mPort = Port;
                mIP = System.Net.Dns.Resolve(System.Environment.MachineName).AddressList[0].ToString();
                localEndPoint = new IPEndPoint(System.Net.Dns.Resolve(System.Environment.MachineName).AddressList[0], Port);
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mySocket.Bind(localEndPoint);
                mySocket.Listen(10000);

                Thread thread = new Thread(new ThreadStart(target));
                thread.Start();
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[SocketServer]", ex.Message);
                }
            }
        }

        private void target()
        {
            try
            {
                while (true)
                {
                    myReset.Reset();
                    mySocket.BeginAccept(new AsyncCallback(AcceptCallback), mySocket);
                    myReset.WaitOne();
                }
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[target]", ex.Message);
                }
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                myReset.Set();
                Socket listener = (Socket)ar.AsyncState;
                handler = listener.EndAccept(ar);

                StateObject state = new StateObject();
                state.workSocket = handler;
            }
            catch (Exception ex1)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex1.HResult.ToString() + "[AcceptCallback]", ex1.Message);
                }
            }

            try
            {
                byte[] byteData = System.Text.Encoding.ASCII.GetBytes("Server is ready......");
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);

                if (!ClientSockets.ContainsKey(handler.RemoteEndPoint.ToString()))
                {
                    string IPS = handler.RemoteEndPoint.ToString();

                    ClientSockets.Add(IPS, handler);

                    ClientHeartBeatTimer = new System.Timers.Timer(60000);//一分钟收不到客户端发来数据，就从ClientSockets集合里剔除对应客户端的连接
                    ClientHeartBeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(ClientsHeartBeatTimeOut);//到达时间时执行的事件
                    ClientHeartBeatTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true);
                    ClientHeartBeatTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件

                    ClientTimers.Add(IPS, ClientHeartBeatTimer);

                    //弹出活动客户端的IP、活动客户端的数量
                    string[] clientsIPS = new string[ClientSockets.Count];
                    ClientSockets.Keys.CopyTo(clientsIPS, 0);
                    OnActiveClient(clientsIPS, ClientSockets.Count);
                }
            }
            catch (Exception ex2)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex2.HResult.ToString() + "[AcceptCallback]", ex2.Message);
                }
            }

            Thread thread = new Thread(new ThreadStart(begReceive));
            thread.Start();
        }

        private void ClientsHeartBeatTimeOut(object sender, ElapsedEventArgs e)
        {
            try
            {
                string KeyString = "";
                foreach (var T in ClientTimers)
                {
                    if (sender.Equals(T.Value))
                    {
                        KeyString = T.Key;
                        break;
                    }
                }
                if (ClientSockets.ContainsKey(KeyString))
                {
                    ClientSockets[KeyString].Shutdown(SocketShutdown.Both);
                    ClientSockets[KeyString].Close();
                    ClientSockets[KeyString].Dispose();
                    ClientSockets.Remove(KeyString);

                    ClientTimers[KeyString].Close();
                    ClientTimers.Remove(KeyString);

                    //弹出活动客户端的IP、活动客户端的数量
                    string[] clientsIPS = new string[ClientSockets.Count];
                    ClientSockets.Keys.CopyTo(clientsIPS, 0);
                    OnActiveClient(clientsIPS, ClientSockets.Count);
                }
            }
            catch (Exception ex1)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex1.HResult.ToString() + "[ClientsHeartBeatTimeOut]", ex1.Message);
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[SendCallback]", ex.Message);
                }
            }
        }

        private void begReceive()
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[begReceive]", ex.Message);
                }
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket ReadSocket = state.workSocket;

                if (ReadSocket != null && ReadSocket.Connected)
                {
                    string IP = (ReadSocket.RemoteEndPoint as IPEndPoint).Address.ToString();
                    int Port = (ReadSocket.RemoteEndPoint as IPEndPoint).Port;
                    string IPS = IP + ":" + Port.ToString();
                    int bytesRead = ReadSocket.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        byte[] ReadData = new byte[bytesRead];
                        Array.Copy(state.buffer, 0, ReadData, 0, bytesRead);

                        //重置计时器
                        ClientTimers[IPS].Enabled = false;
                        ClientTimers[IPS].Enabled = true;

                        string DataString = System.Text.Encoding.ASCII.GetString(ReadData);
                        if (DataString == "Heartbeat......")
                        {
                            SendMessage(ReadData, IP, Port);//向收到发来心跳信号的客户端回发心跳信号
                        }
                        else
                        {
                            OnReiceive(ReadData, IPS);//如果收到的不是心跳，则弹出数据接收事件
                        }

                        ReadSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                }

            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[ReadCallback]", ex.Message);
                }
            }
        }

        /// <summary>
        /// 服务端本地IP
        /// </summary>
        public string myIP
        {
            get
            {
                return mIP;
            }
        }

        /// <summary>
        /// 服务端本地端口
        /// </summary>
        public int myPort
        {
            get
            {
                return mPort;
            }
        }

        /// <summary>
        /// 关闭服务端
        /// </summary>
        public void Close()
        {
            try
            {
                myReset.Close();
                myReset.Dispose();
                foreach (string key in ClientSockets.Keys)
                {
                    ClientSockets[key].Shutdown(SocketShutdown.Both);
                    ClientSockets[key].Close();
                    ClientSockets[key].Dispose();

                    ClientTimers[key].Close();
                    ClientTimers[key].Dispose();
                    ClientTimers.Remove(key);
                }
                ClientTimers.Clear();
                ClientSockets.Clear();
                mySocket.Close();
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[Close]", ex.Message);
                }
            }
        }

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="Msg">需发送的消息</param>
        /// <param name="IP">目标客户端的IP</param>
        /// <param name="Port">目标客户端的端口</param>
        public void SendMessage(byte[] Msg, string IP, int Port)
        {
            try
            {
                string IPS = IP + ":" + Port.ToString();
                if (ClientSockets.ContainsKey(IPS))
                {
                    ClientSockets[IPS].BeginSend(Msg, 0, Msg.Length, 0, new AsyncCallback(SendCallback), ClientSockets[IPS]);
                }
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[SendMessage]", ex.Message);
                }
            }
        }
    }//class SocketServer

    class SocketClient
    {
        

        private IPEndPoint myServer;
        private IPAddress myIP;
        private Socket mySocket;

        private string mServerIP = "";
        private int mServerPort = 0;

        public delegate void ClientReceiveHandler(byte[] ReceiveData);
        public delegate void ClientErrorEventHandler(string ErrCode, string ErrorDiscription);
        private static ManualResetEvent connectReset = new ManualResetEvent(false);
        private static ManualResetEvent sendReset = new ManualResetEvent(false);

        public delegate void socketConnectedEventHandler(bool Connected);
        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event socketConnectedEventHandler OnSocketConnected;

        System.Timers.Timer HeartBeatTimer = new System.Timers.Timer(10000);

        /// <summary>
        /// 接收数据事件
        /// </summary>
        public event ClientReceiveHandler OnReiceive;

        /// <summary>
        /// 弹出错误事件
        /// </summary>
        public event ClientErrorEventHandler OnError;

        public SocketClient()
        {
            HeartBeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(HeartBeatTimeOut);//到达时间时执行的事件
            HeartBeatTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true);
            HeartBeatTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件
        }

        private void HeartBeatTimeOut(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                HeartBeat();
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[HeartBeatTimeOut]", ex.Message);
                }
            }
        }

        /// <summary>
        /// 请求连接服务器
        /// </summary>
        public void RequestConnectServer(string ServerIP, int ServerPort)
        {
            try
            {
                //mServerIP、mServerPort客户端重连服务端时需要的IP和Port
                mServerIP = ServerIP;
                mServerPort = ServerPort;

                myIP = IPAddress.Parse(ServerIP);
                myServer = new IPEndPoint(myIP, ServerPort);
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mySocket.BeginConnect(myServer, new AsyncCallback(ConnectCallback), mySocket);
                if (!connectReset.WaitOne(1000, false))
                {
                    mySocket.Close();
                    throw new SocketException(10060);
                }

            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[RequestConnectServer]", ex.Message);
                }
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket Client = (Socket)ar.AsyncState;
                lock (Client)
                {
                    Client.EndConnect(ar);
                    Thread thread = new Thread(new ThreadStart(target));
                    thread.Start();
                    connectReset.Set();
                }
            }
            catch 
            {
               
            }
        }

        private void target()
        {
            try
            {
                StateObject state = new StateObject();
                lock (state)
                {
                    state.workSocket = mySocket;
                    mySocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[target]", ex.Message);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {

            StateObject state = (StateObject)ar.AsyncState;
            lock (state)
            {
                Socket client = state.workSocket;

                try
                {
                    int bytesRead = client.EndReceive(ar);

                    if (bytesRead > 0)
                    {

                        OnSocketConnected(true);

                        //HeartBeatTimer.Enabled = false;

                        //HeartBeatTimer.Enabled = true;

                        byte[] ReadData = new byte[bytesRead];
                        Array.Copy(state.buffer, 0, ReadData, 0, bytesRead);

                        string DataString = System.Text.Encoding.ASCII.GetString(ReadData);
                        if (DataString != "Heartbeat......")
                        {
                            OnReiceive(ReadData);
                        }
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }

                catch (Exception ex)
                {
                    //弹出相关错误事件
                    if (OnError != null)
                    {
                        OnError(ex.HResult.ToString() + "[ReceiveCallback]", ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 关闭与服务器的连接
        /// </summary>
        public void Close()
        {
            try
            {
                HeartBeatTimer.Enabled = false;
                HeartBeatTimer.Stop();
                HeartBeatTimer.Close();
                HeartBeatTimer.Dispose();

                mySocket.Shutdown(SocketShutdown.Both);
                mySocket.Close();
                mySocket.Disconnect(true);
                mySocket.Dispose();
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[Close]", ex.Message);
                }
            }
        }

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="Msg">需发送的消息</param>
        public void SendMessage(byte[] Msg)
        {
            try
            {
                mySocket.BeginSend(Msg, 0, Msg.Length, 0, new AsyncCallback(SendCallback), mySocket);
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[SendMessage]", ex.Message);
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                sendReset.Set();
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[SendCallback]", ex.Message);
                }
            }
        }

        /// <summary>
        /// 检查客户端是否与服务器连接
        /// </summary>
        public bool Poll()
        {
            try
            {
                bool a = false;
                a = mySocket.Connected;
                return a;
            }
            catch (Exception ex)
            {
                //弹出相关错误事件
                if (OnError != null)
                {
                    OnError(ex.HResult.ToString() + "[Poll]", ex.Message);
                }
                return false;
            }
        }

        /// <summary>
        /// 向服务器发送心跳数据帧
        /// </summary>
        private void HeartBeat()
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Heartbeat......");
            SendMessage(bytes);
        }

        /// <summary>
        /// 心跳使能
        /// </summary>
        /// <param name="Enable">true：发送心跳检测帧；false：不发送心跳检测帧</param>
        public void HeartBeatTimerEnabled(bool bEnable)
        {
            HeartBeatTimer.Enabled = bEnable;
        }

    }//class SocketClient
}
