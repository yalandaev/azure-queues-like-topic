using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMessageQueue<T>
    {
        void Clear();
        void Enqueue(T message);
        void Delete(string id);
        KeyValuePair<string, T>? Get();
    }
}