using MataGames.Controllers;
using Microsoft.Maui.Controls.Shapes;

namespace MataGames.Views;

public partial class ConnectFourPage : ContentPage
{
    private ConnectFourController _controller;
    private Border[,] _celdas = new Border[6, 7]; // Para referenciar los agujeros visualmente

    public ConnectFourPage()
    {
        InitializeComponent();
        _controller = new ConnectFourController();
        CrearSoporteReal();
    }

    // Genera la malla de agujeros del "soporte" de plástico
    private void CrearSoporteReal()
    {
        gridTablero.Children.Clear();
        for (int fila = 0; fila < 6; fila++)
        {
            for (int col = 0; col < 7; col++)
            {
                // Un Border circular gris oscuro para simular el agujero vacío
                var agujero = new Border
                {
                    BackgroundColor = Color.FromArgb("#12121A"), // Fondo oscuro
                    Stroke = Color.FromArgb("#2C2C3E"), // Borde gris
                    StrokeThickness = 1,
                    StrokeShape = new Ellipse(), // Forma circular
                    Padding = 2,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 45,
                    HeightRequest = 45
                };

                // Le asignamos un gesto de toque para toda la columna
                var tapGesture = new TapGestureRecognizer { CommandParameter = col };
                tapGesture.Tapped += OnColumnaClicked;
                agujero.GestureRecognizers.Add(tapGesture);

                _celdas[fila, col] = agujero;
                gridTablero.Add(agujero, col, fila);
            }
        }
    }

    private async void OnColumnaClicked(object sender, EventArgs e)
    {
        if (!_controller.EsTurnoJugador || _controller.JuegoTerminado) return;

        int columna = (int)((TapGestureRecognizer)((Border)sender).GestureRecognizers[0]).CommandParameter;

        // Intentamos soltar ficha del jugador (1)
        int filaDondeCayo = _controller.DropFicha(columna, 1);
        if (filaDondeCayo != -1)
        {
            await ColocarFichaVisual(filaDondeCayo, columna, Color.FromArgb("#00C853")); // Verde Neón

            if (VerificarFin(filaDondeCayo, columna)) return;

            // Turno del Bot
            _controller.EsTurnoJugador = false;
            lblEstado.Text = "Bot pensando...";
            await Task.Delay(500);

            int movBot = _controller.ObtenerMovimientoBot();
            if (movBot != -1)
            {
                int filaBot = _controller.DropFicha(movBot, 2);
                if (filaBot != -1)
                {
                    await ColocarFichaVisual(filaBot, movBot, Color.FromArgb("#FF3B30")); // Rojo Neón
                    if (VerificarFin(filaBot, movBot)) return;
                }
            }
            if (!_controller.JuegoTerminado)
            {
                _controller.EsTurnoJugador = true;
                lblEstado.Text = "Tu turno (Ficha Verde)";
            }
        }
    }

    private async Task ColocarFichaVisual(int f, int c, Color colorNeon)
    {
        // Creamos la ficha "flotando" arriba del soporte
        var ficha = new Ellipse
        {
            Fill = colorNeon,
            WidthRequest = 35,
            HeightRequest = 35,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -(f + 1) * 55 // Posición inicial arriba del soporte
        };

        // Metemos la ficha visualmente en el mismo Grid del soporte
        gridTablero.Add(ficha, c, f);

        // Animación de caída (gravedad)
        await ficha.TranslateTo(0, 0, 300, Easing.BounceOut);
    }

    private bool VerificarFin(int f, int c)
    {
        string res = _controller.VerificarEstadoJuego(f, c);
        if (res == null) return false;

        if (res == "Empate") lblEstado.Text = "¡Empate! 🤝";
        else lblEstado.Text = res == "X" ? "¡Ganaste! 🎉" : "Perdiste... 🤖";
        return true;
    }

    private void OnReiniciarClicked(object sender, EventArgs e)
    {
        _controller.ReiniciarJuego();
        lblEstado.Text = "Toca una columna para soltar ficha (Verde)";
        CrearSoporteReal(); // Regeneramos para vaciar las fichas animadas
    }

    // Gestión de Dificultad
    private void OnDificultadClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        btnFacil.BackgroundColor = btnNormal.BackgroundColor = btnDificil.BackgroundColor = btnImposible.BackgroundColor = Color.FromArgb("#2C2C3E");
        btn.BackgroundColor = Color.FromArgb("#00C853");

        if (btn == btnFacil) _controller.NivelDificultad = Dificultad.Facil;
        else if (btn == btnNormal) _controller.NivelDificultad = Dificultad.Normal;
        else if (btn == btnDificil) _controller.NivelDificultad = Dificultad.Dificil;
        else if (btn == btnImposible) _controller.NivelDificultad = Dificultad.Imposible;

        OnReiniciarClicked(null, null);
    }

    // Botones de navegación de cabecera
    private async void OnBackClicked(object sender, EventArgs e) { await Navigation.PopAsync(); }
    private async void OnSwitchToTicTacToeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TicTacToePage());
        Navigation.RemovePage(this); // Limpieza
    }
}