using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet.IO
{
	internal static class DataReader
	{
		public static int Reconstruct(Mesh mesh, InputGeometry input, ITriangle[] triangles)
		{
			Otri item;
			int num;
			int num1 = 0;
			Otri region = new Otri();
			Otri otri = new Otri();
			Otri l = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Osub osub = new Osub();
			int[] p0 = new int[3];
			int[] p1 = new int[2];
			int i = 0;
			int num2 = (triangles == null ? 0 : (int)triangles.Length);
			int count = input.segments.Count;
			mesh.inelements = num2;
			mesh.regions.AddRange(input.regions);
			for (i = 0; i < mesh.inelements; i++)
			{
				mesh.MakeTriangle(ref region);
			}
			if (mesh.behavior.Poly)
			{
				mesh.insegments = count;
				for (i = 0; i < mesh.insegments; i++)
				{
					mesh.MakeSegment(ref osub);
				}
			}
			List<Otri>[] otris = new List<Otri>[mesh.vertices.Count];
			for (i = 0; i < mesh.vertices.Count; i++)
			{
				Otri otri3 = new Otri()
				{
					triangle = Mesh.dummytri
				};
				otris[i] = new List<Otri>(3);
				otris[i].Add(otri3);
			}
			i = 0;
			foreach (Triangle value in mesh.triangles.Values)
			{
				region.triangle = value;
				p0[0] = triangles[i].P0;
				p0[1] = triangles[i].P1;
				p0[2] = triangles[i].P2;
				for (int j = 0; j < 3; j++)
				{
					if (p0[j] < 0 || p0[j] >= mesh.invertices)
					{
						SimpleLog.Instance.Error("Triangle has an invalid vertex index.", "MeshReader.Reconstruct()");
						throw new Exception("Triangle has an invalid vertex index.");
					}
				}
				region.triangle.region = triangles[i].Region;
				if (mesh.behavior.VarArea)
				{
					region.triangle.area = triangles[i].Area;
				}
				region.orient = 0;
				region.SetOrg(mesh.vertices[p0[0]]);
				region.SetDest(mesh.vertices[p0[1]]);
				region.SetApex(mesh.vertices[p0[2]]);
				region.orient = 0;
				while (region.orient < 3)
				{
					num = p0[region.orient];
					int count1 = otris[num].Count - 1;
					item = otris[num][count1];
					otris[num].Add(region);
					l = item;
					if (l.triangle != Mesh.dummytri)
					{
						Vertex vertex = region.Dest();
						Vertex vertex1 = region.Apex();
						do
						{
							Vertex vertex2 = l.Dest();
							Vertex vertex3 = l.Apex();
							if (vertex1 == vertex2)
							{
								region.Lprev(ref otri);
								otri.Bond(ref l);
							}
							if (vertex == vertex3)
							{
								l.Lprev(ref otri1);
								region.Bond(ref otri1);
							}
							count1--;
							item = otris[num][count1];
							l = item;
						}
						while (l.triangle != Mesh.dummytri);
					}
					region.orient = region.orient + 1;
				}
				i++;
			}
			num1 = 0;
			if (mesh.behavior.Poly)
			{
				int boundary = 0;
				i = 0;
				foreach (Segment segment in mesh.subsegs.Values)
				{
					osub.seg = segment;
					p1[0] = input.segments[i].P0;
					p1[1] = input.segments[i].P1;
					boundary = input.segments[i].Boundary;
					for (int k = 0; k < 2; k++)
					{
						if (p1[k] < 0 || p1[k] >= mesh.invertices)
						{
							SimpleLog.Instance.Error("Segment has an invalid vertex index.", "MeshReader.Reconstruct()");
							throw new Exception("Segment has an invalid vertex index.");
						}
					}
					osub.orient = 0;
					Vertex item1 = mesh.vertices[p1[0]];
					Vertex item2 = mesh.vertices[p1[1]];
					osub.SetOrg(item1);
					osub.SetDest(item2);
					osub.SetSegOrg(item1);
					osub.SetSegDest(item2);
					osub.seg.boundary = boundary;
					osub.orient = 0;
					while (osub.orient < 2)
					{
						num = p1[1 - osub.orient];
						int count2 = otris[num].Count - 1;
						Otri item3 = otris[num][count2];
						item = otris[num][count2];
						l = item;
						Vertex vertex4 = osub.Org();
						bool flag = true;
						while (flag && l.triangle != Mesh.dummytri)
						{
							if (vertex4 == l.Dest())
							{
								otris[num].Remove(item3);
								l.SegBond(ref osub);
								l.Sym(ref otri2);
								if (otri2.triangle == Mesh.dummytri)
								{
									mesh.InsertSubseg(ref l, 1);
									num1++;
								}
								flag = false;
							}
							count2--;
							item3 = otris[num][count2];
							item = otris[num][count2];
							l = item;
						}
						osub.orient = osub.orient + 1;
					}
					i++;
				}
			}
			for (i = 0; i < mesh.vertices.Count; i++)
			{
				int count3 = otris[i].Count - 1;
				item = otris[i][count3];
				for (l = item; l.triangle != Mesh.dummytri; l = item)
				{
					count3--;
					item = otris[i][count3];
					l.SegDissolve();
					l.Sym(ref otri2);
					if (otri2.triangle == Mesh.dummytri)
					{
						mesh.InsertSubseg(ref l, 1);
						num1++;
					}
				}
			}
			return num1;
		}
	}
}