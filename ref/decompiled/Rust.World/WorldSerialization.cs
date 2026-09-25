using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Facepunch;
using LZ4;
using ProtoBuf;
using Unity.Collections;
using UnityEngine;

public class WorldSerialization
{
	public const uint PreviousVersion = 9u;

	public const uint CurrentVersion = 10u;

	public WorldData world = new WorldData
	{
		size = 4000u,
		maps = new List<MapData>(),
		prefabs = new List<PrefabData>(),
		paths = new List<PathData>()
	};

	public uint Version { get; private set; }

	public string Checksum { get; private set; }

	public long Timestamp { get; private set; }

	public WorldSerialization()
	{
		Version = 10u;
		Checksum = null;
		Timestamp = 0L;
	}

	public MapData GetMap(string name)
	{
		for (int i = 0; i < world.maps.Count; i++)
		{
			if (world.maps[i].name == name)
			{
				return world.maps[i];
			}
		}
		return null;
	}

	public void AddMap(string name, byte[] data)
	{
		MapData mapData = new MapData();
		mapData.name = name;
		mapData.data = data;
		world.maps.Add(mapData);
	}

	public IEnumerable<PrefabData> GetPrefabs(string category)
	{
		return world.prefabs.Where((PrefabData p) => p.category == category);
	}

	public void AddPrefab(string category, uint id, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		PrefabData prefabData = new PrefabData();
		prefabData.category = category;
		prefabData.id = id;
		prefabData.position = position;
		prefabData.rotation = rotation;
		prefabData.scale = scale;
		world.prefabs.Add(prefabData);
	}

	public IEnumerable<PathData> GetPaths(string name)
	{
		return world.paths.Where((PathData p) => p.name.Contains(name));
	}

	public PathData GetPath(string name)
	{
		for (int i = 0; i < world.paths.Count; i++)
		{
			if (world.paths[i].name == name)
			{
				return world.paths[i];
			}
		}
		return null;
	}

	public void AddPath(PathData path)
	{
		world.paths.Add(path);
	}

	public void Clear()
	{
		world.maps.Clear();
		world.prefabs.Clear();
		world.paths.Clear();
		Version = 10u;
		Checksum = null;
	}

	public void Save(string fileName)
	{
		long value = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		try
		{
			using BufferStream bufferStream = Pool.Get<BufferStream>().Initialize();
			WorldData.Serialize(bufferStream, world);
			ArraySegment<byte> buffer = bufferStream.GetBuffer();
			using FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
			using BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			binaryWriter.Write(Version);
			binaryWriter.Write(value);
			using LZ4Stream lZ4Stream = new LZ4Stream(fileStream, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression);
			lZ4Stream.Write(buffer.Array, buffer.Offset, buffer.Count);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public void Load(string fileName)
	{
		try
		{
			using FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			ReadWorldData(stream);
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
	}

	public void ReadWorldData(Stream stream)
	{
		using (BinaryReader binaryReader = new BinaryReader(stream))
		{
			Version = binaryReader.ReadUInt32();
			if (Version == 9)
			{
				LoadWorldFromStream(stream);
				Version = 10u;
				Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			}
			else if (Version == 10)
			{
				Timestamp = binaryReader.ReadInt64();
				LoadWorldFromStream(stream);
			}
			Checksum = Hash();
		}
		void LoadWorldFromStream(Stream stream)
		{
			using LZ4Stream lZ4Stream = new LZ4Stream(stream, LZ4StreamMode.Decompress);
			using NativeMemoryStream nativeMemoryStream = new NativeMemoryStream(60000000, Allocator.Temp);
			lZ4Stream.CopyTo(nativeMemoryStream);
			nativeMemoryStream.Position = 0L;
			using BufferStream stream2 = Pool.Get<BufferStream>().Initialize(nativeMemoryStream.Span);
			world = WorldData.Deserialize(stream2);
		}
	}

	public void CalculateChecksum()
	{
		Checksum = Hash();
	}

	private string Hash()
	{
		Checksum checksum = new Checksum();
		List<PrefabData> prefabs = world.prefabs;
		if (prefabs != null)
		{
			for (int i = 0; i < prefabs.Count; i++)
			{
				PrefabData prefabData = prefabs[i];
				checksum.Add(prefabData.id);
				checksum.Add(prefabData.position.x, 3);
				checksum.Add(prefabData.position.y, 3);
				checksum.Add(prefabData.position.z, 3);
				checksum.Add(prefabData.rotation.x, 3);
				checksum.Add(prefabData.rotation.y, 3);
				checksum.Add(prefabData.rotation.z, 3);
				checksum.Add(prefabData.scale.x, 3);
				checksum.Add(prefabData.scale.y, 3);
				checksum.Add(prefabData.scale.z, 3);
			}
		}
		return checksum.MD5();
	}

	public int CalculateCount()
	{
		return world.maps.Count + world.prefabs.Count + world.paths.Count;
	}
}
