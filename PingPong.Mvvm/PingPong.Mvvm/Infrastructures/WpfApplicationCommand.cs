/* 
 * File: WpfApplicationCommand.cs
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



using Reactive.Bindings;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace PingPong.Mvvm.Infrastructures
{
    public abstract class WpfApplicationCommand : ReactiveCommand
    {
        protected WpfApplicationViewModel ApplicationViewModel { get; private set; }

        public TController ResolveController<TController>() where TController : ApplicationController
        {
            return ApplicationViewModel.ResolveController<TController>();
        }

        protected WpfApplicationCommand(WpfApplicationViewModel vm) :
            base(DispatcherScheduler.Current)
        {
            ApplicationViewModel = vm ?? throw new ArgumentNullException(nameof(vm));
            this.Subscribe(Invoke);
        }

        void Invoke(object parameter)
        {
            InvokeCore(parameter);
        }

        protected abstract void InvokeCore(object parameter);
    }
}
