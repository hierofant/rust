using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch.Extend;
using Facepunch.Utility;
using Ionic.Zlib;
using UnityEngine;

public static class ProfileExporter
{
	private struct MainThreadInfo
	{
		public List<uint> SyncIndices;

		public List<uint> FrameStarts;

		public int Frames;

		public int CallstackDepth;
	}

	public static class JSON
	{
		private struct ThreadMetadata
		{
			internal int ThreadId;

			internal int AllocationThreadId;

			internal void Emit(StringStream builder)
			{
				bool isMainThread = ThreadId == ServerProfiler.GetMainThreadId();
				EmitThreadName(isMainThread, isAllocThread: false, ThreadId, ThreadId, builder);
				builder.Append(',');
				EmitThreadName(isMainThread, isAllocThread: true, AllocationThreadId, ThreadId, builder);
			}

			private static void EmitThreadName(bool isMainThread, bool isAllocThread, int id, int origId, StringStream builder)
			{
				builder.Append("{\"name\":\"thread_name\",\"ph\":\"M\",\"pid\":0,\"tid\":");
				builder.Append(id);
				builder.Append(",\"args\":{\"name\":\"");
				if (isMainThread)
				{
					builder.Append("Main Thread");
				}
				else
				{
					builder.Append("Thread");
				}
				if (isAllocThread)
				{
					if (!isMainThread)
					{
						builder.Append(' ');
						builder.Append(origId);
					}
					builder.Append(" Allocations");
				}
				builder.Append("\"}}");
			}
		}

		public class StringStream : MemoryStream
		{
			private GZipStream outputStream;

			public StringStream(int capacity, GZipStream outputStream)
				: base(capacity)
			{
				this.outputStream = outputStream;
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				if (base.Position + count < base.Capacity)
				{
					base.Write(buffer, offset, count);
					return;
				}
				int num = base.Capacity - (int)base.Position;
				if (num > 0)
				{
					base.Write(buffer, offset, num);
				}
				Flush();
				if (count - num > 0)
				{
					base.Write(buffer, offset + num, count - num);
				}
			}

			public override void Write(ReadOnlySpan<byte> source)
			{
				throw new NotSupportedException("Write(ReadOnlySpan<byte>) not implemented!");
			}

			public override void WriteByte(byte value)
			{
				if (base.Position + 1 < base.Capacity)
				{
					base.WriteByte(value);
					return;
				}
				Flush();
				base.WriteByte(value);
			}

			public void Append(char c)
			{
				WriteByte((byte)c);
			}

			public void Append(string text)
			{
				foreach (char c in text)
				{
					Append(c);
				}
			}

			public void Append(int num)
			{
				Span<char> destination = stackalloc char[32];
				num.TryFormat(destination, out var charsWritten);
				Append(destination.Slice(0, charsWritten));
			}

			public void Append(long num)
			{
				Span<char> destination = stackalloc char[32];
				num.TryFormat(destination, out var charsWritten);
				Append(destination.Slice(0, charsWritten));
			}

			public void Append(ulong num)
			{
				Span<char> destination = stackalloc char[32];
				num.TryFormat(destination, out var charsWritten);
				Append(destination.Slice(0, charsWritten));
			}

			public void Append(ReadOnlySpan<char> chars)
			{
				ReadOnlySpan<char> readOnlySpan = chars;
				for (int i = 0; i < readOnlySpan.Length; i++)
				{
					char c = readOnlySpan[i];
					Append(c);
				}
			}

			public override void Flush()
			{
				long position = Position;
				if (position > 0)
				{
					byte[] buffer = GetBuffer();
					outputStream.Write(buffer, 0, (int)position);
					Position = 0L;
				}
			}
		}

		public static bool Export(string filename, IList<ServerProfiler.Profile> profiles, ServerProfiler.MemoryState memState, bool skipToStackStart = true)
		{
			try
			{
				Debug.Log("Starting JSON snapshot generation...");
				Preprocess(profiles, out var mainInfo, out var _, skipToStackStart);
				string text = Path.Join(Server.rootFolder, "profiler");
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				using (FileStream stream = new FileStream(Path.Join(text, filename + ".json.gz"), FileMode.Create, FileAccess.Write, FileShare.None, 20480))
				{
					using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress);
					gZipStream.FlushMode = FlushType.Sync;
					using StringStream stringStream = new StringStream(16384, gZipStream);
					stringStream.Append('[');
					int num = 0;
					foreach (ServerProfiler.Profile profile2 in profiles)
					{
						num = Math.Max(num, profile2.ThreadId);
					}
					List<ThreadMetadata> list = new List<ThreadMetadata>();
					for (int i = 0; i < profiles.Count; i++)
					{
						ServerProfiler.Profile profile = profiles[i];
						ThreadMetadata item = default(ThreadMetadata);
						item.ThreadId = profile.ThreadId;
						num = (item.AllocationThreadId = num + 1);
						if (i != 0)
						{
							stringStream.Append(',');
						}
						item.Emit(stringStream);
						list.Add(item);
					}
					long firstMarkTimestamp = 0L;
					for (int j = 0; j < profiles.Count; j++)
					{
						ServerProfiler.Profile mainProfile = profiles[j];
						if (mainProfile.ThreadId == ServerProfiler.GetMainThreadId())
						{
							ThreadMetadata metadata = list[j];
							if (ProcessMainProfile(in mainProfile, in mainInfo, in metadata, stringStream, out firstMarkTimestamp))
							{
								break;
							}
							return false;
						}
					}
					for (int k = 0; k < profiles.Count; k++)
					{
						ServerProfiler.Profile workerProfile = profiles[k];
						if (workerProfile.ThreadId != ServerProfiler.GetMainThreadId())
						{
							ThreadMetadata metadata = list[k];
							if (!ProcessWorkerProfile(in workerProfile, in metadata, firstMarkTimestamp, stringStream))
							{
								return false;
							}
						}
					}
					ProcessMemoryState(in memState, firstMarkTimestamp, stringStream);
					stringStream.Append("]");
					stringStream.Flush();
					Debug.Log("Generation done, flushing...");
				}
				Debug.Log("Snapshot json export done!");
				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return false;
			}
		}

		private unsafe static bool ProcessMainProfile(in ServerProfiler.Profile mainProfile, in MainThreadInfo info, in ThreadMetadata metadata, StringStream builder, out long firstMarkTimestamp)
		{
			int i = 0;
			byte* data = mainProfile.Data;
			uint num = ((info.Frames > 0) ? info.FrameStarts[0] : 0u);
			if (num >= mainProfile.WriteEnd)
			{
				firstMarkTimestamp = 0L;
				return true;
			}
			firstMarkTimestamp = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(data + num).Timestamp;
			long num2 = 0L;
			int num3 = Mathf.Max(info.Frames, 1);
			for (int j = 0; j < num3; j++)
			{
				uint num4 = ((info.Frames > 0) ? info.FrameStarts[j] : num);
				ServerProfiler.Mark mark = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(data + num4);
				uint num5 = mainProfile.WriteEnd;
				if (j < info.Frames - 1)
				{
					num5 = info.FrameStarts[j + 1];
				}
				ServerProfiler.Mark mark2 = default(ServerProfiler.Mark);
				mark2.Event = ServerProfiler.Mark.Type.Enter;
				mark2.Timestamp = mark.Timestamp;
				AddMark(mark2, $"UnityFrame{j}", mainProfile.ThreadId, firstMarkTimestamp, builder);
				uint totalMem = 0u;
				long offset = ServerProfiler.TimestampToMicros(mark.Timestamp - firstMarkTimestamp);
				EmitCounter("Main Thread", offset, "Total(B)", totalMem, builder);
				if (num4 == 0)
				{
					ServerProfiler.Mark mark3;
					for (; i < -info.CallstackDepth; i += AdjustCallstackDepth(in mark3))
					{
						mark3 = default(ServerProfiler.Mark);
						mark3.Event = ServerProfiler.Mark.Type.Enter;
						mark3.Timestamp = firstMarkTimestamp;
						AddMark(mark3, "Unknown", mainProfile.ThreadId, firstMarkTimestamp, builder);
					}
				}
				uint readInd = num4;
				while (readInd < num5)
				{
					ServerProfiler.Mark mark4 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(data + readInd);
					num2 = mark4.Timestamp;
					i += AdjustCallstackDepth(in mark4);
					if (i < 0)
					{
						Debug.LogError($"ServerProfiler: Unexpected callstack depth: {i}, offset: {readInd}, frame: {j}");
						return false;
					}
					AddMark(in mainProfile, in metadata, readInd, ref totalMem, firstMarkTimestamp, builder);
					if (!AdvanceReadInd(in mark4, data, ref readInd))
					{
						return false;
					}
				}
				long timestamp = ((j < info.Frames - 1) ? System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(data + num5).Timestamp : num2);
				mark2 = default(ServerProfiler.Mark);
				mark2.Event = ServerProfiler.Mark.Type.Exit;
				mark2.Timestamp = timestamp;
				AddMark(mark2, null, mainProfile.ThreadId, firstMarkTimestamp, builder);
			}
			while (i > 0)
			{
				ServerProfiler.Mark mark5 = default(ServerProfiler.Mark);
				mark5.Event = ServerProfiler.Mark.Type.Exit;
				mark5.Timestamp = num2;
				AddMark(mark5, null, mainProfile.ThreadId, firstMarkTimestamp, builder);
				i--;
			}
			return true;
		}

		private unsafe static bool ProcessWorkerProfile(in ServerProfiler.Profile workerProfile, in ThreadMetadata metadata, long firstMarkTimestamp, StringStream builder)
		{
			uint readInd = 0u;
			int num = 0;
			while (readInd < workerProfile.WriteEnd)
			{
				ServerProfiler.Mark mark = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(workerProfile.Data + readInd);
				if (mark.Timestamp >= firstMarkTimestamp)
				{
					break;
				}
				if (mark.Event == ServerProfiler.Mark.Type.Sync)
				{
					Debug.LogError("Unexpected mark in worker stream, aborting!");
					return false;
				}
				num += AdjustCallstackDepth(in mark);
				if (!AdvanceReadInd(in mark, workerProfile.Data, ref readInd))
				{
					return false;
				}
			}
			if (readInd >= workerProfile.WriteEnd)
			{
				return true;
			}
			bool flag = false;
			uint readInd2 = readInd;
			while (readInd2 < workerProfile.WriteEnd)
			{
				ServerProfiler.Mark mark2 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(workerProfile.Data + readInd2);
				if (mark2.Event == ServerProfiler.Mark.Type.Sync)
				{
					Debug.LogError("Unexpected mark in worker stream, aborting!");
					return false;
				}
				if (mark2.Event != ServerProfiler.Mark.Type.Alloc)
				{
					flag = true;
					break;
				}
				if (!AdvanceReadInd(in mark2, workerProfile.Data, ref readInd2))
				{
					return false;
				}
			}
			if (!flag)
			{
				return true;
			}
			if (!FindStartingDepth(in workerProfile, 0u, workerProfile.WriteEnd, out var minDepth))
			{
				return false;
			}
			minDepth = num - minDepth;
			for (int num2 = minDepth; num2 > 0; num2--)
			{
				ServerProfiler.Mark mark3 = default(ServerProfiler.Mark);
				mark3.Event = ServerProfiler.Mark.Type.Enter;
				mark3.Timestamp = firstMarkTimestamp;
				AddMark(mark3, "Unknown", workerProfile.ThreadId, firstMarkTimestamp, builder);
			}
			uint totalMem = 0u;
			long timestamp = 0L;
			uint readInd3 = readInd;
			while (readInd3 < workerProfile.WriteEnd)
			{
				ServerProfiler.Mark mark4 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(workerProfile.Data + readInd3);
				timestamp = mark4.Timestamp;
				minDepth += AdjustCallstackDepth(in mark4);
				if (minDepth < 0)
				{
					Debug.LogError($"ServerProfiler: Unexpected callstack depth: {minDepth}, offset: {readInd3}, thread: {workerProfile.ThreadId}");
					return false;
				}
				if (mark4.Event == ServerProfiler.Mark.Type.Alloc || (mark4.Event == ServerProfiler.Mark.Type.AllocWithStack && totalMem == 0))
				{
					EmitCounter("Thread", metadata.ThreadId, 0L, "Total(B)", 0uL, builder);
				}
				AddMark(in workerProfile, in metadata, readInd3, ref totalMem, firstMarkTimestamp, builder);
				if (!AdvanceReadInd(in mark4, workerProfile.Data, ref readInd3))
				{
					return false;
				}
			}
			while (minDepth > 0)
			{
				ServerProfiler.Mark mark5 = default(ServerProfiler.Mark);
				mark5.Event = ServerProfiler.Mark.Type.Exit;
				mark5.Timestamp = timestamp;
				AddMark(mark5, null, workerProfile.ThreadId, firstMarkTimestamp, builder);
				minDepth--;
			}
			return true;
		}

		private unsafe static void ProcessMemoryState(in ServerProfiler.MemoryState memState, long firstMarkTimestamp, StringStream builder)
		{
			ulong num = 0uL;
			ulong num2 = 0uL;
			for (uint num3 = 0u; num3 < memState.Created; num3++)
			{
				ServerProfiler.MemoryReading memoryReading = memState.Readings[num3];
				long timestamp = memoryReading.Timestamp;
				if (timestamp >= firstMarkTimestamp)
				{
					ulong num4 = memoryReading.WorkingSet / 1024;
					if (num4 != num)
					{
						long offset = ((num == 0L) ? 0 : ServerProfiler.TimestampToMicros(timestamp - firstMarkTimestamp));
						EmitCounter("ws", offset, "WorkingSet(KB)", num4, builder);
						num = num4;
					}
					ulong num5 = memoryReading.VirtualSet / 1024;
					if (num5 != num2)
					{
						long offset2 = ((num2 == 0L) ? 0 : ServerProfiler.TimestampToMicros(timestamp - firstMarkTimestamp));
						EmitCounter("vs", offset2, "VirtualSet(KB)", num5, builder);
						num2 = num5;
					}
				}
			}
		}

		private unsafe static void AddMark(in ServerProfiler.Profile threadProfile, in ThreadMetadata metadata, uint markInd, ref uint totalMem, long startTimestamp, StringStream builder)
		{
			byte* ptr = threadProfile.Data + markInd;
			ServerProfiler.Mark mark = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(ptr);
			ptr += sizeof(ServerProfiler.Mark);
			switch (mark.Event)
			{
			case ServerProfiler.Mark.Type.Enter:
				builder.Append(",{\"name\":\"");
				ServerProfiler.AppendNameTo(*(ServerProfiler.Native.MonoMethod**)ptr, builder);
				builder.Append("\",\"cat\":\"P\",\"ph\":\"B\",\"ts\":");
				builder.Append(ServerProfiler.TimestampToMicros(mark.Timestamp - startTimestamp));
				builder.Append(",\"pid\":0,\"tid\":");
				builder.Append(threadProfile.ThreadId);
				builder.Append("}");
				break;
			case ServerProfiler.Mark.Type.Exit:
			case ServerProfiler.Mark.Type.Exception:
			case ServerProfiler.Mark.Type.GCEnd:
				builder.Append(",{\"ph\":\"E\",\"ts\":");
				builder.Append(ServerProfiler.TimestampToMicros(mark.Timestamp - startTimestamp));
				builder.Append(",\"pid\":0,\"tid\":");
				builder.Append(threadProfile.ThreadId);
				builder.Append("}");
				break;
			case ServerProfiler.Mark.Type.Alloc:
			{
				ServerProfiler.Alloc alloc2 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Alloc>(ptr);
				long num2 = ServerProfiler.TimestampToMicros(mark.Timestamp - startTimestamp);
				builder.Append(",{\"name\":\"");
				builder.Append("Alloc ");
				builder.Append(alloc2.AlignedSize);
				builder.Append("b\",\"ph\":\"i\",\"ts\":");
				builder.Append(num2);
				builder.Append(",\"pid\":0,\"tid\":");
				builder.Append(threadProfile.ThreadId);
				builder.Append(",\"s\":\"t\",\"cat\":\"A\",\"args\":{\"size\":");
				builder.Append(alloc2.AlignedSize);
				builder.Append(",\"type\":\"");
				ServerProfiler.AppendNameTo(alloc2, builder);
				builder.Append("\",\"lastMethod\":\"");
				if (alloc2.LastMethod != null)
				{
					ServerProfiler.AppendNameTo(alloc2.LastMethod, builder);
				}
				else
				{
					builder.Append("<mono-native-runtime>");
				}
				builder.Append("\"}}");
				builder.Append(",{\"name\":\"");
				builder.Append("Alloc ");
				builder.Append(alloc2.AlignedSize);
				builder.Append("b\",\"ph\":\"i\",\"ts\":");
				builder.Append(num2);
				builder.Append(",\"pid\":0,\"tid\":");
				builder.Append(metadata.AllocationThreadId);
				builder.Append(",\"s\":\"t\",\"cat\":\"A\",\"args\":{\"size\":");
				builder.Append(alloc2.AlignedSize);
				builder.Append(",\"type\":\"");
				ServerProfiler.AppendNameTo(alloc2, builder);
				builder.Append("\",\"lastMethod\":\"");
				if (alloc2.LastMethod != null)
				{
					ServerProfiler.AppendNameTo(alloc2.LastMethod, builder);
				}
				else
				{
					builder.Append("<mono-native-runtime>");
				}
				builder.Append("\"}}");
				totalMem += alloc2.AlignedSize;
				if (metadata.ThreadId == ServerProfiler.GetMainThreadId())
				{
					EmitCounter("Main Thread", num2, "Total(B)", totalMem, builder);
				}
				else
				{
					EmitCounter("Thread", metadata.ThreadId, num2, "Total(B)", totalMem, builder);
				}
				break;
			}
			case ServerProfiler.Mark.Type.AllocWithStack:
			{
				ServerProfiler.Alloc alloc = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Alloc>(ptr);
				ptr += sizeof(ServerProfiler.Alloc);
				long num = ServerProfiler.TimestampToMicros(mark.Timestamp - startTimestamp);
				builder.Append(",{\"name\":\"");
				builder.Append("Alloc ");
				builder.Append(alloc.AlignedSize);
				builder.Append("b\",\"ph\":\"i\",\"ts\":");
				builder.Append(num);
				builder.Append(",\"pid\":0,\"tid\":");
				builder.Append(metadata.ThreadId);
				builder.Append(",\"s\":\"t\",\"cat\":\"A\",\"args\":{\"size\":");
				builder.Append(alloc.AlignedSize);
				builder.Append(",\"type\":\"");
				ServerProfiler.AppendNameTo(alloc, builder);
				builder.Append("\",\"lastMethod\":\"");
				if (alloc.LastMethod != null)
				{
					ServerProfiler.AppendNameTo(alloc.LastMethod, builder);
				}
				else
				{
					builder.Append("<mono-native-runtime>");
				}
				builder.Append('"');
				byte b = *ptr;
				ptr++;
				if (b > 0)
				{
					builder.Append(",\"callstack\":{");
					ServerProfiler.Native.MonoMethod** ptr2 = (ServerProfiler.Native.MonoMethod**)ptr;
					for (byte b2 = 0; b2 < b; b2++)
					{
						ServerProfiler.Native.MonoMethod* method = ptr2[(int)b2];
						builder.Append('"');
						char c = (char)(b2 + 32);
						if (c >= '"')
						{
							c = (char)(c + 1);
						}
						if (c >= '.')
						{
							c = (char)(c + 1);
						}
						if (c >= '\\')
						{
							c = (char)(c + 1);
						}
						if (c < '\u007f')
						{
							builder.Append(c);
						}
						else
						{
							builder.Append(b2);
						}
						builder.Append("\":\"");
						ServerProfiler.AppendNameTo(method, builder);
						builder.Append('"');
						if (b2 != b - 1)
						{
							builder.Append(',');
						}
					}
					builder.Append('}');
				}
				builder.Append("}}");
				totalMem += alloc.AlignedSize;
				if (metadata.ThreadId == ServerProfiler.GetMainThreadId())
				{
					EmitCounter("Main Thread", num, "Total(B)", totalMem, builder);
				}
				else
				{
					EmitCounter("Thread", metadata.ThreadId, num, "Total(B)", totalMem, builder);
				}
				break;
			}
			case ServerProfiler.Mark.Type.GCBegin:
				builder.Append(",{\"name\":\"");
				builder.Append("GC.Collect");
				builder.Append("\",\"cat\":\"P\",\"ph\":\"B\",\"ts\":");
				builder.Append(ServerProfiler.TimestampToMicros(mark.Timestamp - startTimestamp));
				builder.Append(",\"pid\":0,\"tid\":");
				builder.Append(threadProfile.ThreadId);
				builder.Append("}");
				break;
			}
		}

		private static void AddMark(ServerProfiler.Mark mark, string name, int threadId, long startTimestamp, StringStream builder)
		{
			switch (mark.Event)
			{
			case ServerProfiler.Mark.Type.Enter:
				builder.Append(",{\"name\":\"");
				builder.Append(name);
				builder.Append("\",\"cat\":\"P\",\"ph\":\"B\",\"ts\":");
				builder.Append(ServerProfiler.TimestampToMicros(mark.Timestamp - startTimestamp));
				builder.Append(",\"pid\":0,\"tid\":");
				builder.Append(threadId);
				builder.Append("}");
				break;
			case ServerProfiler.Mark.Type.Exit:
			case ServerProfiler.Mark.Type.Exception:
				builder.Append(",{\"ph\":\"E\",\"ts\":");
				builder.Append(ServerProfiler.TimestampToMicros(mark.Timestamp - startTimestamp));
				builder.Append(",\"pid\":0,\"tid\":");
				builder.Append(threadId);
				builder.Append("}");
				break;
			}
		}

		private static void EmitCounter(string markName, long offset, string counterName, ulong value, StringStream builder)
		{
			builder.Append(",{\"name\":\"");
			builder.Append(markName);
			builder.Append("\",\"ph\":\"C\",\"ts\":");
			builder.Append(offset);
			builder.Append(",\"args\":{\"");
			builder.Append(counterName);
			builder.Append("\":");
			builder.Append(value);
			builder.Append("}}");
		}

		private static void EmitCounter(string markName, int threadId, long offset, string counterName, ulong value, StringStream builder)
		{
			builder.Append(",{\"name\":\"");
			builder.Append(markName);
			builder.Append(" ");
			builder.Append(threadId);
			builder.Append("\",\"ph\":\"C\",\"ts\":");
			builder.Append(offset);
			builder.Append(",\"args\":{\"");
			builder.Append(counterName);
			builder.Append("\":");
			builder.Append(value);
			builder.Append("}}");
		}

		private unsafe static bool FindStartingDepth(in ServerProfiler.Profile threadProfile, uint start, uint end, out int minDepth)
		{
			int num = 0;
			minDepth = 0;
			uint readInd = start;
			while (readInd < end)
			{
				ServerProfiler.Mark mark = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(threadProfile.Data + readInd);
				num += AdjustCallstackDepth(in mark);
				if (mark.Event == ServerProfiler.Mark.Type.Exit || mark.Event == ServerProfiler.Mark.Type.Exception)
				{
					minDepth = Math.Min(num, minDepth);
				}
				if (!AdvanceReadInd(in mark, threadProfile.Data, ref readInd))
				{
					return false;
				}
			}
			return true;
		}

		private static int AdjustCallstackDepth(in ServerProfiler.Mark mark)
		{
			switch (mark.Event)
			{
			case ServerProfiler.Mark.Type.Enter:
				return 1;
			case ServerProfiler.Mark.Type.Exit:
			case ServerProfiler.Mark.Type.Exception:
				return -1;
			default:
				return 0;
			}
		}
	}

	public static class Binary
	{
		private enum Section : byte
		{
			Info,
			Thread,
			Marks
		}

		private struct SectionBlock : IDisposable
		{
			private MemoryStream stream;

			private long startPos;

			public static SectionBlock New(Section section, MemoryStream stream)
			{
				SectionBlock result = default(SectionBlock);
				result.stream = stream;
				result.startPos = stream.Position;
				result.Begin(section);
				return result;
			}

			private void Begin(Section section)
			{
				Write(4276993775u);
				Write((byte)section);
				Write(ulong.MaxValue);
			}

			private void End()
			{
				long num = startPos + 5;
				long num2 = stream.Position - num - 8;
				byte[] buffer = stream.GetBuffer();
				for (byte b = 0; b < 8; b++)
				{
					buffer[num + b] = (byte)(num2 >> 56 - b * 8);
				}
				Write(3735928559u);
			}

			public void Write(string text)
			{
				Write((ushort)text.Length);
				for (int i = 0; i < text.Length; i++)
				{
					stream.WriteByte((byte)text[i]);
				}
			}

			public void Write(ulong value)
			{
				for (byte b = 0; b < 8; b++)
				{
					stream.WriteByte((byte)(value >> 56 - b * 8));
				}
			}

			public void Write(uint value)
			{
				for (byte b = 0; b < 4; b++)
				{
					stream.WriteByte((byte)(value >> 24 - b * 8));
				}
			}

			public void Write(ushort value)
			{
				stream.WriteByte((byte)(value >> 8));
				stream.WriteByte((byte)value);
			}

			public void Write(byte value)
			{
				stream.WriteByte(value);
			}

			void IDisposable.Dispose()
			{
				End();
			}
		}

		public unsafe static void Export(string filename, IList<ServerProfiler.Profile> profiles)
		{
			Debug.Log("Starting BIN snapshot generation...");
			MemoryStream memoryStream = new MemoryStream(134217728);
			using (SectionBlock sectionBlock = SectionBlock.New(Section.Info, memoryStream))
			{
				sectionBlock.Write("Nothing");
			}
			foreach (ServerProfiler.Profile profile in profiles)
			{
				using SectionBlock sectionBlock2 = SectionBlock.New(Section.Thread, memoryStream);
				sectionBlock2.Write((uint)profile.ThreadId);
				using SectionBlock sectionBlock3 = SectionBlock.New(Section.Marks, memoryStream);
				uint readInd = 0u;
				while (readInd < profile.WriteEnd)
				{
					byte* ptr = profile.Data + readInd;
					ServerProfiler.Mark mark = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(ptr);
					sectionBlock3.Write((byte)mark.Event);
					sectionBlock3.Write((ulong)mark.Timestamp);
					switch (mark.Event)
					{
					case ServerProfiler.Mark.Type.Enter:
						ServerProfiler.SerializeNameTo(*(ServerProfiler.Native.MonoMethod**)(ptr + sizeof(ServerProfiler.Mark)), memoryStream);
						break;
					case ServerProfiler.Mark.Type.Alloc:
					{
						ServerProfiler.Alloc alloc = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Alloc>(ptr + sizeof(ServerProfiler.Mark));
						ServerProfiler.SerializeNameTo(alloc, memoryStream);
						sectionBlock3.Write(alloc.AlignedSize);
						break;
					}
					}
					AdvanceReadInd(in mark, profile.Data, ref readInd);
				}
			}
			Debug.Log("Generation done, compressing...");
			byte[] array = new byte[memoryStream.Position];
			Buffer.BlockCopy(memoryStream.GetBuffer(), 0, array, 0, (int)memoryStream.Position);
			string text = Path.Join(Server.rootFolder, "profiler");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllBytes(Path.Join(text, filename + ".bin.gz"), Compression.Compress(array));
			Debug.Log("Snapshot bin export done!");
		}
	}

	private const string OutputDir = "profiler";

	private unsafe static void Preprocess(IList<ServerProfiler.Profile> profiles, out MainThreadInfo mainInfo, out uint totalBytes, bool skipToStackStart)
	{
		mainInfo = default(MainThreadInfo);
		totalBytes = 0u;
		uint num = 0u;
		foreach (ServerProfiler.Profile profile in profiles)
		{
			if (profile.ThreadId == ServerProfiler.GetMainThreadId())
			{
				mainInfo.SyncIndices = new List<uint>(10);
				mainInfo.FrameStarts = new List<uint>(10);
				sbyte b = 0;
				sbyte b2 = 0;
				uint num2 = 0u;
				uint num3 = 0u;
				uint readInd = 0u;
				while (readInd < profile.WriteEnd)
				{
					ServerProfiler.Mark mark = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(profile.Data + readInd);
					switch (mark.Event)
					{
					case ServerProfiler.Mark.Type.Enter:
						b++;
						if (num3 <= num2)
						{
							num3 = readInd;
						}
						break;
					case ServerProfiler.Mark.Type.Exit:
					case ServerProfiler.Mark.Type.Exception:
						b--;
						if (b < b2)
						{
							b2 = b;
							num2 = readInd;
						}
						break;
					case ServerProfiler.Mark.Type.Sync:
						mainInfo.SyncIndices.Add(readInd);
						if (skipToStackStart || mainInfo.Frames != 0)
						{
							mainInfo.FrameStarts.Add(num3);
						}
						else
						{
							mainInfo.FrameStarts.Add(0u);
						}
						if (mainInfo.Frames == 0)
						{
							mainInfo.CallstackDepth = b2;
						}
						mainInfo.Frames++;
						b = 0;
						b2 = 0;
						break;
					}
					AdvanceReadInd(in mark, profile.Data, ref readInd);
				}
				if (mainInfo.Frames == 0)
				{
					mainInfo.CallstackDepth = b2;
				}
				uint num4 = 0u;
				long num5 = profile.Timestamp;
				for (int i = 0; i < mainInfo.Frames; i++)
				{
					uint num6 = mainInfo.SyncIndices[i];
					long timestamp = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ServerProfiler.Mark>(profile.Data + num6).Timestamp;
					_ = ServerProfiler.TimestampToTimespan(timestamp - num5).TotalMilliseconds;
					uint num7 = num6 - num4;
					num += num7;
					num5 = timestamp;
					num4 = num6;
				}
				if (mainInfo.Frames == 0)
				{
					num = profile.WriteEnd;
				}
				totalBytes += num;
			}
			else
			{
				totalBytes += profile.WriteEnd;
			}
		}
		Debug.Log("Total data: " + totalBytes.FormatBytes() + " (main: " + num.FormatBytes() + ", workers: " + (totalBytes - num).FormatBytes() + ")");
	}

	public unsafe static bool AdvanceReadInd(in ServerProfiler.Mark mark, byte* data, ref uint readInd)
	{
		readInd += (uint)sizeof(ServerProfiler.Mark);
		switch (mark.Event)
		{
		case ServerProfiler.Mark.Type.Enter:
			readInd += (uint)sizeof(ServerProfiler.Native.MonoMethod*);
			break;
		case ServerProfiler.Mark.Type.Alloc:
			readInd += (uint)sizeof(ServerProfiler.Alloc);
			break;
		case ServerProfiler.Mark.Type.AllocWithStack:
		{
			readInd += (uint)sizeof(ServerProfiler.Alloc);
			byte b = data[readInd];
			readInd++;
			readInd += (uint)(b * sizeof(ServerProfiler.Native.MonoMethod*));
			break;
		}
		default:
			Debug.LogError("Unhandled ServerProfiler.Mark.Type!");
			return false;
		case ServerProfiler.Mark.Type.Sync:
		case ServerProfiler.Mark.Type.Exit:
		case ServerProfiler.Mark.Type.Exception:
		case ServerProfiler.Mark.Type.GCBegin:
		case ServerProfiler.Mark.Type.GCEnd:
			break;
		}
		return true;
	}
}
