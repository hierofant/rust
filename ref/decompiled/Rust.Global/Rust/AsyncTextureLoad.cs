using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Rust;

public class AsyncTextureLoad : CustomYieldInstruction
{
	private IntPtr buffer = IntPtr.Zero;

	private byte[] uncompressedTextureBytes;

	private int size;

	private int width;

	private int height;

	private int format;

	public string filename;

	public bool normal;

	public bool dither;

	public bool hqmode;

	public bool cache;

	public bool compressOnLoad;

	private Action worker;

	public override bool keepWaiting => worker != null;

	public bool isDone => worker == null;

	public bool isValid
	{
		get
		{
			if (buffer == IntPtr.Zero && uncompressedTextureBytes == null)
			{
				return false;
			}
			if (size == 0)
			{
				return false;
			}
			if (format == 0)
			{
				return false;
			}
			if (width < 32 || width > 8192 || !Mathf.IsPowerOfTwo(width))
			{
				return false;
			}
			if (height < 32 || height > 8192 || !Mathf.IsPowerOfTwo(height))
			{
				return false;
			}
			if (format != 12 && format != 10 && format != 4)
			{
				return false;
			}
			return true;
		}
	}

	public Texture2D texture
	{
		get
		{
			if (!isValid)
			{
				return null;
			}
			Texture2D texture2D = new Texture2D(width, height, (TextureFormat)format, mipChain: true, normal);
			if (compressOnLoad)
			{
				texture2D.LoadRawTextureData(buffer, size);
				texture2D.Apply(updateMipmaps: false);
				FreeTexture(ref buffer);
				return texture2D;
			}
			if (!texture2D.LoadImage(uncompressedTextureBytes))
			{
				Debug.LogError("Failed to load uncompressedTextureBytes into image!");
			}
			return texture2D;
		}
	}

	public void LoadIntoTexture(Texture2D tex)
	{
		if (isValid && tex.width == width && tex.height == height && tex.format == (TextureFormat)format)
		{
			if (compressOnLoad)
			{
				tex.LoadRawTextureData(buffer, size);
			}
			else if (!tex.LoadImage(uncompressedTextureBytes))
			{
				Debug.LogError("Failed to load uncompressedTextureBytes into image!");
				return;
			}
			tex.Apply(updateMipmaps: false);
			FreeTexture(ref buffer);
		}
	}

	public void WriteToCache(string cachename)
	{
		SaveTextureToCache(cachename, buffer, size, width, height, format);
	}

	[DllImport("RustNative", EntryPoint = "free_texture")]
	private static extern void FreeTexture(ref IntPtr buffer);

	[DllImport("RustNative", EntryPoint = "load_texture_from_file")]
	private static extern void LoadTextureFromFile(string filename, ref IntPtr buffer, ref int size, ref int width, ref int height, ref int channels, bool normal, bool dither, bool hqmode);

	[DllImport("RustNative", EntryPoint = "load_texture_from_cache")]
	private static extern void LoadTextureFromCache(string filename, ref IntPtr buffer, ref int size, ref int width, ref int height, ref int format);

	[DllImport("RustNative", EntryPoint = "save_texture_to_cache")]
	private static extern void SaveTextureToCache(string filename, IntPtr buffer, int size, int width, int height, int format);

	public AsyncTextureLoad(string filename, bool normal, bool dither, bool hqmode, bool cache, bool compressOnLoad = true)
	{
		this.filename = filename;
		this.normal = normal;
		this.dither = dither;
		this.hqmode = hqmode;
		this.cache = cache;
		this.compressOnLoad = compressOnLoad;
		Invoke();
	}

	private void DoWork()
	{
		if (cache)
		{
			LoadTextureFromCache(filename, ref buffer, ref size, ref width, ref height, ref format);
			return;
		}
		int channels = 0;
		if (compressOnLoad)
		{
			LoadTextureFromFile(filename, ref buffer, ref size, ref width, ref height, ref channels, normal, dither, hqmode);
			format = ((channels > 3) ? 12 : 10);
			return;
		}
		LoadTextureFromFile(filename, ref buffer, ref size, ref width, ref height, ref channels, normal, dither, hqmode);
		FreeTexture(ref buffer);
		uncompressedTextureBytes = File.ReadAllBytes(filename);
		if (uncompressedTextureBytes == null)
		{
			throw new Exception("Failed to load uncompressed texture data!");
		}
		format = 4;
		size = width * height * 32;
	}

	private void Invoke()
	{
		worker = DoWork;
		worker.BeginInvoke(Callback, null);
	}

	private void Callback(IAsyncResult result)
	{
		worker.EndInvoke(result);
		worker = null;
	}
}
