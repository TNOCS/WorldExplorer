using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet
{
	internal class Quality
	{
		private Queue<BadSubseg> badsubsegs;

		private BadTriQueue queue;

		private Mesh mesh;

		private Behavior behavior;

		private NewLocation newLocation;

		private Func<Point, Point, Point, double, bool> userTest;

		private ILog<SimpleLogItem> logger;

		public Quality(Mesh mesh)
		{
			this.logger = SimpleLog.Instance;
			this.badsubsegs = new Queue<BadSubseg>();
			this.queue = new BadTriQueue();
			this.mesh = mesh;
			this.behavior = mesh.behavior;
			this.newLocation = new NewLocation(mesh);
		}

		public void AddBadSubseg(BadSubseg badseg)
		{
			this.badsubsegs.Enqueue(badseg);
		}

		public bool CheckDelaunay()
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Osub osub = new Osub();
			bool noExact = Behavior.NoExact;
			Behavior.NoExact = false;
			int num = 0;
			foreach (Triangle value in this.mesh.triangles.Values)
			{
				otri.triangle = value;
				otri.orient = 0;
				while (otri.orient < 3)
				{
					Vertex vertex = otri.Org();
					Vertex vertex1 = otri.Dest();
					Vertex vertex2 = otri.Apex();
					otri.Sym(ref otri1);
					Vertex vertex3 = otri1.Apex();
					bool flag = (otri1.triangle == Mesh.dummytri || Otri.IsDead(otri1.triangle) || otri.triangle.id >= otri1.triangle.id || !(vertex != this.mesh.infvertex1) || !(vertex != this.mesh.infvertex2) || !(vertex != this.mesh.infvertex3) || !(vertex1 != this.mesh.infvertex1) || !(vertex1 != this.mesh.infvertex2) || !(vertex1 != this.mesh.infvertex3) || !(vertex2 != this.mesh.infvertex1) || !(vertex2 != this.mesh.infvertex2) || !(vertex2 != this.mesh.infvertex3) || !(vertex3 != this.mesh.infvertex1) || !(vertex3 != this.mesh.infvertex2) ? false : vertex3 != this.mesh.infvertex3);
					if (this.mesh.checksegments & flag)
					{
						otri.SegPivot(ref osub);
						if (osub.seg != Mesh.dummysub)
						{
							flag = false;
						}
					}
					if (flag && Primitives.NonRegular(vertex, vertex1, vertex2, vertex3) > 0)
					{
						this.logger.Warning(string.Format("Non-regular pair of triangles found (IDs {0}/{1}).", otri.triangle.id, otri1.triangle.id), "Quality.CheckDelaunay()");
						num++;
					}
					otri.orient = otri.orient + 1;
				}
			}
			if (num == 0)
			{
				this.logger.Info("Mesh is Delaunay.");
			}
			Behavior.NoExact = noExact;
			return num == 0;
		}

		public bool CheckMesh()
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			bool noExact = Behavior.NoExact;
			Behavior.NoExact = false;
			int num = 0;
			foreach (Triangle value in this.mesh.triangles.Values)
			{
				otri.triangle = value;
				otri.orient = 0;
				while (otri.orient < 3)
				{
					Vertex vertex = otri.Org();
					Vertex vertex1 = otri.Dest();
					if (otri.orient == 0 && Primitives.CounterClockwise(vertex, vertex1, otri.Apex()) <= 0)
					{
						this.logger.Warning("Triangle is flat or inverted.", "Quality.CheckMesh()");
						num++;
					}
					otri.Sym(ref otri1);
					if (otri1.triangle != Mesh.dummytri)
					{
						otri1.Sym(ref otri2);
						if (otri.triangle != otri2.triangle || otri.orient != otri2.orient)
						{
							if (otri.triangle == otri2.triangle)
							{
								this.logger.Warning("Asymmetric triangle-triangle bond: (Right triangle, wrong orientation)", "Quality.CheckMesh()");
							}
							num++;
						}
						Vertex vertex2 = otri1.Org();
						if (vertex != otri1.Dest() || vertex1 != vertex2)
						{
							this.logger.Warning("Mismatched edge coordinates between two triangles.", "Quality.CheckMesh()");
							num++;
						}
					}
					otri.orient = otri.orient + 1;
				}
			}
			this.mesh.MakeVertexMap();
			foreach (Vertex value1 in this.mesh.vertices.Values)
			{
				if (value1.tri.triangle != null)
				{
					continue;
				}
				this.logger.Warning(string.Concat("Vertex (ID ", value1.id, ") not connected to mesh (duplicate input vertex?)"), "Quality.CheckMesh()");
			}
			if (num == 0)
			{
				this.logger.Info("Mesh topology appears to be consistent.");
			}
			Behavior.NoExact = noExact;
			return num == 0;
		}

		public int CheckSeg4Encroach(ref Osub testsubseg)
		{
			double num;
			Vertex vertex;
			Otri otri = new Otri();
			Osub osub = new Osub();
			int num1 = 0;
			int num2 = 0;
			Vertex vertex1 = testsubseg.Org();
			Vertex vertex2 = testsubseg.Dest();
			testsubseg.TriPivot(ref otri);
			if (otri.triangle != Mesh.dummytri)
			{
				num2++;
				vertex = otri.Apex();
				num = (vertex1.x - vertex.x) * (vertex2.x - vertex.x) + (vertex1.y - vertex.y) * (vertex2.y - vertex.y);
				if (num < 0 && (this.behavior.ConformingDelaunay || num * num >= (2 * this.behavior.goodAngle - 1) * (2 * this.behavior.goodAngle - 1) * ((vertex1.x - vertex.x) * (vertex1.x - vertex.x) + (vertex1.y - vertex.y) * (vertex1.y - vertex.y)) * ((vertex2.x - vertex.x) * (vertex2.x - vertex.x) + (vertex2.y - vertex.y) * (vertex2.y - vertex.y))))
				{
					num1 = 1;
				}
			}
			testsubseg.Sym(ref osub);
			osub.TriPivot(ref otri);
			if (otri.triangle != Mesh.dummytri)
			{
				num2++;
				vertex = otri.Apex();
				num = (vertex1.x - vertex.x) * (vertex2.x - vertex.x) + (vertex1.y - vertex.y) * (vertex2.y - vertex.y);
				if (num < 0 && (this.behavior.ConformingDelaunay || num * num >= (2 * this.behavior.goodAngle - 1) * (2 * this.behavior.goodAngle - 1) * ((vertex1.x - vertex.x) * (vertex1.x - vertex.x) + (vertex1.y - vertex.y) * (vertex1.y - vertex.y)) * ((vertex2.x - vertex.x) * (vertex2.x - vertex.x) + (vertex2.y - vertex.y) * (vertex2.y - vertex.y))))
				{
					num1 = num1 + 2;
				}
			}
			if (num1 > 0 && (this.behavior.NoBisect == 0 || this.behavior.NoBisect == 1 && num2 == 2))
			{
				BadSubseg badSubseg = new BadSubseg();
				if (num1 != 1)
				{
					badSubseg.encsubseg = osub;
					badSubseg.subsegorg = vertex2;
					badSubseg.subsegdest = vertex1;
				}
				else
				{
					badSubseg.encsubseg = testsubseg;
					badSubseg.subsegorg = vertex1;
					badSubseg.subsegdest = vertex2;
				}
				this.badsubsegs.Enqueue(badSubseg);
			}
			return num1;
		}

		public void EnforceQuality()
		{
			this.TallyEncs();
			this.SplitEncSegs(false);
			if (this.behavior.MinAngle > 0 || this.behavior.VarArea || this.behavior.fixedArea || this.behavior.Usertest)
			{
				this.TallyFaces();
				this.mesh.checkquality = true;
				while (this.queue.Count > 0 && this.mesh.steinerleft != 0)
				{
					BadTriangle badTriangle = this.queue.Dequeue();
					this.SplitTriangle(badTriangle);
					if (this.badsubsegs.Count <= 0)
					{
						continue;
					}
					this.queue.Enqueue(badTriangle);
					this.SplitEncSegs(true);
				}
			}
			if (Behavior.Verbose && this.behavior.ConformingDelaunay && this.badsubsegs.Count > 0 && this.mesh.steinerleft == 0)
			{
				this.logger.Warning("I ran out of Steiner points, but the mesh has encroached subsegments, and therefore might not be truly Delaunay. If the Delaunay property is important to you, try increasing the number of Steiner points.", "Quality.EnforceQuality()");
			}
		}

		private void SplitEncSegs(bool triflaws)
		{
			Vertex vertex;
			double num;
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Osub osub = new Osub();
			Osub osub1 = new Osub();
			while (this.badsubsegs.Count > 0 && this.mesh.steinerleft != 0)
			{
				BadSubseg badSubseg = this.badsubsegs.Dequeue();
				osub1 = badSubseg.encsubseg;
				Vertex vertex1 = osub1.Org();
				Vertex vertex2 = osub1.Dest();
				if (!Osub.IsDead(osub1.seg) && vertex1 == badSubseg.subsegorg && vertex2 == badSubseg.subsegdest)
				{
					osub1.TriPivot(ref otri);
					otri.Lnext(ref otri1);
					otri1.SegPivot(ref osub);
					bool flag = osub.seg != Mesh.dummysub;
					otri1.LnextSelf();
					otri1.SegPivot(ref osub);
					bool flag1 = osub.seg != Mesh.dummysub;
					if (!this.behavior.ConformingDelaunay && !flag && !flag1)
					{
						vertex = otri.Apex();
						while (vertex.type == VertexType.FreeVertex && (vertex1.x - vertex.x) * (vertex2.x - vertex.x) + (vertex1.y - vertex.y) * (vertex2.y - vertex.y) < 0)
						{
							this.mesh.DeleteVertex(ref otri1);
							osub1.TriPivot(ref otri);
							vertex = otri.Apex();
							otri.Lprev(ref otri1);
						}
					}
					otri.Sym(ref otri1);
					if (otri1.triangle != Mesh.dummytri)
					{
						otri1.LnextSelf();
						otri1.SegPivot(ref osub);
						bool flag2 = osub.seg != Mesh.dummysub;
						flag1 = flag1 | flag2;
						otri1.LnextSelf();
						otri1.SegPivot(ref osub);
						bool flag3 = osub.seg != Mesh.dummysub;
						flag = flag | flag3;
						if (!this.behavior.ConformingDelaunay && !flag3 && !flag2)
						{
							vertex = otri1.Org();
							while (vertex.type == VertexType.FreeVertex && (vertex1.x - vertex.x) * (vertex2.x - vertex.x) + (vertex1.y - vertex.y) * (vertex2.y - vertex.y) < 0)
							{
								this.mesh.DeleteVertex(ref otri1);
								otri.Sym(ref otri1);
								vertex = otri1.Apex();
								otri1.LprevSelf();
							}
						}
					}
					if (!(flag | flag1))
					{
						num = 0.5;
					}
					else
					{
						double num1 = Math.Sqrt((vertex2.x - vertex1.x) * (vertex2.x - vertex1.x) + (vertex2.y - vertex1.y) * (vertex2.y - vertex1.y));
						double num2 = 1;
						while (num1 > 3 * num2)
						{
							num2 = num2 * 2;
						}
						while (num1 < 1.5 * num2)
						{
							num2 = num2 * 0.5;
						}
						num = num2 / num1;
						if (flag1)
						{
							num = 1 - num;
						}
					}
					Vertex vertex3 = new Vertex(vertex1.x + num * (vertex2.x - vertex1.x), vertex1.y + num * (vertex2.y - vertex1.y), osub1.Mark(), this.mesh.nextras)
					{
						type = VertexType.SegmentVertex
					};
					Mesh mesh = this.mesh;
					int hashVtx = mesh.hash_vtx;
					mesh.hash_vtx = hashVtx + 1;
					vertex3.hash = hashVtx;
					vertex3.id = vertex3.hash;
					this.mesh.vertices.Add(vertex3.hash, vertex3);
					for (int i = 0; i < this.mesh.nextras; i++)
					{
						vertex3.attributes[i] = vertex1.attributes[i] + num * (vertex2.attributes[i] - vertex1.attributes[i]);
					}
					if (!Behavior.NoExact)
					{
						double num3 = Primitives.CounterClockwise(vertex1, vertex2, vertex3);
						double num4 = (vertex1.x - vertex2.x) * (vertex1.x - vertex2.x) + (vertex1.y - vertex2.y) * (vertex1.y - vertex2.y);
						if (num3 != 0 && num4 != 0)
						{
							num3 = num3 / num4;
							if (!double.IsNaN(num3))
							{
								Vertex vertex4 = vertex3;
								vertex4.x = vertex4.x + num3 * (vertex2.y - vertex1.y);
								Vertex vertex5 = vertex3;
								vertex5.y = vertex5.y + num3 * (vertex1.x - vertex2.x);
							}
						}
					}
					if (vertex3.x == vertex1.x && vertex3.y == vertex1.y || vertex3.x == vertex2.x && vertex3.y == vertex2.y)
					{
						this.logger.Error("Ran out of precision: I attempted to split a segment to a smaller size than can be accommodated by the finite precision of floating point arithmetic.", "Quality.SplitEncSegs()");
						throw new Exception("Ran out of precision");
					}
					InsertVertexResult insertVertexResult = this.mesh.InsertVertex(vertex3, ref otri, ref osub1, true, triflaws);
					if (insertVertexResult != InsertVertexResult.Successful && insertVertexResult != InsertVertexResult.Encroaching)
					{
						this.logger.Error("Failure to split a segment.", "Quality.SplitEncSegs()");
						throw new Exception("Failure to split a segment.");
					}
					if (this.mesh.steinerleft > 0)
					{
						Mesh mesh1 = this.mesh;
						mesh1.steinerleft = mesh1.steinerleft - 1;
					}
					this.CheckSeg4Encroach(ref osub1);
					osub1.NextSelf();
					this.CheckSeg4Encroach(ref osub1);
				}
				badSubseg.subsegorg = null;
			}
		}

		private void SplitTriangle(BadTriangle badtri)
		{
			Point point;
			Otri otri = new Otri();
			double num = 0;
			double num1 = 0;
			otri = badtri.poortri;
			Vertex vertex = otri.Org();
			Vertex vertex1 = otri.Dest();
			Vertex vertex2 = otri.Apex();
			if (!Otri.IsDead(otri.triangle) && vertex == badtri.triangorg && vertex1 == badtri.triangdest && vertex2 == badtri.triangapex)
			{
				bool flag = false;
				point = (this.behavior.fixedArea || this.behavior.VarArea ? Primitives.FindCircumcenter(vertex, vertex1, vertex2, ref num, ref num1, this.behavior.offconstant) : this.newLocation.FindLocation(vertex, vertex1, vertex2, ref num, ref num1, true, otri));
				if ((point.x != vertex.x || point.y != vertex.y) && (point.x != vertex1.x || point.y != vertex1.y) && (point.x != vertex2.x || point.y != vertex2.y))
				{
					Vertex vertex3 = new Vertex(point.x, point.y, 0, this.mesh.nextras)
					{
						type = VertexType.FreeVertex
					};
					for (int i = 0; i < this.mesh.nextras; i++)
					{
						vertex3.attributes[i] = vertex.attributes[i] + num * (vertex1.attributes[i] - vertex.attributes[i]) + num1 * (vertex2.attributes[i] - vertex.attributes[i]);
					}
					if (num1 < num)
					{
						otri.LprevSelf();
					}
					Osub osub = new Osub();
					InsertVertexResult insertVertexResult = this.mesh.InsertVertex(vertex3, ref otri, ref osub, true, true);
					if (insertVertexResult == InsertVertexResult.Successful)
					{
						Mesh mesh = this.mesh;
						int hashVtx = mesh.hash_vtx;
						mesh.hash_vtx = hashVtx + 1;
						vertex3.hash = hashVtx;
						vertex3.id = vertex3.hash;
						this.mesh.vertices.Add(vertex3.hash, vertex3);
						if (this.mesh.steinerleft > 0)
						{
							Mesh mesh1 = this.mesh;
							mesh1.steinerleft = mesh1.steinerleft - 1;
						}
					}
					else if (insertVertexResult == InsertVertexResult.Encroaching)
					{
						this.mesh.UndoVertex();
					}
					else if (insertVertexResult != InsertVertexResult.Violating && Behavior.Verbose)
					{
						this.logger.Warning("New vertex falls on existing vertex.", "Quality.SplitTriangle()");
						flag = true;
					}
				}
				else if (Behavior.Verbose)
				{
					this.logger.Warning("New vertex falls on existing vertex.", "Quality.SplitTriangle()");
					flag = true;
				}
				if (flag)
				{
					this.logger.Error("The new vertex is at the circumcenter of triangle: This probably means that I am trying to refine triangles to a smaller size than can be accommodated by the finite precision of floating point arithmetic.", "Quality.SplitTriangle()");
					throw new Exception("The new vertex is at the circumcenter of triangle.");
				}
			}
		}

		private void TallyEncs()
		{
			Osub osub = new Osub()
			{
				orient = 0
			};
			foreach (Segment value in this.mesh.subsegs.Values)
			{
				osub.seg = value;
				this.CheckSeg4Encroach(ref osub);
			}
		}

		private void TallyFaces()
		{
			Otri otri = new Otri()
			{
				orient = 0
			};
			foreach (Triangle value in this.mesh.triangles.Values)
			{
				otri.triangle = value;
				this.TestTriangle(ref otri);
			}
		}

		public void TestTriangle(ref Otri testtri)
		{
			Vertex vertex;
			Vertex vertex1;
			double num;
			double num1;
			double num2;
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Osub osub = new Osub();
			Vertex vertex2 = testtri.Org();
			Vertex vertex3 = testtri.Dest();
			Vertex vertex4 = testtri.Apex();
			double num3 = vertex2.x - vertex3.x;
			double num4 = vertex2.y - vertex3.y;
			double num5 = vertex3.x - vertex4.x;
			double num6 = vertex3.y - vertex4.y;
			double num7 = vertex4.x - vertex2.x;
			double num8 = vertex4.y - vertex2.y;
			double num9 = num3 * num3;
			double num10 = num4 * num4;
			double num11 = num5 * num5;
			double num12 = num6 * num6;
			double num13 = num8 * num8;
			double num14 = num9 + num10;
			double num15 = num11 + num12;
			double num16 = num7 * num7 + num13;
			if (num14 < num15 && num14 < num16)
			{
				num = num14;
				num1 = num5 * num7 + num6 * num8;
				num1 = num1 * num1 / (num15 * num16);
				vertex = vertex2;
				vertex1 = vertex3;
				testtri.Copy(ref otri);
			}
			else if (num15 >= num16)
			{
				num = num16;
				num1 = num3 * num5 + num4 * num6;
				num1 = num1 * num1 / (num14 * num15);
				vertex = vertex4;
				vertex1 = vertex2;
				testtri.Lprev(ref otri);
			}
			else
			{
				num = num15;
				num1 = num3 * num7 + num4 * num8;
				num1 = num1 * num1 / (num14 * num16);
				vertex = vertex3;
				vertex1 = vertex4;
				testtri.Lnext(ref otri);
			}
			if (this.behavior.VarArea || this.behavior.fixedArea || this.behavior.Usertest)
			{
				double num17 = 0.5 * (num3 * num6 - num4 * num5);
				if (this.behavior.fixedArea && num17 > this.behavior.MaxArea)
				{
					this.queue.Enqueue(ref testtri, num, vertex4, vertex2, vertex3);
					return;
				}
				if (this.behavior.VarArea && num17 > testtri.triangle.area && testtri.triangle.area > 0)
				{
					this.queue.Enqueue(ref testtri, num, vertex4, vertex2, vertex3);
					return;
				}
				if (this.behavior.Usertest && this.userTest != null && this.userTest(vertex2, vertex3, vertex4, num17))
				{
					this.queue.Enqueue(ref testtri, num, vertex4, vertex2, vertex3);
					return;
				}
			}
			if (num14 <= num15 || num14 <= num16)
			{
				num2 = (num15 <= num16 ? (num14 + num15 - num16) / (2 * Math.Sqrt(num14 * num15)) : (num14 + num16 - num15) / (2 * Math.Sqrt(num14 * num16)));
			}
			else
			{
				num2 = (num15 + num16 - num14) / (2 * Math.Sqrt(num15 * num16));
			}
			if (num1 > this.behavior.goodAngle || num2 < this.behavior.maxGoodAngle && this.behavior.MaxAngle != 0)
			{
				if (vertex.type == VertexType.SegmentVertex && vertex1.type == VertexType.SegmentVertex)
				{
					otri.SegPivot(ref osub);
					if (osub.seg == Mesh.dummysub)
					{
						otri.Copy(ref otri1);
						do
						{
							otri.OprevSelf();
							otri.SegPivot(ref osub);
						}
						while (osub.seg == Mesh.dummysub);
						Vertex vertex5 = osub.SegOrg();
						Vertex vertex6 = osub.SegDest();
						do
						{
							otri1.DnextSelf();
							otri1.SegPivot(ref osub);
						}
						while (osub.seg == Mesh.dummysub);
						Vertex vertex7 = osub.SegOrg();
						Vertex vertex8 = osub.SegDest();
						Vertex vertex9 = null;
						if (vertex6.x == vertex7.x && vertex6.y == vertex7.y)
						{
							vertex9 = vertex6;
						}
						else if (vertex5.x == vertex8.x && vertex5.y == vertex8.y)
						{
							vertex9 = vertex5;
						}
						if (vertex9 != null)
						{
							double num18 = (vertex.x - vertex9.x) * (vertex.x - vertex9.x) + (vertex.y - vertex9.y) * (vertex.y - vertex9.y);
							double num19 = (vertex1.x - vertex9.x) * (vertex1.x - vertex9.x) + (vertex1.y - vertex9.y) * (vertex1.y - vertex9.y);
							if (num18 < 1.001 * num19 && num18 > 0.999 * num19)
							{
								return;
							}
						}
					}
				}
				this.queue.Enqueue(ref testtri, num, vertex4, vertex2, vertex3);
			}
		}
	}
}