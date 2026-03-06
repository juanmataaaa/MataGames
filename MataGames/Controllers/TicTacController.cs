using System;
using System.Collections.Generic;
using System.Linq;

namespace MataGames.Controllers
{
    public enum Dificultad { Facil, Normal, Dificil, Imposible }

    public class TicTacToeController
    {
        public string[] Tablero { get; private set; }
        public bool EsTurnoJugador { get; set; }
        public bool JuegoTerminado { get; private set; }
        public Dificultad NivelDificultad { get; set; }

        public TicTacToeController()
        {
            Tablero = new string[9];
            EsTurnoJugador = true;
            JuegoTerminado = false;
            NivelDificultad = Dificultad.Normal; // Por defecto
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

        public int ObtenerMovimientoBot()
        {
            Random rnd = new Random();

            switch (NivelDificultad)
            {
                case Dificultad.Facil:
                    return MovimientoAleatorio();

                case Dificultad.Normal:
                    // 50% de probabilidad de jugar bien
                    return rnd.Next(100) < 50 ? MovimientoMinimax() : MovimientoAleatorio();

                case Dificultad.Dificil:
                    // 85% de probabilidad de jugar perfecto
                    return rnd.Next(100) < 85 ? MovimientoMinimax() : MovimientoAleatorio();

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
                    if (puntaje > mejorPuntaje)
                    {
                        mejorPuntaje = puntaje;
                        movimiento = i;
                    }
                }
            }
            return movimiento;
        }

        private int Minimax(string[] board, int depth, bool isMaximizing)
        {
            string res = VerificarGanadorSilencioso(board);
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
                        int score = Minimax(board, depth + 1, false);
                        board[i] = null;
                        bestScore = Math.Max(score, bestScore);
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
                        int score = Minimax(board, depth + 1, true);
                        board[i] = null;
                        bestScore = Math.Min(score, bestScore);
                    }
                }
                return bestScore;
            }
        }

        public string VerificarEstadoJuego()
        {
            string res = VerificarGanadorSilencioso(Tablero);
            if (res != null) JuegoTerminado = true;
            return res;
        }

        private string VerificarGanadorSilencioso(string[] b)
        {
            int[,] winPos = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };
            for (int i = 0; i < 8; i++)
            {
                if (b[winPos[i, 0]] != null && b[winPos[i, 0]] == b[winPos[i, 1]] && b[winPos[i, 0]] == b[winPos[i, 2]])
                    return b[winPos[i, 0]];
            }
            return b.All(x => x != null) ? "Empate" : null;
        }

        public void ReiniciarJuego()
        {
            Tablero = new string[9];
            JuegoTerminado = false;
            EsTurnoJugador = true;
        }
    }
}