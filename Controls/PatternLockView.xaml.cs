using System.Diagnostics;

namespace MauiLocalAuth.Controls;

public partial class PatternLockView : ContentView
{
    #region Private Variables
    private readonly PatternLockDrawable _drawable;
    #endregion

    #region Events
    public event EventHandler<string>? PatternCompleted;
    #endregion

    #region Constructor
    public PatternLockView()
    {
        InitializeComponent();

        Canvas.Drawable = _drawable = new PatternLockDrawable();

        // �������� �� �����
        Canvas.StartInteraction += OnStartInteraction; // ������ ��������������
        Canvas.DragInteraction += OnDragInteraction; // ��������������
        Canvas.EndInteraction += OnEndInteraction; // ���������� ��������������
    }
    #endregion

    #region OnStartInteraction Method
    /// <summary>
    /// ������������ ������ �������������� � Canvas.
    /// </summary>
    private void OnStartInteraction(object? sender, TouchEventArgs e)
    {
        _drawable.StartPath(e.Touches.First());// �������� ����� ���� � ������ ����� �������
        Canvas.Invalidate(); // �������������� Canvas
    }
    #endregion

    #region OnDragInteraction Method
    /// <summary>
    /// ������������ �������� ������/���� �� Canvas.
    /// </summary>
    private void OnDragInteraction(object? sender, TouchEventArgs e)
    {
        _drawable.UpdatePath(e.Touches.First()); // ��������� ���� � ������� ������ �������
        Canvas.Invalidate(); // �������������� Canvas
    }
    #endregion

    #region OnEndInteraction Method
    /// <summary>
    /// ������������ ���������� �������������� � Canvas.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        _drawable.EndPath(); // ��������� ����
        Canvas.Invalidate(); // �������������� Canvas

        var result = string.Join("", _drawable.SelectedPoints);

        //LOG
        Trace.WriteLine($"PatternLockView - OnEndInteraction: {result}");

        if (_drawable.SelectedPoints.Any()) // ���� ���� ��������� �����
        {
            PatternCompleted?.Invoke(this, result); // �������� ������� � ����������� 
            Clear();
        }
    }
    #endregion

    #region Clear Method
    /// <summary>
    /// ������� ��������� ����� � �������������� Canvas.
    /// </summary>
    public void Clear()
    {
        _drawable.Clear(); // ������� ��������� �����
        Canvas.Invalidate(); // �������������� Canvas
    }
    #endregion
}