using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketServerDemo
{
    class Program
    {
        static Socket serverSocket;
        static void Main(string[] args)
        {
            //定义接收数据长度变量            
            int recv;
            //定义接收数据的缓存            
            byte[] data = new byte[1024];
            //定义侦听端口 
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEnd = new IPEndPoint(ipAddress, 5566);
            //定义套接字类型           
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //连接            
            serverSocket.Bind(ipEnd);
            //开始侦听            
            serverSocket.Listen(10);
            //控制台输出侦听状态           
            Console.Write("Waiting for a client");
            //Socket client;            
            while (true){
                //一旦接受连接，创建一个客户端                   
                Socket client = serverSocket.Accept();

                string recvStr = "";
                byte[] recvBytes = new byte[1024];
                int bytes;
                bytes = client.Receive(recvBytes, recvBytes.Length, 0);//从客户端接受信息 
                recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                //获取客户端的IP和端口                    
                IPEndPoint ipEndClient = (IPEndPoint)client.RemoteEndPoint;
                //输出客户端的IP和端口                   
                Console.WriteLine("Connect with {0} at port {1}", ipEndClient.Address, ipEndClient.Port);
                Console.WriteLine("Receive AutoID is {0}",recvStr);
                client.Close();
            }                    
            serverSocket.Close();
        }
    }
}
