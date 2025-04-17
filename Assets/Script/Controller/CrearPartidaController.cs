using Assets.Script.Dtos;
using Assets.Script.Model;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Script.Controller
{
    public class CrearPartidaController : MonoBehaviour
    {
        public ChessWebSocket socket;
        public TMP_Text textoLog;

        private string apiUrl = $"{AppSettings.BaseUrlChessWebApi}/api/games";

        public void CrearPartida()
        {
            StartCoroutine(EnviarCrearPartidaASymfony());
        }

        IEnumerator EnviarCrearPartidaASymfony()
        {
            CrearPartidaDataDto payload = new CrearPartidaDataDto
            {
                color = "W"
            };

            string jsonData = JsonUtility.ToJson(payload);

            UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + AppSettings.token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                CrearPartidaResponseDto respuesta = JsonUtility.FromJson<CrearPartidaResponseDto>(json);
                textoLog.text += $"\nPartida creada con ID: {respuesta.gameId}";

                MatchSettings.fenCurrent = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
                MatchSettings.gameId = respuesta.gameId;

                socket.UnirseAPartida(respuesta.gameId);
            }
            else
            {
                textoLog.text += $"\n❌ Error al crear partida: {request.error}";
            }
        }

    }
}
