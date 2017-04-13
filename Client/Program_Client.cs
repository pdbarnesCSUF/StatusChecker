//https://msdn.microsoft.com/en-us/library/system.net.sockets.tcpclient(v=vs.110).aspx
//https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient(v=vs.110).aspx
//http://stackoverflow.com/questions/4314630/managementobject-class-not-showing-up-in-system-management-namespace
//https://msdn.microsoft.com/en-us/library/windows/desktop/aa394239(v=vs.85).aspx Win32_OperatingSystem
//https://msdn.microsoft.com/en-us/library/windows/desktop/aa393244(v=vs.85).aspx getting WMI
//https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient.beginreceive(v=vs.110).aspx
//Console.WriteLine(cpus[i]["PercentProcessorTime"].GetType().Name); .GetType().Name to get name of type

using ProtoBuf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleNet1
{
    class Program_Client
    {
        //actual options, parse from file or defaults
        static string label = "potatoe client";
        static uint send_frequency = 1; //in seconds
        static int port = 11001; //different because on same computer
        static IPAddress srv_address = IPAddress.Parse("127.0.0.1");
        static int srv_port = 13000;
        //just globals
        const uint client_version = 1;
        static ulong messagesGenerated = 0;
        static ulong MessageCount()
        {
            ++messagesGenerated;
            return messagesGenerated;
        }
        //TODO make a global "static" as in.. for things that dont change, Message struct to only update whats necessary
        static MessageClientServer_Client msgStatic = new MessageClientServer_Client();
            //==========borrowed code
        //http://stackoverflow.com/questions/105031/how-do-you-get-total-amount-of-ram-the-computer-has#105084
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(this);
            }
        }
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        /* to use
           ulong installedMemory;
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if( GlobalMemoryStatusEx( memStatus))
            { 
               installedMemory = memStatus.ullTotalPhys;
            }
*/
        //============ borrowed END
        //==================================================
        //                Update report
        //==================================================
        static void UpdateReport()
        {
            msgStatic.msgtype = MessageTypes.MSG_UPDATEPUSH;
            msgStatic.msg_number = MessageCount();
            msgStatic.time_stamp = DateTime.Now;
#if !NDEBUG
            Console.WriteLine("#" + msgStatic.msg_number + "  " +msgStatic.time_stamp);
#endif
            //--ping
            Ping pingSender = new Ping();
            PingReply pingReply = pingSender.Send(srv_address);
            if (pingReply.Status == IPStatus.Success)
                msgStatic.ping = pingReply.RoundtripTime;
            else
                msgStatic.ping = -1;
            //---cpus
#if !NDEBUG
            Console.WriteLine("Getting Cpus...");
#endif
            //http://stackoverflow.com/questions/9777661/returning-cpu-usage-in-wmi-using-c-sharp
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
            ManagementObjectCollection cpus_collection = searcher.Get();
            ManagementObject[] cpus = new ManagementObject[cpus_collection.Count];
            cpus_collection.CopyTo(cpus, 0);
            msgStatic.cpus = new ushort[cpus.Count() - 1]; //-1 for "_total"
            for (int i = 0; i < msgStatic.cpus.Length; ++i)
            {
                msgStatic.cpus[i] = (ushort)(ulong)(cpus[i]["PercentProcessorTime"]); //raw is Int64
            }
            //---ram
#if !NDEBUG
            Console.WriteLine("Getting Memory...");
#endif
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                msgStatic.ram_used = memStatus.ullAvailPhys;
                msgStatic.swap_used = memStatus.ullAvailPageFile;
            }
            //---get hdds
#if !NDEBUG
            Console.WriteLine("Getting Drives...");
#endif
            DriveInfo[] drives = DriveInfo.GetDrives();
            msgStatic.drives = new DriveInfoSlim[drives.Length];
            for (int i = 0; i < drives.Length; ++i)
            {
                msgStatic.drives[i].name = drives[i].Name;
                msgStatic.drives[i].type = drives[i].DriveType;
                if (drives[i].IsReady)
                {
                    msgStatic.drives[i].free = drives[i].TotalFreeSpace;
                    msgStatic.drives[i].total = drives[i].TotalSize;
                }
            }
            //---processes
#if !NDEBUG
            Console.WriteLine("Getting Processes...");
#endif
            msgStatic.processes_total = Process.GetProcesses().Length;

            //======get network info
#if !NDEBUG
            Console.WriteLine("Getting NICs...");
#endif
            //http://stackoverflow.com/questions/850650/reliable-method-to-get-machines-mac-address-in-c-sharp#7661829
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            msgStatic.nics = new NetworkInterfaceSlim[nics.Length];
            for (int i = 0; i < nics.Length; ++i)
            {
                msgStatic.nics[i].name = nics[i].Name;
                msgStatic.nics[i].status = nics[i].OperationalStatus;
                msgStatic.nics[i].speed = nics[i].Speed;
                msgStatic.nics[i].mac_address = nics[i].GetPhysicalAddress().ToString(); //PhysicalAddress type wont serialize T_T
                                                                                         //ip complciated because multiple addresses possible
                foreach (var x in nics[i].GetIPProperties().UnicastAddresses)
                {
                    if (x.Address.AddressFamily == AddressFamily.InterNetwork && x.IsDnsEligible)
                    {
                        msgStatic.nics[i].ip = x.Address.ToString();
                    }
                }
                //msg.nics[i].type = nics[i].GetType();
            }
        }
        //==================================================
        //                 NewReport()
        //==================================================
        static void NewReport()
        {
            //msgStatic = new MessageClientServer_Client();
            try
            {
                //======get message info
                msgStatic.label = label;
                //msgStatic.msgtype = MessageTypes.MSG_NEW;
                
                msgStatic.client_version = client_version;

                msgStatic.send_frequency = send_frequency;
                //======get general info
                //---hostname
#if !NDEBUG
                Console.WriteLine("Getting Hostname...");
#endif
                msgStatic.hostname = System.Environment.MachineName;
                //---guid
#if !NDEBUG
                Console.WriteLine("Getting Serial...");
#endif
                //turns out that ITS NOT A GUID!!!!
                //getting this sucks apparently
                //CimInstance myDrive = new CimInstance("Win32_OperatingSystem=@");
                //string serial = (string)os["SerialNumber"];
                //TODO remove WMI... because inefficient
                string serial = (string) (from x in new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                            select x.GetPropertyValue("SerialNumber")).FirstOrDefault();
                msgStatic.machine_serial = (serial != null) ? serial : "Unknown";
                //---os version
#if !NDEBUG
                Console.WriteLine("Getting OSversion...");
#endif
                //ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                //foreach (ManagementObject os in searcher.Get())
                //{
                //    result = os["Caption"].ToString();
                //    break;
                //}
                //TODO remove WMI... because inefficient
                var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                            select x.GetPropertyValue("Caption")).FirstOrDefault();
                msgStatic.os_name = (name != null) ? name.ToString() : "Unknown";
                //---ram
#if !NDEBUG
                Console.WriteLine("Getting Memory (max)...");
#endif
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    msgStatic.ram_total = memStatus.ullTotalPhys;
                    //msgStatic.ram_used = memStatus.ullAvailPhys;
                    msgStatic.swap_total = memStatus.ullTotalPageFile;
                    //msgStatic.swap_used = memStatus.ullAvailPageFile;
                }
                //===IP address
                UpdateReport();
                //done! :D
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                msgStatic.msgtype = MessageTypes.MSG_ERROR; //TODO ERROR status
            }
        }
        //==================================================
        //                  SendUDP(msg)
        //==================================================
        //chose UDP since... many computer trashing hte network for a non-critical function...
        //...might as well be lightweight
        //for reliability, red flag if missing several checkins, not just one
        //UDP data
        static void SendUDP( MessageClientServer_Client msg)
        {
            // This constructor arbitrarily assigns the local port number.
            UdpClient udpClient = new UdpClient(port);
            try
            {
                Console.Write("Connecting...");
                udpClient.Connect(srv_address, srv_port);
                Console.WriteLine("gonna send:" + msg.label + ";" + msg.msg);
                // Sends a message to the host to which you have connected.
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize(ms, msg);
                Byte[] sendBytes = ms.ToArray();
                udpClient.Send(sendBytes, sendBytes.Length);

                // Sends a message to a different host using optional hostname and port parameters.
                //udpClientB.Send(sendBytes, sendBytes.Length, "AlternateHostMachineName", 11000);

                //IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(srv_address, 0);

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
                Console.WriteLine("Could not connect to server (" + srv_address + ":" + srv_port + ")");
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        //==================================================
        //                  Main( )
        //==================================================
        static void Main(string[] args)
        {
            //setup some options...
            srv_address = IPAddress.Parse("127.0.0.1");
            srv_port = 13000;
            label = "testing_client";
            //get report
            NewReport();
            msgStatic.msgtype = MessageTypes.MSG_NEW;
            //msg
            Console.Write("MESSAGE?:");
            msgStatic.msg = Console.ReadLine();
            //output it
            msgStatic.output();
            //send it
            SendUDP(msgStatic);
        }
    }
}
