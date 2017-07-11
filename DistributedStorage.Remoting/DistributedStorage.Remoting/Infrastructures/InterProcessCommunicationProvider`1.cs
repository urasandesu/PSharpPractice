/* 
 * File: InterProcessCommunicationProvider`1.cs
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
using Microsoft.PSharp;
using Microsoft.PSharp.Net;
using System;
using System.Collections.Concurrent;
using System.ServiceModel;

namespace DistributedStorage.Remoting.Infrastructures
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public abstract class InterProcessCommunicationProvider<TRemotePSharpRuntime> : INetworkProvider2, IRemotePSharpRuntime
        where TRemotePSharpRuntime : IRemotePSharpRuntime
    {
        readonly PSharpRuntime m_runtime;
        readonly InterProcessCommunicationSetting m_setting;

        readonly ConcurrentDictionary<string, TRemotePSharpRuntime> m_channel = new ConcurrentDictionary<string, TRemotePSharpRuntime>();
        TRemotePSharpRuntime GetOrAddChannel(string endpointUri)
        {
            return m_channel.GetOrAdd(endpointUri, CreateChannel);
        }

        TRemotePSharpRuntime CreateChannel(string endpointUri)
        {
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            var endpointAddress = new EndpointAddress(endpointUri);
            return ChannelFactory<TRemotePSharpRuntime>.CreateChannel(binding, endpointAddress);
        }

        ServiceHost m_serviceHost;
        ServiceHost ServiceHost
        {
            get
            {
                if (m_serviceHost == null)
                    m_serviceHost = CreateServiceHost(m_setting.LocalEndpointUri);
                return m_serviceHost;
            }
        }

        ServiceHost CreateServiceHost(string serviceHostUri)
        {
            var serviceHost = new ServiceHost(this);
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            serviceHost.AddServiceEndpoint(typeof(TRemotePSharpRuntime), binding, serviceHostUri);
            return serviceHost;
        }

        protected InterProcessCommunicationProvider(PSharpRuntime runtime, string localEndpointName) :
            this(runtime, new InterProcessCommunicationSetting(localEndpointName))
        {
        }

        protected InterProcessCommunicationProvider(PSharpRuntime runtime, InterProcessCommunicationSetting setting)
        {
            m_runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            m_setting = setting ?? throw new ArgumentNullException(nameof(setting));

            ServiceHost.Open();
        }

        MachineId INetworkProvider.RemoteCreateMachine(Type type, string friendlyName, string endpoint, Event e)
        {
            throw new NotSupportedException();
        }

        void INetworkProvider.RemoteSend(MachineId target, Event e)
        {
            GetOrAddChannel(target.Endpoint).SendEvent(target, e);
        }

        string INetworkProvider.GetLocalEndpoint()
        {
            return m_setting.LocalEndpointUri;
        }

        void INetworkProvider2.RemoteMonitor(MonitorId target, Event e)
        {
            GetOrAddChannel(target.Endpoint).Monitor(target, e);
        }

        void IRemotePSharpRuntime.SendEvent(MachineId target, Event e)
        {
            m_runtime.SendEvent(target, e);
        }

        void IRemotePSharpRuntime.Monitor(MonitorId target, Event e)
        {
            m_runtime.InvokeMonitor(target, e);
        }

        bool m_disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // Free any other managed objects here. 
                    //
                    m_serviceHost?.Close();
                }

                // Free any unmanaged objects here. 
                //
                m_disposed = true;
            }
        }
    }
}
