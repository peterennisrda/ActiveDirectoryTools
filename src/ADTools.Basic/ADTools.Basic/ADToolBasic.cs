#region NameSpaces
using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
#endregion

namespace ADToolsBasic
{
    class Program
    {
        #region 1. Is User Validated
        /// <summary>
        /// Validation for user login to Active Directory
        /// </summary>
        /// <returns></returns>

        private static string theServerContextName = "example.com";     // e.g. "example.local"

        public static bool IsUserValidated()
        {
            bool validation;
            string theUserName;     // sAM Account Name
            string theUserDomainName;
            string theUserPassword;
            string theServerName;
            int thePortNumber;

            Console.WriteLine("Input the UserName (SAM Account Name) and press Enter");
            theUserName = Console.ReadLine();
            Console.WriteLine("Input the UserDomainName and press Enter");
            theUserDomainName = Console.ReadLine();
            Console.WriteLine("Input the UserPassword and press Enter");
            //theUserPassword = Console.ReadLine();
            theUserPassword = ReadPassword();
            //Console.WriteLine("Your Password is:" + theUserPassword);
            Console.WriteLine("Input the ServerName and press Enter");
            theServerName = Console.ReadLine();
            // LDAP uses port 389 and LDAPS uses port 636
            Console.WriteLine("Enter 389 for LDAP or 636 for LDAPS and press Enter");
            thePortNumber = Convert.ToInt32(Console.ReadLine());
            if (thePortNumber == 389)
            {
                Console.WriteLine("thePortNumber = " + thePortNumber);
            }
            else if (thePortNumber == 636)
            {
                Console.WriteLine("thePortNumber = " + thePortNumber);
            }
            else
            {
                Console.WriteLine("Incorrect data entry!");
                Console.WriteLine("Press Enter to Exit");
                Console.ReadLine();
                Environment.Exit(0);
            }

            try
            {
                // Declare Network Credential with the administrative Username, Password, and Active Directory Domain
                NetworkCredential nc = new NetworkCredential(theUserName, theUserPassword, theUserDomainName);
                // Create a directory identifier and connection
                //var ldapid = new LdapDirectoryIdentifier(theServerName, thePortNumber, false, false);
                var ldapid = new LdapDirectoryIdentifier(theServerName, thePortNumber, false, false);
                LdapConnection lcon = new LdapConnection(ldapid, nc);
                lcon.Credential = nc;
                Console.WriteLine("nc = " + nc);
                Console.WriteLine("nc.UserName = " + nc.UserName);
                Console.WriteLine("nc.Domain = " + nc.Domain);
                Console.WriteLine("nc.Password = " + "************");     //nc.Password);
                Console.WriteLine("nc.SecurePassword = " + nc.SecurePassword);
                lcon.AuthType = AuthType.Basic;
                Console.WriteLine("lcon.AuthType = " + lcon.AuthType);
                lcon.Bind(nc); // User has authenticated as these credentials were used to login to the dc
                validation = true;
            }
            catch (LdapException)
            {
                Console.WriteLine("LdapException...");
                validation = false;
            }
            Console.WriteLine("************************User*************************");
            Console.WriteLine("theUserName = " + theUserName);
            Console.WriteLine("theUserDomainName = " + theUserDomainName);
            Console.WriteLine("IsUserValidated() = " + validation);
            return validation;
        }
        #endregion

        #region Read the User Password
        public static string ReadPassword()
        {
            // Ref: http://stackoverflow.com/questions/29201697/hide-replace-when-typing-a-password-c
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // Remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // Get the location of the cursor
                        int pos = Console.CursorLeft;
                        // Move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // Replace it with space
                        Console.Write(" ");
                        // Move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // Add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
        #endregion

        #region 2. List Active Directory Users
        /// <summary>
        /// List users from current domain
        ///</summary>
        public static void ListADUsers()
        {
            const int MAX_USERS = 20;

            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.UserDomainName);
                string userNames = "";
                string authenticationType = "";
                int i = 0;
                foreach (DirectoryEntry child in directoryEntry.Children)
                {
                    if (child.SchemaClassName == "User")
                    {
                        userNames += i + " " + child.Name + Environment.NewLine; // Iterates and binds all users using a newline
                        authenticationType += child.Username + Environment.NewLine;
                        i++;
                        if (i == MAX_USERS)
                        {
                            Console.WriteLine("Break at " + MAX_USERS + " users");
                            break;
                        }
                    }
                }
                Console.WriteLine(userNames);
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred...");
            }
        }
        #endregion

        #region 3. User Names in Group
        /// <summary>
        /// Get user names from selected group
        /// </summary>
        public static void ListUsersInGroup()
        {
            string theUserGroup;

            try
            {
                Console.WriteLine(ContextType.Domain);
                Console.WriteLine(Environment.UserDomainName);
                Console.WriteLine(Environment.UserName);
                Console.WriteLine("Input the Server Context Name (e.g. example.com) and press Enter");
                theServerContextName = Console.ReadLine();
                Console.WriteLine("theServerContextName = " + theServerContextName);

                // Set up domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, theServerContextName);
                // Find the group in question
                Console.WriteLine("Input the UserGroup and press Enter");
                theUserGroup = Console.ReadLine();
                GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, theUserGroup);
                // If found...
                if (group != null)
                {
                    // Iterate over members
                    foreach (Principal p in group.GetMembers())
                    {
                        Console.WriteLine("{0}: {1}, {2}", p.StructuralObjectClass, p.DisplayName, p.SamAccountName);
                    }
                }
                Console.WriteLine();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred...");
            }
        }
        #endregion

        #region 4. Is User in Group
        /// <summary>
        /// Test if the user is a member of the selected group
        /// </summary>
        public static bool IsUserInGroup()
        {
            string theUserGroup;
            string theUserName;
            bool validation;

            try
            {
                validation = false;

                Console.WriteLine(ContextType.Domain);
                Console.WriteLine(Environment.UserDomainName);
                Console.WriteLine(Environment.UserName);
                Console.WriteLine("Input the Server Context Name (e.g. example.com) and press Enter");
                theServerContextName = Console.ReadLine();
                Console.WriteLine("theServerContextName = " + theServerContextName);

                // Set up domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, theServerContextName);
                // Find the group in question
                Console.WriteLine("Input the UserGroup and press Enter");
                theUserGroup = Console.ReadLine();
                Console.WriteLine("Input the UserName and press Enter");
                theUserName = Console.ReadLine();
                GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, theUserGroup);
                // If found...
                if (group != null)
                {
                    // Iterate over members
                    foreach (Principal p in group.GetMembers())
                    {
                        // Do what is required for the group members
                        if (p.SamAccountName == theUserName)
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

        #region 5. User Details from AD
        /// <summary>
        /// Get details from user's active directory account
        /// </summary>
        public static void UserDetails()
        {
            try
            {
                const int MAX_USERS = 20;
                int i = 0;
                Console.WriteLine(ContextType.Domain);
                Console.WriteLine(Environment.UserDomainName);
                Console.WriteLine(Environment.UserName);
                Console.WriteLine("Input the Server Context Name (e.g. example.com) and press Enter");
                theServerContextName = Console.ReadLine();
                Console.WriteLine("theServerContextName = " + theServerContextName);

                using (var context = new PrincipalContext(ContextType.Domain, theServerContextName))
                {
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        Console.WriteLine("foreach searcher...");
                        foreach (var result in searcher.FindAll())
                        {
                            DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                            Console.WriteLine(i + "> de.Name = " + de.Name);
                            Console.WriteLine(i + "> de.Properties[\"givenName\"].Value = " + de.Properties["givenName"].Value);
                            i++;
                            if (i == MAX_USERS)
                            {
                                Console.WriteLine("Break at " + MAX_USERS + " users");
                                break;
                            }
                            if ((string)de.Properties["samAccountName"].Value == Environment.UserName)
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
                                    //Console.WriteLine();
                                }
                            }
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred...");
            }
        }
        #endregion

        #region Main
        static void Main(string[] args)
        {
            Console.WriteLine("****************Is User Validated********************");
            Console.WriteLine("1. Press Enter to continue");
            Console.ReadLine();
            Console.WriteLine(IsUserValidated());
            Console.WriteLine();

            Console.WriteLine("*************List Active Directory Users*************");
            Console.WriteLine("2. Press Enter to continue");
            Console.ReadLine();
            ListADUsers();

            Console.WriteLine("*******************User Names in Group***************");
            Console.WriteLine("3. Press Enter to continue");
            Console.ReadLine();
            ListUsersInGroup();

            Console.WriteLine("*******************Is User in Group******************");
            Console.WriteLine("4. Press Enter to continue");
            Console.ReadLine();
            Console.WriteLine(IsUserInGroup());
            Console.WriteLine();

            Console.WriteLine("******************User Details from AD***************");
            Console.WriteLine("5. Press Enter to continue");
            Console.ReadLine();
            UserDetails();

            Console.WriteLine("******************Exit the Program*******************");
            Console.WriteLine("6. Press Enter to exit");
            Console.ReadLine();
        }
        #endregion
    }
}
