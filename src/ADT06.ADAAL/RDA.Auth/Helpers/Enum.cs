using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDA.Auth.Helpers
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


        #region DCinfo

        public enum DsEnumerateOptions : int
        {
            None = 0,
            DS_ONLY_DO_SITE_NAME = 0x01,             // Non-site specific names should be avoided.
            DS_NOTIFY_AFTER_SITE_RECORDS = 0x02      // Return ERROR_FILEMARK_DETECTED after all
                                                     // site specific records have been processed.
        }
        #endregion

        public enum DomainControllerAddressType : int
        {
            DS_INET_ADDRESS = 1,
            DS_NETBIOS_ADDRESS = 2
        }
    }


