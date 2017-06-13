/* 
 * File: WpfApplicationWindow.cs
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
using System;
using System.Windows;

namespace PingPong.Mvvm.Infrastructures
{
    public abstract class WpfApplicationWindow : Window
    {
        protected abstract WpfApplicationViewModel ApplicationViewModel { get; }

        public IUnityContainer Container { get; private set; }

        public void RegisterController<TController>() where TController : ApplicationController
        {
            Container.RegisterType<TController, TController>();
        }

        [InjectionMethod]
        public void Initialize(IUnityContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            Container = container.CreateChildContainer();
            Container.BuildUp(ApplicationViewModel);
            Closed += MainWindow_Closed;

            InitializeCore(container);
        }

        protected abstract void InitializeCore(IUnityContainer container);

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Container?.Dispose();
        }
    }
}
