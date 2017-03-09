//https://msdn.microsoft.com/en-us/library/system.net.sockets.tcplistener(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-2
//https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient(v=vs.110).aspx

using ProtoBuf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program_Server
    {
        static int portClients = 13000;
        static UdpClient udpClients = null;
        static IPEndPoint remoteIpClients = null;

        static int portGUIs = 13002;            //for later
        static UdpClient udpGUIs = null;        //for later
        static IPEndPoint remoteIpGUIs = null;  //for later
        //need a container for clients
        static MessageServerGUI_Clients clients;
        
        //need a container for GUI
        //identifier struct
        //how often to push for updates
        //want realtime updates bool

        //UDP string
        static void StartServer()
        {
            StartServer(portClients);
        }
        static void StartServer(int srv_port)
        {
            udpClients = new UdpClient(srv_port);
            //IPEndPoint object will allow us to read datagrams sent from any source. ish
            remoteIpClients = new IPEndPoint(IPAddress.Any, portClients);
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
                    Byte[] receiveBytes = udpClients.Receive(ref remoteIpClients);
                    // Sends a message to the host to which you have connected.
                    
                    MemoryStream ms = new MemoryStream(receiveBytes);
                    MessagerClientServer_Client msg = (MessagerClientServer_Client) Serializer.Deserialize<MessagerClientServer_Client>(ms);
                    // Uses the IPEndPoint object to determine which of these two hosts responded.
                    //msg received
                    //check serial against serials we have
                    //if new
                        //add to list
                        //output - "new client"
                    //if already have
                        //update report
                        //if msgtype = reportpush
                            //update report_last
                            //update report_received
                            //update report_lost
                            //output - "known client reported"
                        //if msgtype = new
                            //update report_last
                            //reset report_received
                            //reset report_lost
                            //output - "known client started"
                    
                    Console.WriteLine("=====");
                    Console.WriteLine(msg.label + " said " +
                                                 msg.msg);
                    Console.WriteLine("Sent from " +
                                                remoteIpClients.Address.ToString() +
                                                " on their port:" +
                                                remoteIpClients.Port.ToString());
                    //Console.WriteLine("=====objdump=====");
                    //msg.output();
                    //send back
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("Hi " +
                                                remoteIpClients.Address.ToString() +
                                                " Got your messsage!");
                    udpClients.Send(sendBytes, sendBytes.Length, remoteIpClients);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                udpClients.Close();
            }
        }//end of main
    }
}
