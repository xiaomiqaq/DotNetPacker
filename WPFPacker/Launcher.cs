using System;
using System.Windows;
using System.Reflection;

namespace WPFPacker
{
    public class Launcher : MarshalByRefObject
    {
        public static void Start(string pathToAssembly)
        {
            //AppDomainSetup setup = new AppDomainSetup
            //{
            //    ApplicationBase = Directory.CreateDirectory(pathToAssembly).Name,
            //    PrivateBinPath = @"E:\MySchoolProject\NetShield\NetShield_Protector-NetShield_1.3.6\Test\bin\Debug\Wind\x86"
            //};
            AppDomain appDomain = AppDomain.CreateDomain("Loading Domain");
            Launcher program = (Launcher)appDomain.CreateInstanceAndUnwrap(typeof(Launcher).Assembly.FullName, typeof(Launcher).FullName);
            //appDomain.AssemblyResolve += OnLoadDomainAssemblyResolve;

            program.Execute(pathToAssembly);
            
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}

            AppDomain.Unload(appDomain);
        }

        private void Execute(string pathToAssembly)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(pathToAssembly);
            Assembly assembly = Assembly.Load(bytes);
            MethodInfo m = assembly.EntryPoint;
            //指定内嵌资源的程序集
            Application.ResourceAssembly = assembly;
            var parameters = m.GetParameters().Length == 0 ? null : new[] { new string[0] };
            m.Invoke(null, parameters);
        }
        private static Assembly OnLoadDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Name))
            {
                return null;
            }
            string[] assemblyNameSplits = args.Name.Split(',');
            if (assemblyNameSplits.Length == 0)
            {
                return null;
            }
            string name = assemblyNameSplits[0];
            string name2 = assemblyNameSplits[2];
            if (name.StartsWith("CefSharp") && name2.EndsWith("neutral"))
            {
                Assembly.LoadFrom(@"E:\Debug2\CefSharp.Wpf.dll");
                Assembly.LoadFrom(@"E:\Debug2\CefSharp.Core.dll");
                return Assembly.LoadFrom(@"E:\Debug2\CefSharp.dll");

            }
            else
            {
                return AppDomain.CurrentDomain.Load(name);
            }

        }
    }
    

}
