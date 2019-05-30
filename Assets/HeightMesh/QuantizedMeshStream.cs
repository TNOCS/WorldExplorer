using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class QuantizedMeshStream : IDisposable
{
    private readonly Stream mStream;
    bool mDisposed = false;

    public QuantizedMeshStream(Stream pInputStream, bool pIsGZipped = false)
    {
        if (pInputStream == null) throw new ArgumentNullException("pInputStream");
        mStream = pIsGZipped ? (new GZipStream(pInputStream, CompressionMode.Decompress)) : pInputStream;
    }

    public float ReadFloat()
    {
        const int floatSize = 4; // 32 bit
        byte[] buffer = new byte[floatSize];
        int nRead = mStream.Read(buffer, 0, buffer.Length);
        if (nRead != floatSize) throw new Exception(" Stream error");
        return System.BitConverter.ToSingle(buffer, 0);
    }

    public double ReadDouble()
    {
        const int doubleSize = 8; // 64 bit
        byte[] buffer = new byte[doubleSize];
        int nRead = mStream.Read(buffer, 0, buffer.Length);
        if (nRead != doubleSize) throw new Exception(" Stream error");
        return System.BitConverter.ToDouble(buffer, 0);
    }

    public uint ReadUnsigned32Bit()
    {
        const int size = 4; //  32-bit unsigned integer
        byte[] buffer = new byte[size];
        int nRead = mStream.Read(buffer, 0, buffer.Length);
        if (nRead != size) throw new Exception(" Stream error");
        return System.BitConverter.ToUInt32(buffer, 0);
    }

    public ushort ReadUnsigned16Bit()
    {
        const int size = 2; //  16-bit unsigned integer
        byte[] buffer = new byte[size];
        int nRead = mStream.Read(buffer, 0, buffer.Length);
        if (nRead != size) throw new Exception(" Stream error");
        return System.BitConverter.ToUInt16(buffer, 0);
    }

    public void ReadPadding(byte pNumberOfBytes)
    {

        byte[] buffer = new byte[pNumberOfBytes];
        int nRead = mStream.Read(buffer, 0, buffer.Length);
        if (nRead != pNumberOfBytes) throw new Exception(" Stream error");
    }

    public long GetPosition()
    {
        return mStream.Position;
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (mDisposed)
            return;

        if (disposing)
        {
            if (mStream != null) mStream.Dispose();
        }

        mDisposed = true;
    }


}
