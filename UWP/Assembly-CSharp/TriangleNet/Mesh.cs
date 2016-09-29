using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using TriangleNet.Algorithm;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.IO;
using TriangleNet.Log;
using TriangleNet.Smoothing;
using TriangleNet.Tools;

namespace TriangleNet
{
	public class Mesh
	{
		private ILog<SimpleLogItem> logger;

		private Quality quality;

		private Stack<Otri> flipstack;

		internal Dictionary<int, Triangle> triangles;

		internal Dictionary<int, Segment> subsegs;

		internal Dictionary<int, Vertex> vertices;

		internal int hash_vtx;

		internal int hash_seg;

		internal int hash_tri;

		internal List<Point> holes;

		internal List<RegionPointer> regions;

		internal BoundingBox bounds;

		internal int invertices;

		internal int inelements;

		internal int insegments;

		internal int undeads;

		internal int edges;

		internal int mesh_dim;

		internal int nextras;

		internal int hullsize;

		internal int steinerleft;

		internal bool checksegments;

		internal bool checkquality;

		internal Vertex infvertex1;

		internal Vertex infvertex2;

		internal Vertex infvertex3;

		internal static Triangle dummytri;

		internal static Segment dummysub;

		internal TriangleLocator locator;

		internal TriangleNet.Behavior behavior;

		internal NodeNumbering numbering;

		public TriangleNet.Behavior Behavior
		{
			get
			{
				return this.behavior;
			}
		}

		public BoundingBox Bounds
		{
			get
			{
				return this.bounds;
			}
		}

		public NodeNumbering CurrentNumbering
		{
			get
			{
				return this.numbering;
			}
		}

		public IEnumerable<Edge> Edges
		{
			get
			{
				EdgeEnumerator edgeEnumerator = new EdgeEnumerator(this);
				while (edgeEnumerator.MoveNext())
				{
					yield return edgeEnumerator.Current;
				}
			}
		}

		public IList<Point> Holes
		{
			get
			{
				return this.holes;
			}
		}

		public bool IsPolygon
		{
			get
			{
				return this.insegments > 0;
			}
		}

		public int NumberOfEdges
		{
			get
			{
				return this.edges;
			}
		}

		public int NumberOfInputPoints
		{
			get
			{
				return this.invertices;
			}
		}

		public ICollection<Segment> Segments
		{
			get
			{
				return this.subsegs.Values;
			}
		}

		public ICollection<Triangle> Triangles
		{
			get
			{
				return this.triangles.Values;
			}
		}

		public ICollection<Vertex> Vertices
		{
			get
			{
				return this.vertices.Values;
			}
		}

		public Mesh() : this(new TriangleNet.Behavior(false, 20))
		{
		}

		public Mesh(TriangleNet.Behavior behavior)
		{
			this.behavior = behavior;
			this.logger = SimpleLog.Instance;
			behavior = new TriangleNet.Behavior(false, 20);
			this.vertices = new Dictionary<int, Vertex>();
			this.triangles = new Dictionary<int, Triangle>();
			this.subsegs = new Dictionary<int, Segment>();
			this.flipstack = new Stack<Otri>();
			this.holes = new List<Point>();
			this.regions = new List<RegionPointer>();
			this.quality = new Quality(this);
			this.locator = new TriangleLocator(this);
			Primitives.ExactInit();
			if (Mesh.dummytri == null)
			{
				this.DummyInit();
			}
		}

		public void Check(out bool isConsistent, out bool isDelaunay)
		{
			isConsistent = this.quality.CheckMesh();
			isDelaunay = this.quality.CheckDelaunay();
		}

		private void ConstrainedEdge(ref Otri starttri, Vertex endpoint2, int newmark)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Osub osub = new Osub();
			Vertex vertex = starttri.Org();
			starttri.Lnext(ref otri);
			this.Flip(ref otri);
			bool flag = false;
			bool flag1 = false;
			do
			{
				Vertex vertex1 = otri.Org();
				if (vertex1.x != endpoint2.x || vertex1.y != endpoint2.y)
				{
					double num = Primitives.CounterClockwise(vertex, endpoint2, vertex1);
					if (num != 0)
					{
						if (num <= 0)
						{
							this.DelaunayFixup(ref otri, false);
							otri.OprevSelf();
						}
						else
						{
							otri.Oprev(ref otri1);
							this.DelaunayFixup(ref otri1, true);
							otri.LprevSelf();
						}
						otri.SegPivot(ref osub);
						if (osub.seg != Mesh.dummysub)
						{
							flag = true;
							this.SegmentIntersection(ref otri, ref osub, endpoint2);
							flag1 = true;
						}
						else
						{
							this.Flip(ref otri);
						}
					}
					else
					{
						flag = true;
						otri.Oprev(ref otri1);
						this.DelaunayFixup(ref otri, false);
						this.DelaunayFixup(ref otri1, true);
						flag1 = true;
					}
				}
				else
				{
					otri.Oprev(ref otri1);
					this.DelaunayFixup(ref otri, false);
					this.DelaunayFixup(ref otri1, true);
					flag1 = true;
				}
			}
			while (!flag1);
			this.InsertSubseg(ref otri, newmark);
			if (flag && !this.ScoutSegment(ref otri, endpoint2, newmark))
			{
				this.ConstrainedEdge(ref otri, endpoint2, newmark);
			}
		}

		private int Delaunay()
		{
			int num = 0;
			if (this.behavior.Algorithm != TriangulationAlgorithm.Dwyer)
			{
				num = (this.behavior.Algorithm != TriangulationAlgorithm.SweepLine ? (new Incremental()).Triangulate(this) : (new SweepLine()).Triangulate(this));
			}
			else
			{
				num = (new Dwyer()).Triangulate(this);
			}
			if (this.triangles.Count != 0)
			{
				return num;
			}
			return 0;
		}

		private void DelaunayFixup(ref Otri fixuptri, bool leftside)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Osub osub = new Osub();
			fixuptri.Lnext(ref otri);
			otri.Sym(ref otri1);
			if (otri1.triangle == Mesh.dummytri)
			{
				return;
			}
			otri.SegPivot(ref osub);
			if (osub.seg != Mesh.dummysub)
			{
				return;
			}
			Vertex vertex = otri.Apex();
			Vertex vertex1 = otri.Org();
			Vertex vertex2 = otri.Dest();
			Vertex vertex3 = otri1.Apex();
			if (leftside)
			{
				if (Primitives.CounterClockwise(vertex, vertex1, vertex3) <= 0)
				{
					return;
				}
			}
			else if (Primitives.CounterClockwise(vertex3, vertex2, vertex) <= 0)
			{
				return;
			}
			if (Primitives.CounterClockwise(vertex2, vertex1, vertex3) > 0 && Primitives.InCircle(vertex1, vertex3, vertex2, vertex) <= 0)
			{
				return;
			}
			this.Flip(ref otri);
			fixuptri.LprevSelf();
			this.DelaunayFixup(ref fixuptri, leftside);
			this.DelaunayFixup(ref otri1, leftside);
		}

		internal void DeleteVertex(ref Otri deltri)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			Otri otri6 = new Otri();
			Otri otri7 = new Otri();
			Osub osub = new Osub();
			Osub osub1 = new Osub();
			this.VertexDealloc(deltri.Org());
			deltri.Onext(ref otri);
			int num = 1;
			while (!deltri.Equal(otri))
			{
				num++;
				otri.OnextSelf();
			}
			if (num > 3)
			{
				deltri.Onext(ref otri1);
				deltri.Oprev(ref otri2);
				this.TriangulatePolygon(otri1, otri2, num, false, this.behavior.NoBisect == 0);
			}
			deltri.Lprev(ref otri3);
			deltri.Dnext(ref otri4);
			otri4.Sym(ref otri6);
			otri3.Oprev(ref otri5);
			otri5.Sym(ref otri7);
			deltri.Bond(ref otri6);
			otri3.Bond(ref otri7);
			otri4.SegPivot(ref osub);
			if (osub.seg != Mesh.dummysub)
			{
				deltri.SegBond(ref osub);
			}
			otri5.SegPivot(ref osub1);
			if (osub1.seg != Mesh.dummysub)
			{
				otri3.SegBond(ref osub1);
			}
			deltri.SetOrg(otri4.Org());
			if (this.behavior.NoBisect == 0)
			{
				this.quality.TestTriangle(ref deltri);
			}
			this.TriangleDealloc(otri4.triangle);
			this.TriangleDealloc(otri5.triangle);
		}

		private void DummyInit()
		{
			Mesh.dummytri = new Triangle()
			{
				hash = -1,
				id = -1
			};
			Mesh.dummytri.neighbors[0].triangle = Mesh.dummytri;
			Mesh.dummytri.neighbors[1].triangle = Mesh.dummytri;
			Mesh.dummytri.neighbors[2].triangle = Mesh.dummytri;
			if (this.behavior.useSegments)
			{
				Mesh.dummysub = new Segment()
				{
					hash = -1
				};
				Mesh.dummysub.subsegs[0].seg = Mesh.dummysub;
				Mesh.dummysub.subsegs[1].seg = Mesh.dummysub;
				Mesh.dummytri.subsegs[0].seg = Mesh.dummysub;
				Mesh.dummytri.subsegs[1].seg = Mesh.dummysub;
				Mesh.dummytri.subsegs[2].seg = Mesh.dummysub;
			}
		}

		private FindDirectionResult FindDirection(ref Otri searchtri, Vertex searchpoint)
		{
			Otri otri = new Otri();
			Vertex vertex = searchtri.Org();
			Vertex vertex1 = searchtri.Dest();
			Vertex vertex2 = searchtri.Apex();
			double num = Primitives.CounterClockwise(searchpoint, vertex, vertex2);
			bool flag = num > 0;
			double num1 = Primitives.CounterClockwise(vertex, searchpoint, vertex1);
			bool flag1 = num1 > 0;
			if (flag & flag1)
			{
				searchtri.Onext(ref otri);
				if (otri.triangle != Mesh.dummytri)
				{
					flag1 = false;
				}
				else
				{
					flag = false;
				}
			}
			while (flag)
			{
				searchtri.OnextSelf();
				if (searchtri.triangle == Mesh.dummytri)
				{
					this.logger.Error("Unable to find a triangle on path.", "Mesh.FindDirection().1");
					throw new Exception("Unable to find a triangle on path.");
				}
				vertex2 = searchtri.Apex();
				num1 = num;
				num = Primitives.CounterClockwise(searchpoint, vertex, vertex2);
				flag = num > 0;
			}
			while (flag1)
			{
				searchtri.OprevSelf();
				if (searchtri.triangle == Mesh.dummytri)
				{
					this.logger.Error("Unable to find a triangle on path.", "Mesh.FindDirection().2");
					throw new Exception("Unable to find a triangle on path.");
				}
				vertex1 = searchtri.Dest();
				num = num1;
				num1 = Primitives.CounterClockwise(vertex, searchpoint, vertex1);
				flag1 = num1 > 0;
			}
			if (num == 0)
			{
				return FindDirectionResult.Leftcollinear;
			}
			if (num1 == 0)
			{
				return FindDirectionResult.Rightcollinear;
			}
			return FindDirectionResult.Within;
		}

		internal void Flip(ref Otri flipedge)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			Otri otri6 = new Otri();
			Otri otri7 = new Otri();
			Otri otri8 = new Otri();
			Osub osub = new Osub();
			Osub osub1 = new Osub();
			Osub osub2 = new Osub();
			Osub osub3 = new Osub();
			Vertex vertex = flipedge.Org();
			Vertex vertex1 = flipedge.Dest();
			Vertex vertex2 = flipedge.Apex();
			flipedge.Sym(ref otri4);
			Vertex vertex3 = otri4.Apex();
			otri4.Lprev(ref otri2);
			otri2.Sym(ref otri7);
			otri4.Lnext(ref otri3);
			otri3.Sym(ref otri8);
			flipedge.Lnext(ref otri);
			otri.Sym(ref otri5);
			flipedge.Lprev(ref otri1);
			otri1.Sym(ref otri6);
			otri2.Bond(ref otri5);
			otri.Bond(ref otri6);
			otri1.Bond(ref otri8);
			otri3.Bond(ref otri7);
			if (this.checksegments)
			{
				otri2.SegPivot(ref osub2);
				otri.SegPivot(ref osub);
				otri1.SegPivot(ref osub1);
				otri3.SegPivot(ref osub3);
				if (osub2.seg != Mesh.dummysub)
				{
					otri3.SegBond(ref osub2);
				}
				else
				{
					otri3.SegDissolve();
				}
				if (osub.seg != Mesh.dummysub)
				{
					otri2.SegBond(ref osub);
				}
				else
				{
					otri2.SegDissolve();
				}
				if (osub1.seg != Mesh.dummysub)
				{
					otri.SegBond(ref osub1);
				}
				else
				{
					otri.SegDissolve();
				}
				if (osub3.seg != Mesh.dummysub)
				{
					otri1.SegBond(ref osub3);
				}
				else
				{
					otri1.SegDissolve();
				}
			}
			flipedge.SetOrg(vertex3);
			flipedge.SetDest(vertex2);
			flipedge.SetApex(vertex);
			otri4.SetOrg(vertex2);
			otri4.SetDest(vertex3);
			otri4.SetApex(vertex1);
		}

		private void FormSkeleton(InputGeometry input)
		{
			this.insegments = 0;
			if (this.behavior.Poly)
			{
				if (this.triangles.Count == 0)
				{
					return;
				}
				if (input.HasSegments)
				{
					this.MakeVertexMap();
				}
				int boundary = 0;
				foreach (Edge segment in input.segments)
				{
					this.insegments = this.insegments + 1;
					int p0 = segment.P0;
					int p1 = segment.P1;
					boundary = segment.Boundary;
					if (p0 < 0 || p0 >= this.invertices)
					{
						if (!TriangleNet.Behavior.Verbose)
						{
							continue;
						}
						this.logger.Warning("Invalid first endpoint of segment.", "Mesh.FormSkeleton().1");
					}
					else if (p1 < 0 || p1 >= this.invertices)
					{
						if (!TriangleNet.Behavior.Verbose)
						{
							continue;
						}
						this.logger.Warning("Invalid second endpoint of segment.", "Mesh.FormSkeleton().2");
					}
					else
					{
						Vertex item = this.vertices[p0];
						Vertex vertex = this.vertices[p1];
						if (item.x != vertex.x || item.y != vertex.y)
						{
							this.InsertSegment(item, vertex, boundary);
						}
						else
						{
							if (!TriangleNet.Behavior.Verbose)
							{
								continue;
							}
							this.logger.Warning("Endpoints of segments are coincident.", "Mesh.FormSkeleton()");
						}
					}
				}
			}
			if (this.behavior.Convex || !this.behavior.Poly)
			{
				this.MarkHull();
			}
		}

		private void InsertSegment(Vertex endpoint1, Vertex endpoint2, int newmark)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Vertex vertex = null;
			otri = endpoint1.tri;
			if (otri.triangle != null)
			{
				vertex = otri.Org();
			}
			if (vertex != endpoint1)
			{
				otri.triangle = Mesh.dummytri;
				otri.orient = 0;
				otri.SymSelf();
				if (this.locator.Locate(endpoint1, ref otri) != LocateResult.OnVertex)
				{
					this.logger.Error("Unable to locate PSLG vertex in triangulation.", "Mesh.InsertSegment().1");
					throw new Exception("Unable to locate PSLG vertex in triangulation.");
				}
			}
			this.locator.Update(ref otri);
			if (this.ScoutSegment(ref otri, endpoint2, newmark))
			{
				return;
			}
			endpoint1 = otri.Org();
			vertex = null;
			otri1 = endpoint2.tri;
			if (otri1.triangle != null)
			{
				vertex = otri1.Org();
			}
			if (vertex != endpoint2)
			{
				otri1.triangle = Mesh.dummytri;
				otri1.orient = 0;
				otri1.SymSelf();
				if (this.locator.Locate(endpoint2, ref otri1) != LocateResult.OnVertex)
				{
					this.logger.Error("Unable to locate PSLG vertex in triangulation.", "Mesh.InsertSegment().2");
					throw new Exception("Unable to locate PSLG vertex in triangulation.");
				}
			}
			this.locator.Update(ref otri1);
			if (this.ScoutSegment(ref otri1, endpoint1, newmark))
			{
				return;
			}
			endpoint2 = otri1.Org();
			this.ConstrainedEdge(ref otri, endpoint2, newmark);
		}

		internal void InsertSubseg(ref Otri tri, int subsegmark)
		{
			Otri otri = new Otri();
			Osub osub = new Osub();
			Vertex vertex = tri.Org();
			Vertex vertex1 = tri.Dest();
			if (vertex.mark == 0)
			{
				vertex.mark = subsegmark;
			}
			if (vertex1.mark == 0)
			{
				vertex1.mark = subsegmark;
			}
			tri.SegPivot(ref osub);
			if (osub.seg != Mesh.dummysub)
			{
				if (osub.seg.boundary == 0)
				{
					osub.seg.boundary = subsegmark;
				}
				return;
			}
			this.MakeSegment(ref osub);
			osub.SetOrg(vertex1);
			osub.SetDest(vertex);
			osub.SetSegOrg(vertex1);
			osub.SetSegDest(vertex);
			tri.SegBond(ref osub);
			tri.Sym(ref otri);
			osub.SymSelf();
			otri.SegBond(ref osub);
			osub.seg.boundary = subsegmark;
		}

		internal InsertVertexResult InsertVertex(Vertex newvertex, ref Otri searchtri, ref Osub splitseg, bool segmentflaws, bool triflaws)
		{
			Vertex vertex;
			Vertex vertex1;
			Vertex vertex2;
			double num;
			LocateResult locateResult;
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			Otri otri6 = new Otri();
			Otri otri7 = new Otri();
			Otri otri8 = new Otri();
			Otri otri9 = new Otri();
			Otri otri10 = new Otri();
			Otri otri11 = new Otri();
			Otri otri12 = new Otri();
			Otri otri13 = new Otri();
			Osub osub = new Osub();
			Osub osub1 = new Osub();
			Osub osub2 = new Osub();
			Osub osub3 = new Osub();
			Osub osub4 = new Osub();
			Osub osub5 = new Osub();
			Osub osub6 = new Osub();
			Osub osub7 = new Osub();
			if (splitseg.seg != null)
			{
				searchtri.Copy(ref otri);
				locateResult = LocateResult.OnEdge;
			}
			else if (searchtri.triangle != Mesh.dummytri)
			{
				searchtri.Copy(ref otri);
				locateResult = this.locator.PreciseLocate(newvertex, ref otri, true);
			}
			else
			{
				otri.triangle = Mesh.dummytri;
				otri.orient = 0;
				otri.SymSelf();
				locateResult = this.locator.Locate(newvertex, ref otri);
			}
			if (locateResult == LocateResult.OnVertex)
			{
				otri.Copy(ref searchtri);
				this.locator.Update(ref otri);
				return InsertVertexResult.Duplicate;
			}
			if (locateResult == LocateResult.OnEdge || locateResult == LocateResult.Outside)
			{
				if (this.checksegments && splitseg.seg == null)
				{
					otri.SegPivot(ref osub4);
					if (osub4.seg != Mesh.dummysub)
					{
						if (segmentflaws)
						{
							bool noBisect = this.behavior.NoBisect != 2;
							if (noBisect && this.behavior.NoBisect == 1)
							{
								otri.Sym(ref otri13);
								noBisect = otri13.triangle != Mesh.dummytri;
							}
							if (noBisect)
							{
								BadSubseg badSubseg = new BadSubseg()
								{
									encsubseg = osub4,
									subsegorg = osub4.Org(),
									subsegdest = osub4.Dest()
								};
								this.quality.AddBadSubseg(badSubseg);
							}
						}
						otri.Copy(ref searchtri);
						this.locator.Update(ref otri);
						return InsertVertexResult.Violating;
					}
				}
				otri.Lprev(ref otri3);
				otri3.Sym(ref otri10);
				otri.Sym(ref otri5);
				bool flag = otri5.triangle != Mesh.dummytri;
				if (!flag)
				{
					this.hullsize = this.hullsize + 1;
				}
				else
				{
					otri5.LnextSelf();
					otri5.Sym(ref otri12);
					this.MakeTriangle(ref otri8);
				}
				this.MakeTriangle(ref otri7);
				vertex1 = otri.Org();
				vertex = otri.Dest();
				vertex2 = otri.Apex();
				otri7.SetOrg(vertex2);
				otri7.SetDest(vertex1);
				otri7.SetApex(newvertex);
				otri.SetOrg(newvertex);
				otri7.triangle.region = otri3.triangle.region;
				if (this.behavior.VarArea)
				{
					otri7.triangle.area = otri3.triangle.area;
				}
				if (flag)
				{
					Vertex vertex3 = otri5.Dest();
					otri8.SetOrg(vertex1);
					otri8.SetDest(vertex3);
					otri8.SetApex(newvertex);
					otri5.SetOrg(newvertex);
					otri8.triangle.region = otri5.triangle.region;
					if (this.behavior.VarArea)
					{
						otri8.triangle.area = otri5.triangle.area;
					}
				}
				if (this.checksegments)
				{
					otri3.SegPivot(ref osub1);
					if (osub1.seg != Mesh.dummysub)
					{
						otri3.SegDissolve();
						otri7.SegBond(ref osub1);
					}
					if (flag)
					{
						otri5.SegPivot(ref osub3);
						if (osub3.seg != Mesh.dummysub)
						{
							otri5.SegDissolve();
							otri8.SegBond(ref osub3);
						}
					}
				}
				otri7.Bond(ref otri10);
				otri7.LprevSelf();
				otri7.Bond(ref otri3);
				otri7.LprevSelf();
				if (flag)
				{
					otri8.Bond(ref otri12);
					otri8.LnextSelf();
					otri8.Bond(ref otri5);
					otri8.LnextSelf();
					otri8.Bond(ref otri7);
				}
				if (splitseg.seg != null)
				{
					splitseg.SetDest(newvertex);
					Vertex vertex4 = splitseg.SegOrg();
					Vertex vertex5 = splitseg.SegDest();
					splitseg.SymSelf();
					splitseg.Pivot(ref osub6);
					this.InsertSubseg(ref otri7, splitseg.seg.boundary);
					otri7.SegPivot(ref osub7);
					osub7.SetSegOrg(vertex4);
					osub7.SetSegDest(vertex5);
					splitseg.Bond(ref osub7);
					osub7.SymSelf();
					osub7.Bond(ref osub6);
					splitseg.SymSelf();
					if (newvertex.mark == 0)
					{
						newvertex.mark = splitseg.seg.boundary;
					}
				}
				if (this.checkquality)
				{
					this.flipstack.Clear();
					this.flipstack.Push(new Otri());
					this.flipstack.Push(otri);
				}
				otri.LnextSelf();
			}
			else
			{
				otri.Lnext(ref otri2);
				otri.Lprev(ref otri3);
				otri2.Sym(ref otri9);
				otri3.Sym(ref otri10);
				this.MakeTriangle(ref otri6);
				this.MakeTriangle(ref otri7);
				vertex1 = otri.Org();
				vertex = otri.Dest();
				vertex2 = otri.Apex();
				otri6.SetOrg(vertex);
				otri6.SetDest(vertex2);
				otri6.SetApex(newvertex);
				otri7.SetOrg(vertex2);
				otri7.SetDest(vertex1);
				otri7.SetApex(newvertex);
				otri.SetApex(newvertex);
				otri6.triangle.region = otri.triangle.region;
				otri7.triangle.region = otri.triangle.region;
				if (this.behavior.VarArea)
				{
					num = otri.triangle.area;
					otri6.triangle.area = num;
					otri7.triangle.area = num;
				}
				if (this.checksegments)
				{
					otri2.SegPivot(ref osub);
					if (osub.seg != Mesh.dummysub)
					{
						otri2.SegDissolve();
						otri6.SegBond(ref osub);
					}
					otri3.SegPivot(ref osub1);
					if (osub1.seg != Mesh.dummysub)
					{
						otri3.SegDissolve();
						otri7.SegBond(ref osub1);
					}
				}
				otri6.Bond(ref otri9);
				otri7.Bond(ref otri10);
				otri6.LnextSelf();
				otri7.LprevSelf();
				otri6.Bond(ref otri7);
				otri6.LnextSelf();
				otri2.Bond(ref otri6);
				otri7.LprevSelf();
				otri3.Bond(ref otri7);
				if (this.checkquality)
				{
					this.flipstack.Clear();
					this.flipstack.Push(otri);
				}
			}
			InsertVertexResult insertVertexResult = InsertVertexResult.Successful;
			Vertex vertex6 = otri.Org();
			vertex1 = vertex6;
			vertex = otri.Dest();
			while (true)
			{
				bool flag1 = true;
				if (this.checksegments)
				{
					otri.SegPivot(ref osub5);
					if (osub5.seg != Mesh.dummysub)
					{
						flag1 = false;
						if (segmentflaws && this.quality.CheckSeg4Encroach(ref osub5) > 0)
						{
							insertVertexResult = InsertVertexResult.Encroaching;
						}
					}
				}
				if (flag1)
				{
					otri.Sym(ref otri1);
					if (otri1.triangle != Mesh.dummytri)
					{
						Vertex vertex7 = otri1.Apex();
						if (vertex == this.infvertex1 || vertex == this.infvertex2 || vertex == this.infvertex3)
						{
							flag1 = Primitives.CounterClockwise(newvertex, vertex1, vertex7) > 0;
						}
						else if (vertex1 == this.infvertex1 || vertex1 == this.infvertex2 || vertex1 == this.infvertex3)
						{
							flag1 = Primitives.CounterClockwise(vertex7, vertex, newvertex) > 0;
						}
						else
						{
							flag1 = (vertex7 == this.infvertex1 || vertex7 == this.infvertex2 || vertex7 == this.infvertex3 ? false : Primitives.InCircle(vertex, newvertex, vertex1, vertex7) > 0);
						}
						if (flag1)
						{
							otri1.Lprev(ref otri4);
							otri4.Sym(ref otri11);
							otri1.Lnext(ref otri5);
							otri5.Sym(ref otri12);
							otri.Lnext(ref otri2);
							otri2.Sym(ref otri9);
							otri.Lprev(ref otri3);
							otri3.Sym(ref otri10);
							otri4.Bond(ref otri9);
							otri2.Bond(ref otri10);
							otri3.Bond(ref otri12);
							otri5.Bond(ref otri11);
							if (this.checksegments)
							{
								otri4.SegPivot(ref osub2);
								otri2.SegPivot(ref osub);
								otri3.SegPivot(ref osub1);
								otri5.SegPivot(ref osub3);
								if (osub2.seg != Mesh.dummysub)
								{
									otri5.SegBond(ref osub2);
								}
								else
								{
									otri5.SegDissolve();
								}
								if (osub.seg != Mesh.dummysub)
								{
									otri4.SegBond(ref osub);
								}
								else
								{
									otri4.SegDissolve();
								}
								if (osub1.seg != Mesh.dummysub)
								{
									otri2.SegBond(ref osub1);
								}
								else
								{
									otri2.SegDissolve();
								}
								if (osub3.seg != Mesh.dummysub)
								{
									otri3.SegBond(ref osub3);
								}
								else
								{
									otri3.SegDissolve();
								}
							}
							otri.SetOrg(vertex7);
							otri.SetDest(newvertex);
							otri.SetApex(vertex1);
							otri1.SetOrg(newvertex);
							otri1.SetDest(vertex7);
							otri1.SetApex(vertex);
							int num1 = Math.Min(otri1.triangle.region, otri.triangle.region);
							otri1.triangle.region = num1;
							otri.triangle.region = num1;
							if (this.behavior.VarArea)
							{
								num = (otri1.triangle.area <= 0 || otri.triangle.area <= 0 ? -1 : 0.5 * (otri1.triangle.area + otri.triangle.area));
								otri1.triangle.area = num;
								otri.triangle.area = num;
							}
							if (this.checkquality)
							{
								this.flipstack.Push(otri);
							}
							otri.LprevSelf();
							vertex = vertex7;
						}
					}
					else
					{
						flag1 = false;
					}
				}
				if (!flag1)
				{
					if (triflaws)
					{
						this.quality.TestTriangle(ref otri);
					}
					otri.LnextSelf();
					otri.Sym(ref otri13);
					if (vertex == vertex6 || otri13.triangle == Mesh.dummytri)
					{
						break;
					}
					otri13.Lnext(ref otri);
					vertex1 = vertex;
					vertex = otri.Dest();
				}
			}
			otri.Lnext(ref searchtri);
			Otri otri14 = new Otri();
			otri.Lnext(ref otri14);
			this.locator.Update(ref otri14);
			return insertVertexResult;
		}

		public void Load(string filename)
		{
			List<ITriangle> triangles;
			InputGeometry inputGeometry;
			FileReader.Read(filename, out inputGeometry, out triangles);
			if (inputGeometry != null && triangles != null)
			{
				this.Load(inputGeometry, triangles);
			}
		}

		public void Load(InputGeometry input, List<ITriangle> triangles)
		{
			if (input == null || triangles == null)
			{
				throw new ArgumentException("Invalid input (argument is null).");
			}
			this.ResetData();
			if (input.HasSegments)
			{
				this.behavior.Poly = true;
				this.holes.AddRange(input.Holes);
			}
			if (!this.behavior.Poly)
			{
				this.behavior.VarArea = false;
				this.behavior.useRegions = false;
			}
			this.behavior.useRegions = input.Regions.Count > 0;
			this.TransferNodes(input);
			this.hullsize = DataReader.Reconstruct(this, input, triangles.ToArray());
			this.edges = (3 * triangles.Count + this.hullsize) / 2;
		}

		internal void MakeSegment(ref Osub newsubseg)
		{
			Segment segment = new Segment();
			int hashSeg = this.hash_seg;
			this.hash_seg = hashSeg + 1;
			segment.hash = hashSeg;
			newsubseg.seg = segment;
			newsubseg.orient = 0;
			this.subsegs.Add(segment.hash, segment);
		}

		internal void MakeTriangle(ref Otri newotri)
		{
			Triangle triangle = new Triangle();
			int hashTri = this.hash_tri;
			this.hash_tri = hashTri + 1;
			triangle.hash = hashTri;
			triangle.id = triangle.hash;
			newotri.triangle = triangle;
			newotri.orient = 0;
			this.triangles.Add(triangle.hash, triangle);
		}

		internal void MakeVertexMap()
		{
			Otri otri = new Otri();
			foreach (Triangle value in this.triangles.Values)
			{
				otri.triangle = value;
				otri.orient = 0;
				while (otri.orient < 3)
				{
					otri.Org().tri = otri;
					otri.orient = otri.orient + 1;
				}
			}
		}

		private void MarkHull()
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			otri.triangle = Mesh.dummytri;
			otri.orient = 0;
			otri.SymSelf();
			otri.Copy(ref otri2);
			do
			{
				this.InsertSubseg(ref otri, 1);
				otri.LnextSelf();
				otri.Oprev(ref otri1);
				while (otri1.triangle != Mesh.dummytri)
				{
					otri1.Copy(ref otri);
					otri.Oprev(ref otri1);
				}
			}
			while (!otri.Equal(otri2));
		}

		public void Refine(bool halfArea)
		{
			if (!halfArea)
			{
				this.Refine();
				return;
			}
			double num = 0;
			foreach (Triangle value in this.triangles.Values)
			{
				double num1 = (value.vertices[2].x - value.vertices[0].x) * (value.vertices[1].y - value.vertices[0].y) - (value.vertices[1].x - value.vertices[0].x) * (value.vertices[2].y - value.vertices[0].y);
				num1 = Math.Abs(num1) / 2;
				if (num1 <= num)
				{
					continue;
				}
				num = num1;
			}
			this.Refine(num / 2);
		}

		public void Refine(double areaConstraint)
		{
			this.behavior.fixedArea = true;
			this.behavior.MaxArea = areaConstraint;
			this.Refine();
			this.behavior.fixedArea = false;
			this.behavior.MaxArea = -1;
		}

		public void Refine()
		{
			this.inelements = this.triangles.Count;
			this.invertices = this.vertices.Count;
			if (this.behavior.Poly)
			{
				if (!this.behavior.useSegments)
				{
					this.insegments = this.hullsize;
				}
				else
				{
					this.insegments = this.subsegs.Count;
				}
			}
			this.Reset();
			this.steinerleft = this.behavior.SteinerPoints;
			this.infvertex1 = null;
			this.infvertex2 = null;
			this.infvertex3 = null;
			if (this.behavior.useSegments)
			{
				this.checksegments = true;
			}
			if (this.triangles.Count > 0)
			{
				this.quality.EnforceQuality();
			}
			this.edges = (3 * this.triangles.Count + this.hullsize) / 2;
		}

		public void Renumber()
		{
			this.Renumber(NodeNumbering.Linear);
		}

		public void Renumber(NodeNumbering num)
		{
			int num1;
			if (num == this.numbering)
			{
				return;
			}
			if (num == NodeNumbering.Linear)
			{
				num1 = 0;
				foreach (Vertex value in this.vertices.Values)
				{
					int num2 = num1;
					num1 = num2 + 1;
					value.id = num2;
				}
			}
			else if (num == NodeNumbering.CuthillMcKee)
			{
				int[] numArray = (new CuthillMcKee()).Renumber(this);
				foreach (Vertex vertex in this.vertices.Values)
				{
					vertex.id = numArray[vertex.id];
				}
			}
			this.numbering = num;
			num1 = 0;
			foreach (Triangle triangle in this.triangles.Values)
			{
				int num3 = num1;
				num1 = num3 + 1;
				triangle.id = num3;
			}
		}

		private void Reset()
		{
			this.numbering = NodeNumbering.None;
			this.undeads = 0;
			this.checksegments = false;
			this.checkquality = false;
			Statistic.InCircleCount = (long)0;
			Statistic.CounterClockwiseCount = (long)0;
			Statistic.InCircleCountDecimal = (long)0;
			Statistic.CounterClockwiseCountDecimal = (long)0;
			Statistic.Orient3dCount = (long)0;
			Statistic.HyperbolaCount = (long)0;
			Statistic.CircleTopCount = (long)0;
			Statistic.CircumcenterCount = (long)0;
		}

		private void ResetData()
		{
			this.vertices.Clear();
			this.triangles.Clear();
			this.subsegs.Clear();
			this.holes.Clear();
			this.regions.Clear();
			this.hash_vtx = 0;
			this.hash_seg = 0;
			this.hash_tri = 0;
			this.flipstack.Clear();
			this.hullsize = 0;
			this.edges = 0;
			this.Reset();
			this.locator.Reset();
		}

		private bool ScoutSegment(ref Otri searchtri, Vertex endpoint2, int newmark)
		{
			Otri otri = new Otri();
			Osub osub = new Osub();
			FindDirectionResult findDirectionResult = this.FindDirection(ref searchtri, endpoint2);
			Vertex vertex = searchtri.Dest();
			Vertex vertex1 = searchtri.Apex();
			if (vertex1.x == endpoint2.x && vertex1.y == endpoint2.y || vertex.x == endpoint2.x && vertex.y == endpoint2.y)
			{
				if (vertex1.x == endpoint2.x && vertex1.y == endpoint2.y)
				{
					searchtri.LprevSelf();
				}
				this.InsertSubseg(ref searchtri, newmark);
				return true;
			}
			if (findDirectionResult == FindDirectionResult.Leftcollinear)
			{
				searchtri.LprevSelf();
				this.InsertSubseg(ref searchtri, newmark);
				return this.ScoutSegment(ref searchtri, endpoint2, newmark);
			}
			if (findDirectionResult == FindDirectionResult.Rightcollinear)
			{
				this.InsertSubseg(ref searchtri, newmark);
				searchtri.LnextSelf();
				return this.ScoutSegment(ref searchtri, endpoint2, newmark);
			}
			searchtri.Lnext(ref otri);
			otri.SegPivot(ref osub);
			if (osub.seg == Mesh.dummysub)
			{
				return false;
			}
			this.SegmentIntersection(ref otri, ref osub, endpoint2);
			otri.Copy(ref searchtri);
			this.InsertSubseg(ref searchtri, newmark);
			return this.ScoutSegment(ref searchtri, endpoint2, newmark);
		}

		private void SegmentIntersection(ref Otri splittri, ref Osub splitsubseg, Vertex endpoint2)
		{
			Osub osub = new Osub();
			Vertex vertex = splittri.Apex();
			Vertex vertex1 = splittri.Org();
			Vertex vertex2 = splittri.Dest();
			double num = vertex2.x - vertex1.x;
			double num1 = vertex2.y - vertex1.y;
			double num2 = endpoint2.x - vertex.x;
			double num3 = endpoint2.y - vertex.y;
			double num4 = vertex1.x - endpoint2.x;
			double num5 = vertex1.y - endpoint2.y;
			double num6 = num1 * num2 - num * num3;
			if (num6 == 0)
			{
				this.logger.Error("Attempt to find intersection of parallel segments.", "Mesh.SegmentIntersection()");
				throw new Exception("Attempt to find intersection of parallel segments.");
			}
			double num7 = (num3 * num4 - num2 * num5) / num6;
			Vertex vertex3 = new Vertex(vertex1.x + num7 * (vertex2.x - vertex1.x), vertex1.y + num7 * (vertex2.y - vertex1.y), splitsubseg.seg.boundary, this.nextras);
			int hashVtx = this.hash_vtx;
			this.hash_vtx = hashVtx + 1;
			vertex3.hash = hashVtx;
			vertex3.id = vertex3.hash;
			for (int i = 0; i < this.nextras; i++)
			{
				vertex3.attributes[i] = vertex1.attributes[i] + num7 * (vertex2.attributes[i] - vertex1.attributes[i]);
			}
			this.vertices.Add(vertex3.hash, vertex3);
			if (this.InsertVertex(vertex3, ref splittri, ref splitsubseg, false, false) != InsertVertexResult.Successful)
			{
				this.logger.Error("Failure to split a segment.", "Mesh.SegmentIntersection()");
				throw new Exception("Failure to split a segment.");
			}
			vertex3.tri = splittri;
			if (this.steinerleft > 0)
			{
				this.steinerleft = this.steinerleft - 1;
			}
			splitsubseg.SymSelf();
			splitsubseg.Pivot(ref osub);
			splitsubseg.Dissolve();
			osub.Dissolve();
			do
			{
				splitsubseg.SetSegOrg(vertex3);
				splitsubseg.NextSelf();
			}
			while (splitsubseg.seg != Mesh.dummysub);
			do
			{
				osub.SetSegOrg(vertex3);
				osub.NextSelf();
			}
			while (osub.seg != Mesh.dummysub);
			this.FindDirection(ref splittri, vertex);
			Vertex vertex4 = splittri.Dest();
			Vertex vertex5 = splittri.Apex();
			if (vertex5.x == vertex.x && vertex5.y == vertex.y)
			{
				splittri.OnextSelf();
				return;
			}
			if (vertex4.x != vertex.x || vertex4.y != vertex.y)
			{
				this.logger.Error("Topological inconsistency after splitting a segment.", "Mesh.SegmentIntersection()");
				throw new Exception("Topological inconsistency after splitting a segment.");
			}
		}

		public void Smooth()
		{
			this.numbering = NodeNumbering.None;
			((ISmoother)(new SimpleSmoother(this))).Smooth();
		}

		internal void SubsegDealloc(Segment dyingsubseg)
		{
			Osub.Kill(dyingsubseg);
			this.subsegs.Remove(dyingsubseg.hash);
		}

		private void TransferNodes(InputGeometry data)
		{
			List<Vertex> vertices = data.points;
			this.invertices = vertices.Count;
			this.mesh_dim = 2;
			if (this.invertices < 3)
			{
				this.logger.Error("Input must have at least three input vertices.", "MeshReader.TransferNodes()");
				throw new Exception("Input must have at least three input vertices.");
			}
			this.nextras = (vertices[0].attributes == null ? 0 : (int)vertices[0].attributes.Length);
			foreach (Vertex vertex in vertices)
			{
				int hashVtx = this.hash_vtx;
				this.hash_vtx = hashVtx + 1;
				vertex.hash = hashVtx;
				vertex.id = vertex.hash;
				this.vertices.Add(vertex.hash, vertex);
			}
			this.bounds = data.Bounds;
		}

		internal void TriangleDealloc(Triangle dyingtriangle)
		{
			Otri.Kill(dyingtriangle);
			this.triangles.Remove(dyingtriangle.hash);
		}

		public void Triangulate(string inputFile)
		{
			this.Triangulate(FileReader.Read(inputFile));
		}

		public void Triangulate(InputGeometry input)
		{
			this.ResetData();
			this.behavior.Poly = input.HasSegments;
			if (!this.behavior.Poly)
			{
				this.behavior.VarArea = false;
				this.behavior.useRegions = false;
			}
			this.behavior.useRegions = input.Regions.Count > 0;
			this.steinerleft = this.behavior.SteinerPoints;
			this.TransferNodes(input);
			this.hullsize = this.Delaunay();
			this.infvertex1 = null;
			this.infvertex2 = null;
			this.infvertex3 = null;
			if (this.behavior.useSegments)
			{
				this.checksegments = true;
				this.FormSkeleton(input);
			}
			if (!this.behavior.Poly || this.triangles.Count <= 0)
			{
				this.holes.Clear();
				this.regions.Clear();
			}
			else
			{
				foreach (Point hole in input.holes)
				{
					this.holes.Add(hole);
				}
				foreach (RegionPointer region in input.regions)
				{
					this.regions.Add(region);
				}
				(new Carver(this)).CarveHoles();
			}
			if (this.behavior.Quality && this.triangles.Count > 0)
			{
				this.quality.EnforceQuality();
			}
			this.edges = (3 * this.triangles.Count + this.hullsize) / 2;
		}

		private void TriangulatePolygon(Otri firstedge, Otri lastedge, int edgecount, bool doflip, bool triflaws)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			int num = 1;
			Vertex vertex = lastedge.Apex();
			Vertex vertex1 = firstedge.Dest();
			firstedge.Onext(ref otri1);
			Vertex vertex2 = otri1.Dest();
			otri1.Copy(ref otri);
			for (int i = 2; i <= edgecount - 2; i++)
			{
				otri.OnextSelf();
				Vertex vertex3 = otri.Dest();
				if (Primitives.InCircle(vertex, vertex1, vertex2, vertex3) > 0)
				{
					otri.Copy(ref otri1);
					vertex2 = vertex3;
					num = i;
				}
			}
			if (num > 1)
			{
				otri1.Oprev(ref otri2);
				this.TriangulatePolygon(firstedge, otri2, num + 1, true, triflaws);
			}
			if (num < edgecount - 2)
			{
				otri1.Sym(ref otri2);
				this.TriangulatePolygon(otri1, lastedge, edgecount - num, true, triflaws);
				otri2.Sym(ref otri1);
			}
			if (doflip)
			{
				this.Flip(ref otri1);
				if (triflaws)
				{
					otri1.Sym(ref otri);
					this.quality.TestTriangle(ref otri);
				}
			}
			otri1.Copy(ref lastedge);
		}

		internal void UndoVertex()
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			Otri otri6 = new Otri();
			Osub osub = new Osub();
			Osub osub1 = new Osub();
			Osub osub2 = new Osub();
			while (this.flipstack.Count > 0)
			{
				Otri otri7 = this.flipstack.Pop();
				if (this.flipstack.Count == 0)
				{
					otri7.Dprev(ref otri);
					otri.LnextSelf();
					otri7.Onext(ref otri1);
					otri1.LprevSelf();
					otri.Sym(ref otri3);
					otri1.Sym(ref otri4);
					otri7.SetApex(otri.Dest());
					otri7.LnextSelf();
					otri7.Bond(ref otri3);
					otri.SegPivot(ref osub);
					otri7.SegBond(ref osub);
					otri7.LnextSelf();
					otri7.Bond(ref otri4);
					otri1.SegPivot(ref osub1);
					otri7.SegBond(ref osub1);
					this.TriangleDealloc(otri.triangle);
					this.TriangleDealloc(otri1.triangle);
				}
				else if (this.flipstack.Peek().triangle != null)
				{
					this.Unflip(ref otri7);
				}
				else
				{
					otri7.Lprev(ref otri6);
					otri6.Sym(ref otri1);
					otri1.LnextSelf();
					otri1.Sym(ref otri4);
					Vertex vertex = otri1.Dest();
					otri7.SetOrg(vertex);
					otri6.Bond(ref otri4);
					otri1.SegPivot(ref osub1);
					otri6.SegBond(ref osub1);
					this.TriangleDealloc(otri1.triangle);
					otri7.Sym(ref otri6);
					if (otri6.triangle != Mesh.dummytri)
					{
						otri6.LnextSelf();
						otri6.Dnext(ref otri2);
						otri2.Sym(ref otri5);
						otri6.SetOrg(vertex);
						otri6.Bond(ref otri5);
						otri2.SegPivot(ref osub2);
						otri6.SegBond(ref osub2);
						this.TriangleDealloc(otri2.triangle);
					}
					this.flipstack.Clear();
				}
			}
		}

		internal void Unflip(ref Otri flipedge)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			Otri otri6 = new Otri();
			Otri otri7 = new Otri();
			Otri otri8 = new Otri();
			Osub osub = new Osub();
			Osub osub1 = new Osub();
			Osub osub2 = new Osub();
			Osub osub3 = new Osub();
			Vertex vertex = flipedge.Org();
			Vertex vertex1 = flipedge.Dest();
			Vertex vertex2 = flipedge.Apex();
			flipedge.Sym(ref otri4);
			Vertex vertex3 = otri4.Apex();
			otri4.Lprev(ref otri2);
			otri2.Sym(ref otri7);
			otri4.Lnext(ref otri3);
			otri3.Sym(ref otri8);
			flipedge.Lnext(ref otri);
			otri.Sym(ref otri5);
			flipedge.Lprev(ref otri1);
			otri1.Sym(ref otri6);
			otri2.Bond(ref otri8);
			otri.Bond(ref otri7);
			otri1.Bond(ref otri5);
			otri3.Bond(ref otri6);
			if (this.checksegments)
			{
				otri2.SegPivot(ref osub2);
				otri.SegPivot(ref osub);
				otri1.SegPivot(ref osub1);
				otri3.SegPivot(ref osub3);
				if (osub2.seg != Mesh.dummysub)
				{
					otri.SegBond(ref osub2);
				}
				else
				{
					otri.SegDissolve();
				}
				if (osub.seg != Mesh.dummysub)
				{
					otri1.SegBond(ref osub);
				}
				else
				{
					otri1.SegDissolve();
				}
				if (osub1.seg != Mesh.dummysub)
				{
					otri3.SegBond(ref osub1);
				}
				else
				{
					otri3.SegDissolve();
				}
				if (osub3.seg != Mesh.dummysub)
				{
					otri2.SegBond(ref osub3);
				}
				else
				{
					otri2.SegDissolve();
				}
			}
			flipedge.SetOrg(vertex2);
			flipedge.SetDest(vertex3);
			flipedge.SetApex(vertex1);
			otri4.SetOrg(vertex3);
			otri4.SetDest(vertex2);
			otri4.SetApex(vertex);
		}

		internal void VertexDealloc(Vertex dyingvertex)
		{
			dyingvertex.type = VertexType.DeadVertex;
			this.vertices.Remove(dyingvertex.hash);
		}
	}
}