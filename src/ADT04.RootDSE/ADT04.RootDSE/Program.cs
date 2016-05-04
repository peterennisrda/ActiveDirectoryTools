#region NameSpaces
using System;
using System.DirectoryServices;
#endregion

namespace ADT04RootDSE
{
    class Program
    {
        static string theRootDSE = "";
        // Ref: http://stackoverflow.com/questions/4015407/determine-current-domain-controller-programmatically

        public static string RetrieveRootDseDefaultNamingContext()
        {
            String RootDsePath = "LDAP://RootDSE";
            const string DefaultNamingContextPropertyName = "defaultNamingContext";

            DirectoryEntry rootDse = new DirectoryEntry(RootDsePath)
            {
                AuthenticationType = AuthenticationTypes.Secure
            };

            object propertyValue = rootDse.Properties[DefaultNamingContextPropertyName].Value;

            return propertyValue != null ? propertyValue.ToString() : null;
        }

        #region Main
        static void Main(string[] args)
        {
            theRootDSE = RetrieveRootDseDefaultNamingContext();
            Console.WriteLine("The RootDSE is " + theRootDSE);
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }
        #endregion
    }
}
