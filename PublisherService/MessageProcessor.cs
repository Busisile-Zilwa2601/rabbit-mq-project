using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublisherService
{
    public class MessageProcessor : IMessageProcessor
    {
        public void ProcesStatement(string name)
        {
            Console.WriteLine($"Hello my name is, {name}");
        }
    }
}
