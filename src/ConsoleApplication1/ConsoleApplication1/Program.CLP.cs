#region NameSpaces
using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Configuration;
using System.Security.Authentication;
using System.Security.Principal;
#endregion

namespace ActiveDirectoryTools
{
    class Program
    {
        #region 1. Is User Validated
        #region CPL @ MSFT Added
        /// <summary>
        /// This is the easier way to validate credentials using .NET objects
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="domainname">Domain name</param>
        /// <param name="password">Password</param>
        /// <param name="optionalDCName">Name of the specific DC if one is desired</param>
        /// <param name="contextOptions">Type of authentication and data transfer; I set the default to Sealed since that is encrypted but doesn't require certificate setup... pass SecureSocketLayer to use TLS instead</param>
        /// <returns>Credential validate success; doesn't trap errors so exceptions could be thrown</returns>
        public static bool ValidateCredentials(string username, string domainname, string password, string optionalDCName = null, ContextOptions contextOptions = ContextOptions.Sealing)
        {
            //This isn't catching exceptions
            bool dcSpecified = string.IsNullOrEmpty(optionalDCName);
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, dcSpecified ? domainname : optionalDCName))
            {
                if (dcSpecified) contextOptions |= ContextOptions.ServerBind;
                return pc.ValidateCredentials(username, password, contextOptions);
            } 
        }
		
        /// <summary>
        /// Version of IsUserValidated that uses ValidateCredentials Method
        /// </summary>
        /// <returns></returns>
        public static bool IsUserValidated2()
        {
            string theUserName;     // SAM Account Name
            string theUserDomainName;
            string theUserPassword;
            string theServerName;
            //int thePortNumber;

            Console.WriteLine("Input the UserName (SAM) and press Enter");
            theUserName = Console.ReadLine();
            Console.WriteLine("Input the UserDomainName and press Enter");
            theUserDomainName = Console.ReadLine();
            Console.WriteLine("Input the UserPassword and press Enter");
            //theUserPassword = Console.ReadLine();
            theUserPassword = ReadPassword();
            //Console.WriteLine("Your Password is:" + theUserPassword);
            Console.WriteLine("Input the ServerName and press Enter");
            theServerName = Console.ReadLine();
            try
            {
                bool result = ValidateCredentials(theUserName, theUserDomainName, theUserPassword);
                Console.WriteLine("************************User*************************");
                Console.WriteLine("theUserName = " + theUserName);
                Console.WriteLine("theUserDomainName = " + theUserDomainName);
                Console.WriteLine("ValidatedUser() = " + result);
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }
        #endregion
		
        /// <summary>
        /// Validation for user login to Active Directory
        /// </summary>
        /// <returns></returns>
        public static bool IsUserValidated()
        {
            bool validation;
            string theUserName;     // SAM Account Name
            string theUserDomainName;
            string theUserPassword;
            string theServerName;
            //int thePortNumber;

            Console.WriteLine("Input the UserName (SAM) and press Enter");
            theUserName = Console.ReadLine();
            Console.WriteLine("Input the UserDomainName and press Enter");
            theUserDomainName = Console.ReadLine();
            Console.WriteLine("Input the UserPassword and press Enter");
            //theUserPassword = Console.ReadLine();
            theUserPassword = ReadPassword();
            //Console.WriteLine("Your Password is:" + theUserPassword);
            Console.WriteLine("Input the ServerName and press Enter");
            theServerName = Console.ReadLine();
            // AuthType.Basic uses port 389
            //Console.WriteLine("Enter the Port number and press Enter");
            //thePortNumber = Convert.ToInt32(Console.ReadLine());

            try
            {
                //NetworkCredential nc = new NetworkCredential(Environment.UserName, theUserPassword, Environment.UserDomainName);
                // Declare Network Credential with the administrative Username, Password, and Active Directory Domain
                NetworkCredential nc = new NetworkCredential(theUserName, theUserPassword, theUserDomainName);
                // Create a directory identifier and connection
                //var ldapid = new LdapDirectoryIdentifier(theServerName, thePortNumber, false, false);
                var ldapid = new LdapDirectoryIdentifier(theServerName, 389, false, false);
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
            Console.WriteLine("ValidatedUser() = " + validation);
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
        #region CPL @ MSFT Added
        //Note that the pre-existing code "ListADUsers" uses the WinNT provider which essentially means you're not querying the domain but rather the SAM on whichever machine it points to (which would be localhost the way this is written)
        //I've put another example here that lists all the actual users in a domain

        /// <summary>
        /// Lists users from the current domain
        /// </summary>
        public static void ListADUsers2()
        {
            int maxUsers = 20;
            //This defaults to using the domain from the current process token so specifying whatever is in Environment.UserDomainName isn't necessary
            //Note that this (listing all the users in a directory) can take a long time in a large domain and isn't something you want to do outside of testing purposes most likely
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain))
            using (UserPrincipal up = new UserPrincipal(pc))
            {
                up.Name = "*";
                using (PrincipalSearcher ps = new PrincipalSearcher(up))
                {
                    int i = 0;
                    foreach(var p in ps.FindAll())
                    {
                        Console.WriteLine("{0}:\t{1}", p.SamAccountName, p.Name);
                        if(++i == maxUsers)
                        {
                            break;
                        }
                    }
                }
            }
        }
		#endregion
		
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
        #region CPL @ MSFT Added
        //Note that the methodology of "ListUsersInGroup" is not recursive; it will not handle users that are members of Group B when Group B is a member of the queried group
        //If you need to do that, let me know and I can show you a sample of how
        //However, you should rarely need to know that for use outside of management scenarios (nor this information either); if you are wanting to restrict things by user identity, there are much better, faster ways to go such as logging in the user and using an impersonation token when access things and letting the kernel object manager sort it out like it does for most things in the OS such as files, registry keys, mutexes, window stations, etc.  SIDS, DACLs, etc. are also familiar things that admins can set with in-box tools such as Windows Explorer to change permissions preventing the need from hardcoding groups or configuration lookup methodologies
        #endregion
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

                // Set up domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
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
        #region CPL @ MSFT Added
        //Note that the methodology of "IsUserInGroup" is not recursive; it will not handle users that are members of Group B when Group B is a member of the queried group
        //If you need to do that, let me know and I can show you a sample of how
        #endregion

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

                // Set up domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
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
        #region CPL @ MSFT Added
        //UserDetails may work but is significantly more code than necessary and requires enumerating all users in the domain, effectively, to work
        //There's actually a simple property off of UserPrincipal that will get that information
        //For a user other than the current user, forego the PrincipalSearcher and use UserPrincipal.FindByIdentity
        public static DirectoryEntry CurrentUserEntry
        {
            get
            {
                return UserPrincipal.Current.GetUnderlyingObject() as DirectoryEntry;
            }
        }

        public static void UserDetails2()
        {
            var de = CurrentUserEntry;
            if(de != null)
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
                    Console.WriteLine();
                }
            }
        }
        #endregion

        /// <summary>
        /// Get details from user's active directory account
        /// </summary>
        public static void UserDetails()
        {
            try
            {
                Console.WriteLine(ContextType.Domain);
                Console.WriteLine(Environment.UserDomainName);
                Console.WriteLine(Environment.UserName);
                using (var context = new PrincipalContext(ContextType.Domain, Environment.UserDomainName))
                {
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                            if ((string)de.Properties["givenName"].Value == Environment.UserName)
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
                                    Console.WriteLine();
                                }
                            }
                        }
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
        #region CPL @ MSFT Added
        //Called from Main to see the methods that I substituted
        static void Main2(string[] args)
        {
            Console.WriteLine("******************Validate User**********************");
            Console.WriteLine("1. Press Enter to continue");
            Console.ReadLine();
            Console.WriteLine(IsUserValidated2());
            Console.WriteLine();

            Console.WriteLine("*************List Active Directory Users*************");
            Console.WriteLine("2. Press Enter to continue (type anything before hitting enter to skip)");
            string input = Console.ReadLine();
            if(string.IsNullOrEmpty(input)) ListADUsers2();

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
            UserDetails2();

            Console.WriteLine("******************Exit the Program*******************");
            Console.WriteLine("6. Press Enter to exit");
            Console.ReadLine();
        }
        #endregion

        static void Main(string[] args)
        {
            Main2(args);
            
            Console.WriteLine("******************Validate User**********************");
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
