using PubSub.Core.Models;
using PubSub.Core.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubSub.Core.Interfaces
{
    public interface IPubSubInfraBuilder
    {
        public Task Build(IEnumerable<Type> subscriptionTopicTypes, PubSubAppMode mode);
    }
}
