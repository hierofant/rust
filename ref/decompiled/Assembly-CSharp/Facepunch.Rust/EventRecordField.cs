using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cysharp.Text;
using UnityEngine;

namespace Facepunch.Rust;

public struct EventRecordField
{
	[StructLayout(LayoutKind.Explicit)]
	public struct ValueUnion
	{
		[FieldOffset(0)]
		public long Number;

		[FieldOffset(0)]
		public double Float;

		[FieldOffset(0)]
		public Vector3 Vector;

		[FieldOffset(0)]
		public Guid Guid;

		[FieldOffset(0)]
		public DateTime DateTime;
	}

	public string Key1;

	public string Key2;

	public string String;

	public ReadOnlyMemory<char> Chars;

	public MemoryStream Bytes;

	public List<int> Ints;

	public ValueUnion Value;

	public FieldType Type;

	public bool IsObject;

	public EventRecordField(string key1)
	{
		Key1 = key1;
		Key2 = null;
		Type = FieldType.None;
		IsObject = false;
		String = null;
		Bytes = null;
		Chars = default(ReadOnlyMemory<char>);
		Ints = null;
		Value = default(ValueUnion);
	}

	public EventRecordField(string key1, string key2)
	{
		Key1 = key1;
		Key2 = key2;
		Type = FieldType.None;
		IsObject = false;
		String = null;
		Bytes = null;
		Chars = default(ReadOnlyMemory<char>);
		Ints = null;
		Value = default(ValueUnion);
	}

	public void Serialize(ref Utf8ValueStringBuilder writer, AnalyticsDocumentMode format)
	{
		switch (Type)
		{
		case FieldType.String:
		{
			if (String == null)
			{
				break;
			}
			if (IsObject)
			{
				writer.Append(String);
				break;
			}
			string @string = String;
			int length = String.Length;
			for (int j = 0; j < length; j++)
			{
				char c2 = @string[j];
				WriteChar(ref writer, c2);
			}
			break;
		}
		case FieldType.Float:
		{
			Span<char> destination = stackalloc char[128];
			Value.Float.TryFormat(destination, out var charsWritten);
			writer.Append((ReadOnlySpan<char>)destination.Slice(0, charsWritten));
			break;
		}
		case FieldType.Number:
			writer.Append(Value.Number);
			break;
		case FieldType.Guid:
			writer.Append(format: new StandardFormat('N'), value: Value.Guid);
			break;
		case FieldType.Vector:
		{
			Span<char> destination2 = stackalloc char[128];
			Vector3 vector = Value.Vector;
			vector.x.TryFormat(destination2, out var charsWritten2);
			writer.Append((ReadOnlySpan<char>)destination2.Slice(0, charsWritten2));
			writer.Append(',');
			vector.y.TryFormat(destination2, out charsWritten2);
			writer.Append((ReadOnlySpan<char>)destination2.Slice(0, charsWritten2));
			writer.Append(',');
			vector.z.TryFormat(destination2, out charsWritten2);
			writer.Append((ReadOnlySpan<char>)destination2.Slice(0, charsWritten2));
			break;
		}
		case FieldType.DateTime:
			writer.Append(Value.DateTime, StandardFormats.DateTime_ISO);
			break;
		case FieldType.Bytes:
			if (Bytes != null)
			{
				Span<char> chars = stackalloc char[128];
				int num = 96;
				byte[] buffer = Bytes.GetBuffer();
				for (int l = 0; l < Bytes.Length; l += num)
				{
					int num2 = (int)Bytes.Length - l;
					int length3 = ((num2 > num) ? num : num2);
					Convert.TryToBase64Chars(new Span<byte>(buffer, l, length3), chars, out var charsWritten3);
					Span<char> span2 = chars.Slice(0, charsWritten3);
					writer.Append((ReadOnlySpan<char>)span2);
				}
			}
			break;
		case FieldType.Chars:
		{
			if (IsObject)
			{
				writer.Append(Chars.Span);
				break;
			}
			int length2 = Chars.Length;
			ReadOnlySpan<char> span = Chars.Span;
			for (int k = 0; k < length2; k++)
			{
				char c3 = span[k];
				WriteChar(ref writer, c3);
			}
			break;
		}
		case FieldType.JsonIntArray:
			writer.Append('[');
			if (Ints != null)
			{
				int count = Ints.Count;
				for (int i = 0; i < count; i++)
				{
					if (i > 0)
					{
						writer.Append(',');
					}
					writer.Append(Ints[i]);
				}
			}
			writer.Append(']');
			break;
		default:
			Debug.LogWarning("Unhandled field type attempted to be serialized");
			break;
		}
		void WriteChar(ref Utf8ValueStringBuilder writer, char c)
		{
			if (c == '\\' && format == AnalyticsDocumentMode.JSON)
			{
				writer.Append("\\\\");
			}
			else
			{
				switch (c)
				{
				case '"':
					if (format == AnalyticsDocumentMode.JSON)
					{
						writer.Append("\\\"");
					}
					else
					{
						writer.Append("\"\"");
					}
					break;
				case '\n':
					writer.Append("\\n");
					break;
				case '\r':
					writer.Append("\\r");
					break;
				case '\t':
					writer.Append("\\t");
					break;
				default:
					writer.Append(c);
					break;
				}
			}
		}
	}
}
