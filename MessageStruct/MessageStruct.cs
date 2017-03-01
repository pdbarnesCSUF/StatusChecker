using ProtoBuf;
using System;

//[Serializable]
[ProtoContract]
public struct MessageStruct
{
    [ProtoMember(1)] public string name;
    [ProtoMember(2)] public string msg;
}