/* 
 * File: SystematicTest.cs
 * 
 * Author: Akira Sugiura (urasandesu@gmail.com)
 * 
 * 
 * Copyright (c) 2017 Akira Sugiura
 *  
 *  This software is MIT License.
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */



using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.PSharp;
using PingPong.Mvvm.Domains;
using System;
using System.IO;
using System.Reflection;
using Test.PingPong.Mvvm.Models;
using Test.PingPong.Mvvm.Repositories;

namespace Test.PingPong.Mvvm
{
    public static class SystematicTest
    {
        static SystematicTest()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var asmName = new AssemblyName(args.Name);
                if (!asmName.Name.Contains("System.Reactive") && !asmName.Name.Contains("Microsoft.Practices.ServiceLocation"))
                    return null;

                var callingAsm = Assembly.GetCallingAssembly();
                var callingDir = Path.GetDirectoryName(callingAsm.Location);
                return Assembly.LoadFrom(Path.Combine(callingDir, asmName.Name + ".dll"));
            };
        }

        [Microsoft.PSharp.Test]
        public static void Execute(PSharpRuntime runtime)
        {
            var container = new UnityContainer();
            var unityServiceLocator = new UnityServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => unityServiceLocator);

            container.RegisterInstance(runtime).
                      RegisterType<IMessageRepository, MessageRepositoryMock>(new ContainerControlledLifetimeManager());

            var data = new MessageCollection();
            var serverId = runtime.CreateMachine(typeof(Server), new Server.Configure(data));
            var clientId = runtime.CreateMachine(typeof(ClientMock), new Client.Configure(data, serverId));
            runtime.CreateMachine(typeof(MainWindowInput), new MainWindowInput.Configure(serverId));
        }
    }
}
