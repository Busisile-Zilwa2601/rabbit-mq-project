using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublisherService
{
    public class MessagePublisher
    {
        public void SendMessage(PublishMessageFactory publishMessageFactory, string name)
        { 
            publishMessageFactory.ExecuteMessage(name);
        }
    }
}
