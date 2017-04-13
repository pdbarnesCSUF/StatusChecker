//https://msdn.microsoft.com/en-us/library/system.net.sockets.tcplistener(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-2
//https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient(v=vs.110).aspx
//http://stackoverflow.com/questions/177856/how-do-i-trap-ctrl-c-in-a-c-sharp-console-app#929717
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
        static bool serverQuit = false;
        static int portClients = 13000;
        static UdpClient udpClients = null;
        static IPEndPoint remoteIpClients = null;

        //static int portGUIs = 13002;            //for later
        //static UdpClient udpGUIs = null;        //for later
        //static IPEndPoint remoteIpGUIs = null;  //for later
        //need a container for clients
        static System.Object clients_lock = new System.Object(); //threadlocking
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
        static void StopServer()
        {
            udpClients.Close();
        }
        static void RunServer()
        {
            while (!serverQuit)
            {
                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = udpClients.Receive(ref remoteIpClients);
                // Sends a message to the host to which you have connected.

                MemoryStream ms = new MemoryStream(receiveBytes);
                MessageClientServer_Client msg = (MessageClientServer_Client)Serializer.Deserialize<MessageClientServer_Client>(ms);
                // Uses the IPEndPoint object to determine which of these two hosts responded.
                //msg received
                //check serial against serials we have
                //http://stackoverflow.com/questions/9854917/how-can-i-find-a-specific-element-in-a-listt
                lock (clients_lock)
                {
                    int idx = clients.client_list.FindIndex(x => x.machine_serial == msg.machine_serial);
                    Console.WriteLine("=====");
                    //Console.WriteLine("idx:" + idx);
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
                            Console.WriteLine("Known client - reported" + " Lost:" + missed);
                        else if (msg.msgtype == MessageTypes.MSG_NEW)
                            Console.WriteLine("Known client - Started");
                        else
                            Console.WriteLine("Known client - ERRORS!!" + msg.msgtype + " Lost:" + missed);
                    }
                }//lock clients_lock
                Console.WriteLine(msg.label + " said " + msg.msg);
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
            }//!serverquit
        }

        static void Main(string[] args)
        {
            //ctrl+c trapping... doesnt work?
            Console.CancelKeyPress += delegate {
                // call methods to clean up
                serverQuit = true;
            };
            try
            {
                Console.WriteLine("Opening Port...");
                StartServer();
                System.Threading.Thread serverThread = new System.Threading.Thread(RunServer);
                serverThread.IsBackground = false;
                Console.WriteLine("Waiting for messages...");
                serverThread.Start();
                //sit here for exit
                char userinput = 'z';
                while (!serverQuit)
                {
                    Console.WriteLine("press q or CTRL+C to quit");
                    Console.WriteLine("l: list of clients");
                    userinput = Console.ReadKey().KeyChar; //true means dont echo
                    Console.WriteLine();//if not echoing.... comment this out!
                    //MENU HERE ...if ever
                    switch (userinput)
                    {
                        case 'q':
                            serverQuit = true;
                            break;
                        case 'l':
                            clients.display();
                            break;
                    }

                }
                Console.WriteLine("Closing");
                Console.Write("Waiting for port...");
                serverThread.Join(1000); //wait 2 seconds in case it is mid process
                if (serverThread.IsAlive)
                {
                    //more agressive kill, probably has no message processing anyways
                    serverThread.Abort();
                }
                Console.Write("Ready to close...");
                //StopServer(); // inside finally block
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                StopServer();
                Console.WriteLine("Closed");
            }
            clients.display();
        }//end of main
    }
}
