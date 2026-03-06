using System;
using System.Collections.Generic;
using System.Linq;

namespace MataGames.Controllers
{
    public class ConnectFourController
    {
        // Tablero de 6 filas (alto) x 7 columnas (ancho)
        // 0=Vacío, 1=Jugador (X/Verde), 2=Bot (O/Rojo)
        public int[,] Tablero { get; private set; }
        public bool EsTurnoJugador { get; set; }
        public bool JuegoTerminado { get; private set; }
        public Dificultad NivelDificultad { get; set; }

        public ConnectFourController()
        {
            Tablero = new int[6, 7];
            EsTurnoJugador = true;
            JuegoTerminado = false;
            NivelDificultad = Dificultad.Normal;
        }

        // Simula la gravedad: busca la fila más baja vacía en una columna
        public int DropFicha(int columna, int jugador)
        {
            if (columna < 0 || columna > 6 || JuegoTerminado) return -1;

            for (int fila = 5; fila >= 0; fila--) // Empezamos desde abajo
            {
                if (Tablero[fila, columna] == 0)
                {
                    Tablero[fila, columna] = jugador;
                    return fila; // Devolvemos la fila donde cayó
                }
            }
            return -1; // Columna llena
        }

        public int ObtenerMovimientoBot()
        {
            Random rnd = new Random();
            int colMinimax = MovimientoInteligente();

            // Si la IA es "imposible", siempre usa Minimax. Si no, a veces falla.
            int probabilidadAcierto = NivelDificultad == Dificultad.Dificil ? 85 : 50;
            if (NivelDificultad == Dificultad.Imposible || rnd.Next(100) < probabilidadAcierto)
            {
                if (colMinimax != -1) return colMinimax;
            }

            // Movimiento aleatorio como fallback
            var columnasDisponibles = Enumerable.Range(0, 7).Where(c => Tablero[0, c] == 0).ToList();
            return columnasDisponibles.Count > 0 ? columnasDisponibles[rnd.Next(columnasDisponibles.Count)] : -1;
        }

        // IA Básica (bloqueo/ganancia instantánea) para simular jugabilidad real
        private int MovimientoInteligente()
        {
            // 1. ¿Puede ganar el Bot?
            for (int c = 0; c < 7; c++)
            {
                int fila = DropFichaSilencioso(c, 2);
                if (fila != -1)
                {
                    bool gana = ComprobarGanadorSilencioso(fila, c, 2);
                    Tablero[fila, c] = 0; // Revertimos
                    if (gana) return c;
                }
            }

            // 2. ¿Puede ganar el Jugador? (Bloquear)
            for (int c = 0; c < 7; c++)
            {
                int fila = DropFichaSilencioso(c, 1);
                if (fila != -1)
                {
                    bool gana = ComprobarGanadorSilencioso(fila, c, 1);
                    Tablero[fila, c] = 0; // Revertimos
                    if (gana) return c;
                }
            }

            // 3. Preferir el centro
            int[] orden = { 3, 2, 4, 1, 5, 0, 6 };
            foreach (int c in orden) { if (Tablero[0, c] == 0) return c; }

            return -1;
        }

        private int DropFichaSilencioso(int c, int j)
        {
            for (int f = 5; f >= 0; f--) { if (Tablero[f, c] == 0) { Tablero[f, c] = j; return f; } }
            return -1;
        }

        public string VerificarEstadoJuego(int ultimaFila, int ultimaCol)
        {
            int jugador = Tablero[ultimaFila, ultimaCol];
            if (ComprobarGanadorSilencioso(ultimaFila, ultimaCol, jugador))
            {
                JuegoTerminado = true;
                return jugador == 1 ? "X" : "O";
            }

            if (Enumerable.Range(0, 7).All(c => Tablero[0, c] != 0))
            {
                JuegoTerminado = true;
                return "Empate";
            }

            return null;
        }

        private bool ComprobarGanadorSilencioso(int r, int c, int j)
        {
            // 1. Horizontal (4 en fila)
            int count = 0;
            for (int i = 0; i < 7; i++) { if (Tablero[r, i] == j) { if (++count == 4) return true; } else count = 0; }

            // 2. Vertical (4 en columna)
            count = 0;
            for (int i = 0; i < 6; i++) { if (Tablero[i, c] == j) { if (++count == 4) return true; } else count = 0; }

            // 3. Diagonal \
            count = 0;
            for (int i = -3; i <= 3; i++) { int fr = r + i, fc = c + i; if (fr >= 0 && fr < 6 && fc >= 0 && fc < 7 && Tablero[fr, fc] == j) { if (++count == 4) return true; } else count = 0; }

            // 4. Diagonal /
            count = 0;
            for (int i = -3; i <= 3; i++) { int fr = r - i, fc = c + i; if (fr >= 0 && fr < 6 && fc >= 0 && fc < 7 && Tablero[fr, fc] == j) { if (++count == 4) return true; } else count = 0; }

            return false;
        }

        public void ReiniciarJuego()
        {
            Tablero = new int[6, 7];
            JuegoTerminado = false;
            EsTurnoJugador = true;
        }
    }
}