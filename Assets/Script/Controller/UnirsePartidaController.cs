using Assets.Script.Dtos;
using Assets.Script.Dtos.Messages;
using Assets.Script.Model;
using Assets.Script.Networking;
using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.Script.Controller
{
    public class UnirsePartidaController : MonoBehaviour
    {
        public TMP_InputField inputColor;
        public TMP_InputField inputIdPartida;
        public Button buttonUnirse;
        public TMP_Text textoLog;
        public TMP_Text textoWhite;
        public TMP_Text textoBlack;
        public ChessWebSocket websocket;

        private void Start()
        {
            buttonUnirse.onClick.AddListener(() => StartCoroutine(UnirsePartida()));
            websocket.OnUnidoAPartida += ConfirmacionRecibida;
            websocket.OnUnidoPartidaListo += ConfirmacionListoRecibida;
        }

        private void ConfirmacionListoRecibida(string obj)
        {
            Debug.Log("La partida: " + obj + ", está en estado LISTO (Hay 2 jugadores)");
        }

        private IEnumerator UnirsePartida()
        {
            string color = inputColor.text.Trim();
            if (string.IsNullOrEmpty(color))
            {
                textoLog.text += "\nColor vacío.";
                yield break;
            }

            string partidaIdTexto = inputIdPartida.text.Trim();
            if (string.IsNullOrEmpty(partidaIdTexto))
            {
                textoLog.text += "\nID de partida vacío.";
                yield break;
            }

            if (!int.TryParse(partidaIdTexto, out int partidaId))
            {
                textoLog.text += "\nID de partida inválido.";
                yield break;
            }

            yield return StartCoroutine(UnirseEnSymfony(partidaId));
        }


        private IEnumerator UnirseEnSymfony(int? partidaId)
        {
            UnirsePartidaDataDto payload = new()
            {
                color = inputColor.text
            };

            string jsonData = JsonUtility.ToJson(payload);
            if (partidaId == MatchSettings.gameId &&  (MatchSettings.whiteId == AppSettings.userId || MatchSettings.blackId == AppSettings.userId))
            {
                textoLog.text += "Ya eres un jugador en la partida.";
                yield break;
            }

            string joinUrl = $"{AppSettings.BaseUrlChessWebApi}/api/games/{partidaId}/join";
            UnityWebRequest postRequest = new UnityWebRequest(joinUrl, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            postRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            postRequest.downloadHandler = new DownloadHandlerBuffer();
            postRequest.SetRequestHeader("Authorization", $"Bearer {AppSettings.token}");
            postRequest.SetRequestHeader("Content-Type", "application/json");
            yield return postRequest.SendWebRequest();

            if (postRequest.result == UnityWebRequest.Result.Success)
            {
                string json = postRequest.downloadHandler.text;
                CrearPartidaResponseDto respuesta = JsonUtility.FromJson<CrearPartidaResponseDto>(json);

                textoLog.text += $"Te has unido a la partida {partidaId}";
                websocket.UnirseAPartida(partidaId);
            }
            else
            {
                textoLog.text += $"Error al unirse en Symfony: {postRequest.error}";
            }
        }

        private void ConfirmacionRecibida(JugadorEntraCliente desde)
        {
            MatchSettings.gameId = desde.body.id;
            MatchSettings.whiteId = desde.body.white.id;
            MatchSettings.blackId = desde.body.black.id;

            textoLog.text += "Confirmación WebSocket desde:" + desde.message;
            textoWhite.text = "White: " + desde.body.white.email;
            textoBlack.text = "Black: " + desde.body.black.email;
        }
    }
}
