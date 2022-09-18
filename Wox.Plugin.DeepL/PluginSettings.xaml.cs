using System.Windows;
using System.Windows.Controls;

namespace Wox.Plugin.DeepL
{
    /// <summary>
    /// Interaction logic for PluginSettings.xaml
    /// </summary>
    public partial class PluginSettings : UserControl
    {
        private readonly Settings _settings;
        private readonly IDeepLApi _client;

        public PluginSettings(Settings settings, IDeepLApi client)
        {
            InitializeComponent();

            _settings = settings;
            _client = client;
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            AuthKey.Text = _settings.AuthKey;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            _settings.AuthKey = AuthKey.Text;

            if (!string.IsNullOrWhiteSpace(_settings.AuthKey))
            {
                _client.AuthKey = $"DeepL-Auth-Key {_settings.AuthKey}";
            }
        }
    }
}
