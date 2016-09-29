using System;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Tools;

namespace TriangleNet
{
	internal class NewLocation
	{
		private const double EPS = 1E-50;

		private Mesh mesh;

		private Behavior behavior;

		public NewLocation(Mesh mesh)
		{
			this.mesh = mesh;
			this.behavior = mesh.behavior;
		}

		private bool ChooseCorrectPoint(double x1, double y1, double x2, double y2, double x3, double y3, bool isObtuse)
		{
			bool flag;
			double num = (x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3);
			double num1 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
			if (!isObtuse)
			{
				flag = (num1 >= num ? false : true);
			}
			else
			{
				flag = (num1 < num ? false : true);
			}
			return flag;
		}

		private void CircleLineIntersection(double x1, double y1, double x2, double y2, double x3, double y3, double r, ref double[] p)
		{
			double num;
			double num1 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
			double num2 = 2 * ((x2 - x1) * (x1 - x3) + (y2 - y1) * (y1 - y3));
			double num3 = x3 * x3 + y3 * y3 + x1 * x1 + y1 * y1 - 2 * (x3 * x1 + y3 * y1) - r * r;
			double num4 = num2 * num2 - 4 * num1 * num3;
			if (num4 < 0)
			{
				p[0] = 0;
				return;
			}
			if (Math.Abs(num4 - 0) < 1E-50)
			{
				p[0] = 1;
				num = -num2 / (2 * num1);
				p[1] = x1 + num * (x2 - x1);
				p[2] = y1 + num * (y2 - y1);
				return;
			}
			if (num4 <= 0 || Math.Abs(num1 - 0) < 1E-50)
			{
				p[0] = 0;
				return;
			}
			p[0] = 2;
			num = (-num2 + Math.Sqrt(num4)) / (2 * num1);
			p[1] = x1 + num * (x2 - x1);
			p[2] = y1 + num * (y2 - y1);
			num = (-num2 - Math.Sqrt(num4)) / (2 * num1);
			p[3] = x1 + num * (x2 - x1);
			p[4] = y1 + num * (y2 - y1);
		}

		private int DoSmoothing(Otri badotri, Vertex torg, Vertex tdest, Vertex tapex, ref double[] newloc)
		{
			int starPoints = 0;
			int num = 0;
			int starPoints1 = 0;
			double[] numArray = new double[6];
			int num1 = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			bool flag = false;
			double[] numArray1 = new double[500];
			double[] numArray2 = new double[500];
			double[] numArray3 = new double[500];
			starPoints = this.GetStarPoints(badotri, torg, tdest, tapex, 1, ref numArray1);
			if (torg.type == VertexType.FreeVertex && starPoints != 0 && this.ValidPolygonAngles(starPoints, numArray1))
			{
				flag = (this.behavior.MaxAngle != 0 ? this.GetWedgeIntersection(starPoints, numArray1, ref newloc) : this.GetWedgeIntersectionWithoutMaxAngle(starPoints, numArray1, ref newloc));
				if (flag)
				{
					numArray[0] = newloc[0];
					numArray[1] = newloc[1];
					num1++;
					num2 = 1;
				}
			}
			num = this.GetStarPoints(badotri, torg, tdest, tapex, 2, ref numArray2);
			if (tdest.type == VertexType.FreeVertex && num != 0 && this.ValidPolygonAngles(num, numArray2))
			{
				flag = (this.behavior.MaxAngle != 0 ? this.GetWedgeIntersection(num, numArray2, ref newloc) : this.GetWedgeIntersectionWithoutMaxAngle(num, numArray2, ref newloc));
				if (flag)
				{
					numArray[2] = newloc[0];
					numArray[3] = newloc[1];
					num1++;
					num3 = 2;
				}
			}
			starPoints1 = this.GetStarPoints(badotri, torg, tdest, tapex, 3, ref numArray3);
			if (tapex.type == VertexType.FreeVertex && starPoints1 != 0 && this.ValidPolygonAngles(starPoints1, numArray3))
			{
				flag = (this.behavior.MaxAngle != 0 ? this.GetWedgeIntersection(starPoints1, numArray3, ref newloc) : this.GetWedgeIntersectionWithoutMaxAngle(starPoints1, numArray3, ref newloc));
				if (flag)
				{
					numArray[4] = newloc[0];
					numArray[5] = newloc[1];
					num1++;
					num4 = 3;
				}
			}
			if (num1 > 0)
			{
				if (num2 > 0)
				{
					newloc[0] = numArray[0];
					newloc[1] = numArray[1];
					return num2;
				}
				if (num3 > 0)
				{
					newloc[0] = numArray[2];
					newloc[1] = numArray[3];
					return num3;
				}
				if (num4 > 0)
				{
					newloc[0] = numArray[4];
					newloc[1] = numArray[5];
					return num4;
				}
			}
			return 0;
		}

		public Point FindLocation(Vertex torg, Vertex tdest, Vertex tapex, ref double xi, ref double eta, bool offcenter, Otri badotri)
		{
			if (this.behavior.MaxAngle == 0)
			{
				return this.FindNewLocationWithoutMaxAngle(torg, tdest, tapex, ref xi, ref eta, true, badotri);
			}
			return this.FindNewLocation(torg, tdest, tapex, ref xi, ref eta, true, badotri);
		}

		private Point FindNewLocation(Vertex torg, Vertex tdest, Vertex tapex, ref double xi, ref double eta, bool offcenter, Otri badotri)
		{
			double num;
			double num1;
			double num2;
			Point point;
			Point point1;
			Point point2;
			Point point3;
			bool flag;
			double num3;
			double num4;
			Vertex vertex;
			Vertex vertex1;
			double num5;
			double num6;
			double num7;
			double num8;
			double num9;
			double num10;
			double num11;
			double num12;
			double num13;
			double num14;
			double num15;
			double num16;
			double num17;
			double num18;
			double num19;
			double num20 = this.behavior.offconstant;
			double num21 = 0;
			double num22 = 0;
			double num23 = 0;
			double num24 = 0;
			double num25 = 0;
			int num26 = 0;
			int num27 = 0;
			Otri otri = new Otri();
			double[] numArray = new double[2];
			double num28 = 0;
			double num29 = 0;
			double[] numArray1 = new double[5];
			double[] numArray2 = new double[4];
			double num30 = 0.06;
			double num31 = 1;
			double num32 = 1;
			int num33 = 0;
			double[] numArray3 = new double[2];
			double num34 = 0;
			double num35 = 0;
			double num36 = 0;
			double num37 = 0;
			double[] numArray4 = new double[3];
			double[] numArray5 = new double[4];
			Statistic.CircumcenterCount = Statistic.CircumcenterCount + (long)1;
			double num38 = tdest.x - torg.x;
			double num39 = tdest.y - torg.y;
			double num40 = tapex.x - torg.x;
			double num41 = tapex.y - torg.y;
			double num42 = tapex.x - tdest.x;
			double num43 = tapex.y - tdest.y;
			double num44 = num38 * num38 + num39 * num39;
			double num45 = num40 * num40 + num41 * num41;
			double num46 = (tdest.x - tapex.x) * (tdest.x - tapex.x) + (tdest.y - tapex.y) * (tdest.y - tapex.y);
			if (!Behavior.NoExact)
			{
				num = 0.5 / Primitives.CounterClockwise(tdest, tapex, torg);
				Statistic.CounterClockwiseCount = Statistic.CounterClockwiseCount - (long)1;
			}
			else
			{
				num = 0.5 / (num38 * num41 - num40 * num39);
			}
			double num47 = (num41 * num44 - num39 * num45) * num;
			double num48 = (num38 * num45 - num40 * num44) * num;
			Point point4 = new Point(torg.x + num47, torg.y + num48);
			Otri otri1 = badotri;
			num26 = this.LongestShortestEdge(num45, num46, num44);
			if (num26 > 213)
			{
				if (num26 == 231)
				{
					num21 = num42;
					num22 = num43;
					num23 = num46;
					num24 = num44;
					num25 = num45;
					point = torg;
					point1 = tapex;
					point2 = tdest;
				}
				else
				{
					if (num26 != 312)
					{
						goto Label1;
					}
					num21 = num38;
					num22 = num39;
					num23 = num44;
					num24 = num45;
					num25 = num46;
					point = tapex;
					point1 = tdest;
					point2 = torg;
				}
			}
			else if (num26 == 123)
			{
				num21 = num40;
				num22 = num41;
				num23 = num45;
				num24 = num46;
				num25 = num44;
				point = tdest;
				point1 = torg;
				point2 = tapex;
			}
			else if (num26 == 132)
			{
				num21 = num40;
				num22 = num41;
				num23 = num45;
				num24 = num44;
				num25 = num46;
				point = tdest;
				point1 = tapex;
				point2 = torg;
			}
			else
			{
				if (num26 != 213)
				{
					goto Label0;
				}
				num21 = num42;
				num22 = num43;
				num23 = num46;
				num24 = num45;
				num25 = num44;
				point = torg;
				point1 = tdest;
				point2 = tapex;
			}
		Label3:
			if (offcenter && num20 > 0)
			{
				if (num26 == 213 || num26 == 231)
				{
					num1 = 0.5 * num21 - num20 * num22;
					num2 = 0.5 * num22 + num20 * num21;
					if (num1 * num1 + num2 * num2 >= (num47 - num38) * (num47 - num38) + (num48 - num39) * (num48 - num39))
					{
						num27 = 1;
					}
					else
					{
						num47 = num38 + num1;
						num48 = num39 + num2;
					}
				}
				else if (num26 == 123 || num26 == 132)
				{
					num1 = 0.5 * num21 + num20 * num22;
					num2 = 0.5 * num22 - num20 * num21;
					if (num1 * num1 + num2 * num2 >= num47 * num47 + num48 * num48)
					{
						num27 = 1;
					}
					else
					{
						num47 = num1;
						num48 = num2;
					}
				}
				else
				{
					num1 = 0.5 * num21 - num20 * num22;
					num2 = 0.5 * num22 + num20 * num21;
					if (num1 * num1 + num2 * num2 >= num47 * num47 + num48 * num48)
					{
						num27 = 1;
					}
					else
					{
						num47 = num1;
						num48 = num2;
					}
				}
			}
			if (num27 == 1)
			{
				double num49 = (num24 + num23 - num25) / (2 * Math.Sqrt(num24) * Math.Sqrt(num23));
				if (num49 >= 0)
				{
					flag = (Math.Abs(num49 - 0) > 1E-50 ? false : true);
				}
				else
				{
					flag = true;
				}
				num33 = this.DoSmoothing(otri1, torg, tdest, tapex, ref numArray3);
				if (num33 <= 0)
				{
					double num50 = Math.Acos((num24 + num25 - num23) / (2 * Math.Sqrt(num24) * Math.Sqrt(num25))) * 180 / 3.14159265358979;
					num50 = (this.behavior.MinAngle <= num50 ? num50 + 0.5 : this.behavior.MinAngle);
					double num51 = Math.Sqrt(num23) / (2 * Math.Sin(num50 * 3.14159265358979 / 180));
					double num52 = (point1.x + point2.x) / 2;
					double num53 = (point1.y + point2.y) / 2;
					double num54 = num52 + Math.Sqrt(num51 * num51 - num23 / 4) * (point1.y - point2.y) / Math.Sqrt(num23);
					double num55 = num53 + Math.Sqrt(num51 * num51 - num23 / 4) * (point2.x - point1.x) / Math.Sqrt(num23);
					double num56 = num52 - Math.Sqrt(num51 * num51 - num23 / 4) * (point1.y - point2.y) / Math.Sqrt(num23);
					double num57 = num53 - Math.Sqrt(num51 * num51 - num23 / 4) * (point2.x - point1.x) / Math.Sqrt(num23);
					if ((num54 - point.x) * (num54 - point.x) + (num55 - point.y) * (num55 - point.y) > (num56 - point.x) * (num56 - point.x) + (num57 - point.y) * (num57 - point.y))
					{
						num3 = num56;
						num4 = num57;
					}
					else
					{
						num3 = num54;
						num4 = num55;
					}
					bool neighborsVertex = this.GetNeighborsVertex(badotri, point1.x, point1.y, point.x, point.y, ref numArray, ref otri);
					double num58 = num47;
					double num59 = num48;
					double num60 = Math.Sqrt((num3 - num52) * (num3 - num52) + (num4 - num53) * (num4 - num53));
					double num61 = (num3 - num52) / num60;
					double num62 = (num4 - num53) / num60;
					double num63 = num3 + num61 * num51;
					double num64 = num4 + num62 * num51;
					double maxAngle = (2 * this.behavior.MaxAngle + num50 - 180) * 3.14159265358979 / 180;
					double num65 = num63 * Math.Cos(maxAngle) + num64 * Math.Sin(maxAngle) + num3 - num3 * Math.Cos(maxAngle) - num4 * Math.Sin(maxAngle);
					double num66 = -num63 * Math.Sin(maxAngle) + num64 * Math.Cos(maxAngle) + num4 + num3 * Math.Sin(maxAngle) - num4 * Math.Cos(maxAngle);
					double num67 = num63 * Math.Cos(maxAngle) - num64 * Math.Sin(maxAngle) + num3 - num3 * Math.Cos(maxAngle) + num4 * Math.Sin(maxAngle);
					double num68 = num63 * Math.Sin(maxAngle) + num64 * Math.Cos(maxAngle) + num4 - num3 * Math.Sin(maxAngle) - num4 * Math.Cos(maxAngle);
					if (!this.ChooseCorrectPoint(num67, num68, point1.x, point1.y, num65, num66, true))
					{
						num16 = num67;
						num17 = num68;
						num18 = num65;
						num19 = num66;
					}
					else
					{
						num16 = num65;
						num17 = num66;
						num18 = num67;
						num19 = num68;
					}
					double num69 = (point1.x + point.x) / 2;
					double num70 = (point1.y + point.y) / 2;
					if (!neighborsVertex)
					{
						Vertex vertex2 = otri.Org();
						vertex = otri.Dest();
						vertex1 = otri.Apex();
						point3 = Primitives.FindCircumcenter(vertex2, vertex, vertex1, ref num28, ref num29);
						num5 = point1.y - point.y;
						num6 = point.x - point1.x;
						num5 = point4.x + num5;
						num6 = point4.y + num6;
						this.CircleLineIntersection(point4.x, point4.y, num5, num6, num3, num4, num51, ref numArray1);
						if (!this.ChooseCorrectPoint(num69, num70, numArray1[3], numArray1[4], point4.x, point4.y, flag))
						{
							num7 = numArray1[1];
							num8 = numArray1[2];
						}
						else
						{
							num7 = numArray1[3];
							num8 = numArray1[4];
						}
						num12 = point1.x;
						num13 = point1.y;
						num61 = point2.x - point1.x;
						num62 = point2.y - point1.y;
						num14 = num16;
						num15 = num17;
						this.LineLineIntersection(point4.x, point4.y, num5, num6, num12, num13, num14, num15, ref numArray4);
						if (numArray4[0] > 0)
						{
							num36 = numArray4[1];
							num37 = numArray4[2];
						}
						this.PointBetweenPoints(num7, num8, point4.x, point4.y, point3.x, point3.y, ref numArray2);
						if (numArray1[0] > 0)
						{
							if (Math.Abs(numArray2[0] - 1) > 1E-50)
							{
								this.PointBetweenPoints(num7, num8, point4.x, point4.y, num36, num37, ref numArray5);
								if (Math.Abs(numArray5[0] - 1) > 1E-50 || numArray4[0] <= 0)
								{
									if (!this.IsBadTriangleAngle(point2.x, point2.y, point1.x, point1.y, num7, num8))
									{
										num58 = num7 - torg.x;
										num59 = num8 - torg.y;
									}
									else
									{
										num11 = Math.Sqrt((num7 - point4.x) * (num7 - point4.x) + (num8 - point4.y) * (num8 - point4.y));
										num9 = point4.x - num7;
										num10 = point4.y - num8;
										num9 = num9 / num11;
										num10 = num10 / num11;
										num7 = num7 + num9 * num30 * Math.Sqrt(num23);
										num8 = num8 + num10 * num30 * Math.Sqrt(num23);
										if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num7, num8))
										{
											num58 = num7 - torg.x;
											num59 = num8 - torg.y;
										}
										else
										{
											num58 = num47;
											num59 = num48;
										}
									}
								}
								else if ((point.x - num16) * (point.x - num16) + (point.y - num17) * (point.y - num17) > num31 * ((point.x - num36) * (point.x - num36) + (point.y - num37) * (point.y - num37)) && this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num16, num17) && this.MinDistanceToNeighbor(num16, num17, ref otri) > this.MinDistanceToNeighbor(num36, num37, ref otri))
								{
									num58 = num16 - torg.x;
									num59 = num17 - torg.y;
								}
								else if (!this.IsBadTriangleAngle(point2.x, point2.y, point1.x, point1.y, num36, num37))
								{
									num58 = numArray5[2] - torg.x;
									num59 = numArray5[3] - torg.y;
								}
								else
								{
									num11 = Math.Sqrt((num36 - point4.x) * (num36 - point4.x) + (num37 - point4.y) * (num37 - point4.y));
									num9 = point4.x - num36;
									num10 = point4.y - num37;
									num9 = num9 / num11;
									num10 = num10 / num11;
									num36 = num36 + num9 * num30 * Math.Sqrt(num23);
									num37 = num37 + num10 * num30 * Math.Sqrt(num23);
									if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num36, num37))
									{
										num58 = num36 - torg.x;
										num59 = num37 - torg.y;
									}
									else
									{
										num58 = num47;
										num59 = num48;
									}
								}
							}
							else
							{
								this.PointBetweenPoints(numArray2[2], numArray2[3], point4.x, point4.y, num36, num37, ref numArray5);
								if (Math.Abs(numArray5[0] - 1) > 1E-50 || numArray4[0] <= 0)
								{
									if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, point3.x, point3.y))
									{
										num58 = numArray2[2] - torg.x;
										num59 = numArray2[3] - torg.y;
									}
									else
									{
										num58 = num47;
										num59 = num48;
									}
								}
								else if ((point.x - num16) * (point.x - num16) + (point.y - num17) * (point.y - num17) > num31 * ((point.x - num36) * (point.x - num36) + (point.y - num37) * (point.y - num37)) && this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num16, num17) && this.MinDistanceToNeighbor(num16, num17, ref otri) > this.MinDistanceToNeighbor(num36, num37, ref otri))
								{
									num58 = num16 - torg.x;
									num59 = num17 - torg.y;
								}
								else if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num36, num37))
								{
									num58 = numArray5[2] - torg.x;
									num59 = numArray5[3] - torg.y;
								}
								else
								{
									num11 = Math.Sqrt((num36 - point4.x) * (num36 - point4.x) + (num37 - point4.y) * (num37 - point4.y));
									num9 = point4.x - num36;
									num10 = point4.y - num37;
									num9 = num9 / num11;
									num10 = num10 / num11;
									num36 = num36 + num9 * num30 * Math.Sqrt(num23);
									num37 = num37 + num10 * num30 * Math.Sqrt(num23);
									if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num36, num37))
									{
										num58 = num36 - torg.x;
										num59 = num37 - torg.y;
									}
									else
									{
										num58 = num47;
										num59 = num48;
									}
								}
							}
							if ((point.x - point4.x) * (point.x - point4.x) + (point.y - point4.y) * (point.y - point4.y) > num31 * ((point.x - (num58 + torg.x)) * (point.x - (num58 + torg.x)) + (point.y - (num59 + torg.y)) * (point.y - (num59 + torg.y))))
							{
								num58 = num47;
								num59 = num48;
							}
						}
					}
					bool neighborsVertex1 = this.GetNeighborsVertex(badotri, point2.x, point2.y, point.x, point.y, ref numArray, ref otri);
					double num71 = num47;
					double num72 = num48;
					double num73 = (point2.x + point.x) / 2;
					double num74 = (point2.y + point.y) / 2;
					if (!neighborsVertex1)
					{
						Vertex vertex3 = otri.Org();
						vertex = otri.Dest();
						vertex1 = otri.Apex();
						point3 = Primitives.FindCircumcenter(vertex3, vertex, vertex1, ref num28, ref num29);
						num5 = point2.y - point.y;
						num6 = point.x - point2.x;
						num5 = point4.x + num5;
						num6 = point4.y + num6;
						this.CircleLineIntersection(point4.x, point4.y, num5, num6, num3, num4, num51, ref numArray1);
						if (!this.ChooseCorrectPoint(num73, num74, numArray1[3], numArray1[4], point4.x, point4.y, false))
						{
							num7 = numArray1[1];
							num8 = numArray1[2];
						}
						else
						{
							num7 = numArray1[3];
							num8 = numArray1[4];
						}
						num12 = point2.x;
						num13 = point2.y;
						num61 = point1.x - point2.x;
						num62 = point1.y - point2.y;
						num14 = num18;
						num15 = num19;
						this.LineLineIntersection(point4.x, point4.y, num5, num6, num12, num13, num14, num15, ref numArray4);
						if (numArray4[0] > 0)
						{
							num36 = numArray4[1];
							num37 = numArray4[2];
						}
						this.PointBetweenPoints(num7, num8, point4.x, point4.y, point3.x, point3.y, ref numArray2);
						if (numArray1[0] > 0)
						{
							if (Math.Abs(numArray2[0] - 1) > 1E-50)
							{
								this.PointBetweenPoints(num7, num8, point4.x, point4.y, num36, num37, ref numArray5);
								if (Math.Abs(numArray5[0] - 1) > 1E-50 || numArray4[0] <= 0)
								{
									if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num7, num8))
									{
										num71 = num7 - torg.x;
										num72 = num8 - torg.y;
									}
									else
									{
										num11 = Math.Sqrt((num7 - point4.x) * (num7 - point4.x) + (num8 - point4.y) * (num8 - point4.y));
										num9 = point4.x - num7;
										num10 = point4.y - num8;
										num9 = num9 / num11;
										num10 = num10 / num11;
										num7 = num7 + num9 * num30 * Math.Sqrt(num23);
										num8 = num8 + num10 * num30 * Math.Sqrt(num23);
										if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num7, num8))
										{
											num71 = num7 - torg.x;
											num72 = num8 - torg.y;
										}
										else
										{
											num71 = num47;
											num72 = num48;
										}
									}
								}
								else if ((point.x - num18) * (point.x - num18) + (point.y - num19) * (point.y - num19) > num31 * ((point.x - num36) * (point.x - num36) + (point.y - num37) * (point.y - num37)) && this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num18, num19) && this.MinDistanceToNeighbor(num18, num19, ref otri) > this.MinDistanceToNeighbor(num36, num37, ref otri))
								{
									num71 = num18 - torg.x;
									num72 = num19 - torg.y;
								}
								else if (!this.IsBadTriangleAngle(point2.x, point2.y, point1.x, point1.y, num36, num37))
								{
									num71 = numArray5[2] - torg.x;
									num72 = numArray5[3] - torg.y;
								}
								else
								{
									num11 = Math.Sqrt((num36 - point4.x) * (num36 - point4.x) + (num37 - point4.y) * (num37 - point4.y));
									num9 = point4.x - num36;
									num10 = point4.y - num37;
									num9 = num9 / num11;
									num10 = num10 / num11;
									num36 = num36 + num9 * num30 * Math.Sqrt(num23);
									num37 = num37 + num10 * num30 * Math.Sqrt(num23);
									if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num36, num37))
									{
										num71 = num36 - torg.x;
										num72 = num37 - torg.y;
									}
									else
									{
										num71 = num47;
										num72 = num48;
									}
								}
							}
							else
							{
								this.PointBetweenPoints(numArray2[2], numArray2[3], point4.x, point4.y, num36, num37, ref numArray5);
								if (Math.Abs(numArray5[0] - 1) > 1E-50 || numArray4[0] <= 0)
								{
									if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, point3.x, point3.y))
									{
										num71 = numArray2[2] - torg.x;
										num72 = numArray2[3] - torg.y;
									}
									else
									{
										num71 = num47;
										num72 = num48;
									}
								}
								else if ((point.x - num18) * (point.x - num18) + (point.y - num19) * (point.y - num19) > num31 * ((point.x - num36) * (point.x - num36) + (point.y - num37) * (point.y - num37)) && this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num18, num19) && this.MinDistanceToNeighbor(num18, num19, ref otri) > this.MinDistanceToNeighbor(num36, num37, ref otri))
								{
									num71 = num18 - torg.x;
									num72 = num19 - torg.y;
								}
								else if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num36, num37))
								{
									num71 = numArray5[2] - torg.x;
									num72 = numArray5[3] - torg.y;
								}
								else
								{
									num11 = Math.Sqrt((num36 - point4.x) * (num36 - point4.x) + (num37 - point4.y) * (num37 - point4.y));
									num9 = point4.x - num36;
									num10 = point4.y - num37;
									num9 = num9 / num11;
									num10 = num10 / num11;
									num36 = num36 + num9 * num30 * Math.Sqrt(num23);
									num37 = num37 + num10 * num30 * Math.Sqrt(num23);
									if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num36, num37))
									{
										num71 = num36 - torg.x;
										num72 = num37 - torg.y;
									}
									else
									{
										num71 = num47;
										num72 = num48;
									}
								}
							}
							if ((point.x - point4.x) * (point.x - point4.x) + (point.y - point4.y) * (point.y - point4.y) > num31 * ((point.x - (num71 + torg.x)) * (point.x - (num71 + torg.x)) + (point.y - (num72 + torg.y)) * (point.y - (num72 + torg.y))))
							{
								num71 = num47;
								num72 = num48;
							}
						}
					}
					if (flag)
					{
						if (neighborsVertex & neighborsVertex1)
						{
							if (num32 * ((point.x - num73) * (point.x - num73) + (point.y - num74) * (point.y - num74)) <= (point.x - num69) * (point.x - num69) + (point.y - num70) * (point.y - num70))
							{
								num47 = num58;
								num48 = num59;
							}
							else
							{
								num47 = num71;
								num48 = num72;
							}
						}
						else if (neighborsVertex)
						{
							if (num32 * ((point.x - (num71 + torg.x)) * (point.x - (num71 + torg.x)) + (point.y - (num72 + torg.y)) * (point.y - (num72 + torg.y))) <= (point.x - num69) * (point.x - num69) + (point.y - num70) * (point.y - num70))
							{
								num47 = num58;
								num48 = num59;
							}
							else
							{
								num47 = num71;
								num48 = num72;
							}
						}
						else if (neighborsVertex1)
						{
							if (num32 * ((point.x - num73) * (point.x - num73) + (point.y - num74) * (point.y - num74)) <= (point.x - (num58 + torg.x)) * (point.x - (num58 + torg.x)) + (point.y - (num59 + torg.y)) * (point.y - (num59 + torg.y)))
							{
								num47 = num58;
								num48 = num59;
							}
							else
							{
								num47 = num71;
								num48 = num72;
							}
						}
						else if (num32 * ((point.x - (num71 + torg.x)) * (point.x - (num71 + torg.x)) + (point.y - (num72 + torg.y)) * (point.y - (num72 + torg.y))) <= (point.x - (num58 + torg.x)) * (point.x - (num58 + torg.x)) + (point.y - (num59 + torg.y)) * (point.y - (num59 + torg.y)))
						{
							num47 = num58;
							num48 = num59;
						}
						else
						{
							num47 = num71;
							num48 = num72;
						}
					}
					else if (neighborsVertex & neighborsVertex1)
					{
						if (num32 * ((point.x - num73) * (point.x - num73) + (point.y - num74) * (point.y - num74)) <= (point.x - num69) * (point.x - num69) + (point.y - num70) * (point.y - num70))
						{
							num47 = num58;
							num48 = num59;
						}
						else
						{
							num47 = num71;
							num48 = num72;
						}
					}
					else if (neighborsVertex)
					{
						if (num32 * ((point.x - (num71 + torg.x)) * (point.x - (num71 + torg.x)) + (point.y - (num72 + torg.y)) * (point.y - (num72 + torg.y))) <= (point.x - num69) * (point.x - num69) + (point.y - num70) * (point.y - num70))
						{
							num47 = num58;
							num48 = num59;
						}
						else
						{
							num47 = num71;
							num48 = num72;
						}
					}
					else if (neighborsVertex1)
					{
						if (num32 * ((point.x - num73) * (point.x - num73) + (point.y - num74) * (point.y - num74)) <= (point.x - (num58 + torg.x)) * (point.x - (num58 + torg.x)) + (point.y - (num59 + torg.y)) * (point.y - (num59 + torg.y)))
						{
							num47 = num58;
							num48 = num59;
						}
						else
						{
							num47 = num71;
							num48 = num72;
						}
					}
					else if (num32 * ((point.x - (num71 + torg.x)) * (point.x - (num71 + torg.x)) + (point.y - (num72 + torg.y)) * (point.y - (num72 + torg.y))) <= (point.x - (num58 + torg.x)) * (point.x - (num58 + torg.x)) + (point.y - (num59 + torg.y)) * (point.y - (num59 + torg.y)))
					{
						num47 = num58;
						num48 = num59;
					}
					else
					{
						num47 = num71;
						num48 = num72;
					}
				}
				else
				{
					Statistic.RelocationCount = Statistic.RelocationCount + (long)1;
					num47 = numArray3[0] - torg.x;
					num48 = numArray3[1] - torg.y;
					num34 = torg.x;
					num35 = torg.y;
					switch (num33)
					{
						case 1:
						{
							this.mesh.DeleteVertex(ref otri1);
							break;
						}
						case 2:
						{
							otri1.LnextSelf();
							this.mesh.DeleteVertex(ref otri1);
							break;
						}
						case 3:
						{
							otri1.LprevSelf();
							this.mesh.DeleteVertex(ref otri1);
							break;
						}
					}
				}
			}
			Point point5 = new Point();
			if (num33 > 0)
			{
				point5.x = num34 + num47;
				point5.y = num35 + num48;
			}
			else
			{
				point5.x = torg.x + num47;
				point5.y = torg.y + num48;
			}
			xi = (num41 * num47 - num40 * num48) * (2 * num);
			eta = (num38 * num48 - num39 * num47) * (2 * num);
			return point5;
		Label0:
			num21 = num38;
			num22 = num39;
			num23 = num44;
			num24 = num46;
			num25 = num45;
			point = tapex;
			point1 = torg;
			point2 = tdest;
			goto Label3;
		Label1:
			if (num26 != 321)
			{
				goto Label0;
			}
			else
			{
				goto Label0;
			}
		}

		private Point FindNewLocationWithoutMaxAngle(Vertex torg, Vertex tdest, Vertex tapex, ref double xi, ref double eta, bool offcenter, Otri badotri)
		{
			double num;
			double num1;
			double num2;
			Point point;
			Point point1;
			Point point2;
			Point point3;
			bool flag;
			double num3;
			double num4;
			Vertex vertex;
			Vertex vertex1;
			double num5;
			double num6;
			double num7;
			double num8;
			double num9;
			double num10;
			double num11;
			double num12 = this.behavior.offconstant;
			double num13 = 0;
			double num14 = 0;
			double num15 = 0;
			double num16 = 0;
			double num17 = 0;
			int num18 = 0;
			int num19 = 0;
			Otri otri = new Otri();
			double[] numArray = new double[2];
			double num20 = 0;
			double num21 = 0;
			double[] numArray1 = new double[5];
			double[] numArray2 = new double[4];
			double num22 = 0.06;
			double num23 = 1;
			double num24 = 1;
			int num25 = 0;
			double[] numArray3 = new double[2];
			double num26 = 0;
			double num27 = 0;
			Statistic.CircumcenterCount = Statistic.CircumcenterCount + (long)1;
			double num28 = tdest.x - torg.x;
			double num29 = tdest.y - torg.y;
			double num30 = tapex.x - torg.x;
			double num31 = tapex.y - torg.y;
			double num32 = tapex.x - tdest.x;
			double num33 = tapex.y - tdest.y;
			double num34 = num28 * num28 + num29 * num29;
			double num35 = num30 * num30 + num31 * num31;
			double num36 = (tdest.x - tapex.x) * (tdest.x - tapex.x) + (tdest.y - tapex.y) * (tdest.y - tapex.y);
			if (!Behavior.NoExact)
			{
				num = 0.5 / Primitives.CounterClockwise(tdest, tapex, torg);
				Statistic.CounterClockwiseCount = Statistic.CounterClockwiseCount - (long)1;
			}
			else
			{
				num = 0.5 / (num28 * num31 - num30 * num29);
			}
			double num37 = (num31 * num34 - num29 * num35) * num;
			double num38 = (num28 * num35 - num30 * num34) * num;
			Point point4 = new Point(torg.x + num37, torg.y + num38);
			Otri otri1 = badotri;
			num18 = this.LongestShortestEdge(num35, num36, num34);
			if (num18 > 213)
			{
				if (num18 == 231)
				{
					num13 = num32;
					num14 = num33;
					num15 = num36;
					num16 = num34;
					num17 = num35;
					point = torg;
					point1 = tapex;
					point2 = tdest;
				}
				else
				{
					if (num18 != 312)
					{
						goto Label1;
					}
					num13 = num28;
					num14 = num29;
					num15 = num34;
					num16 = num35;
					num17 = num36;
					point = tapex;
					point1 = tdest;
					point2 = torg;
				}
			}
			else if (num18 == 123)
			{
				num13 = num30;
				num14 = num31;
				num15 = num35;
				num16 = num36;
				num17 = num34;
				point = tdest;
				point1 = torg;
				point2 = tapex;
			}
			else if (num18 == 132)
			{
				num13 = num30;
				num14 = num31;
				num15 = num35;
				num16 = num34;
				num17 = num36;
				point = tdest;
				point1 = tapex;
				point2 = torg;
			}
			else
			{
				if (num18 != 213)
				{
					goto Label0;
				}
				num13 = num32;
				num14 = num33;
				num15 = num36;
				num16 = num35;
				num17 = num34;
				point = torg;
				point1 = tdest;
				point2 = tapex;
			}
		Label3:
			if (offcenter && num12 > 0)
			{
				if (num18 == 213 || num18 == 231)
				{
					num1 = 0.5 * num13 - num12 * num14;
					num2 = 0.5 * num14 + num12 * num13;
					if (num1 * num1 + num2 * num2 >= (num37 - num28) * (num37 - num28) + (num38 - num29) * (num38 - num29))
					{
						num19 = 1;
					}
					else
					{
						num37 = num28 + num1;
						num38 = num29 + num2;
					}
				}
				else if (num18 == 123 || num18 == 132)
				{
					num1 = 0.5 * num13 + num12 * num14;
					num2 = 0.5 * num14 - num12 * num13;
					if (num1 * num1 + num2 * num2 >= num37 * num37 + num38 * num38)
					{
						num19 = 1;
					}
					else
					{
						num37 = num1;
						num38 = num2;
					}
				}
				else
				{
					num1 = 0.5 * num13 - num12 * num14;
					num2 = 0.5 * num14 + num12 * num13;
					if (num1 * num1 + num2 * num2 >= num37 * num37 + num38 * num38)
					{
						num19 = 1;
					}
					else
					{
						num37 = num1;
						num38 = num2;
					}
				}
			}
			if (num19 == 1)
			{
				double num39 = (num16 + num15 - num17) / (2 * Math.Sqrt(num16) * Math.Sqrt(num15));
				if (num39 >= 0)
				{
					flag = (Math.Abs(num39 - 0) > 1E-50 ? false : true);
				}
				else
				{
					flag = true;
				}
				num25 = this.DoSmoothing(otri1, torg, tdest, tapex, ref numArray3);
				if (num25 <= 0)
				{
					double num40 = Math.Sqrt(num15) / (2 * Math.Sin(this.behavior.MinAngle * 3.14159265358979 / 180));
					double num41 = (point1.x + point2.x) / 2;
					double num42 = (point1.y + point2.y) / 2;
					double num43 = num41 + Math.Sqrt(num40 * num40 - num15 / 4) * (point1.y - point2.y) / Math.Sqrt(num15);
					double num44 = num42 + Math.Sqrt(num40 * num40 - num15 / 4) * (point2.x - point1.x) / Math.Sqrt(num15);
					double num45 = num41 - Math.Sqrt(num40 * num40 - num15 / 4) * (point1.y - point2.y) / Math.Sqrt(num15);
					double num46 = num42 - Math.Sqrt(num40 * num40 - num15 / 4) * (point2.x - point1.x) / Math.Sqrt(num15);
					if ((num43 - point.x) * (num43 - point.x) + (num44 - point.y) * (num44 - point.y) > (num45 - point.x) * (num45 - point.x) + (num46 - point.y) * (num46 - point.y))
					{
						num3 = num45;
						num4 = num46;
					}
					else
					{
						num3 = num43;
						num4 = num44;
					}
					double num47 = num37;
					double num48 = num38;
					if (!this.GetNeighborsVertex(badotri, point1.x, point1.y, point.x, point.y, ref numArray, ref otri))
					{
						Vertex vertex2 = otri.Org();
						vertex = otri.Dest();
						vertex1 = otri.Apex();
						point3 = Primitives.FindCircumcenter(vertex2, vertex, vertex1, ref num20, ref num21);
						num5 = point1.y - point.y;
						num6 = point.x - point1.x;
						num5 = point4.x + num5;
						num6 = point4.y + num6;
						this.CircleLineIntersection(point4.x, point4.y, num5, num6, num3, num4, num40, ref numArray1);
						if (!this.ChooseCorrectPoint((point1.x + point.x) / 2, (point1.y + point.y) / 2, numArray1[3], numArray1[4], point4.x, point4.y, flag))
						{
							num7 = numArray1[1];
							num8 = numArray1[2];
						}
						else
						{
							num7 = numArray1[3];
							num8 = numArray1[4];
						}
						this.PointBetweenPoints(num7, num8, point4.x, point4.y, point3.x, point3.y, ref numArray2);
						if (numArray1[0] > 0)
						{
							if (Math.Abs(numArray2[0] - 1) <= 1E-50)
							{
								if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, point3.x, point3.y))
								{
									num47 = numArray2[2] - torg.x;
									num48 = numArray2[3] - torg.y;
								}
								else
								{
									num47 = num37;
									num48 = num38;
								}
							}
							else if (!this.IsBadTriangleAngle(point2.x, point2.y, point1.x, point1.y, num7, num8))
							{
								num47 = num7 - torg.x;
								num48 = num8 - torg.y;
							}
							else
							{
								num11 = Math.Sqrt((num7 - point4.x) * (num7 - point4.x) + (num8 - point4.y) * (num8 - point4.y));
								num9 = point4.x - num7;
								num10 = point4.y - num8;
								num9 = num9 / num11;
								num10 = num10 / num11;
								num7 = num7 + num9 * num22 * Math.Sqrt(num15);
								num8 = num8 + num10 * num22 * Math.Sqrt(num15);
								if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num7, num8))
								{
									num47 = num7 - torg.x;
									num48 = num8 - torg.y;
								}
								else
								{
									num47 = num37;
									num48 = num38;
								}
							}
							if ((point.x - point4.x) * (point.x - point4.x) + (point.y - point4.y) * (point.y - point4.y) > num23 * ((point.x - (num47 + torg.x)) * (point.x - (num47 + torg.x)) + (point.y - (num48 + torg.y)) * (point.y - (num48 + torg.y))))
							{
								num47 = num37;
								num48 = num38;
							}
						}
					}
					double num49 = num37;
					double num50 = num38;
					if (!this.GetNeighborsVertex(badotri, point2.x, point2.y, point.x, point.y, ref numArray, ref otri))
					{
						Vertex vertex3 = otri.Org();
						vertex = otri.Dest();
						vertex1 = otri.Apex();
						point3 = Primitives.FindCircumcenter(vertex3, vertex, vertex1, ref num20, ref num21);
						num5 = point2.y - point.y;
						num6 = point.x - point2.x;
						num5 = point4.x + num5;
						num6 = point4.y + num6;
						this.CircleLineIntersection(point4.x, point4.y, num5, num6, num3, num4, num40, ref numArray1);
						if (!this.ChooseCorrectPoint((point2.x + point.x) / 2, (point2.y + point.y) / 2, numArray1[3], numArray1[4], point4.x, point4.y, false))
						{
							num7 = numArray1[1];
							num8 = numArray1[2];
						}
						else
						{
							num7 = numArray1[3];
							num8 = numArray1[4];
						}
						this.PointBetweenPoints(num7, num8, point4.x, point4.y, point3.x, point3.y, ref numArray2);
						if (numArray1[0] > 0)
						{
							if (Math.Abs(numArray2[0] - 1) <= 1E-50)
							{
								if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, point3.x, point3.y))
								{
									num49 = numArray2[2] - torg.x;
									num50 = numArray2[3] - torg.y;
								}
								else
								{
									num49 = num37;
									num50 = num38;
								}
							}
							else if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num7, num8))
							{
								num49 = num7 - torg.x;
								num50 = num8 - torg.y;
							}
							else
							{
								num11 = Math.Sqrt((num7 - point4.x) * (num7 - point4.x) + (num8 - point4.y) * (num8 - point4.y));
								num9 = point4.x - num7;
								num10 = point4.y - num8;
								num9 = num9 / num11;
								num10 = num10 / num11;
								num7 = num7 + num9 * num22 * Math.Sqrt(num15);
								num8 = num8 + num10 * num22 * Math.Sqrt(num15);
								if (!this.IsBadTriangleAngle(point1.x, point1.y, point2.x, point2.y, num7, num8))
								{
									num49 = num7 - torg.x;
									num50 = num8 - torg.y;
								}
								else
								{
									num49 = num37;
									num50 = num38;
								}
							}
							if ((point.x - point4.x) * (point.x - point4.x) + (point.y - point4.y) * (point.y - point4.y) > num23 * ((point.x - (num49 + torg.x)) * (point.x - (num49 + torg.x)) + (point.y - (num50 + torg.y)) * (point.y - (num50 + torg.y))))
							{
								num49 = num37;
								num50 = num38;
							}
						}
					}
					if (flag)
					{
						num37 = num47;
						num38 = num48;
					}
					else if (num24 * ((point.x - (num49 + torg.x)) * (point.x - (num49 + torg.x)) + (point.y - (num50 + torg.y)) * (point.y - (num50 + torg.y))) <= (point.x - (num47 + torg.x)) * (point.x - (num47 + torg.x)) + (point.y - (num48 + torg.y)) * (point.y - (num48 + torg.y)))
					{
						num37 = num47;
						num38 = num48;
					}
					else
					{
						num37 = num49;
						num38 = num50;
					}
				}
				else
				{
					Statistic.RelocationCount = Statistic.RelocationCount + (long)1;
					num37 = numArray3[0] - torg.x;
					num38 = numArray3[1] - torg.y;
					num26 = torg.x;
					num27 = torg.y;
					switch (num25)
					{
						case 1:
						{
							this.mesh.DeleteVertex(ref otri1);
							break;
						}
						case 2:
						{
							otri1.LnextSelf();
							this.mesh.DeleteVertex(ref otri1);
							break;
						}
						case 3:
						{
							otri1.LprevSelf();
							this.mesh.DeleteVertex(ref otri1);
							break;
						}
					}
				}
			}
			Point point5 = new Point();
			if (num25 > 0)
			{
				point5.x = num26 + num37;
				point5.y = num27 + num38;
			}
			else
			{
				point5.x = torg.x + num37;
				point5.y = torg.y + num38;
			}
			xi = (num31 * num37 - num30 * num38) * (2 * num);
			eta = (num28 * num38 - num29 * num37) * (2 * num);
			return point5;
		Label0:
			num13 = num28;
			num14 = num29;
			num15 = num34;
			num16 = num36;
			num17 = num35;
			point = tapex;
			point1 = torg;
			point2 = tdest;
			goto Label3;
		Label1:
			if (num18 != 321)
			{
				goto Label0;
			}
			else
			{
				goto Label0;
			}
		}

		private void FindPolyCentroid(int numpoints, double[] points, ref double[] centroid)
		{
			centroid[0] = 0;
			centroid[1] = 0;
			for (int i = 0; i < 2 * numpoints; i = i + 2)
			{
				centroid[0] = centroid[0] + points[i];
				centroid[1] = centroid[1] + points[i + 1];
			}
			centroid[0] = centroid[0] / (double)numpoints;
			centroid[1] = centroid[1] / (double)numpoints;
		}

		private bool GetNeighborsVertex(Otri badotri, double first_x, double first_y, double second_x, double second_y, ref double[] thirdpoint, ref Otri neighotri)
		{
			Otri otri = new Otri();
			bool flag = false;
			Vertex vertex = null;
			Vertex vertex1 = null;
			Vertex vertex2 = null;
			int num = 0;
			int num1 = 0;
			badotri.orient = 0;
			while (badotri.orient < 3)
			{
				badotri.Sym(ref otri);
				if (otri.triangle != Mesh.dummytri)
				{
					vertex = otri.Org();
					vertex1 = otri.Dest();
					vertex2 = otri.Apex();
					if ((vertex.x != vertex1.x || vertex.y != vertex1.y) && (vertex1.x != vertex2.x || vertex1.y != vertex2.y) && (vertex.x != vertex2.x || vertex.y != vertex2.y))
					{
						num = 0;
						if (Math.Abs(first_x - vertex.x) < 1E-50 && Math.Abs(first_y - vertex.y) < 1E-50)
						{
							num = 11;
						}
						else if (Math.Abs(first_x - vertex1.x) < 1E-50 && Math.Abs(first_y - vertex1.y) < 1E-50)
						{
							num = 12;
						}
						else if (Math.Abs(first_x - vertex2.x) < 1E-50 && Math.Abs(first_y - vertex2.y) < 1E-50)
						{
							num = 13;
						}
						num1 = 0;
						if (Math.Abs(second_x - vertex.x) < 1E-50 && Math.Abs(second_y - vertex.y) < 1E-50)
						{
							num1 = 21;
						}
						else if (Math.Abs(second_x - vertex1.x) < 1E-50 && Math.Abs(second_y - vertex1.y) < 1E-50)
						{
							num1 = 22;
						}
						else if (Math.Abs(second_x - vertex2.x) < 1E-50 && Math.Abs(second_y - vertex2.y) < 1E-50)
						{
							num1 = 23;
						}
					}
				}
				if (num == 11 && (num1 == 22 || num1 == 23) || num == 12 && (num1 == 21 || num1 == 23) || num == 13 && (num1 == 21 || num1 == 22))
				{
					break;
				}
				badotri.orient = badotri.orient + 1;
			}
			if (num == 0)
			{
				flag = true;
			}
			else
			{
				switch (num)
				{
					case 11:
					{
						if (num1 == 22)
						{
							thirdpoint[0] = vertex2.x;
							thirdpoint[1] = vertex2.y;
							break;
						}
						else if (num1 != 23)
						{
							flag = true;
							break;
						}
						else
						{
							thirdpoint[0] = vertex1.x;
							thirdpoint[1] = vertex1.y;
							break;
						}
					}
					case 12:
					{
						if (num1 == 21)
						{
							thirdpoint[0] = vertex2.x;
							thirdpoint[1] = vertex2.y;
							break;
						}
						else if (num1 != 23)
						{
							flag = true;
							break;
						}
						else
						{
							thirdpoint[0] = vertex.x;
							thirdpoint[1] = vertex.y;
							break;
						}
					}
					case 13:
					{
						if (num1 == 21)
						{
							thirdpoint[0] = vertex1.x;
							thirdpoint[1] = vertex1.y;
							break;
						}
						else if (num1 != 22)
						{
							flag = true;
							break;
						}
						else
						{
							thirdpoint[0] = vertex.x;
							thirdpoint[1] = vertex.y;
							break;
						}
					}
					default:
					{
						if (num1 != 0)
						{
							break;
						}
						flag = true;
						break;
					}
				}
			}
			neighotri = otri;
			return flag;
		}

		private int GetStarPoints(Otri badotri, Vertex p, Vertex q, Vertex r, int whichPoint, ref double[] points)
		{
			Otri otri = new Otri();
			double num = 0;
			double num1 = 0;
			double num2 = 0;
			double num3 = 0;
			double num4 = 0;
			double num5 = 0;
			double[] numArray = new double[2];
			int num6 = 0;
			switch (whichPoint)
			{
				case 1:
				{
					num = p.x;
					num1 = p.y;
					num2 = r.x;
					num3 = r.y;
					num4 = q.x;
					num5 = q.y;
					break;
				}
				case 2:
				{
					num = q.x;
					num1 = q.y;
					num2 = p.x;
					num3 = p.y;
					num4 = r.x;
					num5 = r.y;
					break;
				}
				case 3:
				{
					num = r.x;
					num1 = r.y;
					num2 = q.x;
					num3 = q.y;
					num4 = p.x;
					num5 = p.y;
					break;
				}
			}
			Otri otri1 = badotri;
			points[num6] = num2;
			num6++;
			points[num6] = num3;
			num6++;
			numArray[0] = num2;
			numArray[1] = num3;
			do
			{
				if (this.GetNeighborsVertex(otri1, num, num1, num2, num3, ref numArray, ref otri))
				{
					num6 = 0;
					break;
				}
				else
				{
					otri1 = otri;
					num2 = numArray[0];
					num3 = numArray[1];
					points[num6] = numArray[0];
					num6++;
					points[num6] = numArray[1];
					num6++;
				}
			}
			while (Math.Abs(numArray[0] - num4) > 1E-50 || Math.Abs(numArray[1] - num5) > 1E-50);
			return num6 / 2;
		}

		private bool GetWedgeIntersection(int numpoints, double[] points, ref double[] newloc)
		{
			int i;
			int j;
			double num;
			double num1;
			double[] numArray = new double[2 * numpoints];
			double[] numArray1 = new double[2 * numpoints];
			double[] numArray2 = new double[2 * numpoints];
			double[] numArray3 = new double[2000];
			double[] numArray4 = new double[3];
			double[] numArray5 = new double[3];
			double[] numArray6 = new double[3];
			double[] numArray7 = new double[3];
			double[] numArray8 = new double[500];
			int num2 = 0;
			int num3 = 0;
			double num4 = 4;
			double num5 = 4;
			double num6 = points[2 * numpoints - 4];
			double num7 = points[2 * numpoints - 3];
			double num8 = points[2 * numpoints - 2];
			double num9 = points[2 * numpoints - 1];
			double minAngle = this.behavior.MinAngle * 3.14159265358979 / 180;
			double num10 = Math.Sin(minAngle);
			double num11 = Math.Cos(minAngle);
			double maxAngle = this.behavior.MaxAngle * 3.14159265358979 / 180;
			double num12 = Math.Sin(maxAngle);
			double num13 = Math.Cos(maxAngle);
			if (this.behavior.goodAngle != 1)
			{
				num = 0.5 / Math.Tan(minAngle);
				num1 = 0.5 / Math.Sin(minAngle);
			}
			else
			{
				num = 0;
				num1 = 0;
			}
			for (i = 0; i < numpoints * 2; i = i + 2)
			{
				double num14 = points[i];
				double num15 = points[i + 1];
				double num16 = num8 - num6;
				double num17 = num9 - num7;
				double num18 = Math.Sqrt(num16 * num16 + num17 * num17);
				numArray[i / 2] = num6 + 0.5 * num16 - num * num17;
				numArray1[i / 2] = num7 + 0.5 * num17 + num * num16;
				numArray2[i / 2] = num1 * num18;
				numArray[numpoints + i / 2] = numArray[i / 2];
				numArray1[numpoints + i / 2] = numArray1[i / 2];
				numArray2[numpoints + i / 2] = numArray2[i / 2];
				double num19 = (num6 + num8) / 2;
				double num20 = (num7 + num9) / 2;
				double num21 = Math.Sqrt((numArray[i / 2] - num19) * (numArray[i / 2] - num19) + (numArray1[i / 2] - num20) * (numArray1[i / 2] - num20));
				double num22 = (numArray[i / 2] - num19) / num21;
				double num23 = (numArray1[i / 2] - num20) / num21;
				double num24 = numArray[i / 2] + num22 * numArray2[i / 2];
				double num25 = numArray1[i / 2] + num23 * numArray2[i / 2];
				num22 = num8 - num6;
				num23 = num9 - num7;
				double num26 = num8 * num11 - num9 * num10 + num6 - num6 * num11 + num7 * num10;
				double num27 = num8 * num10 + num9 * num11 + num7 - num6 * num10 - num7 * num11;
				numArray3[i * 20] = num6;
				numArray3[i * 20 + 1] = num7;
				numArray3[i * 20 + 2] = num26;
				numArray3[i * 20 + 3] = num27;
				num22 = num6 - num8;
				num23 = num7 - num9;
				double num28 = num6 * num11 + num7 * num10 + num8 - num8 * num11 - num9 * num10;
				double num29 = -num6 * num10 + num7 * num11 + num9 + num8 * num10 - num9 * num11;
				numArray3[i * 20 + 4] = num28;
				numArray3[i * 20 + 5] = num29;
				numArray3[i * 20 + 6] = num8;
				numArray3[i * 20 + 7] = num9;
				num22 = num24 - numArray[i / 2];
				num23 = num25 - numArray1[i / 2];
				double num30 = num24;
				double num31 = num25;
				minAngle = 2 * this.behavior.MaxAngle + this.behavior.MinAngle - 180;
				if (minAngle <= 0)
				{
					num3 = 4;
					num4 = 1;
					num5 = 1;
				}
				else if (minAngle <= 5)
				{
					num3 = 6;
					num4 = 2;
					num5 = 2;
				}
				else if (minAngle > 10)
				{
					num3 = 10;
					num4 = 4;
					num5 = 4;
				}
				else
				{
					num3 = 8;
					num4 = 3;
					num5 = 3;
				}
				minAngle = minAngle * 3.14159265358979 / 180;
				for (j = 1; (double)j < num4; j++)
				{
					if (num4 != 1)
					{
						double num32 = num24 * Math.Cos(minAngle / (num4 - 1) * (double)j) + num25 * Math.Sin(minAngle / (num4 - 1) * (double)j) + numArray[i / 2] - numArray[i / 2] * Math.Cos(minAngle / (num4 - 1) * (double)j) - numArray1[i / 2] * Math.Sin(minAngle / (num4 - 1) * (double)j);
						double num33 = -num24 * Math.Sin(minAngle / (num4 - 1) * (double)j) + num25 * Math.Cos(minAngle / (num4 - 1) * (double)j) + numArray1[i / 2] + numArray[i / 2] * Math.Sin(minAngle / (num4 - 1) * (double)j) - numArray1[i / 2] * Math.Cos(minAngle / (num4 - 1) * (double)j);
						numArray3[i * 20 + 8 + 4 * (j - 1)] = num32;
						numArray3[i * 20 + 9 + 4 * (j - 1)] = num33;
						numArray3[i * 20 + 10 + 4 * (j - 1)] = num30;
						numArray3[i * 20 + 11 + 4 * (j - 1)] = num31;
						num30 = num32;
						num31 = num33;
					}
				}
				num22 = num6 - num8;
				num23 = num7 - num9;
				double num34 = num6 * num13 + num7 * num12 + num8 - num8 * num13 - num9 * num12;
				double num35 = -num6 * num12 + num7 * num13 + num9 + num8 * num12 - num9 * num13;
				numArray3[i * 20 + 20] = num8;
				numArray3[i * 20 + 21] = num9;
				numArray3[i * 20 + 22] = num34;
				numArray3[i * 20 + 23] = num35;
				num30 = num24;
				num31 = num25;
				for (j = 1; (double)j < num5; j++)
				{
					if (num5 != 1)
					{
						double num36 = num24 * Math.Cos(minAngle / (num5 - 1) * (double)j) - num25 * Math.Sin(minAngle / (num5 - 1) * (double)j) + numArray[i / 2] - numArray[i / 2] * Math.Cos(minAngle / (num5 - 1) * (double)j) + numArray1[i / 2] * Math.Sin(minAngle / (num5 - 1) * (double)j);
						double num37 = num24 * Math.Sin(minAngle / (num5 - 1) * (double)j) + num25 * Math.Cos(minAngle / (num5 - 1) * (double)j) + numArray1[i / 2] - numArray[i / 2] * Math.Sin(minAngle / (num5 - 1) * (double)j) - numArray1[i / 2] * Math.Cos(minAngle / (num5 - 1) * (double)j);
						numArray3[i * 20 + 24 + 4 * (j - 1)] = num30;
						numArray3[i * 20 + 25 + 4 * (j - 1)] = num31;
						numArray3[i * 20 + 26 + 4 * (j - 1)] = num36;
						numArray3[i * 20 + 27 + 4 * (j - 1)] = num37;
						num30 = num36;
						num31 = num37;
					}
				}
				num22 = num8 - num6;
				num23 = num9 - num7;
				double num38 = num8 * num13 - num9 * num12 + num6 - num6 * num13 + num7 * num12;
				double num39 = num8 * num12 + num9 * num13 + num7 - num6 * num12 - num7 * num13;
				numArray3[i * 20 + 36] = num38;
				numArray3[i * 20 + 37] = num39;
				numArray3[i * 20 + 38] = num6;
				numArray3[i * 20 + 39] = num7;
				if (i == 0)
				{
					switch (num3)
					{
						case 4:
						{
							this.LineLineIntersection(num6, num7, num26, num27, num8, num9, num28, num29, ref numArray4);
							this.LineLineIntersection(num6, num7, num26, num27, num8, num9, num34, num35, ref numArray5);
							this.LineLineIntersection(num6, num7, num38, num39, num8, num9, num34, num35, ref numArray6);
							this.LineLineIntersection(num6, num7, num38, num39, num8, num9, num28, num29, ref numArray7);
							if (numArray4[0] != 1 || numArray5[0] != 1 || numArray6[0] != 1 || numArray7[0] != 1)
							{
								break;
							}
							numArray8[0] = numArray4[1];
							numArray8[1] = numArray4[2];
							numArray8[2] = numArray5[1];
							numArray8[3] = numArray5[2];
							numArray8[4] = numArray6[1];
							numArray8[5] = numArray6[2];
							numArray8[6] = numArray7[1];
							numArray8[7] = numArray7[2];
							break;
						}
						case 6:
						{
							this.LineLineIntersection(num6, num7, num26, num27, num8, num9, num28, num29, ref numArray4);
							this.LineLineIntersection(num6, num7, num26, num27, num8, num9, num34, num35, ref numArray5);
							this.LineLineIntersection(num6, num7, num38, num39, num8, num9, num28, num29, ref numArray6);
							if (numArray4[0] != 1 || numArray5[0] != 1 || numArray6[0] != 1)
							{
								break;
							}
							numArray8[0] = numArray4[1];
							numArray8[1] = numArray4[2];
							numArray8[2] = numArray5[1];
							numArray8[3] = numArray5[2];
							numArray8[4] = numArray3[i * 20 + 8];
							numArray8[5] = numArray3[i * 20 + 9];
							numArray8[6] = num24;
							numArray8[7] = num25;
							numArray8[8] = numArray3[i * 20 + 26];
							numArray8[9] = numArray3[i * 20 + 27];
							numArray8[10] = numArray6[1];
							numArray8[11] = numArray6[2];
							break;
						}
						case 8:
						{
							this.LineLineIntersection(num6, num7, num26, num27, num8, num9, num28, num29, ref numArray4);
							this.LineLineIntersection(num6, num7, num26, num27, num8, num9, num34, num35, ref numArray5);
							this.LineLineIntersection(num6, num7, num38, num39, num8, num9, num28, num29, ref numArray6);
							if (numArray4[0] != 1 || numArray5[0] != 1 || numArray6[0] != 1)
							{
								break;
							}
							numArray8[0] = numArray4[1];
							numArray8[1] = numArray4[2];
							numArray8[2] = numArray5[1];
							numArray8[3] = numArray5[2];
							numArray8[4] = numArray3[i * 20 + 12];
							numArray8[5] = numArray3[i * 20 + 13];
							numArray8[6] = numArray3[i * 20 + 8];
							numArray8[7] = numArray3[i * 20 + 9];
							numArray8[8] = num24;
							numArray8[9] = num25;
							numArray8[10] = numArray3[i * 20 + 26];
							numArray8[11] = numArray3[i * 20 + 27];
							numArray8[12] = numArray3[i * 20 + 30];
							numArray8[13] = numArray3[i * 20 + 31];
							numArray8[14] = numArray6[1];
							numArray8[15] = numArray6[2];
							break;
						}
						case 10:
						{
							this.LineLineIntersection(num6, num7, num26, num27, num8, num9, num28, num29, ref numArray4);
							this.LineLineIntersection(num6, num7, num26, num27, num8, num9, num34, num35, ref numArray5);
							this.LineLineIntersection(num6, num7, num38, num39, num8, num9, num28, num29, ref numArray6);
							if (numArray4[0] != 1 || numArray5[0] != 1 || numArray6[0] != 1)
							{
								break;
							}
							numArray8[0] = numArray4[1];
							numArray8[1] = numArray4[2];
							numArray8[2] = numArray5[1];
							numArray8[3] = numArray5[2];
							numArray8[4] = numArray3[i * 20 + 16];
							numArray8[5] = numArray3[i * 20 + 17];
							numArray8[6] = numArray3[i * 20 + 12];
							numArray8[7] = numArray3[i * 20 + 13];
							numArray8[8] = numArray3[i * 20 + 8];
							numArray8[9] = numArray3[i * 20 + 9];
							numArray8[10] = num24;
							numArray8[11] = num25;
							numArray8[12] = numArray3[i * 20 + 28];
							numArray8[13] = numArray3[i * 20 + 29];
							numArray8[14] = numArray3[i * 20 + 32];
							numArray8[15] = numArray3[i * 20 + 33];
							numArray8[16] = numArray3[i * 20 + 34];
							numArray8[17] = numArray3[i * 20 + 35];
							numArray8[18] = numArray6[1];
							numArray8[19] = numArray6[2];
							break;
						}
					}
				}
				num6 = num8;
				num7 = num9;
				num8 = num14;
				num9 = num15;
			}
			if (numpoints != 0)
			{
				int num40 = (numpoints - 1) / 2 + 1;
				int num41 = 0;
				int k = 0;
				i = 1;
				int num42 = num3;
				for (j = 0; j < 40; j = j + 4)
				{
					if ((num3 != 4 || j != 8 && j != 12 && j != 16 && j != 24 && j != 28 && j != 32) && (num3 != 6 || j != 12 && j != 16 && j != 28 && j != 32) && (num3 != 8 || j != 16 && j != 32))
					{
						num2 = this.HalfPlaneIntersection(num42, ref numArray8, numArray3[40 * num40 + j], numArray3[40 * num40 + 1 + j], numArray3[40 * num40 + 2 + j], numArray3[40 * num40 + 3 + j]);
						if (num2 == 0)
						{
							return false;
						}
						num42 = num2;
					}
				}
				for (k++; k < numpoints - 1; k++)
				{
					for (j = 0; j < 40; j = j + 4)
					{
						if ((num3 != 4 || j != 8 && j != 12 && j != 16 && j != 24 && j != 28 && j != 32) && (num3 != 6 || j != 12 && j != 16 && j != 28 && j != 32) && (num3 != 8 || j != 16 && j != 32))
						{
							num2 = this.HalfPlaneIntersection(num42, ref numArray8, numArray3[40 * (i + num40 * num41) + j], numArray3[40 * (i + num40 * num41) + 1 + j], numArray3[40 * (i + num40 * num41) + 2 + j], numArray3[40 * (i + num40 * num41) + 3 + j]);
							if (num2 == 0)
							{
								return false;
							}
							num42 = num2;
						}
					}
					i = i + num41;
					num41 = (num41 + 1) % 2;
				}
				this.FindPolyCentroid(num2, numArray8, ref newloc);
				if (this.behavior.MaxAngle == 0)
				{
					return true;
				}
				int num43 = 0;
				for (j = 0; j < numpoints * 2 - 2; j = j + 2)
				{
					if (this.IsBadTriangleAngle(newloc[0], newloc[1], points[j], points[j + 1], points[j + 2], points[j + 3]))
					{
						num43++;
					}
				}
				if (this.IsBadTriangleAngle(newloc[0], newloc[1], points[0], points[1], points[numpoints * 2 - 2], points[numpoints * 2 - 1]))
				{
					num43++;
				}
				if (num43 == 0)
				{
					return true;
				}
				int num44 = (numpoints <= 2 ? 20 : 30);
				for (int l = 0; l < 2 * numpoints; l = l + 2)
				{
					for (int m = 1; m < num44; m++)
					{
						newloc[0] = 0;
						newloc[1] = 0;
						for (i = 0; i < 2 * numpoints; i = i + 2)
						{
							double num45 = 1 / (double)numpoints;
							if (i != l)
							{
								num45 = (1 - 0.1 * (double)m * num45) / (double)((double)numpoints - 1);
								newloc[0] = newloc[0] + num45 * points[i];
								newloc[1] = newloc[1] + num45 * points[i + 1];
							}
							else
							{
								newloc[0] = newloc[0] + 0.1 * (double)m * num45 * points[i];
								newloc[1] = newloc[1] + 0.1 * (double)m * num45 * points[i + 1];
							}
						}
						num43 = 0;
						for (j = 0; j < numpoints * 2 - 2; j = j + 2)
						{
							if (this.IsBadTriangleAngle(newloc[0], newloc[1], points[j], points[j + 1], points[j + 2], points[j + 3]))
							{
								num43++;
							}
						}
						if (this.IsBadTriangleAngle(newloc[0], newloc[1], points[0], points[1], points[numpoints * 2 - 2], points[numpoints * 2 - 1]))
						{
							num43++;
						}
						if (num43 == 0)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private bool GetWedgeIntersectionWithoutMaxAngle(int numpoints, double[] points, ref double[] newloc)
		{
			int i;
			int j;
			double num;
			double num1;
			double[] numArray = new double[2 * numpoints];
			double[] numArray1 = new double[2 * numpoints];
			double[] numArray2 = new double[2 * numpoints];
			double[] numArray3 = new double[2000];
			double[] numArray4 = new double[3];
			double[] numArray5 = new double[500];
			int num2 = 0;
			double num3 = points[2 * numpoints - 4];
			double num4 = points[2 * numpoints - 3];
			double num5 = points[2 * numpoints - 2];
			double num6 = points[2 * numpoints - 1];
			double minAngle = this.behavior.MinAngle * 3.14159265358979 / 180;
			if (this.behavior.goodAngle != 1)
			{
				num = 0.5 / Math.Tan(minAngle);
				num1 = 0.5 / Math.Sin(minAngle);
			}
			else
			{
				num = 0;
				num1 = 0;
			}
			for (i = 0; i < numpoints * 2; i = i + 2)
			{
				double num7 = points[i];
				double num8 = points[i + 1];
				double num9 = num5 - num3;
				double num10 = num6 - num4;
				double num11 = Math.Sqrt(num9 * num9 + num10 * num10);
				numArray[i / 2] = num3 + 0.5 * num9 - num * num10;
				numArray1[i / 2] = num4 + 0.5 * num10 + num * num9;
				numArray2[i / 2] = num1 * num11;
				numArray[numpoints + i / 2] = numArray[i / 2];
				numArray1[numpoints + i / 2] = numArray1[i / 2];
				numArray2[numpoints + i / 2] = numArray2[i / 2];
				double num12 = (num3 + num5) / 2;
				double num13 = (num4 + num6) / 2;
				double num14 = Math.Sqrt((numArray[i / 2] - num12) * (numArray[i / 2] - num12) + (numArray1[i / 2] - num13) * (numArray1[i / 2] - num13));
				double num15 = (numArray[i / 2] - num12) / num14;
				double num16 = (numArray1[i / 2] - num13) / num14;
				double num17 = numArray[i / 2] + num15 * numArray2[i / 2];
				double num18 = numArray1[i / 2] + num16 * numArray2[i / 2];
				num15 = num5 - num3;
				num16 = num6 - num4;
				double num19 = num5 * Math.Cos(minAngle) - num6 * Math.Sin(minAngle) + num3 - num3 * Math.Cos(minAngle) + num4 * Math.Sin(minAngle);
				double num20 = num5 * Math.Sin(minAngle) + num6 * Math.Cos(minAngle) + num4 - num3 * Math.Sin(minAngle) - num4 * Math.Cos(minAngle);
				numArray3[i * 16] = num3;
				numArray3[i * 16 + 1] = num4;
				numArray3[i * 16 + 2] = num19;
				numArray3[i * 16 + 3] = num20;
				num15 = num3 - num5;
				num16 = num4 - num6;
				double num21 = num3 * Math.Cos(minAngle) + num4 * Math.Sin(minAngle) + num5 - num5 * Math.Cos(minAngle) - num6 * Math.Sin(minAngle);
				double num22 = -num3 * Math.Sin(minAngle) + num4 * Math.Cos(minAngle) + num6 + num5 * Math.Sin(minAngle) - num6 * Math.Cos(minAngle);
				numArray3[i * 16 + 4] = num21;
				numArray3[i * 16 + 5] = num22;
				numArray3[i * 16 + 6] = num5;
				numArray3[i * 16 + 7] = num6;
				num15 = num17 - numArray[i / 2];
				num16 = num18 - numArray1[i / 2];
				double num23 = num17;
				double num24 = num18;
				for (j = 1; j < 4; j++)
				{
					double num25 = num17 * Math.Cos((1.0471975511966 - minAngle) * (double)j) + num18 * Math.Sin((1.0471975511966 - minAngle) * (double)j) + numArray[i / 2] - numArray[i / 2] * Math.Cos((1.0471975511966 - minAngle) * (double)j) - numArray1[i / 2] * Math.Sin((1.0471975511966 - minAngle) * (double)j);
					double num26 = -num17 * Math.Sin((1.0471975511966 - minAngle) * (double)j) + num18 * Math.Cos((1.0471975511966 - minAngle) * (double)j) + numArray1[i / 2] + numArray[i / 2] * Math.Sin((1.0471975511966 - minAngle) * (double)j) - numArray1[i / 2] * Math.Cos((1.0471975511966 - minAngle) * (double)j);
					numArray3[i * 16 + 8 + 4 * (j - 1)] = num25;
					numArray3[i * 16 + 9 + 4 * (j - 1)] = num26;
					numArray3[i * 16 + 10 + 4 * (j - 1)] = num23;
					numArray3[i * 16 + 11 + 4 * (j - 1)] = num24;
					num23 = num25;
					num24 = num26;
				}
				num23 = num17;
				num24 = num18;
				for (j = 1; j < 4; j++)
				{
					double num27 = num17 * Math.Cos((1.0471975511966 - minAngle) * (double)j) - num18 * Math.Sin((1.0471975511966 - minAngle) * (double)j) + numArray[i / 2] - numArray[i / 2] * Math.Cos((1.0471975511966 - minAngle) * (double)j) + numArray1[i / 2] * Math.Sin((1.0471975511966 - minAngle) * (double)j);
					double num28 = num17 * Math.Sin((1.0471975511966 - minAngle) * (double)j) + num18 * Math.Cos((1.0471975511966 - minAngle) * (double)j) + numArray1[i / 2] - numArray[i / 2] * Math.Sin((1.0471975511966 - minAngle) * (double)j) - numArray1[i / 2] * Math.Cos((1.0471975511966 - minAngle) * (double)j);
					numArray3[i * 16 + 20 + 4 * (j - 1)] = num23;
					numArray3[i * 16 + 21 + 4 * (j - 1)] = num24;
					numArray3[i * 16 + 22 + 4 * (j - 1)] = num27;
					numArray3[i * 16 + 23 + 4 * (j - 1)] = num28;
					num23 = num27;
					num24 = num28;
				}
				if (i == 0)
				{
					this.LineLineIntersection(num3, num4, num19, num20, num5, num6, num21, num22, ref numArray4);
					if (numArray4[0] == 1)
					{
						numArray5[0] = numArray4[1];
						numArray5[1] = numArray4[2];
						numArray5[2] = numArray3[i * 16 + 16];
						numArray5[3] = numArray3[i * 16 + 17];
						numArray5[4] = numArray3[i * 16 + 12];
						numArray5[5] = numArray3[i * 16 + 13];
						numArray5[6] = numArray3[i * 16 + 8];
						numArray5[7] = numArray3[i * 16 + 9];
						numArray5[8] = num17;
						numArray5[9] = num18;
						numArray5[10] = numArray3[i * 16 + 22];
						numArray5[11] = numArray3[i * 16 + 23];
						numArray5[12] = numArray3[i * 16 + 26];
						numArray5[13] = numArray3[i * 16 + 27];
						numArray5[14] = numArray3[i * 16 + 30];
						numArray5[15] = numArray3[i * 16 + 31];
					}
				}
				num3 = num5;
				num4 = num6;
				num5 = num7;
				num6 = num8;
			}
			if (numpoints != 0)
			{
				int num29 = (numpoints - 1) / 2 + 1;
				int num30 = 0;
				int k = 0;
				i = 1;
				int num31 = 8;
				for (j = 0; j < 32; j = j + 4)
				{
					num2 = this.HalfPlaneIntersection(num31, ref numArray5, numArray3[32 * num29 + j], numArray3[32 * num29 + 1 + j], numArray3[32 * num29 + 2 + j], numArray3[32 * num29 + 3 + j]);
					if (num2 == 0)
					{
						return false;
					}
					num31 = num2;
				}
				for (k++; k < numpoints - 1; k++)
				{
					for (j = 0; j < 32; j = j + 4)
					{
						num2 = this.HalfPlaneIntersection(num31, ref numArray5, numArray3[32 * (i + num29 * num30) + j], numArray3[32 * (i + num29 * num30) + 1 + j], numArray3[32 * (i + num29 * num30) + 2 + j], numArray3[32 * (i + num29 * num30) + 3 + j]);
						if (num2 == 0)
						{
							return false;
						}
						num31 = num2;
					}
					i = i + num30;
					num30 = (num30 + 1) % 2;
				}
				this.FindPolyCentroid(num2, numArray5, ref newloc);
				if (!this.behavior.fixedArea)
				{
					return true;
				}
			}
			return false;
		}

		private int HalfPlaneIntersection(int numvertices, ref double[] convexPoly, double x1, double y1, double x2, double y2)
		{
			double num;
			double[][] numArray = new double[][] { new double[2], new double[2], new double[2] };
			double[] numArray1 = null;
			int num1 = 0;
			int num2 = 0;
			double num3 = x2 - x1;
			double num4 = y2 - y1;
			int num5 = this.SplitConvexPolygon(numvertices, convexPoly, x1, y1, x2, y2, ref numArray);
			if (num5 != 3)
			{
				int num6 = 0;
				while (num6 < num5)
				{
					double num7 = 1E+17;
					double num8 = -1E+17;
					for (int i = 1; (double)i <= 2 * numArray[num6][0] - 1; i = i + 2)
					{
						num = num3 * (numArray[num6][i + 1] - y1) - num4 * (numArray[num6][i] - x1);
						num7 = (num < num7 ? num : num7);
						num8 = (num > num8 ? num : num8);
					}
					num = (Math.Abs(num7) > Math.Abs(num8) ? num7 : num8);
					if (num <= 0)
					{
						num6++;
					}
					else
					{
						numArray1 = numArray[num6];
						num2 = 1;
						break;
					}
				}
				if (num2 == 1)
				{
					while ((double)num1 < numArray1[0])
					{
						convexPoly[2 * num1] = numArray1[2 * num1 + 1];
						convexPoly[2 * num1 + 1] = numArray1[2 * num1 + 2];
						num1++;
					}
				}
			}
			else
			{
				num1 = numvertices;
			}
			return num1;
		}

		private bool IsBadPolygonAngle(double x1, double y1, double x2, double y2, double x3, double y3)
		{
			double num = x1 - x2;
			double num1 = y1 - y2;
			double num2 = x2 - x3;
			double num3 = y2 - y3;
			double num4 = x3 - x1;
			double num5 = y3 - y1;
			double num6 = num * num + num1 * num1;
			double num7 = num2 * num2 + num3 * num3;
			if (Math.Acos((num6 + num7 - (num4 * num4 + num5 * num5)) / (2 * Math.Sqrt(num6) * Math.Sqrt(num7))) < 2 * Math.Acos(Math.Sqrt(this.behavior.goodAngle)))
			{
				return true;
			}
			return false;
		}

		private bool IsBadTriangleAngle(double x1, double y1, double x2, double y2, double x3, double y3)
		{
			double num;
			double num1;
			double num2 = x1 - x2;
			double num3 = y1 - y2;
			double num4 = x2 - x3;
			double num5 = y2 - y3;
			double num6 = x3 - x1;
			double num7 = y3 - y1;
			double num8 = num2 * num2;
			double num9 = num3 * num3;
			double num10 = num4 * num4;
			double num11 = num5 * num5;
			double num12 = num7 * num7;
			double num13 = num8 + num9;
			double num14 = num10 + num11;
			double num15 = num6 * num6 + num12;
			if (num13 < num14 && num13 < num15)
			{
				num = num4 * num6 + num5 * num7;
				num = num * num / (num14 * num15);
			}
			else if (num14 >= num15)
			{
				num = num2 * num4 + num3 * num5;
				num = num * num / (num13 * num14);
			}
			else
			{
				num = num2 * num6 + num3 * num7;
				num = num * num / (num13 * num15);
			}
			if (num13 <= num14 || num13 <= num15)
			{
				num1 = (num14 <= num15 ? (num13 + num14 - num15) / (2 * Math.Sqrt(num13 * num14)) : (num13 + num15 - num14) / (2 * Math.Sqrt(num13 * num15)));
			}
			else
			{
				num1 = (num14 + num15 - num13) / (2 * Math.Sqrt(num14 * num15));
			}
			if (num <= this.behavior.goodAngle && (this.behavior.MaxAngle == 0 || num1 >= this.behavior.maxGoodAngle))
			{
				return false;
			}
			return true;
		}

		private void LineLineIntersection(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, ref double[] p)
		{
			double num = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
			double num1 = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
			double num2 = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);
			if (Math.Abs(num - 0) < 1E-50 && Math.Abs(num2 - 0) < 1E-50 && Math.Abs(num1 - 0) < 1E-50)
			{
				p[0] = 0;
				return;
			}
			if (Math.Abs(num - 0) < 1E-50)
			{
				p[0] = 0;
				return;
			}
			p[0] = 1;
			num1 = num1 / num;
			num2 = num2 / num;
			p[1] = x1 + num1 * (x2 - x1);
			p[2] = y1 + num1 * (y2 - y1);
		}

		private void LineLineSegmentIntersection(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, ref double[] p)
		{
			double num = 1E-13;
			double num1 = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
			double num2 = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
			double num3 = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);
			if (Math.Abs(num1 - 0) < num)
			{
				if (Math.Abs(num3 - 0) < num && Math.Abs(num2 - 0) < num)
				{
					p[0] = 2;
					return;
				}
				p[0] = 0;
				return;
			}
			num3 = num3 / num1;
			num2 = num2 / num1;
			if (num3 < -num || num3 > 1 + num)
			{
				p[0] = 0;
				return;
			}
			p[0] = 1;
			p[1] = x1 + num2 * (x2 - x1);
			p[2] = y1 + num2 * (y2 - y1);
		}

		private int LinePointLocation(double x1, double y1, double x2, double y2, double x, double y)
		{
			if (Math.Atan((y2 - y1) / (x2 - x1)) * 180 / 3.14159265358979 == 90)
			{
				if (Math.Abs(x1 - x) <= 1E-11)
				{
					return 0;
				}
			}
			else if (Math.Abs(y1 + (y2 - y1) * (x - x1) / (x2 - x1) - y) <= 1E-50)
			{
				return 0;
			}
			double num = (x2 - x1) * (y - y1) - (y2 - y1) * (x - x1);
			if (Math.Abs(num - 0) <= 1E-11)
			{
				return 0;
			}
			if (num > 0)
			{
				return 1;
			}
			return 2;
		}

		private int LongestShortestEdge(double aodist, double dadist, double dodist)
		{
			int num = 0;
			int num1 = 0;
			int num2 = 0;
			if (dodist < aodist && dodist < dadist)
			{
				num1 = 3;
				if (aodist >= dadist)
				{
					num = 1;
					num2 = 2;
				}
				else
				{
					num = 2;
					num2 = 1;
				}
			}
			else if (aodist >= dadist)
			{
				num1 = 2;
				if (aodist >= dodist)
				{
					num = 1;
					num2 = 3;
				}
				else
				{
					num = 3;
					num2 = 1;
				}
			}
			else
			{
				num1 = 1;
				if (dodist >= dadist)
				{
					num = 3;
					num2 = 2;
				}
				else
				{
					num = 2;
					num2 = 3;
				}
			}
			return num1 * 100 + num2 * 10 + num;
		}

		private double MinDistanceToNeighbor(double newlocX, double newlocY, ref Otri searchtri)
		{
			Otri otri = new Otri();
			LocateResult locateResult = LocateResult.Outside;
			Point point = new Point(newlocX, newlocY);
			Vertex vertex = searchtri.Org();
			Vertex vertex1 = searchtri.Dest();
			if (vertex.x == point.x && vertex.y == point.y)
			{
				locateResult = LocateResult.OnVertex;
				searchtri.Copy(ref otri);
			}
			else if (vertex1.x != point.x || vertex1.y != point.y)
			{
				double num = Primitives.CounterClockwise(vertex, vertex1, point);
				if (num < 0)
				{
					searchtri.SymSelf();
					searchtri.Copy(ref otri);
					locateResult = this.mesh.locator.PreciseLocate(point, ref otri, false);
				}
				else if (num != 0)
				{
					searchtri.Copy(ref otri);
					locateResult = this.mesh.locator.PreciseLocate(point, ref otri, false);
				}
				else if (vertex.x < point.x == point.x < vertex1.x && vertex.y < point.y == point.y < vertex1.y)
				{
					locateResult = LocateResult.OnEdge;
					searchtri.Copy(ref otri);
				}
			}
			else
			{
				searchtri.LnextSelf();
				locateResult = LocateResult.OnVertex;
				searchtri.Copy(ref otri);
			}
			if (locateResult == LocateResult.OnVertex || locateResult == LocateResult.Outside)
			{
				return 0;
			}
			Vertex vertex2 = otri.Org();
			Vertex vertex3 = otri.Dest();
			Vertex vertex4 = otri.Apex();
			double num1 = (vertex2.x - point.x) * (vertex2.x - point.x) + (vertex2.y - point.y) * (vertex2.y - point.y);
			double num2 = (vertex3.x - point.x) * (vertex3.x - point.x) + (vertex3.y - point.y) * (vertex3.y - point.y);
			double num3 = (vertex4.x - point.x) * (vertex4.x - point.x) + (vertex4.y - point.y) * (vertex4.y - point.y);
			if (num1 <= num2 && num1 <= num3)
			{
				return num1;
			}
			if (num2 <= num3)
			{
				return num2;
			}
			return num3;
		}

		private void PointBetweenPoints(double x1, double y1, double x2, double y2, double x, double y, ref double[] p)
		{
			if ((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y) >= (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1))
			{
				p[0] = 0;
				p[1] = 0;
				p[2] = 0;
				p[3] = 0;
				return;
			}
			p[0] = 1;
			p[1] = (x - x2) * (x - x2) + (y - y2) * (y - y2);
			p[2] = x;
			p[3] = y;
		}

		private int SplitConvexPolygon(int numvertices, double[] convexPoly, double x1, double y1, double x2, double y2, ref double[][] polys)
		{
			int num;
			int num1 = 0;
			double[] numArray = new double[3];
			double[] numArray1 = new double[100];
			int num2 = 0;
			double[] numArray2 = new double[100];
			int num3 = 0;
			double num4 = 1E-12;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			int num9 = 0;
			int num10 = 0;
			int num11 = 0;
			int num12 = 0;
			for (int i = 0; i < 2 * numvertices; i = i + 2)
			{
				int num13 = (i + 2 >= 2 * numvertices ? 0 : i + 2);
				this.LineLineSegmentIntersection(x1, y1, x2, y2, convexPoly[i], convexPoly[i + 1], convexPoly[num13], convexPoly[num13 + 1], ref numArray);
				if (Math.Abs(numArray[0] - 0) <= num4)
				{
					if (num1 != 1)
					{
						num2++;
						numArray1[2 * num2 - 1] = convexPoly[num13];
						numArray1[2 * num2] = convexPoly[num13 + 1];
					}
					else
					{
						num3++;
						numArray2[2 * num3 - 1] = convexPoly[num13];
						numArray2[2 * num3] = convexPoly[num13 + 1];
					}
					num5++;
				}
				else if (Math.Abs(numArray[0] - 2) > num4)
				{
					num7++;
					if (Math.Abs(numArray[1] - convexPoly[num13]) <= num4 && Math.Abs(numArray[2] - convexPoly[num13 + 1]) <= num4)
					{
						num8++;
						if (num1 == 1)
						{
							num3++;
							numArray2[2 * num3 - 1] = convexPoly[num13];
							numArray2[2 * num3] = convexPoly[num13 + 1];
							num2++;
							numArray1[2 * num2 - 1] = convexPoly[num13];
							numArray1[2 * num2] = convexPoly[num13 + 1];
							num1++;
						}
						else if (num1 == 0)
						{
							num11++;
							num2++;
							numArray1[2 * num2 - 1] = convexPoly[num13];
							numArray1[2 * num2] = convexPoly[num13 + 1];
							if (i + 4 < 2 * numvertices)
							{
								int num14 = this.LinePointLocation(x1, y1, x2, y2, convexPoly[i], convexPoly[i + 1]);
								int num15 = this.LinePointLocation(x1, y1, x2, y2, convexPoly[i + 4], convexPoly[i + 5]);
								if (num14 != num15 && num14 != 0 && num15 != 0)
								{
									num12++;
									num3++;
									numArray2[2 * num3 - 1] = convexPoly[num13];
									numArray2[2 * num3] = convexPoly[num13 + 1];
									num1++;
								}
							}
						}
					}
					else if (Math.Abs(numArray[1] - convexPoly[i]) > num4 || Math.Abs(numArray[2] - convexPoly[i + 1]) > num4)
					{
						num9++;
						num2++;
						numArray1[2 * num2 - 1] = numArray[1];
						numArray1[2 * num2] = numArray[2];
						num3++;
						numArray2[2 * num3 - 1] = numArray[1];
						numArray2[2 * num3] = numArray[2];
						if (num1 == 1)
						{
							num2++;
							numArray1[2 * num2 - 1] = convexPoly[num13];
							numArray1[2 * num2] = convexPoly[num13 + 1];
						}
						else if (num1 == 0)
						{
							num3++;
							numArray2[2 * num3 - 1] = convexPoly[num13];
							numArray2[2 * num3] = convexPoly[num13 + 1];
						}
						num1++;
					}
					else
					{
						num10++;
						if (num1 != 1)
						{
							num2++;
							numArray1[2 * num2 - 1] = convexPoly[num13];
							numArray1[2 * num2] = convexPoly[num13 + 1];
						}
						else
						{
							num3++;
							numArray2[2 * num3 - 1] = convexPoly[num13];
							numArray2[2 * num3] = convexPoly[num13 + 1];
						}
					}
				}
				else
				{
					num2++;
					numArray1[2 * num2 - 1] = convexPoly[num13];
					numArray1[2 * num2] = convexPoly[num13 + 1];
					num6++;
				}
			}
			if (num1 == 0 || num1 == 2)
			{
				num = (num1 == 0 ? 1 : 2);
				numArray1[0] = (double)num2;
				numArray2[0] = (double)num3;
				polys[0] = numArray1;
				if (num1 == 2)
				{
					polys[1] = numArray2;
				}
			}
			else
			{
				num = 3;
			}
			return num;
		}

		private bool ValidPolygonAngles(int numpoints, double[] points)
		{
			for (int i = 0; i < numpoints; i++)
			{
				if (i == numpoints - 1)
				{
					if (this.IsBadPolygonAngle(points[i * 2], points[i * 2 + 1], points[0], points[1], points[2], points[3]))
					{
						return false;
					}
				}
				else if (i == numpoints - 2)
				{
					if (this.IsBadPolygonAngle(points[i * 2], points[i * 2 + 1], points[(i + 1) * 2], points[(i + 1) * 2 + 1], points[0], points[1]))
					{
						return false;
					}
				}
				else if (this.IsBadPolygonAngle(points[i * 2], points[i * 2 + 1], points[(i + 1) * 2], points[(i + 1) * 2 + 1], points[(i + 2) * 2], points[(i + 2) * 2 + 1]))
				{
					return false;
				}
			}
			return true;
		}
	}
}