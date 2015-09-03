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
        // private string[] todoText;
        private List<string> todoText;
        private bool todoDataWasChanged = false;

        public MainPage()
        {
            this.InitializeComponent();
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Size { Height = 550, Width = 420 };
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;            
        }

        private async void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(inputBox.Text) && !todoList.Items.Contains(inputBox.Text))
            {
                todoText.Add(inputBox.Text);
                fillTodoList();
                if (todoFile != null)
                {
                    await Windows.Storage.FileIO.AppendTextAsync(todoFile, inputBox.Text + Environment.NewLine);
                }
            }
        }

        private void fileOpen_Click(object sender, RoutedEventArgs e)
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
            todoText = new List<string>(todoTextTemp.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            this.fillTodoList();
        }

        private void fillTodoList()
        {
            todoText.Sort(compareTodoStrings);
            todoList.Items.Clear();
            for(int i = 0; i < todoText.Count; ++i)
            {
                todoList.Items.Add(todoText[i]);
            } 
        }

        public static int compareTodoStrings(string s1, string s2)
        {
            if (s1[0].Equals('('))
            {
                if (!s2[0].Equals('('))
                    return -1;
                switch (s1[1].ToString().CompareTo(s2[1].ToString()))
                {
                    case -1:
                        return -1;
                    case 1:
                        return 1;
                    case 0:
                        return dateComparison(s1, s2);
                }
            }
            else if (s2[0].Equals('('))
            {
                return 1;
            }
            else
                return dateComparison(s1, s2);
            return -1;
        }

        public static int dateComparison(string s1, string s2)
        {
            // compare the date in the strings and return the larger one with return 0
            // otherwise return 1
            // strings also might not have a date string
            return 0;
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
