using MataGames.Models;
using MataGames.Controllers;

namespace MataGames.Views;

public partial class NombreJugadorPopup : ContentPage
{
    // Declaramos el controlador y el jugador
    private NombreJugadorController _controller;
    private Jugador _jugador;

    public NombreJugadorPopup(Jugador jugador)
    {
        InitializeComponent();

        // 👇 ¡AQUÍ ESTÁ LA MAGIA QUE FALTABA! Guardamos el jugador que recibimos.
        _jugador = jugador;

        // 1. Inicializamos el controlador pasándole el Modelo (Jugador)
        _controller = new NombreJugadorController(jugador);

        // 2. La vista solo "pinta" lo que el controlador le dice
        lblBienvenida.Text = _controller.GenerarMensajeBienvenida();
    }

    // Bloquea el botón físico de atrás en Android
    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private async void OnImpostorClicked(object sender, EventArgs e)
    {
        // Ahora _jugador sí tiene tu nombre real, ¡se acabó el bug!
        await Navigation.PushAsync(new ImpostorSetupPage(_jugador));
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        // El controlador decide el texto, la vista solo lanza la alerta visual
        string mensaje = _controller.ObtenerAvisoConfiguracion();
        await DisplayAlert("Configuración", mensaje, "OK");
    }
}