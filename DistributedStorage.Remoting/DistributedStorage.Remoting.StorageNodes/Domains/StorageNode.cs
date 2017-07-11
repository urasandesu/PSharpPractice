/* 
 * File: StorageNode.cs
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
using DistributedStorage.Remoting.Infrastructures.Mixins.Microsoft.PSharp;
using Microsoft.PSharp;
using System;

namespace DistributedStorage.Remoting.StorageNodes.Domains
{
    class StorageNode : ApplicationMachine
    {
        public class Configure : Event
        {
            public MessageCollection Messages { get; private set; }
            public MonitorId SafetyMonitorId { get; private set; }

            public Configure(MessageCollection messages, MonitorId safetyMonitorId)
            {
                Messages = messages;
                SafetyMonitorId = safetyMonitorId;
            }
        }

        MessageCollection m_messages;
        MonitorId m_safetyMonitorId;
        MachineId m_serverId;
        long m_log = -1;

        [Start]
        [OnEventDoAction(typeof(Configure), nameof(HandleConfigure))]
        class Initialized : MachineState { }

        protected virtual void HandleConfigure()
        {
            var configure = (Configure)ReceivedEvent;
            m_messages = configure.Messages;
            m_safetyMonitorId = configure.SafetyMonitorId;
            Goto(typeof(Established));
        }

        [OnEventDoAction(typeof(Handshake), nameof(HandleHandshake))]
        class Established : MachineState { }

        protected virtual void HandleHandshake()
        {
            var handshake = (Handshake)ReceivedEvent;
            m_serverId = handshake.DestinationId;
            Goto(typeof(Active));
        }

        [OnEventDoAction(typeof(ReplReq), nameof(HandleReplReq))]
        [OnEventDoAction(typeof(Timer.Timeout), nameof(HandleTimeout))]
        class Active : MachineState { }

        protected virtual void HandleReplReq()
        {
            var log = DateTime.Now.Ticks;
            var replReq = (ReplReq)ReceivedEvent;
            var data = replReq.Data;
            RemoteMonitor(m_safetyMonitorId, new LogUpdated(Id, log));
            lock (m_messages)
                m_messages.Add(new Message<ReplReq>() { Id = Id, Event = replReq, Value = $"storage node: { Id }, data: { data }, log: { log }" });
            m_log = log;
        }

        protected virtual void HandleTimeout()
        {
            var timeout = (Timer.Timeout)ReceivedEvent;
            lock (m_messages)
                m_messages.Add(new Message<Timer.Timeout>() { Id = Id, Event = timeout, Value = $"storage node: { Id }, log: { m_log }" });
            RemoteSend(m_serverId, new Sync(Id, m_log));
        }
    }
}
