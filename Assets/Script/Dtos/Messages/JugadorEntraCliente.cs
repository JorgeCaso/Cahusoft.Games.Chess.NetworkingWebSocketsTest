using System;
using System.Collections.Generic;

namespace Assets.Script.Dtos.Messages
{
    [Serializable]
    public class JugadorEntraCliente
    {
        public string type;
        public string message;
        public JugadorEntraBody body;
    }

    [Serializable]
    public class ErrorComun
    {
        public string type;
        public string message;
    }

    [Serializable]
    public class JugadorListo
    {
        public string type;
        public string partidaId;
    }

    [Serializable]
    public class JugadorEntraBody
    {
        public int id;
        public PlayerCliente white;
        public PlayerCliente black;
        public List<Movimiento> moves;
    }

    [Serializable]
    public class PlayerCliente
    {
        public int id;
        public string email;
    }

    [Serializable]
    public class Movimiento
    {
        public string from;
        public string to;
    }
}
