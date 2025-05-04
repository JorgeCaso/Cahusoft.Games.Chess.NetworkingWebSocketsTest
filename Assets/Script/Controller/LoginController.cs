using System.Collections;
using System.Text;
using TMPro;
using UnityEngine.Networking;
using UnityEngine;
using Assets.Script.Model;
using Assets.Script.Dtos;
using UnityEngine.UI;

namespace Assets.Script.Controller
{
    internal class LoginController : MonoBehaviour
    {
        public TMP_Text textoLog;
        public Button buttonUserAdmin;
        public Button buttonUser1;
        public Button buttonUser2;
        public static User user;

        void Start()
        {
            buttonUserAdmin.onClick.AddListener(() => Login("admin"));
            buttonUser1.onClick.AddListener(() => Login("user1"));
            buttonUser2.onClick.AddListener(() => Login("user2"));
        }

        public void Login(string tipo)
        {
            string user = "";
            string pass = "";

            switch (tipo) {
                case "admin":
                    user = "admin@example.com";
                    pass = "adminpass";
                    break;
                case "user1":
                    user = "user1@example.com";
                    pass = "user1pass";
                    break;
                case "user2":
                    user = "user2@example.com";
                    pass = "user2pass";
                    break;
            }

            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                StartCoroutine(EnviarLogin(user, pass));
            }
            else
            {
                textoLog.text += "\nUsuario o contraseña vacíos";
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
                textoLog.text += $"\nError de login: {request.error}";
            }
        }

        IEnumerator ObtenerPerfil()
        {
            UnityWebRequest request = UnityWebRequest.Get(AppSettings.ApiProfileUrl);
            request.SetRequestHeader("Authorization", "Bearer " + AppSettings.token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                UserProfileDto wrapper = JsonUtility.FromJson<UserProfileDto>(json);

                user = new User
                {
                    id = wrapper.id,
                    email = wrapper.email
                };

                textoLog.text += $"\nUsuario: {user.email}";

                AppSettings.userId = user.id;
            }
            else
            {
                textoLog.text += $"\nError al obtener perfil: {request.error}";
            }
        }
    }
}
