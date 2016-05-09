#region NameSpaces
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
using System.Windows.Forms;
#endregion

class ADValidation
{
    #region Flags
    [Flags]
    public enum DsFlag : uint
    {
        None = 0,
        DS_FORCE_REDISCOVERY = 0x00000001,
        DS_DIRECTORY_SERVICE_REQUIRED = 0x00000010,
        DS_DIRECTORY_SERVICE_PREFERRED = 0x00000020,
        DS_GC_SERVER_REQUIRED = 0x00000040,
        DS_PDC_REQUIRED = 0x00000080,
        DS_BACKGROUND_ONLY = 0x00000100,
        DS_IP_REQUIRED = 0x00000200,
        DS_KDC_REQUIRED = 0x00000400,
        DS_TIMESERV_REQUIRED = 0x00000800,
        DS_WRITABLE_REQUIRED = 0x00001000,
        DS_GOOD_TIMESERV_PREFERRED = 0x00002000,
        DS_AVOID_SELF = 0x00004000,
        DS_ONLY_LDAP_NEEDED = 0x00008000,
        DS_IS_FLAT_NAME = 0x00010000,
        DS_IS_DNS_NAME = 0x00020000,
        DS_RETURN_DNS_NAME = 0x40000000,
        DS_RETURN_FLAT_NAME = 0x80000000
    }

    [Flags]
    public enum DsReturnFlags : uint
    {
        DS_PDC_FLAG = 0x00000001,// DC is PDC of Domain
        DS_GC_FLAG = 0x00000004,// DC is a GC of forest
        DS_LDAP_FLAG = 0x00000008,// Server supports an LDAP server
        DS_DS_FLAG = 0x00000010,// DC supports a DS and is a Domain Controller
        DS_KDC_FLAG = 0x00000020,// DC is running KDC service
        DS_TIMESERV_FLAG = 0x00000040,// DC is running time service
        DS_CLOSEST_FLAG = 0x00000080,// DC is in closest site to client
        DS_WRITABLE_FLAG = 0x00000100,// DC has a writable DS
        DS_GOOD_TIMESERV_FLAG = 0x00000200,// DC is running time service (and has clock hardware)
        DS_NDNC_FLAG = 0x00000400,// DomainName is non-domain NC serviced by the LDAP server
        DS_SELECT_SECRET_DOMAIN_6_FLAG = 0x00000800,// DC has some secrets
        DS_FULL_SECRET_DOMAIN_6_FLAG = 0x00001000,// DC has all secrets
        DS_WS_FLAG = 0x00002000,// DC is running web service
        DS_DS_8_FLAG = 0x00004000,// DC is running Win8 or later
        DS_PING_FLAGS = 0x000FFFFF,// Flags returned on ping
        DS_DNS_CONTROLLER_FLAG = 0x20000000,// DomainControllerName is a DNS name
        DS_DNS_DOMAIN_FLAG = 0x40000000,// DomainName is a DNS name
        DS_DNS_FOREST_FLAG = 0x80000000// DnsForestName is a DNS name
    }
    #endregion

    public enum DomainControllerAddressType : int
    {
        DS_INET_ADDRESS = 1,
        DS_NETBIOS_ADDRESS = 2
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DOMAIN_CONTROLLER_INFO
    {
        public string DomainControllerName;
        public string DomainControllerAddress;
        public DomainControllerAddressType DomainControllerAddressType;
        public Guid DomainGuid;
        public string DomainName;
        public string DnsForestName;
        public DsReturnFlags Flags;
        public string DcSiteName;
        public string ClientSiteName;
    }

    public enum DsEnumerateOptions : int
    {
        None = 0,
        DS_ONLY_DO_SITE_NAME = 0x01,             // Non-site specific names should be avoided.
        DS_NOTIFY_AFTER_SITE_RECORDS = 0x02      // Return ERROR_FILEMARK_DETECTED after all
                                                 // site specific records have been processed.
    }

    #region NativeMethods
    public static class NativeMethods
    {
        [DllImport("NetApi32.dll", CharSet = CharSet.Unicode)]
        public static extern int DsGetDcNameW(
            IntPtr ComputerName,
            string DomainName,
            IntPtr DomainGuid,
            IntPtr SiteName,
            DsFlag Flags,
            out IntPtr DomainControllerInfo);

        [DllImport("NetApi32.dll", CharSet = CharSet.Unicode)]
        public static extern int DsGetDcNameW(
            string ComputerName,
            string DomainName,
            IntPtr DomainGuid,
            IntPtr SiteName,
            DsFlag Flags,
            out IntPtr DomainControllerInfo);

        [DllImport("NetApi32.dll", CharSet = CharSet.Unicode)]
        public static extern int DsGetDcOpenW(
            string DnsName,
            DsEnumerateOptions options,
            IntPtr SiteName,
            IntPtr DomainGuid,
            IntPtr DnsForestName,
            DsFlag Flags,
            out IntPtr GetDcContext);

        [DllImport("NetApi32.dll", CharSet = CharSet.Unicode)]
        public static extern int DsGetDcNextW(
            IntPtr GetDcContext,
            IntPtr SockAddressCount,
            IntPtr SockAddresses,
            out IntPtr DnsHostName);

        [DllImport("NetApi32.dll", CharSet = CharSet.Unicode)]
        public static extern int NetApiBufferFree(IntPtr bufptr);

        [DllImport("NetApi32.dll", CharSet = CharSet.Unicode)]
        public static extern void DsGetDcCloseW(IntPtr GetDcContext);
    }
    #endregion

    #region NativeWrapped
    public static class NativeWrapped
    {
        internal static void ThrowLastError()
        {
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        internal static void ThrowNetApi(int netApi)
        {
            Marshal.ThrowExceptionForHR(HrFromNetApi(netApi));
        }

        internal static int HrFromNetApi(int netApi)
        {
            return unchecked((int)(0x80070000) | netApi);
        }

        //Get a dc in the domain
        public static DOMAIN_CONTROLLER_INFO GetDc(string domainName, DsFlag flags, string computerNameToPerformSearch = null)
        {
            IntPtr returnValue;
            int result = string.IsNullOrWhiteSpace(computerNameToPerformSearch) ? NativeMethods.DsGetDcNameW(IntPtr.Zero, domainName, IntPtr.Zero, IntPtr.Zero, flags, out returnValue) : NativeMethods.DsGetDcNameW(computerNameToPerformSearch, domainName, IntPtr.Zero, IntPtr.Zero, flags, out returnValue);
            if (result != 0) ThrowNetApi(result);
            var dci = (DOMAIN_CONTROLLER_INFO)Marshal.PtrToStructure(returnValue, typeof(DOMAIN_CONTROLLER_INFO));
            NativeMethods.NetApiBufferFree(returnValue);
            return dci;
        }

        //Enumerate all dcs with srv records
        public static IEnumerable<string> EnumerateDCs(string domainDnsName, DsFlag flags)
        {
            IntPtr handle;
            int result = NativeMethods.DsGetDcOpenW(domainDnsName, DsEnumerateOptions.None, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, flags, out handle);
            if (result != 0) ThrowNetApi(result);
            try
            {
                IntPtr hostName;
                while ((result = NativeMethods.DsGetDcNextW(handle, IntPtr.Zero, IntPtr.Zero, out hostName)) == 0)
                {
                    yield return Marshal.PtrToStringUni(hostName);
                    NativeMethods.NetApiBufferFree(hostName);
                }
                if (result != 259) //NoMoreItems
                {
                    ThrowNetApi(result);
                }
            }
            finally
            {
                NativeMethods.DsGetDcCloseW(handle);
            }
        }
    }
    #endregion

    #region Helpers
    public static class Helpers
    {
        public static IEnumerable<string> GetFlagsFromEnum<T>(T val) //we can't constrain on T : Enum but we can throw of T is not an enum... weird language quirk
        {
            Type t = typeof(T);
            if (!t.IsEnum) throw new InvalidCastException("A type that is not an Enum type was passed to GetFlagsFromEnum");
            foreach (var name in Enum.GetNames(t))
            {
                Enum e = val as Enum;
                if (e.HasFlag((Enum)Enum.Parse(t, name)))
                {
                    yield return name;
                }
            }
        }

        public static void OutputToConsole(int tabs, string format, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tabs; i++) sb.Append("\t");
            sb.AppendFormat(format, args);
            Console.WriteLine(sb.ToString());
        }

        public static void TraceRoute(int tabs, IPAddress ip)
        {
            try
            {
                PingOptions po = new PingOptions();
                po.Ttl = 1;
                //We trace the route by setting the ttl at one and increasing by one each time
                //When it hits the address we're trying to hit, we're done
                //Otherwise, an address on the way will reply that the ttl expired
                while (po.Ttl < 50) //Capping the route at fifty
                {
                    Ping p = new Ping();
                    var pr = p.Send(ip, 5000, new byte[] { 0 }, po);
                    OutputToConsole(tabs + 1, "{0} - {1}\t|\t{2} ms", po.Ttl, pr.Address, pr.RoundtripTime);
                    po.Ttl++;
                    if (pr.Address == null) continue;
                    if (pr.Address.Equals(ip)) break;
                }
            }
            catch (Exception ex)
            {
                OutputToConsole(tabs, "Error performing ICMP trace for {0}:\t{1}", ip, ex);
            }
        }

        public static void OutputNetworkResolutionInformationToConsole(int tabs, string hostnameOrIp)
        {
            hostnameOrIp = hostnameOrIp.Trim('\\');
            try
            {
                var entry = Dns.GetHostEntry(hostnameOrIp);
                OutputToConsole(tabs, "Network Resolution for {0}", hostnameOrIp);
                OutputToConsole(tabs + 1, "HostName:\t{0}", entry.HostName);
                OutputToConsole(tabs + 1, "Aliases:\t{0}", string.Join(", ", entry.Aliases));
                OutputToConsole(tabs + 1, "Addresses:\t{0}", string.Join(", ", (IEnumerable<IPAddress>)entry.AddressList));
                foreach (var address in entry.AddressList)
                {
                    OutputToConsole(tabs + 1, "ICMP to {0}", address);
                    TraceRoute(tabs, address);
                }
            }
            catch (Exception ex)
            {
                OutputToConsole(tabs, "Error performing network resolution for {0}:\t{1}", hostnameOrIp, ex);
            }
        }

        public static void OutputDomainFindingInfoToConsoleForDomainOnMachine(string machine, string domain)
        {
            OutputToConsole(0, "Outputting domain networking information for domain {0} retrieved from {1}", domain, string.IsNullOrEmpty(machine) ? "localhost" : machine);
            try
            {
                var dci = NativeWrapped.GetDc(domain, DsFlag.DS_RETURN_DNS_NAME | DsFlag.DS_ONLY_LDAP_NEEDED, machine);
                OutputToConsole(1, "Results from DsGetDcName:");
                OutputToConsole(2, "{0}:\t{1}", "ClientSiteName", dci.ClientSiteName);
                OutputToConsole(2, "{0}:\t{1}", "DcSiteName", dci.DcSiteName);
                OutputToConsole(2, "{0}:\t{1}", "DnsForestName", dci.DnsForestName);
                OutputToConsole(2, "{0}:\t{1}", "DomainControllerAddress", dci.DomainControllerAddress);
                OutputToConsole(2, "{0}:\t{1}", "DomainControllerAddressType", dci.DomainControllerAddressType);
                OutputToConsole(2, "{0}:\t{1}", "DomainControllerName", dci.DomainControllerName);
                OutputToConsole(2, "{0}:\t{1}", "DomainGuid", dci.DomainGuid);
                OutputToConsole(2, "{0}:\t{1}", "DomainName", dci.DomainName);
                OutputToConsole(2, "{0}:\t{1}", "Flags", string.Join(", ", GetFlagsFromEnum<DsReturnFlags>(dci.Flags)));
                OutputNetworkResolutionInformationToConsole(2, dci.DomainControllerAddress);
                OutputToConsole(1, "Results from DsGetDcNext for {0}:", dci.DomainName);
                //Note: The following won't get results from RoDCs
                var dcs = NativeWrapped.EnumerateDCs(dci.DomainName, DsFlag.None);
                foreach (var dc in dcs)
                {
                    OutputNetworkResolutionInformationToConsole(2, dc);
                }
            }
            catch (Exception ex)
            {
                OutputToConsole(0, "Error outputting domain information for {0} retrieved from {2}:\t{1}", domain, ex, string.IsNullOrEmpty(machine) ? "localhost" : machine);
            }
        }

        public static void OutputDomainFindingInfoToConsoleForDomain(string domain)
        {
            OutputDomainFindingInfoToConsoleForDomainOnMachine(null, domain);
        }

        public static void OutputDomainFindingInfoToConsoleForCurrentDomain()
        {
            OutputDomainFindingInfoToConsoleForDomain(Environment.UserDomainName);
        }

        public static void OutputDomainFindingInfoToConsole(IEnumerable<string> additionalDomainsToLookFor = null, IEnumerable<string> additionalComputersToLookOn = null)
        {
            OutputDomainFindingInfoToConsoleForCurrentDomain();
            if (additionalDomainsToLookFor != null)
            {
                foreach (var d in additionalDomainsToLookFor)
                {
                    OutputDomainFindingInfoToConsoleForDomain(d);
                }
            }
            if (additionalComputersToLookOn != null)
            {
                foreach (var c in additionalComputersToLookOn)
                {
                    OutputDomainFindingInfoToConsoleForDomainOnMachine(c, Environment.UserDomainName);
                    if (additionalDomainsToLookFor != null)
                    {
                        foreach (var d in additionalDomainsToLookFor)
                        {
                            OutputDomainFindingInfoToConsoleForDomainOnMachine(c, d);
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region DnsHostNameRootDse
    // Ref: http://stackoverflow.com/questions/4015407/determine-current-domain-controller-programmatically
    public static string RetrieveDnsHostNameRootDseDefaultNamingContext()
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
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, dcSpecified ? domainName : optionalDCName))
            {
                Console.WriteLine(pc.ConnectedServer);
                var up = UserPrincipal.FindByIdentity(pc, userName);
                if (up != null) Console.WriteLine(up.UserPrincipalName);

                return pc.ValidateCredentials(userName, password, contextOptions);
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
                        Console.WriteLine("Failed validating credentials for {0} on {1}:\t{2}", username, dc, ex);
                        try
                        {
                            return ManuallyValidateTLSCredentials(username, domainname, password, dc, LdapSSLPort);
                        }
                        catch (Exception ex2)
                        {
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
            //MessageBox.Show("In IsUserValidated()");

            try
            {
                string serverName;
                bool result = ValidateCredentialsTLS(theUserName, theUserDomainName, theUserPassword, out serverName);

                Console.WriteLine("************************User*************************");
                Console.WriteLine("theUserName = " + theUserName);
                Console.WriteLine("theUserDomainName = " + theUserDomainName);
                Console.WriteLine("theServerName = " + serverName);
                Console.WriteLine("IsUserValidated() = " + result);
                if (!result)
                {
                    result = ValidateCredentials(theUserName, theUserDomainName, theUserPassword, null, ContextOptions.Negotiate);
                    Console.WriteLine("IsUserValidated() [Kerberos] = " + result.ToString());
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                bool result = ValidateCredentials(theUserName, theUserDomainName, theUserPassword);
                Console.WriteLine("IsUserValidated() = " + result.ToString());
            }
            return false;
        }
        #endregion

        //#region 2. User Names in Group
        ///// <summary>
        ///// Get user names from selected group
        ///// </summary>
        //public static void ListUsersInGroup()
        //{
        //    string theUserGroup;

        //    try
        //    {
        //        Console.WriteLine(ContextType.Domain);
        //        Console.WriteLine(Environment.UserDomainName);
        //        Console.WriteLine(Environment.UserName);

        //        // Set up domain context
        //        PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
        //        // Find the group in question
        //        Console.WriteLine("Input the UserGroup and press Enter");
        //        theUserGroup = Console.ReadLine();
        //        GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, theUserGroup);
        //        // If found...
        //        if (group != null)
        //        {
        //            // Iterate over members
        //            foreach (Principal p in group.GetMembers())
        //            {
        //                Console.WriteLine("{0}: {1}, {2}", p.StructuralObjectClass, p.DisplayName, p.SamAccountName);
        //            }
        //        }
        //        Console.WriteLine();
        //    }
        //    catch (Exception)
        //    {
        //        Console.WriteLine("An error occurred...");
        //    }
        //}
        //#endregion

    #region 2. Is User Authorized
    /// <summary>
    /// Test if the user is a member of the selected group
    /// </summary>
    public static bool IsUserInGroup(string userName, string domainName, string userGroup)
    {
//        string theUserGroup;
//        string theUserName;
        bool validation;

        try
        {
            validation = false;

//            Console.WriteLine(ContextType.Domain);
//            Console.WriteLine(Environment.UserDomainName);
//            Console.WriteLine(Environment.UserName);

            // Set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
//            // Find the group in question
//            Console.WriteLine("Input the UserGroup and press Enter");
//            theUserGroup = Console.ReadLine();
//            Console.WriteLine("Input the UserName and press Enter");
//            theUserName = Console.ReadLine();
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
                        validation= false;
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
    #endregion

        //#region Read the User Password
        //public static string ReadPassword()
        //{
        //    // Ref: http://stackoverflow.com/questions/29201697/hide-replace-when-typing-a-password-c
        //    string password = "";
        //    ConsoleKeyInfo info = Console.ReadKey(true);
        //    while (info.Key != ConsoleKey.Enter)
        //    {
        //        if (info.Key != ConsoleKey.Backspace)
        //        {
        //            Console.Write("*");
        //            password += info.KeyChar;
        //        }
        //        else if (info.Key == ConsoleKey.Backspace)
        //        {
        //            if (!string.IsNullOrEmpty(password))
        //            {
        //                // Remove one character from the list of password characters
        //                password = password.Substring(0, password.Length - 1);
        //                // Get the location of the cursor
        //                int pos = Console.CursorLeft;
        //                // Move the cursor to the left by one character
        //                Console.SetCursorPosition(pos - 1, Console.CursorTop);
        //                // Replace it with space
        //                Console.Write(" ");
        //                // Move the cursor to the left by one character again
        //                Console.SetCursorPosition(pos - 1, Console.CursorTop);
        //            }
        //        }
        //        info = Console.ReadKey(true);
        //    }
        //    // Add a new line because user pressed enter at the end of their password
        //    Console.WriteLine();
        //    return password;
        //}
        //#endregion

    /*
        #region Main
       
        static void Main(string[] args)
        {
            Console.WriteLine("****************Is User Validated********************");
            Console.WriteLine("1. Press Enter to continue");
            Console.ReadLine();
            Console.WriteLine(IsUserValidated());
            Console.WriteLine();

            Console.WriteLine("****************User Names in Group******************");
            Console.WriteLine("2. Press Enter to continue");
            Console.ReadLine();
            ListUsersInGroup();

            Console.WriteLine("****************Is User in Group*********************");
            Console.WriteLine("3. Press Enter to continue");
            Console.ReadLine();
            Console.WriteLine(IsUserInGroup());
            Console.WriteLine();

            Console.WriteLine("******************Exit the Program*******************");
            Console.WriteLine("4. Press Enter to exit");
            Console.ReadLine();
        } 
        #endregion
    */
    }
