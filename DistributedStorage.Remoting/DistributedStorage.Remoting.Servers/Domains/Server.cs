/* 
 * File: Server.cs
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



//#define FIXES_SAFETY_BUG
//#define FIXES_LIVENESS_BUG
using DistributedStorage.Remoting.Domains;
using DistributedStorage.Remoting.Infrastructures;
using DistributedStorage.Remoting.Infrastructures.Mixins.Microsoft.PSharp;
using Microsoft.PSharp;
using System;
#if FIXES_SAFETY_BUG
using System.Collections.Generic;
#endif

namespace DistributedStorage.Remoting.Servers.Domains
{
    class Server : ApplicationMachine
    {
        public class Configure : Event
        {
            public MessageCollection Data { get; private set; }
            public MonitorId SafetyMonitorId { get; private set; }
            public MonitorId LivenessMonitorId { get; private set; }

            public Configure(MessageCollection data, MonitorId safetyMonitorId, MonitorId livenessMonitorId)
            {
                Data = data;
                SafetyMonitorId = safetyMonitorId;
                LivenessMonitorId = livenessMonitorId;
            }
        }

        MessageCollection m_data;
        MonitorId m_safetyMonitorId;
        MonitorId m_livenessMonitorId;

        MachineId m_clientId;
        MachineId[] m_snIds;
        int m_dataToReplicate;
        long m_log;
#if FIXES_SAFETY_BUG
        HashSet<MachineId> m_numReplicas = new HashSet<MachineId>();
#else
        int m_numReplicas;
#endif

        [Start]
        [OnEventDoAction(typeof(Configure), nameof(HandleConfigure))]
        [DeferEvents(typeof(Handshake), typeof(ClientReq), typeof(Sync))]
        class Initialized : MachineState { }

        protected virtual void HandleConfigure()
        {
            var configure = (Configure)ReceivedEvent;
            m_data = configure.Data;
            m_safetyMonitorId = configure.SafetyMonitorId;
            m_livenessMonitorId = configure.LivenessMonitorId;
            Goto(typeof(Established));
        }

        [OnEventDoAction(typeof(Handshake), nameof(HandleHandshake))]
        [DeferEvents(typeof(ClientReq), typeof(Sync))]
        class Established : MachineState { }

        protected virtual void HandleHandshake()
        {
            m_clientId = (ReceivedEvent as Handshake).DestinationId;
            m_snIds = (ReceivedEvent as Handshake).StorageNodeIds;
            Goto(typeof(Active));
        }

        [OnEventDoAction(typeof(ClientReq), nameof(HandleClientReq))]
        [OnEventDoAction(typeof(Sync), nameof(HandleSync))]
        class Active : MachineState { }

        protected virtual void HandleClientReq()
        {
            RemoteMonitor(m_livenessMonitorId, ReceivedEvent);
            var clientReq = (ClientReq)ReceivedEvent;
            m_dataToReplicate = clientReq.DataToReplicate;
            foreach (var snId in m_snIds)
                RemoteSend(snId, new ReplReq(m_dataToReplicate));
        }

        protected virtual void HandleSync()
        {
            var sync = (Sync)ReceivedEvent;
            var snId = sync.StorageNodeId;
            var log = sync.Log;
            if (!IsUpToDate(log))
            {
                m_data.Add(new Message<Sync>() { Id = Id, Event = sync, Value = $"request replication again: { snId }, data: { m_dataToReplicate }" });
                RemoteSend(snId, new ReplReq(m_dataToReplicate));
            }
            else
            {
#if FIXES_SAFETY_BUG
                if (!m_numReplicas.Add(snId))
                    return;

                if (m_numReplicas.Count == 3)
#else
                m_numReplicas++;
                if (m_numReplicas == 3)
#endif
                {
                    RemoteMonitor(m_safetyMonitorId, new Ack());
                    RemoteMonitor(m_livenessMonitorId, new Ack());
                    RemoteSend(m_clientId, new Ack());
                    m_log = DateTime.Now.Ticks;
                    m_data.Add(new Message<Sync>() { Id = Id, Event = sync, Value = $"response ack: { m_clientId }, log: { m_log }" });
#if FIXES_LIVENESS_BUG
#if FIXES_SAFETY_BUG
                    m_numReplicas.Clear();
#else
                    m_numReplicas = 0;
#endif
#endif
                }
            }
        }

        bool IsUpToDate(long log)
        {
            if (log <= m_log)
                return false;

            m_log = log;
            return true;
        }
    }
}
