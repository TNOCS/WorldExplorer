using System;
using System.Collections.Generic;
using System.Linq;

namespace TriangleNet
{
	internal class Sampler
	{
		private static Random rand;

		private int samples = 1;

		private int triangleCount;

		private static int samplefactor;

		private int[] keys;

		static Sampler()
		{
			Sampler.rand = new Random(DateTime.Now.Millisecond);
			Sampler.samplefactor = 11;
		}

		public Sampler()
		{
		}

		public int[] GetSamples(Mesh mesh)
		{
			List<int> nums = new List<int>(this.samples);
			int num = this.triangleCount / this.samples;
			for (int i = 0; i < this.samples; i++)
			{
				int num1 = Sampler.rand.Next(i * num, (i + 1) * num - 1);
				if (mesh.triangles.Keys.Contains<int>(this.keys[num1]))
				{
					nums.Add(this.keys[num1]);
				}
				else
				{
					this.Update(mesh, true);
					i--;
				}
			}
			return nums.ToArray();
		}

		public void Reset()
		{
			this.samples = 1;
			this.triangleCount = 0;
		}

		public void Update(Mesh mesh)
		{
			this.Update(mesh, false);
		}

		public void Update(Mesh mesh, bool forceUpdate)
		{
			int count = mesh.triangles.Count;
			if (this.triangleCount != count | forceUpdate)
			{
				this.triangleCount = count;
				while (Sampler.samplefactor * this.samples * this.samples * this.samples < count)
				{
					this.samples = this.samples + 1;
				}
				this.keys = mesh.triangles.Keys.ToArray<int>();
			}
		}
	}
}