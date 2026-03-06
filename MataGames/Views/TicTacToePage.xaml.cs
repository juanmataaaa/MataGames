using MataGames.Controllers;

namespace MataGames.Views;

public partial class TicTacToePage : ContentPage
{
    private TicTacToeController _controller;
    private Button[] _botones = new Button[9];

    public TicTacToePage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);

        _controller = new TicTacToeController();
        CrearTablero();
    }

    private void CrearTablero()
    {
        gridTablero.Children.Clear();
        for (int i = 0; i < 9; i++)
        {
            var btn = new Button
            {
                FontSize = 36,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.FromArgb("#2C2C3E"),
                TextColor = Colors.White,
                CornerRadius = 10,
                CommandParameter = i
            };
            btn.Clicked += OnCasillaClicked;
            _botones[i] = btn;
            gridTablero.Add(btn, i % 3, i / 3);
        }
    }

    private async void OnCasillaClicked(object sender, EventArgs e)
    {
        if (!_controller.EsTurnoJugador || _controller.JuegoTerminado) return;

        var btn = (Button)sender;
        int indice = (int)btn.CommandParameter;

        if (_controller.HacerMovimiento(indice, "X"))
        {
            // Vibración suave al tocar
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            btn.Text = "X";
            btn.TextColor = Color.FromArgb("#00C853");
            await btn.ScaleTo(1.2, 100); await btn.ScaleTo(1.0, 100); // Animación rápida

            if (VerificarFin()) return;

            _controller.EsTurnoJugador = false;
            lblEstado.Text = "Bot pensando...";
            await Task.Delay(500);

            int movBot = _controller.ObtenerMovimientoBot();
            if (movBot != -1)
            {
                _controller.HacerMovimiento(movBot, "O");
                _botones[movBot].Text = "O";
                _botones[movBot].TextColor = Color.FromArgb("#FF3B30");
                await _botones[movBot].ScaleTo(1.2, 100); await _botones[movBot].ScaleTo(1.0, 100);
                VerificarFin();
            }
            if (!_controller.JuegoTerminado)
            {
                _controller.EsTurnoJugador = true;
                lblEstado.Text = "Tu turno (X)";
            }
        }
    }

    private bool VerificarFin()
    {
        string res = _controller.VerificarEstadoJuego();
        if (res == null) return false;

        if (res == "Empate")
        {
            lblEstado.Text = "¡Empate! 🤝";
        }
        else
        {
            lblEstado.Text = res == "X" ? "¡Ganaste! 🎉" : "Perdiste... 🤖";

            // SI EL BOT GANA EN IMPOSIBLE: Vibración larga de "escarnio"
            if (res == "O" && _controller.NivelDificultad == Dificultad.Imposible)
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            }
        }
        return true;
    }

    private void OnReiniciarClicked(object sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        _controller.ReiniciarJuego();
        lblEstado.Text = "Tu turno (X)";
        foreach (var b in _botones) b.Text = "";
    }

    private async void OnBackClicked(object sender, EventArgs e) { await Navigation.PopAsync(); }
    // Dentro de tu clase TicTacToePage, añade esta función al final:

    private async void OnSwitchToConnectFourClicked(object sender, EventArgs e)
    {
        // Hacemos un swap de la página actual por la nueva, para que al volver atrás 
        // desde el 4 en raya, vuelvas al menú principal, no a este 3 en raya.
        await Navigation.PushAsync(new ConnectFourPage());
        Navigation.RemovePage(this); // Opcional: quita esta página de la pila para limpieza
    }
    private void OnDificultadClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        btnFacil.BackgroundColor = btnNormal.BackgroundColor = btnDificil.BackgroundColor = btnImposible.BackgroundColor = Color.FromArgb("#2C2C3E");
        btn.BackgroundColor = Color.FromArgb("#00C853");

        if (btn == btnFacil) _controller.NivelDificultad = Dificultad.Facil;
        else if (btn == btnNormal) _controller.NivelDificultad = Dificultad.Normal;
        else if (btn == btnDificil) _controller.NivelDificultad = Dificultad.Dificil;
        else _controller.NivelDificultad = Dificultad.Imposible;

        OnReiniciarClicked(null, null);
    }

    private async void OnCrearSalaClicked(object sender, EventArgs e) { await DisplayAlert("Modo Online", "Próximamente...", "OK"); }
    private async void OnBuscarSalaClicked(object sender, EventArgs e) { await DisplayAlert("Modo Online", "Próximamente...", "OK"); }
}