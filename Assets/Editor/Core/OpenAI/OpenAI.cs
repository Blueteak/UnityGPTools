namespace OpenAI
{
    public static class Api
    {
        public const string Url = "https://api.openai.com/v1/chat/completions";
    }

    [System.Serializable]
    public struct ResponseMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public struct ResponseChoice
    {
        public int index;
        public ResponseMessage message;
    }

    [System.Serializable]
    public struct Response
    {
        public string id;
        public ResponseChoice[] choices;
    }

    [System.Serializable]
    public struct RequestMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public struct Request
    {
        public string model;
        public bool stream;
        public float temperature;
        public RequestMessage[] messages;
    }
    
    [System.Serializable]
    public struct ResponseChunk
    {
        public string id;
        public long created;
        public string model;
        public ChunkChoice[] choices;
    }
    
    [System.Serializable]
    public struct ChunkChoice
    {
        public int index;
        public string finish_reason;
        public ChunkMessage delta;
    }
    
    [System.Serializable]
    public struct ChunkMessage
    {
        public string role;
        public string content;
    }
}
