using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublisherService
{
    public interface IMessageProcessor
    {
        void ProcesStatement(string name);
    }
}
