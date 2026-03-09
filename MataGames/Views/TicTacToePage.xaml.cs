using MataGames.Controllers;
using MataGames.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.Storage;

namespace MataGames.Views;

public partial class TicTacToePage : ContentPage
{
    private TicTacToeController _controller;
    private TicTacToeService _onlineService;
    private Button[] _botones = new Button[9];
    private string _salaId = "";
    private bool _esOnline = false;
    private bool _soyAnfitrion = false;
    private bool _partidaEnCurso = false;
    private string _miFicha = "X";
    private string _miNombreReal = "Jugador";

    public TicTacToePage()
    {
        InitializeComponent();
        _controller = new TicTacToeController();
        _onlineService = new TicTacToeService();

        // Carga tu nombre de la app de forma segura
        _miNombreReal = Preferences.Get("NombreJugador", Preferences.Get("UserName", Preferences.Get("Nombre", "Jugador")));
        if (string.IsNullOrWhiteSpace(_miNombreReal)) _miNombreReal = "Jugador";

        ConfigurarEventosOnline();
        CrearTablero();
        ActualizarInterfazOnline(false);
    }

    private void ConfigurarEventosOnline()
    {
        _onlineService.OnMoveReceived += (idx, ficha) => RecibirJugadaRival(idx, ficha);

        // Anfitrión: Recibe al invitado
        _onlineService.HubConnection.On<string, string>("RecibirPeticion", (nombreInvitado, connectionId) => {
            MainThread.BeginInvokeOnMainThread(async () => {
                _controller.NombreRival = string.IsNullOrWhiteSpace(nombreInvitado) ? "Invitado" : nombreInvitado;
                bool acepta = await DisplayAlert("NUEVO RETO", $"{_controller.NombreRival} quiere jugar.", "ACEPTAR", "RECHAZAR");

                await _onlineService.HubConnection.InvokeAsync("ResponderInvitacion", _salaId, acepta, connectionId);
                if (!acepta)
                {
                    lblEstado.Text = "ESPERANDO RIVAL...";
                    _partidaEnCurso = false;
                }
            });
        });

        // INICIO DE PARTIDA PROFESIONAL (Separa X y O)
        _onlineService.HubConnection.On("EmpezarPartida", () => {
            MainThread.BeginInvokeOnMainThread(() => {
                _partidaEnCurso = true; // Abre candado

                if (_soyAnfitrion)
                {
                    _miFicha = "X";
                    _controller.EsTurnoJugador = true;
                    lblEstado.Text = "TU TURNO (X)";
                }
                else
                {
                    _miFicha = "O";
                    _controller.EsTurnoJugador = false;
                    if (string.IsNullOrEmpty(_controller.NombreRival) || _controller.NombreRival == "Rival")
                        _controller.NombreRival = "Anfitrión"; // Azure no manda el nombre, ponemos el rol
                    lblEstado.Text = $"TURNO DE {_controller.NombreRival.ToUpper()} (X)";
                }
                LimpiarTableroUI();
                ActualizarInterfazOnline(true);
            });
        });

        // Revancha blindada
        _onlineService.HubConnection.On("PeticionRevancha", async () => {
            bool ok = await MainThread.InvokeOnMainThreadAsync(async () =>
                await DisplayAlert("REVANCHA", $"{_controller.NombreRival} pide otra partida.", "SÍ", "NO")
            );

            if (ok)
            {
                try { await _onlineService.HubConnection.InvokeAsync("AceptarRevancha", _salaId); } catch { }
            }
            else
            {
                try { await _onlineService.HubConnection.InvokeAsync("AbandonarSala", _salaId); } catch { }
                MainThread.BeginInvokeOnMainThread(() => FinalizarModoOnline());
            }
        });

        _onlineService.HubConnection.On("ReiniciarTableroOnline", () => {
            MainThread.BeginInvokeOnMainThread(() => {
                LimpiarTableroUI();
                _partidaEnCurso = true;

                // ¡AQUÍ ESTABA EL BUG DE LA REVANCHA! 
                // Restauramos el turno correcto según quién sea el anfitrión
                _controller.EsTurnoJugador = _soyAnfitrion;

                // Restauramos el botón visualmente para que no se quede atascado
                btnRevancha.Text = "🔥 SOLICITAR REVANCHA";
                btnRevancha.IsEnabled = true;
                btnRevancha.IsVisible = false;

                lblEstado.Text = _controller.EsTurnoJugador ? $"TU TURNO ({_miFicha})" : $"TURNO DE {_controller.NombreRival.ToUpper()}";
            });
        });

        // Abandono perfecto
        _onlineService.HubConnection.On("JugadorDesconectado", () => {
            MainThread.BeginInvokeOnMainThread(async () => {
                await DisplayAlert("Partida Cancelada", "El rival ha abandonado la sala.", "OK");
                FinalizarModoOnline();
            });
        });
    }

    private void ActualizarInterfazOnline(bool online)
    {
        _esOnline = online;
        MainThread.BeginInvokeOnMainThread(() => {
            layoutDificultad.IsVisible = !online;
            gridOnlineControls.IsVisible = !online;
            btnReiniciar.IsVisible = !online;
            btnCambiarJuego.IsVisible = !online; // OCULTA EL 4 EN RAYA
            btnSalirOnline.IsVisible = online;
            if (!online) btnRevancha.IsVisible = false;
        });
    }

    private async void OnCrearSalaClicked(object sender, EventArgs e)
    {
        _salaId = "SALA" + new Random().Next(100, 999);
        _soyAnfitrion = true;
        _partidaEnCurso = false; // Candado cerrado
        ActualizarInterfazOnline(true);
        lblEstado.Text = "CONECTANDO...";

        try
        {
            await _onlineService.Conectar();
            await _onlineService.HubConnection.InvokeAsync("JoinRoom", _salaId, _miNombreReal);
            lblEstado.Text = "ESPERANDO RIVAL...";
            await DisplayAlert("SALA CREADA", $"CÓDIGO: {_salaId}\nPásalo para que entren.", "OK");
        }
        catch
        {
            await DisplayAlert("Error", "Fallo al conectar al servidor.", "OK");
            FinalizarModoOnline();
        }
    }

    private async void OnBuscarSalaClicked(object sender, EventArgs e)
    {
        string cod = await DisplayPromptAsync("UNIRSE", "Código de Sala:");
        if (string.IsNullOrEmpty(cod)) return;

        _salaId = cod;
        _soyAnfitrion = false;
        _partidaEnCurso = false; // Candado cerrado
        ActualizarInterfazOnline(true);
        lblEstado.Text = "ENVIANDO PETICIÓN...";

        try
        {
            await _onlineService.Conectar();
            await _onlineService.HubConnection.InvokeAsync("JoinRoom", _salaId, _miNombreReal);
        }
        catch
        {
            await DisplayAlert("Error", "Fallo al conectar al servidor.", "OK");
            FinalizarModoOnline();
        }
    }

    private async void OnCasillaClicked(object sender, EventArgs e)
    {
        if (!_partidaEnCurso && _esOnline) return; // Si no hay rival, bloqueado
        if (!_controller.EsTurnoJugador || _controller.JuegoTerminado) return;

        // BLOQUEO INMEDIATO: Evita clics dobles que rompen el juego
        _controller.EsTurnoJugador = false;

        var btn = (Button)sender;
        int idx = (int)btn.CommandParameter;

        if (_controller.HacerMovimiento(idx, _miFicha))
        {
            ActualizarBotonVisual(idx, _miFicha);

            if (_esOnline)
            {
                lblEstado.Text = $"TURNO DE {_controller.NombreRival.ToUpper()}";
                await _onlineService.EnviarMovimiento(_salaId, idx, _miFicha);
            }

            await Task.Delay(150);
            if (await VerificarYProcesarFin()) return;

            if (!_esOnline) { await TurnoBotMVC(); }
        }
        else
        {
            _controller.EsTurnoJugador = true; // Si falló el clic, devuelve turno
        }
    }

    private void RecibirJugadaRival(int idx, string ficha)
    {
        MainThread.BeginInvokeOnMainThread(async () => {
            _controller.HacerMovimiento(idx, ficha);
            ActualizarBotonVisual(idx, ficha);
            await Task.Delay(150);
            if (!await VerificarYProcesarFin())
            {
                _controller.EsTurnoJugador = true; // AHORA ES TU TURNO
                lblEstado.Text = $"TU TURNO ({_miFicha})";
            }
        });
    }

    private async Task<bool> VerificarYProcesarFin()
    {
        string res = _controller.VerificarEstadoJuego();
        if (res == null) return false;

        gridTablero.IsEnabled = false;
        _partidaEnCurso = false; // Fin de partida

        if (res != "Empate")
        {
            foreach (int i in _controller.IndicesGanadores) _botones[i].BackgroundColor = Color.FromArgb("#FFD700");
        }
        lblEstado.Text = res == "Empate" ? "¡EMPATE! 🤝" : (res == _miFicha ? "¡HAS GANADO! 🎉" : "HAS PERDIDO 💀");
        if (_esOnline) btnRevancha.IsVisible = true;
        return true;
    }

    private void ActualizarBotonVisual(int idx, string ficha)
    {
        _botones[idx].Text = ficha;
        _botones[idx].TextColor = ficha == "X" ? Color.FromArgb("#00C853") : Color.FromArgb("#FF3B30");
    }

    private void LimpiarTableroUI()
    {
        _controller.ReiniciarJuego();
        gridTablero.IsEnabled = true;
        foreach (var b in _botones) { b.Text = ""; b.BackgroundColor = Color.FromArgb("#2C2C3E"); b.Scale = 1; }
    }

    private async void OnSalirOnlineClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Abandonar", "¿Seguro que quieres salir de la partida online?", "SÍ", "NO");
        if (!confirmar) return;

        try { await _onlineService.HubConnection.InvokeAsync("AbandonarSala", _salaId); } catch { }
        FinalizarModoOnline();
    }

    private void FinalizarModoOnline()
    {
        _salaId = "";
        _soyAnfitrion = false;
        _partidaEnCurso = false;
        ActualizarInterfazOnline(false);
        LimpiarTableroUI();

        // Desbloquea el botón por si se quedó pillado en una revancha cancelada
        btnRevancha.Text = "🔥 SOLICITAR REVANCHA";
        btnRevancha.IsEnabled = true;

        lblEstado.Text = "TU TURNO (X)";
    }

    private async void OnRevanchaClicked(object sender, EventArgs e)
    {
        try
        {
            btnRevancha.Text = "SOLICITADO...";
            btnRevancha.IsEnabled = false;
            await _onlineService.HubConnection.InvokeAsync("SolicitarRevancha", _salaId);
        }
        catch
        {
            await DisplayAlert("Error", "Fallo al solicitar la revancha.", "OK");
            btnRevancha.Text = "🔥 SOLICITAR REVANCHA";
            btnRevancha.IsEnabled = true;
        }
    }

    private void OnReiniciarClicked(object sender, EventArgs e)
    {
        LimpiarTableroUI();
        if (!_esOnline)
        {
            _controller.EsTurnoJugador = true;
            lblEstado.Text = "TU TURNO (X)";
        }
    }

    private void OnBackClicked(object sender, EventArgs e) => Navigation.PopAsync();

    private void CrearTablero()
    {
        for (int i = 0; i < 9; i++)
        {
            var b = new Button { FontSize = 32, BackgroundColor = Color.FromArgb("#2C2C3E"), CommandParameter = i, CornerRadius = 10, FontAttributes = FontAttributes.Bold };
            b.Clicked += OnCasillaClicked; _botones[i] = b; gridTablero.Add(b, i % 3, i / 3);
        }
    }

    private void OnDificultadClicked(object sender, EventArgs e)
    {
        var b = (Button)sender;
        btnFacil.BackgroundColor = btnNormal.BackgroundColor = btnDificil.BackgroundColor = btnImposible.BackgroundColor = Color.FromArgb("#1E1E2E");
        b.BackgroundColor = Color.FromArgb("#00C853");

        string diffAjustada = b.Text.Replace("Fácil", "Facil").Replace("Difícil", "Dificil").Replace("💀", "Imposible");
        _controller.NivelDificultad = (Dificultad)Enum.Parse(typeof(Dificultad), diffAjustada);
        OnReiniciarClicked(null, null);
    }

    private async Task TurnoBotMVC()
    {
        _partidaEnCurso = true;
        _controller.EsTurnoJugador = false; lblEstado.Text = "BOT PENSANDO...";
        await Task.Delay(600);
        int m = _controller.ObtenerMovimientoBot();
        if (m != -1)
        {
            _controller.HacerMovimiento(m, "O"); ActualizarBotonVisual(m, "O");
            await Task.Delay(150); await VerificarYProcesarFin();
        }
        if (!_controller.JuegoTerminado) { _controller.EsTurnoJugador = true; lblEstado.Text = "TU TURNO (X)"; }
    }

    private async void OnSwitchToConnectFourClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ConnectFourPage()); Navigation.RemovePage(this);
    }
}