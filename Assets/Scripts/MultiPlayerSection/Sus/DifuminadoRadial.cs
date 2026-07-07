using UnityEngine;
using UnityEngine.UI;

namespace MultiPlayerSection.Sus
{
    public class DifuminadoRadial : MonoBehaviour
    {
        [Header("Configuración del Color")]
        [SerializeField] private Color colorCentro = Color.white;
        
        [Tooltip("El color de afuera suele tener el Alpha en 0 para que sea transparente")]
        [SerializeField] private Color colorBorde = new Color(1f, 1f, 1f, 0f);

        [Header("Tamaño de la Textura")]
        [SerializeField] private int resolucionTextura = 256;

        private void Awake()
        {
            Texture2D texturaGradiente = GenerarTexturaRadial();

            if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                spriteRenderer.sprite = Sprite.Create(texturaGradiente, new Rect(0, 0, resolucionTextura, resolucionTextura), new Vector2(0.5f, 0.5f));
            }
            else if (TryGetComponent<Image>(out var uiImage))
            {
                uiImage.sprite = Sprite.Create(texturaGradiente, new Rect(0, 0, resolucionTextura, resolucionTextura), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogWarning("[Difuminado] -> Este script necesita un componente SpriteRenderer o una Image para mostrarse.");
            }
        }

        private Texture2D GenerarTexturaRadial()
        {
            Texture2D tex = new Texture2D(resolucionTextura, resolucionTextura);
            tex.wrapMode = TextureWrapMode.Clamp;

            float centro = resolucionTextura / 2f;

            for (int y = 0; y < resolucionTextura; y++)
            {
                for (int x = 0; x < resolucionTextura; x++)
                {
                    float distCentro = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro));
                    
                    float ratio = distCentro / centro;
                    ratio = Mathf.Clamp01(ratio);

                    Color colorPixel = Color.Lerp(colorCentro, colorBorde, ratio);
                    
                    tex.SetPixel(x, y, colorPixel);
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
