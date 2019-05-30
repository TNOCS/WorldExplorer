using System;
using System.Collections.Generic;
using System.Text;
using WorldExplorerClient.interfaces;

namespace WorldExplorerClient
{
    public class Logger
    {
        public class Log : IWorldClientLogging
        {
            public Log(string pMessage)
            {
                Message = pMessage;
            }
            public string Message { get; private set; }
        }
        private Action<Log> mCallback = null;

        public void SetCallback(Action<Log> pCallback)
        {
            mCallback = pCallback;
        }

        public void LogMessage(string pMessage)
        {
            mCallback?.Invoke(new Log(pMessage));
        }
    }
}
