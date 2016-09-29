using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet.Algorithm
{
	internal class Dwyer
	{
		private static Random rand;

		private bool useDwyer = true;

		private Vertex[] sortarray;

		private Mesh mesh;

		static Dwyer()
		{
			Dwyer.rand = new Random(DateTime.Now.Millisecond);
		}

		public Dwyer()
		{
		}

		private void AlternateAxes(int left, int right, int axis)
		{
			int num = right - left + 1;
			int num1 = num >> 1;
			if (num <= 3)
			{
				axis = 0;
			}
			this.VertexMedian(left, right, left + num1, axis);
			if (num - num1 >= 2)
			{
				if (num1 >= 2)
				{
					this.AlternateAxes(left, left + num1 - 1, 1 - axis);
				}
				this.AlternateAxes(left + num1, right, 1 - axis);
			}
		}

		private void DivconqRecurse(int left, int right, int axis, ref Otri farleft, ref Otri farright)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			int num = right - left + 1;
			if (num == 2)
			{
				this.mesh.MakeTriangle(ref farleft);
				farleft.SetOrg(this.sortarray[left]);
				farleft.SetDest(this.sortarray[left + 1]);
				this.mesh.MakeTriangle(ref farright);
				farright.SetOrg(this.sortarray[left + 1]);
				farright.SetDest(this.sortarray[left]);
				farleft.Bond(ref farright);
				farleft.LprevSelf();
				farright.LnextSelf();
				farleft.Bond(ref farright);
				farleft.LprevSelf();
				farright.LnextSelf();
				farleft.Bond(ref farright);
				farright.Lprev(ref farleft);
				return;
			}
			if (num != 3)
			{
				int num1 = num >> 1;
				this.DivconqRecurse(left, left + num1 - 1, 1 - axis, ref farleft, ref otri4);
				this.DivconqRecurse(left + num1, right, 1 - axis, ref otri5, ref farright);
				this.MergeHulls(ref farleft, ref otri4, ref otri5, ref farright, axis);
				return;
			}
			this.mesh.MakeTriangle(ref otri);
			this.mesh.MakeTriangle(ref otri1);
			this.mesh.MakeTriangle(ref otri2);
			this.mesh.MakeTriangle(ref otri3);
			double num2 = Primitives.CounterClockwise(this.sortarray[left], this.sortarray[left + 1], this.sortarray[left + 2]);
			if (num2 == 0)
			{
				otri.SetOrg(this.sortarray[left]);
				otri.SetDest(this.sortarray[left + 1]);
				otri1.SetOrg(this.sortarray[left + 1]);
				otri1.SetDest(this.sortarray[left]);
				otri2.SetOrg(this.sortarray[left + 2]);
				otri2.SetDest(this.sortarray[left + 1]);
				otri3.SetOrg(this.sortarray[left + 1]);
				otri3.SetDest(this.sortarray[left + 2]);
				otri.Bond(ref otri1);
				otri2.Bond(ref otri3);
				otri.LnextSelf();
				otri1.LprevSelf();
				otri2.LnextSelf();
				otri3.LprevSelf();
				otri.Bond(ref otri3);
				otri1.Bond(ref otri2);
				otri.LnextSelf();
				otri1.LprevSelf();
				otri2.LnextSelf();
				otri3.LprevSelf();
				otri.Bond(ref otri1);
				otri2.Bond(ref otri3);
				otri1.Copy(ref farleft);
				otri2.Copy(ref farright);
				return;
			}
			otri.SetOrg(this.sortarray[left]);
			otri1.SetDest(this.sortarray[left]);
			otri3.SetOrg(this.sortarray[left]);
			if (num2 <= 0)
			{
				otri.SetDest(this.sortarray[left + 2]);
				otri1.SetOrg(this.sortarray[left + 2]);
				otri2.SetDest(this.sortarray[left + 2]);
				otri.SetApex(this.sortarray[left + 1]);
				otri2.SetOrg(this.sortarray[left + 1]);
				otri3.SetDest(this.sortarray[left + 1]);
			}
			else
			{
				otri.SetDest(this.sortarray[left + 1]);
				otri1.SetOrg(this.sortarray[left + 1]);
				otri2.SetDest(this.sortarray[left + 1]);
				otri.SetApex(this.sortarray[left + 2]);
				otri2.SetOrg(this.sortarray[left + 2]);
				otri3.SetDest(this.sortarray[left + 2]);
			}
			otri.Bond(ref otri1);
			otri.LnextSelf();
			otri.Bond(ref otri2);
			otri.LnextSelf();
			otri.Bond(ref otri3);
			otri1.LprevSelf();
			otri2.LnextSelf();
			otri1.Bond(ref otri2);
			otri1.LprevSelf();
			otri3.LprevSelf();
			otri1.Bond(ref otri3);
			otri2.LnextSelf();
			otri3.LprevSelf();
			otri2.Bond(ref otri3);
			otri1.Copy(ref farleft);
			if (num2 > 0)
			{
				otri2.Copy(ref farright);
				return;
			}
			farleft.Lnext(ref farright);
		}

		private void MergeHulls(ref Otri farleft, ref Otri innerleft, ref Otri innerright, ref Otri farright, int axis)
		{
			Vertex vertex;
			Vertex vertex1;
			Vertex vertex2;
			Vertex vertex3;
			Vertex vertex4;
			Vertex i;
			bool flag;
			bool flag1;
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			Otri otri6 = new Otri();
			Otri otri7 = new Otri();
			Vertex vertex5 = innerleft.Dest();
			Vertex vertex6 = innerleft.Apex();
			Vertex vertex7 = innerright.Org();
			Vertex vertex8 = innerright.Apex();
			if (this.useDwyer && axis == 1)
			{
				vertex = farleft.Org();
				vertex2 = farleft.Apex();
				vertex1 = farright.Dest();
				vertex3 = farright.Apex();
				while (vertex2.y < vertex.y)
				{
					farleft.LnextSelf();
					farleft.SymSelf();
					vertex = vertex2;
					vertex2 = farleft.Apex();
				}
				innerleft.Sym(ref otri6);
				for (i = otri6.Apex(); i.y > vertex5.y; i = otri6.Apex())
				{
					otri6.Lnext(ref innerleft);
					vertex6 = vertex5;
					vertex5 = i;
					innerleft.Sym(ref otri6);
				}
				while (vertex8.y < vertex7.y)
				{
					innerright.LnextSelf();
					innerright.SymSelf();
					vertex7 = vertex8;
					vertex8 = innerright.Apex();
				}
				farright.Sym(ref otri6);
				for (i = otri6.Apex(); i.y > vertex1.y; i = otri6.Apex())
				{
					otri6.Lnext(ref farright);
					vertex3 = vertex1;
					vertex1 = i;
					farright.Sym(ref otri6);
				}
			}
			do
			{
				flag = false;
				if (Primitives.CounterClockwise(vertex5, vertex6, vertex7) > 0)
				{
					innerleft.LprevSelf();
					innerleft.SymSelf();
					vertex5 = vertex6;
					vertex6 = innerleft.Apex();
					flag = true;
				}
				if (Primitives.CounterClockwise(vertex8, vertex7, vertex5) <= 0)
				{
					continue;
				}
				innerright.LnextSelf();
				innerright.SymSelf();
				vertex7 = vertex8;
				vertex8 = innerright.Apex();
				flag = true;
			}
			while (flag);
			innerleft.Sym(ref otri);
			innerright.Sym(ref otri1);
			this.mesh.MakeTriangle(ref otri7);
			otri7.Bond(ref innerleft);
			otri7.LnextSelf();
			otri7.Bond(ref innerright);
			otri7.LnextSelf();
			otri7.SetOrg(vertex7);
			otri7.SetDest(vertex5);
			vertex = farleft.Org();
			if (vertex5 == vertex)
			{
				otri7.Lnext(ref farleft);
			}
			vertex1 = farright.Dest();
			if (vertex7 == vertex1)
			{
				otri7.Lprev(ref farright);
			}
			Vertex vertex9 = vertex5;
			Vertex vertex10 = vertex7;
			Vertex vertex11 = otri.Apex();
			Vertex vertex12 = otri1.Apex();
			while (true)
			{
				bool flag2 = Primitives.CounterClockwise(vertex11, vertex9, vertex10) <= 0;
				bool flag3 = Primitives.CounterClockwise(vertex12, vertex9, vertex10) <= 0;
				if (flag2 & flag3)
				{
					break;
				}
				if (!flag2)
				{
					otri.Lprev(ref otri2);
					otri2.SymSelf();
					vertex4 = otri2.Apex();
					if (vertex4 != null)
					{
						flag1 = Primitives.InCircle(vertex9, vertex10, vertex11, vertex4) > 0;
						while (flag1)
						{
							otri2.LnextSelf();
							otri2.Sym(ref otri4);
							otri2.LnextSelf();
							otri2.Sym(ref otri3);
							otri2.Bond(ref otri4);
							otri.Bond(ref otri3);
							otri.LnextSelf();
							otri.Sym(ref otri5);
							otri2.LprevSelf();
							otri2.Bond(ref otri5);
							otri.SetOrg(vertex9);
							otri.SetDest(null);
							otri.SetApex(vertex4);
							otri2.SetOrg(null);
							otri2.SetDest(vertex11);
							otri2.SetApex(vertex4);
							vertex11 = vertex4;
							otri3.Copy(ref otri2);
							vertex4 = otri2.Apex();
							flag1 = (vertex4 == null ? false : Primitives.InCircle(vertex9, vertex10, vertex11, vertex4) > 0);
						}
					}
				}
				if (!flag3)
				{
					otri1.Lnext(ref otri2);
					otri2.SymSelf();
					vertex4 = otri2.Apex();
					if (vertex4 != null)
					{
						flag1 = Primitives.InCircle(vertex9, vertex10, vertex12, vertex4) > 0;
						while (flag1)
						{
							otri2.LprevSelf();
							otri2.Sym(ref otri4);
							otri2.LprevSelf();
							otri2.Sym(ref otri3);
							otri2.Bond(ref otri4);
							otri1.Bond(ref otri3);
							otri1.LprevSelf();
							otri1.Sym(ref otri5);
							otri2.LnextSelf();
							otri2.Bond(ref otri5);
							otri1.SetOrg(null);
							otri1.SetDest(vertex10);
							otri1.SetApex(vertex4);
							otri2.SetOrg(vertex12);
							otri2.SetDest(null);
							otri2.SetApex(vertex4);
							vertex12 = vertex4;
							otri3.Copy(ref otri2);
							vertex4 = otri2.Apex();
							flag1 = (vertex4 == null ? false : Primitives.InCircle(vertex9, vertex10, vertex12, vertex4) > 0);
						}
					}
				}
				if (flag2 || !flag3 && Primitives.InCircle(vertex11, vertex9, vertex10, vertex12) > 0)
				{
					otri7.Bond(ref otri1);
					otri1.Lprev(ref otri7);
					otri7.SetDest(vertex9);
					vertex10 = vertex12;
					otri7.Sym(ref otri1);
					vertex12 = otri1.Apex();
				}
				else
				{
					otri7.Bond(ref otri);
					otri.Lnext(ref otri7);
					otri7.SetOrg(vertex10);
					vertex9 = vertex11;
					otri7.Sym(ref otri);
					vertex11 = otri.Apex();
				}
			}
			this.mesh.MakeTriangle(ref otri2);
			otri2.SetOrg(vertex9);
			otri2.SetDest(vertex10);
			otri2.Bond(ref otri7);
			otri2.LnextSelf();
			otri2.Bond(ref otri1);
			otri2.LnextSelf();
			otri2.Bond(ref otri);
			if (this.useDwyer && axis == 1)
			{
				vertex = farleft.Org();
				vertex2 = farleft.Apex();
				vertex1 = farright.Dest();
				vertex3 = farright.Apex();
				farleft.Sym(ref otri6);
				for (i = otri6.Apex(); i.x < vertex.x; i = otri6.Apex())
				{
					otri6.Lprev(ref farleft);
					vertex2 = vertex;
					vertex = i;
					farleft.Sym(ref otri6);
				}
				while (vertex3.x > vertex1.x)
				{
					farright.LprevSelf();
					farright.SymSelf();
					vertex1 = vertex3;
					vertex3 = farright.Apex();
				}
			}
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

		public int Triangulate(Mesh m)
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			this.mesh = m;
			this.sortarray = new Vertex[m.invertices];
			int num = 0;
			foreach (Vertex value in m.vertices.Values)
			{
				int num1 = num;
				num = num1 + 1;
				this.sortarray[num1] = value;
			}
			this.VertexSort(0, m.invertices - 1);
			num = 0;
			for (int i = 1; i < m.invertices; i++)
			{
				if (this.sortarray[num].x != this.sortarray[i].x || this.sortarray[num].y != this.sortarray[i].y)
				{
					num++;
					this.sortarray[num] = this.sortarray[i];
				}
				else
				{
					if (Behavior.Verbose)
					{
						SimpleLog.Instance.Warning(string.Format("A duplicate vertex appeared and was ignored (ID {0}).", this.sortarray[i].hash), "DivConquer.DivconqDelaunay()");
					}
					this.sortarray[i].type = VertexType.UndeadVertex;
					Mesh mesh = m;
					mesh.undeads = mesh.undeads + 1;
				}
			}
			num++;
			if (this.useDwyer)
			{
				int num2 = num >> 1;
				if (num - num2 >= 2)
				{
					if (num2 >= 2)
					{
						this.AlternateAxes(0, num2 - 1, 1);
					}
					this.AlternateAxes(num2, num - 1, 1);
				}
			}
			this.DivconqRecurse(0, num - 1, 0, ref otri, ref otri1);
			return this.RemoveGhosts(ref otri);
		}

		private void VertexMedian(int left, int right, int median, int axis)
		{
			Vertex vertex;
			int num = left;
			int num1 = right;
			if (right - left + 1 == 2)
			{
				if (this.sortarray[left][axis] > this.sortarray[right][axis] || this.sortarray[left][axis] == this.sortarray[right][axis] && this.sortarray[left][1 - axis] > this.sortarray[right][1 - axis])
				{
					vertex = this.sortarray[right];
					this.sortarray[right] = this.sortarray[left];
					this.sortarray[left] = vertex;
				}
				return;
			}
			int num2 = Dwyer.rand.Next(left, right);
			double item = this.sortarray[num2][axis];
			double item1 = this.sortarray[num2][1 - axis];
			left--;
			right++;
			while (left < right)
			{
				do
				{
					left++;
				}
				while (left <= right && (this.sortarray[left][axis] < item || this.sortarray[left][axis] == item && this.sortarray[left][1 - axis] < item1));
				do
				{
					right--;
				}
				while (left <= right && (this.sortarray[right][axis] > item || this.sortarray[right][axis] == item && this.sortarray[right][1 - axis] > item1));
				if (left >= right)
				{
					continue;
				}
				vertex = this.sortarray[left];
				this.sortarray[left] = this.sortarray[right];
				this.sortarray[right] = vertex;
			}
			if (left > median)
			{
				this.VertexMedian(num, left - 1, median, axis);
			}
			if (right < median - 1)
			{
				this.VertexMedian(right + 1, num1, median, axis);
			}
		}

		private void VertexSort(int left, int right)
		{
			int j;
			int num = left;
			int num1 = right;
			if (right - left + 1 < 32)
			{
				for (int i = left + 1; i <= right; i++)
				{
					Vertex vertex = this.sortarray[i];
					for (j = i - 1; j >= left && (this.sortarray[j].x > vertex.x || this.sortarray[j].x == vertex.x && this.sortarray[j].y > vertex.y); j--)
					{
						this.sortarray[j + 1] = this.sortarray[j];
					}
					this.sortarray[j + 1] = vertex;
				}
				return;
			}
			int num2 = Dwyer.rand.Next(left, right);
			double num3 = this.sortarray[num2].x;
			double num4 = this.sortarray[num2].y;
			left--;
			right++;
			while (left < right)
			{
				do
				{
					left++;
				}
				while (left <= right && (this.sortarray[left].x < num3 || this.sortarray[left].x == num3 && this.sortarray[left].y < num4));
				do
				{
					right--;
				}
				while (left <= right && (this.sortarray[right].x > num3 || this.sortarray[right].x == num3 && this.sortarray[right].y > num4));
				if (left >= right)
				{
					continue;
				}
				Vertex vertex1 = this.sortarray[left];
				this.sortarray[left] = this.sortarray[right];
				this.sortarray[right] = vertex1;
			}
			if (left > num)
			{
				this.VertexSort(num, left);
			}
			if (num1 > right + 1)
			{
				this.VertexSort(right + 1, num1);
			}
		}
	}
}