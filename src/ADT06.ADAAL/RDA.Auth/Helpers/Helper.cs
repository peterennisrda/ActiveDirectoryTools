using RDA.Auth.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static RDA.Auth.ADValidation;


namespace RDA.Auth.Helpers
{
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
                var dci = Auth.Helpers.NativeWrapped.GetDc(domain, DsFlag.DS_RETURN_DNS_NAME | DsFlag.DS_ONLY_LDAP_NEEDED, machine);
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
}
