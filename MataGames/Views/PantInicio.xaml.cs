using MataGames.Controllers;

namespace MataGames.Views;

public partial class PantInicio : ContentPage
{
    InicioController controller = new InicioController();

    public PantInicio()
    {
        InitializeComponent();
    }

    private async void OnEmpezarClicked(object sender, EventArgs e)
    {
        string nombre = await DisplayPromptAsync(
            "Jugador",
            "Introduce tu nombre:"
        );

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            // Usamos tu controlador para crear el modelo
            var jugador = controller.CrearJugador(nombre);

            // Navegamos a la nueva página pasando el jugador
            await Navigation.PushAsync(new NombreJugadorPopup(jugador));
        }
    }
}