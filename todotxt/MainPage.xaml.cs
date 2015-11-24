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


namespace todotxt
{
    public sealed partial class MainPage : Page
    {
        private Windows.Storage.ApplicationDataContainer localSettings;
        private Windows.Storage.StorageFile todoFile;
        private Windows.Storage.StorageFile doneFile;
        private string todoFileToken = string.Empty;
        private string doneFileToken = string.Empty;
        private List<string> todoText;
        private string doneTextToAdd = string.Empty;
        // TODO: check hast to be done if file successfully written
        private bool doneFileSuccessfullyWritten = true;
        // TODO: when selected item is changed, update item instead of creating a new one
        private Object currentItem;

        public Style CrossedOutStyle { set; get; }

        public MainPage()
        {
            this.InitializeComponent();
            todoText = new List<string>();
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            Object autoAddDate = localSettings.Values["autoAddDate"];
            if (autoAddDate != null)
            {
                if (autoAddDate is bool && (bool)autoAddDate == true)
                {
                    autoDateCB.IsChecked = true;
                }
                else
                {
                    autoDateCB.IsChecked = false;
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
                    loadFileFromToken("todo");
                }
            }

            Object loadedDoneFileToken = localSettings.Values["doneFileToken"];
            if (loadedDoneFileToken != null)
            {
                if (loadedDoneFileToken.GetType() == doneFileToken.GetType())
                {
                    doneFileToken = (string)loadedDoneFileToken;
                    loadFileFromToken("done");
                }
            }

        }

        private async void loadFileFromToken(string fileType)
        // Tries to open a todo or done file from a token previously stored in the settings store.
        {
            bool done = false;
            switch (fileType)
            {
                case "todo":
                    try
                    {
                        todoFile = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(todoFileToken);
                        done = true;
                    }
                    catch (FileNotFoundException)
                    {
                        Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("todo.txt file not found. Specify a new one in the Settings.", "todo.txt file not found");
                    }
                    if (done)
                    {
                        updateTodoFile();
                    }
                    break;
                case "done":
                    try
                    {
                        doneFile = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(doneFileToken);
                        done = true;

                    }
                    catch (FileNotFoundException)
                    {
                        Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("done.txt file not found. Specify a new one in the Settings.", "done.txt file not found");
                    }
                    break;
            }
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

        private async void updateTodoFile(string mode = "refresh", string itemToAlter = "")
        {
            string todoPlainTextTemp = string.Empty;
            try
            {
                if (todoFile != null)
                {
                    todoPlainTextTemp = await Windows.Storage.FileIO.ReadTextAsync(todoFile);
                }
            }
            catch (FileNotFoundException)
            {
                //filenotfound
            }
            //string[] seperator = { "\r\n", "\n" }; // matches unix and windows newlines
            List<string> todoTextTemp = new List<string>(todoPlainTextTemp.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            // check if the file changed locally and if so load the changes into todoText
            foreach (string item in todoTextTemp)
            {
                if (!todoText.Contains(item))
                {
                    todoText.Insert(todoTextTemp.IndexOf(item), item);
                }
            }

            if (mode == "remove")
            {
                todoText.Remove(itemToAlter);
            }
            if (mode == "add")
            {
                todoText.Add(itemToAlter);
            }

            // check if changes were made to todoText, if so write changes to file (rewrite file)
            if (!todoTextTemp.SequenceEqual(todoText))
            {
                try
                {
                    if (todoFile != null)
                    {
                        string todoTextToWrite = string.Empty;
                        foreach (string text in todoText)
                        {
                            todoTextToWrite = todoTextToWrite.Insert(todoTextToWrite.Length, text);
                            todoTextToWrite = todoTextToWrite.Insert(todoTextToWrite.Length, Environment.NewLine);
                        }
                        await Windows.Storage.FileIO.WriteTextAsync(todoFile, todoTextToWrite);
                    }
                }
                catch (FileNotFoundException)
                {
                    //filenotfound
                }
            }
            populateTodoList();
        }

        private async void updateDoneFile()
        {
            try
            {
                if (doneFile != null)
                {
                    await Windows.Storage.FileIO.AppendTextAsync(doneFile, doneTextToAdd);
                    await Windows.Storage.FileIO.AppendTextAsync(doneFile, Environment.NewLine);
                    doneFileSuccessfullyWritten = true;
                }
            }
            catch (FileNotFoundException)
            {
                //filenotfound
                //filenotset
            }
        }

        private void addTodoElement()
        // Adds a todo element to the todo.txt file while respecting the apps settings.
        {
            string textToAdd = "";
            if (inputBox.Text[0] == '(' && inputBox.Text[2] == ')')
            {

            }
            if (autoDateCB.IsChecked == true)
            {
                textToAdd = DateTime.Now.ToString("yyyy-MM-dd") + " ";
            }
            textToAdd = textToAdd.Insert(textToAdd.Length, inputBox.Text);
            updateTodoFile("add", textToAdd);
        }

        private void removeTodoElement()
        // Initiates removal (deletion) of the currently selected todo item.
        {
            updateTodoFile("remove", currentItem.ToString());
        }

        private async void loadFile(string fileType)
        // Opens the file picker to let the user chose a todo or done file.
        // Stores the choice in the app's settings store.
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
                            updateTodoFile();
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        //filenotefound
                    }
                    break;
                case "done":
                    doneFile = await openPicker.PickSingleFileAsync();
                    doneFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(doneFile, doneFile.Name);
                    localSettings.Values["doneFileToken"] = doneFileToken;
                    break;
            }
        }

        private void populateTodoList()
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

        public void showOnItemSelection()
        {
            deleteButton.Visibility = Visibility.Visible;
            doneButton.Visibility = Visibility.Visible;

        }

        public void hideOnItemSelection()
        {
            deleteButton.Visibility = Visibility.Collapsed;
            doneButton.Visibility = Visibility.Collapsed;
        }

        private void archiveItem()
        {
            if (currentItem == null)
            {
                return;
            }
            doneTextToAdd = currentItem.ToString();
            string todoTextToRemove = doneTextToAdd;
            doneTextToAdd = doneTextToAdd.Insert(0, "x ");
            if (autoDateCB.IsChecked == true)
            {
                doneTextToAdd = doneTextToAdd.Insert(2, DateTime.Now.ToString("yyyy-MM-dd") + " ");
            }
            updateDoneFile();
            // TODO: check hast to be done if file successfully written
            if (doneFileSuccessfullyWritten)
            {
                updateTodoFile("remove", todoTextToRemove);
            }
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

        private void todoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentItem = todoList.SelectedItem;
            if (todoList.Items.Contains(currentItem))
            {
                
                inputBox.Text = currentItem.ToString();
            }
            else
            {
                todoList.SelectedIndex = -1;
                inputBox.Text = "";
            }
            showOnItemSelection();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            currentItem = todoList.SelectedItem;
            inputBox.Text = "";
            removeTodoElement();
            hideOnItemSelection();
        }

        // TODO: only cross item out. remove only if auto archive is enabled
        private void doneButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentItem == null)
            {
                return;
            }

            if (autoArchiveCB.IsChecked == true)
            {
                archiveItem();
            }
            else
            {
                Style st = new Style();
                st.TargetType = typeof(ListBoxItem);
                st.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, "Blue"));
                //Resources.Add(typeof(ListBoxItem), st);
                todoList.ItemContainerStyle = st;
            }

        }
        
    }

}
