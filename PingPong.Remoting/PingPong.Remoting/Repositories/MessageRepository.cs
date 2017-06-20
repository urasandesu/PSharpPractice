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



using PingPong.Remoting.Domains;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace PingPong.Mvvm.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        readonly string m_filePath = Environment.ExpandEnvironmentVariables(@"%TEMP%\messages.txt");

        public Message FindLastOne()
        {
            Thread.Sleep(250);  // imagine that there is a heavy side-effect
            var lines = ReadAllLinesIfAvailable(m_filePath);
            return lines.Length == 0 ? null : Message.Parse(lines[lines.Length - 1]);
        }

        public Message FindOne(int id)
        {
            Thread.Sleep(250);  // imagine that there is a heavy side-effect
            var lines = ReadAllLinesIfAvailable(m_filePath);
            if (id < 1 || lines.Length < id)
                throw new ArgumentOutOfRangeException(nameof(id), id, $"The parameter must be between 1 and { lines.Length }.");

            return Message.Parse(lines[id - 1]);
        }

        public void Store(Message item)
        {
            Thread.Sleep(125);  // imagine that there is a heavy side-effect
            StoreCore(item);
        }

        void StoreCore(Message item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var lines = ReadAllLinesIfAvailable(m_filePath);
            if (item.Id != lines.Length + 1)
                throw new ArgumentOutOfRangeException(nameof(item), item, $"The parameter must be made by the { nameof(NextOne) } method.");

            DeleteAll();
            File.WriteAllLines(m_filePath, lines.Concat(new[] { item + "" }));
        }

        public Message NextOne()
        {
            Thread.Sleep(125);  // imagine that there is a heavy side-effect
            return NextOneCore();
        }

        Message NextOneCore()
        {
            var lines = ReadAllLinesIfAvailable(m_filePath);
            var nextId = lines.Length + 1;
            return new Message() { Id = nextId, Value = nextId % 2 == 0 ? "Pong!!" : "Ping!!" };
        }

        static string[] ReadAllLinesIfAvailable(string path)
        {
            return File.Exists(path) ? File.ReadAllLines(path) : new string[0];
        }

        public void DeleteAll()
        {
            if (File.Exists(m_filePath))
                File.Delete(m_filePath);
        }
    }
}