/* 
 * File: MessageRepository.cs
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



using PingPong.Mvvm.Domains;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PingPong.Mvvm.Repositories
{
    class MessageRepository : IMessageRepository
    {
        List<Message> m_messages = new List<Message>();

        public Message FindLastOne()
        {
            Thread.Sleep(250);  // imagine that there is a side-effect
            return m_messages.Count == 0 ? null : m_messages[m_messages.Count - 1];
        }

        public Message FindOne(int id)
        {
            Thread.Sleep(250);  // imagine that there is a side-effect
            if (id < 1 || m_messages.Count < id)
                throw new ArgumentOutOfRangeException(nameof(id), id, $"The parameter must be between 1 and { m_messages.Count }.");

            return m_messages[id - 1];
        }

        public void Store(Message item)
        {
            Thread.Sleep(125);  // imagine that there is a side-effect
            StoreCore(item);
        }

        void StoreCore(Message item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item.Id != m_messages.Count + 1)
                throw new ArgumentOutOfRangeException(nameof(item), item, $"The parameter must be made by the { nameof(NextOne) } method.");

            m_messages.Add(item);
        }

        public Message NextOne()
        {
            Thread.Sleep(125);  // imagine that there is a side-effect
            return NextOneCore();
        }

        Message NextOneCore()
        {
            var nextId = m_messages.Count + 1;
            return new Message() { Id = nextId, Value = nextId % 2 == 0 ? "Pong!!" : "Ping!!" };
        }
    }
}
