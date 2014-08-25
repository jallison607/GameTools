using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameTools.Networking
{
    public class ClientSocket
    {
        private int _id;
        public Socket _clientSocket;
        public byte[] _localBuffer;

        public ClientSocket(int tmpID, Socket tmpSocket)
        {
            this._id = tmpID;
            this._clientSocket = tmpSocket;
        }

        public int GetID()
        {
            return this._id;
        }

    }

    class TCPSocketServer
    {
        private byte[] _buffer;
        private Socket _serverSocket;
        private Dictionary<int, ClientSocket> _clientSockets = new Dictionary<int, ClientSocket>();
        private List<int> _availableIDs = new List<int>();
        private object _availableIDsBaton = new object();
        private IPAddress _serverIP;
        private int _serverPort;
        private IPEndPoint _serverEndPoint;
        private const int socketQueSize = 10;
        private ManualResetEvent allDone = new ManualResetEvent(false);

        public TCPSocketServer(string tmpServerIP, int tmpServerPort, int maxID)
        {
            lock (_availableIDsBaton)
            {
                _availableIDs.AddRange(Enumerable.Range(0, maxID));
            }
            this._buffer = new byte[1024];
            this._serverIP = IPAddress.Parse(tmpServerIP);
            this._serverPort = tmpServerPort;
            this._serverEndPoint = new IPEndPoint(this._serverIP, this._serverPort);
            this._serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._serverSocket.Bind(this._serverEndPoint);
        }

        public void StartTCPServer()
        {
            try
            {
                this._serverSocket.Listen(socketQueSize);
                while (true)
                {
                    allDone.Reset();

                    this._serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);

                    allDone.WaitOne();
                }
                //    Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        //#############Accept and receive messages

        private void AcceptCallBack(IAsyncResult ar)
        {
            allDone.Set();
            try
            {
                Socket tmpSocket = _serverSocket.EndAccept(ar);
                ClientSocket tmpClientSocket = null;
                bool created = false;

                lock (_availableIDsBaton)
                {
                    if (this._availableIDs.Count > 0)
                    {
                        tmpClientSocket = new ClientSocket(NextIDAvailable(), tmpSocket);
                        tmpClientSocket._localBuffer = new Byte[tmpSocket.ReceiveBufferSize];
                        this.newUserConnected(tmpClientSocket);
                        created = true;
                    }

                }

                if (created)
                {
                    tmpClientSocket._clientSocket.BeginReceive(tmpClientSocket._localBuffer, 0, tmpClientSocket._localBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), tmpClientSocket);
                }
                else
                {
                    disconnectSocket(tmpSocket);
                }


            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ReceiveCallBack(IAsyncResult AR)
        {
            try
            {
                Console.WriteLine("Receving Data");
                ClientSocket tmpClientSocket = (ClientSocket)AR.AsyncState;
                int receivedBytes = tmpClientSocket._clientSocket.EndReceive(AR);
                string tmpResult = String.Empty;

                byte[] tmpBuffer = new byte[1024];
                if (receivedBytes == 0)
                {
                    Console.WriteLine("Nothing received");
                }
                else
                {
                    
                    tmpBuffer = tmpClientSocket._localBuffer;
                    Array.Resize(ref tmpBuffer, receivedBytes);
                    tmpResult = Encoding.ASCII.GetString(tmpBuffer);
                    Console.WriteLine(tmpClientSocket._clientSocket.RemoteEndPoint.ToString() + " sent Data: " + tmpResult);
                    
                }


                //this._clientSockets[tmpClientSocket.GetID()]._clientSocket.Send(Encoding.ASCII.GetBytes("Reply from server at:" + System.DateTime.Now));
                tmpClientSocket._clientSocket.BeginReceive(tmpClientSocket._localBuffer, 0, tmpClientSocket._localBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), tmpClientSocket);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        //#################Functional Methods
        //Disconnect user
        public void disconnectUser(int tmpID)
        {
            ClientSocket tmpClientSocket;
            if(_clientSockets.TryGetValue(tmpID,out tmpClientSocket))
            {
                removeUser(tmpID);
                disconnectSocket(tmpClientSocket._clientSocket);
            }
        }

        private void disconnectSocket(Socket tmpSocket)
        {
            tmpSocket.Disconnect(false);
            tmpSocket.Dispose();
         
        }

        //Add new user to client socket list
        private void newUserConnected(ClientSocket tmpClientSocket)
        {
            this._clientSockets.Add(tmpClientSocket.GetID(),tmpClientSocket);
            this._availableIDs.Remove(tmpClientSocket.GetID());
        }

        //Remove specific user
        private void removeUser(int tmpClientToGo)
        {
            lock (_availableIDsBaton)
            {
                _clientSockets.Remove(tmpClientToGo);
                _availableIDs.Add(tmpClientToGo);
            }
        }

        //Return next available ID
        private int NextIDAvailable()
        {
            return _availableIDs.First();
        }



    }
}
