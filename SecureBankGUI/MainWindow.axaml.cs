using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using SecureBankGUI.Clients;

namespace SecureBankGUI;

public partial class MainWindow : Window
{
    private BankingClient _bankingClient = new BankingClient();
    private ChatClient _chatClient = new ChatClient();
    private RatesClient _ratesClient = new RatesClient();

    private string _serverIP = "127.0.0.1";
    private int _tcpPort = 8000;
    private int _udpPort = 8001;

    private int _totalDeposited = 0;
    private int _totalWithdrawn = 0;

    private TextBlock _lblStatus = null!;
    private Button _btnConnect = null!;
    private TextBlock _lblBalance = null!;
    private TextBlock _lblDeposited = null!;
    private TextBlock _lblWithdrawn = null!;
    private TextBox _txtAmount = null!;
    private Button _btnDeposit = null!;
    private Button _btnWithdraw = null!;
    private TextBox _rtbBankLog = null!;
    private TextBox _rtbChatLog = null!;
    private TextBox _txtChatMessage = null!;
    private Button _btnSendChat = null!;
    private Button _btnRefresh = null!;
    private TextBlock _lblUSD = null!;
    private TextBlock _lblEUR = null!;
    private TextBlock _lblGBP = null!;
    private TextBlock _lblSAR = null!;
    private TextBlock _lblLastUpdated = null!;

    public MainWindow()
    {
        InitializeComponent();

        _lblStatus      = this.FindControl<TextBlock>("lblStatus")!;
        _btnConnect     = this.FindControl<Button>("btnConnect")!;
        _lblBalance     = this.FindControl<TextBlock>("lblBalance")!;
        _lblDeposited   = this.FindControl<TextBlock>("lblDeposited")!;
        _lblWithdrawn   = this.FindControl<TextBlock>("lblWithdrawn")!;
        _txtAmount      = this.FindControl<TextBox>("txtAmount")!;
        _btnDeposit     = this.FindControl<Button>("btnDeposit")!;
        _btnWithdraw    = this.FindControl<Button>("btnWithdraw")!;
        _rtbBankLog     = this.FindControl<TextBox>("rtbBankLog")!;
        _rtbChatLog     = this.FindControl<TextBox>("rtbChatLog")!;
        _txtChatMessage = this.FindControl<TextBox>("txtChatMessage")!;
        _btnSendChat    = this.FindControl<Button>("btnSendChat")!;
        _btnRefresh     = this.FindControl<Button>("btnRefresh")!;
        _lblUSD         = this.FindControl<TextBlock>("lblUSD")!;
        _lblEUR         = this.FindControl<TextBlock>("lblEUR")!;
        _lblGBP         = this.FindControl<TextBlock>("lblGBP")!;
        _lblSAR         = this.FindControl<TextBlock>("lblSAR")!;
        _lblLastUpdated = this.FindControl<TextBlock>("lblLastUpdated")!;
    }

    private async void BtnConnect_Click(object sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_btnConnect.Content?.ToString() == "Connect")
        {
            SetStatus("Connecting...", "#E08000");

            string result = await Task.Run(() =>
                _bankingClient.Connect(_serverIP, _tcpPort)
            );

            if (result.StartsWith("ERROR"))
            {
                SetStatus("● Connection failed", "#CC0000");
            }
            else
            {
                SetStatus("● Connected to " + _serverIP, "#3B6D11");
                _btnConnect.Content = "Disconnect";
                // Show only first line of welcome message
                string firstLine = result.Split('\n')[0];
                AppendBankLog("System", firstLine);
                // Set initial balance
                int wIdx = firstLine.LastIndexOf("$");
                if (wIdx >= 0)
                {
                    string afterW = firstLine.Substring(wIdx + 1);
                    string numOnly = "";
                    foreach (char c in afterW)
                    {
                        if (char.IsDigit(c) || c == ',')
                            numOnly += c;
                        else
                            break;
                    }
                    if (numOnly.Length > 0)
                        _lblBalance.Text = "$" + numOnly;
                }
                // Set initial balance from welcome message

            }
        }
        else
        {
            _bankingClient.Disconnect();
            _chatClient.Disconnect();
            SetStatus("● Not connected", "Gray");
            _btnConnect.Content = "Connect";
        }
    }

    private async void BtnDeposit_Click(object sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        string amount = _txtAmount.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(amount)) return;

        _btnDeposit.IsEnabled = false;

        string reply = await Task.Run(() =>
            _bankingClient.SendCommand("DEPOSIT:" + amount)
        );

        _btnDeposit.IsEnabled = true;
        HandleBankingReply(reply, "DEPOSIT", amount);
        _txtAmount.Text = "";
    }

    private async void BtnWithdraw_Click(object sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        string amount = _txtAmount.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(amount)) return;

        _btnWithdraw.IsEnabled = false;

        string reply = await Task.Run(() =>
            _bankingClient.SendCommand("WITHDRAW:" + amount)
        );

        _btnWithdraw.IsEnabled = true;
        HandleBankingReply(reply, "WITHDRAW", amount);
        _txtAmount.Text = "";
    }

    private void HandleBankingReply(string reply, string action, string amount)
    {
        if (reply.StartsWith("SUCCESS") || reply.StartsWith("INFO"))
        {
            int dollarIndex = reply.LastIndexOf("$");
            if (dollarIndex >= 0)
                _lblBalance.Text = reply.Substring(dollarIndex);

            if (action == "DEPOSIT")
            {
                _totalDeposited += int.TryParse(amount, out int d) ? d : 0;
                _lblDeposited.Text = "$" + _totalDeposited;
            }
            else
            {
                _totalWithdrawn += int.TryParse(amount, out int w) ? w : 0;
                _lblWithdrawn.Text = "$" + _totalWithdrawn;
            }
            AppendBankLog(action, reply);
        }
        else
        {
            AppendBankLog("Error", reply);
        }
    }

    private void TxtChat_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            SendChatMessage();
    }

    private void BtnSendChat_Click(object sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        SendChatMessage();
    }

    private async void SendChatMessage()
    {
        string message = _txtChatMessage.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(message)) return;

        if (!_chatClient.IsConnected)
        {
            string welcome = await Task.Run(() =>
                _chatClient.Connect(_serverIP, _tcpPort)
            );
            if (welcome.StartsWith("ERROR"))
            {
                AppendChatLog("System", welcome);
                return;
            }
            AppendChatLog("Bot", welcome);
        }

        AppendChatLog("You", message);
        _txtChatMessage.Text = "";
        _btnSendChat.IsEnabled = false;

        string reply = await Task.Run(() =>
            _chatClient.SendMessage(message)
        );

        _btnSendChat.IsEnabled = true;
        AppendChatLog("Bot", reply);
    }

    private async void BtnRefresh_Click(object sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        _btnRefresh.IsEnabled = false;
        _btnRefresh.Content = "Fetching...";

        string result = await Task.Run(() =>
            _ratesClient.GetRates(_serverIP, _udpPort)
        );

        _btnRefresh.IsEnabled = true;
        _btnRefresh.Content = "Refresh Rates";

        if (result.StartsWith("ERROR"))
        {
            _lblLastUpdated.Text = "Error: " + result;
            return;
        }

        foreach (string line in result.Split('\n'))
        {
            if (line.StartsWith("USD:"))
                _lblUSD.Text = line.Replace("USD:", "").Trim();
            else if (line.StartsWith("EUR:"))
                _lblEUR.Text = line.Replace("EUR:", "").Trim();
            else if (line.StartsWith("GBP:"))
                _lblGBP.Text = line.Replace("GBP:", "").Trim();
            else if (line.StartsWith("SAR:"))
                _lblSAR.Text = line.Replace("SAR:", "").Trim();
        }

        _lblLastUpdated.Text = "Last updated: " +
            DateTime.Now.ToString("hh:mm:ss tt");
    }

    private void AppendBankLog(string type, string message)
    {
        _rtbBankLog.Text += DateTime.Now.ToString("hh:mm:ss") +
                            " [" + type + "] " + message + "\n";
    }

    private void AppendChatLog(string sender, string message)
    {
        _rtbChatLog.Text += sender + "  " +
                            DateTime.Now.ToString("hh:mm tt") + "\n" +
                            message + "\n\n";
    }

    private void SetStatus(string text, string color)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _lblStatus.Text = text;
            _lblStatus.Foreground = Avalonia.Media.Brush.Parse(color);
        });
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _bankingClient.Disconnect();
        _chatClient.Disconnect();
        base.OnClosing(e);
    }
}
