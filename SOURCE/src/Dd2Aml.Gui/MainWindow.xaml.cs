﻿/*
 *  Copyright (C) 
 *  2019 GSD2AML Team (Nico Dietz, Steffen Gerdes, Constantin Ruhdorfer,
 *  Jonas Komarek, Phuc Quang Vu, Michael Weidmann)
 *  2020 DD2AML Team (Antonia Wermerskirch, Nora Baitinger,
 *  Bastiane Storz, Lara Mack)
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation version 3 of the License.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Dd2Aml.Lib;
using Dd2Aml.Lib.Logging;
using Microsoft.Win32;

namespace Dd2Aml.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _strictModeEnabled = true;
        private bool _warningShown = false;

        public string ProductTitle => System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;

        public string ProductVersion => System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        public string Copyright => System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).LegalCopyright;

        public bool StrictModeEnabled
        {
            get { return _strictModeEnabled; }
            set
            {
                if (!value)
                {
                    if (_warningShown)
                    {
                        _strictModeEnabled = value;
                    }
                    else if (MessageBox.Show(this, "WARNING!\n\nBy turning strict mode off, the converter will not validate your files anymore.",
                        "DD2AML Converter", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    {
                        _warningShown = true;
                        _strictModeEnabled = value;
                    }
                }
                else
                {
                    _strictModeEnabled = value;
                }
            }
        }

        /// <summary>
        /// Initializes the main window component.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Dialogs
        private void BrowseGsdFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                DefaultExt = ".xml",
                Filter = "Files (*.xml;*.cspp)|*.xml;*.cspp",
                InitialDirectory = string.IsNullOrEmpty(TxtGsdFile.Text.Trim()) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : System.IO.Path.GetDirectoryName(TxtGsdFile.Text) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "DD2AML Converter"
            };

            if (dialog.ShowDialog(this).Value)
            {
                TxtGsdFile.Text = dialog.FileName;
                App.Logger.Log(LogLevel.Info, "Open file \"" + TxtGsdFile.Text + "\"");
            }
        }

        private void BrowseAmlFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = ".amlx",
                Filter = "AutomationML archives (.amlx)|*.amlx",
                InitialDirectory = string.IsNullOrEmpty(TxtAmlFile.Text.Trim()) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : System.IO.Path.GetDirectoryName(TxtAmlFile.Text) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                FileName = !string.IsNullOrEmpty(TxtAmlFile.Text.Trim()) ? System.IO.Path.GetFileName(TxtAmlFile.Text) : "",
                Title = "DD2AML Converter",
                ValidateNames = true
            };

            if (dialog.ShowDialog(this).Value)
            {
                TxtAmlFile.Text = dialog.FileName;
                App.Logger.Log(LogLevel.Info, "Save file to \"" + TxtGsdFile.Text + "\"");
            }
        }
        #endregion

        /// <summary>
        /// On click tries to convert the file and prompts the user whether or not to open the file in AutomationML Editor if one is installed.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">Corresponding event.</param>
        private void Convert_OnClick(object sender, RoutedEventArgs e)
        {
            if (v1.IsChecked.HasValue == true && v1.IsChecked.Value)
            {
                Lib.Converter.CAEXVersion = 2;
            }
            else if (v2.IsChecked.HasValue == true && v2.IsChecked.Value)
            {
                Lib.Converter.CAEXVersion = 3;
            }

            try
            {
                var overwrite = false;
                if (System.IO.File.Exists(TxtAmlFile.Text))
                {
                    overwrite = MessageBox.Show(this, $"The File \"{TxtAmlFile.Text}\" already exits.\n\nDo you want to overwrite it?", "DD2AML Converter", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                }

                App.Logger.Log(LogLevel.Info, "Start conversion of file \"" + TxtGsdFile.Text + "\"");
                Lib.Converter.Convert(TxtGsdFile.Text, TxtAmlFile.Text, overwrite, StrictModeEnabled);

                App.Logger.Log(LogLevel.Info, "Conversion successfully completed!");
                MessageBox.Show(this, "Conversion successfully completed!", "DD2AML Converter");

                if (GetAmlEditor() is string editor && !string.IsNullOrEmpty(editor))
                {
                    if (MessageBox.Show(this, "Do you want to open the file in AutomationML Editor?", "DD2AML Converter", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            App.Logger.Log(LogLevel.Debug, "Trying to start AutomationML Editor");
                            System.Diagnostics.Process.Start(editor, "\"" + TxtAmlFile.Text + "\"");
                        }
                        catch (Exception ex)
                        {
                            App.Logger.Log(LogLevel.Error, ex.Message);
                            MessageBox.Show(this, "An error occured when trying to open the AutomationML Editor.", "DD2AML Converter", MessageBoxButton.OK, MessageBoxImage.Error);                           
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Log(LogLevel.Error, ex.Message);
                MessageBox.Show(this, ex.Message, "DD2AML Converter: Conversion failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Searches for an installed AutomationML Editor and returns it.
        /// </summary>
        /// <returns>Path to the AutomationML Editor.</returns>
        private string GetAmlEditor()
        {
            try
            {
                App.Logger.Log(LogLevel.Debug, "Search for AutomationML Editor");
                var key = Registry.CurrentUser.OpenSubKey(@"Software\AutomationML\AutomationML Editor");

                App.Logger.Log(LogLevel.Debug, "Opened key \"" + key.Name + "\"");
                key = key.OpenSubKey(key.GetSubKeyNames().FirstOrDefault());

                App.Logger.Log(LogLevel.Debug, "Opened subkey \"" + key.Name + "\"");
                var valueName = key.GetValueNames().FirstOrDefault();

                App.Logger.Log(LogLevel.Debug, "Reading value from \"" + valueName + "\"");
                var value = key.GetValue(valueName).ToString();

                App.Logger.Log(LogLevel.Debug, "Result from value is \"" + value + "\"");
                var dir = new System.IO.DirectoryInfo(value);

                if (dir.Exists)
                {
                    var file = dir.GetFiles("AutomationMLEditor.lnk", System.IO.SearchOption.TopDirectoryOnly).FirstOrDefault();
                    return file.FullName;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Log(LogLevel.Debug, ex.Message);
            }

            return string.Empty;
        }

        #region Drag&Drop

        #region Window
        private void MainWindow_OnDragEnter(object sender, DragEventArgs e)
        {
            App.Logger.Log(LogLevel.Debug, "Drag entered window");
            var data = e.Data.GetData(DataFormats.FileDrop);
            if ((data as string[])?.FirstOrDefault() != null)
            {
                var fileName = System.IO.Path.GetFileName(((string[])data)[0]);
                App.Logger.Log(LogLevel.Debug, "File \"" + fileName + "\" drag onto window");
                if (!string.IsNullOrEmpty(fileName) && (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || fileName.EndsWith(".cspp", StringComparison.InvariantCultureIgnoreCase)))
                {
                    App.Logger.Log(LogLevel.Debug, "File is legal");
                    e.Effects = DragDropEffects.All | DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move;
                }
                else
                {
                    App.Logger.Log(LogLevel.Debug, "File is not legal");
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MainWindow_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            if ((data as string[])?.FirstOrDefault() != null)
            {
                var fileName = System.IO.Path.GetFileName(((string[])data)[0]);
                App.Logger.Log(LogLevel.Debug, "File \"" + fileName + "\" draged over window");
                if (!string.IsNullOrEmpty(fileName) &&  (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || fileName.EndsWith(".cspp", StringComparison.InvariantCultureIgnoreCase)))

                {
                    e.Effects = DragDropEffects.All | DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            App.Logger.Log(LogLevel.Debug, "Dropped file onto window");
            var data = e.Data.GetData(DataFormats.FileDrop);

            if ((data as string[])?.FirstOrDefault() == null) return;

            var fileName = System.IO.Path.GetFileName(((string[])data)[0]);
            App.Logger.Log(LogLevel.Debug, "Dropped file \"" + fileName + "\"");
            if (!string.IsNullOrEmpty(fileName) && (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || fileName.EndsWith(".cspp", StringComparison.InvariantCultureIgnoreCase)))
            {
                TxtGsdFile.Text = ((string[])data)[0];
            }
        }
        #endregion

        #region Textbox
        private void TxtGsdFile_OnDragEnter(object sender, DragEventArgs e)
        {
            App.Logger.Log(LogLevel.Debug, "Drag entered first textbox");
            var data = e.Data.GetData(DataFormats.FileDrop);
            if ((data as string[])?.FirstOrDefault() != null)
            {
                var fileName = System.IO.Path.GetFileName(((string[])data)[0]);
                App.Logger.Log(LogLevel.Debug, "File \"" + fileName + "\" drag onto first textbox");
                if (!string.IsNullOrEmpty(fileName) && 
                    (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || fileName.EndsWith(".cspp", StringComparison.InvariantCultureIgnoreCase)))    
                {
                    App.Logger.Log(LogLevel.Debug, "File is legal");
                    e.Effects = DragDropEffects.All | DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move;
                    e.Handled = true;
                }
                else
                {
                    App.Logger.Log(LogLevel.Debug, "File is not legal");
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TxtGsdFile_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            if ((data as string[])?.FirstOrDefault() != null)
            {
                var fileName = System.IO.Path.GetFileName(((string[])data)[0]);
                App.Logger.Log(LogLevel.Debug, "File \"" + fileName + "\" draged over dd textbox");
                if (!string.IsNullOrEmpty(fileName) &&
                    (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || fileName.EndsWith(".cspp", StringComparison.InvariantCultureIgnoreCase)))          
                {
                    e.Effects = DragDropEffects.All | DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move;
                    e.Handled = true;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TxtGsdFile_OnDrop(object sender, DragEventArgs e)
        {
            App.Logger.Log(LogLevel.Debug, "Dropped file onto dd textbox");
            var data = e.Data.GetData(DataFormats.FileDrop);

            if ((data as string[])?.FirstOrDefault() == null) return;

            var fileName = System.IO.Path.GetFileName(((string[])data)[0]);
            App.Logger.Log(LogLevel.Debug, "Dropped file \"" + fileName + "\"");
            if (!string.IsNullOrEmpty(fileName) &&
                (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || fileName.EndsWith(".cspp", StringComparison.InvariantCultureIgnoreCase)))          
            { 
                ((TextBox)sender).Text = fileName;
            }
        }
        #endregion

        #endregion

        /// <summary>
        /// Takes the DD file path and tries to convert it to an .amlx output path.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">Corresponding event.</param>
        private void TxtGsdFile_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var senderText = ((TextBox)sender).Text;
            if (string.IsNullOrEmpty(senderText))
                return;                                          

            // GSD
            if (Regex.IsMatch(senderText, $"(.+(GSDML|gsdml)-.+{Regex.Escape(".xml")})"))
            {
                var diretoryName = System.IO.Path.GetDirectoryName(senderText) ?? "";
                var fileName = System.IO.Path.GetFileNameWithoutExtension(senderText).Remove(0, "GSDML-".Length) + ".amlx";

                TxtAmlFile.Text = System.IO.Path.Combine(diretoryName, fileName);
                Util.filetype =  1;
            }

            //IODD
            if (Regex.IsMatch(senderText, $"(.+.-(IODD|iodd).+{Regex.Escape(".xml")})"))
            {
                var diretoryName = System.IO.Path.GetDirectoryName(senderText) ?? "";
                var fileName = System.IO.Path.GetFileNameWithoutExtension(senderText);
                if (senderText.Contains("IODD1.1"))
                {
                    fileName = fileName.Remove(fileName.Length - 8, "-IODD1.1".Length) + ".amlx";
                }
                else if (senderText.Contains("IODD1.0.1"))
                {
                    fileName = fileName.Remove(fileName.Length - 10, "-IODD1.0.1".Length) + ".amlx";
                }

                TxtAmlFile.Text = System.IO.Path.Combine(diretoryName, fileName);
                Util.filetype = 2;
            }

            //CSP+
            if (Regex.IsMatch(senderText, $"(.+{Regex.Escape(".cspp")})"))
            {
                var diretoryName = System.IO.Path.GetDirectoryName(senderText) ?? "";
                var fileName = System.IO.Path.GetFileNameWithoutExtension(senderText) + ".amlx";
            
                TxtAmlFile.Text = System.IO.Path.Combine(diretoryName, fileName);
                Util.filetype = 3;
            }

        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow() { DataContext = this, Owner = this };
            about.ShowDialog();
        }
        
    }    

}
