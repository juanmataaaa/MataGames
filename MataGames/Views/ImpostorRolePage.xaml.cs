using MataGames.Controllers;
using Microsoft.Maui.Graphics;

namespace MataGames.Views;

public partial class ImpostorRolePage : ContentPage
{
    private ImpostorGameController _controller;
    private bool _tarjetaVolteada = false;

    public ImpostorRolePage(List<string> jugadores, List<string> palabras, int numImpostores)
    {
        InitializeComponent();

        // Iniciamos el controlador (MVC)
        _controller = new ImpostorGameController(jugadores, palabras, numImpostores);

        MostrarTurnoActual();
    }

    // Bloquea el botón de atrás físico para evitar que alguien vea el rol anterior
    protected override bool OnBackButtonPressed() => true;

    private void MostrarTurnoActual()
    {
        _tarjetaVolteada = false;

        // Ponemos el nombre del jugador (con su corona si la tiene)
        lblNombreJugador.Text = _controller.ObtenerJugadorActual();

        // Reiniciamos el estado visual de la tarjeta (Frontal visible)
        cardFront.RotationY = 0;
        cardFront.IsVisible = true;

        cardBack.RotationY = 180;
        cardBack.IsVisible = false;

        // Ocultamos el botón de finalizar hasta que se vea el rol
        btnFinalizarTurno.IsVisible = false;
    }

    // --- LÓGICA DE ANIMACIÓN FLIP CON AJUSTE DE TEXTO DINÁMICO ---
    private async void OnVerRolClicked(object sender, EventArgs e)
    {
        if (_tarjetaVolteada) return;
        _tarjetaVolteada = true;

        // 1. Configuramos la cara trasera antes de mostrarla
        if (_controller.EsImpostorActual())
        {
            lblTituloRol.Text = "¡CUIDADO!";
            lblRol.Text = "ERES EL\nIMPOSTOR";

            // Estilo Rojo (Impostor)
            lblRol.Style = (Style)Application.Current.Resources["ImpostorWordStyle"];
            cardBack.Stroke = Color.FromArgb("#FF3B30");
            cardBack.StrokeThickness = 4;
        }
        else
        {
            lblTituloRol.Text = "La palabra secreta es:";

            string palabra = _controller.ObtenerRolActual().ToUpper();
            lblRol.Text = palabra;

            // Estilo Verde (Inocente)
            lblRol.Style = (Style)Application.Current.Resources["InnocentWordStyle"];
            cardBack.Stroke = Color.FromArgb("#00C853");
            cardBack.StrokeThickness = 4;

            // Ajuste dinámico de tamaño para que palabras largas no se corten
            if (palabra.Length > 10)
                lblRol.FontSize = 24;
            else if (palabra.Length > 7)
                lblRol.FontSize = 32;
            else
                lblRol.FontSize = 42;
        }

        // 2. Ejecutamos la animación de volteo (Flip)
        // Giramos la parte delantera hasta 90 grados
        await cardFront.RotateYTo(90, 250, Easing.Linear);
        cardFront.IsVisible = false;

        // Aparece la trasera y termina el giro de 90 a 0
        cardBack.IsVisible = true;
        cardBack.RotationY = 90;
        await cardBack.RotateYTo(0, 250, Easing.Linear);

        // 3. Mostramos el botón para avanzar al siguiente jugador
        btnFinalizarTurno.IsVisible = true;
    }

    private async void OnSiguienteClicked(object sender, EventArgs e)
    {
        // Si quedan jugadores por ver su rol, reiniciamos la tarjeta
        if (_controller.AvanzarTurno())
        {
            MostrarTurnoActual();
        }
        else
        {
            // Si todos han visto su rol, pasamos a la fase de Votación
            await DisplayAlert("¡Listos!", "Todos conocen su rol. ¡Que empiece el debate!", "A jugar");

            // Pasamos el controlador (que ya tiene la lista de jugadores) a la nueva página
            await Navigation.PushAsync(new ImpostorVotingPage(_controller, _controller.Jugadores));
        }
    }
}