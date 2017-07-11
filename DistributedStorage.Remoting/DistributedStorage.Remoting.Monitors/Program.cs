/* 
 * File: Program.cs
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



using DistributedStorage.Remoting.Domains;
using DistributedStorage.Remoting.Infrastructures;
using DistributedStorage.Remoting.Monitors.Domains;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.PSharp;

namespace DistributedStorage.Remoting.Monitors
{
    class Program
    {
        static int Main(string[] args)
        {
            var container = new UnityContainer();
            var unityServiceLocator = new UnityServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => unityServiceLocator);

            var configuration = Configuration.Create();
            configuration.EnableMonitorsInProduction = true;
            var runtime = PSharpRuntime.Create(configuration);
            using (var networkProvider = new DomainCommunicationProvider(runtime, "monitors"))
            {
                runtime.SetNetworkProvider(networkProvider);

                container.RegisterInstance(runtime).
                          RegisterInstance("SafetyMonitor", typeof(SafetyMonitor)).
                          RegisterInstance("LivenessMonitor", typeof(LivenessMonitor)).
                          RegisterType<IProcessExecutor, ProcessExecutor>().
                          RegisterType<ApplicationConsole, MainMonitorsConsole>("DistributedStorage.Remoting.Monitors.MainMonitorsConsole");

                var console = container.Resolve<ApplicationConsole>("DistributedStorage.Remoting.Monitors.MainMonitorsConsole");
                return console.Run(args);
            }
        }
    }
}
