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
using PingPong.Mvvm.Infrastructures;

namespace PingPong.Mvvm.Domains
{
    class Server : ApplicationMachine
    {
        MessageCollection m_data;

        [Dependency]
        public IMessageRepository MessageRepository { private get; set; }

        public class Configure : Event
        {
            public MessageCollection Data { get; private set; }

            public Configure(MessageCollection data)
            {
                Data = data;
            }
        }

        public class Pong : Event { }

        public class Complete : Event
        {
            public int MessageId { get; private set; }

            public Complete(int messageId)
            {
                MessageId = messageId;
            }
        }

        public class Reset : Event { }

        [Start]
        [OnEntry(nameof(EntryInitialized))]
        class Initialized : MachineState { }

        protected virtual void EntryInitialized()
        {
            m_data = (ReceivedEvent as Configure).Data;
            Goto(typeof(Active));
        }

        [OnEventDoAction(typeof(Client.Ping), nameof(HandlePing))]
        [OnEventDoAction(typeof(Reset), nameof(HandleReset))]
        [OnEventGotoState(typeof(Client.Complete), typeof(Exit))]
        class Active : MachineState { }

        protected virtual void HandlePing()
        {
            var clientId = (ReceivedEvent as Client.Ping).ClientId;
            var messageId = (ReceivedEvent as Client.Ping).MessageId;
            var message = MessageRepository.FindOne(messageId);
            m_data.Add(message);
            Send(clientId, new Pong());
        }

        protected virtual void HandleReset()
        {
            m_data.Clear();
        }

        [OnEntry(nameof(EntryExit))]
        [IgnoreEvents(typeof(Reset))]
        class Exit : MachineState { }

        protected virtual void EntryExit()
        {
            var clientId = (ReceivedEvent as Client.Complete).ClientId;
            var messageId = (ReceivedEvent as Client.Complete).MessageId;
            Send(clientId, new Complete(messageId));
        }
    }
}
