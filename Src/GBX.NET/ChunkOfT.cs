﻿namespace GBX.NET;

/// <summary>
/// A data chunk interface belonging to <typeparamref name="T"/>.
/// </summary>
public interface IChunk<T> : IChunk where T : IClass
{

}

/// <summary>
/// A data chunk belonging to <typeparamref name="T"/>.
/// </summary>
public abstract class Chunk<T> : Chunk, IReadableWritableChunk<T> where T : IClass
{
    /// <inheritdoc />
    public virtual void Read(T n, IGbxReader r) => Read(n, (GbxReader)r);

    /// <inheritdoc />
    public virtual void Write(T n, IGbxWriter w) => Write(n, (GbxWriter)w);

    /// <inheritdoc />
    public virtual void ReadWrite(T n, IGbxReaderWriter rw) => ReadWrite(n, (GbxReaderWriter)rw);

    internal virtual void Read(T n, GbxReader r)
    {

    }

    internal virtual void Write(T n, GbxWriter w)
    {

    }

    internal virtual void ReadWrite(T n, GbxReaderWriter rw)
    {
        if (rw.Reader is not null)
        {
            Read(n, rw.Reader);
        }

        if (rw.Writer is not null)
        {
            Write(n, rw.Writer);
        }
    }

    internal override void Read(IClass n, GbxReader r)
    {
        Read((T)n, r);
    }

    internal override void Write(IClass n, GbxWriter w)
    {
        Write((T)n, w);
    }

    internal override void ReadWrite(IClass n, GbxReaderWriter rw)
    {
        ReadWrite((T)n, rw);
    }
}
