using System;
using System.Text.Json.Serialization;

public struct NetworkableId : IEquatable<NetworkableId>
{
	[JsonInclude]
	public ulong Value;

	[JsonInclude]
	public bool IsValid => Value != 0;

	public static NetworkableId EmptyId => new NetworkableId(0uL);

	public NetworkableId(ulong value)
	{
		Value = value;
	}

	public override string ToString()
	{
		return Value.ToString("G");
	}

	public bool Equals(NetworkableId other)
	{
		return Value == other.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is NetworkableId other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(NetworkableId left, NetworkableId right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(NetworkableId left, NetworkableId right)
	{
		return !left.Equals(right);
	}
}
