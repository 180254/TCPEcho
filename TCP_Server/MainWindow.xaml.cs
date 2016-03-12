using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kis
{
    /*
     * Nazwa: MainWindow
     * Opis: Klasa reprezentująca okno główne programu, odpowiedzi na zdarzenia z niego pochodzące.
     * Autor: Adrian Pędziwiatr
     */

    public partial class MainWindow
    {
        private readonly ObservableCollection<EnhancedNetworkInterface> interfaceBoxElements =
            new ObservableCollection<EnhancedNetworkInterface>();

        private readonly TcpServer tcpServer;
        /*
         * Nazwa: MainWindow (konstruktor).
         * Opis: Konstruktor inicjalizuje obiekt TcpServer, do którego będzie zlecał zadania sieciowe,  oraz ustawia stan okna na "rozłączony".
         * Opis: Ponadto wypełnia listę interfejsów i ustawia źródło danych listy klientów.
         * Argumenty: nie dotyczy
         * Zwraca: nie dotyczy
         * Używa: brak
         * Modyfikuje: tcpServer 
         * Autor: Adrian Pędziwiatr
         */

        public MainWindow()
        {
            tcpServer = new TcpServer(this);
            InitializeComponent();
            SetWindowStateAsStopped();
            InitNetworkInterfaceBox();
            InitClientList();
        }

        /*
         * Nazwa: SetWindowStateAsStarted
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada rozpoczęciu działania programu.
         * Opis: Po starcie programu nie jest jeszcze zainicjalizowana lista intefejsów sieciowych.
         * Opis: Jest to więcej inny stan niż w momencie, gdy serwer jest zastopowany.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowStateAsStarted()
        {
            InterfaceBox.IsEnabled = false;
            PortBox.IsEnabled = false;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            DisconnectClientButton.IsEnabled = false;
        }

        /*
         * Nazwa: SetWindowWstateAsAwaiting
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada działaniu serwera..
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowWstateAsAwaiting()
        {
            InterfaceBox.IsEnabled = false;
            PortBox.IsEnabled = false;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            DisconnectClientButton.IsEnabled = false;
        }

        /*
         * Nazwa: SetWindowStateAsStopped
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada braku nasłuchu.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowStateAsStopped()
        {
            UpdateListOfNetworkInterfaces();

            InterfaceBox.IsEnabled = true;
            PortBox.IsEnabled = true;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            DisconnectClientButton.IsEnabled = false;
        }

        /*
         * Nazwa: SetClientIsSelected
         * Opis: Ustawia stan kontrolek na oknie na odpowiedni, dla sytuacji gdy na liście klientów został wskazany rekord.
         * Opis: Uaktywniany jest przycisk rozłączenia klienta, oraz możliwość wysłania do niego wiadomości.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetClientIsSelected()
        {
            MessageBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            DisconnectClientButton.IsEnabled = true;
        }

        /*
         * Nazwa: SetClientIsNotSelected
         * Opis: Ustawia stan kontrolek na oknie na odpowiedni, dla sytuacji gdy na liście klientów został nie ma zaznaczonego rekordu.
         * Opis: Deaktywowany jest przycisk rozłączenia klienta, oraz możliwość wysłania do niego wiadomości.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetClientIsNotSelected()
        {
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            DisconnectClientButton.IsEnabled = false;
        }

        /*
         * Nazwa: InitNetworkInterfaceBox
         * Opis: Inicjalizuje listę interfejsów sieciowych.
         * Opis: Kontrolka InterfaceBox wskazuje teraz na stosowną listę.
         * Argumenty: brak
         * Zwraca: void
         * Używa: interfaceBoxElements - lista elementów sieciowych
         * Modyfikuje: kontrolkę InterfaceBox, jej zasób ItemsSource
         * Autor: Adrian Pędziwiatr
         */

        private void InitNetworkInterfaceBox()
        {
            InterfaceBox.Items.Clear();
            InterfaceBox.ItemsSource = interfaceBoxElements;
        }

        /*
         * Nazwa: UpdateListOfNetworkInterfaces
         * Opis: Odświeża listę intefejsów sieciowych.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Zasób listowy, i stan kontrolki InterfaceBox. 
         * Autor: Adrian Pędziwiatr
         */

        private void UpdateListOfNetworkInterfaces()
        {
            interfaceBoxElements.Clear();
            foreach (EnhancedNetworkInterface network in TcpServer.GetAllNetworkInterfacesAsEnhanced())
            {
                interfaceBoxElements.Add(network);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() => { InterfaceBox.SelectedIndex = 0; }));
        }

        /*
         * Nazwa: InitClientList
         * Opis: Ustawia źródło danych listy klientów wskazując na listę oferowaną przez tcpServer.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje:  Zasób listowy ConnectedClientBox. 
         * Autor: Adrian Pędziwiatr
         */

        private void InitClientList()
        {
            ConnectedClientBox.ItemsSource = tcpServer.Clients;
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

        private static string GetCurrentTimeAsString()
        {
            return "(" + DateTime.Now.ToString("H':'mm':'ss'.'ffffff") + ")";
        }

        /*
         * Nazwa: AppendClientMsg
         * Opis: Funkcja dododanie do kontrolki z logiem serwera wiadomości od klienta.
         * Opis: Informacja zostaje dodana na końcu.
         * Argumenty: msgType - typ wiadomości (przychodząca, wychodząca)
         * Argumenty: client - klient, którego wiadomość dotyczy
         * Argumenty: msg - dodawana wiadomość
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Zawartość kontrolki ServerLogBox.
         * Autor: Adrian Pędziwiatr
         */

        private void AppendClientMsg(MsgType msgType, ClientState client, string msg)
        {
            string newText = LimitTextTo2048(ServerLogBox.Text +
                                             GetCurrentTimeAsString() +
                                             (msgType == MsgType.Rcved ? " MsgFrom" : " MsgTo") +
                                             "(" + client.ClientId + "): " + msg + "\n");
            ServerLogBox.Text = newText;
            ServerLogBox.ScrollToEnd();
        }

        /*
         * Nazwa: AppendServerState
         * Opis: Funkcja dododanie do kontrolki z logiem serwera informację o stanie serwera.
         * Opis: Informacja zostaje dodana na końcu.
         * Argumenty: msg - dodawana treść stanu
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Zawartość kontrolki ServerLogBox.
         * Autor: Adrian Pędziwiatr
         */

        private void AppendServerState(string msg)
        {
            string newText = LimitTextTo2048(ServerLogBox.Text + GetCurrentTimeAsString() + " Log: " + msg + "\n");
            ServerLogBox.Text = newText;
            ServerLogBox.ScrollToEnd();
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

        private static string LimitTextTo2048(string text)
        {
            const int lenLimit = 2048;
            return (text.Length <= lenLimit) ? text : text.Substring(text.Length - lenLimit, lenLimit);
        }

        /*
         * Nazwa: StartButton_OnClick
         * Opis: Funkcja wywoływana po kliknięciu przyciku "Start".
         * Opis: Funkcja rozpoczyna próbę rozpoczęcia nasłuchu. Zleca to zadanie przy użyciu obiektu TcpServer.
         * Opis: Jeżeli wpisane dane nie pozwalają na rozpoczęcie  natychmniastowo loguje taką informację.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: Zawartości kontrolek z wybranych interfejsem sieciowym, i hostem. 
         * Modyfikuje: Zawartość kontrolki z logiem.
         * Autor: Adrian Pędziwiatr
         */

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowWstateAsAwaiting();

            try
            {
                EnhancedNetworkInterface network = ((EnhancedNetworkInterface) InterfaceBox.SelectedItem);
                int port = Int32.Parse(PortBox.Text);
                tcpServer.AsyncConnect(network, port);
            }
            catch (FormatException)
            {
                AppendServerState("Connection Error. Written port is not proper whole number.");
                SetWindowStateAsStopped();
            }
        }

        /*
         * Nazwa: PortBox_OnKeyDown
         * Opis: Funkcja wywoływana po każdorazowym kliknięciu klawisza w polu z numerem portu.
         * Opis: W praktyce wykonuje jakiekolwiek zadania jedynie jeżeli klawiszem tym był enter.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "start".
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void PortBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                StartButton_OnClick(null, null);
            }
        }

        /*
         * Nazwa: StopButton_OnClick
         * Opis: Funkcja wywoływana po kliknięciu przycisku stop realizującego żadanie końca nasłuchu.
         * Opis: Ustawia stan okna na oczekujący. Zleca rozłączenie z serwerem obiektowi TcpServer.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: obiekt TcpServer
         * Modyfikuje: Stan kontrolek w oknie.
         * Autor: Adrian Pędziwiatr
         */

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            tcpServer.AsyncUnbind();
        }

        /*
         * Nazwa: MainWindow_OnClosed
         * Opis: Funkcja wywoływana przy zamykaniu okna serwera.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "stop".
         * Opis: Jeżeli zostało zamknięte okno serwera najpierw poprawnie kończy nasłuchiwanie.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: nie dotyczy - "symuluje inne zdarzenie"
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            StopButton_OnClick(null, null);
        }

        /*
         * Nazwa: MainWindow_OnKeyDown
         * Opis: Funkcja wywoływana każdorazowo po kliknięciu przycisku na oknie serwera.
         * Opis: Realizuje wymaganie, że klawisz ESC ma zamykać serwer. Jest to jedyny klawisz, który powoduje jakąś akcję.
         * Opis: W praktyce wykonuje jakiekolwiek zadanie tożsame z zamknięciem okna przez klawisz "X".
         * Opis: Serwer kończy nasłuch, a następnie okno jest zamykane.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            StopButton_OnClick(null, null);
            Close();
        }

        /*
         * Nazwa: SendButton_OnClick
         * Opis: Funkcja wywoływania po kliknięciu klawisza "send" realizującego żadanie wysłania tekstu do klienta.
         * Opis: zlecenia wysłanie tekstu obiektowi TcpServer
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: obiekt TcpServer
         * Modyfikuje: Stan kontrolek w oknie - kasuje pole do wpisywania tekstu.
         * Autor: Adrian Pędziwiatr
         */

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            tcpServer.AsyncSendData((ClientState) ConnectedClientBox.SelectedItem, MessageBox.Text);
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
         * Nazwa: ConnectedClientBox_OnSelected
         * Opis: Funkcja wywoływana po każdorazowym wyborze klienta na liście klientów.
         * Opis: Ustawia odpowiedni stan kontrolek w zależności czy aktualnie jest wybrany jakiś klient.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: kontrolki ConnectedClientBox
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void ConnectedClientBox_OnSelected(object sender, RoutedEventArgs e)
        {
            if (ConnectedClientBox.SelectedIndex != -1)
                SetClientIsSelected();
            else
                SetClientIsNotSelected();
        }

        /*
         * Nazwa: DisconnectClientButton_OnClick
         * Opis: Funkcja wywoływana po każdorazowym kliknięciu przycisku "disconnect client", oznaczający rozłączenie od serwera wskazanego klienta.
         * Opis: Zadanie to jest zlecane obiektowi typu TcpServer.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void 
         * Używa: obiekt TcpServer, kontrolka ConnectedClientBox
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private void DisconnectClientButton_OnClick(object sender, RoutedEventArgs e)
        {
            tcpServer.DisconnectClient((ClientState) ConnectedClientBox.SelectedItem);
        }

        /*
         * Nazwa: MsgBindSuccess
         * Opis: Funkcja, która realiuje zadania odpowiednie dla poprawnego ustanowienia nasłuchu.
         * Opis: Dopisuje informacje do loga, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna. 
         * Autor: Adrian Pędziwiatr
         */

        public void MsgBindSuccess(string host, int port)
        {
            Dispatcher.Invoke(() =>
            {
                AppendServerState("Successfully started. Listening " + host + ":" + port + ".");
                SetWindowStateAsStarted();
            });
        }

        /*
         * Nazwa: MsgBindError
         * Opis: Funkcja, która realiuje zadania odpowiednie dla braku realizacji nasłuchu.
         * Opis: Dopisuje informacje do loga - powód błedu, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgBindError(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                AppendServerState("Connection error. " + msg);
                SetWindowStateAsStopped();
            });
        }

        /*
         * Nazwa: MsgBindError
         * Opis: Funkcja, która realiuje zadania odpowiednie dla braku realizacji nasłuchu.
         * Opis: Jest ona przeznaczona dla przypadku specyficznego - wybrany interfejs sieciowy nie jest podłączony.
         * Opis: Dopisuje informacje do loga - powód błedu, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgBindErrorNetworkIsDown()
        {
            Dispatcher.Invoke(() =>
            {
                AppendServerState("Connection error. Selected network interface is down.");
                SetWindowStateAsStopped();
            });
        }

        /*
         * Nazwa: MsgUnbind
         * Opis: Funkcja, która realiuje zadania odpowiednie dla zakończenia nasłuchu na życzenie użytkownika.
         * Opis: Dopisuje informacje do loga, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgUnbind()
        {
            Dispatcher.Invoke(() =>
            {
                AppendServerState("Successfully stopped.");
                SetWindowStateAsStopped();
            });
        }

        /*
         * Nazwa: MsgReceived
         * Opis: Funkcja, która realiuje zadania odpowiednie dla odbioru danych od klienta.
         * Opis: Dopisuje odebrany tekst do stosownej kontrolki, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: client - klient, który wysłał tekst, msg - odebrany tekst
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgReceived(ClientState client, string msg)
        {
            Dispatcher.Invoke(() => AppendClientMsg(MsgType.Rcved, client, msg));
        }

        /*
          * Nazwa: MsgSent
          * Opis: Funkcja, która realiuje zadania odpowiednie dla potwierdzenia wysłania danych do serwera.
          * Opis: Dopisuje wysłany tekst do stosownej kontrolki, ustawia odpowiedni stan okna.
          * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
          * Argumenty: client - klient, do którego została wysłana wiadomość, msg - wysłany tekst
          * Zwraca: void
          * Używa: brak
          * Modyfikuje: Stan kontrolek okna.
          * Autor: Adrian Pędziwiatr
          */

        public void MsgSent(ClientState client, string msg)
        {
            Dispatcher.Invoke(() => AppendClientMsg(MsgType.Sent, client, msg));
        }

        /*
         * Nazwa: MsgClientConnected
         * Opis: Funkcje realizuje zadanie odpowiednie dla poinformowania o połączonym nowym kliencie.
         * Opis: Dopisuje wysłany tekst do stosownej kontrolki, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: client - klient, który nawiązał połączenie
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgClientConnected(ClientState client)
        {
            Dispatcher.Invoke(() => AppendServerState("Client connected: " + client.ClientId));
        }

        /*
         * Nazwa: MsgClientDisconnected
         * Opis: Funkcje realizuje zadanie odpowiednie dla poinformowania o zerwaniu połączenia do klienta.
         * Opis: Dopisuje wysłany tekst do stosownej kontrolki, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: client - klient, który utracił połączenie
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgClientDisconnected(ClientState client)
        {
            Dispatcher.Invoke(() => AppendServerState("Client disconnected: " + client.ClientId));
        }

        /*
         * Nazwa: MsgClientDisconnected
         * Opis: Funkcje realizuje zadanie odpowiednie dla poinformowania o braku nawiązania połączenia z klientem z powodu limitu liczby połączeń.
         * Opis: Dopisuje wysłany tekst do stosownej kontrolki, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: client - klient, który nie został polączony
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgClientLimit(ClientState client)
        {
            Dispatcher.Invoke(
                () => AppendServerState("Client connected && disconnected: " + client.ClientId + " (client limit)"));
        }

        /*
         * Nazwa: MsgType
         * Opis: Enum wyróżniający typy wiaodmości.
         * Opis: Wyróżnia się: wiadomość otrzymana od klienta, wiadomość wysłana do klienta.
         * Opis: Zastosowane w celu wyeliminowania magic numbers.
         * Argumenty: nie dotyczy
         * Zwraca: nie dotyczy
         * Używa: nie dotyczy
         * Modyfikuje: nie dotyczy
         * Autor: Adrian Pędziwiatr
         */

        private enum MsgType
        {
            Rcved,
            Sent
        };
    }
}