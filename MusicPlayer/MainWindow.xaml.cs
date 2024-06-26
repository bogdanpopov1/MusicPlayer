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
using System.Windows.Threading;

namespace MusicPlayer
{
    public partial class MainWindow : Window
    {
        private MediaPlayer _player = new MediaPlayer();
        private DispatcherTimer _timer;
        private bool triggerOpen = true;

        private List<FileInfo> fileInfoFolder = new List<FileInfo>();

        public static RoutedCommand CloseCommand = new RoutedCommand();
        public static RoutedCommand OpenFileCommand = new RoutedCommand();
        public static RoutedCommand OpenFolderCommand = new RoutedCommand();

        public List<string> Playlist = new List<string>();

        private FileInfo _currentTrack = null;
        private int _currentTrackIndex = -1;

        public MainWindow()
        {
            InitializeComponent();

            _player = new MediaPlayer();
            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            this.DataContext = this;
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            MusicSlider.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
            TotalTimeLabel.Content = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            _timer.Start();
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            MusicSlider.Value = 0;
            CurrentTimeLabel.Content = "0:00";
            _timer.Stop();
            PlayNextTrack();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            MusicSlider.Value = _player.Position.TotalSeconds;
            CurrentTimeLabel.Content = _player.Position.ToString(@"mm\:ss");
        }

        private void MusicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double volume = e.NewValue / 100.0;
            _player.Volume = volume;
        }

        private void VolumeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_player.Volume == 0)
            {
                _player.Volume = 100;
                VolumeSlider.Value = 100;
            }
            else
            {
                _player.Volume = 0;
                VolumeSlider.Value = 0;
            }
        }

        private void PlayNextTrack()
        {
            string nextTrackPath = GetNextTrackPath();

            if (!string.IsNullOrEmpty(nextTrackPath))
            {
                if (_currentTrackIndex == fileInfoFolder.Count)
                    _currentTrack = fileInfoFolder[0];
                else
                    _currentTrack = fileInfoFolder[_currentTrackIndex];
                _player.Open(new Uri(nextTrackPath));
                _player.Play();
                FilesLV.SelectedIndex = _currentTrackIndex;
                ReadMP3File(nextTrackPath);
            }
        }

        private string GetNextTrackPath()
        {
            if (fileInfoFolder.Count == 0) return null;

            if (FilesLV.SelectedItem != null)
            {
                var selectedItem = FilesLV.SelectedItem;

                foreach (var f in fileInfoFolder)
                {
                    if (f.Name == selectedItem.ToString())
                    {
                        _currentTrackIndex = fileInfoFolder.IndexOf(f) + 1;
                    }
                }

                FilesLV.SelectedItem = null;
            }
            else
            {
                _currentTrackIndex = (_currentTrackIndex + 1) % fileInfoFolder.Count;
            }

            if (_currentTrackIndex == fileInfoFolder.Count)
                _currentTrack = fileInfoFolder[0];
            else
                _currentTrack = fileInfoFolder[_currentTrackIndex];
            return _currentTrack.FullName;
        }

        private void PlayPrevTrack()
        {
            string prevTrackPath = GetPrevTrackPath();

            if (!string.IsNullOrEmpty(prevTrackPath))
            {
                if (_currentTrackIndex == -1)
                    _currentTrack = fileInfoFolder.Last();
                else
                    _currentTrack = fileInfoFolder[_currentTrackIndex];
                _player.Open(new Uri(prevTrackPath));
                _player.Play();
                FilesLV.SelectedIndex = _currentTrackIndex;
                ReadMP3File(prevTrackPath);
            }
        }

        private string GetPrevTrackPath()
        {
            if (fileInfoFolder.Count == 0) return null;

            if (FilesLV.SelectedItem != null)
            {
                var selectedItem = FilesLV.SelectedItem;

                foreach (var f in fileInfoFolder)
                {
                    if (f.Name == selectedItem.ToString())
                    {
                        _currentTrackIndex = fileInfoFolder.IndexOf(f) - 1;
                    }
                }

                FilesLV.SelectedItem = null;
            }
            else
            {
                _currentTrackIndex = (_currentTrackIndex - 1 + fileInfoFolder.Count) % fileInfoFolder.Count;
            }

            if (_currentTrackIndex == -1)
                _currentTrack = fileInfoFolder.Last();
            else
                _currentTrack = fileInfoFolder[_currentTrackIndex];
            return _currentTrack.FullName;
        }


        private void OpenFileMI_Click(object sender, RoutedEventArgs e)
        {
            triggerOpen = true;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                ReadMP3File(openFileDialog.FileName);
                _player.Open(new Uri(openFileDialog.FileName));
            }
            else
                triggerOpen = false;
        }

        private void OpenFolderMI_Click(object sender, RoutedEventArgs e)
        {
            triggerOpen = true;

            FolderBrowserDialog fileDialog = new FolderBrowserDialog();

            fileDialog.ShowDialog();

            var filename = fileDialog.SelectedPath;

            if (filename == "")
            {
                triggerOpen = false;
                return;
            }

            fileInfoFolder.Clear();
            Playlist.Clear();

            DirectoryInfo dirInfo = new DirectoryInfo(filename);

            FileInfo[] fileInfo = dirInfo.GetFiles("*.mp3");
            foreach (FileInfo f in fileInfo)
            {
                fileInfoFolder.Add(f);
            }

            FilesLV.Items.Clear();

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



        private void OpenFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileMI_Click(sender, e);
        }

        private void OpenFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenFolderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFolderMI_Click(sender, e);
        }

        private void OpenFolderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
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
                    _currentTrack = f;
                    _player.Open(new Uri(f.FullName));
                    ReadMP3File(f.FullName);
                }
            }
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayNextTrack();
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayPrevTrack();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimazieBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
