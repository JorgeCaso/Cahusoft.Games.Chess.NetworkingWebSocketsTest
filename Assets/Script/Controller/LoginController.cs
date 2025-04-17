using System.Collections;
using System.Text;
using TMPro;
using UnityEngine.Networking;
using UnityEngine;
using Assets.Script.Model;
using Assets.Script.Dtos;

namespace Assets.Script.Controller
{
    internal class LoginController : MonoBehaviour
    {
        public TMP_InputField inputUsuario;
        public TMP_InputField inputPassword;
        public TMP_Text textoLog;

        public static string tokenJwt;
        public static User user;

        public void Login()
        {
            string user = inputUsuario.text.Trim();
            string pass = inputPassword.text.Trim();

            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                StartCoroutine(EnviarLogin(user, pass));
            }
            else
            {
                textoLog.text += "\n⚠️ Usuario o contraseña vacíos";
            }
        }

        IEnumerator EnviarLogin(string user, string pass)
        {
            var payload = new LoginDataDto { email = user, password = pass };
            string jsonData = JsonUtility.ToJson(payload);

            UnityWebRequest request = new UnityWebRequest(AppSettings.ApiLoginUrl, "POST");
            byte[] body = new UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResponseDto response = JsonUtility.FromJson<LoginResponseDto>(request.downloadHandler.text);
                AppSettings.token = response.token;

                textoLog.text += "\nLogin exitoso";
                StartCoroutine(ObtenerPerfil());
            }
            else
            {
                textoLog.text += $"\n❌ Error de login: {request.error}";
            }
        }

        IEnumerator ObtenerPerfil()
        {
            UnityWebRequest request = UnityWebRequest.Get(AppSettings.ApiProfileUrl);
            request.SetRequestHeader("Authorization", "Bearer " + AppSettings.token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // ⚠️ Extrae solo los campos que interesan
                string json = request.downloadHandler.text;
                UserProfileDto wrapper = JsonUtility.FromJson<UserProfileDto>(json);

                user = new User
                {
                    id = wrapper.id,
                    email = wrapper.email
                };

                textoLog.text += $"\n Usuario: {user.email}";

                MatchSettings.playerId = user.id.ToString();
            }
            else
            {
                textoLog.text += $"\n❌ Error al obtener perfil: {request.error}";
            }
        }
    }
}
