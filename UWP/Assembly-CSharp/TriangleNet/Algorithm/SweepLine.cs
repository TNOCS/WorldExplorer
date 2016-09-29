using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;
using TriangleNet.Tools;

namespace TriangleNet.Algorithm
{
	internal class SweepLine
	{
		private static int randomseed;

		private static int SAMPLERATE;

		private Mesh mesh;

		private double xminextreme;

		private List<SweepLine.SplayNode> splaynodes;

		static SweepLine()
		{
			SweepLine.randomseed = 1;
			SweepLine.SAMPLERATE = 10;
		}

		public SweepLine()
		{
		}

		private void Check4DeadEvent(ref Otri checktri, SweepLine.SweepEvent[] eventheap, ref int heapsize)
		{
			SweepLine.SweepEventVertex sweepEventVertex = checktri.Org() as SweepLine.SweepEventVertex;
			if (sweepEventVertex != null)
			{
				this.HeapDelete(eventheap, heapsize, sweepEventVertex.evt.heapposition);
				heapsize = heapsize - 1;
				checktri.SetOrg(null);
			}
		}

		private double CircleTop(Vertex pa, Vertex pb, Vertex pc, double ccwabc)
		{
			Statistic.CircleTopCount = Statistic.CircleTopCount + (long)1;
			double num = pa.x - pc.x;
			double num1 = pa.y - pc.y;
			double num2 = pb.x - pc.x;
			double num3 = pb.y - pc.y;
			double num4 = pa.x - pb.x;
			double num5 = pa.y - pb.y;
			double num6 = num * num + num1 * num1;
			double num7 = num2 * num2 + num3 * num3;
			double num8 = num4 * num4 + num5 * num5;
			return pc.y + (num * num7 - num2 * num6 + Math.Sqrt(num6 * num7 * num8)) / (2 * ccwabc);
		}

		private SweepLine.SplayNode CircleTopInsert(SweepLine.SplayNode splayroot, Otri newkey, Vertex pa, Vertex pb, Vertex pc, double topy)
		{
			Point point = new Point();
			Otri otri = new Otri();
			double num = Primitives.CounterClockwise(pa, pb, pc);
			double num1 = pa.x - pc.x;
			double num2 = pa.y - pc.y;
			double num3 = pb.x - pc.x;
			double num4 = pb.y - pc.y;
			double num5 = num1 * num1 + num2 * num2;
			double num6 = num3 * num3 + num4 * num4;
			point.x = pc.x - (num2 * num6 - num4 * num5) / (2 * num);
			point.y = topy;
			return this.SplayInsert(this.Splay(splayroot, point, ref otri), newkey, point);
		}

		private void CreateHeap(out SweepLine.SweepEvent[] eventheap)
		{
			int num = 3 * this.mesh.invertices / 2;
			eventheap = new SweepLine.SweepEvent[num];
			int num1 = 0;
			foreach (Vertex value in this.mesh.vertices.Values)
			{
				SweepLine.SweepEvent sweepEvent = new SweepLine.SweepEvent()
				{
					vertexEvent = value,
					xkey = value.x,
					ykey = value.y
				};
				int num2 = num1;
				num1 = num2 + 1;
				this.HeapInsert(eventheap, num2, sweepEvent);
			}
		}

		private SweepLine.SplayNode FrontLocate(SweepLine.SplayNode splayroot, Otri bottommost, Vertex searchvertex, ref Otri searchtri, ref bool farright)
		{
			bool i;
			bottommost.Copy(ref searchtri);
			splayroot = this.Splay(splayroot, searchvertex, ref searchtri);
			for (i = false; !i && this.RightOfHyperbola(ref searchtri, searchvertex); i = searchtri.Equal(bottommost))
			{
				searchtri.OnextSelf();
			}
			farright = i;
			return splayroot;
		}

		private void HeapDelete(SweepLine.SweepEvent[] heap, int heapsize, int eventnum)
		{
			bool flag;
			SweepLine.SweepEvent sweepEvent = heap[heapsize - 1];
			if (eventnum > 0)
			{
				double num = sweepEvent.xkey;
				double num1 = sweepEvent.ykey;
				do
				{
					int num2 = eventnum - 1 >> 1;
					if (heap[num2].ykey < num1 || heap[num2].ykey == num1 && heap[num2].xkey <= num)
					{
						flag = false;
					}
					else
					{
						heap[eventnum] = heap[num2];
						heap[eventnum].heapposition = eventnum;
						eventnum = num2;
						flag = eventnum > 0;
					}
				}
				while (flag);
			}
			heap[eventnum] = sweepEvent;
			sweepEvent.heapposition = eventnum;
			this.Heapify(heap, heapsize - 1, eventnum);
		}

		private void Heapify(SweepLine.SweepEvent[] heap, int heapsize, int eventnum)
		{
			int num;
			SweepLine.SweepEvent sweepEvent = heap[eventnum];
			double num1 = sweepEvent.xkey;
			double num2 = sweepEvent.ykey;
			int num3 = 2 * eventnum + 1;
			bool flag = num3 < heapsize;
			while (flag)
			{
				num = (heap[num3].ykey < num2 || heap[num3].ykey == num2 && heap[num3].xkey < num1 ? num3 : eventnum);
				int num4 = num3 + 1;
				if (num4 < heapsize && (heap[num4].ykey < heap[num].ykey || heap[num4].ykey == heap[num].ykey && heap[num4].xkey < heap[num].xkey))
				{
					num = num4;
				}
				if (num != eventnum)
				{
					heap[eventnum] = heap[num];
					heap[eventnum].heapposition = eventnum;
					heap[num] = sweepEvent;
					sweepEvent.heapposition = num;
					eventnum = num;
					num3 = 2 * eventnum + 1;
					flag = num3 < heapsize;
				}
				else
				{
					flag = false;
				}
			}
		}

		private void HeapInsert(SweepLine.SweepEvent[] heap, int heapsize, SweepLine.SweepEvent newevent)
		{
			double num = newevent.xkey;
			double num1 = newevent.ykey;
			int num2 = heapsize;
			bool flag = num2 > 0;
			while (flag)
			{
				int num3 = num2 - 1 >> 1;
				if (heap[num3].ykey < num1 || heap[num3].ykey == num1 && heap[num3].xkey <= num)
				{
					flag = false;
				}
				else
				{
					heap[num2] = heap[num3];
					heap[num2].heapposition = num2;
					num2 = num3;
					flag = num2 > 0;
				}
			}
			heap[num2] = newevent;
			newevent.heapposition = num2;
		}

		private int randomnation(int choices)
		{
			SweepLine.randomseed = (SweepLine.randomseed * 1366 + 150889) % 714025;
			return SweepLine.randomseed / (714025 / choices + 1);
		}

		private int RemoveGhosts(ref Otri startghost)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			bool poly = !this.mesh.behavior.Poly;
			startghost.Lprev(ref otri);
			otri.SymSelf();
			Mesh.dummytri.neighbors[0] = otri;
			startghost.Copy(ref otri1);
			int num = 0;
			do
			{
				num++;
				otri1.Lnext(ref otri2);
				otri1.LprevSelf();
				otri1.SymSelf();
				if (poly && otri1.triangle != Mesh.dummytri)
				{
					Vertex vertex = otri1.Org();
					if (vertex.mark == 0)
					{
						vertex.mark = 1;
					}
				}
				otri1.Dissolve();
				otri2.Sym(ref otri1);
				this.mesh.TriangleDealloc(otri2.triangle);
			}
			while (!otri1.Equal(startghost));
			return num;
		}

		private bool RightOfHyperbola(ref Otri fronttri, Point newsite)
		{
			Statistic.HyperbolaCount = Statistic.HyperbolaCount + (long)1;
			Vertex vertex = fronttri.Dest();
			Vertex vertex1 = fronttri.Apex();
			if (vertex.y >= vertex1.y && (vertex.y != vertex1.y || vertex.x >= vertex1.x))
			{
				if (newsite.x <= vertex.x)
				{
					return false;
				}
			}
			else if (newsite.x >= vertex1.x)
			{
				return true;
			}
			double num = vertex.x - newsite.x;
			double num1 = vertex.y - newsite.y;
			double num2 = vertex1.x - newsite.x;
			double num3 = vertex1.y - newsite.y;
			return num1 * (num2 * num2 + num3 * num3) > num3 * (num * num + num1 * num1);
		}

		private SweepLine.SplayNode Splay(SweepLine.SplayNode splaytree, Point searchpoint, ref Otri searchtri)
		{
			SweepLine.SplayNode splayNode;
			SweepLine.SplayNode splayNode1;
			if (splaytree == null)
			{
				return null;
			}
			if (splaytree.keyedge.Dest() != splaytree.keydest)
			{
				SweepLine.SplayNode splayNode2 = this.Splay(splaytree.lchild, searchpoint, ref searchtri);
				SweepLine.SplayNode splayNode3 = this.Splay(splaytree.rchild, searchpoint, ref searchtri);
				this.splaynodes.Remove(splaytree);
				if (splayNode2 == null)
				{
					return splayNode3;
				}
				if (splayNode3 == null)
				{
					return splayNode2;
				}
				if (splayNode2.rchild == null)
				{
					splayNode2.rchild = splayNode3.lchild;
					splayNode3.lchild = splayNode2;
					return splayNode3;
				}
				if (splayNode3.lchild == null)
				{
					splayNode3.lchild = splayNode2.rchild;
					splayNode2.rchild = splayNode3;
					return splayNode2;
				}
				SweepLine.SplayNode splayNode4 = splayNode2.rchild;
				while (splayNode4.rchild != null)
				{
					splayNode4 = splayNode4.rchild;
				}
				splayNode4.rchild = splayNode3;
				return splayNode2;
			}
			bool flag = this.RightOfHyperbola(ref splaytree.keyedge, searchpoint);
			if (!flag)
			{
				splayNode = splaytree.lchild;
			}
			else
			{
				splaytree.keyedge.Copy(ref searchtri);
				splayNode = splaytree.rchild;
			}
			if (splayNode == null)
			{
				return splaytree;
			}
			if (splayNode.keyedge.Dest() != splayNode.keydest)
			{
				splayNode = this.Splay(splayNode, searchpoint, ref searchtri);
				if (splayNode == null)
				{
					if (!flag)
					{
						splaytree.lchild = null;
					}
					else
					{
						splaytree.rchild = null;
					}
					return splaytree;
				}
			}
			bool flag1 = this.RightOfHyperbola(ref splayNode.keyedge, searchpoint);
			if (!flag1)
			{
				splayNode1 = this.Splay(splayNode.lchild, searchpoint, ref searchtri);
				splayNode.lchild = splayNode1;
			}
			else
			{
				splayNode.keyedge.Copy(ref searchtri);
				splayNode1 = this.Splay(splayNode.rchild, searchpoint, ref searchtri);
				splayNode.rchild = splayNode1;
			}
			if (splayNode1 == null)
			{
				if (!flag)
				{
					splaytree.lchild = splayNode.rchild;
					splayNode.rchild = splaytree;
				}
				else
				{
					splaytree.rchild = splayNode.lchild;
					splayNode.lchild = splaytree;
				}
				return splayNode;
			}
			if (!flag1)
			{
				if (!flag)
				{
					splaytree.lchild = splayNode.rchild;
					splayNode.rchild = splaytree;
				}
				else
				{
					splaytree.rchild = splayNode1.lchild;
					splayNode1.lchild = splaytree;
				}
				splayNode.lchild = splayNode1.rchild;
				splayNode1.rchild = splayNode;
			}
			else
			{
				if (!flag)
				{
					splaytree.lchild = splayNode1.rchild;
					splayNode1.rchild = splaytree;
				}
				else
				{
					splaytree.rchild = splayNode.lchild;
					splayNode.lchild = splaytree;
				}
				splayNode.rchild = splayNode1.lchild;
				splayNode1.lchild = splayNode;
			}
			return splayNode1;
		}

		private SweepLine.SplayNode SplayInsert(SweepLine.SplayNode splayroot, Otri newkey, Point searchpoint)
		{
			SweepLine.SplayNode splayNode = new SweepLine.SplayNode();
			this.splaynodes.Add(splayNode);
			newkey.Copy(ref splayNode.keyedge);
			splayNode.keydest = newkey.Dest();
			if (splayroot == null)
			{
				splayNode.lchild = null;
				splayNode.rchild = null;
			}
			else if (!this.RightOfHyperbola(ref splayroot.keyedge, searchpoint))
			{
				splayNode.lchild = splayroot.lchild;
				splayNode.rchild = splayroot;
				splayroot.lchild = null;
			}
			else
			{
				splayNode.lchild = splayroot;
				splayNode.rchild = splayroot.rchild;
				splayroot.rchild = null;
			}
			return splayNode;
		}

		public int Triangulate(Mesh mesh)
		{
			SweepLine.SweepEvent[] sweepEventArray;
			SweepLine.SweepEvent sweepEvent;
			Vertex vertex;
			Vertex vertex1;
			Vertex vertex2;
			Vertex vertex3;
			this.mesh = mesh;
			this.xminextreme = 10 * mesh.bounds.Xmin - 9 * mesh.bounds.Xmax;
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			Otri otri6 = new Otri();
			bool i = false;
			this.splaynodes = new List<SweepLine.SplayNode>();
			SweepLine.SplayNode splayNode = null;
			this.CreateHeap(out sweepEventArray);
			int num = mesh.invertices;
			mesh.MakeTriangle(ref otri2);
			mesh.MakeTriangle(ref otri3);
			otri2.Bond(ref otri3);
			otri2.LnextSelf();
			otri3.LprevSelf();
			otri2.Bond(ref otri3);
			otri2.LnextSelf();
			otri3.LprevSelf();
			otri2.Bond(ref otri3);
			Vertex vertex4 = sweepEventArray[0].vertexEvent;
			this.HeapDelete(sweepEventArray, num, 0);
			num--;
			do
			{
				if (num == 0)
				{
					SimpleLog.Instance.Error("Input vertices are all identical.", "SweepLine.SweepLineDelaunay()");
					throw new Exception("Input vertices are all identical.");
				}
				vertex = sweepEventArray[0].vertexEvent;
				this.HeapDelete(sweepEventArray, num, 0);
				num--;
				if (vertex4.x != vertex.x || vertex4.y != vertex.y)
				{
					continue;
				}
				if (Behavior.Verbose)
				{
					SimpleLog.Instance.Warning("A duplicate vertex appeared and was ignored.", "SweepLine.SweepLineDelaunay().1");
				}
				vertex.type = VertexType.UndeadVertex;
				Mesh mesh1 = mesh;
				mesh1.undeads = mesh1.undeads + 1;
			}
			while (vertex4.x == vertex.x && vertex4.y == vertex.y);
			otri2.SetOrg(vertex4);
			otri2.SetDest(vertex);
			otri3.SetOrg(vertex);
			otri3.SetDest(vertex4);
			otri2.Lprev(ref otri);
			Vertex vertex5 = vertex;
			while (num > 0)
			{
				SweepLine.SweepEvent sweepEvent1 = sweepEventArray[0];
				this.HeapDelete(sweepEventArray, num, 0);
				num--;
				bool flag = true;
				if (sweepEvent1.xkey >= mesh.bounds.Xmin)
				{
					Vertex vertex6 = sweepEvent1.vertexEvent;
					if (vertex6.x != vertex5.x || vertex6.y != vertex5.y)
					{
						vertex5 = vertex6;
						splayNode = this.FrontLocate(splayNode, otri, vertex6, ref otri1, ref i);
						otri.Copy(ref otri1);
						for (i = false; !i && this.RightOfHyperbola(ref otri1, vertex6); i = otri1.Equal(otri))
						{
							otri1.OnextSelf();
						}
						this.Check4DeadEvent(ref otri1, sweepEventArray, ref num);
						otri1.Copy(ref otri5);
						otri1.Sym(ref otri4);
						mesh.MakeTriangle(ref otri2);
						mesh.MakeTriangle(ref otri3);
						Vertex vertex7 = otri5.Dest();
						otri2.SetOrg(vertex7);
						otri2.SetDest(vertex6);
						otri3.SetOrg(vertex6);
						otri3.SetDest(vertex7);
						otri2.Bond(ref otri3);
						otri2.LnextSelf();
						otri3.LprevSelf();
						otri2.Bond(ref otri3);
						otri2.LnextSelf();
						otri3.LprevSelf();
						otri2.Bond(ref otri4);
						otri3.Bond(ref otri5);
						if (!i && otri5.Equal(otri))
						{
							otri2.Copy(ref otri);
						}
						if (this.randomnation(SweepLine.SAMPLERATE) == 0)
						{
							splayNode = this.SplayInsert(splayNode, otri2, vertex6);
						}
						else if (this.randomnation(SweepLine.SAMPLERATE) == 0)
						{
							otri3.Lnext(ref otri6);
							splayNode = this.SplayInsert(splayNode, otri6, vertex6);
						}
					}
					else
					{
						if (Behavior.Verbose)
						{
							SimpleLog.Instance.Warning("A duplicate vertex appeared and was ignored.", "SweepLine.SweepLineDelaunay().2");
						}
						vertex6.type = VertexType.UndeadVertex;
						Mesh mesh2 = mesh;
						mesh2.undeads = mesh2.undeads + 1;
						flag = false;
					}
				}
				else
				{
					Otri otri7 = sweepEvent1.otriEvent;
					otri7.Oprev(ref otri4);
					this.Check4DeadEvent(ref otri4, sweepEventArray, ref num);
					otri7.Onext(ref otri5);
					this.Check4DeadEvent(ref otri5, sweepEventArray, ref num);
					if (otri4.Equal(otri))
					{
						otri7.Lprev(ref otri);
					}
					mesh.Flip(ref otri7);
					otri7.SetApex(null);
					otri7.Lprev(ref otri2);
					otri7.Lnext(ref otri3);
					otri2.Sym(ref otri4);
					if (this.randomnation(SweepLine.SAMPLERATE) == 0)
					{
						otri7.SymSelf();
						vertex1 = otri7.Dest();
						vertex2 = otri7.Apex();
						vertex3 = otri7.Org();
						splayNode = this.CircleTopInsert(splayNode, otri2, vertex1, vertex2, vertex3, sweepEvent1.ykey);
					}
				}
				if (!flag)
				{
					continue;
				}
				vertex1 = otri4.Apex();
				vertex2 = otri2.Dest();
				vertex3 = otri2.Apex();
				double num1 = Primitives.CounterClockwise(vertex1, vertex2, vertex3);
				if (num1 > 0)
				{
					sweepEvent = new SweepLine.SweepEvent()
					{
						xkey = this.xminextreme,
						ykey = this.CircleTop(vertex1, vertex2, vertex3, num1),
						otriEvent = otri2
					};
					this.HeapInsert(sweepEventArray, num, sweepEvent);
					num++;
					otri2.SetOrg(new SweepLine.SweepEventVertex(sweepEvent));
				}
				vertex1 = otri3.Apex();
				vertex2 = otri3.Org();
				vertex3 = otri5.Apex();
				double num2 = Primitives.CounterClockwise(vertex1, vertex2, vertex3);
				if (num2 <= 0)
				{
					continue;
				}
				sweepEvent = new SweepLine.SweepEvent()
				{
					xkey = this.xminextreme,
					ykey = this.CircleTop(vertex1, vertex2, vertex3, num2),
					otriEvent = otri5
				};
				this.HeapInsert(sweepEventArray, num, sweepEvent);
				num++;
				otri5.SetOrg(new SweepLine.SweepEventVertex(sweepEvent));
			}
			this.splaynodes.Clear();
			otri.LprevSelf();
			return this.RemoveGhosts(ref otri);
		}

		private class SplayNode
		{
			public Otri keyedge;

			public Vertex keydest;

			public SweepLine.SplayNode lchild;

			public SweepLine.SplayNode rchild;

			public SplayNode()
			{
			}
		}

		private class SweepEvent
		{
			public double xkey;

			public double ykey;

			public Vertex vertexEvent;

			public Otri otriEvent;

			public int heapposition;

			public SweepEvent()
			{
			}
		}

		private class SweepEventVertex : Vertex
		{
			public SweepLine.SweepEvent evt;

			public SweepEventVertex(SweepLine.SweepEvent e)
			{
				this.evt = e;
			}
		}
	}
}