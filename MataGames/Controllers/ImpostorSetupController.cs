using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace MataGames.Controllers
{
    public class ImpostorSetupController
    {
        public ObservableCollection<string> Jugadores { get; set; }
        public ObservableCollection<string> Palabras { get; set; }
        public int NumeroImpostores { get; private set; }

        // Nuevas variables para saber si estamos usando un paquete
        public bool EsModoAutomatico { get; private set; }
        public string NombrePaqueteAutomatico { get; private set; }

        public ImpostorSetupController(string creadorDeLaSala)
        {
            Jugadores = new ObservableCollection<string>();
            Palabras = new ObservableCollection<string>();
            NumeroImpostores = 1;
            EsModoAutomatico = false; // Por defecto empezamos en manual

            if (!string.IsNullOrWhiteSpace(creadorDeLaSala))
            {
                Jugadores.Add(creadorDeLaSala);
            }
        }

        public bool AgregarJugador(string nombre)
        {
            if (!string.IsNullOrWhiteSpace(nombre) && !Jugadores.Contains(nombre.Trim()))
            {
                Jugadores.Add(nombre.Trim());
                return true; // Éxito
            }
            return false; // Duplicado o vacío
        }

        public bool AgregarPalabra(string palabra)
        {
            if (!EsModoAutomatico && !string.IsNullOrWhiteSpace(palabra) && !Palabras.Contains(palabra.Trim()))
            {
                Palabras.Add(palabra.Trim());
                return true; // Éxito
            }
            return false; // Duplicado o modo automático
        }

        // Función para cargar las palabras del paquete automático
        public void CargarPaqueteAutomatico(string nombrePaquete, List<string> nuevasPalabras)
        {
            EsModoAutomatico = true;
            NombrePaqueteAutomatico = nombrePaquete;
            Palabras.Clear(); // Borramos lo que hubiera

            foreach (var p in nuevasPalabras)
            {
                Palabras.Add(p);
            }
        }

        // Si se arrepienten y quieren volver a escribir manual
        public void VolverAModoManual()
        {
            EsModoAutomatico = false;
            NombrePaqueteAutomatico = string.Empty;
            Palabras.Clear();
        }

        public void ModificarImpostores(int cantidad)
        {
            if (cantidad >= 1) NumeroImpostores = cantidad;
        }
    }
}