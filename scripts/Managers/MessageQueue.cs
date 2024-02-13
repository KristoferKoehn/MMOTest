using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json.Linq;

namespace MMOTest.scripts.Managers
{
    public class MessageQueue
    {
        private static MessageQueue instance = new MessageQueue();
        private Queue<JObject> queue;

        private MessageQueue()
        {
            queue = new Queue<JObject>();
        }

        public static MessageQueue GetInstance()
        {

       /*     if (instance == null)
            {
                instance = new MessageQueue();
            }
         */   return instance;
        }

        public void AddMessage(JObject message)
        {
            //GD.Print("ADDED " +  message.ToString());
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
