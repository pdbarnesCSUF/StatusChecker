//https://msdn.microsoft.com/en-us/library/system.net.sockets.tcpclient(v=vs.110).aspx
//https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient(v=vs.110).aspx

using ProtoBuf;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ConsoleNet1
{
    class Program_Client
    {
        //actual options, parse from file or defaults
        static string label = "potatoe client";
        static uint send_frequency = 1; //in seconds
        static int port = 11001; //different because on same computer
        //just globals
        static int status = 1;
        const uint client_version = 1;
        static uint messagesGenerated = 0;
        static uint MessageCount()
        {
            ++messagesGenerated;
            return messagesGenerated;
        }
        //need a options struct (might as well be struct to send to the server

        //chose UDP since... many computer trashing hte network for a non-critical function...
        //...might as well be lightweight
        //for reliability, red flag if missing several checkins, not just one
        static MessageStruct GetReport()
        {
            MessageStruct msg = new MessageStruct();
            try
            {
                //======get message info
                msg.label = label;
                msg.status = status; //TODO
                msg.msg_number = MessageCount();
                msg.time_stamp = DateTime.Now;
                msg.ping = 999; //TODO
                msg.client_version = client_version;
                msg.send_frequency = send_frequency;
                //======get general info

                //======get network info
                //http://stackoverflow.com/questions/850650/reliable-method-to-get-machines-mac-address-in-c-sharp#7661829
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                msg.nics = new NetworkInterfaceSlim[nics.Length];
                for(int i = 0; i < nics.Length; ++i)
                {
                    msg.nics[i].name = nics[i].Name;
                    msg.nics[i].status = nics[i].OperationalStatus;
                    msg.nics[i].speed = nics[i].Speed;
                    msg.nics[i].mac_address = nics[i].GetPhysicalAddress();
                    //ip complciated because multiple addresses possible
                    foreach (var x in nics[i].GetIPProperties().UnicastAddresses)
                    {
                        if (x.Address.AddressFamily == AddressFamily.InterNetwork && x.IsDnsEligible)
                        {
                            msg.nics[i].ip = x.Address;
                        }
                    }
                    //msg.nics[i].type = nics[i].GetType();
                }
                //===IP address
                //done! :D
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                msg.status = 0; //TODO ERROR status
            }
            return msg;
        }
        //UDP data
        static void SendUDP(string address, int srv_port, MessageStruct msg)
        {
            // This constructor arbitrarily assigns the local port number.
            UdpClient udpClient = new UdpClient(port);
            try
            {
                Console.Write("Connecting...");
                udpClient.Connect(address, srv_port);
                Console.WriteLine("gonna send:" + msg.label + ";" + msg.os_name);
                // Sends a message to the host to which you have connected.
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize(ms, msg);
                Byte[] sendBytes = ms.ToArray();
                udpClient.Send(sendBytes, sendBytes.Length);

                // Sends a message to a different host using optional hostname and port parameters.
                //UdpClient udpClientB = new UdpClient();
                //udpClientB.Send(sendBytes, sendBytes.Length, "AlternateHostMachineName", 11000);

                //IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(address), 0);

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                // Uses the IPEndPoint object to determine which of these two hosts responded.
                Console.WriteLine("This is the message you received " +
                                             returnData.ToString());
                Console.WriteLine("This message was sent from " +
                                            RemoteIpEndPoint.Address.ToString() +
                                            " on their port number " +
                                            RemoteIpEndPoint.Port.ToString());

                udpClient.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("Could not connect to server (" + address + ":" + srv_port + ")");
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void Main(string[] args)
        {
            MessageStruct data = new MessageStruct();
            data.label = "patatoe";
            Console.Write("Message:");
            data.os_name = Console.ReadLine();
            SendUDP("127.0.0.1", 13000, data);
        }
    }
}
