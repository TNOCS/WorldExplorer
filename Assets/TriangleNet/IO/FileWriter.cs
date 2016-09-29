using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.IO
{
	public static class FileWriter
	{
		private static NumberFormatInfo nfi;

		static FileWriter()
		{
			FileWriter.nfi = CultureInfo.InvariantCulture.NumberFormat;
		}

		public static void Write(Mesh mesh, string filename)
		{
			FileWriter.WritePoly(mesh, Path.ChangeExtension(filename, ".poly"));
			FileWriter.WriteElements(mesh, Path.ChangeExtension(filename, ".ele"));
		}

		public static void WriteEdges(Mesh mesh, string filename)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Osub osub = new Osub();
			Behavior behavior = mesh.behavior;
			using (StreamWriter streamWriter = new StreamWriter(new FileStream(filename, FileMode.Create)))
			{
				streamWriter.WriteLine("{0} {1}", mesh.edges, (behavior.UseBoundaryMarkers ? "1" : "0"));
				long num = (long)0;
				foreach (Triangle value in mesh.triangles.Values)
				{
					otri.triangle = value;
					otri.orient = 0;
					while (otri.orient < 3)
					{
						otri.Sym(ref otri1);
						if (otri.triangle.id < otri1.triangle.id || otri1.triangle == Mesh.dummytri)
						{
							Vertex vertex = otri.Org();
							Vertex vertex1 = otri.Dest();
							if (!behavior.UseBoundaryMarkers)
							{
								streamWriter.WriteLine("{0} {1} {2}", num, vertex.id, vertex1.id);
							}
							else if (!behavior.useSegments)
							{
								StreamWriter streamWriter1 = streamWriter;
								object[] objArray = new object[] { num, vertex.id, vertex1.id, null };
								objArray[3] = (otri1.triangle == Mesh.dummytri ? "1" : "0");
								streamWriter1.WriteLine("{0} {1} {2} {3}", objArray);
							}
							else
							{
								otri.SegPivot(ref osub);
								if (osub.seg != Mesh.dummysub)
								{
									streamWriter.WriteLine("{0} {1} {2} {3}", new object[] { num, vertex.id, vertex1.id, osub.seg.boundary });
								}
								else
								{
									streamWriter.WriteLine("{0} {1} {2} {3}", new object[] { num, vertex.id, vertex1.id, 0 });
								}
							}
							num = num + (long)1;
						}
						otri.orient = otri.orient + 1;
					}
				}
			}
		}

		public static void WriteElements(Mesh mesh, string filename)
		{
			Otri otri = new Otri();
			bool flag = mesh.behavior.useRegions;
			int num = 0;
			otri.orient = 0;
			using (StreamWriter streamWriter = new StreamWriter(new FileStream(filename, FileMode.Create)))
			{
				streamWriter.WriteLine("{0} 3 {1}", mesh.triangles.Count, (flag ? 1 : 0));
				foreach (Triangle value in mesh.triangles.Values)
				{
					otri.triangle = value;
					Vertex vertex = otri.Org();
					Vertex vertex1 = otri.Dest();
					Vertex vertex2 = otri.Apex();
					streamWriter.Write("{0} {1} {2} {3}", new object[] { num, vertex.id, vertex1.id, vertex2.id });
					if (flag)
					{
						streamWriter.Write(" {0}", otri.triangle.region);
					}
					streamWriter.WriteLine();
					int num1 = num;
					num = num1 + 1;
					value.id = num1;
				}
			}
		}

		public static void WriteNeighbors(Mesh mesh, string filename)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			int num = 0;
			using (StreamWriter streamWriter = new StreamWriter(new FileStream(filename, FileMode.Create)))
			{
				streamWriter.WriteLine("{0} 3", mesh.triangles.Count);
				Mesh.dummytri.id = -1;
				foreach (Triangle value in mesh.triangles.Values)
				{
					otri.triangle = value;
					otri.orient = 1;
					otri.Sym(ref otri1);
					int num1 = otri1.triangle.id;
					otri.orient = 2;
					otri.Sym(ref otri1);
					int num2 = otri1.triangle.id;
					otri.orient = 0;
					otri.Sym(ref otri1);
					int num3 = otri1.triangle.id;
					object[] objArray = new object[4];
					int num4 = num;
					num = num4 + 1;
					objArray[0] = num4;
					objArray[1] = num1;
					objArray[2] = num2;
					objArray[3] = num3;
					streamWriter.WriteLine("{0} {1} {2} {3}", objArray);
				}
			}
		}

		public static void WriteNodes(Mesh mesh, string filename)
		{
			using (StreamWriter streamWriter = new StreamWriter(new FileStream(filename, FileMode.Create)))
			{
				FileWriter.WriteNodes(streamWriter, mesh);
			}
		}

		private static void WriteNodes(StreamWriter writer, Mesh mesh)
		{
			int count = mesh.vertices.Count;
			Behavior behavior = mesh.behavior;
			if (behavior.Jettison)
			{
				count = mesh.vertices.Count - mesh.undeads;
			}
			if (writer != null)
			{
				StreamWriter streamWriter = writer;
				object[] meshDim = new object[] { count, mesh.mesh_dim, mesh.nextras, null };
				meshDim[3] = (behavior.UseBoundaryMarkers ? "1" : "0");
				streamWriter.WriteLine("{0} {1} {2} {3}", meshDim);
				if (mesh.numbering == NodeNumbering.None)
				{
					mesh.Renumber();
				}
				if (mesh.numbering == NodeNumbering.Linear)
				{
					FileWriter.WriteNodes(writer, mesh.vertices.Values, behavior.UseBoundaryMarkers, mesh.nextras, behavior.Jettison);
					return;
				}
				Vertex[] vertexArray = new Vertex[mesh.vertices.Count];
				foreach (Vertex value in mesh.vertices.Values)
				{
					vertexArray[value.id] = value;
				}
				FileWriter.WriteNodes(writer, vertexArray, behavior.UseBoundaryMarkers, mesh.nextras, behavior.Jettison);
			}
		}

		private static void WriteNodes(StreamWriter writer, IEnumerable<Vertex> nodes, bool markers, int attribs, bool jettison)
		{
			int num = 0;
			foreach (Vertex node in nodes)
			{
				if (jettison && node.type == VertexType.UndeadVertex)
				{
					continue;
				}
				writer.Write("{0} {1} {2}", num, node.x.ToString(FileWriter.nfi), node.y.ToString(FileWriter.nfi));
				for (int i = 0; i < attribs; i++)
				{
					writer.Write(" {0}", node.attributes[i].ToString(FileWriter.nfi));
				}
				if (markers)
				{
					writer.Write(" {0}", node.mark);
				}
				writer.WriteLine();
				num++;
			}
		}

		public static void WriteOffFile(Mesh mesh, string filename)
		{
			Otri otri = new Otri();
			Vertex value = null;
			long count = (long)mesh.vertices.Count;
			if (mesh.behavior.Jettison)
			{
				count = (long)(mesh.vertices.Count - mesh.undeads);
			}
			int num = 0;
			using (StreamWriter streamWriter = new StreamWriter(new FileStream(filename, FileMode.Create)))
			{
				streamWriter.WriteLine("OFF");
				streamWriter.WriteLine("{0}  {1}  {2}", count, mesh.triangles.Count, mesh.edges);
				foreach (Vertex v in mesh.vertices.Values)
				{
					if (mesh.behavior.Jettison && v.type == VertexType.UndeadVertex)
					{
						continue;
					}
					double item = v[0];
					string str = item.ToString(FileWriter.nfi);
					item = v[1];
					streamWriter.WriteLine(" {0}  {1}  0.0", str, item.ToString(FileWriter.nfi));
					int num1 = num;
					num = num1 + 1;
					v.id = num1;
				}
				otri.orient = 0;
				foreach (Triangle triangle in mesh.triangles.Values)
				{
					otri.triangle = triangle;
					value = otri.Org();
					Vertex vertex = otri.Dest();
					Vertex vertex1 = otri.Apex();
					streamWriter.WriteLine(" 3   {0}  {1}  {2}", value.id, vertex.id, vertex1.id);
				}
			}
		}

		public static void WritePoly(Mesh mesh, string filename)
		{
			FileWriter.WritePoly(mesh, filename, true);
		}

		public static void WritePoly(Mesh mesh, string filename, bool writeNodes)
		{
			double x;
			Osub osub = new Osub();
			bool useBoundaryMarkers = mesh.behavior.UseBoundaryMarkers;
			using (StreamWriter streamWriter = new StreamWriter(new FileStream(filename, FileMode.Create)))
			{
				if (!writeNodes)
				{
					streamWriter.WriteLine("0 {0} {1} {2}", mesh.mesh_dim, mesh.nextras, (useBoundaryMarkers ? "1" : "0"));
				}
				else
				{
					FileWriter.WriteNodes(streamWriter, mesh);
				}
				streamWriter.WriteLine("{0} {1}", mesh.subsegs.Count, (useBoundaryMarkers ? "1" : "0"));
				osub.orient = 0;
				int num = 0;
				foreach (Segment value in mesh.subsegs.Values)
				{
					osub.seg = value;
					Vertex vertex = osub.Org();
					Vertex vertex1 = osub.Dest();
					if (!useBoundaryMarkers)
					{
						streamWriter.WriteLine("{0} {1} {2}", num, vertex.id, vertex1.id);
					}
					else
					{
						streamWriter.WriteLine("{0} {1} {2} {3}", new object[] { num, vertex.id, vertex1.id, osub.seg.boundary });
					}
					num++;
				}
				num = 0;
				streamWriter.WriteLine("{0}", mesh.holes.Count);
				foreach (Point hole in mesh.holes)
				{
					int num1 = num;
					num = num1 + 1;
					object obj = num1;
					x = hole.X;
					string str = x.ToString(FileWriter.nfi);
					x = hole.Y;
					streamWriter.WriteLine("{0} {1} {2}", obj, str, x.ToString(FileWriter.nfi));
				}
				if (mesh.regions.Count > 0)
				{
					num = 0;
					streamWriter.WriteLine("{0}", mesh.regions.Count);
					foreach (RegionPointer region in mesh.regions)
					{
						object[] objArray = new object[] { num, null, null, null };
						x = region.point.X;
						objArray[1] = x.ToString(FileWriter.nfi);
						x = region.point.Y;
						objArray[2] = x.ToString(FileWriter.nfi);
						objArray[3] = region.id;
						streamWriter.WriteLine("{0} {1} {2} {3}", objArray);
						num++;
					}
				}
			}
		}

		public static void WriteVoronoi(Mesh mesh, string filename)
		{
			Vertex vertex;
			Vertex vertex1;
			double x;
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			double num = 0;
			double num1 = 0;
			int num2 = 0;
			otri.orient = 0;
			using (StreamWriter streamWriter = new StreamWriter(new FileStream(filename, FileMode.Create)))
			{
				streamWriter.WriteLine("{0} 2 {1} 0", mesh.triangles.Count, mesh.nextras);
				foreach (Triangle value in mesh.triangles.Values)
				{
					otri.triangle = value;
					vertex = otri.Org();
					vertex1 = otri.Dest();
					Vertex vertex2 = otri.Apex();
					Point point = Primitives.FindCircumcenter(vertex, vertex1, vertex2, ref num, ref num1);
					object obj = num2;
					x = point.X;
					string str = x.ToString(FileWriter.nfi);
					x = point.Y;
					streamWriter.Write("{0} {1} {2}", obj, str, x.ToString(FileWriter.nfi));
					for (int i = 0; i < mesh.nextras; i++)
					{
						streamWriter.Write(" 0");
					}
					streamWriter.WriteLine();
					int num3 = num2;
					num2 = num3 + 1;
					otri.triangle.id = num3;
				}
				streamWriter.WriteLine("{0} 0", mesh.edges);
				num2 = 0;
				foreach (Triangle triangle in mesh.triangles.Values)
				{
					otri.triangle = triangle;
					otri.orient = 0;
					while (otri.orient < 3)
					{
						otri.Sym(ref otri1);
						if (otri.triangle.id < otri1.triangle.id || otri1.triangle == Mesh.dummytri)
						{
							int num4 = otri.triangle.id;
							if (otri1.triangle != Mesh.dummytri)
							{
								int num5 = otri1.triangle.id;
								streamWriter.WriteLine("{0} {1} {2}", num2, num4, num5);
							}
							else
							{
								vertex = otri.Org();
								vertex1 = otri.Dest();
								object[] objArray = new object[] { num2, num4, null, null };
								x = vertex1[1] - vertex[1];
								objArray[2] = x.ToString(FileWriter.nfi);
								x = vertex[0] - vertex1[0];
								objArray[3] = x.ToString(FileWriter.nfi);
								streamWriter.WriteLine("{0} {1} -1 {2} {3}", objArray);
							}
							num2++;
						}
						otri.orient = otri.orient + 1;
					}
				}
			}
		}
	}
}