#define UNITY_ASSERTIONS
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using ConVar;
using Cysharp.Text;
using Network;
using UnityEngine;

namespace Facepunch.Rust;

public class AnalyticsManager
{
	public interface IAccumulator
	{
		int ItemsWritten { get; }

		long BytesWritten { get; }

		bool HasPending { get; }

		void Accumulate(AnalyticsTable table, EventRecord record);

		MemoryStream Consume(AnalyticsTable table);

		void Recycle(MemoryStream stream);

		void Clear();

		void ResetStats();

		string ConsumeLogs();
	}

	private abstract class BaseAccumulator : IAccumulator
	{
		private const int BufferSize = 10485760;

		private ConcurrentQueue<MemoryStream> availableBuffers = new ConcurrentQueue<MemoryStream>();

		private MemoryStream buffer;

		private GZipStream compressStream;

		private Utf8ValueStringBuilder stringBuilder;

		private string logString = string.Empty;

		private long streamLength;

		private long bytesWritten;

		private int itemsWritten;

		protected abstract bool ShouldCompress { get; }

		public long BytesWritten => bytesWritten;

		public int ItemsWritten => itemsWritten;

		public bool HasPending => buffer != null;

		public void Accumulate(AnalyticsTable table, EventRecord record)
		{
			if (!HasPending)
			{
				Start(table);
			}
			AccumulateImpl(table, record, ref stringBuilder);
			stringBuilder.WriteTo((compressStream != null) ? ((Stream)compressStream) : ((Stream)buffer));
			if (Analytics.Log)
			{
				logString += stringBuilder.ToString();
			}
			stringBuilder.Clear();
			itemsWritten++;
			bytesWritten += buffer.Length - streamLength;
			streamLength = buffer.Length;
		}

		private void Start(AnalyticsTable table)
		{
			if (buffer == null)
			{
				if (!availableBuffers.TryDequeue(out buffer))
				{
					buffer = new MemoryStream(10485760);
				}
				if (ShouldCompress)
				{
					compressStream = new GZipStream(buffer, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true);
				}
			}
			stringBuilder = ZString.CreateUtf8StringBuilder();
			streamLength = buffer.Length;
			StartImpl(table, ref stringBuilder);
		}

		private void End(AnalyticsTable table)
		{
			EndImpl(table, ref stringBuilder);
			if (stringBuilder.Length > 0)
			{
				stringBuilder.WriteTo((compressStream != null) ? ((Stream)compressStream) : ((Stream)buffer));
				if (Analytics.Log)
				{
					logString += stringBuilder.ToString();
				}
			}
			stringBuilder.Dispose();
			bytesWritten += buffer.Length - streamLength;
			streamLength = 0L;
			if (compressStream != null)
			{
				long length = buffer.Length;
				compressStream.Dispose();
				compressStream = null;
				bytesWritten += buffer.Length - length;
			}
		}

		protected virtual void StartImpl(AnalyticsTable table, ref Utf8ValueStringBuilder builder)
		{
		}

		protected abstract void AccumulateImpl(AnalyticsTable table, EventRecord record, ref Utf8ValueStringBuilder builder);

		protected virtual void EndImpl(AnalyticsTable table, ref Utf8ValueStringBuilder builder)
		{
		}

		public MemoryStream Consume(AnalyticsTable table)
		{
			End(table);
			MemoryStream memoryStream = buffer;
			buffer = null;
			memoryStream.Position = 0L;
			return memoryStream;
		}

		public void Recycle(MemoryStream stream)
		{
			stream.SetLength(0L);
			availableBuffers.Enqueue(stream);
		}

		public void Clear()
		{
			if (HasPending)
			{
				compressStream?.Dispose();
				compressStream = null;
				Recycle(buffer);
				buffer = null;
				stringBuilder.Dispose();
			}
			streamLength = 0L;
		}

		public void ResetStats()
		{
			itemsWritten = 0;
			bytesWritten = 0L;
			streamLength = 0L;
		}

		public string ConsumeLogs()
		{
			string result = logString;
			logString = string.Empty;
			return result;
		}
	}

	private class CsvAccumulator : BaseAccumulator
	{
		protected override bool ShouldCompress => false;

		protected override void AccumulateImpl(AnalyticsTable table, EventRecord record, ref Utf8ValueStringBuilder builder)
		{
			record.SerializeAsCSV(ref builder);
			builder.AppendLine();
		}
	}

	private class JsonAccumulator : BaseAccumulator
	{
		private int writtenCount;

		protected override bool ShouldCompress => true;

		protected override void StartImpl(AnalyticsTable table, ref Utf8ValueStringBuilder builder)
		{
			builder.Append('[');
			writtenCount = 0;
		}

		protected override void AccumulateImpl(AnalyticsTable table, EventRecord record, ref Utf8ValueStringBuilder builder)
		{
			if (writtenCount > 0)
			{
				builder.Append(',');
			}
			record.SerializeAsJson(ref builder, table.UseJsonDataObject);
			writtenCount++;
		}

		protected override void EndImpl(AnalyticsTable table, ref Utf8ValueStringBuilder builder)
		{
			builder.Append(']');
		}
	}

	private class BulkAccumulator : BaseAccumulator
	{
		protected override bool ShouldCompress => true;

		protected override void AccumulateImpl(AnalyticsTable table, EventRecord record, ref Utf8ValueStringBuilder builder)
		{
			switch (table.Mode)
			{
			case AnalyticsDocumentMode.JSON:
				record.SerializeAsJson(ref builder, table.UseJsonDataObject);
				break;
			case AnalyticsDocumentMode.CSV:
				record.SerializeAsCSV(ref builder);
				break;
			default:
				throw new NotImplementedException($"Not implemented: {table.Mode}");
			}
			builder.AppendLine();
		}
	}

	public interface IUploader
	{
		bool Enabled { get; }

		string Name { get; }

		bool IsCompressed { get; }

		long BytesUploaded { get; }

		int ItemsSerialized { get; }

		long BytesSerialized { get; }

		IAccumulator GetAccumulatorFor(AnalyticsTable table);

		Task Upload(AnalyticsTable table, MemoryStream stream);

		void ResetStats();

		IUploader Resolve();
	}

	private class NullUploaderImpl : IUploader
	{
		private JsonAccumulator accumulator = new JsonAccumulator();

		private TimeSpan simulatedUploadTime;

		private long bytesUploaded;

		public bool Enabled => true;

		public string Name => "Null";

		public bool IsCompressed => false;

		public long BytesUploaded => bytesUploaded;

		public int ItemsSerialized => accumulator.ItemsWritten;

		public long BytesSerialized => accumulator.BytesWritten;

		public NullUploaderImpl(TimeSpan simulatedUploadTime = default(TimeSpan))
		{
			this.simulatedUploadTime = simulatedUploadTime;
		}

		public IAccumulator GetAccumulatorFor(AnalyticsTable table)
		{
			return accumulator;
		}

		public async Task Upload(AnalyticsTable table, MemoryStream stream)
		{
			if (Analytics.Log || Analytics.DryRun)
			{
				string text = $"Data: {stream.Length} bytes\n";
				int num = (int)System.Math.Min(stream.Length, 256L);
				string text2 = "";
				byte[] buffer = stream.GetBuffer();
				for (int i = 0; i < num; i++)
				{
					string text3 = text2;
					char c = (char)buffer[i];
					text2 = text3 + c;
				}
				text += text2;
				Debug.Log(text);
			}
			if (simulatedUploadTime != default(TimeSpan))
			{
				await Task.Delay(simulatedUploadTime);
			}
			bytesUploaded += stream.Length;
			accumulator.Recycle(stream);
		}

		public void ResetStats()
		{
			bytesUploaded = 0L;
			accumulator.ResetStats();
		}

		public IUploader Resolve()
		{
			return this;
		}
	}

	private class FPUploaderImpl : IUploader
	{
		private static readonly MediaTypeHeaderValue JsonContentType = new MediaTypeHeaderValue("application/json")
		{
			CharSet = Encoding.UTF8.WebName
		};

		private Dictionary<AnalyticsTable, JsonAccumulator> accumulators = new Dictionary<AnalyticsTable, JsonAccumulator>();

		private HttpClient httpClient = new HttpClient();

		private long bytesUploaded;

		private bool isClient;

		public bool Enabled
		{
			get
			{
				if (!Analytics.DryRun)
				{
					if (!isClient)
					{
						return (Application.Manifest?.Features?.ServerAnalytics).GetValueOrDefault();
					}
					if ((Application.Manifest?.Features?.ClientAnalytics).GetValueOrDefault())
					{
						return PlatformService.Instance.IsValid;
					}
					return false;
				}
				return true;
			}
		}

		public string Name
		{
			get
			{
				if (!isClient)
				{
					return "FPApi(server)";
				}
				return "FPApi(client)";
			}
		}

		public bool IsCompressed => true;

		public long BytesUploaded => bytesUploaded;

		public int ItemsSerialized
		{
			get
			{
				int num = 0;
				foreach (KeyValuePair<AnalyticsTable, JsonAccumulator> accumulator in accumulators)
				{
					accumulator.Deconstruct(out var _, out var value);
					JsonAccumulator jsonAccumulator = value;
					num += jsonAccumulator.ItemsWritten;
				}
				return num;
			}
		}

		public long BytesSerialized
		{
			get
			{
				long num = 0L;
				foreach (KeyValuePair<AnalyticsTable, JsonAccumulator> accumulator in accumulators)
				{
					accumulator.Deconstruct(out var _, out var value);
					JsonAccumulator jsonAccumulator = value;
					num += jsonAccumulator.BytesWritten;
				}
				return num;
			}
		}

		public FPUploaderImpl(bool isClient)
		{
			this.isClient = isClient;
		}

		public IAccumulator GetAccumulatorFor(AnalyticsTable table)
		{
			if (accumulators.TryGetValue(table, out var value))
			{
				return value;
			}
			if (table.Mode != 0)
			{
				Debug.LogWarning($"FPUploader only supports JSON serialization, ignoring {table.Name}'s {table.Mode}");
			}
			JsonAccumulator jsonAccumulator = new JsonAccumulator();
			accumulators.Add(table, jsonAccumulator);
			return jsonAccumulator;
		}

		public async Task Upload(AnalyticsTable table, MemoryStream stream)
		{
			if (!accumulators.TryGetValue(table, out var accumulator))
			{
				return;
			}
			using (TimeWarning.New("FPUploader.Upload"))
			{
				try
				{
					using ByteArrayContent content = new ByteArrayContent(stream.GetBuffer(), 0, (int)stream.Length);
					content.Headers.ContentType = JsonContentType;
					content.Headers.ContentEncoding.Add("gzip");
					if (!string.IsNullOrEmpty(Analytics.AnalyticsSecret))
					{
						content.Headers.Add(Analytics.AnalyticsHeader, Analytics.AnalyticsSecret);
					}
					else
					{
						content.Headers.Add(Analytics.AnalyticsHeader, Analytics.AnalyticsPublicKey);
					}
					if (!isClient)
					{
						content.Headers.Add("X-SERVER-IP", global::Network.Net.sv.ip);
						content.Headers.Add("X-SERVER-PORT", global::Network.Net.sv.port.ToString());
					}
					string text = (isClient ? Analytics.ClientAnalyticsUrl : Analytics.ServerAnalyticsUrl);
					if (Analytics.Log || Analytics.DryRun)
					{
						string text2 = table.Name + " POST " + text + ":\n";
						text2 += "Headers:\n";
						foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
						{
							text2 = text2 + "  " + header.Key + ": " + string.Join(',', header.Value) + "\n";
						}
						text2 += $"Data: {stream.Length} bytes\n";
						int num = (int)System.Math.Min(stream.Length, 256L);
						string text3 = "";
						byte[] buffer = stream.GetBuffer();
						for (int i = 0; i < num; i++)
						{
							string text4 = text3;
							char c = (char)buffer[i];
							text3 = text4 + c;
						}
						text2 += text3;
						Debug.Log(text2);
					}
					if (!Analytics.DryRun)
					{
						(await httpClient.PostAsync(text, content)).EnsureSuccessStatusCode();
					}
					bytesUploaded += stream.Length;
				}
				catch (Exception ex)
				{
					if (ex is HttpRequestException ex2)
					{
						Debug.Log("HTTP Error when uploading analytics: " + ex2.Message);
					}
					else
					{
						Debug.LogException(ex);
					}
				}
				accumulator.Recycle(stream);
			}
		}

		public void ResetStats()
		{
			bytesUploaded = 0L;
			foreach (KeyValuePair<AnalyticsTable, JsonAccumulator> accumulator in accumulators)
			{
				accumulator.Deconstruct(out var _, out var value);
				value.ResetStats();
			}
		}

		public IUploader Resolve()
		{
			return this;
		}
	}

	private class AzureBulkUploaderImpl : IUploader
	{
		private Dictionary<AnalyticsTable, BulkAccumulator> accumulators = new Dictionary<AnalyticsTable, BulkAccumulator>();

		private BlobContainerClient containerClient;

		private long bytesUploaded;

		private string usedBulkUploadConnectionString = string.Empty;

		private string usedBulkContainerUrl = string.Empty;

		private string usedAzureTenantId = string.Empty;

		private string usedAzureClientId = string.Empty;

		private string usedAzureClientSecret = string.Empty;

		public bool Enabled
		{
			get
			{
				if (!Analytics.DryRun)
				{
					return CanCreateContainerClient();
				}
				return true;
			}
		}

		public string Name => "Azure";

		public bool IsCompressed => true;

		public long BytesUploaded => bytesUploaded;

		public int ItemsSerialized
		{
			get
			{
				int num = 0;
				foreach (KeyValuePair<AnalyticsTable, BulkAccumulator> accumulator in accumulators)
				{
					accumulator.Deconstruct(out var _, out var value);
					BulkAccumulator bulkAccumulator = value;
					num += bulkAccumulator.ItemsWritten;
				}
				return num;
			}
		}

		public long BytesSerialized
		{
			get
			{
				long num = 0L;
				foreach (KeyValuePair<AnalyticsTable, BulkAccumulator> accumulator in accumulators)
				{
					accumulator.Deconstruct(out var _, out var value);
					BulkAccumulator bulkAccumulator = value;
					num += bulkAccumulator.BytesWritten;
				}
				return num;
			}
		}

		public IAccumulator GetAccumulatorFor(AnalyticsTable table)
		{
			if (accumulators.TryGetValue(table, out var value))
			{
				return value;
			}
			BulkAccumulator bulkAccumulator = new BulkAccumulator();
			accumulators.Add(table, bulkAccumulator);
			return bulkAccumulator;
		}

		public async Task Upload(AnalyticsTable table, MemoryStream stream)
		{
			if (!accumulators.TryGetValue(table, out var accumulator))
			{
				return;
			}
			using (TimeWarning.New("AzureBulkUploader.Upload"))
			{
				if (containerClient == null)
				{
					if (CanCreateContainerClient())
					{
						containerClient = CreateContainerClient();
					}
				}
				else
				{
					if (!CanCreateContainerClient())
					{
						containerClient = null;
						usedBulkUploadConnectionString = string.Empty;
						usedBulkContainerUrl = string.Empty;
						usedAzureTenantId = string.Empty;
						usedAzureClientId = string.Empty;
						usedAzureClientSecret = string.Empty;
					}
					if (HasEndpointChanged())
					{
						containerClient = CreateContainerClient();
					}
				}
				if (containerClient != null || Analytics.DryRun)
				{
					string text = ((table.Mode == AnalyticsDocumentMode.JSON) ? ".json" : ".csv");
					string text2 = Path.Combine(table.Name, ConVar.Server.server_id, Guid.NewGuid().ToString("N") + text + ".gz");
					if (Analytics.Log || Analytics.DryRun)
					{
						string text3 = ((!string.IsNullOrEmpty(usedBulkUploadConnectionString)) ? usedBulkUploadConnectionString : usedBulkContainerUrl);
						Debug.Log(string.Concat(string.Concat(table.Name + " Uploading to " + text3 + "\n", "BlobPath: ", text2, "\n"), $"Data: {stream.Length} bytes compressed\n"));
					}
					if (!Analytics.DryRun)
					{
						await containerClient.UploadBlobAsync(text2, (Stream)stream, default(CancellationToken));
					}
					bytesUploaded += stream.Length;
				}
				accumulator.Recycle(stream);
			}
		}

		private BlobContainerClient CreateContainerClient()
		{
			BlobContainerClient result;
			if (!string.IsNullOrEmpty(Analytics.BulkUploadConnectionString))
			{
				result = new BlobContainerClient(new Uri(Analytics.BulkUploadConnectionString));
				usedBulkUploadConnectionString = Analytics.BulkUploadConnectionString;
				usedBulkContainerUrl = string.Empty;
				usedAzureTenantId = string.Empty;
				usedAzureClientId = string.Empty;
				usedAzureClientSecret = string.Empty;
			}
			else
			{
				if (string.IsNullOrEmpty(Analytics.AzureTenantId) || string.IsNullOrEmpty(Analytics.AzureClientId) || string.IsNullOrEmpty(Analytics.AzureClientSecret))
				{
					Debug.Log("analytics_bulk_container_url set but missing Azure AD credentials, disabling bulk uploader.");
					return null;
				}
				ClientSecretCredential credential = new ClientSecretCredential(Analytics.AzureTenantId, Analytics.AzureClientId, Analytics.AzureClientSecret);
				result = new BlobContainerClient(new Uri(Analytics.BulkContainerUrl), (TokenCredential)credential, (BlobClientOptions)null);
				usedBulkUploadConnectionString = string.Empty;
				usedBulkContainerUrl = Analytics.BulkContainerUrl;
				usedAzureTenantId = Analytics.AzureTenantId;
				usedAzureClientId = Analytics.AzureClientId;
				usedAzureClientSecret = Analytics.AzureClientSecret;
			}
			return result;
		}

		private bool CanCreateContainerClient()
		{
			if (string.IsNullOrEmpty(Analytics.BulkUploadConnectionString))
			{
				if (!string.IsNullOrEmpty(Analytics.BulkContainerUrl) && !string.IsNullOrEmpty(Analytics.AzureTenantId) && !string.IsNullOrEmpty(Analytics.AzureClientId))
				{
					return !string.IsNullOrEmpty(Analytics.AzureClientSecret);
				}
				return false;
			}
			return true;
		}

		private bool HasEndpointChanged()
		{
			if (!(Analytics.BulkUploadConnectionString != usedBulkUploadConnectionString) && !(Analytics.BulkContainerUrl != usedBulkContainerUrl) && !(Analytics.AzureTenantId != usedAzureTenantId) && !(Analytics.AzureClientId != usedAzureClientId))
			{
				return Analytics.AzureClientSecret != usedAzureClientSecret;
			}
			return true;
		}

		public void ResetStats()
		{
			bytesUploaded = 0L;
			foreach (KeyValuePair<AnalyticsTable, BulkAccumulator> accumulator in accumulators)
			{
				accumulator.Deconstruct(out var _, out var value);
				value.ResetStats();
			}
		}

		public IUploader Resolve()
		{
			return this;
		}
	}

	public class FallbackUploader : IUploader
	{
		private IUploader primaryUploader;

		private IUploader fallbackUploader;

		private string name;

		public bool DisablePrimary;

		public bool Enabled
		{
			get
			{
				if (!primaryUploader.Enabled)
				{
					return fallbackUploader.Enabled;
				}
				return true;
			}
		}

		public string Name => name;

		public bool IsCompressed => false;

		public long BytesUploaded => 0L;

		public int ItemsSerialized => 0;

		public long BytesSerialized => 0L;

		public FallbackUploader(IUploader primaryUploader, IUploader fallbackUploader)
		{
			this.primaryUploader = primaryUploader;
			this.fallbackUploader = fallbackUploader;
			name = primaryUploader.Name + "->" + fallbackUploader.Name;
		}

		public IAccumulator GetAccumulatorFor(AnalyticsTable table)
		{
			if (!DisablePrimary && primaryUploader.Enabled)
			{
				return primaryUploader.GetAccumulatorFor(table);
			}
			return fallbackUploader.GetAccumulatorFor(table);
		}

		public Task Upload(AnalyticsTable table, MemoryStream stream)
		{
			if (!DisablePrimary && primaryUploader.Enabled)
			{
				return primaryUploader.Upload(table, stream);
			}
			return fallbackUploader.Upload(table, stream);
		}

		public void ResetStats()
		{
			primaryUploader.ResetStats();
			fallbackUploader.ResetStats();
		}

		public IUploader Resolve()
		{
			if (primaryUploader.Enabled)
			{
				return primaryUploader.Resolve();
			}
			return fallbackUploader.Resolve();
		}
	}

	public struct TelemStats
	{
		public int SerializedCount;

		public long SerializedSize;

		public long UploadedSize;

		public int QueueCount;

		public int MaxQueueCount;
	}

	public struct UploadingTable
	{
		public AnalyticsTable Table;

		public IUploader Uploader;
	}

	public static AnalyticsTable GameplayEventsTableServer = new AnalyticsTable("gameplay_events", TimeSpan.FromMinutes(5.0), AnalyticsDocumentMode.JSON, useJsonDataObject: true);

	public IUploader NullUploader;

	public IUploader ServerFPUploader;

	public IUploader AzureBulkUploader;

	public IUploader ServerUploader;

	private List<UploadingTable> tables = new List<UploadingTable>();

	private ConcurrentQueue<UploadingTable> addQueue = new ConcurrentQueue<UploadingTable>();

	private List<IUploader> uploaders = new List<IUploader>();

	private Thread thread;

	private bool keepRunning;

	private bool wantsStatsReset;

	private DateTime statsStartTime = DateTime.Now;

	private const int ThreadSleepTimeMs = 100;

	private const int ThreadShutdownUploadWaitTimeMs = 10000;

	public DateTime StatsStartTime => statsStartTime;

	public ReadOnlySpan<UploadingTable> Tables => tables.ListAsReadOnlySpan();

	public ReadOnlySpan<IUploader> Uploaders => uploaders.ListAsReadOnlySpan();

	public AnalyticsManager()
	{
		NullUploader = new NullUploaderImpl();
		uploaders.Add(NullUploader);
		ServerFPUploader = new FPUploaderImpl(isClient: false);
		uploaders.Add(ServerFPUploader);
		AzureBulkUploader = new AzureBulkUploaderImpl();
		uploaders.Add(AzureBulkUploader);
		ServerUploader = new FallbackUploader(AzureBulkUploader, ServerFPUploader);
		uploaders.Add(ServerUploader);
	}

	public void AddTable(AnalyticsTable table, IUploader uploader)
	{
		Debug.Assert(table != null);
		addQueue.Enqueue(new UploadingTable
		{
			Table = table,
			Uploader = uploader
		});
	}

	public void StartThead()
	{
		keepRunning = true;
		thread = new Thread(DoWork);
		thread.Name = "AnalyticsManager.Thread";
		thread.Start();
	}

	public void Shutdown(int msTimeout = int.MaxValue)
	{
		keepRunning = false;
		thread.Join(msTimeout);
	}

	private void DoWork()
	{
		bool flag = true;
		while (flag)
		{
			using (TimeWarning.New("AnalyticsManager.DoWork"))
			{
				flag = Volatile.Read(ref keepRunning);
				UploadingTable result;
				while (addQueue.TryDequeue(out result))
				{
					tables.Add(result);
				}
				List<Task> list = ((!flag) ? new List<Task>() : null);
				foreach (UploadingTable table2 in tables)
				{
					AnalyticsTable table = table2.Table;
					IUploader uploader = table2.Uploader;
					IAccumulator accumulatorFor = uploader.GetAccumulatorFor(table);
					if (uploader.Enabled)
					{
						MemoryStream memoryStream = table.Accumulate(accumulatorFor, !flag);
						if (memoryStream != null)
						{
							Task item = uploader.Upload(table, memoryStream);
							list?.Add(item);
						}
					}
					else
					{
						table.Clear();
						accumulatorFor.Clear();
					}
				}
				if (list != null)
				{
					TimeSpan timeSpan = TimeSpan.FromMilliseconds(10000.0);
					DateTime dateTime = DateTime.Now + timeSpan;
					while (DateTime.Now < dateTime)
					{
						PlatformService.Instance.Update();
						bool flag2 = true;
						foreach (Task item2 in list)
						{
							flag2 &= item2.IsCompleted;
						}
						if (flag2)
						{
							break;
						}
						Thread.Sleep(10);
					}
				}
				if (Volatile.Read(ref wantsStatsReset))
				{
					foreach (UploadingTable table3 in tables)
					{
						table3.Table.ResetStats();
						table3.Uploader.ResetStats();
					}
					wantsStatsReset = false;
					statsStartTime = DateTime.Now;
				}
			}
			if (flag)
			{
				Thread.Sleep(100);
			}
		}
	}

	public void ResetStats()
	{
		wantsStatsReset = true;
	}

	public TelemStats GatherStats()
	{
		TelemStats result = default(TelemStats);
		foreach (UploadingTable table in tables)
		{
			result.QueueCount += table.Table.QueueCount;
			result.MaxQueueCount = System.Math.Max(result.MaxQueueCount, table.Table.MaxQueueCount);
		}
		foreach (IUploader uploader in uploaders)
		{
			result.UploadedSize += uploader.BytesUploaded;
			result.SerializedCount += uploader.ItemsSerialized;
			result.SerializedSize += uploader.BytesSerialized;
		}
		return result;
	}
}
