using UnityEngine;


namespace MyScripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
