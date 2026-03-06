using MataGames.Controllers;
using MataGames.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace MataGames.Views;

public partial class TicTacToePage : ContentPage
{
    private TicTacToeController _controller;
    private TicTacToeService _onlineService;
    private Button[] _botones = new Button[9];
    private string _salaId = "";
    private bool _esOnline = false;
    private string _miFicha = "X";
    private string _fichaRival = "O";

    public TicTacToePage()
    {
        InitializeComponent();
        _controller = new TicTacToeController();
        _onlineService = new TicTacToeService();
        ConfigurarEventosOnline();
        CrearTablero();
        ActualizarInterfazOnline(false);
    }

    private void ConfigurarEventosOnline()
    {
        _onlineService.OnMoveReceived += (indice, ficha) => RecibirJugadaRival(indice, ficha);

        _onlineService.HubConnection.On<string>("JugadorConectado", (nombre) => {
            MainThread.BeginInvokeOnMainThread(async () => {
                await DisplayAlert("¡Rival!", $"{nombre} ha entrado.", "A JUGAR");
                OnReiniciarClicked(null, null); // LIMPIAR TABLERO AL CONECTAR
                _miFicha = "X"; _fichaRival = "O";
                _controller.EsTurnoJugador = true;
                lblEstado.Text = "TU TURNO (X)";
                ActualizarInterfazOnline(true);
            });
        });

        _onlineService.HubConnection.On<string, string>("RecibirPeticion", (nombre, id) => {
            MainThread.BeginInvokeOnMainThread(async () => {
                bool ok = await DisplayAlert("Reto", $"{nombre} quiere jugar.", "Sí", "No");
                await _onlineService.HubConnection.InvokeAsync("ResponderInvitacion", _salaId, ok, id);
                if (ok)
                {
                    OnReiniciarClicked(null, null); // LIMPIAR TABLERO
                    _miFicha = "X"; _fichaRival = "O";
                    _controller.EsTurnoJugador = true;
                    lblEstado.Text = "TU TURNO (X)";
                    ActualizarInterfazOnline(true);
                }
            });
        });

        _onlineService.HubConnection.On("EmpezarPartida", () => {
            MainThread.BeginInvokeOnMainThread(() => {
                OnReiniciarClicked(null, null); // LIMPIAR TABLERO
                _miFicha = "O"; _fichaRival = "X";
                _controller.EsTurnoJugador = false;
                lblEstado.Text = "ESPERANDO RIVAL (X)...";
                ActualizarInterfazOnline(true);
            });
        });

        _onlineService.HubConnection.On("JugadorDesconectado", () => {
            MainThread.BeginInvokeOnMainThread(async () => {
                await DisplayAlert("Aviso", "Rival desconectado.", "OK");
                FinalizarModoOnline();
            });
        });
    }

    private void CrearTablero()
    {
        gridTablero.Children.Clear();
        for (int i = 0; i < 9; i++)
        {
            var btn = new Button { FontSize = 32, BackgroundColor = Color.FromArgb("#2C2C3E"), CommandParameter = i, CornerRadius = 10, FontAttributes = FontAttributes.Bold };
            btn.Clicked += OnCasillaClicked;
            _botones[i] = btn;
            gridTablero.Add(btn, i % 3, i / 3);
        }
    }

    private async void OnCasillaClicked(object sender, EventArgs e)
    {
        if (!_controller.EsTurnoJugador || _controller.JuegoTerminado) return;
        var btn = (Button)sender;
        int idx = (int)btn.CommandParameter;

        if (_controller.HacerMovimiento(idx, _miFicha))
        {
            btn.Text = _miFicha;
            btn.TextColor = _miFicha == "X" ? Color.FromArgb("#00C853") : Color.FromArgb("#FF3B30");
            if (await VerificarYAnimarFin()) return;

            if (_esOnline)
            {
                _controller.EsTurnoJugador = false;
                lblEstado.Text = "TURNO DEL RIVAL...";
                await _onlineService.EnviarMovimiento(_salaId, idx, _miFicha);
            }
            else { EjecutarTurnoBot(); } // EL BOT SOLO JUEGA SI NO ES ONLINE
        }
    }

    private async void EjecutarTurnoBot()
    {
        if (_esOnline) return; // DOBLE SEGURO CONTRA EL BOT
        _controller.EsTurnoJugador = false;
        lblEstado.Text = "PENSANDO...";
        await Task.Delay(600);
        int mov = _controller.ObtenerMovimientoBot();
        if (mov != -1)
        {
            _controller.HacerMovimiento(mov, "O");
            _botones[mov].Text = "O";
            _botones[mov].TextColor = Color.FromArgb("#FF3B30");
            await VerificarYAnimarFin();
        }
        if (!_controller.JuegoTerminado) { _controller.EsTurnoJugador = true; lblEstado.Text = "TU TURNO"; }
    }

    private void RecibirJugadaRival(int idx, string ficha)
    {
        MainThread.BeginInvokeOnMainThread(async () => {
            _controller.HacerMovimiento(idx, ficha);
            _botones[idx].Text = ficha;
            _botones[idx].TextColor = ficha == "X" ? Color.FromArgb("#00C853") : Color.FromArgb("#FF3B30");
            if (!await VerificarYAnimarFin()) { _controller.EsTurnoJugador = true; lblEstado.Text = "TU TURNO"; }
        });
    }

    private async Task<bool> VerificarYAnimarFin()
    {
        string res = _controller.VerificarEstadoJuego();
        if (res == null) return false;

        gridTablero.IsEnabled = false;
        if (res != "Empate" && _controller.IndicesGanadores != null)
        {
            foreach (int i in _controller.IndicesGanadores)
            {
                _botones[i].BackgroundColor = Color.FromArgb("#FFD700"); // DORADO VICTORIA
                _ = _botones[i].ScaleTo(1.2, 300);
            }
        }
        lblEstado.Text = res == "Empate" ? "¡EMPATE! 🤝" : (res == _miFicha ? "¡GANASTE! 🎉" : "PERDISTE 💀");
        return true;
    }

    private void ActualizarInterfazOnline(bool onlineActivo)
    {
        gridOnlineControls.IsVisible = !onlineActivo;
        layoutDificultad.IsVisible = !onlineActivo;
        btnSalirOnline.IsVisible = onlineActivo;
        btnReiniciar.IsEnabled = !onlineActivo;
    }

    private async void OnCrearSalaClicked(object sender, EventArgs e)
    {
        string nombre = await DisplayPromptAsync("Perfil", "¿Nombre?");
        if (string.IsNullOrEmpty(nombre)) return;
        _esOnline = true;
        _miFicha = "X";
        _salaId = "SALA" + new Random().Next(100, 999);
        lblEstado.Text = "CREANDO SALA...";
        await _onlineService.Conectar();
        await _onlineService.UnirseASala(_salaId, nombre);
        await DisplayAlert("SALA", $"CÓDIGO: {_salaId}\nEspera a tu rival...", "OK");
    }

    private async void OnBuscarSalaClicked(object sender, EventArgs e)
    {
        string cod = await DisplayPromptAsync("UNIRSE", "CÓDIGO:");
        if (string.IsNullOrEmpty(cod)) return;
        string nombre = await DisplayPromptAsync("Perfil", "¿Nombre?");
        _esOnline = true; _salaId = cod;
        _miFicha = "O";
        lblEstado.Text = "UNIÉNDOSE...";
        await _onlineService.Conectar();
        await _onlineService.UnirseASala(_salaId, nombre);
    }

    private void FinalizarModoOnline()
    {
        _esOnline = false;
        ActualizarInterfazOnline(false);
        OnReiniciarClicked(null, null);
    }

    public void OnReiniciarClicked(object sender, EventArgs e)
    {
        _controller.ReiniciarJuego();
        gridTablero.IsEnabled = true;
        foreach (var b in _botones) { b.Text = ""; b.BackgroundColor = Color.FromArgb("#2C2C3E"); b.Scale = 1; }
        if (!_esOnline) lblEstado.Text = "TU TURNO (X)";
    }

    private void OnSalirOnlineClicked(object sender, EventArgs e) => FinalizarModoOnline();
    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private void OnDificultadClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        btnFacil.BackgroundColor = btnNormal.BackgroundColor = btnDificil.BackgroundColor = btnImposible.BackgroundColor = Color.FromArgb("#1E1E2E");
        btn.BackgroundColor = Color.FromArgb("#00C853");
        if (btn.Text == "Fácil") _controller.NivelDificultad = Dificultad.Facil;
        else if (btn.Text == "Normal") _controller.NivelDificultad = Dificultad.Normal;
        else if (btn.Text == "Difícil") _controller.NivelDificultad = Dificultad.Dificil;
        else _controller.NivelDificultad = Dificultad.Imposible;
        OnReiniciarClicked(null, null);
    }
    private async void OnSwitchToConnectFourClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ConnectFourPage());
        Navigation.RemovePage(this);
    }
}