using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro;

namespace PastySharpClient
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }

        private readonly string[] _resourceNames;

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dll = args.Name.Substring(0, args.Name.IndexOf(','));

            if (dll.EndsWith("resources")) return null;

            byte[] bytes = null;

            foreach (var name in _resourceNames)
            {
                if (name.EndsWith(".dlls." + dll + ".dll"))
                {
                    using (var ms = new MemoryStream())
                    {
                        var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                        if (manifestResourceStream == null) return null;

                        manifestResourceStream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                }
            }

            return Assembly.Load(bytes);
        }
    }
}
