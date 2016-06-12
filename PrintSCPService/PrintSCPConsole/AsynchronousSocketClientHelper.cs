using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PrintSystem.Common;

namespace Dicom.Printing
{
    #region Socket Client Side Code
    public class ClientStateObject
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

    public class AsynchronousClient
    {    
        // ManualResetEvent instances signal completion.     
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        // The response from the remote device.     
        private static String response = String.Empty;
        public static Dicom.Log.Logger Log { get; private set; }

        public AsynchronousClient(Dicom.Log.Logger log)
        {
            Log = log;
        }

        public  void StartClient(string autoID,IPAddress callingIPAddress, string callingAETitle)
        {   
            try
            {
                // Establish the remote endpoint for the socket.    
                List<MonitorClient> monitorList = MonitorClientHelper.GetMonitorsByCallingSide(callingIPAddress, callingAETitle);

                foreach (var monitorItem in monitorList)
                {
                    IPAddress ipAddress = IPAddress.Parse(monitorItem.MonitorIP);
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, monitorItem.MonitorPort);
                    Log.Info("AsynchronousClient Start to connected to IPAddress:{0} and IPPort:{1}", ipAddress.ToString(),monitorItem.MonitorPort.ToString());
                    // Create a TCP/IP socket.     
                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    // Connect to the remote endpoint.     
                    client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                    connectDone.WaitOne();
                    // Send  data to the remote device.     
                    Send(client, autoID);
                    sendDone.WaitOne();
                    sendDone.Reset();
                    // Release the socket.     
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error("StartClient Fail: {0}",e.ToString());
            }
        }
        private  void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket client = (Socket)ar.AsyncState;
                // Complete the connection.     
                client.EndConnect(ar);
                //Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                Log.Info("Socket connected to {0}", client.RemoteEndPoint.ToString());
                // Signal that the connection has been made.     
                connectDone.Set();
            }
            catch (Exception e)
            {
                Log.Error("Connect to Client FAIL: {0}", e.ToString());
            }
        }

        private  void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.     
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            Log.Info("Sent {0} to server", data);
            // Begin sending the data to the remote device.     
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }
        private  void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket client = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.     
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                Log.Info("Sent {0} bytes to server", bytesSent);
                // Signal that all bytes have been sent.     
                sendDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                Log.Error("Send to Client FAIL: {0}", e.ToString());
            }
        }
    }
    #endregion
}
