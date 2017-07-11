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



using DistributedStorage.Remoting.Clients;
using DistributedStorage.Remoting.Clients.Domains;
using DistributedStorage.Remoting.Domains;
using DistributedStorage.Remoting.Infrastructures;
using DistributedStorage.Remoting.Monitors;
using DistributedStorage.Remoting.Monitors.Domains;
using DistributedStorage.Remoting.Servers;
using DistributedStorage.Remoting.Servers.Domains;
using DistributedStorage.Remoting.StorageNodes;
using DistributedStorage.Remoting.StorageNodes.Domains;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.PSharp;
using System;
using System.IO;
using System.Reflection;
using Test.DistributedStorage.Remoting.Models;

namespace Test.DistributedStorage.Remoting
{
    public class SystematicTest
    {
        static SystematicTest()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var asmName = new AssemblyName(args.Name);
                if (!asmName.Name.Contains("Microsoft.Practices.ServiceLocation"))
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
                      RegisterInstance("SafetyMonitor", typeof(SafetyMonitor)).
                      RegisterInstance("LivenessMonitor", typeof(LivenessMonitor)).
                      RegisterInstance("Server", typeof(Server)).
                      RegisterInstance("StorageNode", typeof(StorageNode)).
                      RegisterInstance("Timer", typeof(TimerMock)).
                      RegisterInstance("Client", typeof(Client)).
                      RegisterType<IProcessExecutor, ProcessExecutorMock>();

            var ctx = new DistributedStorageContext();
            var messages = new MessageCollection();


            container.Resolve<MainMonitorsController>().NewMonitors(ctx, messages);
            container.Resolve<MainServersController>().NewServer(ctx, messages);
            container.Resolve<MainStorageNodesController>().NewStorageNodes(ctx, messages);
            container.Resolve<MainClientsController>().NewClient(ctx, messages);
        }
    }
}
