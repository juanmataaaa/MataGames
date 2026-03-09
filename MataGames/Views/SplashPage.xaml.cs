namespace MataGames.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        IrAPantallaPrincipal();
    }

    private async void IrAPantallaPrincipal()
    {
        // Espera 2.5 segundos para que se vea tu logo
        await Task.Delay(2500);

        // Cambia la raíz de la app a tu pantalla de inicio (pon el nombre de tu clase real)
        Application.Current.MainPage = new NavigationPage(new PantInicio());
    }
}