using Newtonsoft.Json;
using RDA.Auth.Helpers;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RDA.Auth.Providers
{
    [ComVisible(true)]
    public class AuthorizeCOM
    {
        public string Domain
            {
                get { return AuthConfig.GetCurrentLocalADDomain(); }
            }
        public string User
        {
            get
            {
              //  using (PrincipalContext pc = new PrincipalContext(ContextType.Domain))
               // {
                    return UserPrincipal.Current.UserPrincipalName;
                //}
            }
        }
        
        [ComVisible(true)]
        public  bool Authenticate(string userName, string userPassword,string domainName)
        {
            try
            {
                string serverName;
                //SSL
                bool result = Validate(userName, userPassword, domainName, out serverName);

                if (!result)
                    //Principal Context
                    result = Validate(userName, userPassword, domainName, null, ContextOptions.Negotiate);

                //  //Logging.Info(typeof(ADValidation), new AuthResultModel(AuthResultModel.AuthAction.Logon) { Succeeded = result, UserName = theUserName });
                Console.WriteLine("IsUserValidated() [Kerberos] = " + result.ToString());

                return result;
            }
            catch (Exception ex)
            {
                //   //Logging.Error(typeof(ADValidation), GetErrors(ex));
                Console.WriteLine(ex);
                bool result = Validate(userName, userPassword, domainName);
                //   //Logging.Info(typeof(ADValidation), new AuthResultModel(AuthResultModel.AuthAction.Logon) { Succeeded = result, UserName = theUserName });
                Console.WriteLine("IsUserValidated() = " + result.ToString());
            }
            return false;
        }

        [ComVisible(true)]
        public bool Authorize(string userName, string domainName, string userGroup)
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

        
        
            

        
        //Principal Context
        private bool Validate(string userName, string password, string domainName, string optionalDCName = null, ContextOptions contextOptions = ContextOptions.SecureSocketLayer)
        {
            bool dcSpecified = string.IsNullOrEmpty(optionalDCName);
            if (dcSpecified) contextOptions |= ContextOptions.ServerBind;
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, dcSpecified ? domainName : optionalDCName))
                {

                    //Logging.Info(typeof(ADValidation), pc.ConnectedServer);
                    //    Console.WriteLine(pc.ConnectedServer);
                    var up = UserPrincipal.FindByIdentity(pc, userName);
                    if (up != null) Console.WriteLine(up.UserPrincipalName);

                    if (pc.ValidateCredentials(userName, password, contextOptions))
                        return true;// string.Format("User {0} is authenticated", up.UserPrincipalName);
                    else
                        return false;// string.Format("User {0} is not authenticated", userName);
                }
            }
            catch (Exception ex)
            {
                throw;// return GetErrors(ex);
            }
        }
       
        //SSL         
        private bool Validate(string userName, string password, string domainName , out string serverNameUsed)
        {
            //Reference port numbers = https://technet.microsoft.com/en-us/library/dd772723(v=ws.10).aspx
            int LdapSSLPort = 636;
            int LdapGcSSLPort = 3269;
            foreach (var dc in NativeWrapped.EnumerateDCs(domainName, DsFlag.DS_ONLY_LDAP_NEEDED))
            {
                if (TryConnect(dc, LdapSSLPort))
                {
                    serverNameUsed = dc;
                    try
                    {
                        return Validate(userName, password, domainName, dc, ContextOptions.SecureSocketLayer | ContextOptions.SimpleBind);
                    }
                    catch (Exception ex)
                    {
                        //Logging.Error(typeof(ADValidation), "Failed validating credentials for {0} on {1}:\t{2}", username, dc, ex);
                        Console.WriteLine("Failed validating credentials for {0} on {1}:\t{2}", userName, dc, ex);

                        try
                        {
                            return Validate(userName, password, domainName, dc, LdapSSLPort);
                        }
                        catch (Exception ex2)
                        {
                            //Logging.Error(typeof(ADValidation), "Failed manually validating credentials for {0} on {1}:\t{2}", username, dc, ex2);
                            Console.WriteLine("Failed manually validating credentials for {0} on {1}:\t{2}", userName, dc, ex2);
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
                        //LDap
                        return Validate(userName,  password, domainName, dc, LdapGcSSLPort);
                    }
                    catch (Exception ex2)
                    {
                        //Logging.Error(typeof(ADValidation), "Failed manually validating credentials for {0} on {1}:\t{2}", username, dc, ex2);
                        Console.WriteLine("Failed manually validating credentials for {0} on {1}:\t{2}", userName, dc, ex2);
                    }
                }
            }
            serverNameUsed = null;
            return false;// string.Format("User {0} is not authenticated", userName);
        }

        //LDAP
        private bool Validate(string username, string password,string domainname, string dc, int port)
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
                    return false;//GetErrors(l);// "false";
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
        
        private static string GetErrors(Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                Message = ex.Message,
                Type = ex.GetType().FullName,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException == null ? null
                        : new
                        {
                            Message = ex.InnerException.Message,
                            Type = ex.InnerException.GetType().FullName
                        }
            });
        }
    }
}
