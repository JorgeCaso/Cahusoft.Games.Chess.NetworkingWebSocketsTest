using Assets.Script.Dtos;
using Assets.Script.Interfaces;
using Assets.Script.Model;
using Assets.Script.Networking;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.Script.Controller
{
    public class CrearPartidaController : MonoBehaviour
    {

        public TMP_Text textoLog;
        public Button buttonCrearPartida;
        public ChessWebSocket websocket;

        void Start()
        {
            buttonCrearPartida.onClick.AddListener(() => StartCoroutine(CrearPartida()));
        }

        private IEnumerator CrearPartida()
        {

            yield return StartCoroutine(EnviarCrearPartidaASymfony());
        }

        IEnumerator EnviarCrearPartidaASymfony()
        {
            CrearPartidaDataDto payload = new CrearPartidaDataDto
            {
            };

            string jsonData = JsonUtility.ToJson(payload);

            UnityWebRequest request = new UnityWebRequest(AppSettings.ApiGames, "POST");
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
                MatchSettings.gameId = int.Parse(respuesta.gameId);
            }
            else
            {
                textoLog.text += $"\nError al crear partida: {request.error}";
            }
        }

    }
}
