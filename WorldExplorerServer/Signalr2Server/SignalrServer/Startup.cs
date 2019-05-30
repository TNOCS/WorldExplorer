using System.Diagnostics;
using System.Web.Cors;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Tracing;
using Microsoft.Owin.Cors;
using Owin;

namespace SignalrServer
{
    public class Startup
    {
        // https://docs.microsoft.com/en-us/aspnet/signalr/overview/deployment/tutorial-signalr-self-host
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();


            // Turn tracing on programmatically
            GlobalHost.TraceManager.Switch.Level = SourceLevels.Information;
        }
    }
}
