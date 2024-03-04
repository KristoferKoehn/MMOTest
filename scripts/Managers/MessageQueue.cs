using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;



namespace MMOTest.scripts.Managers
{
    public partial class MessageQueue : Node
    {

        private static MessageQueue instance = null;
        private Queue<JObject> queue;

        private MessageQueue()
        {
            queue = new Queue<JObject>();
        }

        public static MessageQueue GetInstance()
        {
            if (instance == null)
            {
                instance = new MessageQueue();
                GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
                instance.Name = "MessageQueue";
            }
            return instance;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
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
