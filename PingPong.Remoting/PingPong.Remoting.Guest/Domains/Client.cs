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



using Microsoft.Practices.Unity;
using Microsoft.PSharp;
using PingPong.Remoting.Domains;
using PingPong.Remoting.Infrastructures;

namespace PingPong.Remoting.Guest.Domains
{
    class Client : ApplicationMachine
    {
        MachineId m_serverId;
        MessageCollection m_data;

        [Dependency]
        public IMessageRepository MessageRepository { private get; set; }

        [Start]
        [OnEntry(nameof(EntryInitialized))]
        class Initialized : MachineState { }

        protected virtual void EntryInitialized()
        {
            m_serverId = (ReceivedEvent as Configure).ServerId;
            m_data = (ReceivedEvent as Configure).Data;
            Goto(typeof(Active));
        }

        [OnEntry(nameof(EntryActive))]
        [OnEventDoAction(typeof(Pong), nameof(HandlePong))]
        [OnEventDoAction(typeof(Reset), nameof(HandleReset))]
        [OnEventGotoState(typeof(Complete), typeof(Exit))]
        class Active : MachineState { }

        protected virtual void EntryActive()
        {
            HandlePong();
        }

        protected virtual void HandlePong()
        {
            var message = MessageRepository.NextOne();
            if (40 <= message.Id)
            {
                RemoteSend(m_serverId, new Complete(Id, message.Id));
            }
            else
            {
                MessageRepository.Store(message);
                m_data.Add(message);

                message = MessageRepository.NextOne();
                MessageRepository.Store(message);
                RemoteSend(m_serverId, new Ping(Id, message.Id));
            }
        }

        protected virtual void HandleReset()
        {
            m_data.Clear();
        }

        [OnEntry(nameof(EntryExit))]
        class Exit : MachineState { }

        protected virtual void EntryExit()
        {
            Raise(new Halt());
            m_data.RaiseExit();
        }
    }
}
