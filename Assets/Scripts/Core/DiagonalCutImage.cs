using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Core {
    [AddComponentMenu("UI/Diagonal Cut Image")]
    public class DiagonalCutImage : Image {
        private const float MaxSafeCut = 0.925f;

        public enum Corner {
            TopRight,
            TopLeft,
            BottomRight,
            BottomLeft,
        }

        [SerializeField] [Range(0f, 1f)] public float cutAmount = 0.2f;
        [SerializeField] public Corner cutCorner = Corner.TopRight;

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh) {
            base.OnPopulateMesh(vh);

            Rect r = GetPixelAdjustedRect();
            float cutPixels = r.width * cutAmount * MaxSafeCut;

            bool isTopCorner = cutCorner is Corner.TopLeft or Corner.TopRight;
            bool isRightCorner = cutCorner is Corner.TopRight or Corner.BottomRight;
            float sign = isRightCorner ? -1f : 1f;

            int vertCount = vh.currentVertCount;
            var v = new UIVertex();

            var xs = new List<float>();

            for (int i = 0; i < vertCount; i++) {
                vh.PopulateUIVertex(ref v, i);
                AddUnique(xs, v.position.x);
            }

            xs.Sort();

            for (int i = 0; i < vertCount; i++) {
                vh.PopulateUIVertex(ref v, i);

                int colIdx = IndexOf(xs, v.position.x);
                bool affectedCol = isRightCorner ? colIdx >= xs.Count / 2 : colIdx < xs.Count / 2;

                if (affectedCol) {
                    float tY = isTopCorner
                        ? Mathf.InverseLerp(r.yMin, r.yMax, v.position.y)
                        : Mathf.InverseLerp(r.yMax, r.yMin, v.position.y);

                    float newX = v.position.x + sign * cutPixels * tY;

                    newX = isRightCorner
                        ? Mathf.Max(newX, r.xMin)
                        : Mathf.Min(newX, r.xMax);

                    v.position = new Vector3(newX, v.position.y, v.position.z);
                }

                vh.SetUIVertex(v, i);
            }
        }

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
            if (!base.IsRaycastLocationValid(screenPoint, eventCamera))
                return false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera,
                out Vector2 localPoint);

            Rect r = GetPixelAdjustedRect();
            float cutPixels = r.width * cutAmount;

            bool isTopCorner = cutCorner is Corner.TopLeft or Corner.TopRight;
            bool isRightCorner = cutCorner is Corner.TopRight or Corner.BottomRight;

            float tY = isTopCorner
                ? Mathf.InverseLerp(r.yMin, r.yMax, localPoint.y)
                : Mathf.InverseLerp(r.yMax, r.yMin, localPoint.y);

            float cutXAtY = isRightCorner ? r.xMax - cutPixels * tY : r.xMin + cutPixels * tY;
            bool inCutZone = isRightCorner ? localPoint.x > cutXAtY : localPoint.x < cutXAtY;

            return !inCutZone;
        }

        private static void AddUnique(List<float> list, float value) {
            if (list.Any(t => Mathf.Approximately(t, value))) {
                return;
            }

            list.Add(value);
        }

        private static int IndexOf(List<float> sorted, float value) {
            for (int i = 0; i < sorted.Count; i++) {
                if (Mathf.Approximately(sorted[i], value)) {
                    return i;
                }
            }

            return 0;
        }
    }
}
