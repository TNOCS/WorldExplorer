using System;

namespace TriangleNet.Data
{
	internal class BadSubseg
	{
		private static int hashSeed;

		internal int Hash;

		public Osub encsubseg;

		public Vertex subsegorg;

		public Vertex subsegdest;

		static BadSubseg()
		{
		}

		public BadSubseg()
		{
			int num = BadSubseg.hashSeed;
			BadSubseg.hashSeed = num + 1;
			this.Hash = num;
		}

		public override int GetHashCode()
		{
			return this.Hash;
		}

		public override string ToString()
		{
			return string.Format("B-SID {0}", this.encsubseg.seg.hash);
		}
	}
}