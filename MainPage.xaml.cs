using BitChat.Maui.Presentation.Views;

namespace BitChat.Maui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnSettingsClicked(object sender, EventArgs e)
	{
		try
		{
			var settingsPage = ServiceHelper.GetService<SettingsPage>();
			await Navigation.PushAsync(settingsPage);
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Could not open settings: {ex.Message}", "OK");
		}
	}
}

