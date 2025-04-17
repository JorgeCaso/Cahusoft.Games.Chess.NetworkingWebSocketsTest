using UnityEngine;
using NativeWebSocket;
using System.Text;
using Assets.Script.Model;
using System.Collections;
using TMPro;
using Assets.Script.Controller;
using UnityEngine.Networking;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;

public class ChessWebSocket : MonoBehaviour
{
    WebSocket websocket;
    public MatchSettings game;
    public ChessUI ui;
    public TMP_InputField partidaId;

    private static bool AcceptAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;  // Acepta todos los certificados (incluidos los autofirmados)
    }

    async void Start()
    {
        websocket = new WebSocket(AppSettings.BaseUrlChessWebSockets);

        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertificates);

        websocket.OnOpen += async () =>
        {
            Debug.Log("🟢 conectando al servidor");

            //await websocket.SendText(JsonUtility.ToJson(
            //   new Conexion
            //   {
            //       type = "join",
            //       partidaId = "1"
            //   }
            //));

            await websocket.SendText(JsonUtility.ToJson(
                new {
                    type = "connected"
                }
             ));

            Debug.Log("✅ conectado");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("❌ Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("🔴 Desconectado del servidor");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("📨 Movimiento recibido: " + message);

            try
            {
                MovimientoResponseDto m = JsonUtility.FromJson<MovimientoResponseDto>(message);
                ui.MostrarMovimientoRecibido(m.fromSquare, m.toSquare);
                if (MatchSettings.fenCurrent != "") {
                    MatchSettings.fenCurrent = m.fenAfter;
                }
            }
            catch
            {
                Debug.LogWarning("⚠️ No se pudo parsear el mensaje recibido.");
            }
        };

        await websocket.Connect();
    }

    public async void EnviarMovimiento(string from, string to, string additional="")
    {
        var movimiento = new MovimientoDataDto
        {
            partidaId = MatchSettings.gameId,
            playerId = MatchSettings.playerId,
            move = from + to,
            fenCurrent = MatchSettings.fenCurrent,
            additional = ""
        };

        string mensaje = JsonUtility.ToJson(movimiento);
        await websocket.SendText(mensaje);

        Debug.Log("Movimiento enviado: " + mensaje);
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
        #endif
    }

    public void UnirseAPartida(string partidaId)
    {
        bool actualizar = false;

        if (partidaId == "" || partidaId == null) {
            partidaId = this.partidaId.text;
            actualizar = true;
        }

        StartCoroutine(Unirse(partidaId, actualizar));
    }

    IEnumerator Unirse(string partidaId, bool actualizar)
    {
        while (websocket == null || websocket.State != WebSocketState.Open)
            yield return null;

        string join = JsonUtility.ToJson(new Conexion
        {
            type = "join",
            partidaId = partidaId
        });

        websocket.SendText(join);
        Debug.Log("📡 Unido a partida: " + partidaId);

        if (actualizar)
        {
            StartCoroutine(ActualizarPartidaEnSymfony(partidaId));
        }
    }

    IEnumerator ActualizarPartidaEnSymfony(string partidaId)
    {
        string url = $"{AppSettings.BaseUrlChessWebApi}/api/games/{partidaId}/join";

        //var payload = new UpdatePartidaData
        //{
        //    player2 = LoginController.user.id,
        //  //  status = "activa"
        //};

        //string jsonData = JsonUtility.ToJson(payload);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        string body = "{}";                       //  ←  cuerpo mínimo válido
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + LoginController.tokenJwt);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Partida actualizada en Symfony");
        }
        else
        {
            Debug.LogError("❌ Error al actualizar partida en Symfony: " + request.error);
        }
    }

    [System.Serializable]
    public class UpdatePartidaData
    {
        public int player2;
        public string status;
    }
}
