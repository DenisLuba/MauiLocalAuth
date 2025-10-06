using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiLocalAuth.Resources.Strings.LocalAuthDialogViewModelResources;
using MauiShared.Services;

namespace MauiLocalAuth.ViewModels;

public partial class LocalAuthDialogViewModel(
        LocalAuthPreferencesService authPreferencesService,
        SecurityService securityService,
        INavigationService navigationService,
        string? authPage = null) : ObservableObject
{
    #region Observable Properties
    [ObservableProperty] private bool _isPattern;
    [ObservableProperty] private bool _isPinCode;

    [ObservableProperty] private string _currentAuthTemplateKey = string.Empty;
    #endregion

    #region Constants
    private const string PATTERN_HASH_KEY = "HASH KEY";
    private const string PIN_CODE_HASH_KEY = "PIN CODE HASH KEY";

    internal const string PATTERN_TEMPLATE = "PATTERN TEMPLATE";
    internal const string PIN_CODE_TEMPLATE = "PIN CODE TEMPLATE";
    #endregion

    #region Private Fields
    private string? _firstInputCache; // временное хранилище для первого ввода
    private bool _isFirstSetup; // это первый вход?
    #endregion

    #region EntranceCompletedEvent
    public EventHandler<bool>? EntranceCompletedEvent; 
    #endregion

    #region InitializeAsync Method
    /// <summary>
    /// Загружает настройки методов локальной аутентификации и обновляет свойства IsPattern и IsPinCode.
    /// </summary>
    internal async Task InitializeAsync()
    {
        var methods = await authPreferencesService.GetAuthMethodAsync();

        IsPattern = methods.HasFlag(LocalAuthMethod.Pattern);
        IsPinCode = methods.HasFlag(LocalAuthMethod.PinCode);

        // Если хэш не установлен, значит это первый вход
        _isFirstSetup = !(await securityService.HasHashAsync(
            IsPattern ? PATTERN_HASH_KEY : PIN_CODE_HASH_KEY));

        CurrentAuthTemplateKey = IsPattern ?
            PATTERN_TEMPLATE : IsPinCode ?
            PIN_CODE_TEMPLATE : string.Empty;
    }
    #endregion

    #region HandlePatternInputAsync Method
    /// <summary>
    /// Обрабатывает ввод графического узора и управляет логикой первого ввода и подтверждения.
    /// </summary>
    internal async Task HandlePatternInputAsync(string pattern)
    {
        if (_isFirstSetup)
        {
            if (_firstInputCache is null)
            {
                // Сохраняем первый ввод во временное хранилище
                _firstInputCache = pattern;
                await Shell.Current.DisplayAlert(ResourcesLocalAuthDialogViewModel.confirmation, ResourcesLocalAuthDialogViewModel.repeat_pattern, "OK");
            }
            else
            {
                // Сверяем второй ввод с первым
                if (_firstInputCache == pattern)
                {
                    var hash = securityService.ComputeHash(pattern);
                    await securityService.SaveHashAsync(PATTERN_HASH_KEY, hash);
                    await navigationService.PopModalAsync(true);
                }
                else
                {
                    _firstInputCache = null; // Сбрасываем кэш
                    await Shell.Current.DisplayAlert(ResourcesLocalAuthDialogViewModel.error, ResourcesLocalAuthDialogViewModel.invalid_pattern, "OK");
                }
            }
        }
        else
        {
            await PatternCompletedAsync(pattern);
        }
    }
    #endregion

    #region PatternCompletedAsync Method
    /// <summary>
    /// Обрабатывает завершение ввода графического узора.
    /// </summary>
    internal async Task PatternCompletedAsync(string pattern) =>
        await EntranceCompletedAsync(
            code: pattern, 
            hashKey: PATTERN_HASH_KEY, 
            errorMessage: ResourcesLocalAuthDialogViewModel.invalid_pattern);
    #endregion

    #region HandlePinInputAsync Method
    /// <summary>
    /// Обрабатывает ввод PIN-кода и управляет логикой первого ввода и подтверждения.
    /// </summary>
    /// <param name="pinCode">ПИН-КОД</param>
    internal async Task HandlePinInputAsync(string pinCode)
    {
        if (_isFirstSetup)
        {
            if (_firstInputCache is null)
            {
                // Сохраняем первый ввод во временное хранилище
                _firstInputCache = pinCode;
                await Shell.Current.DisplayAlert(ResourcesLocalAuthDialogViewModel.confirmation, ResourcesLocalAuthDialogViewModel.repeat_pin, "OK");
            }
            else
            {
                if (_firstInputCache == pinCode)
                {
                    var hash = securityService.ComputeHash(pinCode);
                    await securityService.SaveHashAsync(PIN_CODE_HASH_KEY, hash);
                    await navigationService.PopModalAsync(true);
                }
                else
                {
                    _firstInputCache = null; // Сбрасываем кэш
                    await Shell.Current.DisplayAlert(ResourcesLocalAuthDialogViewModel.error, ResourcesLocalAuthDialogViewModel.invalid_pin_code, "OK");
                }
            }
        }
        else
        {
            await PinCodeCompletedAsync(pinCode);
        }
    } 
    #endregion

    #region PinCodeCompletedCommand
    /// <summary>
    /// Обрабатывает завершение ввода PIN-кода.
    /// </summary>
    [RelayCommand]
    internal async Task PinCodeCompletedAsync(string pinCode) => 
        await EntranceCompletedAsync(
            code: pinCode, 
            hashKey: PIN_CODE_HASH_KEY, 
            errorMessage: ResourcesLocalAuthDialogViewModel.invalid_pin_code);
    #endregion

    #region CloseCommand
    [RelayCommand]
    internal async Task CloseAsync()
    {
        if (authPage is not null) await Shell.Current.GoToAsync(authPage);

        await navigationService.PopModalAsync(true);
    }
    #endregion

    #region EntranceCompletedAsync Method
    /// <summary>
    /// Обрабатывает завершение ввода (графического узора или ПИН-КОДА) с возможностью указания сообщения об ошибке.
    /// </summary>
    /// <param name="code">ПИН-КОД или графический узор (его строковое представление)</param>
    /// <param name="hashKey">Ключ для значения в хранилище.</param>
    /// <param name="errorMessage">Сообщение об ошибке.</param>
    /// <returns></returns>
    private async Task EntranceCompletedAsync(string code, string hashKey, string errorMessage)
    {
        var hash = securityService.ComputeHash(code);
        var isValid = await securityService.CheckHashAsync(hashKey, hash);

        if (isValid)
        {
            EntranceCompletedEvent?.Invoke(this, true);
            await navigationService.PopModalAsync(true);
        }
        else
        {
            await Shell.Current.DisplayAlert(ResourcesLocalAuthDialogViewModel.error, errorMessage, "OK");
        }
    } 
    #endregion
}
