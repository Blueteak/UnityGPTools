using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using OpenAI;
using System.Text.RegularExpressions;

public class GradientWindow : OdinEditorWindow
{
    [LabelWidth(50)]
    public string Prompt;

    [Range(0, 2)]
    [HorizontalGroup("Details", 200)]
    [LabelWidth(40)]
    [Tooltip("Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.")]
    public float Temp = 1;
    
    [HorizontalGroup("Details", 110)]
    [LabelWidth(85)]
    public bool SoftGradient;

    [FoldoutGroup("Advanced Settings")]
    [Tooltip("GPT responds differently if asked for RGB vs Hex colors, so you can see changing it gives better results.")]
    [LabelText("Use RGB Prompt"), LabelWidth(110)]
    public bool UseRGB;

    [HorizontalGroup("Advanced Settings/Color")]
    [Tooltip("You can try to ask GPT to include a particular color in the response.")]
    [LabelWidth(110)]
    public bool IncludeColor;

    [HorizontalGroup("Advanced Settings/Color")]
    [HideLabel]
    [ShowIf("IncludeColor")]
    public Color ToInclude;
    
    [HideLabel]
    public Gradient gradient;

    private UnityWebRequest request;

    private bool isWaiting => request == null;

    private static string HexSystemMessage = "You are a gradient generator, you generate up to 5 colors in the hex format " +
                                          "[#RRGGBB|#RRGGBB|#RRGGBB]. " +
                                          "You base these colors on the prompt from the user, and " +
                                          "only respond with the color list, no extra text.";
    
    private static string RGBSystemMessage = "You are a gradient generator, you generate 5 colors in the format " +
                                             "[(R,G,B)|(R,G,B)|(R,G,B)] where R, G, and B are " +
                                             "Red Green and Blue values from 0 to 255. " +
                                             "You base these colors on the prompt from the user, and " +
                                             "only respond with the color list, no extra text.";
    
    string RGBPattern = @"\((\d+,\s*\d+,\s*\d+)\)";
    string HexPattern = @"#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})";
    
    [MenuItem("Tools/GPT/Gradient Generator")]
    private static void OpenWindow()
    {
        var window = GetWindow<GradientWindow>();
        window.titleContent = new GUIContent("Gradient Generator");
        window.Show();
    }

    [Button]
    [ShowIf("isWaiting")]
    public void Generate()
    {
        if (Prompt == null || Prompt.Length == 0)
            return;
        var msg = OpenAIUtil.CreateUserMessage("Prompt: " + Prompt);

        var systemMsg = UseRGB ? RGBSystemMessage : HexSystemMessage;
        if (IncludeColor)
            systemMsg += " You should include the color " + ColorUtility.ToHtmlStringRGB(ToInclude) +
                         " in your gradient design, and include it as one of the colors you return.";
        request = OpenAIUtil.InvokeChat(msg, null, systemMsg, false, Temp);
        request.SendWebRequest();
        EditorApplication.update += WaitForResult; 
    }
    
    void WaitForResult()
    {
        if (request == null || request.isDone)
        {
            EditorApplication.update -= WaitForResult;
            if(request != null)
                RequestCompleted();
            return;
        }
        Repaint(); 
    }

    void RequestCompleted()
    {
        var output = OpenAIUtil.ParseData(request);
        request.Dispose();
        request = null;

        MatchCollection matches = Regex.Matches(output, UseRGB ? RGBPattern : HexPattern);
        List<GradientColorKey> keys = new List<GradientColorKey>();
        List<Color> colors = new List<Color>();
        foreach (Match match in matches)
        {
            string s = match.Value;
            Debug.Log(s);
            if (UseRGB)
            {
                //RGB Color Parsing
                s = s.Replace("(", "").Replace(")", "");
                var vals = s.Split(',');
                if (vals.Length == 3)
                {
                    int r, g, b;
                    int.TryParse(vals[0], out r);
                    int.TryParse(vals[1], out g);
                    int.TryParse(vals[2], out b);
                    colors.Add(new Color(r/255f, g/255f, b/255f));
                }
            }
            else
            {
                //Hex Color Parsing
                ColorUtility.TryParseHtmlString(s, out var c);
                colors.Add(c);
            }
        }

        for (int i = 0; i < colors.Count; i++)
        {
            if (SoftGradient)
            {
                GradientColorKey key = new GradientColorKey(colors[i], i / (float)colors.Count);
                keys.Add(key);
            }
            else
            {
                if (i > 0)
                {
                    GradientColorKey keyA = new GradientColorKey(colors[i], i / (float) colors.Count);
                    keys.Add(keyA);
                }

                if (i < colors.Count - 1)
                {
                    GradientColorKey keyB = new GradientColorKey(colors[i], ((i+1) / (float)colors.Count) - 0.01f);
                    keys.Add(keyB);
                }
            }
        }
        gradient.colorKeys = keys.ToArray();
        Repaint();
    }
}
