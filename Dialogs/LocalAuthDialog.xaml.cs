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
                // ��������� ��������� ������ � ContentView
                var view = template?.CreateContent() as View ?? throw new NullReferenceException("");
                AuthContentView.Content = view;

                // ������������� �� ������� ���������� ����� �����, ���� ������ ������ �����
                if (view is PatternLockView patternView)
                {
                    patternView.PatternCompleted += OnPatternCompleted;
                }
                // ��� �� ������� ���������� ����� PIN-����, ���� ������ ������ PIN-����
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
    /// ������������ ���������� ����� PIN -���� �������������.
    /// </summary>
    /// <remarks>���� ����� ������������ ��������������� PIN -��� ���������� ����� ������������� ��� ���������
    /// ������ ������������� (ViewModel). ���������, ��� <paramref name = "pinCode"/> ������������ � �� ����.</remarks>
    /// <param name="sender">�������� �������. ���� �������� ����� ���� <see langword="null"/>.</param>
    /// <param name="pinCode">PIN -���, ��������� �������������. �� ����� ���� <see langword="null"/> ��� ������.</param>
    private async void OnPinCompleted(object? sender, string pinCode)
    {
        await _viewModel.HandlePinInputAsync(pinCode);
    }
    #endregion

    #region OnPatternCompleted Method
    /// <summary>
    /// ������������ ���������� ����� ����� �������������.
    /// </summary>
    /// <remarks>���� ����� ������������ ��������������� ���� ���������� ����� ������������� ��� ���������
    /// ������ ������������� (ViewModel). ���������, ��� <paramref name = "pattern"/> ������������ � �� ����.</remarks>
    /// <param name="sender">�������� �������. ���� �������� ����� ���� <see langword="null"/>.</param>
    /// <param name="pattern">����������� ����, ������������ �������������, ������������������ ��� �������� ������������������ � ������� <see cref="string"/>. 
    /// �� ����� ���� <see langword="null"/> ��� ������.</param>
    private async void OnPatternCompleted(object? sender, string pattern)
    {
        await _viewModel.HandlePatternInputAsync(pattern);
    } 
    #endregion
}