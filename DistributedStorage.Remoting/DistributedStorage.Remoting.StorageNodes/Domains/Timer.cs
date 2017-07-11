/* 
 * File: Timer.cs
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



using DistributedStorage.Remoting.Infrastructures;
using Microsoft.PSharp;

namespace DistributedStorage.Remoting.StorageNodes.Domains
{
    class Timer : ApplicationMachine
    {
        public class Configure : Event
        {
            public MachineId StorageNodeId { get; private set; }

            public Configure(MachineId snId)
            {
                StorageNodeId = snId;
            }
        }

        public class Timeout : Event
        {
        }

        public class Tick : Event
        {
        }

        public class StartLoop : Event
        {
        }

        MachineId m_snId;

        [Start]
        [OnEventDoAction(typeof(Configure), nameof(HandleConfigure))]
        class Initialized : MachineState { }

        void HandleConfigure()
        {
            m_snId = (ReceivedEvent as Configure).StorageNodeId;
            Goto(typeof(Loop));
        }

        [OnEntry(nameof(EntryLoop))]
        [OnEventGotoState(typeof(Tick), typeof(Elapsed))]
        class Loop : MachineState { }

        protected virtual void EntryLoop()
        {
            System.Threading.Thread.Sleep(1);
            Send(Id, new Tick());
        }

        [OnEntry(nameof(EntryElapsed))]
        [OnEventGotoState(typeof(StartLoop), typeof(Loop))]
        class Elapsed : MachineState { }

        protected virtual void EntryElapsed()
        {
            if (Random())
                Send(m_snId, new Timeout());

            Send(Id, new StartLoop());
        }
    }
}
