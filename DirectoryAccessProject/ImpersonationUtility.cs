using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.IO;
using System.ComponentModel;

namespace DirectoryAccessProject
{
    public class ImpersonationUtility
    {
        protected bool IsInContext { get { return _context != null; } }
        private WindowsImpersonationContext _context = null;
        private string _domain;
        private string _password;
        private string _username;
        private IntPtr _token;

        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token.
        const int LOGON32_LOGON_INTERACTIVE = 2;

        public ImpersonationUtility(string domain, string username, string password)
        {
            _domain = domain;
            _username = username;
            _password = password;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrus")]
        public void Enter()
        {
            if (this.IsInContext) return;
            _token = new IntPtr(0);
            try
            {
                _token = IntPtr.Zero;
                bool logonSuccessfull = LogonUser(_username, _domain, _password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref _token);
                if (logonSuccessfull == false)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error);
                }
                WindowsIdentity identity = new WindowsIdentity(_token);
                _context = identity.Impersonate();
            }
            catch (Exception ex)
            {
                throw new Exception("ImpersonationUtility Enter() Error: " + ex.Message);
            }
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void Leave()
        {
            if (!this.IsInContext)
            {
                return;
            }

            _context.Undo();

            if (_token != IntPtr.Zero)
            {
                CloseHandle(_token);
            }

            _context = null;
        }
    }

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle() : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }
}
