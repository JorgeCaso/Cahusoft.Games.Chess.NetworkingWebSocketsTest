using System;

namespace Assets.Script.Dtos.Messages
{
    [Serializable]
    public class JugadorEntraServidor
    {
        public string type;
        public string partidaId;
        public string message;
    }
}
