using System;
using System.Collections.Generic;
using System.Linq;

namespace MataGames.Controllers
{
    public enum Dificultad { Facil, Normal, Dificil, Imposible }

    public class TicTacToeController
    {
        public string[] Tablero { get; private set; } = new string[9];
        public bool EsTurnoJugador { get; set; } = true;
        public bool JuegoTerminado { get; set; }
        public Dificultad NivelDificultad { get; set; } = Dificultad.Normal;
        public int[] IndicesGanadores { get; private set; }
        public string NombreRival { get; set; } = "Rival";

        public void ReiniciarJuego()
        {
            Tablero = new string[9];
            JuegoTerminado = false;
            EsTurnoJugador = true;
            IndicesGanadores = null;
        }

        public bool HacerMovimiento(int indice, string jugador)
        {
            if (string.IsNullOrEmpty(Tablero[indice]) && !JuegoTerminado)
            {
                Tablero[indice] = jugador;
                return true;
            }
            return false;
        }

        public string VerificarEstadoJuego()
        {
            int[,] winPos = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };
            for (int i = 0; i < 8; i++)
            {
                if (Tablero[winPos[i, 0]] != null && Tablero[winPos[i, 0]] == Tablero[winPos[i, 1]] && Tablero[winPos[i, 0]] == Tablero[winPos[i, 2]])
                {
                    IndicesGanadores = new int[] { winPos[i, 0], winPos[i, 1], winPos[i, 2] };
                    JuegoTerminado = true;
                    return Tablero[winPos[i, 0]];
                }
            }
            if (Tablero.All(x => x != null)) { JuegoTerminado = true; return "Empate"; }
            return null;
        }

        // --- INTELIGENCIA ARTIFICIAL NIVELADA ---
        public int ObtenerMovimientoBot()
        {
            Random rnd = new Random();
            switch (NivelDificultad)
            {
                case Dificultad.Facil:
                    return MovimientoAleatorio();

                case Dificultad.Normal:
                    int movNormal = BuscarAtaqueODefensa();
                    return movNormal != -1 ? movNormal : MovimientoAleatorio();

                case Dificultad.Dificil:
                    if (rnd.Next(100) < 80) return MovimientoMinimax();
                    int movDificil = BuscarAtaqueODefensa();
                    return movDificil != -1 ? movDificil : MovimientoAleatorio();

                case Dificultad.Imposible:
                    return MovimientoMinimax();

                default:
                    return MovimientoAleatorio();
            }
        }

        private int MovimientoAleatorio()
        {
            var disponibles = Tablero.Select((v, i) => new { v, i }).Where(x => x.v == null).Select(x => x.i).ToList();
            return disponibles.Count > 0 ? disponibles[new Random().Next(disponibles.Count)] : -1;
        }

        // Lógica humana: ataca si puede ganar, defiende si le van a ganar
        private int BuscarAtaqueODefensa()
        {
            // 1. ¿Puedo ganar en este turno? (El bot es la 'O')
            for (int i = 0; i < 9; i++)
            {
                if (Tablero[i] == null)
                {
                    Tablero[i] = "O";
                    if (VerificarGanadorInterno(Tablero) == "O") { Tablero[i] = null; return i; }
                    Tablero[i] = null;
                }
            }
            // 2. ¿El jugador me va a ganar en su próximo turno? (El jugador es la 'X')
            for (int i = 0; i < 9; i++)
            {
                if (Tablero[i] == null)
                {
                    Tablero[i] = "X";
                    if (VerificarGanadorInterno(Tablero) == "X") { Tablero[i] = null; return i; }
                    Tablero[i] = null;
                }
            }
            return -1;
        }

        // Algoritmo perfecto e invencible
        private int MovimientoMinimax()
        {
            int mejorPuntaje = int.MinValue;
            int movimiento = -1;
            for (int i = 0; i < 9; i++)
            {
                if (Tablero[i] == null)
                {
                    Tablero[i] = "O";
                    int puntaje = Minimax(Tablero, 0, false);
                    Tablero[i] = null;
                    if (puntaje > mejorPuntaje) { mejorPuntaje = puntaje; movimiento = i; }
                }
            }
            return movimiento;
        }

        private int Minimax(string[] board, int depth, bool isMaximizing)
        {
            string res = VerificarGanadorInterno(board);
            if (res == "O") return 10 - depth;
            if (res == "X") return depth - 10;
            if (res == "Empate") return 0;

            if (isMaximizing)
            {
                int bestScore = int.MinValue;
                for (int i = 0; i < 9; i++)
                {
                    if (board[i] == null)
                    {
                        board[i] = "O";
                        bestScore = Math.Max(Minimax(board, depth + 1, false), bestScore);
                        board[i] = null;
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int i = 0; i < 9; i++)
                {
                    if (board[i] == null)
                    {   
                        board[i] = "X";
                        bestScore = Math.Min(Minimax(board, depth + 1, true), bestScore);
                        board[i] = null;
                    }
                }
                return bestScore;
            }
        }

        // Función interna rápida para que el bot simule las jugadas en su cabeza
        private string VerificarGanadorInterno(string[] b)
        {
            int[,] winPos = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };
            for (int i = 0; i < 8; i++)
            {
                if (b[winPos[i, 0]] != null && b[winPos[i, 0]] == b[winPos[i, 1]] && b[winPos[i, 0]] == b[winPos[i, 2]])
                    return b[winPos[i, 0]];
            }
            return b.All(x => x != null) ? "Empate" : null;
        }
    }
}