using System;
using System.Collections.Generic;

namespace TriangleNet.Log
{
	public sealed class SimpleLog : ILog<SimpleLogItem>
	{
		private List<SimpleLogItem> log = new List<SimpleLogItem>();

		private TriangleNet.Log.LogLevel level;

		private readonly static SimpleLog instance;

		public IList<SimpleLogItem> Data
		{
			get
			{
				return this.log;
			}
		}

		public static ILog<SimpleLogItem> Instance
		{
			get
			{
				return SimpleLog.instance;
			}
		}

		public TriangleNet.Log.LogLevel Level
		{
			get
			{
				return this.level;
			}
		}

		static SimpleLog()
		{
			SimpleLog.instance = new SimpleLog();
		}

		private SimpleLog()
		{
		}

		public void Add(SimpleLogItem item)
		{
			this.log.Add(item);
		}

		public void Clear()
		{
			this.log.Clear();
		}

		public void Error(string message, string location)
		{
			this.log.Add(new SimpleLogItem(TriangleNet.Log.LogLevel.Error, message, location));
		}

		public void Info(string message)
		{
			this.log.Add(new SimpleLogItem(TriangleNet.Log.LogLevel.Info, message));
		}

		public void Warning(string message, string location)
		{
			this.log.Add(new SimpleLogItem(TriangleNet.Log.LogLevel.Warning, message, location));
		}
	}
}