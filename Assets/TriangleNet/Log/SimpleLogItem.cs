using System;

namespace TriangleNet.Log
{
	public class SimpleLogItem : ILogItem
	{
		private DateTime time;

		private TriangleNet.Log.LogLevel level;

		private string message;

		private string info;

		public string Info
		{
			get
			{
				return this.info;
			}
		}

		public TriangleNet.Log.LogLevel Level
		{
			get
			{
				return this.level;
			}
		}

		public string Message
		{
			get
			{
				return this.message;
			}
		}

		public DateTime Time
		{
			get
			{
				return this.time;
			}
		}

		public SimpleLogItem(TriangleNet.Log.LogLevel level, string message) : this(level, message, "")
		{
		}

		public SimpleLogItem(TriangleNet.Log.LogLevel level, string message, string info)
		{
			this.time = DateTime.Now;
			this.level = level;
			this.message = message;
			this.info = info;
		}
	}
}