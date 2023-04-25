using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using OpenAI;
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;

public class RegexWindow : OdinEditorWindow
{
    [TextArea(5, 8)]
    [LabelText("Example Text Block")]
    public string FullText;
    
    [TextArea(3, 6)]
    [LabelText("Wanted Extracted Text")]
    public string ExtractedText;
    
    [HideInInspector]
    public string Pattern = "";

    [HideInInspector]
    public string TestResults = "";

    private string systemMessage =
        "You generate minimal C# regex given an Input and an desired Output from the regex. " +
        "You do not respond with any text other than the raw regular expression in the format '@XXXXX'";
    
    private UnityWebRequest request;
    
    [MenuItem("Tools/GPT/Regex Gen")]
    private static void OpenWindow()
    {
        var window = GetWindow<RegexWindow>();
        window.titleContent = new GUIContent("Regex Creator");
        window.Show();
    }

    [PropertySpace(10)]
    [Button(ButtonSizes.Medium)]
    [ShowIf("@request == null")]
    public void Generate()
    {
        Pattern = "";
        TestResults = "";
            
        string prompt = "Generate Regex\n";
        prompt += "Input: '" + FullText + "'";
        prompt += "\nOutput: '" + ExtractedText + "'";

        var msg = OpenAIUtil.CreateUserMessage(prompt);
        request = OpenAIUtil.InvokeChat(msg, null, systemMessage, false, 1f);
        request.SendWebRequest();
        Debug.Log("Sent Regex Request: " + prompt);
        EditorApplication.update += WaitForResult; 
    }
    
    void WaitForResult()
    {
        if (request == null || request.isDone)
        {
            if (request != null && request.error != null && request.error.Length > 2)
                Debug.LogError(request.error);

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
        Debug.Log(output);
        request.Dispose();
        request = null;

        if (output.Length > 2) 
        {
            output = output.Replace("Regex:", "");
            output = output.Replace("regex:", "");
            output = output.Trim();
            if (output.Length > 1 && output[0] == '@')
                output = output[1..];
        }
        
        Pattern = output;
        Repaint();
    }
    
    [PropertySpace(10)]
    [OnInspectorGUI]
    void ShowPattern()
    {
        if (Pattern == null || Pattern.Length == 0)
            return;
        
        var style = new GUIStyle( GUI.skin.label );
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        style.stretchWidth = true;
        
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label("Pattern: ", EditorStyles.boldLabel);
        
        var content = new GUIContent(Pattern);
        var position = GUILayoutUtility.GetRect(content, style);
        EditorGUI.SelectableLabel(position, Pattern, style);
        EditorGUILayout.EndHorizontal();

        if (TestResults == null || TestResults.Length == 0)
            return;
        
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Test Results", EditorStyles.boldLabel);
        GUILayout.Label(TestResults, style);
        EditorGUILayout.EndVertical();
    }
    
    [PropertySpace(10)]
    [Button]
    [ShowIf("@Pattern != null && Pattern.Length > 1")]
    public void TestRegex()
    {
        var pattern = $@"{Pattern}";
        MatchCollection matches = Regex.Matches(FullText, pattern);
        string s = "";
        foreach (var v in matches)
        {
            s += v.ToString();
        }
        TestResults = s;
    }
    
}
