namespace Cod.IoT.Networking
{
    public class FakeNetworkManager : GenericService, INetworkManager
    {
        public bool AutoConnect { get; set; }

        public bool IsEstablished { get; private set; }

        public override int ID => Constants.NetworkManagerID;

        public event EventHandler? Established;

        public void Connect()
        {
            IsEstablished = true;
            OnEstablished();
        }

        private void OnEstablished()
        {
            Established?.Invoke(this, EventArgs.Empty);
        }
    }
}
