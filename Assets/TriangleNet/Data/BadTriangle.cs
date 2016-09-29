using System;

namespace TriangleNet.Data
{
	internal class BadTriangle
	{
		public static int OTID;

		public int ID;

		public Otri poortri;

		public double key;

		public Vertex triangorg;

		public Vertex triangdest;

		public Vertex triangapex;

		public BadTriangle nexttriang;

		static BadTriangle()
		{
		}

		public BadTriangle()
		{
			int oTID = BadTriangle.OTID;
			BadTriangle.OTID = oTID + 1;
			this.ID = oTID;
		}

		public override string ToString()
		{
			return string.Format("B-TID {0}", this.poortri.triangle.hash);
		}
	}
}