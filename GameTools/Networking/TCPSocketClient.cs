using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

//################Dependencies!!
using GameTools.Basic; //For the logger



namespace GameTools.Networking
{
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }


    class TCPSocketClient
    {
        //Port of remote device
        private int _ServerPort = 10000;
        private IPAddress _ServerIP;
        private IPEndPoint _ServerEndPoint;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent _connectDone =
            new ManualResetEvent(false);
        private ManualResetEvent _sendDone =
            new ManualResetEvent(false);
        private ManualResetEvent _receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private string _response = String.Empty;
        private string _ServerResponse = String.Empty;
        private object stringBaton = new object();

        private Socket _client;
        private BasicLogger basicLoger;

        public TCPSocketClient(string tmpIP, int tmpPort)
        {
            this.basicLoger = new BasicLogger("TCPSocketClient.log");
            this._ServerIP = IPAddress.Parse(tmpIP);
            this._ServerPort = tmpPort;
            this._ServerEndPoint = new IPEndPoint(_ServerIP, _ServerPort);
            this._client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool StartClient(string serializedData)
        {
            bool connected = false;
            lock (this.stringBaton)
            {
                this._response = string.Empty;
                this._ServerResponse = string.Empty;
            }
            // Connect to a remote device.
            try
            {
                // Connect to the remote endpoint.
                _client.BeginConnect(this._ServerEndPoint,
                    new AsyncCallback(ConnectCallback), this._client);
                this._connectDone.WaitOne();

                // Send test data to the remote device.
                Send(this._client, serializedData);
                this._sendDone.WaitOne();
                this._sendDone.Set();

                // Write the response to the console.
                connected = true;
                
            }
            catch (Exception e)
            {
                this.basicLoger.Log(e.ToString());
            }

            return connected;
        }

        //##################Connection Methods
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                // Signal that the connection has been made.
                this._connectDone.Set();
            }
            catch (Exception e)
            {
                this.basicLoger.Log(e.ToString());
            }
        }

        //##################Disconnection Methods
        public bool DisconnectFromServer()
        {
            bool disconnected = false;
            try
            {
                Send(this._client, "*exit*");
                this._client.Shutdown(SocketShutdown.Both);
                this._client.BeginDisconnect(true, new AsyncCallback(DisconnectCallBack), this._client);
                disconnected = true;
            }
            catch (Exception ex)
            {
                this.basicLoger.Log(ex.Message);
            }
            return disconnected;

        }

        private void DisconnectCallBack(IAsyncResult ar)
        {

                // Complete the disconnect request.
                Socket client = (Socket)ar.AsyncState;
                client.EndDisconnect(ar);

        }

        //##################Send Methods
        public bool SendToServer(string serializedData)
        {
            this._sendDone.Set();
            this._ServerResponse = String.Empty;
            Send(this._client ,serializedData);
            this._sendDone.WaitOne();

            return true;
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);

                // Signal that all bytes have been sent.
                this._sendDone.Set();
            }
            catch (Exception e)
            {
                this.basicLoger.Log(e.ToString());
            }
        }

        //##################Receive Methods
        public string ReceiveResult()
        {
            lock (this.stringBaton)
            {
                this._receiveDone.Set();
                Receive(this._client);
                this._receiveDone.WaitOne();
                this._ServerResponse = this._response;
                this._response = string.Empty;
            
            }

            return _ServerResponse;

        }
        
        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;
                //this.basicLoger.Log("Waiting for response");
                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                string tmpResult = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                state.sb.Append(tmpResult);

                if (state.sb.Length > 1)
                {
                    this._response = state.sb.ToString();
                }

                // Signal that all bytes have been received.
                this._receiveDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        //#####################Other Methods

    }
}
