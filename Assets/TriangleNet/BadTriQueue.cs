using System;
using TriangleNet.Data;

namespace TriangleNet
{
	internal class BadTriQueue
	{
		private readonly static double SQRT2;

		private BadTriangle[] queuefront;

		private BadTriangle[] queuetail;

		private int[] nextnonemptyq;

		private int firstnonemptyq;

		private int count;

		public int Count
		{
			get
			{
				return this.count;
			}
		}

		static BadTriQueue()
		{
			BadTriQueue.SQRT2 = 1.4142135623731;
		}

		public BadTriQueue()
		{
			this.queuefront = new BadTriangle[4096];
			this.queuetail = new BadTriangle[4096];
			this.nextnonemptyq = new int[4096];
			this.firstnonemptyq = -1;
			this.count = 0;
		}

		public BadTriangle Dequeue()
		{
			if (this.firstnonemptyq < 0)
			{
				return null;
			}
			this.count = this.count - 1;
			BadTriangle badTriangle = this.queuefront[this.firstnonemptyq];
			this.queuefront[this.firstnonemptyq] = badTriangle.nexttriang;
			if (badTriangle == this.queuetail[this.firstnonemptyq])
			{
				this.firstnonemptyq = this.nextnonemptyq[this.firstnonemptyq];
			}
			return badTriangle;
		}

		public void Enqueue(BadTriangle badtri)
		{
			double num;
			double i;
			int num1;
			int num2;
			this.count = this.count + 1;
			if (badtri.key < 1)
			{
				num = 1 / badtri.key;
				num2 = 0;
			}
			else
			{
				num = badtri.key;
				num2 = 1;
			}
			int num3 = 0;
			while (num > 2)
			{
				int num4 = 1;
				for (i = 0.5; num * i * i > 1; i = i * i)
				{
					num4 = num4 * 2;
				}
				num3 = num3 + num4;
				num = num * i;
			}
			num3 = 2 * num3 + (num > BadTriQueue.SQRT2 ? 1 : 0);
			num1 = (num2 <= 0 ? 2048 + num3 : 2047 - num3);
			if (this.queuefront[num1] != null)
			{
				this.queuetail[num1].nexttriang = badtri;
			}
			else
			{
				if (num1 <= this.firstnonemptyq)
				{
					int num5 = num1 + 1;
					while (this.queuefront[num5] == null)
					{
						num5++;
					}
					this.nextnonemptyq[num1] = this.nextnonemptyq[num5];
					this.nextnonemptyq[num5] = num1;
				}
				else
				{
					this.nextnonemptyq[num1] = this.firstnonemptyq;
					this.firstnonemptyq = num1;
				}
				this.queuefront[num1] = badtri;
			}
			this.queuetail[num1] = badtri;
			badtri.nexttriang = null;
		}

		public void Enqueue(ref Otri enqtri, double minedge, Vertex enqapex, Vertex enqorg, Vertex enqdest)
		{
			BadTriangle badTriangle = new BadTriangle()
			{
				poortri = enqtri,
				key = minedge,
				triangapex = enqapex,
				triangorg = enqorg,
				triangdest = enqdest
			};
			this.Enqueue(badTriangle);
		}
	}
}