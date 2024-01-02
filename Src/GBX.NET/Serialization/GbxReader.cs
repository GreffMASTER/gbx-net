﻿using GBX.NET.Components;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace GBX.NET.Serialization;

/// <summary>
/// A binary/text reader specialized for Gbx.
/// </summary>
public interface IGbxReader : IDisposable
{
    Stream BaseStream { get; }
    SerializationMode Mode { get; }
    GbxFormat Format { get; }

    bool ReadGbxMagic();

    byte ReadByte();
    sbyte ReadSByte();
    short ReadInt16();
    ushort ReadUInt16();
    int ReadInt32();
    uint ReadUInt32();
    long ReadInt64();
    ulong ReadUInt64();
    float ReadSingle();
    void Close();

    int ReadHexInt32();
    uint ReadHexUInt32();
    BigInteger ReadBigInt(int byteLength);
    BigInteger ReadInt128();
    Int2 ReadInt2();
    Int3 ReadInt3();
    Byte3 ReadByte3();
    Vec2 ReadVec2();
    Vec3 ReadVec3();
    Vec4 ReadVec4();
    bool ReadBoolean();
    bool ReadBoolean(bool asByte);
    byte[] ReadData();
    byte[] ReadBytes(int count);
    string ReadString();
    string ReadString(int length);
    string ReadString(StringLengthPrefix readPrefix);
    string ReadIdAsString();
    Id ReadId();
    Ident ReadIdent();
    PackDesc ReadPackDesc();
    IClass ReadNodeRef();
    TimeInt32 ReadTimeInt32();
    TimeSingle ReadTimeSingle();
    TimeSpan? ReadTimeOfDay();

    void SkipData(int length);
    byte[] ReadToEnd();
    void ResetIdState();
}

/// <summary>
/// A binary/text reader specialized for Gbx.
/// </summary>
internal sealed class GbxReader : BinaryReader, IGbxReader
{
    internal const int MaxDataSize = 0x10000000; // ~268MB

    private static readonly Encoding encoding = Encoding.UTF8;

#if NET6_0_OR_GREATER
    private string? prevString;
#endif

    private bool enablePreviousStringCache;
    public bool EnablePreviousStringCache { get => enablePreviousStringCache; set => enablePreviousStringCache = value; }

    private readonly GbxRefTable? refTable;
    private readonly XmlReader? xmlReader;

    private int? idVersion;
    private Dictionary<int, string>? idDict;
    private Encapsulation? encapsulation;

    internal int? IdVersion
    {
        get => encapsulation is null ? idVersion : encapsulation.IdVersion;
        set
        {
            if (encapsulation is null)
            {
                idVersion = value;
            }
            else
            {
                encapsulation.IdVersion = value;
            }
        }
    }

    internal Dictionary<int, string> IdDict => encapsulation is null
        ? idDict ??= []
        : encapsulation.IdReadDict;

    internal Encapsulation? Encapsulation { get => encapsulation; set => encapsulation = value; }

    public SerializationMode Mode { get; }
    public GbxFormat Format { get; private set; }

    public GbxReader(Stream input, GbxRefTable? refTable = null) : base(input, encoding)
    {
        this.refTable = refTable;
    }

    public GbxReader(Stream input, bool leaveOpen, GbxRefTable? refTable = null) : base(input, encoding, leaveOpen)
    {
        this.refTable = refTable;
    }

    public GbxReader(XmlReader input) : base(Stream.Null, encoding)
    {
        xmlReader = input;
        Mode = SerializationMode.Xml;
    }

    public bool ReadGbxMagic()
    {
        return base.ReadByte() == 'G' && base.ReadByte() == 'B' && base.ReadByte() == 'X';
    }

    public override byte ReadByte()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadByte(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public override sbyte ReadSByte()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadSByte(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public override short ReadInt16()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadInt16(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public override int ReadInt32()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadInt32(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public int ReadHexInt32()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadInt32(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public override uint ReadUInt32()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadUInt32(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public uint ReadHexUInt32()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadUInt32(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public override long ReadInt64()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadInt64(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public override ulong ReadUInt64()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadUInt64(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public override float ReadSingle()
    {
        return Mode switch
        {
            SerializationMode.Gbx => base.ReadSingle(),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public BigInteger ReadBigInt(int byteLength)
    {
        return new BigInteger(ReadBytes(byteLength));
    }

    public BigInteger ReadInt128()
    {
        return ReadBigInt(byteLength: 16);
    }

    public Int2 ReadInt2()
    {
        return new(ReadInt32(), ReadInt32());
    }

    public Int3 ReadInt3()
    {
        return new(ReadInt32(), ReadInt32(), ReadInt32());
    }

    public Byte3 ReadByte3()
    {
        return new(ReadByte(), ReadByte(), ReadByte());
    }

    public Vec2 ReadVec2()
    {
        return new(ReadSingle(), ReadSingle());
    }

    public Vec3 ReadVec3()
    {
        return new(ReadSingle(), ReadSingle(), ReadSingle());
    }

    public Vec4 ReadVec4()
    {
        return new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
    }

    public GbxFormat ReadFormatByte()
    {
        Format = (GbxFormat)ReadByte();
        return Format;
    }

    public override bool ReadBoolean()
    {
        switch (Mode)
        {
            case SerializationMode.Gbx:
                var booleanAsInt = base.ReadUInt32();

                if (Gbx.StrictBooleans && booleanAsInt > 1)
                {
                    throw new BooleanOutOfRangeException(booleanAsInt);
                }

                return booleanAsInt != 0;
            default:
                throw new SerializationModeNotSupportedException(Mode);
        }
    }

    public bool ReadBoolean(bool asByte)
    {
        if (!asByte)
        {
            return ReadBoolean();
        }

        switch (Mode)
        {
            case SerializationMode.Gbx:
                var booleanAsByte = base.ReadByte();

                if (Gbx.StrictBooleans && booleanAsByte > 1)
                {
                    throw new BooleanOutOfRangeException(booleanAsByte);
                }

                return booleanAsByte != 0;
            default:
                throw new SerializationModeNotSupportedException(Mode);
        }
    }

    public override string ReadString()
    {
        return Mode switch
        {
            SerializationMode.Gbx => ReadString(base.ReadInt32()),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public string ReadString(StringLengthPrefix readPrefix)
    {
        switch (Mode)
        {
            case SerializationMode.Gbx:
                // Length of the string in bytes, not chars
                var length = readPrefix switch
                {
                    StringLengthPrefix.Byte => base.ReadByte(),
                    StringLengthPrefix.Int32 => base.ReadInt32(),
                    _ => throw new ArgumentException("Can't read string without knowing its length.", nameof(readPrefix)),
                };

                return ReadString(length);
            default:
                throw new SerializationModeNotSupportedException(Mode);
        }
    }

    public string ReadString(int length)
    {
        switch (Mode)
        {
            case SerializationMode.Gbx:
                if (length == 0)
                {
                    return string.Empty;
                }

                if (length < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
                }

                if (length > MaxDataSize) // ~268MB
                {
                    throw new LengthLimitException(length);
                }

#if NET6_0_OR_GREATER
                if (length > 2048)
                {
#endif
                    return encoding.GetString(ReadBytes(length));
#if NET6_0_OR_GREATER
                }

                Span<byte> bytes = stackalloc byte[length];

                if (Read(bytes) != length)
                {
                    throw new EndOfStreamException();
                }

                if (!enablePreviousStringCache)
                {
                    return encoding.GetString(bytes);
                }

                Span<char> chars = stackalloc char[1024];

                var charLength = encoding.GetChars(bytes, chars);
                var charSlice = chars.Slice(0, charLength);

                if (prevString is not null && MemoryExtensions.Equals(charSlice, prevString, StringComparison.Ordinal))
                {
                    return prevString;
                }

                return prevString = charSlice.ToString();
#endif
            default:
                throw new SerializationModeNotSupportedException(Mode);
        }
        
    }

    public byte[] ReadData()
    {
        return ReadBytes(base.ReadInt32());
    }

    public override byte[] ReadBytes(int count)
    {
        if (count > MaxDataSize)
        {
            throw new LengthLimitException(count);
        }

        return Mode switch
        {
            SerializationMode.Gbx => base.ReadBytes(count),
            _ => throw new SerializationModeNotSupportedException(Mode),
        };
    }

    public string ReadIdAsString()
    {
        var index = ReadIdIndex();

        if ((index & 0xC0000000) is not 0x40000000 and not 0x80000000)
        {
            throw new NotSupportedException("This Id cannot be read as string.");
        }

        return ReadIdAsString(index);
    }

    public Id ReadId()
    {
        var index = ReadIdIndex();

        if ((index & 0xC0000000) is not 0x40000000 and not 0x80000000)
        {
            return new(index);
        }

        return new(ReadIdAsString(index));
    }

    private int ReadIdIndex()
    {
        switch (Mode)
        {
            case SerializationMode.Gbx:
                IdVersion ??= ReadInt32();

                if (IdVersion < 3)
                {
                    throw new NotSupportedException($"Unsupported Id version ({IdVersion}).");
                }

                return ReadInt32();
            default:
                throw new SerializationModeNotSupportedException(Mode);
        }
        
    }

    private string ReadIdAsString(int index)
    {
        if ((index & 0xFFFFFFF) != 0)
        {
            return IdDict?[index] ?? throw new Exception("Invalid Id index.");
        }

        var str = ReadString();

        if ((index & 0xC0000000) == 0x40000000)
        {
            // SetLocalName
            IdDict.Add(index + IdDict.Count + 1, str);
        }
        else
        {
            // AddName
            IdDict.Add(index + IdDict.Count + 1, str);
        }

        return str;
    }

    public Ident ReadIdent()
    {
        var id = ReadIdAsString();
        var collection = ReadId();
        var author = ReadIdAsString();

        return new Ident(id, collection, author);
    }

    public PackDesc ReadPackDesc()
    {
        var version = ReadByte();

        var checksum = default(byte[]);
        var locatorUrl = "";

        if (version >= 3)
        {
            checksum = ReadBytes(32);
        }

        var filePath = ReadString();

        if ((filePath.Length > 0 && version >= 1) || version >= 3)
        {
            locatorUrl = ReadString();
        }

        return new PackDesc(version, checksum, filePath, locatorUrl);
    }

    public IClass ReadNodeRef()
    {
        throw new NotImplementedException();
    }

    public TimeInt32 ReadTimeInt32()
    {
        return new(ReadInt32());
    }

    public TimeSingle ReadTimeSingle()
    {
        return new(ReadSingle());
    }

    public TimeSpan? ReadTimeOfDay()
    {
        var dayTime = ReadUInt32();

        if (dayTime == uint.MaxValue)
        {
            return null;
        }

        if (dayTime > ushort.MaxValue)
        {
            throw new InvalidDataException("Day time is over 65535");
        }

        var maxTime = TimeSpan.FromDays(1) - TimeSpan.FromSeconds(1);
        var maxSecs = maxTime.TotalSeconds;

        return TimeSpan.FromSeconds(Convert.ToInt32(dayTime / (float)ushort.MaxValue * maxSecs));
    }

    internal T[] ReadArray<T>(int length, bool lengthInBytes = false) where T : struct
    {
        if (length == 0)
        {
            return [];
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length is not valid.");
        }

        var l = lengthInBytes ? length : Marshal.SizeOf<T>() * length;

        if (l < 0 || l > 0x10000000) // ~268MB
        {
            throw new Exception($"Length is too big to handle ({(l < 0 ? length : l)}).");
        }

        if (l > 1_500_000)
        {
            return MemoryMarshal.Cast<byte, T>(ReadBytes(l)).ToArray();
        }

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        Span<byte> bytes = stackalloc byte[l];
        Read(bytes);
#else
        var bytes = ReadBytes(l);
#endif

        return MemoryMarshal.Cast<byte, T>(bytes).ToArray();
    }

    internal T[] ReadArray_deprec<T>(int length, bool lengthInBytes = false) where T : struct
    {
        ReadInt32(); // Version
        return ReadArray<T>(length, lengthInBytes);
    }

    /// <summary>
    /// If can seek, position moves past the <paramref name="length"/>. If seeking is NOT supported, data is read with no allocation using <see cref="BinaryReader.Read(Span{byte})"/>. If .NET Standard 2.0, unavoidable byte array allocation happens with <see cref="BinaryReader.ReadBytes(int)"/>.
    /// </summary>
    /// <param name="length">Length in bytes to skip.</param>
    /// <exception cref="EndOfStreamException"></exception>
    public void SkipData(int length)
    {
        if (BaseStream.CanSeek)
        {
            if (BaseStream.Position + length > BaseStream.Length)
            {
                throw new EndOfStreamException();
            }

            _ = BaseStream.Seek(length, SeekOrigin.Current);

            return;
        }

#if NET6_0_OR_GREATER
        if (Read(stackalloc byte[length]) != length)
#else
        if (ReadBytes(length).Length != length)
#endif
        {
            throw new EndOfStreamException();
        }
    }

    public byte[] ReadToEnd()
    {
        if (BaseStream.CanSeek)
        {
            return ReadBytes((int)(BaseStream.Length - BaseStream.Position));
        }

        using var ms = new MemoryStream();
        BaseStream.CopyTo(ms);
        return ms.ToArray();
    }

    public void ResetIdState()
    {
        IdVersion = null;
        IdDict.Clear();
    }

    private string ReadToWindowsNewLine()
    {
        var sb = new StringBuilder();

        while (true)
        {
            var b = base.ReadByte();

            if (b == 0x0D)
            {
                if (base.ReadByte() != 0x0A)
                {
                    throw new Exception("Invalid string format.");
                }

                break;
            }

            sb.Append((char)b);
        }

        return sb.ToString();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            idDict = null;
        }

        base.Dispose(disposing);
    }
}