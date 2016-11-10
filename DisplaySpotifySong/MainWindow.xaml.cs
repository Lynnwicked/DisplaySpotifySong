using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using MessageBox = System.Windows.MessageBox;

namespace DisplaySpotifySong {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public delegate void ElementsSavedEvent(object sender, EventArgs e);

    private SpotifyLocalAPI _spotify;
    private Storyboard _sb1;
    private Storyboard _sb2;

    private readonly PictureBox _albumArt = new PictureBox() {
      SizeMode = PictureBoxSizeMode.Zoom
    };

    private bool _isBusy;

    public MainWindow() {
      InitializeComponent();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
      AlbumArt.Child = _albumArt;

      _spotify = new SpotifyLocalAPI {
        ListenForEvents = true
      };

      if (!_spotify.Connect()) {
        var error = MessageBox.Show(this, "Spotify connection could not be established,\ndo you want to reconnect?", "Connection Error D:", MessageBoxButton.YesNo);
        if (error == MessageBoxResult.Yes) {
          MainWindow_OnLoaded(sender, e);
        }
        else {
          Environment.Exit(0);
        }
      }

      _sb1 = (Storyboard) TryFindResource("Storyboard1");
      _sb2 = (Storyboard) TryFindResource("Storyboard2");

      SetElements(_spotify.GetStatus().Track);

      _spotify.OnTrackChange += (s, args) => {
        Dispatcher.BeginInvoke(new Action(() => {
          _sb1.Completed += (s1, e1) => {
            SetElements(_spotify.GetStatus().Track);
          };

          _sb1.Begin();
        }));
      };
    }

    private void SetElements(Track track) {
      if (_isBusy) {
        return;
      }

      Bitmap albumArt;
      Task.Run(() => {
        _isBusy = true;

        albumArt = track.GetAlbumArt(AlbumArtSize.Size160);

        _isBusy = false;

        Dispatcher.BeginInvoke(new Action(() => {
          _albumArt.Image = albumArt;

          SongName.Text = track.TrackResource.Name;
          ArtistName.Text = track.ArtistResource.Name;
          AlbumName.Text = track.AlbumResource.Name;

          _sb2.Begin();
        }));
      });
    }
  }
}