/* 
 * File: MainGuestViewModel.cs
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
using PingPong.Remoting.Domains;
using PingPong.Remoting.Infrastructures;
using PingPong.Remoting.Infrastructures.Mixins.System;
using Reactive.Bindings;
using System;
using System.Collections.Specialized;

namespace PingPong.Remoting.Guest
{
    class MainGuestViewModel : ApplicationViewModel
    {
        public ApplicationProperty<MachineId> ServerId { get; private set; } = new ApplicationProperty<MachineId>();

        MessageCollection m_data;
        public MessageCollection Data
        {
            get { return m_data; }
            set
            {
                DisplayData?.Dispose();
                SetProperty(ref m_data, value);
                DisplayData = value?.ToCollectionChanged().
                                     DelayUntilNextSchedule(_ => _.Action == NotifyCollectionChangedAction.Reset ? TimeSpan.Zero : TimeSpan.FromMilliseconds(500)).
                                     ToReadOnlyReactiveCollection(true);
            }
        }

        ReadOnlyReactiveCollection<Message> m_displayData;
        public ReadOnlyReactiveCollection<Message> DisplayData
        {
            get { return m_displayData; }
            private set { SetProperty(ref m_displayData, value); }
        }

        public ApplicationProperty<int> ExitCode { get; private set; } = new ApplicationProperty<int>(-1);

        ReactiveCommand m_loadedCommand;
        public ReactiveCommand LoadedCommand
        {
            get
            {
                if (m_loadedCommand == null)
                    m_loadedCommand = new LoadedCommand(this);
                return m_loadedCommand;
            }
        }
    }
}
