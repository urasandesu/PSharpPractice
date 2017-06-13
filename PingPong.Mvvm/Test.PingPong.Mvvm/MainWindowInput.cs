﻿/* 
 * File: MainWindowInput.cs
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



using Microsoft.PSharp;
using PingPong.Mvvm.Domains;
using PingPong.Mvvm.Infrastructures;

namespace Test.PingPong.Mvvm
{
    class MainWindowInput : ApplicationMachine
    {
        MachineId m_serverId;

        public class Configure : Event
        {
            public MachineId ServerId { get; private set; }

            public Configure(MachineId serverId)
            {
                ServerId = serverId;
            }
        }

        public class StartLoop : Event { }
        public class Tick : Event { }

        [Start]
        [OnEntry(nameof(EntryInitialized))]
        class Initialized : MachineState { }

        void EntryInitialized()
        {
            m_serverId = (ReceivedEvent as Configure).ServerId;
            Goto(typeof(Loop));
        }

        [OnEntry(nameof(EntryLoop))]
        [OnEventGotoState(typeof(Tick), typeof(Elapsed))]
        class Loop : MachineState { }

        protected virtual void EntryLoop()
        {
            Send(Id, new Tick());
        }

        [OnEntry(nameof(EntryElapsed))]
        [OnEventGotoState(typeof(StartLoop), typeof(Loop))]
        class Elapsed : MachineState { }

        protected virtual void EntryElapsed()
        {
            if (Random())
                Send(m_serverId, new Server.Reset());
            else
                Send(Id, new StartLoop());
        }
    }
}