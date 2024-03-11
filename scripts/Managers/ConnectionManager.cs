using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOTest.scripts.Managers
{
    public partial class ConnectionManager : Node
    {

        private static ConnectionManager instance = null;
        private ConnectionManager() { }

        public static ConnectionManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ConnectionManager();
                GameLoop.Root.GetNode<MainLevel>("GameLoop").AddChild(instance);
                instance.Name = "ConnectionManager";
            }

            return instance;
        }



    }
}
