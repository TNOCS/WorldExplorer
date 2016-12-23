using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.IO
{
	internal class DebugWriter
	{
		private static NumberFormatInfo nfi;

		private int iteration;

		private string session;

		private StreamWriter stream;

		private string tmpFile;

		private int[] vertices;

		private int triangles;

		private readonly static DebugWriter instance;

		public static DebugWriter Session
		{
			get
			{
				return DebugWriter.instance;
			}
		}

		static DebugWriter()
		{
			DebugWriter.nfi = CultureInfo.InvariantCulture.NumberFormat;
			DebugWriter.instance = new DebugWriter();
		}

		private DebugWriter()
		{
		}

		public void Finish()
		{
			this.Finish(string.Concat(this.session, ".mshx"));
		}

		private void Finish(string path)
		{
			if (this.stream != null)
			{
				this.stream.Flush();
				this.stream.Dispose();
				this.stream = null;
				string str = string.Concat("#!N", this.iteration, Environment.NewLine);
				using (FileStream fileStream = new FileStream(path, FileMode.Create))
				{
					using (GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Compress, false))
					{
						byte[] bytes = Encoding.UTF8.GetBytes(str);
						gZipStream.Write(bytes, 0, (int)bytes.Length);
						bytes = File.ReadAllBytes(this.tmpFile);
						gZipStream.Write(bytes, 0, (int)bytes.Length);
					}
				}
				File.Delete(this.tmpFile);
			}
		}

		private void HashVertices(Mesh mesh)
		{
			if (this.vertices == null || mesh.Vertices.Count != (int)this.vertices.Length)
			{
				this.vertices = new int[mesh.Vertices.Count];
			}
			int num = 0;
			foreach (Vertex vertex in mesh.Vertices)
			{
				int num1 = num;
				num = num1 + 1;
				this.vertices[num1] = vertex.id;
			}
		}

		public void Start(string session)
		{
			this.iteration = 0;
			this.session = session;
			if (this.stream != null)
			{
				throw new Exception("A session is active. Finish before starting a new.");
			}
			this.tmpFile = Path.GetTempFileName();
			this.stream = new StreamWriter(new FileStream(this.tmpFile, FileMode.Create));
		}

		private bool VerticesChanged(Mesh mesh)
		{
			bool flag;
			if (this.vertices == null || mesh.Vertices.Count != (int)this.vertices.Length)
			{
				return true;
			}
			int num = 0;
			using (IEnumerator<Vertex> enumerator = mesh.Vertices.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int num1 = num;
					num = num1 + 1;
					if (enumerator.Current.id == this.vertices[num1])
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			// return flag;
		}

		public void Write(Mesh mesh, bool skip = false)
		{
			this.WriteMesh(mesh, skip);
			this.triangles = mesh.Triangles.Count;
		}

		private void WriteGeometry(InputGeometry geometry)
		{
			StreamWriter streamWriter = this.stream;
			int num = this.iteration;
			this.iteration = num + 1;
			streamWriter.WriteLine("#!G{0}", num);
		}

		private void WriteMesh(Mesh mesh, bool skip)
		{
			Vertex vertex;
			Vertex vertex1;
			if (this.triangles == mesh.triangles.Count & skip)
			{
				return;
			}
			StreamWriter streamWriter = this.stream;
			int num = this.iteration;
			this.iteration = num + 1;
			streamWriter.WriteLine("#!M{0}", num);
			if (!this.VerticesChanged(mesh))
			{
				this.stream.WriteLine("0");
			}
			else
			{
				this.HashVertices(mesh);
				this.stream.WriteLine("{0}", mesh.vertices.Count);
				foreach (Vertex value in mesh.vertices.Values)
				{
					this.stream.WriteLine("{0} {1} {2} {3}", new object[] { value.hash, value.x.ToString(DebugWriter.nfi), value.y.ToString(DebugWriter.nfi), value.mark });
				}
			}
			this.stream.WriteLine("{0}", mesh.subsegs.Count);
			Osub osub = new Osub()
			{
				orient = 0
			};
			foreach (Segment segment in mesh.subsegs.Values)
			{
				if (segment.hash <= 0)
				{
					continue;
				}
				osub.seg = segment;
				vertex = osub.Org();
				vertex1 = osub.Dest();
				this.stream.WriteLine("{0} {1} {2} {3}", new object[] { osub.seg.hash, vertex.hash, vertex1.hash, osub.seg.boundary });
			}
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			otri.orient = 0;
			this.stream.WriteLine("{0}", mesh.triangles.Count);
			foreach (Triangle triangle in mesh.triangles.Values)
			{
				otri.triangle = triangle;
				vertex = otri.Org();
				vertex1 = otri.Dest();
				Vertex vertex2 = otri.Apex();
				int num1 = (vertex == null ? -1 : vertex.hash);
				int num2 = (vertex1 == null ? -1 : vertex1.hash);
				int num3 = (vertex2 == null ? -1 : vertex2.hash);
				this.stream.Write("{0} {1} {2} {3}", new object[] { otri.triangle.hash, num1, num2, num3 });
				otri.orient = 1;
				otri.Sym(ref otri1);
				int num4 = otri1.triangle.hash;
				otri.orient = 2;
				otri.Sym(ref otri1);
				int num5 = otri1.triangle.hash;
				otri.orient = 0;
				otri.Sym(ref otri1);
				int num6 = otri1.triangle.hash;
				this.stream.WriteLine(" {0} {1} {2}", num4, num5, num6);
			}
		}
	}
}