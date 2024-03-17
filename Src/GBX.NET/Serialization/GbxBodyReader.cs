﻿using GBX.NET.Components;
using System.Collections.Immutable;

namespace GBX.NET.Serialization;

internal sealed partial class GbxBodyReader(GbxReaderWriter readerWriter, GbxReadSettings settings, GbxCompression compression)
{
    private readonly GbxReader reader = readerWriter.Reader ?? throw new Exception("Reader is required but not available.");

    [Zomp.SyncMethodGenerator.CreateSyncVersion]
    public static async ValueTask<GbxBody> ParseAsync(
        GbxReader reader,
        GbxCompression compression,
        GbxReadSettings settings,
        CancellationToken cancellationToken = default)
    {
        switch (compression)
        {
            case GbxCompression.Compressed:

                var uncompressedSize = reader.ReadInt32();

                if (IsValidUncompressedSize(uncompressedSize, settings))
                {
                    throw new Exception($"Uncompressed body size {uncompressedSize} exceeds maximum allowed size {settings.MaxUncompressedBodySize}.");
                }

                var compressedSize = reader.ReadInt32();
                var rawData = settings.ReadRawBody
                    ? ImmutableArray.Create(await reader.ReadBytesAsync(compressedSize, cancellationToken))
                    : ImmutableArray<byte>.Empty;

                return new GbxBody
                {
                    UncompressedSize = uncompressedSize,
                    CompressedSize = compressedSize,
                    RawData = rawData
                };

            case GbxCompression.Uncompressed:

                return new GbxBody
                {
                    RawData = settings.ReadRawBody
                        ? ImmutableArray.Create(await reader.ReadToEndAsync(cancellationToken))
                        : ImmutableArray<byte>.Empty
                };

            default:
                throw new ArgumentException("Unknown compression type.", nameof(compression));
        }
    }

    private static bool IsValidUncompressedSize(int uncompressedSize, GbxReadSettings settings)
    {
        return uncompressedSize > GbxReader.MaxDataSize
            || (settings.MaxUncompressedBodySize.HasValue && uncompressedSize > settings.MaxUncompressedBodySize.Value);
    }

    [Zomp.SyncMethodGenerator.CreateSyncVersion]
    public async Task<GbxBody> ParseAsync(IClass node, CancellationToken cancellationToken = default)
    {
        if (settings.ReadRawBody)
        {
            throw new NotSupportedException("Reading raw body is not supported when parsing body to a node.");
        }

        var body = await ParseAsync(reader, compression, settings, cancellationToken);

        if (body.CompressedSize is null)
        {
            ReadMainNode(node, body, readerWriter);
            return body;
        }

        var decompressedData = await DecompressDataAsync(body.CompressedSize.Value, body.UncompressedSize, cancellationToken);

        using var ms = new MemoryStream(decompressedData);
        using var decompressedReader = new GbxReader(ms, settings.Logger);
        decompressedReader.LoadFrom(reader);
        using var decompressedReaderWriter = new GbxReaderWriter(decompressedReader);

        ReadMainNode(node, body, decompressedReaderWriter);

        return body;
    }

    [Zomp.SyncMethodGenerator.CreateSyncVersion]
    public async Task<GbxBody> ParseAsync<T>(T node, CancellationToken cancellationToken = default) where T : IClass
    {
        if (settings.ReadRawBody)
        {
            throw new NotSupportedException("Reading raw body is not supported when parsing body to a node.");
        }

        var body = await ParseAsync(reader, compression, settings, cancellationToken);

        if (body.CompressedSize is null)
        {
            ReadMainNode(node, body, readerWriter);
            return body;
        }

        var decompressedData = await DecompressDataAsync(body.CompressedSize.Value, body.UncompressedSize, cancellationToken);

        using var ms = new MemoryStream(decompressedData);
        using var decompressedReader = new GbxReader(ms, settings.Logger);
        decompressedReader.LoadFrom(reader);
        using var decompressedReaderWriter = new GbxReaderWriter(decompressedReader);

        ReadMainNode(node, body, decompressedReaderWriter);

        return body;
    }

    private async Task<byte[]> DecompressDataAsync(int compressedSize, int uncompressedSize, CancellationToken cancellationToken)
    {
        var compressedData = await reader.ReadBytesAsync(compressedSize, cancellationToken);
        var decompressedData = new byte[uncompressedSize];

        if (Gbx.LZO is null)
        {
            throw new LzoNotDefinedException();
        }

        Gbx.LZO.Decompress(compressedData, decompressedData);

        return decompressedData;
    }

    private byte[] DecompressData(int compressedSize, int uncompressedSize)
    {
#if NET5_0_OR_GREATER
        if (compressedSize > 1_000_000)
        {
            var compressedDataOver1MB = reader.ReadBytes(compressedSize);
            var decompressedDataOver1MB = new byte[uncompressedSize];

            if (Gbx.LZO is null)
            {
                throw new LzoNotDefinedException();
            }

            Gbx.LZO.Decompress(compressedDataOver1MB, decompressedDataOver1MB);

            return decompressedDataOver1MB;
        }

        Span<byte> compressedData = stackalloc byte[compressedSize];
        if (reader.Read(compressedData) != compressedSize)
        {
            throw new Exception("Failed to read compressed data");
        }
#else
        var compressedData = reader.ReadBytes(compressedSize);
#endif
        var decompressedData = new byte[uncompressedSize];

        if (Gbx.LZO is null)
        {
            throw new LzoNotDefinedException();
        }

#if NET5_0_OR_GREATER
        Gbx.LZO.Decompress(in compressedData, decompressedData);
#else
        Gbx.LZO.Decompress(compressedData, decompressedData);
#endif

        return decompressedData;
    }

    private void ReadMainNode(IClass node, GbxBody body, GbxReaderWriter rw)
    {
        try
        {
            node.ReadWrite(rw);
        }
        catch (Exception ex)
        {
            body.Exception = ex;

            if (!settings.IgnoreExceptionsInBody)
            {
                throw;
            }
        }
    }

    private void ReadMainNode<T>(T node, GbxBody body, GbxReaderWriter rw) where T : IClass
    {
        try
        {
#if NET8_0_OR_GREATER
            T.Read(node, rw);
#else
            node.ReadWrite(rw);
#endif
        }
        catch (Exception ex)
        {
            body.Exception = ex;

            if (!settings.IgnoreExceptionsInBody)
            {
                throw;
            }
        }
    }
}
