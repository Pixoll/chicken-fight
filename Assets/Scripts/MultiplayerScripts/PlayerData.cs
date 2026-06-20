using System;
using Unity.Collections;
using Unity.Netcode;

namespace MultiplayerScripts
{
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public FixedString32Bytes nombreJugador;
        public int puntosVictoria;
        public float cooldownHabilidad;
        public bool tieneObjeto;
        public float vidaActual;

        public bool Equals(PlayerData other)
        {
            return vidaActual == other.vidaActual &&
                   nombreJugador.Equals(other.nombreJugador) &&
                   puntosVictoria == other.puntosVictoria &&
                   cooldownHabilidad == other.cooldownHabilidad &&
                   tieneObjeto == other.tieneObjeto;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref nombreJugador);
            serializer.SerializeValue(ref puntosVictoria);
            serializer.SerializeValue(ref cooldownHabilidad);
            serializer.SerializeValue(ref tieneObjeto);
            serializer.SerializeValue(ref vidaActual);
        }
    }
}
