﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayer
{
    public partial class MainWindow : Window
    {
        private MediaPlayer _player = new MediaPlayer();
        private List<FileInfo> fileInfoFolder = new List<FileInfo>();
        public static RoutedCommand CloseCommand = new RoutedCommand();
        public List<string> Playlist = new List<string>();
        public MainWindow()
        {
            InitializeComponent();

            _player = new MediaPlayer();
            this.DataContext = this;            
        }

        private void OpenFileMI_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                ReadMP3File(openFileDialog.FileName);
                _player.Open(new Uri(openFileDialog.FileName));
            }
        }

        private void OpenFolderMI_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();

            var fileDialogOk = fileDialog.ShowDialog();

            var filename = fileDialog.SelectedPath;

            DirectoryInfo dirInfo = new DirectoryInfo(filename);

            FileInfo[] fileInfo = dirInfo.GetFiles("*.mp3");
            foreach (FileInfo f in fileInfo)
            {
                fileInfoFolder.Add(f);
            }

            foreach (var f in fileInfoFolder)
            {
                Playlist.Add(f.Name);
                FilesLV.Items.Add(f.Name);
            }
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            _player.Play();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            _player.Pause();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Close");
        }

        private void ReadMP3File(string filePath)
        {
            var file = TagLib.File.Create(filePath);

            TimeSpan duration = file.Properties.Duration;

            string artist = file.Tag.FirstAlbumArtist ?? file.Tag.FirstArtist ?? "Unknown Artist";

            string title = file.Tag.Title ?? "Unknown Title";

            string album = file.Tag.Album ?? "Unknown Album";

            var pictures = file.Tag.Pictures;
            if (pictures.Length > 0)
            {
                var coverData = pictures[0].Data.Data;

                BitmapImage coverImage = new BitmapImage();
                using (MemoryStream coverStream = new MemoryStream(coverData))
                {
                    coverImage.BeginInit();
                    coverImage.CacheOption = BitmapCacheOption.OnLoad;
                    coverImage.StreamSource = coverStream;
                    coverImage.EndInit();
                }

                MusicImage.Source = coverImage;
            }

            TitleLabel.Content = title;
            ArtistTB.Text = artist;
            AlbumTB.Text = album;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show($"Commits >= 3: 1 pont\nCustom disign: 1 point\nPlayer can play & work correctly: 2 point\nHot keys: 1 point\nLoad mp3 info & image: 1 point\n*Can choose track from folder: 1 point\n*Save new playlist: 1 point",
                "Points total: 8 points", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = FilesLV.SelectedItem;

            foreach (var f in fileInfoFolder)
            {
                if (f.Name == selectedItem.ToString())
                {
                    _player.Open(new Uri(f.FullName));
                    ReadMP3File(f.FullName);
                }
            }
        }
    }
}
