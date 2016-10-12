using System;

namespace TriangleNet.Log
{
	public interface ILogItem
	{
		string Info
		{
			get;
		}

		TriangleNet.Log.LogLevel Level
		{
			get;
		}

		string Message
		{
			get;
		}

		DateTime Time
		{
			get;
		}
	}
}