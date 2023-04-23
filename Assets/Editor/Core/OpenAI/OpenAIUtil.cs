using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;

namespace OpenAI
{
static class OpenAIUtil
{
    public static OpenAIConfig Config;

    public static RequestMessage CreateUserMessage(string prompt)
    {
        var msg = new OpenAI.RequestMessage
        {
            role = "user",
            content = prompt
        };
        return msg;
    }
    
    static string CreateChatRequestBody(RequestMessage msg, List<OpenAI.RequestMessage> prevMessages, string systemMessage, bool stream, float temperature=1)
    {
        var req = new OpenAI.Request();
        req.model = Config.UseGPT4 ? "gpt-4" : "gpt-3.5-turbo";
        req.stream = stream;
        req.temperature = temperature;

        List<OpenAI.RequestMessage> messages = new List<RequestMessage>();
        if (prevMessages != null)
            foreach (var v in prevMessages)
                messages.Add(v);
        messages.Add(msg);

        if (systemMessage != null && systemMessage.Length > 0)
        {
            var sys = new OpenAI.RequestMessage();
            sys.role = "system";
            sys.content = systemMessage;
            messages.Add(sys);
        }
        
        req.messages = messages.ToArray();

        return JsonUtility.ToJson(req);
    }

    public static UnityWebRequest InvokeChat(RequestMessage msg, List<OpenAI.RequestMessage> prevMessages, string systemMessage, bool stream, float temp=1)
    {
        if (Config == null)
            GetConfig();

        if (Config == null)
        {
            Debug.LogError("No OpenAI Config exists - please create one!");
            return null;
        }
        
        var jsonVals = CreateChatRequestBody(msg, prevMessages, systemMessage, stream, temp);

        // POST
        /*
        UnityWebRequest post;
        if (stream)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonVals);
            post = UnityWebRequest.Put(OpenAI.Api.Url, bytes);//, "application/json");
            post.method = "POST";
            post.downloadHandler = new DownloadHandlerBuffer();
        }
        else
        {
            post = UnityWebRequest.Post(OpenAI.Api.Url, jsonVals);
        }
        */
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonVals);
        var post = UnityWebRequest.Put(OpenAI.Api.Url, bytes);//, "application/json");
        post.method = "POST";
        post.downloadHandler = new DownloadHandlerBuffer();
        
        post.SetRequestHeader("Content-Type", "application/json");
        
        // Request timeout setting
        post.timeout = 0;

        // API key authorization
        string key = Config.APIKey;
        post.SetRequestHeader("Authorization", "Bearer " + key);
        
        return post;
    }

     static void GetConfig() 
     {
        if (Config != null) 
            return;
        
        var configs = AssetDatabase.FindAssets("t:OpenAIConfig");
        if (configs.Length == 0)
            return;
        
        Config = (OpenAIConfig)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(configs[0]), typeof(OpenAIConfig));
        if (Config.APIKey.Length == 0)
            Debug.LogError("API Key not set, you'll need one from OpenAI for this to work!");
    }

    public static string ParseData(UnityWebRequest post)
    {
        var json = post.downloadHandler.text;
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
            full.Append(data.choices[0].delta.content);
        }
        return (full.ToString(), isDone);
    }
}
}
