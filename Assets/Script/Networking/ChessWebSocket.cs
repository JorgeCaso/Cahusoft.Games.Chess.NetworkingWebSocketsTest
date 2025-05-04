using Assets.Script.Model;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TMPro;
using UnityEngine;
using NativeWebSocket;
using System;
using System.Threading.Tasks;
using Assets.Script.Dtos.Messages;

namespace Assets.Script.Networking
{
    public class ChessWebSocket : MonoBehaviour
    {
        public const string JOIN_GAME = "join_game";
        public const string PLAYER_JOINED = "player_joined";

        private WebSocket websocket;
        public TMP_InputField partidaId;

        public event Action<string, string> OnMovimientoRecibido;
        public event Action<JugadorEntraCliente> OnUnidoAPartida;
        public event Action<string> OnUnidoPartidaListo;

        async void Start()
        {
            Debug.Log("🔧 ChessWebSocket.Start ejecutado");
            websocket = new WebSocket(AppSettings.BaseUrlChessWebSockets);
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertificates;

            websocket.OnOpen += async () =>
            {
                Debug.Log("🟢 Conectando al servidor...");


                await websocket.SendText(JsonUtility.ToJson(new { type = "connected" }));
                Debug.Log("✅ Conectado");
            };

            websocket.OnMessage += (bytes) =>
            {
                var message = Encoding.UTF8.GetString(bytes);
                Debug.Log($"Mensaje recibido: {message}");
                try
                {
                    var baseData = JsonUtility.FromJson<GenericMessage>(message);
                    
                    switch (baseData.type)
                    {
                        case PLAYER_JOINED:
                            var jugadorEntraCliente = JsonUtility.FromJson<JugadorEntraCliente>(message);
                            OnUnidoAPartida?.Invoke(jugadorEntraCliente);
                            return;
                        case "move":
                            var m = JsonUtility.FromJson<MovimientoResponseDto>(message);
                            OnMovimientoRecibido?.Invoke(m.body.fromSquare, m.body.toSquare);
                            MatchSettings.fenCurrent = m.body.fenAfter;
                            return;
                        case "listo":
                            var list = JsonUtility.FromJson<JugadorListo>(message);
                            OnUnidoPartidaListo?.Invoke(list.partidaId);
                            return;
                        case "error_movimiento":
                            var e_movimiento = JsonUtility.FromJson<ErrorComun>(message);
                            //OnUnidoPartidaListo?.Invoke(list.partidaId);
                            return;
                        case "error_guardar":
                            var e_guardar = JsonUtility.FromJson<ErrorComun>(message);
                            //OnUnidoPartidaListo?.Invoke(list.partidaId);
                            return;
                        default:
                            Debug.LogWarning($"⚠️ Tipo desconocido: {baseData.type}");
                            return;
                    }
                }
                catch
                {
                    Debug.LogWarning("⚠️ No se pudo parsear el mensaje recibido.");
                }
            };
            try
            {
                await websocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error al conectar WebSocket: {ex.Message}");
            }
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
#endif
        }

        public async void EnviarMovimiento(string from, string to, string additional = "")
        {
            Debug.Log("fenCurrent: " + MatchSettings.fenCurrent);

            var movimiento = new MovimientoDataDto
            {
                partidaId = MatchSettings.gameId.ToString(),
                playerId = AppSettings.userId.ToString(),
                move = from + to,
                fenCurrent = MatchSettings.fenCurrent,
                additional = additional
            };

            string mensaje = JsonUtility.ToJson(movimiento);
            await websocket.SendText(mensaje);
        }

        public async void UnirseAPartida(int? partidaId)
        {
            if (partidaId != null)
            {
                MatchSettings.gameId = partidaId;
            }

            while (websocket == null || websocket.State != WebSocketState.Open)
            {
                await Task.Yield();
            }

            var jugadorEntraServidor = new JugadorEntraServidor
            {
                type = JOIN_GAME,
                partidaId = partidaId.ToString()
            };

            await websocket.SendText(JsonUtility.ToJson(jugadorEntraServidor));
        }

        private static bool AcceptAllCertificates(object s, X509Certificate c, X509Chain ch, SslPolicyErrors e) => true;
    }
}
