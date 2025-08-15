using BitChat.Maui.Presentation.ViewModels;
using BitChat.Maui.Presentation.Views;

namespace BitChat.Maui;

public partial class MainPage : ContentPage
{
	private readonly ChatViewModel _viewModel;

	public MainPage(ChatViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		BindingContext = _viewModel;

		// Subscribe to scroll to latest event
		_viewModel.ScrollToLatestRequested += OnScrollToLatestRequested;
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

	private void OnScrollToLatestRequested(object? sender, EventArgs e)
	{
		// Scroll to the last item in the collection view
		if (_viewModel.Messages.Count > 0)
		{
			MessagesCollectionView.ScrollTo(_viewModel.Messages.Count - 1, animate: true);
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		// Focus the message entry when the page appears
		MessageEntry?.Focus();
		
		// Scroll to latest message
		if (_viewModel.Messages.Count > 0)
		{
			MessagesCollectionView.ScrollTo(_viewModel.Messages.Count - 1, animate: false);
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		// Clean up if needed
	}
}

