using System;
using System.Collections.Generic;
using ConVar;

namespace Windows;

public class ConsoleInput
{
	public string inputString = "";

	private int caretPosition;

	private int scrollOffset;

	public string[] statusText = new string[3] { "", "", "" };

	private LinkedList<string> history = new LinkedList<string>();

	private int historyCount;

	private LinkedListNode<string> lastSelected;

	private string pendingInput = "";

	public int StatusLineCount
	{
		get
		{
			if (statusText != null)
			{
				return statusText.Length;
			}
			return 0;
		}
	}

	public int InputCursorTop => StatusCursorTop - 1;

	public int StatusCursorTop => System.Console.BufferHeight - StatusLineCount - 1;

	public bool valid => System.Console.BufferWidth > 0;

	public int lineWidth => System.Console.BufferWidth;

	public event Action<string> OnInputText;

	public string GetNext()
	{
		if (historyCount == 0)
		{
			return string.Empty;
		}
		if (lastSelected == null)
		{
			pendingInput = inputString;
			lastSelected = history.First;
		}
		else if (lastSelected.Next != null)
		{
			lastSelected = lastSelected.Next;
		}
		return lastSelected.Value;
	}

	public string GetPrevious()
	{
		if (historyCount == 0 || lastSelected == null)
		{
			return string.Empty;
		}
		if (lastSelected.Previous == null)
		{
			lastSelected = null;
			return pendingInput;
		}
		lastSelected = lastSelected.Previous;
		return lastSelected.Value;
	}

	public void AddToHistory(string value)
	{
		if (value.Length == 0)
		{
			return;
		}
		if (history.First?.Value == value)
		{
			lastSelected = null;
			return;
		}
		if (historyCount >= ConVar.Console.consolehistorysize)
		{
			history.AddFirst(value);
			history.RemoveLast();
		}
		else
		{
			history.AddFirst(value);
			historyCount++;
		}
		lastSelected = null;
	}

	public void TrimHistory(int maxSize)
	{
		if (maxSize < 0)
		{
			maxSize = 0;
		}
		while (historyCount > maxSize && history.Last != null)
		{
			history.RemoveLast();
			historyCount--;
		}
		lastSelected = null;
	}

	public void ClearLine(int numLines)
	{
		System.Console.CursorLeft = 0;
		System.Console.Write(new string(' ', lineWidth * numLines));
		System.Console.CursorTop -= numLines;
		System.Console.CursorLeft = 0;
	}

	public void RedrawInputLine(bool clear = true)
	{
		ConsoleColor backgroundColor = System.Console.BackgroundColor;
		ConsoleColor foregroundColor = System.Console.ForegroundColor;
		System.Console.CursorVisible = false;
		try
		{
			if (clear)
			{
				ClearLine(1);
			}
			System.Console.ForegroundColor = ConsoleColor.White;
			System.Console.CursorLeft = 0;
			System.Console.BackgroundColor = ConsoleColor.Black;
			System.Console.ForegroundColor = ConsoleColor.Green;
			if (inputString.Length == 0)
			{
				System.Console.BackgroundColor = backgroundColor;
				System.Console.ForegroundColor = foregroundColor;
				System.Console.CursorLeft = 0;
				return;
			}
			int num = lineWidth - 2;
			if (inputString.Length <= num)
			{
				scrollOffset = 0;
			}
			else
			{
				if (caretPosition < scrollOffset)
				{
					scrollOffset = caretPosition;
				}
				else if (caretPosition > scrollOffset + num)
				{
					scrollOffset = caretPosition - num;
				}
				int num2 = inputString.Length - num;
				if (scrollOffset > num2)
				{
					scrollOffset = num2;
				}
				if (scrollOffset < 0)
				{
					scrollOffset = 0;
				}
			}
			System.Console.Write(inputString.Substring(scrollOffset, Math.Min(num, inputString.Length - scrollOffset)));
			System.Console.CursorLeft = caretPosition - scrollOffset;
		}
		catch (Exception)
		{
		}
		System.Console.BackgroundColor = backgroundColor;
		System.Console.ForegroundColor = foregroundColor;
		System.Console.CursorVisible = true;
	}

	public void RedrawStatusText()
	{
		ConsoleColor backgroundColor = System.Console.BackgroundColor;
		ConsoleColor foregroundColor = System.Console.ForegroundColor;
		int cursorTop = System.Console.CursorTop;
		int cursorLeft = System.Console.CursorLeft;
		try
		{
			System.Console.CursorTop++;
			System.Console.ForegroundColor = ConsoleColor.White;
			for (int i = 0; i < statusText.Length; i++)
			{
				System.Console.CursorLeft = 0;
				System.Console.Write(statusText[i].PadRight(lineWidth));
			}
		}
		catch
		{
		}
		System.Console.BackgroundColor = backgroundColor;
		System.Console.ForegroundColor = foregroundColor;
		try
		{
			System.Console.CursorTop = cursorTop;
			System.Console.CursorLeft = cursorLeft;
		}
		catch
		{
		}
	}

	public void FixBottomOfBuffer()
	{
		try
		{
			System.Console.CursorTop = InputCursorTop;
		}
		catch
		{
		}
	}

	private int PrevWordBoundary(int from)
	{
		int num = from;
		while (num > 0 && char.IsWhiteSpace(inputString[num - 1]))
		{
			num--;
		}
		while (num > 0 && !char.IsWhiteSpace(inputString[num - 1]))
		{
			num--;
		}
		return num;
	}

	private int NextWordBoundary(int from)
	{
		int i = from;
		int length;
		for (length = inputString.Length; i < length && !char.IsWhiteSpace(inputString[i]); i++)
		{
		}
		for (; i < length && char.IsWhiteSpace(inputString[i]); i++)
		{
		}
		return i;
	}

	internal void OnBackspace()
	{
		if (caretPosition >= 1)
		{
			inputString = inputString.Remove(caretPosition - 1, 1);
			caretPosition--;
			RedrawInputLine();
		}
	}

	internal void OnDelete()
	{
		if (caretPosition < inputString.Length)
		{
			inputString = inputString.Remove(caretPosition, 1);
			RedrawInputLine();
		}
	}

	internal void OnEscape()
	{
		inputString = "";
		caretPosition = 0;
		RedrawInputLine();
	}

	internal void OnEnter()
	{
		AddToHistory(inputString);
		ClearLine(statusText.Length);
		ConsoleColor foregroundColor = System.Console.ForegroundColor;
		System.Console.ForegroundColor = ConsoleColor.Green;
		System.Console.WriteLine("> " + inputString);
		System.Console.ForegroundColor = foregroundColor;
		string obj = inputString;
		inputString = "";
		caretPosition = 0;
		if (this.OnInputText != null)
		{
			this.OnInputText(obj);
		}
		RedrawInputLine();
	}

	public void Update()
	{
		if (!valid)
		{
			return;
		}
		try
		{
			if (!System.Console.KeyAvailable)
			{
				return;
			}
		}
		catch (Exception)
		{
			return;
		}
		ConsoleKeyInfo consoleKeyInfo = System.Console.ReadKey();
		if (consoleKeyInfo.Key == ConsoleKey.UpArrow)
		{
			string next = GetNext();
			if (!string.IsNullOrEmpty(next))
			{
				inputString = next;
				caretPosition = inputString.Length;
				RedrawInputLine();
			}
			return;
		}
		if (consoleKeyInfo.Key == ConsoleKey.DownArrow)
		{
			if (lastSelected != null)
			{
				inputString = GetPrevious();
				caretPosition = inputString.Length;
				RedrawInputLine();
			}
			return;
		}
		bool flag = (consoleKeyInfo.Modifiers & ConsoleModifiers.Control) != 0;
		if (consoleKeyInfo.Key == ConsoleKey.LeftArrow)
		{
			if (caretPosition > 0)
			{
				caretPosition = (flag ? PrevWordBoundary(caretPosition) : (caretPosition - 1));
				RedrawInputLine(clear: false);
			}
		}
		else if (consoleKeyInfo.Key == ConsoleKey.RightArrow)
		{
			if (caretPosition < inputString.Length)
			{
				caretPosition = (flag ? NextWordBoundary(caretPosition) : (caretPosition + 1));
				RedrawInputLine(clear: false);
			}
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Home)
		{
			caretPosition = 0;
			RedrawInputLine(clear: false);
		}
		else if (consoleKeyInfo.Key == ConsoleKey.End)
		{
			caretPosition = inputString.Length;
			RedrawInputLine(clear: false);
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Enter)
		{
			OnEnter();
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Backspace)
		{
			OnBackspace();
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Delete)
		{
			OnDelete();
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Escape)
		{
			OnEscape();
		}
		else if (consoleKeyInfo.KeyChar != 0)
		{
			bool flag2 = caretPosition == inputString.Length;
			inputString = inputString.Insert(caretPosition, consoleKeyInfo.KeyChar.ToString());
			caretPosition++;
			RedrawInputLine(!flag2);
		}
	}
}
