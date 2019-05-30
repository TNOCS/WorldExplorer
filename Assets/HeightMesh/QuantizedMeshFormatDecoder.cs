using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class QuantizedMeshFormatDecoder 
{
    public enum SizePerStruct
    {
        StructSize_32bit,
        StructSize_16bit,
    }

    public struct NormilizedVertexData
    {
        public float u;
        public float v;
        public float height;
    }

    public struct VertexData
    {
        /* The horizontal coordinate of the vertex in the tile. 
         * When the u value is 0, the vertex is on the Western edge of the tile. 
         * When the value is 32767, the vertex is on the Eastern edge of the tile. 
         * For other values, the vertex's longitude is a linear interpolation between the longitudes of the Western and Eastern edges of the tile. */
        public int u;
        /*
         * 	The vertical coordinate of the vertex in the tile. 
         * 	When the v value is 0, the vertex is on the Southern edge of the tile. 
         * 	When the value is 32767, the vertex is on the Northern edge of the tile. 
         * 	For other values, the vertex's latitude is a linear interpolation between the latitudes of the Southern and Nothern edges of the tile. */
        public int v;
        /* The height of the vertex in the tile. 
         * When the height value is 0, the vertex's height is equal to the minimum height within the tile, as specified in the tile's header. 
         * When the value is 32767, the vertex's height is equal to the maximum height within the tile. 
         * For other values, the vertex's height is a linear interpolation between the minimum and maximum heights. */
        public int height;
    }

    // The center of the tile in Earth-centered Fixed coordinates.
    public double CenterX { get; private set; }
    public double CenterY { get; private set; }
    public double CenterZ { get; private set; }

    // The minimum and maximum heights in the area covered by this tile.
    // The minimum may be lower and the maximum may be higher than
    // the height of any vertex in this tile in the case that the min/max vertex
    // was removed during mesh simplification, but these are the appropriate
    // values to use for analysis or visualization.
    public float MinimumHeight { get; private set; }
    public float MaximumHeight { get; private set; }

    // The tile’s bounding sphere.  The X,Y,Z coordinates are again expressed
    // in Earth-centered Fixed coordinates, and the radius is in meters.
    public double BoundingSphereCenterX { get; private set; }
    public double BoundingSphereCenterY { get; private set; }
    public double BoundingSphereCenterZ { get; private set; }
    public double BoundingSphereRadius { get; private set; }

    // The horizon occlusion point, expressed in the ellipsoid-scaled Earth-centered Fixed frame.
    // If this point is below the horizon, the entire tile is below the horizon.
    // See http://cesiumjs.org/2013/04/25/Horizon-culling/ for more information.
    public double HorizonOcclusionPointX { get; private set; }
    public double HorizonOcclusionPointY { get; private set; }
    public double HorizonOcclusionPointZ { get; private set; }

    private VertexData[] mVertexList = null;
    public VertexData[] VertexList { get { return mVertexList; } }

    // counter-clockwise winding order
    private uint[] mTriangleIndices = null;
    public uint[] TriangleIndices { get { return mTriangleIndices; } }

    private uint[] mWestIndices = null;
    public uint[] WestIndices { get { return mWestIndices; } }
    private uint[] mSouthIndices = null;
    public uint[] SouthIndices { get { return mSouthIndices; } }
    private uint[] mEastIndices = null;
    public uint[] EastIndices { get { return mEastIndices; } }
    private uint[] mNorthIndices = null;
    public uint[] NorthIndices { get { return mNorthIndices; } }

    public QuantizedMeshFormatDecoder(FileInfo pTerrainFile, bool pIsGzipped)
    {
        using (var inputStream = File.OpenRead(pTerrainFile.FullName))
        {
            DecodeFile(inputStream, pIsGzipped);
        }
    }

    public QuantizedMeshFormatDecoder(Stream pStream, bool pIsGzipped)
    {
        DecodeFile(pStream, pIsGzipped);
    }

    public void DecodeFile(Stream pInputStream, bool pIsGzipped)
    {
        if (pInputStream == null) throw new ArgumentNullException("pInputStream");
        using (var quantizedMeshStream = new QuantizedMeshStream(pInputStream, pIsGzipped))
        {
            // Read Header
            CenterX = quantizedMeshStream.ReadDouble();
            CenterY = quantizedMeshStream.ReadDouble();
            CenterZ = quantizedMeshStream.ReadDouble();
            MinimumHeight = quantizedMeshStream.ReadFloat();
            MaximumHeight = quantizedMeshStream.ReadFloat();
            BoundingSphereCenterX = quantizedMeshStream.ReadDouble();
            BoundingSphereCenterY = quantizedMeshStream.ReadDouble();
            BoundingSphereCenterZ = quantizedMeshStream.ReadDouble();
            BoundingSphereRadius = quantizedMeshStream.ReadDouble();
            HorizonOcclusionPointX = quantizedMeshStream.ReadDouble();
            HorizonOcclusionPointY = quantizedMeshStream.ReadDouble();
            HorizonOcclusionPointZ = quantizedMeshStream.ReadDouble();

            // Read Vertex Data
            getVertexList(quantizedMeshStream, ref mVertexList);

            // Read Index data (traingle indices)
            SizePerStruct structSize = (mVertexList.Length > 65536) ? SizePerStruct.StructSize_32bit : SizePerStruct.StructSize_16bit;
            GetIndices(quantizedMeshStream, ref mTriangleIndices, structSize);

            // Read edge indices
            getEdgeIndices(quantizedMeshStream, ref mWestIndices, structSize);
            getEdgeIndices(quantizedMeshStream, ref mSouthIndices, structSize);
            getEdgeIndices(quantizedMeshStream, ref mEastIndices, structSize);
            getEdgeIndices(quantizedMeshStream, ref mNorthIndices, structSize);
        }
    }

    // Normalize between 0..1 (for vertex u, v and height)
    public static double Normalize(int pValue)
    {
        return (float)pValue / 32767.0;
    }

    public float GetHeightInMeter(int pVertexIndex /* start at zero*/)
    {
        if ((mVertexList != null) && (pVertexIndex < mVertexList.Length))
        {
            float delta = MaximumHeight - MinimumHeight;
            return (float)(MinimumHeight + (Normalize(mVertexList[pVertexIndex].height) * delta));
        }
        return float.MinValue;
    }

    private static void GetIndices(QuantizedMeshStream pStream, ref uint[] pTriangleIndices, SizePerStruct pSizePerStruct)
    {
        byte bytesInStruct = (pSizePerStruct == SizePerStruct.StructSize_32bit) ? (byte)4 : (byte)2;
        var alignment = pStream.GetPosition() % bytesInStruct;
        if (alignment != 0)
        {
            pStream.ReadPadding((byte)(bytesInStruct - alignment));
        }


        uint triangleCount = pStream.ReadUnsigned32Bit();
        pTriangleIndices = new uint[triangleCount * 3]; /* 3 indices for 1 triangle */
        for (int index = 0; index < pTriangleIndices.Length; index++)
        {
            pTriangleIndices[index] = (pSizePerStruct == SizePerStruct.StructSize_32bit) ? pStream.ReadUnsigned32Bit() : pStream.ReadUnsigned16Bit();
        }


        // Decompress values
        uint highest = 0;
        for (var i = 0; i < pTriangleIndices.Length; ++i)
        {
            uint code = pTriangleIndices[i];
            pTriangleIndices[i] = highest - code;
            if (code == 0) { ++highest; }
        }

    }

    private static void getEdgeIndices(QuantizedMeshStream pStream, ref uint[] pEdgeIndices, SizePerStruct pSizePerStruct)
    {
        uint triangleCount = pStream.ReadUnsigned32Bit();
        pEdgeIndices = new uint[triangleCount];
        for (int index = 0; index < pEdgeIndices.Length; index++)
        {
            pEdgeIndices[index] = (pSizePerStruct == SizePerStruct.StructSize_32bit) ? pStream.ReadUnsigned32Bit() : pStream.ReadUnsigned16Bit();
        }
    }

    private static void getVertexList(QuantizedMeshStream pStream, ref VertexData[] pVertexList)
    {
        uint numberOfVertex = pStream.ReadUnsigned32Bit();
        pVertexList = new VertexData[numberOfVertex];

        for (int index = 0; index < numberOfVertex; index++) pVertexList[index].u = pStream.ReadUnsigned16Bit();
        for (int index = 0; index < numberOfVertex; index++) pVertexList[index].v = pStream.ReadUnsigned16Bit();
        for (int index = 0; index < numberOfVertex; index++) pVertexList[index].height = pStream.ReadUnsigned16Bit();

        // Apply ZigZag (data is compressed)
        var u = 0;
        var v = 0;
        var height = 0;
        for (int index = 0; index < numberOfVertex; ++index)
        {
            u += ZigZagDecode((ushort)pVertexList[index].u);
            v += ZigZagDecode((ushort)pVertexList[index].v);
            height += ZigZagDecode((ushort)pVertexList[index].height);

            pVertexList[index].u = u;
            pVertexList[index].v = v;
            pVertexList[index].height = height;
        }
    }

    private static int ZigZagDecode(ushort pValue)
    {
        return ((pValue >> 1) ^ (-(pValue & 1)));
    }
}
