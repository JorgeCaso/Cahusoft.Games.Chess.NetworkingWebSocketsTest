using Assets.Script.Dtos.Messages;
using System;

namespace Assets.Script.Model
{
    [Serializable]
    public class GenericMessage
    {
        public string type;
        public string message;
        public JugadorEntraBody body;  // Aquí se refleja el objeto body
    }

}
