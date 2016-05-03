#region NameSpaces
using System;
using System.DirectoryServices.AccountManagement;
#endregion

namespace ADT01.Validation
{
    class Program
    {
        #region 1. Is User Validated
        /// <summary>
        /// This is the easier way to validate credentials using .NET objects
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="domainName">Domain Name</param>
        /// <param name="password">Password</param>
        /// <param name="optionalDCName">Name of the specific DC if one is desired</param>
        /// <param name="contextOptions">Type of authentication and data transfer; default set to Sealed since that is encrypted but doesn't require certificate setup... pass SecureSocketLayer to use TLS instead</param>
        /// <returns>Credential validate success; doesn't trap errors so exceptions could be thrown</returns>
        public static bool ValidateCredentials(string userName, string domainName, string password, string optionalDCName = null, ContextOptions contextOptions = ContextOptions.Sealing)
        {
            //This isn't catching exceptions
            bool dcSpecified = string.IsNullOrEmpty(optionalDCName);
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, dcSpecified ? domainName : optionalDCName))
            {
                if (dcSpecified) contextOptions |= ContextOptions.ServerBind;
                return pc.ValidateCredentials(userName, password, contextOptions);
            }
        }

        /// <summary>
        /// Version of IsUserValidated that uses ValidateCredentials Method
        /// </summary>
        /// <returns></returns>
        public static bool IsUserValidated()
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
            //Console.WriteLine("Input the ServerName and press Enter");
            //theServerName = Console.ReadLine();
            try
            {
                bool result = ValidateCredentials(theUserName, theUserDomainName, theUserPassword);
                Console.WriteLine("************************User*************************");
                Console.WriteLine("theUserName = " + theUserName);
                Console.WriteLine("theUserDomainName = " + theUserDomainName);
                Console.WriteLine("IsUserValidated() = " + result);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }
        #endregion

        #region 2. User Names in Group
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

        #region 3. Is User in Group
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
    }
}
