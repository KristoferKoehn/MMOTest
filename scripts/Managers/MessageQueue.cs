using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MMOTest.scripts.Managers
{
    public class MessageQueue
    {
        private static MessageQueue instance;
        private Queue<JObject> queue = new Queue<JObject>();

        public static MessageQueue GetInstance()
        {
            if (instance == null)
            {
                instance = new MessageQueue();
            }
            return instance;
        }

        public void AddMessage(JObject message)
        {
            queue.Enqueue(message);
        }

        public JObject PopMessage()
        {
            return queue.Dequeue();
        }
        
        public int Count()
        {
            return queue.Count;
        }

    }
}
