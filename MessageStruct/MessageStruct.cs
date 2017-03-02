﻿using ProtoBuf;
using System;
using System.Net;
using System.Net.NetworkInformation;

//https://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkinterface(v=vs.110).aspx
//has some networkstuff and some domain stuff in the code
//TODO add default blank values
[ProtoContract]
public struct MessageStruct
{
    //=====message=====
    [ProtoMember(1)] public string label; //doi a personel label
    [ProtoMember(2)] public uint status; //ENUM'd... in THIS? file?
    [ProtoMember(3)] public uint msg_number; //msg number, used to detect missed packets
    [ProtoMember(4)] public DateTime time_stamp; //when generated
    [ProtoMember(5)] public uint ping; //ping to server result, can be used to detect one-way network problems
    [ProtoMember(6)] public uint client_version;
    //=====general=====
    [ProtoMember(7)] public string hostname;
    [ProtoMember(8)] public Guid machine_guid; //from HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\MachineGuid
    [ProtoMember(9)] public string os_name;  //FIXME temp use as "msg"
    [ProtoMember(10)] public uint[] cpus; //one for each cpu/core/whatever
    [ProtoMember(11)] public uint ram_total;
    [ProtoMember(12)] public uint ram_used;
    [ProtoMember(13)] public uint swap_total;
    [ProtoMember(14)] public uint swap_used;
    [ProtoMember(15)] public uint[] hdds_total; //combine to a hdd struct with driveletter, name, used, total
    [ProtoMember(16)] public uint[] hdds_used;
    // [ProtoMember(1)] //public uint domain_status; //no idea what type yet
    [ProtoMember(17)] public uint processes_total;
    //=====networking=====
    [ProtoMember(18)] public NetworkInterfaceSlim[] nics;

    //=====func=====
    public void output()
    {
        Console.WriteLine(label);
        Console.WriteLine(status + " " + time_stamp + " " + msg_number);
        Console.WriteLine("ver:" + client_version + " ping:" + ping);
        Console.WriteLine(hostname);
        Console.WriteLine(machine_guid);
        Console.WriteLine(os_name);
        Console.WriteLine(cpus);
        Console.WriteLine(ram_used + "/" + ram_total);
        Console.WriteLine(swap_used + "/" + swap_total);
        for (int i = 0; i < hdds_total.Length; ++i)
        {
            Console.WriteLine(hdds_used[i] + "/" + hdds_total[i]);
        }
        Console.WriteLine("Processes:" + processes_total);
        foreach (NetworkInterfaceSlim nic in nics)
        {
            nic.output();
        }
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
    [ProtoMember(3)] public Int64 speed; //in bits per second
    [ProtoMember(4)] public PhysicalAddress mac_address;
    [ProtoMember(5)] public IPAddress ip;
    //[ProtoMember(6)] public NetworkInterfaceType type; //dont know if we really care about this

    public void output()
    {
        Console.WriteLine(name);
        Console.WriteLine(status + " spd(MB):" + speed/1024/1024); //mega
        Console.WriteLine(mac_address);
        Console.WriteLine(ip);
    }
}

//need an client options struct
    //auto send frequency

//need a server options struct
    //check frequency
    //force recheck frequency

//need a GUI identifier struct
    //label
    //username??
    //hostname
    //ip
    //mac
