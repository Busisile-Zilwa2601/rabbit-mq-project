using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublisherService
{
    public abstract class PublishMessageFactory
    {
        //Factory Method
        public abstract IMessageProcessor CreateMessageProcessor();

        public void ExecuteMessage(string name)
        { 
            var messageProcessor = CreateMessageProcessor();
            messageProcessor.ProcesStatement(name);
        }
    }
}
