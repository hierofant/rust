using System;
using System.Collections.Generic;
using System.Text;

namespace Facepunch;

public readonly struct StringView
{
	public class ComparerIgnoreCase : EqualityComparer<StringView>
	{
		public static ComparerIgnoreCase Instance = new ComparerIgnoreCase();

		public override bool Equals(StringView x, StringView y)
		{
			if (x.Length != y.Length)
			{
				return false;
			}
			for (int i = 0; i != x.Length; i++)
			{
				char c = x._source[x._start + i];
				char c2 = y._source[y._start + i];
				if (c != c2 && ((c | 0x20) != (c2 | 0x20) || (uint)((c | 0x20) - 97) > 25u))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode(StringView obj)
		{
			int num = 5381;
			int num2 = num;
			int start = obj._start;
			while (start != obj._end)
			{
				int num3 = obj._source[start++];
				if ((uint)((num3 | 0x20) - 97) <= 25u)
				{
					num3 |= 0x20;
				}
				num = ((num << 5) + num) ^ num3;
				if (start == obj._end)
				{
					break;
				}
				num3 = obj._source[start++];
				if ((uint)((num3 | 0x20) - 97) <= 25u)
				{
					num3 |= 0x20;
				}
				num2 = ((num2 << 5) + num2) ^ num3;
			}
			return num + num2 * 1566083941;
		}
	}

	private readonly string _source;

	private readonly int _start;

	private readonly int _end;

	public int Length => _end - _start;

	public char this[int index]
	{
		get
		{
			if (index >= Length || index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return _source[_start + index];
		}
	}

	public StringView(string source)
	{
		if (source == null)
		{
			this = default(StringView);
			return;
		}
		_source = source;
		_start = 0;
		_end = _source.Length;
	}

	public StringView(string source, int start)
	{
		if (source == null)
		{
			if (start != 0)
			{
				throw new ArgumentOutOfRangeException("Start can only be 0 for a null string input!");
			}
			this = default(StringView);
			return;
		}
		_source = source;
		_start = start;
		_end = _source.Length;
		if (_start > _end)
		{
			throw new ArgumentOutOfRangeException($"Invalid view arguments: start({_start}) is after end({_end})!");
		}
		if (_start >= 0)
		{
			return;
		}
		throw new ArgumentOutOfRangeException($"Start({_start}) was past the start of string!");
	}

	public StringView(string source, int start, int length)
	{
		if (source == null)
		{
			if (start != 0 || length != 0)
			{
				throw new ArgumentOutOfRangeException("Start and length can only be 0 for a null string input!");
			}
			this = default(StringView);
			return;
		}
		_source = ((source != null) ? source : string.Empty);
		_start = start;
		_end = _start + length;
		if (_start > _end)
		{
			throw new ArgumentOutOfRangeException($"Invalid view arguments: start({_start}) is after end({_end})!");
		}
		if (_end > _source.Length)
		{
			throw new ArgumentOutOfRangeException($"End({_end}) was past the length of string!");
		}
		if (_start >= 0)
		{
			return;
		}
		throw new ArgumentOutOfRangeException($"Start({_start}) was past the start of string!");
	}

	public static implicit operator StringView(string source)
	{
		return new StringView(source);
	}

	public static implicit operator ReadOnlySpan<char>(StringView view)
	{
		return view._source.AsSpan(view._start, view.Length);
	}

	public static implicit operator ReadOnlyMemory<char>(StringView view)
	{
		return view._source.AsMemory(view._start, view.Length);
	}

	public static explicit operator string(StringView view)
	{
		return view.ToString();
	}

	public override string ToString()
	{
		return _source?.Substring(_start, Length) ?? string.Empty;
	}

	public override int GetHashCode()
	{
		int num = 5381;
		int num2 = num;
		int start = _start;
		while (start != _end)
		{
			int num3 = _source[start++];
			num = ((num << 5) + num) ^ num3;
			if (start == _end)
			{
				break;
			}
			num3 = _source[start++];
			num2 = ((num2 << 5) + num2) ^ num3;
		}
		return num + num2 * 1566083941;
	}

	public override bool Equals(object obj)
	{
		if (obj is StringView otherSv)
		{
			return Equals(otherSv);
		}
		if (obj is string other)
		{
			return Equals(other);
		}
		throw new ArgumentException($"Unsupported type for equality check! Other object was {obj.GetType()}");
	}

	public bool Equals(string other)
	{
		return Equals((StringView)other);
	}

	public bool Equals(string other, StringComparison comparisonOptions)
	{
		if (other == null)
		{
			return Length == 0;
		}
		if (Length != other.Length)
		{
			return false;
		}
		if (Length == 0)
		{
			return true;
		}
		return string.Compare(_source, _start, other, 0, Length, comparisonOptions) == 0;
	}

	public bool Equals(StringView otherSv)
	{
		if (Length != otherSv.Length)
		{
			return false;
		}
		for (int i = 0; i != Length; i++)
		{
			if (_source[_start + i] != otherSv._source[otherSv._start + i])
			{
				return false;
			}
		}
		return true;
	}

	public bool Equals(StringView otherSv, StringComparison comparisonOptions)
	{
		if (Length != otherSv.Length)
		{
			return false;
		}
		if (Length == 0)
		{
			return true;
		}
		return string.Compare(_source, _start, otherSv._source, otherSv._start, Length, comparisonOptions) == 0;
	}

	public static bool operator ==(StringView left, StringView right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(StringView left, StringView right)
	{
		return !left.Equals(right);
	}

	public void Split(char delim, ICollection<StringView> collection)
	{
		int num = _start;
		for (int i = _start; i != _end; i++)
		{
			if (_source[i] == delim)
			{
				if (num != i)
				{
					collection.Add(new StringView(_source, num, i - num));
				}
				num = i + 1;
			}
		}
		if (num != _end)
		{
			collection.Add(new StringView(_source, num, _end - num));
		}
	}

	public bool StartsWith(StringView other)
	{
		if (other.Length > Length)
		{
			return false;
		}
		return Substring(0, other.Length).Equals(other);
	}

	public bool StartsWith(StringView other, StringComparison comp)
	{
		if (other.Length > Length)
		{
			return false;
		}
		return Substring(0, other.Length).Equals(other, comp);
	}

	public bool EndsWith(StringView other)
	{
		if (other.Length > Length)
		{
			return false;
		}
		return Substring(Length - other.Length).Equals(other);
	}

	public bool EndsWith(StringView other, StringComparison comp)
	{
		if (other.Length > Length)
		{
			return false;
		}
		return Substring(Length - other.Length).Equals(other, comp);
	}

	public StringView Substring(int offset)
	{
		if (offset > Length || offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		return new StringView(_source, _start + offset, Length - offset);
	}

	public StringView Substring(int offset, int length)
	{
		if (offset > Length || offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (_start + offset + length > _end)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		return new StringView(_source, _start + offset, length);
	}

	public StringView Slice(int offset, int length)
	{
		return Substring(offset, length);
	}

	public bool Contains(StringView other)
	{
		return IndexOf(other) > -1;
	}

	public int IndexOf(StringView other)
	{
		if (other.Length > Length)
		{
			return -1;
		}
		if (other.Length == 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < Length; i++)
		{
			if (_source[_start + i] == other._source[other._start + num])
			{
				num++;
				if (num == other._end - other._start)
				{
					return i - num + 1;
				}
			}
			else
			{
				if (num > 0)
				{
					i--;
				}
				num = 0;
			}
		}
		return -1;
	}

	public int IndexOf(char other)
	{
		for (int i = 0; i < Length; i++)
		{
			if (_source[_start + i] == other)
			{
				return i;
			}
		}
		return -1;
	}

	public int IndexOfAny(StringView anyCharsView)
	{
		if (anyCharsView.Length == 0)
		{
			return -1;
		}
		for (int i = 0; i < Length; i++)
		{
			char other = _source[_start + i];
			if (anyCharsView.IndexOf(other) != -1)
			{
				return i;
			}
		}
		return -1;
	}

	public StringView Replace(StringView oldVal, StringView newVal)
	{
		if (oldVal.Length == 0)
		{
			throw new ArgumentException("oldVal must be non-empty!");
		}
		int num = IndexOf(oldVal);
		if (num == -1)
		{
			return this;
		}
		List<int> obj = Pool.Get<List<int>>();
		obj.Add(num);
		StringView stringView = Substring(num + oldVal.Length);
		while (true)
		{
			num = stringView.IndexOf(oldVal);
			if (num == -1)
			{
				break;
			}
			obj.Add(num);
			stringView = stringView.Substring(num + oldVal.Length);
		}
		StringBuilder obj2 = Pool.Get<StringBuilder>();
		obj2.EnsureCapacity(Length + obj.Count * (newVal.Length - oldVal.Length));
		stringView = this;
		foreach (int item in obj)
		{
			obj2.Append(stringView.Substring(0, item));
			obj2.Append(newVal);
			stringView = stringView.Substring(item + oldVal.Length);
		}
		obj2.Append(stringView);
		StringView result = obj2.ToString();
		Pool.FreeUnmanaged(ref obj2);
		Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public StringView Trim()
	{
		int i;
		for (i = _start; i < _end && char.IsWhiteSpace(_source[i]); i++)
		{
		}
		int num = _end - 1;
		while (num > i && char.IsWhiteSpace(_source[num]))
		{
			num--;
		}
		return Substring(i - _start, num + 1 - i);
	}

	public StringView Trim(char c)
	{
		int i;
		for (i = _start; i < _end && _source[i] == c; i++)
		{
		}
		int num = _end - 1;
		while (num > i && _source[num] == c)
		{
			num--;
		}
		return Substring(i - _start, num + 1 - i);
	}

	public StringView Trim(char c1, char c2)
	{
		int i;
		for (i = _start; i < _end && (_source[i] == c1 || _source[i] == c2); i++)
		{
		}
		int num = _end - 1;
		while (num > i && (_source[num] == c1 || _source[num] == c2))
		{
			num--;
		}
		return Substring(i - _start, num + 1 - i);
	}

	public StringView Trim(char c1, char c2, char c3)
	{
		int i;
		for (i = _start; i < _end && (_source[i] == c1 || _source[i] == c2 || _source[i] == c3); i++)
		{
		}
		int num = _end - 1;
		while (num > i && (_source[num] == c1 || _source[num] == c2 || _source[num] == c3))
		{
			num--;
		}
		return Substring(i - _start, num + 1 - i);
	}
}
