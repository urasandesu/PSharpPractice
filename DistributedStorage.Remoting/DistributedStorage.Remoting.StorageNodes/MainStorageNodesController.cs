/* 
 * File: MainStorageNodesController.cs
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
using DistributedStorage.Remoting.Infrastructures;
using DistributedStorage.Remoting.Infrastructures.Mixins.Microsoft.PSharp;
using DistributedStorage.Remoting.Infrastructures.Mixins.System;
using DistributedStorage.Remoting.StorageNodes.Domains;
using Microsoft.Practices.Unity;
using Microsoft.PSharp;
using System;
using System.Collections.Generic;

namespace DistributedStorage.Remoting.StorageNodes
{
    class MainStorageNodesController : ApplicationController
    {
        [Dependency]
        public IProcessExecutor ProcessExecutor { private get; set; }

        [Dependency("StorageNode")]
        public Type StorageNodeType { private get; set; }

        [Dependency("Timer")]
        public Type TimerType { private get; set; }

        public void Load(MainStorageNodesViewModel vm, string[] args)
        {
            var messages = new MessageCollection();
            vm.Messages = messages;
            vm.Context = ObjectMixin.FromJson<DistributedStorageContext>(args[0]);
            NewStorageNodes(vm.Context, messages);

            ProcessExecutor.StartProcess(@"..\..\..\DistributedStorage.Remoting.Clients\bin\Debug\DistributedStorage.Remoting.Clients.exe", vm.Context.ToJson().ToCommandLineArgument());
        }

        public void NewStorageNodes(DistributedStorageContext ctx, MessageCollection messages)
        {
            var configure = new StorageNode.Configure(messages, ctx.SafetyMonitorId);

            var snIds = new List<MachineId>();
            for (var i = 0; i < 3; i++)
            {
                var snId = Runtime.NewMachine(StorageNodeType, configure);
                Runtime.SendEvent(snId, new Handshake(ctx.ServerId));
                snIds.Add(snId);
            }
            Runtime.RemoteInvokeMonitor(configure.SafetyMonitorId, new Handshake(storageNodeIds: snIds.ToArray()));
            foreach (var snId in snIds)
                Runtime.NewMachine(TimerType, new Timer.Configure(snId));
            ctx.StorageNodeIds = snIds.ToArray();
        }
    }
}
