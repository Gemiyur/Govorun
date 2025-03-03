using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Govorun.Media;

namespace Govorun.Dialogs
{
    /// <summary>
    /// Класс окна создания файла M4B из файлов MP3.
    /// </summary>
    public partial class CreateM4BDialog : Window
    {
        private readonly BackgroundWorker BackgroundWorker = new();

        public CreateM4BDialog()
        {
            InitializeComponent();
            BackgroundWorker.DoWork += BackgroundWorker_DoWork;
            BackgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var parameters = (object[]?)e.Argument;
            BackgroundWorker? worker = sender as BackgroundWorker;
            e.Result = parameters != null
                ? CreatorM4B.Create((string)parameters[0], (string)parameters[1], (bool)parameters[2], worker)
                : null;
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                StatusTextBlock.Text = "Ошибка";
            }
            else
            {
                //StatusTextBlock.Text = e.Result.ToString();
                StatusTextBlock.Text = "Готово";
            }
            var result = (FileInfo?)e.Result;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackgroundWorker.IsBusy == true)
            {
                MessageBox.Show("Выполняется предыдущая операция", Title);
                return;
            }

            var folderDialog = new OpenFolderDialog()
            {
                AddToRecent = false,
                Title = "Выбор папки с файлами книги",
            };
            if (folderDialog.ShowDialog() != true)
                return;
            var fileDialog = new SaveFileDialog()
            {
                AddToRecent = false,
                DefaultExt = ".m4b",
                Title = "Создание файл книги",
                Filter = $"Файлы книг|*.m4b"
            };
            if (fileDialog.ShowDialog() != true)
                return;
            var allImages =  MessageBox.Show("Добавить изображения из всех файлов?", Title,
                                             MessageBoxButton.YesNo) == MessageBoxResult.Yes;
            object[] parameters = [folderDialog.FolderName, fileDialog.FileName, allImages];
            StatusTextBlock.Text = "Создание...";
            BackgroundWorker.RunWorkerAsync(parameters);
            //var result = CreatorM4B.Create(folderDialog.FolderName);
            //StatusTextBlock.Text = "Готово";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (BackgroundWorker.IsBusy == true)
            {
                MessageBox.Show("Выполняется операция", Title);
                e.Cancel = true;
            }
        }
    }
}
