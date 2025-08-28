// MetallicPackerWindow
// Output format: R = Metalness, A = Smoothness (G,B unused=0).
// - Metalness input is optional (treated as R=0 if omitted).
// - Roughness/Smoothness input is required. If input is Roughness, Smoothness = 1 - value.

using UnityEditor;
using UnityEngine;
using System.IO;

public class MetallicPackerWindow : EditorWindow
{
    // Inputs
    private Texture metalnessTex;       // Optional Metalness source (reads R). Null => R=0.
    private Texture roughOrSmoothTex;   // Required: Roughness or Smoothness source.

    private enum Channel { R, G, B, A }
    private Channel roughnessChannel = Channel.R;

    // If true => source is Roughness (invert to Smoothness); if false => source is Smoothness (passthrough).
    private bool inputIsRoughness = true;

    private const string DefaultFileName = "Metallic";

    [MenuItem("Tools/Textures/Metallic Packer (Single)")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<MetallicPackerWindow>();
        wnd.titleContent = new GUIContent("Metallic Packer");
        wnd.minSize = new Vector2(420, 220);
        wnd.Show();
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            // Metalness (optional)
            metalnessTex = EditorGUILayout.ObjectField(
                new GUIContent("Metalness (optional)"),
                metalnessTex, typeof(Texture), false) as Texture;

            EditorGUILayout.Space(10);

            // Roughness / Smoothness (required)
            roughOrSmoothTex = EditorGUILayout.ObjectField(
                new GUIContent("Roughness / Smoothness (required)"),
                roughOrSmoothTex, typeof(Texture), false) as Texture;

            EditorGUILayout.Space(4);

            // Which channel to read from the required texture
            roughnessChannel = (Channel)EditorGUILayout.EnumPopup(
                new GUIContent("Source Channel"),
                roughnessChannel);

            // If checked, interpret the source as Roughness (invert to Smoothness = 1 - value)
            inputIsRoughness = EditorGUILayout.ToggleLeft(
                "Input is Roughness (invert to Smoothness = 1 - value)",
                inputIsRoughness);

            EditorGUILayout.Space(10);

            using (new EditorGUI.DisabledScope(roughOrSmoothTex == null))
            {
                if (GUILayout.Button("Pack Selected", GUILayout.Height(28)))
                {
                    PackAndSaveWithDialog(metalnessTex, roughOrSmoothTex, roughnessChannel, inputIsRoughness);
                }
            }
        }
    }

    private static void PackAndSaveWithDialog(Texture metal, Texture roughOrSmooth, Channel ch, bool sourceIsRoughness)
    {
        if (roughOrSmooth == null)
        {
            EditorUtility.DisplayDialog("Error", "Please specify a Roughness/Smoothness texture.", "OK");
            return;
        }

        bool noMetal = (metal == null);

        // Output size: Metalness size if provided, otherwise Roughness/Smoothness size
        var (mw, mh) = noMetal ? GetTexSize(roughOrSmooth) : GetTexSize(metal);
        if (mw <= 0 || mh <= 0)
        {
            EditorUtility.DisplayDialog("Error", "Could not retrieve valid texture size.", "OK");
            return;
        }

        // Sample inputs in Linear space (works even if textures are not Read/Write)
        var roughPixels = SampleTextureLinear(roughOrSmooth, mw, mh);
        Color32[] metalPixels = null;
        if (!noMetal) metalPixels = SampleTextureLinear(metal, mw, mh);

        var outTex = new Texture2D(mw, mh, TextureFormat.RGBA32, true, true); // linear
        var outCols = new Color32[mw * mh];

        for (int i = 0; i < outCols.Length; i++)
        {
            // R: Metalness (from Metal input R, or 0 if missing)
            byte m = noMetal ? (byte)0 : ChannelValue(metalPixels[i], Channel.R);

            // A: Smoothness (from chosen channel; invert if source is Roughness)
            byte src = ChannelValue(roughPixels[i], ch);
            byte smooth = sourceIsRoughness ? (byte)(255 - src) : src;

            outCols[i] = new Color32(m, 0, 0, smooth); // G,B=0 (unused)
        }
        outTex.SetPixels32(outCols);
        outTex.Apply(true, false);

        // Save PNG and import with proper settings (Linear / Alpha-is-Transparency OFF)
        var basePathTex = noMetal ? roughOrSmooth : metal;
        var basePath = AssetDatabase.GetAssetPath(basePathTex);
        var defaultDir = Path.GetDirectoryName(basePath);

        string savePath = EditorUtility.SaveFilePanelInProject(
            "Save Metallic (R) + Smoothness (A)",
            DefaultFileName,
            "png",
            noMetal
                ? "Metalness not specified: saving with R=0, A=Smoothness."
                : "Saving with R=Metalness, A=Smoothness.",
            defaultDir
        );

        if (string.IsNullOrEmpty(savePath))
        {
            UnityEngine.Object.DestroyImmediate(outTex);
            return;
        }

        File.WriteAllBytes(savePath, outTex.EncodeToPNG());
        AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);

        var ti = (TextureImporter)AssetImporter.GetAtPath(savePath);
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = false;         // Linear import
            ti.alphaIsTransparency = false; // Alpha stores Smoothness
            ti.mipmapEnabled = true;
            ti.SaveAndReimport();
        }

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
        UnityEngine.Object.DestroyImmediate(outTex);
    }

    private static (int w, int h) GetTexSize(Texture tex) => tex != null ? (tex.width, tex.height) : (0, 0);

    // Linear-space GPU readback
    private static Color32[] SampleTextureLinear(Texture tex, int width, int height)
    {
        var prevRT = RenderTexture.active;
        var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        try
        {
            Graphics.Blit(tex, rt);
            RenderTexture.active = rt;

            var tmp = new Texture2D(width, height, TextureFormat.RGBA32, false, true); // linear
            tmp.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tmp.Apply(false, false);

            var cols = tmp.GetPixels32();
            UnityEngine.Object.DestroyImmediate(tmp);
            return cols;
        }
        finally
        {
            RenderTexture.active = prevRT;
            RenderTexture.ReleaseTemporary(rt);
        }
    }

    private static byte ChannelValue(in Color32 c, Channel ch)
    {
        switch (ch)
        {
            case Channel.R: return c.r;
            case Channel.G: return c.g;
            case Channel.B: return c.b;
            case Channel.A: return c.a;
            default: return c.r;
        }
    }
}
