using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace RDA.Auth.Helpers
{
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
}
