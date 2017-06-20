/* 
 * File: MessageRepositoryMock.cs
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
using Microsoft.PSharp;
using PingPong.Remoting.Domains;
using System;

namespace Test.PingPong.Remoting.Repositories
{
    class MessageRepositoryMock : IMessageRepository
    {
        [Dependency]
        public PSharpRuntime Runtime { private get; set; }

        public void DeleteAll()
        {
            throw new NotImplementedException();
        }

        public Message FindLastOne()
        {
            throw new NotImplementedException();
        }

        public Message FindOne(int id)
        {
            var value = Runtime.Random() ? "Ping!!" : "Pong!!";
            return new Message() { Id = id, Value = value };
        }

        public Message NextOne()
        {
            var id = Runtime.RandomInteger(40) + 1;
            var value = Runtime.Random() ? "Ping!!" : "Pong!!";
            return new Message() { Id = id, Value = value };
        }

        public void Store(Message item)
        {
            // nop
        }
    }
}
