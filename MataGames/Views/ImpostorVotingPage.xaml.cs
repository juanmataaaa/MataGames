using MataGames.Controllers;
using System.Collections.ObjectModel;

namespace MataGames.Views;

public partial class ImpostorVotingPage : ContentPage
{
    private ImpostorGameController _controller;
    // Usamos esto para que la lista se refresque sola en la pantalla
    public ObservableCollection<string> Sospechosos { get; set; }

    public ImpostorVotingPage(ImpostorGameController controller, List<string> jugadores)
    {
        InitializeComponent();
        _controller = controller;

        Sospechosos = new ObservableCollection<string>(jugadores);
        listaVotacion.ItemsSource = Sospechosos;
    }

    protected override bool OnBackButtonPressed() => true;

    private async void OnVotoSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string sospechoso)
        {
            listaVotacion.SelectedItem = null;

            bool confirmar = await DisplayAlert("Confirmar Voto", $"¿Estáis seguros de que {sospechoso} es el impostor?", "SÍ", "NO");

            if (confirmar)
            {
                bool esImpostor = _controller.VerificarSiEsImpostor(sospechoso);
                _controller.EliminarJugador(sospechoso);
                Sospechosos.Remove(sospechoso); // Se quita de la lista al momento

                if (esImpostor)
                {
                    if (_controller.ImpostoresRestantes > 0)
                    {
                        // Si quedan impostores y hay gente, seguimos en esta pantalla
                        await DisplayAlert("🎉 ¡ACIERTO!", $"¡Era un impostor! Pero aún quedan {_controller.ImpostoresRestantes}. ¿Quién será el siguiente?", "Seguir Votando");
                    }
                    else
                    {
                        await DisplayAlert("🏆 VICTORIA", "¡Habéis atrapado a todos los impostores!", "Volver");
                        await TerminarPartida();
                    }
                }
                else
                {
                    // DERROTA: Si los impostores igualan o superan a los inocentes, o quedan 2 o menos en total
                    if (_controller.InocentesRestantes <= _controller.ImpostoresRestantes || _controller.Jugadores.Count <= 2)
                    {
                        await DisplayAlert("💀 DERROTA", $"¡{sospechoso} era INOCENTE! Los impostores han ganado por mayoría.", "Cerrar");
                        await TerminarPartida();
                    }
                    else
                    {
                        await DisplayAlert("❌ FALLO", $"¡{sospechoso} era INOCENTE! El juego sigue, votad a otro.", "Continuar");
                        // Al no hacer nada, te quedas en esta pantalla con la lista actualizada
                    }
                }
            }
        }
    }

    private async Task TerminarPartida()
    {
        // 1. Buscamos la pantalla de cartas (Roles) en la memoria y la borramos
        var stack = Navigation.NavigationStack;
        var rolePage = stack.FirstOrDefault(p => p is ImpostorRolePage);
        if (rolePage != null)
        {
            Navigation.RemovePage(rolePage);
        }

        // 2. Al hacer Pop, como hemos borrado la de cartas, caerás directamente en Setup
        await Navigation.PopAsync();
    }
}