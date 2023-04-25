using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using OpenAI;
using Sirenix.OdinInspector;

public class ChatbotWindow : OdinEditorWindow
{
    [HideLabel]
    public Conversation Chat;
    
    [TextArea]
    [HideLabel]
    public string InputMessage;

    public bool HasConversation => Chat != null && Chat.HasConversation;

    //For internal Use
    List<RequestMessage> MessageChain = new List<RequestMessage>();

    private UnityWebRequest request;
    
    [MenuItem("Tools/GPT/Chat Window")]
    private static void OpenWindow()
    {
        var window = GetWindow<ChatbotWindow>();
        window.titleContent = new GUIContent("Chat GPT");
        window.Show();
    }
    
    [Button(ButtonSizes.Medium)]
    [ShowIf("@request == null")]
    public void Send()
    {
        if (InputMessage.Length == 0)
            return;
        var msg = OpenAIUtil.CreateUserMessage(InputMessage);
        request = OpenAIUtil.InvokeChat(msg, Chat.History, "", true);
        request.SendWebRequest();
        Chat.AddMessage(msg);
        output = "";
        EditorApplication.update += WaitForResult; 
        Debug.Log("Send Chat request: " + InputMessage);
    }

    private string output = "";
    void WaitForResult()
    {
        if (!request.isDone)
        {
            string result = OpenAIUtil.ParseChunks(request).text;
            if (result.Length > 0)
            {
                if (output.Length == 0)
                    Chat.AddMessage(new RequestMessage {role = "assistant"});
                Chat.UpdateLastMessage(output);
                output = result;
            }
            Repaint(); 
            return;
        }
        
        Debug.Log("-- Request Response Completed --");
        if (request.error != null && request.error.Length > 2)
        {
            Chat.UpdateLastMessage(request.error);
        }
        request.Dispose();
        request = null;
        EditorApplication.update -= WaitForResult;
    }

    [Button]
    [ShowIf("HasConversation")]
    public void Clear()
    {
        Chat.Clear();
        MessageChain.Clear();
    }

    [System.Serializable]
    public class Conversation
    {
        [HideInInspector]
        public List<RequestMessage> History = new List<RequestMessage>();
        
        public bool HasConversation => History != null && History.Count > 0;
        
        private Vector2 chatScroll;
        [OnInspectorGUI]
        void DrawConversation()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.FlexibleSpace();
            
            chatScroll = EditorGUILayout.BeginScrollView(chatScroll);
            var style = new GUIStyle( GUI.skin.label );
            style.alignment = TextAnchor.UpperLeft;
            style.wordWrap = true;
            style.stretchWidth = true;
            
            foreach (var v in History)
            {
                bool isAIMessage = v.role == "assistant";
                if(isAIMessage)
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                else
                    EditorGUILayout.BeginHorizontal();
                GUILayout.Label((isAIMessage ? "GPT" : "User") + ":");
                if (isAIMessage)
                {
                    var content = new GUIContent(v.content);
                    var position = GUILayoutUtility.GetRect(content, style);
                    EditorGUI.SelectableLabel(position, v.content, style);
                }
                else
                    GUILayout.Label(v.content, style);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        public void UpdateLastMessage(string content)
        {
            if (History.Count > 0)
            {
                RequestMessage m = new RequestMessage()
                {
                    role = "assistant",
                    content = content
                };
                History[^1] = m;
            }
                
        }

        public void AddMessage(RequestMessage msg)
        {
            History.Add(msg);
        }

        public void Clear()
        {
            History.Clear();
        }
    }
}
