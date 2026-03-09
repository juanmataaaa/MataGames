using System;
using System.Collections.Generic;
using System.Linq;

namespace MataGames.Controllers
{
    public class ConnectFourController
    {
        public int[,] Tablero { get; private set; }
        public bool EsTurnoJugador { get; set; }
        public bool JuegoTerminado { get; private set; }
        public Dificultad NivelDificultad { get; set; }

        public List<int[]> FichasGanadoras { get; private set; } = new List<int[]>();

        public ConnectFourController()
        {
            Tablero = new int[6, 7];
            EsTurnoJugador = true;
            JuegoTerminado = false;
            NivelDificultad = Dificultad.Normal;
        }

        public int DropFicha(int columna, int jugador)
        {
            if (columna < 0 || columna > 6 || JuegoTerminado) return -1;
            for (int fila = 5; fila >= 0; fila--)
            {
                if (Tablero[fila, columna] == 0)
                {
                    Tablero[fila, columna] = jugador;
                    return fila;
                }
            }
            return -1;
        }

        public int ObtenerMovimientoBot()
        {
            Random rnd = new Random();
            int mejorColumna = -1;

            switch (NivelDificultad)
            {
                case Dificultad.Facil:
                    mejorColumna = MovimientoAleatorio(rnd);
                    break;
                case Dificultad.Normal:
                    // Normal: Piensa 2 jugadas por adelantado el 50% de las veces
                    mejorColumna = (rnd.Next(100) < 50) ? ObtenerMejorMovimientoMinimax(2) : MovimientoAleatorio(rnd);
                    break;
                case Dificultad.Dificil:
                    // Difícil: Piensa 4 jugadas por adelantado
                    mejorColumna = ObtenerMejorMovimientoMinimax(4);
                    break;
                case Dificultad.Imposible:
                    // Calavera: Piensa 6 jugadas por adelantado. ¡Indestructible!
                    mejorColumna = ObtenerMejorMovimientoMinimax(6);
                    break;
            }

            if (mejorColumna == -1) mejorColumna = MovimientoAleatorio(rnd);
            return mejorColumna;
        }

        private int MovimientoAleatorio(Random rnd)
        {
            var disponibles = Enumerable.Range(0, 7).Where(c => Tablero[0, c] == 0).ToList();
            return disponibles.Count > 0 ? disponibles[rnd.Next(disponibles.Count)] : -1;
        }

        // 👉 INICIO DEL ALGORITMO MINIMAX
        private int ObtenerMejorMovimientoMinimax(int profundidadMax)
        {
            int mejorPuntaje = int.MinValue;
            int mejorColumna = -1;
            int[] orden = { 3, 2, 4, 1, 5, 0, 6 }; // Siempre prioriza evaluar el centro primero

            foreach (int c in orden)
            {
                if (Tablero[0, c] != 0) continue;

                int f = SimularTiro(c, 2);
                int puntaje = Minimax(profundidadMax - 1, int.MinValue, int.MaxValue, false);
                DeshacerTiro(f, c);

                if (puntaje > mejorPuntaje)
                {
                    mejorPuntaje = puntaje;
                    mejorColumna = c;
                }
            }
            return mejorColumna;
        }

        private int Minimax(int profundidad, int alfa, int beta, bool esMaximizador)
        {
            int puntajeEval = EvaluarTableroCompleto();

            if (puntajeEval >= 900000) return puntajeEval - (10 - profundidad);
            if (puntajeEval <= -900000) return puntajeEval + (10 - profundidad);
            if (profundidad == 0 || EsEmpateVirtual()) return puntajeEval;

            int[] orden = { 3, 2, 4, 1, 5, 0, 6 };

            if (esMaximizador)
            {
                int maxEval = int.MinValue;
                foreach (int c in orden)
                {
                    if (Tablero[0, c] != 0) continue;
                    int f = SimularTiro(c, 2);
                    int eval = Minimax(profundidad - 1, alfa, beta, false);
                    DeshacerTiro(f, c);
                    maxEval = Math.Max(maxEval, eval);
                    alfa = Math.Max(alfa, eval);
                    if (beta <= alfa) break; // Poda: no calcula si ya sabe que es malo
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (int c in orden)
                {
                    if (Tablero[0, c] != 0) continue;
                    int f = SimularTiro(c, 1);
                    int eval = Minimax(profundidad - 1, alfa, beta, true);
                    DeshacerTiro(f, c);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alfa) break; // Poda
                }
                return minEval;
            }
        }

        // 👉 CEREBRO MATEMÁTICO: PUNTÚA EL TABLERO
        private int EvaluarTableroCompleto()
        {
            int puntaje = 0;

            int fichasCentro = 0;
            for (int r = 0; r < 6; r++) if (Tablero[r, 3] == 2) fichasCentro++;
            puntaje += fichasCentro * 3;

            for (int r = 0; r < 6; r++)
                for (int c = 0; c < 4; c++)
                    puntaje += EvaluarVentana(Tablero[r, c], Tablero[r, c + 1], Tablero[r, c + 2], Tablero[r, c + 3]);

            for (int c = 0; c < 7; c++)
                for (int r = 0; r < 3; r++)
                    puntaje += EvaluarVentana(Tablero[r, c], Tablero[r + 1, c], Tablero[r + 2, c], Tablero[r + 3, c]);

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 4; c++)
                    puntaje += EvaluarVentana(Tablero[r, c], Tablero[r + 1, c + 1], Tablero[r + 2, c + 2], Tablero[r + 3, c + 3]);

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 4; c++)
                    puntaje += EvaluarVentana(Tablero[r + 3, c], Tablero[r + 2, c + 1], Tablero[r + 1, c + 2], Tablero[r, c + 3]);

            return puntaje;
        }

        private int EvaluarVentana(int v1, int v2, int v3, int v4)
        {
            int bot = 0, jug = 0, vacio = 0;

            if (v1 == 2) bot++; else if (v1 == 1) jug++; else vacio++;
            if (v2 == 2) bot++; else if (v2 == 1) jug++; else vacio++;
            if (v3 == 2) bot++; else if (v3 == 1) jug++; else vacio++;
            if (v4 == 2) bot++; else if (v4 == 1) jug++; else vacio++;

            if (bot == 4) return 1000000;
            if (jug == 4) return -1000000;

            int score = 0;
            if (bot == 3 && vacio == 1) score += 50;
            else if (bot == 2 && vacio == 2) score += 10;

            if (jug == 3 && vacio == 1) score -= 80; // Defender vale más
            else if (jug == 2 && vacio == 2) score -= 20;

            return score;
        }

        private int SimularTiro(int c, int j)
        {
            for (int f = 5; f >= 0; f--) { if (Tablero[f, c] == 0) { Tablero[f, c] = j; return f; } }
            return -1;
        }

        private void DeshacerTiro(int f, int c) { Tablero[f, c] = 0; }

        private bool EsEmpateVirtual()
        {
            for (int c = 0; c < 7; c++) if (Tablero[0, c] == 0) return false;
            return true;
        }

        public string VerificarEstadoJuego(int ultimaFila, int ultimaCol)
        {
            int jugador = Tablero[ultimaFila, ultimaCol];
            if (ComprobarGanadorSilencioso(ultimaFila, ultimaCol, jugador))
            {
                JuegoTerminado = true;
                return jugador == 1 ? "X" : "O";
            }

            if (EsEmpateVirtual())
            {
                JuegoTerminado = true;
                return "Empate";
            }
            return null;
        }

        private bool ComprobarGanadorSilencioso(int r, int c, int j)
        {
            FichasGanadoras.Clear();
            List<int[]> temporal = new List<int[]>();

            for (int i = 0; i < 7; i++) { if (Tablero[r, i] == j) { temporal.Add(new int[] { r, i }); if (temporal.Count == 4) { FichasGanadoras = new List<int[]>(temporal); return true; } } else temporal.Clear(); }

            temporal.Clear();
            for (int i = 0; i < 6; i++) { if (Tablero[i, c] == j) { temporal.Add(new int[] { i, c }); if (temporal.Count == 4) { FichasGanadoras = new List<int[]>(temporal); return true; } } else temporal.Clear(); }

            temporal.Clear();
            for (int i = -3; i <= 3; i++) { int fr = r + i, fc = c + i; if (fr >= 0 && fr < 6 && fc >= 0 && fc < 7 && Tablero[fr, fc] == j) { temporal.Add(new int[] { fr, fc }); if (temporal.Count == 4) { FichasGanadoras = new List<int[]>(temporal); return true; } } else temporal.Clear(); }

            temporal.Clear();
            for (int i = -3; i <= 3; i++) { int fr = r - i, fc = c + i; if (fr >= 0 && fr < 6 && fc >= 0 && fc < 7 && Tablero[fr, fc] == j) { temporal.Add(new int[] { fr, fc }); if (temporal.Count == 4) { FichasGanadoras = new List<int[]>(temporal); return true; } } else temporal.Clear(); }

            return false;
        }

        public void ReiniciarJuego()
        {
            Tablero = new int[6, 7];
            JuegoTerminado = false;
            EsTurnoJugador = true;
            FichasGanadoras.Clear();
        }
    }
}