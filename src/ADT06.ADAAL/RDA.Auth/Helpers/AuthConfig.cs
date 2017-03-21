using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.MicrosoftAccount;
using Microsoft.Owin.Security.WsFederation;
using Owin;
using RDA.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RDA.Auth.Helpers
{

        [ComVisible(true)]
        public  class AuthConfig
        {
            public const string LocalADAuthenticationType = "Local AD";

            public static string AzureADGroupId { get; private set; }
            public static bool LocalAuthEnabled { get; private set; }
            public static string LocalADGroup { get; private set; }
            public static string LocalADEmailDomain { get; private set; }

            public static string GenericLoginFailureMessage { get; private set; }

            private static string LocalADCurrentDomain { get; set; }
            public static string GetCurrentLocalADDomain()
            {
                ADValidation auth = new ADValidation();
                if (LocalADCurrentDomain == null)
                {
                    string theDnsHostNameRootDSE = auth.RetrieveDnsHostNameRootDseDefaultNamingContext();
                    if (theDnsHostNameRootDSE != null)
                    {
                        string[] subStrings = theDnsHostNameRootDSE.Split('|');
                        if (subStrings.Length > 0)
                            LocalADCurrentDomain = subStrings[0];
                    }
                }

                return LocalADCurrentDomain ?? string.Empty;
            }

            public static void ConfigureAuth(IAppBuilder app)
            {
                var config = Config;
                if (string.IsNullOrWhiteSpace(config.GenericAuthFailureMessage))
                {
                    throw new ConfigurationErrorsException("Web configuration key \'authGenericAuthFailureMessage\' is missing or corrupted.");
                }

                LocalAuthEnabled = config.EnableLocalAuth;
                GenericLoginFailureMessage = config.GenericAuthFailureMessage;

                if (config.EnableOpenAuth)
                {
                 
                    if (config.EnableLocalAD)
                    {
                        if (string.IsNullOrWhiteSpace(config.LocalADGroup))
                        {
                            throw new ConfigurationErrorsException("Web configuration key \'authLocalADGroup\' is missing or corrupted.");
                        }
                        if (string.IsNullOrWhiteSpace(config.LocalADEmailDomain))
                        {
                            throw new ConfigurationErrorsException("Web configuration key \'authLocalADEmailDomain\' is missing or corrupted.");
                        }

                        LocalADGroup = config.LocalADGroup;
                        LocalADEmailDomain = config.LocalADEmailDomain;

                        app.UseLocalADAuthentication(new LocalADAuthenticationOptions
                        {
                            AuthenticationType = LocalADAuthenticationType,
                            AuthenticationMode = AuthenticationMode.Passive
                        });
                    }

                    if (config.EnableAzureAD)
                    {
                        if (string.IsNullOrWhiteSpace(config.AzureGroupId))
                        {
                            throw new ConfigurationErrorsException("Web configuration key \'authAzureGroupId\' is missing or corrupted.");
                        }

                        AzureADGroupId = config.AzureGroupId;

                        // Azure AD cookie
                        app.UseWsFederationAuthentication(
                            new WsFederationAuthenticationOptions
                            {
                                AuthenticationType = "Azure AD",
                                AuthenticationMode = AuthenticationMode.Passive,
                                MetadataAddress = string.Format(
                                    "https://login.windows.net/{0}/federationmetadata/2007-06/federationmetadata.xml",
                                    config.AzureAppTenant),
                                Wtrealm = config.WebAppURL
                            });

                        // client (JavaScript) token
                        app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                            new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                            {
                                AuthenticationMode = AuthenticationMode.Passive,
#pragma warning disable 618
                            Audience = config.AzureAppClientID,
#pragma warning restore 618
                            Tenant = config.AzureAppTenant
                            });
                    }

                    if (config.EnableMS)
                    {
                        app.UseMicrosoftAccountAuthentication(new MicrosoftAccountAuthenticationOptions
                        {
                            AuthenticationMode = AuthenticationMode.Passive,
                            AuthenticationType = "Microsoft",
                            ClientId = config.MSAppClientID,
                            ClientSecret = config.MSAppClientSecret,
                        });
                    }

                    if (config.EnableGoogle)
                    {
                        app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
                        {
                            AuthenticationMode = AuthenticationMode.Passive,
                            AuthenticationType = "Google",
                            ClientId = config.GoogleAppClientID,
                            ClientSecret = config.GoogleAppClientSecret,
                        });
                    }

                    //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
                    //app.UseWebApi(GlobalConfiguration.Configuration);
                }
            }

            private static dynamic Config
            {
                get
                {
                    dynamic conf = new ExpandoObject();
                    foreach (var key in ConfigurationManager.AppSettings.AllKeys.Where(
                        k => k.StartsWith("auth")).ToList())
                    {
                        var val = ConfigurationManager.AppSettings[key];
                        var prop = key.Remove(0, "auth".Length);
                        ((IDictionary<string, object>)conf)[prop] = val.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                            || val.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase)
                                ? (object)Boolean.Parse(val) : val;
                    }

                    return conf;
                }
            }
        }
    }

