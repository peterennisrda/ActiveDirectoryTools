using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace RDA.Auth.Helpers
{
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
}
