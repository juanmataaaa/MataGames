using MataGames.Controllers;
using MataGames.Models;
using Microsoft.Maui.Controls;

namespace MataGames.Views;

public partial class ImpostorSetupPage : ContentPage
{
    private ImpostorSetupController _controller;

    // Comando para controlar la flecha de atrás original (por si acaso)
    public Command BackCommand { get; }

    public ImpostorSetupPage(Jugador creador)
    {
        InitializeComponent();

        // --- BLOQUEO TOTAL DE LA BARRA BLANCA ---
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });

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

    // Bloquea el botón físico de atrás en Android para mostrar el aviso
    protected override bool OnBackButtonPressed()
    {
        ConfirmarSalida();
        return true;
    }

    // Acción para el botón circular personalizado que creamos en el XAML
    private void OnBackClicked(object sender, EventArgs e)
    {
        ConfirmarSalida();
    }

    private async void ConfirmarSalida()
    {
        bool salir = await DisplayAlert("¿Volver atrás?", "Si sales ahora perderás todos los jugadores y palabras añadidas. ¿Estás seguro?", "Sí, salir", "Cancelar");

        if (salir)
        {
            await Navigation.PopAsync();
        }
    }

    // --- LÓGICA DE JUGADORES ---

    private async void OnAddJugadorClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtNuevoJugador.Text))
        {
            if (_controller.AgregarJugador(txtNuevoJugador.Text))
            {
                txtNuevoJugador.Text = string.Empty;
            }
            else
            {
                await DisplayAlert("Nombre Duplicado", "Ya hay un jugador con este nombre. Por favor, introduce uno diferente.", "OK");
                txtNuevoJugador.Focus();
            }
        }
    }

    private async void OnJugadorSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string jugadorSeleccionado)
        {
            listaJugadores.SelectedItem = null;

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

    // --- LÓGICA DE PALABRAS ---

    private async void OnAddPalabraClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtNuevaPalabra.Text))
        {
            if (_controller.AgregarPalabra(txtNuevaPalabra.Text))
            {
                txtNuevaPalabra.Text = string.Empty;
                ActualizarBotonPalabras();
            }
            else
            {
                await DisplayAlert("Palabra Duplicada", "Esta palabra ya ha sido añadida. Introduce otra.", "OK");
                txtNuevaPalabra.Focus();
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

    // --- LÓGICA DE IMPOSTORES ---

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

    // --- LÓGICA DE PAQUETES TEMÁTICOS ---

    private async void OnElegirPaqueteClicked(object sender, EventArgs e)
    {
        string[] paquetes = { "⚽ Futbolistas", "🏠 Cosas de Casa", "🏅 Deportes", "🎮 Videojuegos" };
        string accion = await DisplayActionSheet("Elige un Paquete Temático", "Cancelar", null, paquetes);

        if (accion != "Cancelar" && accion != null)
        {
            List<string> palabrasPaquete = new List<string>();

            switch (accion)
            {
                case "⚽ Futbolistas":
                    palabrasPaquete = new List<string> {
        // Los Dioses del Fútbol (Históricos y Contemporáneos)
        "Messi", "Cristiano Ronaldo", "Maradona", "Pelé", "Cruyff",
        "Di Stéfano", "Zidane", "Ronaldo Nazário", "Ronaldinho",
        
        // Superestrellas Actuales (El Top de Hoy)
        "Mbappé", "Haaland", "Vinícius", "Bellingham", "Neymar",
        "De Bruyne", "Salah", "Lewandowski", "Lamine Yamal", "Rodri",

        // Leyendas Españolas y del Fútbol Moderno
        "Casillas", "Iniesta", "Xavi", "Puyol", "Sergio Ramos",
        "David Villa", "Fernando Torres", "Raúl",

        // Leyendas Históricas Ultra Famosas
        "Roberto Carlos", "Maldini", "Buffon", "Beckham", "Thierry Henry",
        "Gerrard", "Lampard", "Pirlo", "Kaká", "Rivaldo",

        // Cracks Actuales Muy Conocidos
        "Modric", "Kroos", "Griezmann", "Benzema", "Harry Kane",
        "Pedri", "Fede Valverde", "Phil Foden", "Musiala",

        // Defensas y Porteros (Mezcla Clásicos y Actuales)
        "Neuer", "Dibu Martínez", "Courtois", "Ter Stegen",
        "Virgil van Dijk", "Dani Alves", "Cafu", "Beckenbauer"
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
                        "Puenting", "Ajedrez", "Billar", "Dardos", "Bolos", "Petanca", "Polo"
                    };
                    break;
                case "🎮 Videojuegos":
                    palabrasPaquete = new List<string> {
        // Fenómenos Mundiales y Casuales
        "Minecraft", "Fortnite", "Roblox", "Los Sims", "Among Us",
        "Fall Guys", "Animal Crossing", "Tetris", "Pac-Man",

        // Nintendo y Plataformas (Para todos los públicos)
        "Super Mario Maker", "Mario Kart", "Super Smash Bros", "Zelda",
        "Pokémon", "Sonic", "Crash Bandicoot",

        // Shooters y Competitivos Top
        "Call of Duty", "Valorant", "Overwatch", "League of Legends",
        "FIFA", "EA FC", "Rocket League",

        // Aventura, Acción y Mundo Abierto (Los más famosos)
        "GTA V", "Red Dead Redemption", "The Last of Us", "Spider-Man",
        "God of War", "Uncharted", "Tomb Raider", "Assassin's Creed",
        "Cyberpunk 2077", "The Witcher", "Skyrim", "Fallout",

        // Terror y Pelea (Solo los más icónicos)
        "Resident Evil", "Five Nights at Freddy's",
        "Street Fighter", "Mortal Kombat", "Tekken"
    };
                    
                    break;
            }

            _controller.CargarPaqueteAutomatico(accion, palabrasPaquete);

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

    // --- INICIAR JUEGO ---

    private async void OnEmpezarPartidaClicked(object sender, EventArgs e)
    {
        if (_controller.Jugadores.Count < 3)
        {
            await DisplayAlert("Faltan jugadores", "Necesitas al menos 3 jugadores en total.", "OK");
            return;
        }

        if (!_controller.EsModoAutomatico && _controller.Palabras.Count < 5)
        {
            await DisplayAlert("Faltan palabras", $"Necesitas al menos 5 palabras secretas para jugar.", "OK");
            return;
        }

        await Navigation.PushAsync(new ImpostorRolePage(
            _controller.Jugadores.ToList(),
            _controller.Palabras.ToList(),
            _controller.NumeroImpostores
        ));
    }
}