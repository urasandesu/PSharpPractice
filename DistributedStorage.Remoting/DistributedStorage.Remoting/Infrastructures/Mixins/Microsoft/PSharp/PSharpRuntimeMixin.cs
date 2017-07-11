/* 
 * File: PSharpRuntimeMixin.cs
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



using DistributedStorage.Remoting.Infrastructures.Mixins.Microsoft.PSharp.Net;
using Microsoft.PSharp;
using System;

namespace DistributedStorage.Remoting.Infrastructures.Mixins.Microsoft.PSharp
{
    public static class PSharpRuntimeMixin
    {
        public static void InvokeMonitor(this PSharpRuntime @this, MonitorId target, Event e)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var t = Type.GetType(target.AssemblyQualifiedName);
            var mi = typeof(PSharpRuntime).GetMethod("InvokeMonitor");
            mi.MakeGenericMethod(t).Invoke(@this, new object[] { e });
        }

        public static void RemoteInvokeMonitor(this PSharpRuntime @this, MonitorId target, Event e)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            {
                var networkProvider2 = @this.NetworkProvider as INetworkProvider2;
                if (networkProvider2 != null)
                {
                    networkProvider2.RemoteMonitor(target, e);
                    return;
                }
            }

            @this.InvokeMonitor(target, e);
        }

        public static MachineId NewMachine(this PSharpRuntime @this, Type type, Event e = null)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var machineId = @this.CreateMachine(type);
            @this.SendEvent(machineId, e);
            return machineId;
        }

        public static MonitorId NewMonitor(this PSharpRuntime @this, Type type, Event e = null)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var monitorId = new MonitorId(@this, type);
            @this.RegisterMonitor(type);
            if (e != null)
                @this.InvokeMonitor(monitorId, e);
            return monitorId;
        }
    }
}
