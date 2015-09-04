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
        private Windows.Storage.ApplicationDataContainer localSettings;
        private Windows.Storage.StorageFile todoFile;
        private Windows.Storage.StorageFile doneFile;
        private string todoFileToken = "";
        private List<string> todoText;

        public MainPage()
        {
            this.InitializeComponent();
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Size { Height = 550, Width = 420 };
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;
            todoText = new List<string>();
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            Object autoAddDate = localSettings.Values["autoAddDate"];
            if (autoAddDate != null)
            {
                if (autoAddDate is bool)
                {
                    if ((bool)autoAddDate == true)
                    {
                        autoDateCB.IsChecked = true;
                    }
                    else
                    {
                        autoDateCB.IsChecked = false;
                    }

                }
            }

            Object autoArchive = localSettings.Values["autoArchive"];
            if (autoArchive != null)
            {
                if (autoArchive is bool && (bool)autoArchive == true)
                {
                    autoArchiveCB.IsChecked = true;
                }
            }

            Object loadedTodoFileToken = localSettings.Values["todoFileToken"];
            if (loadedTodoFileToken != null)
            {
                if (loadedTodoFileToken.GetType() == todoFileToken.GetType())
                {
                    todoFileToken = (string)loadedTodoFileToken;
                    // error handling missing
                    loadTodoFileFromToken();
                }
            }
        }

        private async void loadTodoFileFromToken()
        {
            // error handling missing
            todoFile = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(todoFileToken);
            readTodoFile();
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(inputBox.Text) && !todoList.Items.Contains(inputBox.Text))
            {
                addTodoElement();
            }
        }

        private void chooseTodoFile_Click(object sender, RoutedEventArgs e)
        {
            loadFile("todo");
        }

        private void chooseDoneFile_Click(object sender, RoutedEventArgs e)
        {
            loadFile("done");
        }

        private async void addTodoElement()
        {
            string textToAdd = "";
            if (autoDateCB.IsChecked == true)
            {
                textToAdd = DateTime.Now.ToString("yyyy-MM-dd") + " ";
            }
            textToAdd = textToAdd.Insert(textToAdd.Length, inputBox.Text);
            todoText.Add(textToAdd);
            fillTodoList();
            if (todoFile != null)
            {
                await Windows.Storage.FileIO.AppendTextAsync(todoFile, textToAdd + Environment.NewLine);
            }
        }

        private async void loadFile(string fileType)
        {
            Windows.Storage.Pickers.FileOpenPicker openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".txt");
            switch (fileType)
            {
                case "todo":
                    todoFile = await openPicker.PickSingleFileAsync();
                    try
                    {
                        if (todoFile != null)
                        {
                            todoFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(todoFile, todoFile.Name);
                            localSettings.Values["todoFileToken"] = todoFileToken;
                            readTodoFile();
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        //filenotefound
                    }
                    break;
                case "done":
                    doneFile = await openPicker.PickSingleFileAsync();
                    localSettings.Values["doneFile"] = doneFile;
                    break;
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

        private void autoDateCB_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["autoAddDate"] = true;
        }

        private void autoDateCB_Unchecked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["autoAddDate"] = false;
        }

        private void autoArchiveCB_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["autoArchive"] = true;
        }

        private void autoArchiveCB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (localSettings.Values["autoArchive"] != null)
            {
                localSettings.Values.Remove("autoArchive");
            }
        }
    }
}
