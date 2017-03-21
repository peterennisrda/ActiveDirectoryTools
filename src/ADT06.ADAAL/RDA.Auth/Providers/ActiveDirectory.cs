
using System;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using RDA.Auth.Helpers;

namespace RDA.Auth
{
    [ComVisible(true)]
    
    public class ADValidation
    {
      

        #region DnsHostNameRootDse
        // Ref: http://stackoverflow.com/questions/4015407/determine-current-domain-controller-programmatically
        public string RetrieveDnsHostNameRootDseDefaultNamingContext()
        {
            string dnsHostName = "";
            String RootDsePath = "LDAP://RootDSE";
            const string DefaultNamingContextPropertyName = "defaultNamingContext";

            DirectoryEntry rootDse = new DirectoryEntry(RootDsePath)
            {
                AuthenticationType = AuthenticationTypes.Secure
            };

            dnsHostName = (string)rootDse.Properties["dnsHostName"].Value;
            //Console.WriteLine("dnsHostName = " + dnsHostName);

            dnsHostName = dnsHostName + "|";
            object combinedPropertyValue = dnsHostName + rootDse.Properties[DefaultNamingContextPropertyName].Value;

            return combinedPropertyValue != null ? combinedPropertyValue.ToString() : null;
        }
        #endregion

        #region 1. Is User Authenticated
        /// <summary>
        /// This is the easier way to validate credentials using .NET objects
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="domainName">Domain Name</param>
        /// <param name="password">Password</param>
        /// <param name="optionalDCName">Name of the specific DC if one is desired</param>
        /// <param name="contextOptions">Type of authentication and data transfer; default set to Sealed since that is encrypted but doesn't require certificate setup... pass SecureSocketLayer to use TLS instead</param>
        /// <returns>Credential validate success; doesn't trap errors so exceptions could be thrown</returns>
        //public static bool ValidateCredentials(string userName, string domainName, string password, string optionalDCName = null, ContextOptions contextOptions = ContextOptions.Sealing)

        public static bool ValidateCredentials(string userName, string domainName, string password, string optionalDCName = null, ContextOptions contextOptions = ContextOptions.SecureSocketLayer)
        {
            //This isn't catching exceptions
            bool dcSpecified = string.IsNullOrEmpty(optionalDCName);
            if (dcSpecified) contextOptions |= ContextOptions.ServerBind;
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, dcSpecified ? domainName : optionalDCName))
                {
                    //Logging.Info(typeof(ADValidation), pc.ConnectedServer);
                    Console.WriteLine(pc.ConnectedServer);
                    var up = UserPrincipal.FindByIdentity(pc, userName);
                    if (up != null) Console.WriteLine(up.UserPrincipalName);

                    return pc.ValidateCredentials(userName, password, contextOptions);
                }
            }
            catch ( Exception ex)
            {
                return false;
            }

        }
        
        private static bool ManuallyValidateTLSCredentials(string username, string domainname, string password, string dc, int port)
        {
            try
            {
                using (LdapConnection ldap = new LdapConnection(new LdapDirectoryIdentifier(dc, port)))
                {
                    ldap.SessionOptions.Sealing = false; //turns off kerberos encryption
                    ldap.SessionOptions.SecureSocketLayer = true; //turns on ssl
                    ldap.SessionOptions.ProtocolVersion = 3;
                    ldap.Bind(new NetworkCredential(username, password, domainname));
                }
                return true;
            }
            catch (LdapException l)
            {
                //Logging.Error(typeof(ADValidation), l);
                if (l.ErrorCode == 0x31) //error logon failure
                {
                    return false;
                }
                throw;
            }
        }

        private static bool TryConnect(string hostname, int port)
        {
            try
            {
                using (TcpClient t = new TcpClient(hostname, port))
                {
                    t.LingerState.Enabled = false;
                }
                return true;
            }
            catch (Exception ex)
            {
                //Logging.Error("Couldn't connect to {0}:{1}:\t{2}", hostname, port, ex);
                Console.WriteLine("Couldn't connect to {0}:{1}:\t{2}", hostname, port, ex);
            }
            return false;
        }

        /// <summary>
        /// Validates credentials using TLS
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="domainname">Domain name</param>
        /// <param name="password">User Password</param>
        /// <param name="serverNameUsed">Name of the server used; if return value is false and this is null, no server that could use TLS was found for the domain</param>
        /// <returns>Success in validating the credentials</returns>
        public static bool ValidateCredentialsTLS(string username, string domainname, string password, out string serverNameUsed)
        {
            //Reference port numbers = https://technet.microsoft.com/en-us/library/dd772723(v=ws.10).aspx
            int LdapSSLPort = 636;
            int LdapGcSSLPort = 3269;
            foreach (var dc in NativeWrapped.EnumerateDCs(domainname, DsFlag.DS_ONLY_LDAP_NEEDED))
            {
                if (TryConnect(dc, LdapSSLPort))
                {
                    serverNameUsed = dc;
                    try
                    {
                        return ValidateCredentials(username, domainname, password, dc, ContextOptions.SecureSocketLayer | ContextOptions.SimpleBind);
                    }
                    catch (Exception ex)
                    {
                        //Logging.Error(typeof(ADValidation), "Failed validating credentials for {0} on {1}:\t{2}", username, dc, ex);
                        Console.WriteLine("Failed validating credentials for {0} on {1}:\t{2}", username, dc, ex);

                        try
                        {
                            return ManuallyValidateTLSCredentials(username, domainname, password, dc, LdapSSLPort);
                        }
                        catch (Exception ex2)
                        {
                            //Logging.Error(typeof(ADValidation), "Failed manually validating credentials for {0} on {1}:\t{2}", username, dc, ex2);
                            Console.WriteLine("Failed manually validating credentials for {0} on {1}:\t{2}", username, dc, ex2);
                        }
                    }
                }
                else if (TryConnect(dc, LdapGcSSLPort))
                {
                    serverNameUsed = dc;
                    //You could roll your own validator using LDAPConnection for this if you wanted and as I have done for a fallback on
                    //the above where the ldap options set for the session within the .NET library can cause credential validation to fail,
                    //but the ValidateCredentials method is hard coded to the other port
                    try
                    {
                        return ManuallyValidateTLSCredentials(username, domainname, password, dc, LdapGcSSLPort);
                    }
                    catch (Exception ex2)
                    {
                        //Logging.Error(typeof(ADValidation), "Failed manually validating credentials for {0} on {1}:\t{2}", username, dc, ex2);
                        Console.WriteLine("Failed manually validating credentials for {0} on {1}:\t{2}", username, dc, ex2);
                    }
                }
            }
            serverNameUsed = null;
            return false;
        }

        /// <summary>
        /// Version of IsUserValidated that uses ValidateCredentials Method
        /// </summary>
        /// <returns></returns>
        public static bool IsUserValidated(string theUserName, string theUserDomainName, string theUserPassword)
        {
            try
            {
                string serverName;
                bool result = ValidateCredentialsTLS(theUserName, theUserDomainName, theUserPassword, out serverName);
               
              //  //Logging.Info(typeof(ADValidation), "theUserDomainName: {0}", theUserDomainName);
              //  //Logging.Info(typeof(ADValidation), "theServerName: {0}", serverName);
              //  //Logging.Info(typeof(ADValidation), "User Is validated: {0}", result);

                Console.WriteLine("************************User*************************");
                Console.WriteLine("theUserName = " + theUserName);
                Console.WriteLine("theUserDomainName = " + theUserDomainName);
                Console.WriteLine("theServerName = " + serverName);
                Console.WriteLine("IsUserValidated() = " + result);

                if (!result)
                    result = ValidateCredentials(theUserName, theUserDomainName, theUserPassword, null, ContextOptions.Negotiate);
                
              //  //Logging.Info(typeof(ADValidation), new AuthResultModel(AuthResultModel.AuthAction.Logon) { Succeeded = result, UserName = theUserName });
                Console.WriteLine("IsUserValidated() [Kerberos] = " + result.ToString());

                return result;
            }
            catch (Exception ex)
            {
             //   //Logging.Error(typeof(ADValidation), GetErrors(ex));
                Console.WriteLine(ex);
                bool result = ValidateCredentials(theUserName, theUserDomainName, theUserPassword);
             //   //Logging.Info(typeof(ADValidation), new AuthResultModel(AuthResultModel.AuthAction.Logon) { Succeeded = result, UserName = theUserName });
                Console.WriteLine("IsUserValidated() = " + result.ToString());
            }
            return false;
        }
        #endregion

        #region 2. Is User Authorized
        /// <summary>
        /// Test if the user is a member of the selected group
        /// </summary>
        public static bool IsUserInGroup(string userName, string domainName, string userGroup)
        {
            bool validation;

            try
            {
                validation = false;

                // Set up domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, userGroup);
                // If found...
                if (group != null)
                {
                    // Iterate over members
                    foreach (Principal p in group.GetMembers())
                    {
                        // Do what is required for the group members
                        if (p.SamAccountName == userName)
                        {
                            Console.WriteLine("{0}: {1}, {2}", p.StructuralObjectClass, p.DisplayName, p.SamAccountName);
                            validation = true;
                            break;
                        }
                        else
                        {
                            validation = false;
                        }
                    }
                    return validation;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred...");
                return false;
            }
        }
        private AuthResultModel GetFailedLogonResult(string userName, Exception ex)
        {
            List<string> errors = GetErrors(ex);
            var ret = new AuthResultModel(AuthResultModel.AuthAction.Logon)
            {
                UserName = userName,
                Succeeded = false,
                Reasons = errors.ToArray()
            };
            return ret;
        }

        private static List<string> GetErrors(Exception ex)
        {
            var errors = new List<string>();// (useModelState ? GetErrorsFromModelState().ToList() : new List<string>());
            if (ex != null)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    errors.Add(ex.Message);
                    if (!string.IsNullOrEmpty(ex.StackTrace))
                    {
                        errors.Add(ex.StackTrace);
                    }
                }
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errors.Add(ex.InnerException.Message);
                    if (!string.IsNullOrEmpty(ex.InnerException.StackTrace))
                    {
                        errors.Add(ex.InnerException.StackTrace);
                    }
                }
            }

            return errors;
        }
        #endregion

        #region TestCOM
        public string Login(string userName, string password, string localAD)
        {
            return "success";
        }
        #endregion
    }
}