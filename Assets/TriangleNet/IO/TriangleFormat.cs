using System;
using System.Collections.Generic;
using System.IO;
using TriangleNet;
using TriangleNet.Geometry;

namespace TriangleNet.IO
{
	public class TriangleFormat : IGeometryFormat, IMeshFormat
	{
		public TriangleFormat()
		{
		}

		public Mesh Import(string filename)
		{
			List<ITriangle> triangles;
			InputGeometry inputGeometry;
			string extension = Path.GetExtension(filename);
			if (extension == ".node" || extension == ".poly" || extension == ".ele")
			{
				FileReader.Read(filename, out inputGeometry, out triangles);
				if (inputGeometry != null && triangles != null)
				{
					Mesh mesh = new Mesh();
					mesh.Load(inputGeometry, triangles);
					return mesh;
				}
			}
			throw new NotSupportedException(string.Concat("Could not load '", filename, "' file."));
		}

		public InputGeometry Read(string filename)
		{
			string extension = Path.GetExtension(filename);
			if (extension == ".node")
			{
				return FileReader.ReadNodeFile(filename);
			}
			if (extension != ".poly")
			{
				throw new NotSupportedException(string.Concat("File format '", extension, "' not supported."));
			}
			return FileReader.ReadPolyFile(filename);
		}

		public void Write(Mesh mesh, string filename)
		{
			FileWriter.WritePoly(mesh, Path.ChangeExtension(filename, ".poly"));
			FileWriter.WriteElements(mesh, Path.ChangeExtension(filename, ".ele"));
		}
	}
}