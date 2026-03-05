using MataGames.Models;

namespace MataGames.Controllers
{
    public class NombreJugadorController
    {
        private Jugador _jugador;

        // El controlador recibe el modelo
        public NombreJugadorController(Jugador jugador)
        {
            _jugador = jugador;
        }

        // Lógica para formatear el saludo
        public string GenerarMensajeBienvenida()
        {
            if (_jugador != null && !string.IsNullOrWhiteSpace(_jugador.Nombre))
            {
                return $"¡Bienvenido,\n{_jugador.Nombre}!";
            }
            return "¡Bienvenido!";
        }

        // Lógica para el botón de configuración
        public string ObtenerAvisoConfiguracion()
        {
            return "Ajustes del juego próximamente";
        }
    }
}