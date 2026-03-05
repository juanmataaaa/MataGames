using MataGames.Models;
using MataGames.Controllers; // Añadimos la referencia a los controladores

namespace MataGames.Views;

public partial class NombreJugadorPopup : ContentPage
{
    // Declaramos el controlador de esta vista
    private NombreJugadorController _controller;

    public NombreJugadorPopup(Jugador jugador)
    {
        InitializeComponent();

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

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        // El controlador decide el texto, la vista solo lanza la alerta visual
        string mensaje = _controller.ObtenerAvisoConfiguracion();
        await DisplayAlert("Configuración", mensaje, "OK");
    }
}