
namespace Assets.Script.Model
{
    public class AppSettings
    {
        public const string BaseUrlChessWebSockets = "ws://localhost:3002";
        public const string BaseUrlChessWebApi = "https://chesswebapi:4443";
        public static readonly string ApiLoginUrl = $"{BaseUrlChessWebApi}/auth";
        public static readonly string ApiProfileUrl = $"{BaseUrlChessWebApi}/api/profile";
        public static string token = "";

    }
}
