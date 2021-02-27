using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace WebViewWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        async void InitializeAsync()
        {

            await WebView.EnsureCoreWebView2Async(null);
            WebView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
window.chrome.webview.addEventListener('message', event => {
  if (event.data === 'copy') {
    var results = [...document.querySelectorAll('[data-bi-name=""result""]')].map(a => {
            let aElement = a.querySelector('a');
            return {
                title: aElement.innerText,
                link: aElement.getAttribute('href')
            };
        });
    if (results.length >= 1){
      window.chrome.webview.postMessage(results);
    }
    else {
      alert('There are no results in the page');
    }
  }
});");
            WebView.CoreWebView2.WebMessageReceived += DataReceived;
        }

        void DataReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var data = args.WebMessageAsJson;
            Clipboard.SetText(data);
            MessageBox.Show("Results copied to the clipboard");
        }
  

        private async void CoreWebView2_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {

            await WebView.ExecuteScriptAsync(
                @"
window.onload = () => {
  var rss = document.querySelector('[data-bi-name=""search-rss-link""]');
  console.log(rss);
  if (rss)
    rss.style.display = 'none'; 
  var form = document.getElementById('facet-search-form');
  console.log(form);
  if (form)
    form.style.display = 'none';  
  var container = document.getElementById('left-container');
  console.log(container);
  if (container)
    container.style.display = 'none';  
  var hiddenClasses = ['header-holder', 'footerContainer'];
  var divs = document.getElementsByTagName('div');
  for( var i = 0; i < divs.length; i++) {
    if (hiddenClasses.some(r=> divs[i].classList.contains(r))){
      divs[i].style.display = 'none';
    }
  }
}");
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchText.Text))
                return;
            var searchAddress =
                $"https://docs.microsoft.com/en-us/search/?terms={HttpUtility.UrlEncode(SearchText.Text)}";
            WebView?.CoreWebView2?.Navigate(searchAddress);
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            if (WebView.CanGoBack)
                WebView.GoBack();
        }

        private void GoForwardClick(object sender, RoutedEventArgs e)
        {
            if (WebView.CanGoForward)
                WebView.GoForward();
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            WebView?.CoreWebView2?.PostWebMessageAsString("copy");
        }
    }
}
