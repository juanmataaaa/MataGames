using MataGames.Models;

namespace MataGames.Controllers
{
    public class InicioController
    {
        public Jugador CrearJugador(string nombre)
        {
            return new Jugador
            {
                Nombre = nombre
            };
        }
    }
}