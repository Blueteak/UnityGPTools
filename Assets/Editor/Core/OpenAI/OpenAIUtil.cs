using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;

namespace OpenAI
{
static class OpenAIUtil
{
    public static string APIKey;

    public static RequestMessage CreateUserMessage(string prompt)
    {
        var msg = new OpenAI.RequestMessage();
        msg.role = "user";
        msg.content = prompt;
        return msg;
    }
    
    static string CreateChatRequestBody(RequestMessage msg, List<OpenAI.RequestMessage> prevMessages, string systemMessage)
    {
        var req = new OpenAI.Request();
        req.model = "gpt-3.5-turbo";
        req.stream = true;
        req.temperature = 1;

        List<OpenAI.RequestMessage> messages = new List<RequestMessage>();
        if (prevMessages != null)
            foreach (var v in prevMessages)
                messages.Add(v);

        if (systemMessage != null && systemMessage.Length > 0)
        {
            var sys = new OpenAI.RequestMessage();
            sys.role = "system";
            sys.content = systemMessage;
            messages.Add(sys);
        }
        
        req.messages = messages.ToArray(); //new [] { msg };

        Debug.Log("");
        
        return JsonUtility.ToJson(req);
    }

    public static UnityWebRequest InvokeChat(RequestMessage msg, List<OpenAI.RequestMessage> prevMessages, string systemMessage)
    {
        var jsonVals = CreateChatRequestBody(msg, prevMessages, systemMessage);
        Debug.Log("Sending Chat JSON: " + jsonVals);

        // POST
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonVals);
        var post = UnityWebRequest.Put(OpenAI.Api.Url, bytes);//, "application/json");
        post.method = "POST";
        post.SetRequestHeader("Content-Type", "application/json");
        
        //Buffer?
        post.downloadHandler = new DownloadHandlerBuffer();
        
        // Request timeout setting
        post.timeout = 0;

        // API key authorization
        string key = GetAPIKey();
        post.SetRequestHeader("Authorization", "Bearer " + key);
        
        return post;
    }

    public static string GetAPIKey()
    {
        if (APIKey != null && APIKey.Length > 1)
            return APIKey;
        
        var configs = AssetDatabase.FindAssets("t:OpenAIConfig");
        if (configs.Length > 0)
        {
            var config = (OpenAIConfig)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(configs[0]), typeof(OpenAIConfig));
            APIKey = config.APIKey;
            if (APIKey.Length == 0)
                Debug.LogError("API Key not set, you'll need one from OpenAI for this tool to work!");
        }
        else
            Debug.LogError("No OpenAI Config exists for API Key - please create one!");

        return APIKey;
    }

    public static string ParseData(UnityWebRequest post)
    {
        var json = post.downloadHandler.text;
        Debug.Log("Parsing Data:" + json);
        var data = JsonUtility.FromJson<OpenAI.Response>(json);
        return data.choices[0].message.content;
    }

    public static (string text, bool isDone) ParseChunks(UnityWebRequest post)
    {
        StringBuilder full = new StringBuilder();
        var textData = post.downloadHandler.text;
        if (textData == null || textData.Length < 4)
            return ("", false);
        var chunks = textData.Split("data: ");
        bool isDone = false;
        foreach (var v in chunks)
        {
            if(v == null || v.Length < 4)
                continue;
            if (v == "[DONE]")
            {
                isDone = true;
                continue;
            }
            
            var data = JsonUtility.FromJson<OpenAI.ResponseChunk>(v);
            if(data.choices.Length == 0 || data.choices[0].delta.content == null)
                continue;
            Debug.Log(data.choices[0].delta.content);
            full.Append(data.choices[0].delta.content);
        }

        return (full.ToString(), isDone);
    }
}
}
