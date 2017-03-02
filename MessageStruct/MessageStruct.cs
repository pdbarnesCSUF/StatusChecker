using ProtoBuf;
using System;
using System.Net;
using System.Net.NetworkInformation;

//https://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkinterface(v=vs.110).aspx
//has some networkstuff and some domain stuff in the code
//TODO add default blank values
[ProtoContract]
public struct MessageStruct
{
    [ProtoMember(1)] public string name;
    [ProtoMember(2)] public string msg;
    //sketching

    //=====message=====
    public string label; //doi a personel label
    public uint status; //MACRO'd... in THIS? file?
    public uint msg_number; //msg number, used to detect missed packets
    public DateTime time_stamp; //when generated
    public uint ping; //ping to server result, can be used to detect one-way network problems
    public uint client_version;
    //=====general=====
    public string hostname;
    public Guid machine_guid; //from HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\MachineGuid
    public string os_name;
    public uint[] cpu; //one for each cpu/core/whatever
    public uint ram_total;
    public uint ram_used;
    public uint swap_total;
    public uint swap_used;
    public uint[] hdd_total;
    public uint[] hdd_used;
    //public uint domain_status; //no idea what type yet
    public uint processes_total;
    //=====networking=====
    public PhysicalAddress mac_address;
    public IPAddress ip;
    
}

//https://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkinterface(v=vs.110).aspx
public struct NetworkInterfaceSlim
{
    public string name;
    public Int64 speed; //in bits per second
    public PhysicalAddress mac_address;
    public IPAddress ip;
    public OperationalStatus status; //https://msdn.microsoft.com/en-us/library/system.net.networkinformation.operationalstatus(v=vs.110).aspx
    public 
}