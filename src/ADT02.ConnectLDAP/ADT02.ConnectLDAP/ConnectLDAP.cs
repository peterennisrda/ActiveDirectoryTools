using System;
using System.Net;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Security.Permissions;

namespace ADT02.ConnectLDAP
{
    // Ref: https://msdn.microsoft.com/en-us/library/ms257181(v=vs.90).aspx
    // Adapted for manual data entry and providing a correct description for the ldap_init process.
    [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted = true)]

    public class LDAPConnect
    {
        // static variables used throughout the example
        static LdapConnection ldapConnection;
        static string ldapServer;
        static NetworkCredential credential;
        static string targetOU; // dn of an OU. eg: "OU=sample,DC=fabrikam,DC=com"

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    string theUserName;     // sAM Account Name
                    string theUserDomainName;
                    string theUserPassword;

                    Console.WriteLine("************Connect to LDAP Server*******************");
                    Console.WriteLine("1. Press Enter to continue");
                    Console.ReadLine();
                    Console.WriteLine("Input the LDAP ServerName and press Enter");
                    ldapServer = Console.ReadLine();
                    Console.WriteLine("Input UserName (sAM) and press Enter");
                    theUserName = Console.ReadLine();
                    Console.WriteLine("Input UserPassword and press Enter");
                    theUserPassword = ReadPassword();
                    //Console.WriteLine("Your Password is:" + theUserPassword);
                    Console.WriteLine("Input the UserDomainName and press Enter");
                    theUserDomainName = Console.ReadLine();

                    credential = new NetworkCredential(theUserName, theUserPassword, theUserDomainName);

                    Console.WriteLine("Input TargetOU and press Enter");
                    targetOU = Console.ReadLine();
                }
                else
                {
                    GetParameters(args);  // Get the Command Line parameters
                }

                // Create the new LDAP connection
                ldapConnection = new LdapConnection(ldapServer);
                ldapConnection.Credential = credential;
                Console.WriteLine("At the point of the constructor returning ldap_init has been called.");
                Console.WriteLine("Ref: https://msdn.microsoft.com/en-us/library/windows/desktop/aa366938(v=vs.85).aspx");
                Console.WriteLine("This means the data structures have been set up for a later connection. ");
                Console.WriteLine("Setting the credentials property marks the object to perform a new Bind()");
                Console.WriteLine("if it’s already bound the next time it would need to be used to get data from the ldap server.");
                Console.WriteLine();
                Console.WriteLine("2. Press Enter to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\r\nUnexpected exception occurred:\r\n\t" + ex.GetType() + ":" + ex.Message);
                Console.ReadLine();
            }
        }

        static void GetParameters(string[] args)
        {
            // When running: ConnectLDAP.exe <ldapServer> <user> <pwd> <domain> <targetOU>

            if (args.Length != 5)
            {
                Console.WriteLine("Usage: ConnectLDAP.exe <ldapServer> <user> <pwd> <domain> <targetOU>");
                Environment.Exit(-1);// return an error code of -1
            }

            // test arguments to ensure they are valid and secure

            // initialize variables
            ldapServer = args[0];
            credential = new NetworkCredential(args[1], args[2], args[3]);
            targetOU = args[4];
        }

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
    }
}