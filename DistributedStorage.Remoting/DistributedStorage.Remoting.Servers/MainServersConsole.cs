/* 
 * File: MainServersConsole.cs
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
using Microsoft.Practices.Unity;
using Reactive.Bindings;
using System;
using System.Collections.Specialized;

namespace DistributedStorage.Remoting.Servers
{
    class MainServersConsole : ApplicationConsole
    {
        public MainServersConsole()
        {
            ViewModel = new MainServersViewModel();
        }

        protected override ApplicationViewModel ApplicationViewModel => ViewModel;

        public MainServersViewModel ViewModel { get; private set; }

        protected override void InitializeCore(IUnityContainer container)
        {
            RegisterController<MainServersController>();
        }

        protected override int RunCore(params string[] args)
        {
            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.DisplayMessages))
                {
                    ViewModel.DisplayMessages.ToCollectionChanged().Subscribe(cc =>
                    {
                        if (cc.Action == NotifyCollectionChangedAction.Add)
                        {
                            Console.WriteLine(cc.Value);
                        }
                    });
                }
            };

            ViewModel.LoadedCommand.Execute(args);

            Console.WriteLine("Press Enter to terminate...");
            Console.ReadLine();

            return ViewModel.ExitCode.Value;
        }
    }
}
