using Assets.Script.Model;
using Assets.Script.Networking;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Controller
{
    public class MovimientoController : MonoBehaviour
    {
        public Button botonMovimiento1;
        public Button botonMovimiento2;
        public Button botonMovimiento3;
        public Button botonMovimiento4;
        public TMP_Text textoLog;
        public ChessWebSocket websocket;

        private void Start()
        {
            botonMovimiento1.onClick.AddListener(() => StartCoroutine(EnviarMovimiento(1)));
            botonMovimiento2.onClick.AddListener(() => StartCoroutine(EnviarMovimiento(2)));
            botonMovimiento3.onClick.AddListener(() => StartCoroutine(EnviarMovimiento(3)));
            botonMovimiento4.onClick.AddListener(() => StartCoroutine(EnviarMovimiento(4)));
            websocket.OnMovimientoRecibido += MostrarMovimientoRecibido;
        }

        private IEnumerator EnviarMovimiento(int tipo)
        {
            string desde = string.Empty;
            string hasta = string.Empty;

            switch (tipo)
            {
                case 1:
                    desde = "e2";
                    hasta = "e4";
                    break;
                case 2:
                    desde = "e7";
                    hasta = "e6";
                    break;
                case 3:
                    desde = "g1";
                    hasta = "f3";
                    break;
                case 4:
                    desde = "g7";
                    hasta = "g6";
                    break;
                default:
                    break;
            }


            // 🔍 Validación 1: campos vacíos
            if (string.IsNullOrEmpty(desde) || string.IsNullOrEmpty(hasta))
            {
                textoLog.text += "\nMovimiento incompleto.";
                yield break;
            }

            // 🔍 Validación 2: partida activa
            if (MatchSettings.gameId == null)
            {
                textoLog.text += "\nNo estás en una partida activa.";
                yield break;
            }

            //🔍 Validación 3: pertenencia
            if (MatchSettings.blackId != AppSettings.userId &&
                MatchSettings.whiteId != AppSettings.userId)
            {
                textoLog.text += "\nNo eres parte de esta partida.";
                yield break;
            }

            websocket.EnviarMovimiento(desde, hasta);

            textoLog.text += $"\nEnviado: {desde} → {hasta}";

            yield return null;
        }

        public void MostrarMovimientoRecibido(string desde, string hasta)
        {
            textoLog.text += $"\nRecibido: {desde} → {hasta}";
        }
    }
}
