using RDA.Auth.Helpers;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RDA.Auth.Providers
{
    [ComVisible(true)]
    public class ADCOM
    {
        public string RootDseDefaultNamingContext()
        {
            const string ROOTDSE = "LDAP://RootDSE";
            const string DNSHOSTNAME = "dnsHostName";
            const string DEFAULTNAMINGCONTEXT = "defaultNamingContext";
            
            DirectoryEntry rootDse = new DirectoryEntry(ROOTDSE) { AuthenticationType = AuthenticationTypes.Secure};
            
            StringBuilder sb = new StringBuilder();
            sb.Append(rootDse.Properties[DNSHOSTNAME].Value);
            sb.Append("|");
            sb.Append(rootDse.Properties[DEFAULTNAMINGCONTEXT].Value);
            
            return sb.ToString() != null ? sb.ToString() : null;
        }
        public  bool ValidateCredentials(string userName, string password, string domainName,  string dcName = null, ContextOptions contextOptions = ContextOptions.SecureSocketLayer)
        {
            try
            {
                string name = dcName;
                if (string.IsNullOrEmpty(dcName))
                {
                    contextOptions |= ContextOptions.ServerBind;
                    name = domainName;
                }
                   
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, name))
                {
                    //Logging.Info(typeof(ADValidation), pc.ConnectedServer);
                    Console.WriteLine(pc.ConnectedServer);
                    var up = UserPrincipal.FindByIdentity(pc, userName);
                    if (up != null) Console.WriteLine(up.UserPrincipalName);

                    return pc.ValidateCredentials(userName, password, contextOptions);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public  bool ValidateCredentials(string userName, string password,string domainName, string dc, int port)
        {
            try
            {
                using (LdapConnection ldap = new LdapConnection(new LdapDirectoryIdentifier(dc, port)))
                {
                    ldap.SessionOptions.Sealing = false; //turns off kerberos encryption
                    ldap.SessionOptions.SecureSocketLayer = true; //turns on ssl
                    ldap.SessionOptions.ProtocolVersion = 3;
                    ldap.Bind(new NetworkCredential(userName, password, domainName));
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

        public static bool ValidateCredentialsTLS(string username, string domainname, string password, out string serverNameUsed)
        {
            //Reference port numbers = https://technet.microsoft.com/en-us/library/dd772723(v=ws.10).aspx
            int LdapSSLPort = 636;
            int LdapGcSSLPort = 3269;
            foreach (var dc in Auth.Helpers.NativeWrapped.EnumerateDCs(domainname, DsFlag.DS_ONLY_LDAP_NEEDED))
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
    }
}
