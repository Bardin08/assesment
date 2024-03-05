using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixel.Shared.Contracts;
using StackExchange.Redis;

namespace PixelStorage.Infrastructure;

public class TrackerRecordRepository(
    IOptions<FileStorageOptions> fileStorageOptions) : ITrackerRecordRepository
{
    private readonly StreamWriter _streamWriter = GetStreamWriter(fileStorageOptions);
    private readonly object _lockObject = new();
    private int _wroteRecords;

    private static StreamWriter GetStreamWriter(IOptions<FileStorageOptions> fileStorageOptions)
    {
        const int oneKb = 1024;

        ArgumentNullException.ThrowIfNull(fileStorageOptions.Value);
        ArgumentNullException.ThrowIfNull(fileStorageOptions.Value.FilePath);

        var fullPath = Path.GetFullPath(fileStorageOptions.Value.FilePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        var bufferSize = fileStorageOptions.Value.BufferSize < oneKb
            ? oneKb
            : fileStorageOptions.Value.BufferSize;

        return new StreamWriter(fullPath, append: true,
            Encoding.UTF8, bufferSize);
    }

    public void SaveTrackerRecord(RedisChannel _, RedisValue message)
    {
        if (!message.HasValue)
        {
            return;
        }

        var recordStr = GetRecordLog(message);
        if (string.IsNullOrEmpty(recordStr))
        {
            return;
        }

        lock (_lockObject)
        {
            _streamWriter.WriteLine(recordStr);
            Interlocked.Add(ref _wroteRecords, 1);

            if (_wroteRecords > 1000)
            {
                _streamWriter.Flush();
                Interlocked.Exchange(ref _wroteRecords, 0);
            }
        }
    }

    private string? GetRecordLog(RedisValue message)
    {
        var record = JsonSerializer.Deserialize<TrackerRecord>(message!);
        if (record is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(record.IpAddress))
        {
            // no need to save records without IP_Address,
            // according to the requirements it's the only mandatory field. 
            return null;
        }

        var recordStr = string.Join("|",
            [
                record.Timestamp.ToString("O"),
                record.Referer ?? "null",
                record.UserAgent ?? "null",
                record.IpAddress
            ]
        );
        return recordStr;
    }

    public void Dispose()
    {
        _streamWriter.Dispose();
    }
}