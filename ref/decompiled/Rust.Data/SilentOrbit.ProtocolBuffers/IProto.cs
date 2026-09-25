namespace SilentOrbit.ProtocolBuffers;

public interface IProto
{
	void WriteToStream(BufferStream stream);

	void ReadFromStream(BufferStream stream, bool isDelta = false);

	void ReadFromStream(BufferStream stream, int size, bool isDelta = false);
}
public interface IProto<in T> : IProto where T : IProto
{
	void WriteToStreamDelta(BufferStream stream, T previousProto);

	void CopyTo(T other);
}
