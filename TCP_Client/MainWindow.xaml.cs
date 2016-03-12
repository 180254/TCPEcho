using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kis
{
    /*
     * Nazwa: MainWindow
     * Opis: Klasa reprezentująca okno główne programu, odpowiedzi na zdarzenia z niego pochodzące.
     * Autor: Adrian Pędziwiatr
     */

    public partial class MainWindow
    {
        private readonly TcpClient tcpEcho;
        /*
         * Nazwa: MainWindow (konstruktor).
         * Opis: Konstruktor inicjalizuje obiekt TcpClient, do którego będzie zlecał zadania sieciowe,
         * Opis: oraz ustawia stan okna na "rozłączony".
         * Argumenty: nie dotyczy
         * Zwraca: nie dotyczy
         * Używa: nic
         * Modyfikuje: tcpEcho 
         * Autor: Adrian Pędziwiatr
         */

        public MainWindow()
        {
            tcpEcho = new TcpClient(this);
            InitializeComponent();
            SetWindowStateAsDisconnected();
        }

        /*
         * Nazwa: SetWindowStateAsDisconnected
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada braku połączenia z serwerem.
         * Argumenty: brak
         * Zwraca: void
         * Używa: nic
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowStateAsDisconnected()
        {
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            IpBox.IsEnabled = true;
            PortBox.IsEnabled = true;
            IpBox.Focus();
        }

        /*
         * Nazwa: SetWindowWstateAsAwaiting
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada oczekiwaniu na połączenie do serwera.
         * Argumenty: brak
         * Zwraca: void
         * Używa: nic
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowWstateAsAwaiting()
        {
            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = false;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            IpBox.IsEnabled = false;
            PortBox.IsEnabled = false;
        }

        /*
         * Nazwa: SetWindowStateAsConnected
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada istniejącemu połączeniu z serwerem.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowStateAsConnected()
        {
            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
            MessageBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            IpBox.IsEnabled = false;
            PortBox.IsEnabled = false;
        }

        /*
         * Nazwa: LimitTextTo1024
         * Opis: Funkcja pomocnicza, który obcina tekst do 1024 znaków.
         * Opis: Zakładamy, że nie istnieje potrzeba wyświetlać naraz więcej informacji,
         * Opis: a doświadczanie zauważyłem, że zbyt duża ilośc informacji naraz przychodzącej,
         * Opis: znacząco przywiesza okno.
         * Argumenty: text - tekst do przycięcia
         * Zwraca: string - przycięty tekst
         * Używa: nie dotyczy (static)
         * Modyfikuje: nie dotyczy (static) 
         * Autor: Adrian Pędziwiatr
         */

        public static string LimitTextTo1024(string text)
        {
            const int lenLimit = 1024;
            return (text.Length <= lenLimit) ? text : text.Substring(text.Length - lenLimit, lenLimit);
        }

        /*
         * Nazwa: GetCurrentTimeAsString
         * Opis: Funkcja pomocnicza zwracająca aktualną datę w formie stringa.
         * Argumenty: brak
         * Zwraca: string - aktualna data, wraz z czasem
         * Używa: nie dotyczy (static) 
         * Modyfikuje: nie dotyczy (static) 
         * Autor: Adrian Pędziwiatr
         */

        public static string GetCurrentTimeAsString()
        {
            return "(" + DateTime.Now.ToString("H':'mm':'ss'.'ffffff") + ")";
        }

        /*
         * Nazwa: AppendToBox
         * Opis: Funkcja dododanie do wskazanej kontrolki tekstowej tekst.
         * Opis: Informacja zostaje dodana na końcu.
         * Argumenty: box - kontrolka tekstowa do modyfikacji, data - nowy tekst.
         * Zwraca: void
         * Używa: nie dotyczy (static) 
         * Modyfikuje: wskazaną przez parametr box kontrolkę tekstową
         * Autor: Adrian Pędziwiatr
         */

        private static void AppendToBox(TextBox box, string data)
        {
            box.Text = LimitTextTo1024(box.Text + GetCurrentTimeAsString() + " " + data + "\n");
            box.ScrollToEnd();
        }

        /*
         * Nazwa: AppendToBox
         * Opis: Przeciążenie do funkcji AppendToBox.
         * Opis: Funkcja ta przekazuje do niej informację, że dane mają być dodane do pola z logiem klienta.
         * Argumenty: data - tekst do dodania, isError - informacja, czy tekst jest treścią błędy
         * Zwraca: void
         * Używa: nie dotyczy (static) 
         * Modyfikuje: Kontrolkę z logiem klienta..
         * Autor: Adrian Pędziwiatr
         */

        public void AppendToLogBox(string data, bool isError)
        {
            AppendToBox(LogBox, (isError ? "Connection error: " : "") + data);
        }

        /*
         * Nazwa: AppendToLogBox
         * Opis: Przeciążenie do funkcji AppendToBox.
         * Opis: Funkcja ta przekazuje do niej informację, że dane mają być dodane do pola z danymi wysyłanymi.
         * Argumenty: data - tekst do dodania
         * Zwraca: void
         * Używa: nie dotyczy (static) 
         * Modyfikuje: Kontrolkę z wysyłanymi danymi.
         * Autor: Adrian Pędziwiatr
         */

        public void AppendToSentDataBox(string data)
        {
            AppendToBox(SentDataBox, data);
        }

        /*
         * Nazwa: AppendToRcvDataBox
         * Opis: Przeciążenie do funkcji AppendToBox.
         * Opis: Funkcja ta przekazuje do niej informację, że dane mają być dodane do pola z danymi odebranymi.
         * Argumenty: data - tekst do dodania
         * Zwraca: void
         * Używa: nie dotyczy (static) 
         * Modyfikuje: Kontrolkę z odebranymi danymi.
         * Autor: Adrian Pędziwiatr
         */

        public void AppendToRcvDataBox(string data)
        {
            AppendToBox(RcvDataNpx, data);
        }

        /*
         * Nazwa: ConnectButton_OnClick
         * Opis: Funkcja wywoływana po kliknięciu przyciku "Connect".
         * Opis: Funkcja rozpoczyna próbę połączenia z serwerem. Zleca to zadanie przy użyciu obiektu TcpClient.
         * Opis: Jeżeli wpisane dane nie pozwalają na rozpoczęcie połączenia natychmniastowo loguje taką informację.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: Zawartości kontrolek z adresem ip, i hostem. 
         * Modyfikuje: Zawartość kontrolki z logiem klienta.
         * Autor: Adrian Pędziwiatr
         */

        public void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowWstateAsAwaiting();

            try
            {
                string host = IpBox.Text;
                int port = Int32.Parse(PortBox.Text);
                tcpEcho.AsyncConnect(host, port);
            }
            catch (FormatException)
            {
                AppendToLogBox("Written port is not proper whole number.", true);
                SetWindowStateAsDisconnected();
            }
        }

        /*
         * Nazwa: IpBox_OnKeyDown
         * Opis: Funkcja wywoływana po każdorazowym kliknięciu klawisza w polu z adresem IP.
         * Opis: W praktyce wykonuje jakiekolwiek zadania jedynie jeżeli klawiszem tym był enter.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "connect".
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void IpBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ConnectButton_OnClick(null, null);
            }
        }

        /*
         * Nazwa: PortBox_OnKeyDown
         * Opis: Funkcja wywoływana po każdorazowym kliknięciu klawisza w polu z numerem portu.
         * Opis: W praktyce wykonuje jakiekolwiek zadania jedynie jeżeli klawiszem tym był enter.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "connect".
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void PortBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            IpBox_OnKeyDown(null, e);
        }

        /*
          * Nazwa: SendButton_OnClick
          * Opis: Funkcja wywoływania po kliknięciu klawisza "send" realizującego żadanie wysłania tekstu do serwera.
          * Opis: zlecenia wysłanie tekstu obiektowi TcpClient
          * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
          * Zwraca: void
          * Używa: obiekt TcpClient
          * Modyfikuje: Stan kontrolek w oknie - kasuje pole do wpisywania tekstu.
          * Autor: Adrian Pędziwiatr
          */

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            tcpEcho.AsyncSendData(MessageBox.Text);
            MessageBox.Text = "";
        }

        /*
         * Nazwa: MessageBox_OnKeyDown
         * Opis: Funkcja wywoływana po każdorazowym kliknięciu klawisza w polu z wysyłanym tekstem.
         * Opis: W praktyce wykonuje jakiekolwiek zadania jedynie jeżeli klawiszem tym był enter.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "send".
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void MessageBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendButton_OnClick(null, null);
            }
        }

        /*
         * Nazwa: DisconnectButton_OnClick
         * Opis: Funkcja wywoływana po kliknięciu przycisku disconnect realizującego żadanie rozłączenia się od serwera.
         * Opis: Ustawia stan okna na oczekujący. Zleca rozłączenie z serwerem obiektowi TcpClient
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: obiekt TcpClient
         * Modyfikuje: Stan kontrolek w oknie.
         * Autor: Adrian Pędziwiatr
         */

        private void DisconnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowWstateAsAwaiting();
            tcpEcho.AsyncDisconnect();
        }

        /*
         * Nazwa: MainWindow_OnClosed
         * Opis: Funkcja wywoływana przy zamykaniu okna klienta.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "disconnect".
         * Opis: Jeżeli zostało zamknięte okno klient najpierw poprawnie rozłącza się od serwera
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: nie dotyczy - "symuluje inne zdarzenie"
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            DisconnectButton_OnClick(null, null);
        }

        /*
         * Nazwa: MsgConnectSuccess
         * Opis: Funkcja, która realiuje zadania odpowiednie dla poprawnego połączenia z serwerem.
         * Opis: Dopisuje informacje do loga, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpClient jako informacja o stanie klienta.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna. 
         * Autor: Adrian Pędziwiatr
         */

        public void MsgConnectSuccess(string host, int ip)
        {
            Dispatcher.Invoke(() =>
            {
                AppendToLogBox("Successfully connected to given server (" + host + ":" + ip + ").", false);
                SetWindowStateAsConnected();
                MessageBox.Focus();
            });
        }

        /*
         * Nazwa: MsgConnectError
         * Opis: Funkcja, która realiuje zadania odpowiednie dla braku realizacji połączenia z serwerem.
         * Opis: Dopisuje informacje do loga - powód błedu, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpClient jako informacja o stanie klienta.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgConnectError(string errorMsg)
        {
            Dispatcher.Invoke(() =>
            {
                AppendToLogBox(errorMsg, true);
                SetWindowStateAsDisconnected();
            });
        }

        /*
         * Nazwa: MsgDisconnected
         * Opis: Funkcja, która realiuje zadania odpowiednie dla rozłączenia się z serwem na życzenie klienta.
         * Opis: Dopisuje informacje do loga, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpClient jako informacja o stanie klienta.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgDisconnected()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    AppendToLogBox("Connection closed as requested.", false);
                    SetWindowStateAsDisconnected();
                    ConnectButton.Focus();
                }
                catch (TaskCanceledException)
                {
                    // may appear if window was closed
                }
            });
        }

        /*
         * Nazwa: MsgDisconnectedByServer
         * Opis: Funkcja, która realiuje zadania odpowiednie dla rozłączenia się z serwem na żadanie serwera.
         * Opis: Dopisuje informacje do loga, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpClient jako informacja o stanie klienta.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgDisconnectedByServer()
        {
            Dispatcher.Invoke(() =>
            {
                AppendToLogBox("Server has closed connection.", false);
                SetWindowStateAsDisconnected();
                ConnectButton.Focus();
            });
        }

        /*
         * Nazwa: MsgDisconnectedServerStoppedWorking
         * Opis: Funkcja, która realiuje zadania odpowiednie dla nagłej utraty połączenia z serwerem.
         * Opis: Dopisuje informacje do loga, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpClient jako informacja o stanie klienta.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgDisconnectedServerStoppedWorking()
        {
            Dispatcher.Invoke(() =>
            {
                AppendToLogBox("Connection has been lost suddenly.", false);
                SetWindowStateAsDisconnected();
                ConnectButton.Focus();
            });
        }

        /*
         * Nazwa: MsgDataSentSuccess
         * Opis: Funkcja, która realiuje zadania odpowiednie dla potwierdzenia wysłania danych do serwera.
         * Opis: Dopisuje wysłany tekst do stosownej kontrolki, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpClient jako informacja o stanie klienta.
         * Argumenty: data - wysłany tekst
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgDataSentSuccess(string data)
        {
            Dispatcher.Invoke(() => AppendToSentDataBox(data));
        }

        /*
         * Nazwa: MsgReceived
         * Opis: Funkcja, która realiuje zadania odpowiednie dla odbioru danych od serwera.
         * Opis: Dopisuje odebrany tekst do stosownej kontrolki, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpClient jako informacja o stanie klienta.
         * Argumenty: data - odebrany tekst
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgReceived(string data)
        {
            try
            {
                Dispatcher.Invoke(() => AppendToRcvDataBox(data));
            }
            catch (TaskCanceledException)
            {
                // may be cancelled. if closed connection by close program, but server sent msg
            }
        }
    }
}