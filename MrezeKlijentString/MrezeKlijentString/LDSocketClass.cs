using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LDSocket
{
    class SocketClient
    {
        /*
         * Coded by Darko Lukic
         * 17:36 24. 2. 2013.
         * 
         * Reference: http://www.codeguru.com/csharp/csharp/cs_misc/sampleprograms/article.php/c7695/Asynchronous-Socket-Programming-in-C-Part-I.htm
         */



        /*
           Invoke(new MethodInvoker(
                delegate
                {
                }
            ));
         */


        public string ip;
        private int port;
        public string breakChar;
        public bool connected = false;

        private Socket socket;
        private AsyncCallback callBack;
        private IAsyncResult result;
        private string receivedData = "";

        public delegate void DataReceivedHandler(string data);
        public event DataReceivedHandler DataReceived;

        public class SocketPacket
        {
            public Socket socket;
            public byte[] dataBuffer = new byte[1];
        }

        public SocketClient(string _ip, int _port = 8000, string _breakChar = "$")
        {
            ip = _ip;
            port = _port;
            breakChar = _breakChar;
        }

        public bool connect()
        {
            if (connected)
                disconnect();

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

                callBack = new AsyncCallback(OnByteReceived);

                if (socket.Connected)
                    WaitForData();

                return true;
                connected = true;
            }
            catch { return false; connected = false; }
        }

        public void disconnect()
        {
            socket.Dispose();
            receivedData = "";
            connected = false;
        }

        private void WaitForData()
        {
            try
            {
                SocketPacket socketPacket = new SocketPacket();
                socketPacket.socket = socket;

                result = socket.BeginReceive(socketPacket.dataBuffer, 0, socketPacket.dataBuffer.Length, SocketFlags.None, callBack, socketPacket);
            }
            catch { }
        }


        private void OnByteReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket socketPacket = (SocketPacket)asyn.AsyncState;
                string receivedChar = Convert.ToChar(socketPacket.dataBuffer[0]).ToString();

                if (receivedChar == breakChar)
                {
                    DataReceived(receivedData);
                    receivedData = "";
                }
                else receivedData += receivedChar;


                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                //connection lost
            }
        }

        public void sendMessage(string data)
        {
            try
            {
                socket.Send(System.Text.Encoding.ASCII.GetBytes(data + breakChar));
            }
            catch { }
        }
    }





    /*
     * Coded by Darko Lukic
     * 17:01 24. 2. 2013.
     * 
     * Reference: http://www.codeguru.com/csharp/csharp/cs_misc/sampleprograms/article.php/c7695/Asynchronous-Socket-Programming-in-C-Part-I.htm
     */

    class SocketServer
    {
        public int port;
        public bool active = false;
        public string breakChar;

        public int count { get { return sockets.Count; } }

        private AsyncCallback clientCallBack;
        private Socket mainSocket;
        private List<Socket> sockets = new List<Socket>();
        private List<string> socketsData = new List<string>();


        public delegate void DataReceivedHandler(string data, int index);
        public event DataReceivedHandler DataReceived;

        public delegate void ClientConnectinChangedHandler(bool state, int index);
        public event ClientConnectinChangedHandler ClientConnectionChanged;


        public SocketServer(int _port = 8000, string _breakChar = "$")
        {
            port = _port;
            breakChar = _breakChar;

            start();
        }

        public void start()
        {
            if (active)
                stop();

            try
            {
                mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mainSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                mainSocket.Listen(4);
                mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);

                clientCallBack = new AsyncCallback(OnByteReceived);

                active = true;
            }
            catch { active = false; }
        }

        public void stop()
        {
            mainSocket.Close();
            sockets.Clear();
            socketsData.Clear();

            active = false;
        }

        private void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                sockets.Add(mainSocket.EndAccept(asyn));
                socketsData.Add("");

                try { ClientConnectionChanged(true, sockets.Count - 1); }
                catch { }

                WaitForData(sockets[sockets.Count - 1], sockets.Count - 1);

                mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (ObjectDisposedException)
            {
                try
                {
                    ClientConnectionChanged(false, sockets.Count - 1);
                }
                catch { }
            }
        }

        private void WaitForData(Socket socket, int index)
        {
            try
            {
                SocketPacket socketPacket = new SocketPacket();
                socketPacket.socket = socket;
                socketPacket.index = index;

                socket.BeginReceive(socketPacket.dataBuffer, 0, socketPacket.dataBuffer.Length, SocketFlags.None, clientCallBack, socketPacket);
            }
            catch
            {
                try
                {
                    ClientConnectionChanged(false, index);
                }
                catch { }
            }
        }

        private class SocketPacket
        {
            public int index;
            public Socket socket;
            public byte[] dataBuffer = new byte[1];
        }


        private void OnByteReceived(IAsyncResult asyn)
        {
            SocketPacket socketPacket = (SocketPacket)asyn.AsyncState;

            try
            {
                string receivedChar = Convert.ToChar(socketPacket.dataBuffer[0]).ToString();

                if (receivedChar == breakChar)
                {
                    DataReceived(socketsData[socketPacket.index], socketPacket.index);

                    socketsData[socketPacket.index] = "";
                }
                else socketsData[socketPacket.index] += receivedChar;


                WaitForData(socketPacket.socket, socketPacket.index);
            }
            catch (ObjectDisposedException)
            {
                try
                {
                    ClientConnectionChanged(false, socketPacket.index);
                }
                catch { }
            }
        }


        public void sendMessage(string data, int index = -1)
        {
            try
            {
                if (index == -1)
                {
                    foreach (Socket socket in sockets)
                    {
                        try
                        {
                            socket.Send(System.Text.Encoding.ASCII.GetBytes(data + breakChar));
                        }
                        catch { }
                    }
                }

                else
                {
                    try
                    {
                        sockets[index].Send(System.Text.Encoding.ASCII.GetBytes(data + breakChar));
                    }
                    catch { }
                }
            }

            catch { }
        }
    }
}