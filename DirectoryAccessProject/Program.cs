using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;

namespace DirectoryAccessProject
{
    class Program
    {
        private static string directoryPath = @"\\tpesdev5\d$\Work\ultimus\attachment";
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Before impersonation: " + WindowsIdentity.GetCurrent().Name);
                //AccessDirectoryWithoutImpersonation();
                //ImpersonateByUtility();
                ImpersonateByService();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {                
                Console.WriteLine("After Undo: " + WindowsIdentity.GetCurrent().Name);
            }

            Console.ReadLine();
        }

        static void AccessDirectoryWithoutImpersonation()
        {
            foreach (string dir in Directory.GetDirectories(directoryPath))
            {
                Console.WriteLine(dir);
            }
        }

        static void ImpersonateByUtility()
        {
            //ImpersonationUtility impersonation = new ImpersonationUtility("tutorabc", "bpm_admin", "Abc123");
            ImpersonationUtility impersonation = new ImpersonationUtility("longoriayou", "bpm_admin", "Fgl77vxs589");
            impersonation.Enter();
            Console.WriteLine("impersonating...: " + WindowsIdentity.GetCurrent().Name);
            foreach (string dir in Directory.GetDirectories(directoryPath))
            {
                Console.WriteLine(dir);
            }
            impersonation.Leave();
        }

        static void ImpersonateByService()
        {
            //using (WindowsImpersonationContext wic = ImpersonateService.GetImpersonationContext("bpm_admin", "Abc123", "tutorabc"))
            using (WindowsImpersonationContext wic = ImpersonateService.GetImpersonationContext("longoriayou", "Fgl77vxs589", "tutorabc"))
            {                
                Console.WriteLine("impersonating...: " + WindowsIdentity.GetCurrent().Name);
                foreach (string dir in Directory.GetDirectories(directoryPath))
                {
                    Console.WriteLine(dir);
                }
                wic.Undo();
            }
        }
    }
}

