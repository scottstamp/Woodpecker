using System;
using ExtensionMethods;
using Woodpecker.Net.Game.Messages;
using System.Threading.Tasks;

namespace Woodpecker.Game.Rooms.Instances
{
    public partial class roomInstance
    {
        private int cameraInterval = 20; // interval in seconds
        private long cameraLastUpdate = 0;
        private Random rand = new Random((int)DateTime.Now.Ticks);

        private void runCamera()
        {
            if (DateTime.Now.Ticks - cameraLastUpdate > (cameraInterval * 10000000))
            {
                Units.roomUser randomUser = null;

                if (this.roomUsers.Count > 0)
                    randomUser = (Units.roomUser)Extensions.RandomValue(this.roomUsers);
                else
                    return;

                sendMessage(serverMessage.createCastAction("cam1", "transition fade"));
                sendMessage(serverMessage.createCastAction("cam1", $"setcamera {rand.Next(1, 2)}"));
                sendMessage(serverMessage.createCastAction("cam1", $"targetcamera {randomUser.ID}"));

                cameraLastUpdate = DateTime.Now.Ticks;
            }
        }
    }
}
