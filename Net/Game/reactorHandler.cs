using System;

using Woodpecker.Sessions;
using Woodpecker.Game;

namespace Woodpecker.Net.Game
{
    public class reactorHandler
    {
        #region Fields
        private Session Session;
        public Reactor[] Reactors = new Reactor[10];
        
        #endregion

        #region Constructors
        public reactorHandler(Session Session)
        {
            this.Session = Session;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Registers a reactor (if not registered yet) and makes it available for processing messages.
        /// </summary>
        /// <param name="Reactor">The Reactor instance to register.</param>
        public void Register(Reactor Reactor)
        {
            for (int i = 0; i < 10; i++) // 10 = max reactors
            {
                if (this.Reactors[i] == null)
                {
                    Reactor.setSession(Session);
                    this.Reactors[i] = Reactor;
                    return;
                }
            }
        }
        /// <summary>
        /// Unregisters the reactor of a certain type.
        /// </summary>
        /// <param name="reactorType">The System.Type of the Reactor to unregister.</param>
        public void unRegister(Type reactorType)
        {
            try
            {
                for (int i = 0; i < 10; i++) // 10 = max reactors
                {
                    if (this.Reactors[i].GetType() == reactorType)
                    {
                        this.Reactors[i] = null;
                        return;
                    }
                }
            }
            catch { }
        }
        #endregion
    }
}
