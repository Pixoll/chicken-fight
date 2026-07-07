using System;
using UnityEngine;

namespace MultiPlayerSection.HUDScripts
{
    [Serializable]
    public class TimeElement
    {
        public enum AccionFinal { DestruirObjeto, LiberarObjeto }
        public enum TipoObjeto { VisualOLocal, PeligroDeRed }

        [Header("Configuración de Tiempo")]
        public float segundoAparicion;
        public float segundoDesaparicion;

        [Header("Referencias y Prefabs (Opcional)")]
        public GameObject objetoVisualoFisico;
        
        [Header("Transform Spawneo")]
        public Transform puntoDeSpawneo;
        
        [Tooltip("Si está activo, el objeto clonado heredará la rotación y la escala del punto de spawneo.")]
        public bool heredarTransformCompleto = true;

        [Header("Audio del Evento (Opcional)")]
        [Tooltip("Sonido que se reproducirá inmediatamente al llegar al segundo de aparición")]
        public AudioClip sonidoAlAparecer;
        [Range(0f, 1f)] public float volumenSonido = 0.8f;

        [Header("Fin de Evento")]
        public AccionFinal queHacerAlTerminar = AccionFinal.DestruirObjeto;

        [HideInInspector] public bool yaAparecio = false;
        [HideInInspector] public bool yaTermino = false;

        public void ResetearEstado()
        {
            yaAparecio = false;
            yaTermino = false;
        }
    }
}
