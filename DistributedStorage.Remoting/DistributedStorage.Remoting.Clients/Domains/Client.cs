/* 
 * File: Client.cs
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
using Microsoft.PSharp;

namespace DistributedStorage.Remoting.Clients.Domains
{
    class Client : ApplicationMachine
    {
        public class Configure : Event
        {
            public MessageCollection Messages { get; private set; }
            public MachineId ServerId { get; private set; }

            public Configure(MessageCollection messages, MachineId serverId)
            {
                Messages = messages;
                ServerId = serverId;
            }
        }

        public class Unit : Event { }

        MachineId m_serverId;
        MessageCollection m_messages;

        [Start]
        [OnEventDoAction(typeof(Configure), nameof(HandleConfigure))]
        class Initialized : MachineState { }

        protected virtual void HandleConfigure()
        {
            var configure = (Configure)ReceivedEvent;
            m_messages = configure.Messages;
            m_serverId = configure.ServerId;
            Goto(typeof(Active));
        }

        [OnEntry(nameof(EntryActive))]
        [OnEventGotoState(typeof(Ack), typeof(Exit))]
        class Active : MachineState { }

        protected virtual void EntryActive()
        {
            var dataToReplicate = RandomInteger(43);
            lock (m_messages)
                m_messages.Add(new Message<Unit>() { Id = Id, Event = new Unit(), Value = $"client: { Id }, data to replicate: { dataToReplicate }" });
            RemoteSend(m_serverId, new ClientReq(dataToReplicate));
        }

        [OnEntry(nameof(EntryExit))]
        [OnEventGotoState(typeof(Unit), typeof(Active))]
        class Exit : MachineState { }

        protected virtual void EntryExit()
        {
            if (Random())
            {
                lock (m_messages)
                    m_messages.Add(new Message<Unit>() { Id = Id, Event = new Unit(), Value = $"halt client: { Id }" });
                Raise(new Halt());
            }
            else
            {
                Raise(new Unit());
            }
        }
    }
}
