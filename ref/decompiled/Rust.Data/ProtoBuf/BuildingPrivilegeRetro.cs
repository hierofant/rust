using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BuildingPrivilegeRetro : IDisposable, Pool.IPooled, IProto<BuildingPrivilegeRetro>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<float> resources;

	[NonSerialized]
	public List<BuildingPrivilegeRetroTool> tools;

	public static void ResetToPool(BuildingPrivilegeRetro instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.resources != null)
		{
			List<float> obj = instance.resources;
			Pool.FreeUnmanaged(ref obj);
			instance.resources = obj;
		}
		if (instance.tools != null)
		{
			for (int i = 0; i < instance.tools.Count; i++)
			{
				if (instance.tools[i] != null)
				{
					instance.tools[i].ResetToPool();
					instance.tools[i] = null;
				}
			}
			List<BuildingPrivilegeRetroTool> obj2 = instance.tools;
			Pool.Free(ref obj2, freeElements: false);
			instance.tools = obj2;
		}
		Pool.Free(ref instance);
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose BuildingPrivilegeRetro with ShouldPool set to false!");
		}
		if (!_disposed)
		{
			ResetToPool();
			_disposed = true;
		}
	}

	public virtual void EnterPool()
	{
		_disposed = true;
	}

	public virtual void LeavePool()
	{
		_disposed = false;
	}

	public void CopyTo(BuildingPrivilegeRetro instance)
	{
		if (resources != null)
		{
			instance.resources = Pool.Get<List<float>>();
			for (int i = 0; i < resources.Count; i++)
			{
				float item = resources[i];
				instance.resources.Add(item);
			}
		}
		else
		{
			instance.resources = null;
		}
		if (tools != null)
		{
			instance.tools = Pool.Get<List<BuildingPrivilegeRetroTool>>();
			for (int j = 0; j < tools.Count; j++)
			{
				BuildingPrivilegeRetroTool item2 = tools[j].Copy();
				instance.tools.Add(item2);
			}
		}
		else
		{
			instance.tools = null;
		}
	}

	public BuildingPrivilegeRetro Copy()
	{
		BuildingPrivilegeRetro buildingPrivilegeRetro = Pool.Get<BuildingPrivilegeRetro>();
		CopyTo(buildingPrivilegeRetro);
		return buildingPrivilegeRetro;
	}

	public static BuildingPrivilegeRetro Deserialize(BufferStream stream)
	{
		BuildingPrivilegeRetro buildingPrivilegeRetro = Pool.Get<BuildingPrivilegeRetro>();
		Deserialize(stream, buildingPrivilegeRetro, isDelta: false);
		return buildingPrivilegeRetro;
	}

	public static BuildingPrivilegeRetro DeserializeLengthDelimited(BufferStream stream)
	{
		BuildingPrivilegeRetro buildingPrivilegeRetro = Pool.Get<BuildingPrivilegeRetro>();
		DeserializeLengthDelimited(stream, buildingPrivilegeRetro, isDelta: false);
		return buildingPrivilegeRetro;
	}

	public static BuildingPrivilegeRetro DeserializeLength(BufferStream stream, int length)
	{
		BuildingPrivilegeRetro buildingPrivilegeRetro = Pool.Get<BuildingPrivilegeRetro>();
		DeserializeLength(stream, length, buildingPrivilegeRetro, isDelta: false);
		return buildingPrivilegeRetro;
	}

	public static BuildingPrivilegeRetro Deserialize(byte[] buffer)
	{
		BuildingPrivilegeRetro buildingPrivilegeRetro = Pool.Get<BuildingPrivilegeRetro>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, buildingPrivilegeRetro, isDelta: false);
		return buildingPrivilegeRetro;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BuildingPrivilegeRetro previous)
	{
		if (previous == null)
		{
			Serialize(stream, this);
		}
		else
		{
			SerializeDelta(stream, this, previous);
		}
	}

	public virtual void ReadFromStream(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void ReadFromStream(BufferStream stream, int size, bool isDelta = false)
	{
		DeserializeLength(stream, size, this, isDelta);
	}

	public static BuildingPrivilegeRetro Deserialize(BufferStream stream, BuildingPrivilegeRetro instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.resources == null)
			{
				instance.resources = Pool.Get<List<float>>();
			}
			if (instance.tools == null)
			{
				instance.tools = Pool.Get<List<BuildingPrivilegeRetroTool>>();
			}
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeRetro");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				stream.ConsumeRepeatedElement();
				instance.resources.Add(ProtocolParser.ReadSingle(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				stream.ConsumeRepeatedElement();
				instance.tools.Add(BuildingPrivilegeRetroTool.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingPrivilegeRetro DeserializeLengthDelimited(BufferStream stream, BuildingPrivilegeRetro instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.resources == null)
			{
				instance.resources = Pool.Get<List<float>>();
			}
			if (instance.tools == null)
			{
				instance.tools = Pool.Get<List<BuildingPrivilegeRetroTool>>();
			}
		}
		long num = ProtocolParser.ReadUInt32(stream);
		num += stream.Position;
		uint lastFieldId = 0u;
		while (true)
		{
			if (stream.Position >= num)
			{
				if (stream.Position == num)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int num2 = stream.ReadByte();
			if (num2 == -1)
			{
				throw new EndOfStreamException();
			}
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeRetro");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				stream.ConsumeRepeatedElement();
				instance.resources.Add(ProtocolParser.ReadSingle(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				stream.ConsumeRepeatedElement();
				instance.tools.Add(BuildingPrivilegeRetroTool.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingPrivilegeRetro DeserializeLength(BufferStream stream, int length, BuildingPrivilegeRetro instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.resources == null)
			{
				instance.resources = Pool.Get<List<float>>();
			}
			if (instance.tools == null)
			{
				instance.tools = Pool.Get<List<BuildingPrivilegeRetroTool>>();
			}
		}
		long num = stream.Position + length;
		uint lastFieldId = 0u;
		while (true)
		{
			if (stream.Position >= num)
			{
				if (stream.Position == num)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int num2 = stream.ReadByte();
			if (num2 == -1)
			{
				throw new EndOfStreamException();
			}
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeRetro");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				stream.ConsumeRepeatedElement();
				instance.resources.Add(ProtocolParser.ReadSingle(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				stream.ConsumeRepeatedElement();
				instance.tools.Add(BuildingPrivilegeRetroTool.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetro");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BuildingPrivilegeRetro instance, BuildingPrivilegeRetro previous)
	{
		if (instance.resources != null)
		{
			for (int i = 0; i < instance.resources.Count; i++)
			{
				float f = instance.resources[i];
				stream.WriteByte(13);
				ProtocolParser.WriteSingle(stream, f);
			}
		}
		if (instance.tools == null)
		{
			return;
		}
		for (int j = 0; j < instance.tools.Count; j++)
		{
			BuildingPrivilegeRetroTool buildingPrivilegeRetroTool = instance.tools[j];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			BuildingPrivilegeRetroTool.SerializeDelta(stream, buildingPrivilegeRetroTool, buildingPrivilegeRetroTool);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field tools (ProtoBuf.BuildingPrivilegeRetroTool)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, BuildingPrivilegeRetro instance)
	{
		if (instance.resources != null)
		{
			for (int i = 0; i < instance.resources.Count; i++)
			{
				float f = instance.resources[i];
				stream.WriteByte(13);
				ProtocolParser.WriteSingle(stream, f);
			}
		}
		if (instance.tools == null)
		{
			return;
		}
		for (int j = 0; j < instance.tools.Count; j++)
		{
			BuildingPrivilegeRetroTool instance2 = instance.tools[j];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			BuildingPrivilegeRetroTool.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field tools (ProtoBuf.BuildingPrivilegeRetroTool)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (tools != null)
		{
			for (int i = 0; i < tools.Count; i++)
			{
				tools[i]?.InspectUids(action);
			}
		}
	}
}
