using Microsoft.AspNetCore.SignalR.Client;

namespace MataGames.Services
{
    public class TicTacToeService
    {
        private HubConnection _connection;
        public HubConnection HubConnection => _connection;
        public event Action<int, string> OnMoveReceived;

        public TicTacToeService()
        {
            // Inicializamos la conexión AQUÍ para que nunca sea null
            _connection = new HubConnectionBuilder()
                .WithUrl("https://matagames-server-juanma-abcwc0ekdghkb9c3.canadacentral-01.azurewebsites.net/gamehub")
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task Conectar()
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected) return;

                // Configuramos la escucha de movimientos
                _connection.On<int, string>("ReceiveMove", (indice, ficha) =>
                {
                    OnMoveReceived?.Invoke(indice, ficha);
                });

                await _connection.StartAsync();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error SignalR: {ex.Message}"); }
        }

        public async Task UnirseASala(string salaId, string nombre)
        {
            if (_connection.State == HubConnectionState.Connected)
                await _connection.InvokeAsync("JoinRoom", salaId, nombre);
        }

        public async Task EnviarMovimiento(string salaId, int indice, string ficha)
        {
            if (_connection.State == HubConnectionState.Connected)
                await _connection.InvokeAsync("SendMove", salaId, indice, ficha);
        }
    }
}