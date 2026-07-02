using Unity.Netcode.Components;

namespace MultiPlayerSection.NetworkScripts
{
    public class ClientNetworkAnimator : NetworkAnimator {
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
