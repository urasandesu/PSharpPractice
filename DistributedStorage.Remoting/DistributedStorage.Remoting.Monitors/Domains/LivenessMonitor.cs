/* 
 * File: LivenessMonitor.cs
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

namespace DistributedStorage.Remoting.Monitors.Domains
{
    class LivenessMonitor : Monitor
    {
        public class Unit : Event { }

        [Start]
        [Hot]
        [OnEventDoAction(typeof(Ack), nameof(HandleAck))]
        [OnEventGotoState(typeof(Unit), typeof(Progressed))]
        [IgnoreEvents(typeof(ClientReq))]
        class Progressing : MonitorState { }

        protected virtual void HandleAck()
        {
            Raise(new Unit());
        }

        [Cold]
        [OnEventDoAction(typeof(ClientReq), nameof(HandleClientReq))]
        [OnEventGotoState(typeof(Unit), typeof(Progressing))]
        [IgnoreEvents(typeof(Ack))]
        class Progressed : MonitorState { }

        protected virtual void HandleClientReq()
        {
            Raise(new Unit());
        }
    }
}
