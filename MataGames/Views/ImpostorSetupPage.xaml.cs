using MataGames.Controllers;
using MataGames.Models;

namespace MataGames.Views;

public partial class ImpostorSetupPage : ContentPage
{
    private ImpostorSetupController _controller;

    // Comando para controlar la flecha de atrás de la barra superior
    public Command BackCommand { get; }

    public ImpostorSetupPage(Jugador creador)
    {
        InitializeComponent();

        // Inicializamos el comando de ir atrás y conectamos el BindingContext
        BackCommand = new Command(ConfirmarSalida);
        BindingContext = this;

        // Aseguramos que salga el nombre real con la corona
        string nombreReal = creador != null && !string.IsNullOrWhiteSpace(creador.Nombre)
                            ? creador.Nombre
                            : "Propietario";

        _controller = new ImpostorSetupController($"{nombreReal} 👑");

        listaJugadores.ItemsSource = _controller.Jugadores;
        ActualizarBotonPalabras();
    }

    // Bloquea el botón físico de atrás en Android
    protected override bool OnBackButtonPressed()
    {
        ConfirmarSalida();
        return true;
    }

    private async void ConfirmarSalida()
    {
        bool salir = await DisplayAlert("¿Volver atrás?", "Si sales ahora perderás todos los jugadores y palabras añadidas. ¿Estás seguro?", "Sí, salir", "Cancelar");

        if (salir)
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnAddJugadorClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtNuevoJugador.Text))
        {
            // El controlador nos dice si es un éxito o un duplicado
            if (_controller.AgregarJugador(txtNuevoJugador.Text))
            {
                txtNuevoJugador.Text = string.Empty; // Éxito, limpiamos
            }
            else
            {
                // Duplicado o vacío
                await DisplayAlert("Nombre Duplicado", "Ya hay un jugador con este nombre. Por favor, introduce uno diferente.", "OK");
                txtNuevoJugador.Focus();
            }
        }
    }

    private async void OnAddPalabraClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtNuevaPalabra.Text))
        {
            // El controlador nos dice si es un éxito o un duplicado
            if (_controller.AgregarPalabra(txtNuevaPalabra.Text))
            {
                txtNuevaPalabra.Text = string.Empty; // Éxito, limpiamos
                ActualizarBotonPalabras();
            }
            else
            {
                // Duplicado o vacío
                await DisplayAlert("Palabra Duplicada", "Esta palabra ya ha sido añadida. Introduce otra.", "OK");
                txtNuevaPalabra.Focus();
            }
        }
    }
    // Lógica para borrar jugador tocándolo en la lista
    private async void OnJugadorSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string jugadorSeleccionado)
        {
            listaJugadores.SelectedItem = null; // Quita la selección visual

            if (jugadorSeleccionado.Contains("👑"))
            {
                await DisplayAlert("Aviso", "No puedes eliminar al creador de la sala.", "OK");
                return;
            }

            bool borrar = await DisplayAlert("Eliminar Jugador", $"¿Quieres eliminar a '{jugadorSeleccionado}' de la partida?", "Sí, echar", "Cancelar");

            if (borrar)
            {
                _controller.Jugadores.Remove(jugadorSeleccionado);
            }
        }
    }


    private async void OnVerPalabrasClicked(object sender, EventArgs e)
    {
        if (_controller.Palabras.Count == 0)
        {
            await DisplayAlert("Lista Vacía", "Aún no has añadido ninguna palabra secreta.", "OK");
            return;
        }

        string[] palabrasArray = _controller.Palabras.ToArray();
        string accion = await DisplayActionSheet("Palabras Secretas (Toca para borrar)", "Cerrar", null, palabrasArray);

        if (accion != "Cerrar" && accion != null)
        {
            bool borrar = await DisplayAlert("Borrar", $"¿Quieres eliminar '{accion}'?", "Sí", "No");
            if (borrar)
            {
                _controller.Palabras.Remove(accion);
                ActualizarBotonPalabras();
            }
        }
    }

    private void ActualizarBotonPalabras()
    {
        btnVerPalabras.Text = $"👀 Ver Palabras Añadidas ({_controller.Palabras.Count})";
        btnVerPalabras.TextColor = _controller.Palabras.Count > 0 ? Colors.White : Color.FromArgb("#A0A0B0");
    }

    private void OnRestarImpostorClicked(object sender, EventArgs e)
    {
        if (_controller.NumeroImpostores > 1)
        {
            _controller.ModificarImpostores(_controller.NumeroImpostores - 1);
            lblNumImpostores.Text = _controller.NumeroImpostores.ToString();
        }
    }

    private void OnSumarImpostorClicked(object sender, EventArgs e)
    {
        if (_controller.NumeroImpostores < _controller.Jugadores.Count - 1)
        {
            _controller.ModificarImpostores(_controller.NumeroImpostores + 1);
            lblNumImpostores.Text = _controller.NumeroImpostores.ToString();
        }
        else
        {
            DisplayAlert("Aviso", "No puedes añadir más impostores. Añade más amigos primero.", "OK");
        }
    }


    // --- LÓGICA DE PAQUETES AUTOMÁTICOS ---
    private async void OnElegirPaqueteClicked(object sender, EventArgs e)
    {
        // Las opciones que le saldrán al jugador
        string[] paquetes = { "⚽ Futbolistas", "🏠 Cosas de Casa", "🏅 Deportes", "🎮 Videojuegos" };
        string accion = await DisplayActionSheet("Elige un Paquete Temático", "Cancelar", null, paquetes);

        if (accion != "Cancelar" && accion != null)
        {
            List<string> palabrasPaquete = new List<string>();

            // Cargamos el paquete según lo que haya elegido (mínimo 10 palabras por paquete)
            switch (accion)
            {
                case "⚽ Futbolistas":
                    palabrasPaquete = new List<string> {
            "Messi", "Cristiano Ronaldo", "Maradona", "Pelé", "Neymar", "Zidane", "Ronaldinho", "Iniesta", "Xavi", "Mbappé", "Haaland", "Casillas",
            "Modric", "Vinícius", "Bellingham", "Benzema", "Griezmann", "Sergio Ramos", "Piqué", "Puyol", "Busquets", "Kante", "De Bruyne", "Salah",
            "Lewandowski", "Neuer", "Buffon", "Roberto Carlos", "Figo", "Ronaldo Nazário", "Romário", "Stoichkov", "Cruyff", "Platini", "Di Stéfano",
            "Eusebio", "George Best", "Bobby Charlton", "Bobby Moore", "Beckenbauer", "Maldini", "Baresi", "Cafu", "Roberto Baggio", "Del Piero",
            "Totti", "Pirlo", "Seedorf", "Kaká", "Rivaldo", "Thierry Henry", "Vieira", "Bergkamp", "Cantona", "Shearer", "Rooney", "Gerrard",
            "Lampard", "Scholes", "Giggs", "John Terry", "Ferdinand"
        };
                    break;

                case "🏠 Cosas de Casa":
                    palabrasPaquete = new List<string> {
            "Televisor", "Sofá", "Microondas", "Nevera", "Cama", "Lámpara", "Espejo", "Lavadora", "Silla", "Mesa", "Tenedor", "Escoba",
            "Licuadora", "Tostadora", "Horno", "Lavavajillas", "Congelador", "Batidora", "Cafetera", "Freidora de aire", "Plancha", "Aspiradora",
            "Secador", "Cepillo de dientes", "Jabón", "Toalla", "Sábana", "Almohada", "Manta", "Cortina", "Alfombra", "Cuadro", "Reloj", "Florero",
            "Estantería", "Escritorio", "Armario", "Cómoda", "Zapatero", "Perchero", "Puerta", "Ventana", "Balcón", "Grifo", "Ducha", "Inodoro",
            "Bidet", "Sartén", "Olla", "Cazo", "Plato", "Vaso", "Copa", "Cuchillo", "Cuchara", "Servilleta", "Mantel", "Escurreplatos", "Cubo de basura",
            "Recogedor", "Fregona", "Bayeta"
        };
                    break;

                case "🏅 Deportes":
                    palabrasPaquete = new List<string> {
            "Fútbol", "Baloncesto", "Tenis", "Natación", "Voleibol", "Ciclismo", "Boxeo", "Béisbol", "Golf", "Rugby", "Atletismo", "Judo",
            "Pádel", "Surf", "Esquí", "Snowboard", "Patinaje", "Hockey", "Waterpolo", "Balonmano", "Críquet", "Lacrosse", "Bádminton", "Tenis de mesa",
            "Squash", "Remo", "Piragüismo", "Vela", "Windsurf", "Kitesurf", "Motociclismo", "Fórmula 1", "Rally", "Ciclismo de montaña", "BMX",
            "Triatlón", "Pentatlón", "Esgrima", "Tiro con arco", "Tiro olímpico", "Gimnasia rítmica", "Gimnasia artística", "Halterofilia", "Crossfit",
            "Yoga", "Pilates", "Karate", "Taekwondo", "Muay Thai", "Lucha libre", "Sumo", "Escalada", "Senderismo", "Alpinismo", "Paracaidismo",
            "Puentismo", "Ajedrez", "Billar", "Dardos", "Bolos", "Petanca", "Polo"
        };
                    break;

                case "🎮 Videojuegos":
                    palabrasPaquete = new List<string> {
            "Minecraft", "Fortnite", "Super Mario", "GTA V", "Zelda", "Call of Duty", "FIFA", "Pokémon", "Tetris", "Pac-Man", "League of Legends", "Roblox",
            "Halo", "Gears of War", "God of War", "Uncharted", "The Last of Us", "Resident Evil", "Silent Hill", "Metal Gear Solid", "Final Fantasy",
            "Dragon Quest", "Kingdom Hearts", "Street Fighter", "Mortal Kombat", "Tekken", "SoulCalibur", "Super Smash Bros", "Mario Kart", "Sonic",
            "Crash Bandicoot", "Spyro", "Tomb Raider", "Assassin's Creed", "Far Cry", "Watch Dogs", "Splinter Cell", "Rainbow Six", "Ghost Recon",
            "Fallout", "Skyrim", "Oblivion", "Starfield", "Mass Effect", "Dragon Age", "The Witcher", "Cyberpunk 2077", "Elden Ring", "Dark Souls",
            "Bloodborne", "Sekiro", "Monster Hunter", "Devil May Cry", "Bayonetta", "Persona", "Yakuza", "Red Dead Redemption", "Bioshock",
            "Borderlands", "Destiny", "Overwatch", "Valorant"
        };
                    break;
            }

            // Enviamos todo al controlador
            _controller.CargarPaqueteAutomatico(accion, palabrasPaquete);

            // Actualizamos la interfaz: ocultamos lo manual, mostramos lo automático
            layoutManual.IsVisible = false;
            layoutAutomatico.IsVisible = true;
            lblPaqueteActivo.Text = $"Paquete activado:\n{accion}";
        }
    }

    private void OnVolverManualClicked(object sender, EventArgs e)
    {
        _controller.VolverAModoManual();
        layoutManual.IsVisible = true;
        layoutAutomatico.IsVisible = false;
        ActualizarBotonPalabras();
    }

    private async void OnEmpezarPartidaClicked(object sender, EventArgs e)
    {
        if (_controller.Jugadores.Count < 3)
        {
            await DisplayAlert("Faltan jugadores", "Necesitas al menos 3 jugadores en total.", "OK");
            return;
        }

        if (!_controller.EsModoAutomatico && _controller.Palabras.Count < 10)
        {
            await DisplayAlert("Faltan palabras", $"Necesitas al menos 10 palabras secretas. Llevas {_controller.Palabras.Count}.", "OK");
            return;
        }

        // 👇 AÑADE ESTA LÍNEA PARA VIAJAR A LA NUEVA PANTALLA 👇
        await Navigation.PushAsync(new ImpostorRolePage(
            _controller.Jugadores.ToList(),
            _controller.Palabras.ToList(),
            _controller.NumeroImpostores
        ));
    }
}