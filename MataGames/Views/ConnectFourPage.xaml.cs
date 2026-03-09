using MataGames.Controllers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices; // VIBRACIÓN

namespace MataGames.Views;

public partial class ConnectFourPage : ContentPage
{
    private ConnectFourController _controller;
    private Border[,] _celdas = new Border[6, 7];

    public ConnectFourPage()
    {
        InitializeComponent();
        _controller = new ConnectFourController();
        CrearSoporteReal();
    }

    private void CrearSoporteReal()
    {
        gridTablero.Children.Clear();
        for (int fila = 0; fila < 6; fila++)
        {
            for (int col = 0; col < 7; col++)
            {
                var agujero = new Border
                {
                    BackgroundColor = Color.FromArgb("#12121A"),
                    Stroke = Color.FromArgb("#2C2C3E"),
                    StrokeThickness = 1,
                    StrokeShape = new Ellipse(),
                    Padding = 2,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 36,
                    HeightRequest = 36
                };

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

        int filaDondeCayo = _controller.DropFicha(columna, 1);
        if (filaDondeCayo != -1)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }

            await ColocarFichaVisual(filaDondeCayo, columna, Color.FromArgb("#00C853"));

            if (VerificarFin(filaDondeCayo, columna)) return;

            _controller.EsTurnoJugador = false;
            lblEstado.Text = "Bot pensando...";
            await Task.Delay(500);

            int movBot = _controller.ObtenerMovimientoBot();
            if (movBot != -1)
            {
                int filaBot = _controller.DropFicha(movBot, 2);
                if (filaBot != -1)
                {
                    try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }

                    await ColocarFichaVisual(filaBot, movBot, Color.FromArgb("#FF3B30"));
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
        var ficha = new Ellipse
        {
            Fill = colorNeon,
            WidthRequest = 30,
            HeightRequest = 30,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -(f + 1) * 46
        };

        gridTablero.Add(ficha, c, f);
        await ficha.TranslateTo(0, 0, 300, Easing.BounceOut);
    }

    private bool VerificarFin(int f, int c)
    {
        string res = _controller.VerificarEstadoJuego(f, c);
        if (res == null) return false;

        if (res == "Empate")
        {
            lblEstado.Text = "¡Empate! 🤝";
        }
        else
        {
            lblEstado.Text = res == "X" ? "¡Ganaste! 🎉" : "Perdiste... 💀";
            DibujarLineaGanadora(); // Llamamos a la magia visual
        }
        return true;
    }

    // NUEVA FUNCIÓN: Dibuja la línea sobre las fichas
    private void DibujarLineaGanadora()
    {
        var fichas = _controller.FichasGanadoras;
        if (fichas == null || fichas.Count < 4) return;

        var inicio = fichas.First();
        var fin = fichas.Last();

        // Matemáticas para encontrar el centro exacto de la ficha inicial y final.
        // Fórmula: Columna/Fila * (AnchoFicha + Espaciado) + (AnchoFicha / 2)
        double startX = (inicio[1] * 46) + 19;
        double startY = (inicio[0] * 46) + 19;
        double endX = (fin[1] * 46) + 19;
        double endY = (fin[0] * 46) + 19;

        var linea = new Line
        {
            X1 = startX,
            Y1 = startY,
            X2 = endX,
            Y2 = endY,
            Stroke = Color.FromArgb("#FFFFFF"), // Línea blanca
            StrokeThickness = 6, // Grosor de la línea
            StrokeLineCap = PenLineCap.Round, // Bordes redondeados
            Opacity = 0 // Empieza invisible para animarse
        };

        // Le decimos que ocupe todo el tablero para que no se corte
        gridTablero.Add(linea, 0, 0);
        Grid.SetColumnSpan(linea, 7);
        Grid.SetRowSpan(linea, 6);

        // Animamos su aparición
        linea.FadeTo(1, 400, Easing.CubicOut);
    }

    private void OnReiniciarClicked(object sender, EventArgs e)
    {
        _controller.ReiniciarJuego();
        lblEstado.Text = "Toca una columna para soltar ficha (Verde)";
        CrearSoporteReal();
    }

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

    private async void OnBackClicked(object sender, EventArgs e) { await Navigation.PopAsync(); }
    private async void OnSwitchToTicTacToeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TicTacToePage());
        Navigation.RemovePage(this);
    }
}