using System;
using TriangleNet.Geometry;
using TriangleNet.Tools;

namespace TriangleNet
{
	public static class Primitives
	{
		private static double splitter;

		private static double epsilon;

		private static double ccwerrboundA;

		private static double iccerrboundA;

		public static double CounterClockwise(Point pa, Point pb, Point pc)
		{
			double num;
			Statistic.CounterClockwiseCount = Statistic.CounterClockwiseCount + (long)1;
			double num1 = (pa.x - pc.x) * (pb.y - pc.y);
			double num2 = (pa.y - pc.y) * (pb.x - pc.x);
			double num3 = num1 - num2;
			if (Behavior.NoExact)
			{
				return num3;
			}
			if (num1 <= 0)
			{
				if (num1 >= 0)
				{
					return num3;
				}
				if (num2 >= 0)
				{
					return num3;
				}
				num = -num1 - num2;
			}
			else
			{
				if (num2 <= 0)
				{
					return num3;
				}
				num = num1 + num2;
			}
			double num4 = Primitives.ccwerrboundA * num;
			if (num3 >= num4 || -num3 >= num4)
			{
				return num3;
			}
			return (double)((double)Primitives.CounterClockwiseDecimal(pa, pb, pc));
		}

		private static decimal CounterClockwiseDecimal(Point pa, Point pb, Point pc)
		{
			Statistic.CounterClockwiseCountDecimal = Statistic.CounterClockwiseCountDecimal + (long)1;
			decimal num = ((decimal)pa.x - (decimal)pc.x) * ((decimal)pb.y - (decimal)pc.y);
			decimal num1 = ((decimal)pa.y - (decimal)pc.y) * ((decimal)pb.x - (decimal)pc.x);
			decimal num2 = num - num1;
			if (num > new decimal(0, 0, 0, false, 1))
			{
				if (num1 <= new decimal(0, 0, 0, false, 1))
				{
					return num2;
				}
			}
			else if (num < new decimal(0, 0, 0, false, 1))
			{
				if (num1 >= new decimal(0, 0, 0, false, 1))
				{
					return num2;
				}
			}
			return num2;
		}

		public static void ExactInit()
		{
			double num;
			bool flag = true;
			double num1 = 0.5;
			Primitives.epsilon = 1;
			Primitives.splitter = 1;
			double num2 = 1;
			do
			{
				num = num2;
				Primitives.epsilon = Primitives.epsilon * num1;
				if (flag)
				{
					Primitives.splitter = Primitives.splitter * 2;
				}
				flag = !flag;
				num2 = 1 + Primitives.epsilon;
			}
			while (num2 != 1 && num2 != num);
			Primitives.splitter = Primitives.splitter + 1;
			Primitives.ccwerrboundA = (3 + 16 * Primitives.epsilon) * Primitives.epsilon;
			Primitives.iccerrboundA = (10 + 96 * Primitives.epsilon) * Primitives.epsilon;
		}

		public static Point FindCircumcenter(Point torg, Point tdest, Point tapex, ref double xi, ref double eta, double offconstant)
		{
			double num;
			double num1;
			double num2;
			Statistic.CircumcenterCount = Statistic.CircumcenterCount + (long)1;
			double num3 = tdest.x - torg.x;
			double num4 = tdest.y - torg.y;
			double num5 = tapex.x - torg.x;
			double num6 = tapex.y - torg.y;
			double num7 = num3 * num3 + num4 * num4;
			double num8 = num5 * num5 + num6 * num6;
			double num9 = (tdest.x - tapex.x) * (tdest.x - tapex.x) + (tdest.y - tapex.y) * (tdest.y - tapex.y);
			if (!Behavior.NoExact)
			{
				num = 0.5 / Primitives.CounterClockwise(tdest, tapex, torg);
				Statistic.CounterClockwiseCount = Statistic.CounterClockwiseCount - (long)1;
			}
			else
			{
				num = 0.5 / (num3 * num6 - num5 * num4);
			}
			double num10 = (num6 * num7 - num4 * num8) * num;
			double num11 = (num3 * num8 - num5 * num7) * num;
			if (num7 < num8 && num7 < num9)
			{
				if (offconstant > 0)
				{
					num1 = 0.5 * num3 - offconstant * num4;
					num2 = 0.5 * num4 + offconstant * num3;
					if (num1 * num1 + num2 * num2 < num10 * num10 + num11 * num11)
					{
						num10 = num1;
						num11 = num2;
					}
				}
			}
			else if (num8 < num9)
			{
				if (offconstant > 0)
				{
					num1 = 0.5 * num5 + offconstant * num6;
					num2 = 0.5 * num6 - offconstant * num5;
					if (num1 * num1 + num2 * num2 < num10 * num10 + num11 * num11)
					{
						num10 = num1;
						num11 = num2;
					}
				}
			}
			else if (offconstant > 0)
			{
				num1 = 0.5 * (tapex.x - tdest.x) - offconstant * (tapex.y - tdest.y);
				num2 = 0.5 * (tapex.y - tdest.y) + offconstant * (tapex.x - tdest.x);
				if (num1 * num1 + num2 * num2 < (num10 - num3) * (num10 - num3) + (num11 - num4) * (num11 - num4))
				{
					num10 = num3 + num1;
					num11 = num4 + num2;
				}
			}
			xi = (num6 * num10 - num5 * num11) * (2 * num);
			eta = (num3 * num11 - num4 * num10) * (2 * num);
			return new Point(torg.x + num10, torg.y + num11);
		}

		public static Point FindCircumcenter(Point torg, Point tdest, Point tapex, ref double xi, ref double eta)
		{
			double num;
			Statistic.CircumcenterCount = Statistic.CircumcenterCount + (long)1;
			double num1 = tdest.x - torg.x;
			double num2 = tdest.y - torg.y;
			double num3 = tapex.x - torg.x;
			double num4 = tapex.y - torg.y;
			double num5 = num1 * num1 + num2 * num2;
			double num6 = num3 * num3 + num4 * num4;
			if (!Behavior.NoExact)
			{
				num = 0.5 / Primitives.CounterClockwise(tdest, tapex, torg);
				Statistic.CounterClockwiseCount = Statistic.CounterClockwiseCount - (long)1;
			}
			else
			{
				num = 0.5 / (num1 * num4 - num3 * num2);
			}
			double num7 = (num4 * num5 - num2 * num6) * num;
			double num8 = (num1 * num6 - num3 * num5) * num;
			xi = (num4 * num7 - num3 * num8) * (2 * num);
			eta = (num1 * num8 - num2 * num7) * (2 * num);
			return new Point(torg.x + num7, torg.y + num8);
		}

		public static double InCircle(Point pa, Point pb, Point pc, Point pd)
		{
			Statistic.InCircleCount = Statistic.InCircleCount + (long)1;
			double num = pa.x - pd.x;
			double num1 = pb.x - pd.x;
			double num2 = pc.x - pd.x;
			double num3 = pa.y - pd.y;
			double num4 = pb.y - pd.y;
			double num5 = pc.y - pd.y;
			double num6 = num1 * num5;
			double num7 = num2 * num4;
			double num8 = num * num + num3 * num3;
			double num9 = num2 * num3;
			double num10 = num * num5;
			double num11 = num1 * num1 + num4 * num4;
			double num12 = num * num4;
			double num13 = num1 * num3;
			double num14 = num2 * num2 + num5 * num5;
			double num15 = num8 * (num6 - num7) + num11 * (num9 - num10) + num14 * (num12 - num13);
			if (Behavior.NoExact)
			{
				return num15;
			}
			double num16 = (Math.Abs(num6) + Math.Abs(num7)) * num8 + (Math.Abs(num9) + Math.Abs(num10)) * num11 + (Math.Abs(num12) + Math.Abs(num13)) * num14;
			double num17 = Primitives.iccerrboundA * num16;
			if (num15 > num17 || -num15 > num17)
			{
				return num15;
			}
			return (double)((double)Primitives.InCircleDecimal(pa, pb, pc, pd));
		}

		private static decimal InCircleDecimal(Point pa, Point pb, Point pc, Point pd)
		{
			Statistic.InCircleCountDecimal = Statistic.InCircleCountDecimal + (long)1;
			decimal num = (decimal)pa.x - (decimal)pd.x;
			decimal num1 = (decimal)pb.x - (decimal)pd.x;
			decimal num2 = (decimal)pc.x - (decimal)pd.x;
			decimal num3 = (decimal)pa.y - (decimal)pd.y;
			decimal num4 = (decimal)pb.y - (decimal)pd.y;
			decimal num5 = (decimal)pc.y - (decimal)pd.y;
			decimal num6 = num1 * num5;
			decimal num7 = num2 * num4;
			decimal num8 = (num * num) + (num3 * num3);
			decimal num9 = num2 * num3;
			decimal num10 = num * num5;
			decimal num11 = (num1 * num1) + (num4 * num4);
			decimal num12 = num * num4;
			decimal num13 = num1 * num3;
			decimal num14 = (num2 * num2) + (num5 * num5);
			return ((num8 * (num6 - num7)) + (num11 * (num9 - num10))) + (num14 * (num12 - num13));
		}

		public static double NonRegular(Point pa, Point pb, Point pc, Point pd)
		{
			return Primitives.InCircle(pa, pb, pc, pd);
		}
	}
}