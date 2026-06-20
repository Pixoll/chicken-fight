using System;
using UnityEngine;

namespace MultiplayerScripts.GlobalGameState
{
    [Serializable]
    public class TimeElement
    {
        public enum AccionFinal { DestruirObjeto, LiberarObjeto }

        [Header("Configuración de Tiempo")]
        public float segundoAparicion;
        public float segundoDesaparicion;

        [Header("Referencias")]
        public GameObject objetoVisualoFisico;
        public Transform puntoDeSpawneo;
        public AccionFinal queHacerAlTerminar = AccionFinal.DestruirObjeto;

        [HideInInspector] public bool yaAparecio = false;
        [HideInInspector] public bool yaTermino = false;
    }
}
