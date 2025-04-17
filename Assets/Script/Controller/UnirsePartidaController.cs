using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Controller
{
    public class UnirsePartidaController : MonoBehaviour
    {
        public TMP_InputField inputDesde;
        public TMP_InputField inputHasta;
        public Button botonEnviar;
        public TMP_Text textoLog;
        public ChessWebSocket socket;

        void Start()
        {
            botonEnviar.onClick.AddListener(EnviarMovimiento);
        }

        void EnviarMovimiento()
        {
            string desde = inputDesde.text.Trim();
            string hasta = inputHasta.text.Trim();

            if (!string.IsNullOrEmpty(desde) && !string.IsNullOrEmpty(hasta))
            {
                socket.EnviarMovimiento(desde, hasta);
                textoLog.text += $"\n Enviado: {desde} → {hasta}";
            }
        }

        public void MostrarMovimientoRecibido(string desde, string hasta)
        {
            textoLog.text += $"\n Recibido: {desde} → {hasta}";
        }
    }
}
