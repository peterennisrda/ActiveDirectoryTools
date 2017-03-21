using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RDA.Auth.Helpers
{
    public class LocalADAuthenticationOptions : AuthenticationOptions
    {
        private const string CAPTION = "LocalAD";

        public LocalADAuthenticationOptions() : this(string.Empty) { }
        public LocalADAuthenticationOptions(string authenticationType)
            : base(authenticationType)
        {
            Description.Properties.Add("Caption", CAPTION);
            Description.Caption = CAPTION;
        }
    }

    [ClassInterface(ClassInterfaceType.None)]
    public class LocalADAuthenticationMiddleware : AuthenticationMiddleware<LocalADAuthenticationOptions>
    {
        public LocalADAuthenticationMiddleware(OwinMiddleware next, LocalADAuthenticationOptions options)
            : base(next, options)
        {

        }

        protected override AuthenticationHandler<LocalADAuthenticationOptions> CreateHandler()
        {
            return new LocalADAuthenticationHandler();
        }
    }

    //COM Interop - 
    [ClassInterface(ClassInterfaceType.None)]
    public class LocalADAuthenticationHandler : AuthenticationHandler<LocalADAuthenticationOptions>
    {
        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            // for now, we only have interface for our LocalAD middleware,
            // but no true functionality as an Owin custom middleware;
            // TODO: make LocalADAuthenticationMiddleware a full-fledged Owin custom middleware
            return Task.FromResult<AuthenticationTicket>(null);
        }
    }

    public static class LocalADAuthenticationExtensions
    {
        public static IAppBuilder UseLocalADAuthentication(this IAppBuilder app, LocalADAuthenticationOptions options)
        {
            app.Use<LocalADAuthenticationMiddleware>(options);
            return app;
        }
    }
}
