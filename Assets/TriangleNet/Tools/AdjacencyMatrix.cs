using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class AdjacencyMatrix
	{
		private int node_num;

		private int adj_num;

		private int[] adj_row;

		private int[] adj;

		public int[] Adjacency
		{
			get
			{
				return this.adj;
			}
		}

		public int[] AdjacencyRow
		{
			get
			{
				return this.adj_row;
			}
		}

		public AdjacencyMatrix(Mesh mesh)
		{
			this.node_num = mesh.vertices.Count;
			this.adj_row = this.AdjacencyCount(mesh);
			this.adj_num = this.adj_row[this.node_num] - 1;
			this.adj = this.AdjacencySet(mesh, this.adj_row);
		}

		private int[] AdjacencyCount(Mesh mesh)
		{
			int i;
			int[] numArray = new int[this.node_num + 1];
			for (i = 0; i < this.node_num; i++)
			{
				numArray[i] = 1;
			}
			foreach (Triangle value in mesh.triangles.Values)
			{
				int num = value.id;
				int num1 = value.vertices[0].id;
				int num2 = value.vertices[1].id;
				int num3 = value.vertices[2].id;
				int num4 = value.neighbors[2].triangle.id;
				if (num4 < 0 || num < num4)
				{
					numArray[num1] = numArray[num1] + 1;
					numArray[num2] = numArray[num2] + 1;
				}
				num4 = value.neighbors[0].triangle.id;
				if (num4 < 0 || num < num4)
				{
					numArray[num2] = numArray[num2] + 1;
					numArray[num3] = numArray[num3] + 1;
				}
				num4 = value.neighbors[1].triangle.id;
				if (num4 >= 0 && num >= num4)
				{
					continue;
				}
				numArray[num3] = numArray[num3] + 1;
				numArray[num1] = numArray[num1] + 1;
			}
			for (i = this.node_num; 1 <= i; i--)
			{
				numArray[i] = numArray[i - 1];
			}
			numArray[0] = 1;
			for (int j = 1; j <= this.node_num; j++)
			{
				numArray[j] = numArray[j - 1] + numArray[j];
			}
			return numArray;
		}

		private int[] AdjacencySet(Mesh mesh, int[] rows)
		{
			int i;
			int[] numArray = new int[this.node_num];
			Array.Copy(rows, numArray, this.node_num);
			int num = rows[this.node_num] - 1;
			int[] numArray1 = new int[num];
			for (i = 0; i < num; i++)
			{
				numArray1[i] = -1;
			}
			for (i = 0; i < this.node_num; i++)
			{
				numArray1[numArray[i] - 1] = i;
				numArray[i] = numArray[i] + 1;
			}
			foreach (Triangle value in mesh.triangles.Values)
			{
				int num1 = value.id;
				int num2 = value.vertices[0].id;
				int num3 = value.vertices[1].id;
				int num4 = value.vertices[2].id;
				int num5 = value.neighbors[2].triangle.id;
				if (num5 < 0 || num1 < num5)
				{
					numArray1[numArray[num2] - 1] = num3;
					numArray[num2] = numArray[num2] + 1;
					numArray1[numArray[num3] - 1] = num2;
					numArray[num3] = numArray[num3] + 1;
				}
				num5 = value.neighbors[0].triangle.id;
				if (num5 < 0 || num1 < num5)
				{
					numArray1[numArray[num3] - 1] = num4;
					numArray[num3] = numArray[num3] + 1;
					numArray1[numArray[num4] - 1] = num3;
					numArray[num4] = numArray[num4] + 1;
				}
				num5 = value.neighbors[1].triangle.id;
				if (num5 >= 0 && num1 >= num5)
				{
					continue;
				}
				numArray1[numArray[num2] - 1] = num4;
				numArray[num2] = numArray[num2] + 1;
				numArray1[numArray[num4] - 1] = num2;
				numArray[num4] = numArray[num4] + 1;
			}
			for (i = 0; i < this.node_num; i++)
			{
				int num6 = rows[i];
				int num7 = rows[i + 1] - 1;
				this.HeapSort(numArray1, num6 - 1, num7 + 1 - num6);
			}
			return numArray1;
		}

		public int Bandwidth()
		{
			int num = 0;
			int num1 = 0;
			for (int i = 0; i < this.node_num; i++)
			{
				for (int j = this.adj_row[i]; j <= this.adj_row[i + 1] - 1; j++)
				{
					int num2 = this.adj[j - 1];
					num = Math.Max(num, i - num2);
					num1 = Math.Max(num1, num2 - i);
				}
			}
			return num + 1 + num1;
		}

		private void CreateHeap(int[] a, int offset, int size)
		{
			for (int i = size / 2 - 1; 0 <= i; i--)
			{
				int num = a[offset + i];
				int num1 = i;
				while (true)
				{
					int num2 = 2 * num1 + 1;
					if (size <= num2)
					{
						break;
					}
					if (num2 + 1 < size && a[offset + num2] < a[offset + num2 + 1])
					{
						num2++;
					}
					if (num >= a[offset + num2])
					{
						break;
					}
					a[offset + num1] = a[offset + num2];
					num1 = num2;
				}
				a[offset + num1] = num;
			}
		}

		private void HeapSort(int[] a, int offset, int size)
		{
			if (size <= 1)
			{
				return;
			}
			this.CreateHeap(a, offset, size);
			int num = a[offset];
			a[offset] = a[offset + size - 1];
			a[offset + size - 1] = num;
			for (int i = size - 1; 2 <= i; i--)
			{
				this.CreateHeap(a, offset, i);
				num = a[offset];
				a[offset] = a[offset + i - 1];
				a[offset + i - 1] = num;
			}
		}
	}
}