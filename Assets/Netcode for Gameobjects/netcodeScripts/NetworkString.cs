using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct NetworkString : INetworkSerializable
{
    private FixedString32Bytes fixedString;

    public NetworkString(string value)
    {
        fixedString = value;
    }

    public static implicit operator string(NetworkString ns)
    {
        return ns.fixedString.ToString();
    }

    public static implicit operator NetworkString(string s)
    {
        return new NetworkString(s);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref fixedString);
    }
}