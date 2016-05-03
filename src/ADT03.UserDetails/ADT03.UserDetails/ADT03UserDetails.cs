#region NameSpaces
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
#endregion

namespace ADT03UserDetails
{  
    class Program
    {
        #region 1. User Details from AD
        //UserDetails here does not enumerate users in the domain.
        //Use a simple property of UserPrincipal to get that information for a user other than the current user.
        //Forego the PrincipalSearcher and use UserPrincipal.FindByIdentity
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
                Console.WriteLine("    First Name: " + de.Properties["givenName"].Value);
                Console.WriteLine("    Last Name : " + de.Properties["sn"].Value);
                Console.WriteLine("    sAM account name   : " + de.Properties["samAccountName"].Value);
                Console.WriteLine("    User principal name: " + de.Properties["userPrincipalName"].Value);
                Console.WriteLine("    PropertyValueCollection");
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
            Console.WriteLine("******************User Details from AD***************");
            Console.WriteLine("1. Press Enter to continue");
            Console.ReadLine();
            UserDetails();

            Console.WriteLine();
            Console.WriteLine("******************Exit the Program*******************");
            Console.WriteLine("2. Press Enter to exit");
            Console.ReadLine();
        }
        #endregion
    }
}

