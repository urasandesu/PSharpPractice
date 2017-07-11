﻿/* 
 * File: ApplicationMachine.cs
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



using DistributedStorage.Remoting.Infrastructures.Mixins.Microsoft.PSharp;
using DistributedStorage.Remoting.Infrastructures.Mixins.Microsoft.PSharp.Net;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.PSharp;

namespace DistributedStorage.Remoting.Infrastructures
{
    public abstract class ApplicationMachine : Machine
    {
        [Dependency]
        public PSharpRuntime Runtime { protected get; set; }

        protected ApplicationMachine()
        {
            var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
            container.BuildUp(GetType(), this);
        }

        protected void RemoteMonitor(MonitorId target, Event e)
        {
            {
                var networkProvider2 = Runtime.NetworkProvider as INetworkProvider2;
                if (networkProvider2 != null)
                {
                    networkProvider2.RemoteMonitor(target, e);
                    return;
                }
            }

            Runtime.InvokeMonitor(target, e);
        }
    }
}
