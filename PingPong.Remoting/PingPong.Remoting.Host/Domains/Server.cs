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



using Microsoft.Practices.Unity;
using Microsoft.PSharp;
using PingPong.Remoting.Domains;
using PingPong.Remoting.Infrastructures;
using System.Collections.Concurrent;

namespace PingPong.Remoting.Host.Domains
{
    class Server : ApplicationMachine
    {
        MessageCollection m_data;
        readonly ConcurrentDictionary<MachineId, bool> m_clientIds = new ConcurrentDictionary<MachineId, bool>();

        [Dependency]
        public IMessageRepository MessageRepository { private get; set; }

        [Start]
        [OnEntry(nameof(EntryInitialized))]
        class Initialized : MachineState { }

        protected virtual void EntryInitialized()
        {
            m_data = (ReceivedEvent as Configure).Data;
            Goto(typeof(Active));
        }

        [OnEventDoAction(typeof(Ping), nameof(HandlePing))]
        [OnEventDoAction(typeof(Reset), nameof(HandleReset))]
        [OnEventGotoState(typeof(Complete), typeof(Exit))]
        class Active : MachineState { }

        protected virtual void HandlePing()
        {
            var clientId = (ReceivedEvent as Ping).ClientId;
            var messageId = (ReceivedEvent as Ping).MessageId;
            m_clientIds.TryAdd(clientId, false);
            var message = MessageRepository.FindOne(messageId);
            m_data.Add(message);
            RemoteSend(clientId, new Pong());
        }

        protected virtual void HandleReset()
        {
            m_data.Clear();
            var clientIds = m_clientIds.Keys;
            foreach (var clientId in clientIds)
                RemoteSend(clientId, new Reset());
        }

        [OnEntry(nameof(EntryExit))]
        [IgnoreEvents(typeof(Reset))]
        class Exit : MachineState { }

        protected virtual void EntryExit()
        {
            var clientId = (ReceivedEvent as Complete).ClientId;
            var messageId = (ReceivedEvent as Complete).MessageId;
            RemoteSend(clientId, new Complete(Id, messageId));
        }
    }
}
