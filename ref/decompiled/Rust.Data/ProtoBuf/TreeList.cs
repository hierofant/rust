using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class TreeList : IDisposable, Pool.IPooled, IProto<TreeList>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Tree> trees;

	public static void ResetToPool(TreeList instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.trees != null)
		{
			for (int i = 0; i < instance.trees.Count; i++)
			{
				if (instance.trees[i] != null)
				{
					instance.trees[i].ResetToPool();
					instance.trees[i] = null;
				}
			}
			List<Tree> obj = instance.trees;
			Pool.Free(ref obj, freeElements: false);
			instance.trees = obj;
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
			throw new Exception("Trying to dispose TreeList with ShouldPool set to false!");
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

	public void CopyTo(TreeList instance)
	{
		if (trees != null)
		{
			instance.trees = Pool.Get<List<Tree>>();
			for (int i = 0; i < trees.Count; i++)
			{
				Tree item = trees[i].Copy();
				instance.trees.Add(item);
			}
		}
		else
		{
			instance.trees = null;
		}
	}

	public TreeList Copy()
	{
		TreeList treeList = Pool.Get<TreeList>();
		CopyTo(treeList);
		return treeList;
	}

	public static TreeList Deserialize(BufferStream stream)
	{
		TreeList treeList = Pool.Get<TreeList>();
		Deserialize(stream, treeList, isDelta: false);
		return treeList;
	}

	public static TreeList DeserializeLengthDelimited(BufferStream stream)
	{
		TreeList treeList = Pool.Get<TreeList>();
		DeserializeLengthDelimited(stream, treeList, isDelta: false);
		return treeList;
	}

	public static TreeList DeserializeLength(BufferStream stream, int length)
	{
		TreeList treeList = Pool.Get<TreeList>();
		DeserializeLength(stream, length, treeList, isDelta: false);
		return treeList;
	}

	public static TreeList Deserialize(byte[] buffer)
	{
		TreeList treeList = Pool.Get<TreeList>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, treeList, isDelta: false);
		return treeList;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TreeList previous)
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

	public static TreeList Deserialize(BufferStream stream, TreeList instance, bool isDelta)
	{
		if (!isDelta && instance.trees == null)
		{
			instance.trees = Pool.Get<List<Tree>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.TreeList");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.TreeList");
				stream.ConsumeRepeatedElement();
				instance.trees.Add(Tree.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.TreeList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TreeList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static TreeList DeserializeLengthDelimited(BufferStream stream, TreeList instance, bool isDelta)
	{
		if (!isDelta && instance.trees == null)
		{
			instance.trees = Pool.Get<List<Tree>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.TreeList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.TreeList");
				stream.ConsumeRepeatedElement();
				instance.trees.Add(Tree.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.TreeList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TreeList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static TreeList DeserializeLength(BufferStream stream, int length, TreeList instance, bool isDelta)
	{
		if (!isDelta && instance.trees == null)
		{
			instance.trees = Pool.Get<List<Tree>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.TreeList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.TreeList");
				stream.ConsumeRepeatedElement();
				instance.trees.Add(Tree.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.TreeList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TreeList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TreeList instance, TreeList previous)
	{
		if (instance.trees == null)
		{
			return;
		}
		for (int i = 0; i < instance.trees.Count; i++)
		{
			Tree tree = instance.trees[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Tree.SerializeDelta(stream, tree, tree);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field trees (ProtoBuf.Tree)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, TreeList instance)
	{
		if (instance.trees == null)
		{
			return;
		}
		for (int i = 0; i < instance.trees.Count; i++)
		{
			Tree instance2 = instance.trees[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Tree.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field trees (ProtoBuf.Tree)");
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
		if (trees != null)
		{
			for (int i = 0; i < trees.Count; i++)
			{
				trees[i]?.InspectUids(action);
			}
		}
	}
}
