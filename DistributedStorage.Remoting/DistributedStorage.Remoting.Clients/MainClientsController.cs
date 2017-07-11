/* 
 * File: MainClientsController.cs
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



using DistributedStorage.Remoting.Clients.Domains;
using DistributedStorage.Remoting.Domains;
using DistributedStorage.Remoting.Infrastructures;
using DistributedStorage.Remoting.Infrastructures.Mixins.Microsoft.PSharp;
using DistributedStorage.Remoting.Infrastructures.Mixins.System;
using Microsoft.Practices.Unity;
using System;

namespace DistributedStorage.Remoting.Clients
{
    class MainClientsController : ApplicationController
    {
        [Dependency]
        public IProcessExecutor ProcessExecutor { private get; set; }

        [Dependency("Client")]
        public Type ClientType { private get; set; }

        public void Load(MainClientsViewModel vm, string[] args)
        {
            var messages = new MessageCollection();
            vm.Messages = messages;
            vm.Context = ObjectMixin.FromJson<DistributedStorageContext>(args[0]);
            NewClient(vm.Context, messages);
        }

        public void NewClient(DistributedStorageContext ctx, MessageCollection messages)
        {
            ctx.ClientId = Runtime.NewMachine(ClientType, new Client.Configure(messages, ctx.ServerId));
            Runtime.RemoteSendEvent(ctx.ServerId, new Handshake(ctx.ClientId, ctx.StorageNodeIds));
        }
    }
}
