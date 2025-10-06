using System.Diagnostics;
using MauiLocalAuth.ViewModels;
using MauiLocalAuth.Controls;

namespace MauiLocalAuth.Dialogs;

public partial class LocalAuthDialog : ContentPage
{
    #region Private Properties
    private readonly LocalAuthDialogViewModel _viewModel; 
    #endregion

    public LocalAuthDialogViewModel ViewModel => _viewModel;

    #region Constructor
    public LocalAuthDialog(LocalAuthDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    } 
    #endregion

    #region OnAppearing override Method
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.InitializeAsync();

        if (Resources["AuthSelector"] is Selectors.AuthTemplateSelector selector)
        {
            try
            {
                var template = selector.SelectTemplate(item: _viewModel, container: this);
                // ¬ставл€ем выбранный шаблон в ContentView
                var view = template?.CreateContent() as View ?? throw new NullReferenceException("");
                AuthContentView.Content = view;

                // ѕодписываемс€ на событие завершени€ ввода узора, если выбран шаблон узора
                if (view is PatternLockView patternView)
                {
                    patternView.PatternCompleted += OnPatternCompleted;
                }
                // или на событие завершени€ ввода PIN-кода, если выбран шаблон PIN-кода
                else if (view is PinCodeLockView pinCodeView)
                {
                    pinCodeView.PinCodeCompleted += OnPinCompleted;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"MauiLocalAuth.Dialogs.LocalAuthDialog - OnAppearing: {e.Message}");
            }
        }
    } 
    #endregion

    #region OnPinCompleted Method
    /// <summary>
    /// ќбрабатывает завершение ввода PIN -кода пользователем.
    /// </summary>
    /// <remarks>Ётот метод обрабатывает предоставленный PIN -код асинхронно путем делегировани€ его св€занной
    /// модели представлени€ (ViewModel). ”бедитесь, что <paramref name = "pinCode"/> действителен и не пуст.</remarks>
    /// <param name="sender">»сточник событи€. Ётот параметр может быть <see langword="null"/>.</param>
    /// <param name="pinCode">PIN -код, введенный пользователем. Ќе может быть <see langword="null"/> или пустой.</param>
    private async void OnPinCompleted(object? sender, string pinCode)
    {
        await _viewModel.HandlePinInputAsync(pinCode);
    }
    #endregion

    #region OnPatternCompleted Method
    /// <summary>
    /// ќбрабатывает завершение ввода узора пользователем.
    /// </summary>
    /// <remarks>Ётот метод обрабатывает предоставленный узор асинхронно путем делегировани€ его св€занной
    /// модели представлени€ (ViewModel). ”бедитесь, что <paramref name = "pattern"/> действителен и не пуст.</remarks>
    /// <param name="sender">»сточник событи€. Ётот параметр может быть <see langword="null"/>.</param>
    /// <param name="pattern">√рафический узор, нарисованный пользователем, интерпретированный как числова€ последовательность в формате <see cref="string"/>. 
    /// Ќе может быть <see langword="null"/> или пустой.</param>
    private async void OnPatternCompleted(object? sender, string pattern)
    {
        await _viewModel.HandlePatternInputAsync(pattern);
    } 
    #endregion
}