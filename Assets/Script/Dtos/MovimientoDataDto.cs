using System;

namespace Assets.Script.Model
{
    [Serializable]
    public class MovimientoDataDto
    {
        public string type="move";
        public string partidaId;
        public string playerId;
        public string move;
        public string fenCurrent;
        public string additional;
    }
}
