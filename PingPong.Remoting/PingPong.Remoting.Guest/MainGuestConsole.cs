/* 
 * File: MainGuestConsole.cs
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
using PingPong.Remoting.Infrastructures;
using PingPong.Remoting.Infrastructures.Mixins.Microsoft.PSharp;
using Reactive.Bindings;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace PingPong.Remoting.Guest
{
    class MainGuestConsole : ApplicationConsole
    {
        public MainGuestConsole()
        {
            ViewModel = new MainGuestViewModel();
        }

        protected override ApplicationViewModel ApplicationViewModel { get { return ViewModel; } }

        public MainGuestViewModel ViewModel { get; private set; }

        protected override void InitializeCore(IUnityContainer container)
        {
            RegisterController<MainGuestController>();
        }

        protected override int RunCore(params string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.DisplayData))
                {
                    ViewModel.DisplayData.ToCollectionChanged().Subscribe(cc =>
                    {
                        if (cc.Action == NotifyCollectionChangedAction.Add)
                        {
                            Console.WriteLine(cc.Value);
                        }
                        else if (cc.Action == NotifyCollectionChangedAction.Reset)
                        {
                            Console.Clear();
                        }
                    });
                }
            };

            ViewModel.ServerId.Value = MachineIdMixin.FromJson(args[0]);

            ViewModel.LoadedCommand.Execute(this);

            var waitHandle = new ManualResetEventSlim(false);
            ViewModel.ExitCode.Skip(1).Subscribe(exitCode =>
            {
                Console.WriteLine("Press enter to exit . . .");
                Console.ReadLine();
                waitHandle.Set();
            });
            waitHandle.Wait();

            return ViewModel.ExitCode.Value;
        }
    }
}
