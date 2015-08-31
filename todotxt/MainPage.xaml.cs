using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace todotxt
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Windows.Storage.StorageFile todoFile;
        private string[] todoText;

        public MainPage()
        {
            this.InitializeComponent();

            
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(inputBox.Text) && !todoList.Items.Contains(inputBox.Text))
            {
                todoList.Items.Add(inputBox.Text);
            }
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            this.loadTodoFile();
        }

        private async void loadTodoFile()
        {
            Windows.Storage.Pickers.FileOpenPicker openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".txt");
            todoFile = await openPicker.PickSingleFileAsync();
            try
            {
                if (todoFile != null)
                {
                    //todoFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(todoFile);
                    readTodoFile();
                }
            }
            catch(FileNotFoundException)
            {
                //filenotefound
            }
        }

        private async void readTodoFile()
        {
            string todoTextTemp = await Windows.Storage.FileIO.ReadTextAsync(todoFile);
            // string[] seperator = { "\r\n", "\n" };  matches unix and windows newlines
            todoText = todoTextTemp.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            this.fillTodoList();
        }

        private void fillTodoList()
        { 
            for(int i = 0; i < todoText.Length; ++i)
            {
                todoList.Items.Add(todoText[i]);
            }
        }
    }
}
