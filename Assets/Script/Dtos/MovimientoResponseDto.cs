using Assets.Script.Dtos.Messages;
using System;

namespace Assets.Script.Model
{
    [Serializable]
    public class MovimientoResponseDto
    {
        public string type;
        public string message;
        public MovimientoResponseDtoBody body;
    }

    [Serializable]
    public class MovimientoResponseDtoBody
    {
        public string fenAfter;
        public string additional;
        public string fromSquare;
        public string toSquare;
    }
}
