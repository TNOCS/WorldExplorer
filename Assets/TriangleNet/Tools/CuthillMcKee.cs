using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Log;

namespace TriangleNet.Tools
{
	public class CuthillMcKee
	{
		private int node_num;

		private AdjacencyMatrix matrix;

		public CuthillMcKee()
		{
		}

		private void Degree(int root, int[] mask, int[] deg, ref int iccsze, int[] ls, int offset)
		{
			int i;
			int num;
			int[] adjacencyRow = this.matrix.AdjacencyRow;
			int[] adjacency = this.matrix.Adjacency;
			int num1 = 1;
			ls[offset] = root;
			adjacencyRow[root] = -adjacencyRow[root];
			int num2 = 0;
			iccsze = 1;
			while (num1 > 0)
			{
				int num3 = num2 + 1;
				num2 = iccsze;
				for (i = num3; i <= num2; i++)
				{
					num = ls[offset + i - 1];
					int num4 = Math.Abs(adjacencyRow[num + 1]) - 1;
					int num5 = 0;
					for (int j = -adjacencyRow[num]; j <= num4; j++)
					{
						int num6 = adjacency[j - 1];
						if (mask[num6] != 0)
						{
							num5++;
							if (0 <= adjacencyRow[num6])
							{
								adjacencyRow[num6] = -adjacencyRow[num6];
								iccsze = iccsze + 1;
								ls[offset + iccsze - 1] = num6;
							}
						}
					}
					deg[num] = num5;
				}
				num1 = iccsze - num2;
			}
			for (i = 0; i < iccsze; i++)
			{
				num = ls[offset + i];
				adjacencyRow[num] = -adjacencyRow[num];
			}
		}

		private void FindRoot(ref int root, int[] mask, ref int level_num, int[] level_row, int[] level, int offset)
		{
			int[] adjacencyRow = this.matrix.AdjacencyRow;
			int[] adjacency = this.matrix.Adjacency;
			int num = 0;
			this.GetLevelSet(ref root, mask, ref level_num, level_row, level, offset);
			int levelRow = level_row[level_num] - 1;
			if (level_num == 1 || level_num == levelRow)
			{
				return;
			}
			do
			{
				int num1 = levelRow;
				int levelRow1 = level_row[level_num - 1];
				root = level[offset + levelRow1 - 1];
				if (levelRow1 < levelRow)
				{
					for (int i = levelRow1; i <= levelRow; i++)
					{
						int num2 = level[offset + i - 1];
						int num3 = 0;
						int num4 = adjacencyRow[num2 - 1];
						int num5 = adjacencyRow[num2] - 1;
						for (int j = num4; j <= num5; j++)
						{
							if (mask[adjacency[j - 1]] > 0)
							{
								num3++;
							}
						}
						if (num3 < num1)
						{
							root = num2;
							num1 = num3;
						}
					}
				}
				this.GetLevelSet(ref root, mask, ref num, level_row, level, offset);
				if (num <= level_num)
				{
					break;
				}
				level_num = num;
			}
			while (levelRow > level_num);
		}

		private int[] GenerateRcm()
		{
			int i;
			int[] numArray = new int[this.node_num];
			int num = 0;
			int num1 = 0;
			int[] numArray1 = new int[this.node_num + 1];
			int[] numArray2 = new int[this.node_num];
			for (i = 0; i < this.node_num; i++)
			{
				numArray2[i] = 1;
			}
			int num2 = 1;
			for (i = 0; i < this.node_num; i++)
			{
				if (numArray2[i] != 0)
				{
					int num3 = i;
					this.FindRoot(ref num3, numArray2, ref num1, numArray1, numArray, num2 - 1);
					this.Rcm(num3, numArray2, numArray, num2 - 1, ref num);
					num2 = num2 + num;
					if (this.node_num < num2)
					{
						return numArray;
					}
				}
			}
			return numArray;
		}

		private void GetLevelSet(ref int root, int[] mask, ref int level_num, int[] level_row, int[] level, int offset)
		{
			int i;
			int[] adjacencyRow = this.matrix.AdjacencyRow;
			int[] adjacency = this.matrix.Adjacency;
			mask[root] = 0;
			level[offset] = root;
			level_num = 0;
			int num = 0;
			int num1 = 1;
			do
			{
				int num2 = num + 1;
				num = num1;
				level_num = level_num + 1;
				level_row[level_num - 1] = num2;
				for (i = num2; i <= num; i++)
				{
					int num3 = level[offset + i - 1];
					int num4 = adjacencyRow[num3];
					int num5 = adjacencyRow[num3 + 1] - 1;
					for (int j = num4; j <= num5; j++)
					{
						int num6 = adjacency[j - 1];
						if (mask[num6] != 0)
						{
							num1++;
							level[offset + num1 - 1] = num6;
							mask[num6] = 0;
						}
					}
				}
			}
			while (num1 - num > 0);
			level_row[level_num] = num + 1;
			for (i = 0; i < num1; i++)
			{
				mask[level[offset + i]] = 1;
			}
		}

		private int PermBandwidth(int[] perm, int[] perm_inv)
		{
			int[] adjacencyRow = this.matrix.AdjacencyRow;
			int[] adjacency = this.matrix.Adjacency;
			int num = 0;
			int num1 = 0;
			for (int i = 0; i < this.node_num; i++)
			{
				for (int j = adjacencyRow[perm[i]]; j <= adjacencyRow[perm[i] + 1] - 1; j++)
				{
					int permInv = perm_inv[adjacency[j - 1]];
					num = Math.Max(num, i - permInv);
					num1 = Math.Max(num1, permInv - i);
				}
			}
			return num + 1 + num1;
		}

		private int[] PermInverse(int n, int[] perm)
		{
			int[] numArray = new int[this.node_num];
			for (int i = 0; i < n; i++)
			{
				numArray[perm[i]] = i;
			}
			return numArray;
		}

		private void Rcm(int root, int[] mask, int[] perm, int offset, ref int iccsze)
		{
			int num;
			int[] adjacencyRow = this.matrix.AdjacencyRow;
			int[] adjacency = this.matrix.Adjacency;
			int[] numArray = new int[this.node_num];
			this.Degree(root, mask, numArray, ref iccsze, perm, offset);
			mask[root] = 0;
			if (iccsze <= 1)
			{
				return;
			}
			int num1 = 0;
			int num2 = 1;
			while (num1 < num2)
			{
				int num3 = num1 + 1;
				num1 = num2;
				for (int i = num3; i <= num1; i++)
				{
					int num4 = perm[offset + i - 1];
					int num5 = adjacencyRow[num4];
					int num6 = adjacencyRow[num4 + 1] - 1;
					int num7 = num2 + 1;
					for (int j = num5; j <= num6; j++)
					{
						num = adjacency[j - 1];
						if (mask[num] != 0)
						{
							num2++;
							mask[num] = 0;
							perm[offset + num2 - 1] = num;
						}
					}
					if (num2 > num7)
					{
						int num8 = num7;
						while (num8 < num2)
						{
							int num9 = num8;
							num8++;
							num = perm[offset + num8 - 1];
							while (num7 < num9)
							{
								int num10 = perm[offset + num9 - 1];
								if (numArray[num10 - 1] <= numArray[num - 1])
								{
									break;
								}
								perm[offset + num9] = num10;
								num9--;
							}
							perm[offset + num9] = num;
						}
					}
				}
			}
			this.ReverseVector(perm, offset, iccsze);
		}

		public int[] Renumber(Mesh mesh)
		{
			this.node_num = mesh.vertices.Count;
			mesh.Renumber(NodeNumbering.Linear);
			this.matrix = new AdjacencyMatrix(mesh);
			int num = this.matrix.Bandwidth();
			int[] numArray = this.GenerateRcm();
			int[] numArray1 = this.PermInverse(this.node_num, numArray);
			int num1 = this.PermBandwidth(numArray, numArray1);
			if (Behavior.Verbose)
			{
				SimpleLog.Instance.Info(string.Format("Reverse Cuthill-McKee (Bandwidth: {0} > {1})", num, num1));
			}
			return numArray1;
		}

		private void ReverseVector(int[] a, int offset, int size)
		{
			for (int i = 0; i < size / 2; i++)
			{
				int num = a[offset + i];
				a[offset + i] = a[offset + size - 1 - i];
				a[offset + size - 1 - i] = num;
			}
		}
	}
}