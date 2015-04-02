using System;
using System.IO;

namespace Lior.Job.Service.PluginableService
{

    internal class Program
    {
        private static AppDomain domain;

        [STAThread]
        private static void Main()
        {
            var cachePath = @"E:\ShadowDLL";// Path.Combine (AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "ShadowCopyCache");
            var pluginPath = @"E:\plugins";// Path.Combine (AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Plugins");
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            if (!Directory.Exists(pluginPath))
            {
                Directory.CreateDirectory(pluginPath);
            }

            // This creates a ShadowCopy of the MEF DLL's 
            // (and any other DLL's in the ShadowCopyDirectories)
            var setup = new AppDomainSetup
            {
                CachePath = cachePath,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = pluginPath
            };

            // Create a new AppDomain then create a new instance 
            // of this application in the new AppDomain.            
            domain = AppDomain.CreateDomain("Host_AppDomain", AppDomain.CurrentDomain.Evidence, setup);
            var runner = (Runner)domain.CreateInstanceAndUnwrap (typeof(Runner).Assembly.FullName, typeof(Runner).FullName);

            Console.WriteLine("The main AppDomain is:    {0}", AppDomain.CurrentDomain.FriendlyName);

            // We now have access to all the methods and properties of Program.   
            runner.DoWorkInShadowCopiedDomain();
            runner.DoSomething();

            Console.WriteLine("\nHere you can remove a DLL from the Plugins folder.");
            Console.WriteLine("Press any key when ready...");
            Console.ReadKey();

            // After removing a DLL, we can now recompose the 
            // MEF parts and see that the removed DLL is no longer accessed.
            runner.Recompose();
            runner.DoSomething();
            Console.WriteLine("Press any key when ready...");
            Console.ReadKey();

            // Clean up.
            AppDomain.Unload(domain);

        }
    }
}