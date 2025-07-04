using System.Windows;
using System.Windows.Controls;

namespace Govorun.Controls;

/// <summary>
/// Класс всплывающего редактора текста.
/// </summary>
/// <remarks>
/// Взаимодействие происходит через свойство Visibility.<br/>
/// После нажатия кнопки свойство устанавливается в Visibility.Collapsed.<br/>
/// Для определения когда было завершено редактирование следует подписаться на событие IsVisibleChanged.<br/>
/// Какая кнопка была нажата содержится в свойстве Result:<br/>
/// true - была нажата кнопка "Сохранить", false - была нажата кнопка "Отмена".<br/>
/// </remarks>
public partial class PopupTextEditorControl : UserControl
{
    /// <summary>
    /// Возвращает или задаёт заголовок.
    /// </summary>
    public string Header
    {
        get => HeaderTextBlock.Text;
        set => HeaderTextBlock.Text = value;
    }

    /// <summary>
    /// Возвращает результат редактирования после нажатия кнопки.
    /// </summary>
    /// <remarks>true - была нажата кнопка "Сохранить", false - была нажата кнопка "Отмена".</remarks>
    public bool Result { get; private set; }

    private string text = string.Empty;

    /// <summary>
    /// Возвращает или задаёт редактируемый текст.
    /// </summary>
    /// <remarks>
    /// Во время редактирования возвращает исходный текст.<br/>
    /// После нажатия кнопки "Сохранить" возвращает результат.
    /// </remarks>
    public string Text
    {
        get => text;
        set
        {
            text = value;
            EditorTextBox.Text = text;
            Result = false;
        }
    }

    public PopupTextEditorControl()
    {
        InitializeComponent();
    }

    private void EditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = EditorTextBox.Text.Length > 0 && EditorTextBox.Text != text;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        text = EditorTextBox.Text;
        Result = true;
        Visibility = Visibility.Collapsed;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        Visibility = Visibility.Collapsed;
    }
}
