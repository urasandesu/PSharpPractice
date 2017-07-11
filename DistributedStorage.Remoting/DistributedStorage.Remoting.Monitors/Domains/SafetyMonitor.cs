/* 
 * File: SafetyMonitor.cs
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
using Microsoft.PSharp;
using System.Collections.Generic;
using System.Linq;

namespace DistributedStorage.Remoting.Monitors.Domains
{
    class SafetyMonitor : Monitor
    {
        public class Configure : Event
        {
            public MessageCollection Messages { get; private set; }

            public Configure(MessageCollection messages)
            {
                Messages = messages;
            }
        }

        MessageCollection m_messages;
        Dictionary<MachineId, bool> m_replicas;

        [Start]
        [OnEventDoAction(typeof(Configure), nameof(HandleConfigure))]
        [DeferEvents(typeof(Handshake), typeof(LogUpdated), typeof(Ack))]
        class Initialized : MonitorState { }

        protected virtual void HandleConfigure()
        {
            var configure = (Configure)ReceivedEvent;
            m_messages = configure.Messages;
            Goto(typeof(Established));
        }

        [OnEventDoAction(typeof(Handshake), nameof(HandleHandshake))]
        [DeferEvents(typeof(LogUpdated), typeof(Ack))]
        class Established : MonitorState { }

        protected virtual void HandleHandshake()
        {
            var handshake = (Handshake)ReceivedEvent;
            var snIds = handshake.StorageNodeIds;
            m_replicas = new Dictionary<MachineId, bool>();
            foreach (var snId in snIds)
                m_replicas.Add(snId, false);
            Goto(typeof(Checking));
        }

        [OnEventDoAction(typeof(LogUpdated), nameof(HandleLogUpdated))]
        [OnEventDoAction(typeof(Ack), nameof(HandleAck))]
        class Checking : MonitorState { }

        protected virtual void HandleLogUpdated()
        {
            var logUpdated = (LogUpdated)ReceivedEvent;
            var snId = logUpdated.StorageNodeId;
            var log = logUpdated.Log;
            m_replicas[snId] = true;
            lock (m_messages)
                m_messages.Add(new Message<LogUpdated>() { Id = Id, Event = logUpdated, Value = $"storage node: { snId }, log: { log }" });
        }

        protected virtual void HandleAck()
        {
            lock (m_messages)
                m_messages.Add(new Message<Ack>() { Id = Id, Event = new Ack(), Value = $"ack" });
            Assert(m_replicas.All(_ => _.Value));
            foreach (var snId in m_replicas.Keys.ToArray())
                m_replicas[snId] = false;
            //Goto(typeof(Reinitialized));
        }

        //[OnEntry(nameof(EntryReinitialized))]
        //class Reinitialized : MonitorState { }

        //protected virtual void EntryReinitialized()
        //{
        //    foreach (var snId in m_replicas.Keys.ToArray())
        //        m_replicas[snId] = false;
        //    Goto(typeof(Checking));
        //}
    }
}
