using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.KeyListener;
using System.Diagnostics;

namespace MauiLocalAuth.Controls;

public partial class PinCodeLockView : ContentView
{
    #region Events
    public event EventHandler<string>? PinCodeCompleted;
    #endregion

    #region Bindable Properties
    #region PinCode
    public static readonly BindableProperty PinCodeProperty =
        BindableProperty.Create(
            propertyName: nameof(PinCode),
            returnType: typeof(string),
            declaringType: typeof(PinCodeLockView),
            defaultValue: string.Empty,
            propertyChanged: OnPinCodeChanged);

    public string PinCode
    {
        get => (string)GetValue(PinCodeProperty);
        set => SetValue(PinCodeProperty, value);
    }

    private static void OnPinCodeChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is PinCodeLockView control &&
            newValue is string pinCode)
        {
            if (pinCode.Length == 4)
            {
                var completedPin = pinCode;

                // Используем небольшой таймаут для визуальной обратной связи
                Task.Delay(100).ContinueWith(t =>
                {
                    // сигнатура метода ContinueWith: 
                    /*
                        ContinueWith(
                            action: Action<Task>, 
                            cancellationToken?: CancellationToken, 
                            taskContinuationOptions?: TaskContinuationOptions, 
                            scheduler?: TaskScheduler
                        )
                    */

                    // Dispatcher управляет очередью сообщений для потока (в MAUI - для UI-потока)
                    // Dispatcher.Dispatch() ставит действие в очередь UI-потока 
                    // и обеспечивает выполнение кода именно в UI-потоке 
                    // (в дополнение к TaskScheduler.FromCurrentSynchronizationContext() для гарантии)
                    control.Dispatcher.Dispatch(() =>
                    {
                        control.PinCode = string.Empty;
                        control.PinCodeCompleted?.Invoke(control, completedPin);
                    });
                }, TaskScheduler.FromCurrentSynchronizationContext());
                // TaskScheduler.FromCurrentSynchronizationContext() возвращает планировщик задач,
                // который позволяет вернуться из фонового потока, в котором выполняется Task.Delay(),
                // в текущий поток - главный поток (для обновления UI) 


                // код выше можно было заменить на код с async/await:
                /*
                    await Task.Delay(100);
                    control.PinCode = string.Empty;
                    control.PinCodeCompleted?.Invoke(control, completedPin);
                */
                // но тогда метод OnPinCodeChanged() должен быть async
            }
        }
    }
    #endregion

    #region AddDigitCommand
    public IAsyncRelayCommand<string> AddDigitCommand { get; }
    #endregion

    #region RemoveDigitCommand
    public IAsyncRelayCommand RemoveDigitCommand { get; }
    #endregion
    #endregion

    #region Constructor
    public PinCodeLockView()
    {
        AddDigitCommand = new AsyncRelayCommand<string>(AddDigit);
        RemoveDigitCommand = new AsyncRelayCommand(RemoveDigit);

        InitializeComponent();

        BindingContext = this;

        
    }
    #endregion

    #region AddDigit Method
    /// <summary>
    /// Добавляет цифру к полю с PIN-кодом.
    /// </summary>
    /// <param name="digit">Цифра в формате <see cref="string"/></param>
    private async Task AddDigit(string? digit)
    {
        if (digit is null) return;

        try
        {
            // Анимация нажатия
            var gridButton = FindDigitGrid(digit);
            if (gridButton is not null) await AnimateDigitTap(gridButton);

            // логика добавления цифры
            if (PinCode.Length < 4) PinCode += digit;
        }
        catch (Exception ex)
        {
            // Обработка ошибок
            System.Diagnostics.Trace.WriteLine($"Ошибка в AddDigit: {ex.Message}");
        }
    }
    #endregion

    #region RemoveDigit Method
    /// <summary>
    /// Удаляет последнюю цифру из поля с PIN-кодом.
    /// </summary>
    public async Task RemoveDigit()
    {
        try
        {
            if (!string.IsNullOrEmpty(PinCode))
            {
                // Анимация для кнопки удаления (опционально)
                var backspaceGrid = FindBackspaceGrid();
                if (backspaceGrid is not null) await AnimateDigitTap(backspaceGrid);

                PinCode = PinCode[..^1];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Ошибка в RemoveDigit: {ex.Message}");
        }
    }
    #endregion

    #region Clear Method
    /// <summary>
    /// Очищает поле с PIN-кодом.
    /// </summary>
    public void Clear() => PinCode = string.Empty;
    #endregion

    #region AnimateDigitTap Method
    private async Task AnimateDigitTap(View view)
    {
        if (view is Grid gridButton)
        {
            var label = gridButton.Children.OfType<Label>().FirstOrDefault();
            if (label is not null) await AnimateViewTap(label);
        }
    }
    #endregion

    #region AnimateBackspaceTap Method
    private async Task AnimateBackspaceTap()
    {
        var backspaceGrid = FindBackspaceGrid();
        if (backspaceGrid is not null)
        {
            var label = backspaceGrid.Children.OfType<Label>().FirstOrDefault();
            if (label is not null) await AnimateViewTap(label);
        }
    }
    #endregion

    #region AnimateViewTap Method
    private async Task AnimateViewTap(View view)
    {
        // Сохраняем исходный масштаб
        var originalScale = view.Scale;

        // Быстрая анимация нажатия
        await view.ScaleTo(originalScale * 0.8, 50, Easing.SinIn);
        // Easing.SignIn - в начале - медленное, затем быстрое движение (по синусойде от 0 до π/2)

        // Возвращаем к нормальному размеру
        await view.ScaleTo(originalScale, 100, Easing.SpringOut);
        // Easing.SpringOut - с пружинкой, создает эффект упругости
    }
    #endregion

    #region FindDigitGrid Method
    private Grid? FindDigitGrid(string digit) => digit switch
    {
        "0" => FindGridByPosition(3, 1),

        "1" => FindGridByPosition(0, 0),
        "2" => FindGridByPosition(0, 1),
        "3" => FindGridByPosition(0, 2),

        "4" => FindGridByPosition(1, 0),
        "5" => FindGridByPosition(1, 1),
        "6" => FindGridByPosition(1, 2),

        "7" => FindGridByPosition(2, 0),
        "8" => FindGridByPosition(2, 1),
        "9" => FindGridByPosition(2, 2),
        _ => null
    };
    #endregion

    #region FindBackspaceGrid Method
    private Grid? FindBackspaceGrid() => FindGridByPosition(3, 0);
    #endregion

    #region FindGridByPosition Method
    private Grid? FindGridByPosition(int row, int column)
    {
        if (Content is VerticalStackLayout layout)
        {
            // Children возвращает все дочерние элементы, 
            // а LINQ-метод OfType<T> возвращает все элементы типа T
            var grid = layout.Children.OfType<Grid>().FirstOrDefault();
            if (grid is not null)
                return grid.Children
                    .OfType<Grid>()
                    .FirstOrDefault(grid =>
                        Grid.GetRow(grid) == row &&
                        Grid.GetColumn(grid) == column);
        }
        return null;
    }
    #endregion

    private void OnKeyDown(object sender, KeyPressedEventArgs e)
    {
        if (e.KeyChar == '\0')
        {
            var key = e.Keys switch 
            {
                KeyboardKeys.NumPad0 or KeyboardKeys.Insert => '0', // Insert
                KeyboardKeys.NumPad1 or KeyboardKeys.End => '1', // End
                KeyboardKeys.NumPad2 or KeyboardKeys.DownArrow => '2', // DownArrow
                KeyboardKeys.NumPad3 or KeyboardKeys.PageDown => '3', // PageDown
                KeyboardKeys.NumPad4 or KeyboardKeys.LeftArrow => '4', // LeftArrow
                KeyboardKeys.NumPad5 or KeyboardKeys.None => '5', // None
                KeyboardKeys.NumPad6 or KeyboardKeys.RightArrow => '6', // RightArrow
                KeyboardKeys.NumPad7 or KeyboardKeys.Home => '7', // Home
                KeyboardKeys.NumPad8 or KeyboardKeys.UpArrow => '8', // UpArrow
                KeyboardKeys.NumPad9 or KeyboardKeys.PageUp => '9', // PageUp
                _ => '\0'
            };

            if (key != '\0')
            {
                _ = AddDigit(key.ToString());
            }
        }

        if (char.IsDigit(e.KeyChar) && e.KeyChar != '\0')
        {
            _ = AddDigit(e.KeyChar.ToString());
        }

        else if (e.Keys == KeyboardKeys.Backspace)
        {
            _ = RemoveDigit();
        }
    }
}