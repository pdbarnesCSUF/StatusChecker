using ProtoBuf;
using System;
using System.Net;
using System.Net.NetworkInformation;

//https://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkinterface(v=vs.110).aspx
//has some networkstuff and some domain stuff in the code
//TODO stop calling this a message.... a report maybe?
[ProtoContract]
public struct MessageStruct
{
    //=====message=====
    [ProtoMember(1)]  public string label; //doi a personel label
    [ProtoMember(2)]  public int status; //ENUM'd... in THIS? file?
    [ProtoMember(3)]  public uint msg_number; //msg number, used to detect missed packets
    [ProtoMember(4)]  public DateTime time_stamp; //when generated
    [ProtoMember(5)]  public uint ping; //ping to server result, can be used to detect one-way network problems
    [ProtoMember(6)]  public uint client_version;
    [ProtoMember(7)]  public uint send_frequency; //in seconds, its just one option anyways, put here
    //=====general=====
    [ProtoMember(8)]  public string hostname;
    [ProtoMember(9)]  public string machine_serial; //apparetnyl hard to get and unreliable? whatever
    [ProtoMember(10)] public string os_name;  //Friendly OS name (Windows Potato SP23)
    [ProtoMember(11)] public uint[] cpus; //one for each cpu/core/whatever, combine into struct w load on it?
    [ProtoMember(12)] public uint ram_total;
    [ProtoMember(13)] public uint ram_used;
    [ProtoMember(14)] public uint swap_total;
    [ProtoMember(15)] public uint swap_used;
    [ProtoMember(16)] public uint[] hdds_total; //combine to a hdd struct with driveletter, name, used, total
    [ProtoMember(17)] public uint[] hdds_used;
    // [ProtoMember(1)] //public uint domain_status; //no idea what type yet
    [ProtoMember(18)] public uint processes_total;
    //=====networking=====
    [ProtoMember(19)] public NetworkInterfaceSlim[] nics;

    [ProtoMember(20)] public string msg; //the CUSTOM message, either by a person or debug

    //=====func=====
    public void output()
    {
        Console.WriteLine(label);
        Console.WriteLine(status + " " + time_stamp + " " + msg_number);
        Console.WriteLine("ver:" + client_version + " ping:" + ping);
        Console.WriteLine("sec:" + send_frequency);
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

//need a server options struct
    //check frequency
    //force recheck frequency

//need a GUI identifier struct
    //label
    //username??
    //hostname
    //ip
    //mac
