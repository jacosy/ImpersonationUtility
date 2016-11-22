using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryAccessProject
{
    public class ImpersonateService
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr existingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr duplicateTokenHandle);

        // logon types
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        // logon providers
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_PROVIDER_WINNT50 = 3;
        const int LOGON32_PROVIDER_WINNT40 = 2;
        const int LOGON32_PROVIDER_WINNT35 = 1;

        public static IntPtr GetDupToken(string userName, string password, string domain)
        {
            IntPtr token = IntPtr.Zero;
            IntPtr dupToken = IntPtr.Zero;
            bool isSuccess = LogonUser(userName, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref token);

            if (!isSuccess)
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }

            isSuccess = DuplicateToken(token, 2, ref dupToken);
            if (!isSuccess)
            {
                throw new ApplicationException("Failed to DuplicateToken, Code = " + Marshal.GetLastWin32Error());
            }

            return dupToken;
        }

        public static WindowsImpersonationContext GetImpersonationContext(string userName, string password, string domainName)
        {
            return new WindowsIdentity(ImpersonateService.GetDupToken(userName, password, domainName)).Impersonate();
        }
    }
}
