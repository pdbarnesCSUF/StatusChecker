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

        //static int portGUIs = 13002;            //for later
        //static UdpClient udpGUIs = null;        //for later
        //static IPEndPoint remoteIpGUIs = null;  //for later
        //need a container for clients
        static MessageServerGUI_Clients clients = new MessageServerGUI_Clients(MessageTypes.MSG_NEW); //has a list inside
        
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
                    MessageClientServer_Client msg = (MessageClientServer_Client) Serializer.Deserialize<MessageClientServer_Client>(ms);
                    // Uses the IPEndPoint object to determine which of these two hosts responded.
                    //msg received
                    //check serial against serials we have
                    //http://stackoverflow.com/questions/9854917/how-can-i-find-a-specific-element-in-a-listt
                    //TESTEST
                    clients.client_list.Add(new ClientStatus(msg));


                    Console.WriteLine("done");
                    //END
                    int idx = clients.client_list.FindIndex(x => x.machine_serial == msg.machine_serial);
                    if (idx < 0) //if new
                    {
                        clients.client_list.Add(new ClientStatus(msg));
                        if (msg.msgtype == MessageTypes.MSG_NEW)
                            Console.WriteLine("New client - Started");
                        else if (msg.msgtype == MessageTypes.MSG_UPDATEPUSH)
                            Console.WriteLine("New client - reported");
                        else
                            Console.WriteLine("New client - ERRORS!!" + msg.msgtype);
                    }
                    else //if update
                    {
                        ulong missed = clients.client_list[idx].Update(msg);
                        if (msg.msgtype == MessageTypes.MSG_UPDATEPUSH)
                            Console.Write("Known client - reported");
                        else if (msg.msgtype == MessageTypes.MSG_NEW)
                            Console.Write("Known client - Started");
                        else
                            Console.Write("Known client - ERRORS!!" + msg.msgtype);
                        //report errors
                        if (missed == 0)
                            Console.WriteLine(" Lost:" + missed);
                        else
                            Console.WriteLine();
                    }

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
