using System;
using System.Collections.Generic;
using System.Linq;

namespace MataGames.Controllers
{
    public class ImpostorGameController
    {
        // Corregido a public para que no de error en las fotos
        public List<string> Jugadores { get; private set; }
        private Dictionary<string, string> _roles;
        private int _turnoActualIndex;

        public string PalabraSecreta { get; private set; }

        public int ImpostoresRestantes => _roles.Values.Count(r => r == "IMPOSTOR");
        // Para saber cuántos inocentes quedan
        public int InocentesRestantes => _roles.Values.Count(r => r != "IMPOSTOR");

        public ImpostorGameController(List<string> jugadores, List<string> palabras, int numImpostores)
        {
            Jugadores = jugadores.ToList(); // Clonamos la lista para trabajar
            _roles = new Dictionary<string, string>();
            _turnoActualIndex = 0;

            AsignarRoles(palabras, numImpostores);
        }

        private void AsignarRoles(List<string> palabras, int numImpostores)
        {
            Random rnd = new Random();
            PalabraSecreta = palabras[rnd.Next(palabras.Count)];

            var jugadoresMezclados = Jugadores.OrderBy(x => rnd.Next()).ToList();

            for (int i = 0; i < jugadoresMezclados.Count; i++)
            {
                if (i < numImpostores)
                    _roles[jugadoresMezclados[i]] = "IMPOSTOR";
                else
                    _roles[jugadoresMezclados[i]] = PalabraSecreta;
            }
        }

        public void EliminarJugador(string nombre)
        {
            Jugadores.Remove(nombre);
            _roles.Remove(nombre);
        }

        public string ObtenerJugadorActual() => _turnoActualIndex < Jugadores.Count ? Jugadores[_turnoActualIndex] : null;

        public string ObtenerRolActual()
        {
            string jugador = ObtenerJugadorActual();
            return (jugador != null && _roles.ContainsKey(jugador)) ? _roles[jugador] : "";
        }

        public bool VerificarSiEsImpostor(string nombreJugador)
        {
            return _roles.ContainsKey(nombreJugador) && _roles[nombreJugador] == "IMPOSTOR";
        }

        public bool EsImpostorActual() => ObtenerRolActual() == "IMPOSTOR";

        public bool AvanzarTurno() => ++_turnoActualIndex < Jugadores.Count;
    }
}