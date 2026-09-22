//──────────────────────────────────────────────────────────────
// RelayConfig.cs
// One place to point every RemoteInput script at your deployed
// relay server. Fill this in after following RelayServer/README.md.
//──────────────────────────────────────────────────────────────

namespace ALIyerEdon.RemoteInput
{
    public static class RelayConfig
    {
        /// <summary>
        /// WebSocket URL of your deployed relay server, e.g.
        /// "wss://relay.yourdomain.com/relay". Must use wss:// (secure)
        /// once the WebGL build is served over https — browsers block
        /// a page loaded over https from opening an insecure ws:// connection.
        /// </summary>
        public const string ServerUrl = "wss://YOUR-RELAY-SERVER-DOMAIN/relay";
    }
}
