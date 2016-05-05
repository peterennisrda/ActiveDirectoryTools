#region NameSpaces
using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
#endregion

namespace ADT00DomainInfo
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
        DS_DNS_FOREST_FLAG = 0x80000000    // DnsForestName is a DNS name
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
            int MAX_TTL = 6;

            try
            {
                PingOptions po = new PingOptions();
                po.Ttl = 1;
                //We trace the route by setting the ttl at one and increasing by one each time
                //When it hits the address we're trying to hit, we're done
                //Otherwise, an address on the way will reply that the ttl expired
                while (po.Ttl < MAX_TTL) //Capping the route at MAX_TTL
                {
                    Ping p = new Ping();
                    var pr = p.Send(ip, 5000, new byte[] { 0 }, po);
                    OutputToConsole(tabs + 1, "{0} - {1}\t|\t{2} ms", po.Ttl, pr.Address, pr.RoundtripTime);
                    po.Ttl++;
                    if (pr.Address == null) continue;
                    if (pr.Address.Equals(ip)) break;
                }
                Console.WriteLine("TraceRoute Ping < " + MAX_TTL);
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

    class Program
    {
        #region User Details from AD

        //More efficient implementation of UserDetails that does not require enumerating all users in the domain, effectively, to work.
        //There's actually a simple property off of UserPrincipal that will get that information
        //For a user other than the current user, forego the PrincipalSearcher and use UserPrincipal.FindByIdentity
        /// <summary>
        /// Get details from user's active directory account
        /// </summary>
        public static DirectoryEntry CurrentUserEntry
        {
            get
            {
                return UserPrincipal.Current.GetUnderlyingObject() as DirectoryEntry;
            }
        }

        public static void UserDetails()
        {
            var de = CurrentUserEntry;
            if (de != null)
            {
                Console.WriteLine("First Name: " + de.Properties["givenName"].Value);
                Console.WriteLine("Last Name : " + de.Properties["sn"].Value);
                Console.WriteLine("SAM account name   : " + de.Properties["samAccountName"].Value);
                Console.WriteLine("User principal name: " + de.Properties["userPrincipalName"].Value);
                Console.WriteLine("PropertyValueCollection");
                PropertyCollection pc = de.Properties;
                foreach (PropertyValueCollection col in pc)
                {
                    Console.WriteLine(col.PropertyName + " : " + col.Value);
                }
            }
        }
        #endregion

        #region Main
        static void Main(string[] args)
        {
            List<string> additionalDomains = new List<string>();
            List<string> additionalMachines = new List<string>();
            Console.WriteLine("*******Output Domain Network Information*************");
            do
            {
                Console.WriteLine("Input the name of an additional domain to query or leave blank to stop inputting domains and press Enter");
                string next = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(next))
                {
                    break;
                }
                else
                {
                    additionalDomains.Add(next);
                }
            }
            while (true);

            do
            {
                Console.WriteLine("Input the name of an additional machine to query the domain information on or leave blank to stop inputting machines and press Enter");
                string next = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(next))
                {
                    break;
                }
                else
                {
                    additionalMachines.Add(next);
                }
            }
            while (true);

            Helpers.OutputDomainFindingInfoToConsole(additionalDomains, additionalMachines);

            Console.WriteLine();
            Console.WriteLine("******************User Details from AD***************");
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            UserDetails();

            Console.WriteLine();
            Console.WriteLine("******************Exit the Program*******************");
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
        #endregion
    }
}

