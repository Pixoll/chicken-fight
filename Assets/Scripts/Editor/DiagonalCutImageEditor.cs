using Core;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(DiagonalCutImage))]
// ReSharper disable once CheckNamespace
public class DiagonalCutImageEditor : ImageEditor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        var img = (DiagonalCutImage)target;

        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        float newCut = EditorGUILayout.Slider("Cut Amount", img.cutAmount, 0f, 1f);
        var newCorner = (DiagonalCutImage.Corner)EditorGUILayout.EnumPopup("Cut Corner", img.cutCorner);

        if (!EditorGUI.EndChangeCheck()) return;

        Undo.RecordObject(img, "Change Diagonal Cut");
        img.cutAmount = newCut;
        img.cutCorner = newCorner;
        img.SetVerticesDirty();
        EditorUtility.SetDirty(img);
    }
}
