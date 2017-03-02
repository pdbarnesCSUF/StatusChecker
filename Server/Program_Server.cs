//https://msdn.microsoft.com/en-us/library/system.net.sockets.tcplistener(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-2
//https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient(v=vs.110).aspx

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program_Server
    {
        static int port = 13000;
        static UdpClient udpClient = null;
        static IPEndPoint RemoteIpEndPoint = null;
        //UDP string
        static void StartServer()
        {
            StartServer(port);
        }
        static void StartServer(int srv_port)
        {
            udpClient = new UdpClient(srv_port);
            //IPEndPoint object will allow us to read datagrams sent from any source. ish
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
        }
        static void Main(string[] args)
        {
            try
            {
                StartServer();
                // Blocks until a message returns on this socket from a remote host.
                Console.WriteLine("Waiting for messages...");
                while (true)
                {
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    //string receiveData = Encoding.ASCII.GetString(receiveBytes);
                    // Sends a message to the host to which you have connected.
                    
                    MemoryStream ms = new MemoryStream(receiveBytes);
                    MessageStruct msg = (MessageStruct) Serializer.Deserialize<MessageStruct>(ms);

                    // Uses the IPEndPoint object to determine which of these two hosts responded.
                    Console.WriteLine(msg.label + " said " +
                                                 msg.os_name);
                    Console.WriteLine("This message was sent from " +
                                                RemoteIpEndPoint.Address.ToString() +
                                                " on their port number " +
                                                RemoteIpEndPoint.Port.ToString());
                    //send back
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("Hi " +
                                                RemoteIpEndPoint.Address.ToString() +
                                                " Got your messsage!");
                    udpClient.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                udpClient.Close();
            }
        }//end of main
    }
}
