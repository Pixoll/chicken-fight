using Unity.Netcode.Components;

namespace MultiplayerScripts
{
    public class ClientNetworkTransform : NetworkTransform {
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
