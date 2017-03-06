﻿//http://stackoverflow.com/questions/400135/listt-or-ilistt an interesting but ultimately confusing read
//rename file/projectto be more.... accurate
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;

//https://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkinterface(v=vs.110).aspx
//has some networkstuff and some domain stuff in the code
//TODO stop calling this a message.... a report maybe?
[ProtoContract]
public struct MessageClient
{
    //=====message=====
    [ProtoMember(1)]  public string label; //doi a personel label
    [ProtoMember(2)]  public int status; //ENUM'd... in THIS? file?
    [ProtoMember(3)]  public ulong msg_number; //msg number, used to detect missed packets
    [ProtoMember(4)]  public DateTime time_stamp; //when generated
    [ProtoMember(5)]  public long ping; //ping to server result, can be used to detect one-way network problems
    [ProtoMember(6)]  public uint client_version;
    [ProtoMember(7)]  public uint send_frequency; //in seconds, its just one option anyways, put here
    //=====general=====
    [ProtoMember(8)]  public string hostname;
    [ProtoMember(9)]  public string machine_serial; //apparetnyl hard to get and unreliable? whatever
    [ProtoMember(10)] public string os_name;  //Friendly OS name (Windows Potato SP23)
    [ProtoMember(11)] public ushort[] cpus; //one for each core, dont need class, length for total.
    [ProtoMember(12)] public ulong ram_total;
    [ProtoMember(13)] public ulong ram_used;
    [ProtoMember(14)] public ulong swap_total;
    [ProtoMember(15)] public ulong swap_used;
    [ProtoMember(16)] public DriveInfoSlim[] drives;
    // [ProtoMember(17)] //public uint domain_status; //no idea what type yet
    [ProtoMember(18)] public int processes_total;
    //=====networking=====
    [ProtoMember(19)] public NetworkInterfaceSlim[] nics;

    [ProtoMember(20)] public string msg; //the CUSTOM message, either by a person or debug

    //=====func=====
    public void output()
    {
        Console.WriteLine(label);
        Console.WriteLine(status + " " + time_stamp + " #" + msg_number);
        Console.WriteLine("ver:" + client_version + " ping:" + ping);
        Console.WriteLine("send freq:" + send_frequency);
        Console.WriteLine(hostname);
        Console.WriteLine(machine_serial);
        Console.WriteLine(os_name);
        if (cpus != null)
        {
            uint sum = 0;
            foreach (ushort cpu in cpus)
            {
                Console.Write(cpu + "% ");
                sum += cpu;
            }
            Console.WriteLine("AVG:" + sum/cpus.Length + "%");
        }
        Console.WriteLine("RAM:" + ram_used / 1024 / 1024 + "/" + ram_total / 1024 / 1024 + " (mb)");
        Console.WriteLine("SWAP:" + swap_used / 1024 / 1024 + "/" + swap_total / 1024 / 1024 + " (mb)");
        if (drives != null)
        {
            for (int i = 0; i < drives.Length; ++i) //FIXME!!!! CRASH HERE!!!
            {
                Console.WriteLine(drives[i]);
            }
        }
        else
        {
            Console.WriteLine("No drives!?!?");
        }
        Console.WriteLine("Processes:" + processes_total);
        if (nics != null)
        {
            foreach (NetworkInterfaceSlim nic in nics)
            {
                nic.output();
            }
        }
        else
        {
            Console.WriteLine("No NICs!?!?");
        }
        Console.WriteLine("MSG:" + msg);
    }
}

//https://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkinterface(v=vs.110).aspx
//yes you must protobuf for all nested classes
//GOOD TO KNOW (:
[ProtoContract]
public struct NetworkInterfaceSlim
{
    [ProtoMember(1)] public string name;
    [ProtoMember(2)] public OperationalStatus status; //https://msdn.microsoft.com/en-us/library/system.net.networkinformation.operationalstatus(v=vs.110).aspx
    [ProtoMember(3)] public long speed; //in bits per second
    [ProtoMember(4)] public string mac_address; //PhysicalAddress type wont serialize T_T
    [ProtoMember(5)] public string ip; //IPAddress doesn't serialize either!
    //[ProtoMember(6)] public NetworkInterfaceType type; //dont know if we really care about this

    public void output()
    {
        Console.WriteLine(name);
        Console.WriteLine("Status:" + status + " spd(MB):" + speed/1024/1024); //mega
        Console.WriteLine(mac_address);
        Console.WriteLine(ip);
    }
}

[ProtoContract]
public struct DriveInfoSlim
{
    [ProtoMember(1)] public DriveType type;
    [ProtoMember(2)] public string name;
    [ProtoMember(3)] public long free;
    [ProtoMember(4)] public long total;

    public override string ToString()
    {
        return name + " " + type + " " + free / 1024 / 2014 + "/" + total / 1024 / 1024 + " Free(mb)";
    }
}

//a list of them kept on server
//a list sent to GUIs to display
[ProtoContract]
public struct ClientStatus
{
    [ProtoMember(1)] public string label;
    [ProtoMember(2)] public string machine_serial; //used to uniquely identify the computer
    [ProtoMember(3)] public MessageClient report;
    [ProtoMember(4)] public ulong report_last;
    [ProtoMember(5)] public ulong report_received;
    [ProtoMember(6)] public ulong report_lost;
    public override string ToString()
    {
        return label + report_lost + ":" + report_received;
    }
}

//a single one, sent from server to GUI
[ProtoContract]
public struct MessageClientList
{
    [ProtoMember(1)] public List<ClientStatus> client_list;
}
//one static on server if use as options
//sent out to other GUIs if they ask for it.
[ProtoContract]
public struct MessageServer
{
    [ProtoMember(1)] public int check_freq_client; //if client doesnt talk after x time, manually ask for a update
    [ProtoMember(2)] public List<MessageGUI> guis;
}

//on the server, one per connected gui, inside of MessageServer
//on the gui, just one static if used as options
[ProtoContract]
public struct MessageGUI
{
    [ProtoMember(1)] public string label; //personal label set by gui
    [ProtoMember(2)] public string username; //username of the running user - not editable
    [ProtoMember(3)] public string hostname; //hostname of gui - not editable
    [ProtoMember(4)] public string ip; //ipaddress
    [ProtoMember(5)] public string mac; //physical MAC address
}
