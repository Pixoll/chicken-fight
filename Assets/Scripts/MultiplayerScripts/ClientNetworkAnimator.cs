using Unity.Netcode.Components;

namespace MultiplayerScripts
{
    public class ClientNetworkAnimator : NetworkAnimator {
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
