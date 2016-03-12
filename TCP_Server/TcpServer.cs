using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Kis
{
    /*
     * Nazwa: TcpServer
     * Opis: Klasa reprezentująca serwera TCP. Realizuje połączenia sieciowe, i przekazuje stan do okna MainWindow.
     * Autor: Adrian Pędziwiatr
     */


    internal class TcpServer
    {
        private const int ClientConnectionsLimit = 2;
        private const string ClientConnectionLimitMsg = "ERROR_TOO_MANY_CLIENTS";
        private readonly byte[] clientConnectionLimitMsgBytes = Encoding.UTF8.GetBytes(ClientConnectionLimitMsg);
        public readonly BindingList<ClientState> Clients = new BindingList<ClientState>();
        private readonly MainWindow window;
        private Socket socket;
        /*
         * Nazwa: TcpServer (konstruktor)
         * Opis: Konstruktor ustawia referencję do głównego okna, i wstępnie inicjalizuje socket.
         * Argumenty: window - referencja do głównego okna
         * Zwraca: nie dotyczy
         * Używa: brak
         * Modyfikuje: window, socket
         * Autor: Adrian Pędziwiatr
         */

        public TcpServer(MainWindow window)
        {
            this.window = window;
            InitializeSocket();
        }

        /*
        * Nazwa: InitializeSocket
        * Opis: Inicjalizuje obiekt typu Socket wstawiająć nową referencję.
        * Argumenty: brak
        * Zwraca: void
        * Używa: klasy Socket
        * Modyfikuje: socket
        * Autor: Adrian Pędziwiatr
        */

        private void InitializeSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /*
         * Nazwa: GetAllNetworkInterfacesAsEnhanced
         * Opis: Funkcja przygotowująca listę interfejsów sieciowych w formie możliwiej do wylisowania na kontrolce select.
         * Argumenty: brak
         * Zwraca: EnhancedNetworkInterface[] - tablica interfejsów
         * Używa: klasy NetworkInterface i jej metody statycznej GetAllNetworkInterfaces.
         * Używa: Zwrócone interfejsy zostają opakowane własną klasą EnhancedNetworkInterface
         * Modyfikuje nie dotyczy (static) 
         * Autor: Adrian Pędziwiatr
         */

        public static EnhancedNetworkInterface[] GetAllNetworkInterfacesAsEnhanced()
        {
            List<EnhancedNetworkInterface> networkInterfacesUdpsList = new List<EnhancedNetworkInterface>
            {
                EnhancedNetworkInterface.GetDefaultInstance()
            };

            networkInterfacesUdpsList.AddRange(from networkInterface in NetworkInterface.GetAllNetworkInterfaces()
                where networkInterface.OperationalStatus == OperationalStatus.Up
                select new EnhancedNetworkInterface(networkInterface));

            networkInterfacesUdpsList.RemoveAll(i => i.IpAddress == null);

            return networkInterfacesUdpsList.ToArray();
        }

        /*
         * Nazwa: AsyncConnect
         * Opis: Funkcja inicjalizująca asynchroniczne ropoczęcie nasłuchu serwera.
         * Argumenty: EnhancedNetworkInterface - interfejs na którym powinien serwer nasłuchiwać 
         * Argumenty: port - nr portu na którym serwer ma nasłuchiwać
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        public void AsyncConnect(EnhancedNetworkInterface enhancedNetworkInterface, int port)
        {
            lock (socket)
            {
                InitializeSocket();

                if (enhancedNetworkInterface.NetworkInterface == null ||
                    enhancedNetworkInterface.NetworkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    try
                    {
                        EndPoint endPoint = new IPEndPoint(enhancedNetworkInterface.IpAddress, port);
                        socket.Bind(endPoint);
                        socket.Listen(port);
                        socket.BeginAccept(AcceptCallback, null);
                        window.MsgBindSuccess(enhancedNetworkInterface.IpAddress.ToString(), port);
                    }
                    catch (SocketException ex)
                    {
                        window.MsgBindError(ex.Message);
                    }
                }
                else
                {
                    window.MsgBindErrorNetworkIsDown();
                }
            }
        }

        /*
         * Nazwa: AsyncConnectCallback
         * Opis: Callback dla funkcji AsyncConnect. Wywołana po zrealizowaniu rozpoczęcia nasłuchu.
         * Opis: Informuje okno główne o stanie serwera.
         * Opis: W przypadku sukcesu rozpoczyna odbieranie danych od klientów.
         * Opis: W przypadku niepowodzenia przekazuje do okna głównego komunikat błędu.
         * Opis: W przypadku przekroczonej ilości klientów wysyła stosowny komunikat i kończy połączenie
         * Argumenty: IAsyncResult ar - ten argument zawiera stan próby połączenia.
         * Zwraca: void
         * Używa: socket, window
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        public void AcceptCallback(IAsyncResult ar)
        {
            lock (socket)
            {
                Socket handler;
                try
                {
                    handler = socket.EndAccept(ar);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                ClientState client = new ClientState(handler);

                if (Clients.Count != ClientConnectionsLimit)
                {
                    handler.BeginReceive(client.ReadBuffer, 0, ClientState.BuffersSize, 0, AsyncReceiveCallback, client);
                    window.Dispatcher.Invoke(() => Clients.Add(client));
                    window.MsgClientConnected(client);
                }
                else
                {
                    AsyncSendClientConnectionLimitThenDisconnect(client);
                }

                socket.BeginAccept(AcceptCallback, null);
            }
        }

        /*
         * Nazwa: AsyncReceiveCallback
         * Opis: Callback wykonywany, gdy serwer otrzyma dane od klienta.
         * Opis: Zostaje wykonana także w przypadku zerwania połączenia z klientem.
         * Opis: Przekazuje do okna głównego informację jaki tekst został otrzymany.
         * Opis: Rozpoczyna proces odsyłania (echo) wiadomości do klienta.
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego. 
         * Zwraca: void
         * Używa: socket, readBuffer
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncReceiveCallback(IAsyncResult ar)
        {
            ClientState client = (ClientState) ar.AsyncState;

            lock (client.Handler)
            {
                int read;
                try
                {
                    read = client.Handler.EndReceive(ar);
                }
                catch (SocketException)
                {
                    read = 0;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                if (read > 0)
                {
                    string rcvedText = Encoding.UTF8.GetString(client.ReadBuffer, 0, read);
                    ClientStateMsg clientEcho = new ClientStateMsg(client, rcvedText, read);

                    client.Handler.BeginReceive(client.ReadBuffer, 0, ClientState.BuffersSize, 0, AsyncReceiveCallback,
                        client);
                    client.Handler.BeginSend(clientEcho.SentBuffer, 0, clientEcho.SentBuffer.Length, 0,
                        AsyncSendDataEchoCallback, clientEcho);

                    window.MsgReceived(client, rcvedText);
                }
                else
                {
                    DisconnectClient(client, false);
                }
            }
        }

        /*
         * Nazwa: AsyncSendDataEchoCallback
         * Opis: Funkcja wysolywana w przypadku zakończenia odsyłania wiadomości (echo) do klienta.
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego. 
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncSendDataEchoCallback(IAsyncResult ar)
        {
            ClientStateMsg clientEcho = (ClientStateMsg) ar.AsyncState;
            lock (clientEcho.Client.Handler)
            {
                clientEcho.Client.Handler.EndSend(ar);
                window.MsgSent(clientEcho.Client, clientEcho.SentData);
            }
        }

        /*
         * Nazwa: AsyncSendClientConnectionLimitThenDisconnect
         * Opis: Funkcja wysoływana w przypadku połączenia klienta, który nie może zostać przyjęty ze względu na limit połączeń.
         * Opis: Funkcja rozpoczyna asynchroniczne wysłanie stosownej wiadomości błędu.
         * Argumenty: client - pechowy klient
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncSendClientConnectionLimitThenDisconnect(ClientState client)
        {
            lock (client.Handler)
            {
                client.Handler.BeginSend(clientConnectionLimitMsgBytes, 0, clientConnectionLimitMsgBytes.Length, 0,
                    AsyncSendClientConnectionLimitThenDisconnectCallback, client);
            }
        }

        /*
         * Nazwa: AsyncSendClientConnectionLimitThenDisconnectCallback
         * Opis: Funkcja wywoływana jako callback dla AsyncSendClientConnectionLimitThenDisconnect.
         * Opis: Jej wywołanie oznacza zakończenie wysyłania wiadomości błędu.
         * Opis: Funkcja kończy wysyłanie, oraz zamyka połączenie z klientem, który nie może zostać obsłuzony.
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego. 
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncSendClientConnectionLimitThenDisconnectCallback(IAsyncResult ar)
        {
            ClientState client = (ClientState) ar.AsyncState;
            lock (client.Handler)
            {
                client.Handler.EndSend(ar);
            }

            DisconnectClient(client, true);
            window.MsgSent(client, ClientConnectionLimitMsg);
        }

        /*
         * Nazwa: AsyncSendData
         * Opis: Funkcja rozpoczyna wysyłanie do klienta danych w formie tekstowej.
         * Argumenty: client - klient, który powinien otrzymać dane
         * Argumenty: data - string do wysłania.
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: nie
         * Autor: Adrian Pędziwiatr
         */

        public void AsyncSendData(ClientState client, string msg)
        {
            if (msg.Length == 0)
            {
                return;
            }

            byte[] dataBytes = Encoding.UTF8.GetBytes(msg);
            ClientStateMsg clientMsg = new ClientStateMsg(client, dataBytes, msg);
            lock (client.Handler)
            {
                client.Handler.BeginSend(dataBytes, 0, dataBytes.Length, 0, AsyncSendDataCallback, clientMsg);
            }
        }

        /*
         * Nazwa: AsyncSendDataCallback
         * Opis: Callback dla funkcji AsyncSendData. Wywołana zostanie po zrealizowaniu wysyłania.
         * Opis: informuje okno główne, że tekst został wysłany.
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego. Zawiera między innymi informację jaki tekst został wysłany,
         * Argumenty: i do którego klienta, gdyż informacja ta została przekazana jako parametr z funkcji wywołującej..
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncSendDataCallback(IAsyncResult ar)
        {
            ClientStateMsg clientMsg = (ClientStateMsg) ar.AsyncState;
            lock (clientMsg.Client.Handler)
            {
                clientMsg.Client.Handler.EndSend(ar);
            }

            window.MsgReceived(clientMsg.Client, clientMsg.SentData);
        }

        /*
         * Nazwa: DisconnectClient
         * Opis: Funkcja rozłaczająca sesję wskazanego klienta.
         * Argumenty: client - klient, które połączenie zostanie zakończone.
         * Argumenty: cousedByClientLimit - informacja czy zerwanie połączenia nastąpiło z powodu przepełnienia limitu klientów.
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: brak 
         * Autor: Adrian Pędziwiatr
         */

        private void DisconnectClient(ClientState client, bool cousedByClientLimit)
        {
            // if (client.Disconnected) return;
            lock (client.Handler)
            {
                client.Disconnected = true;
                client.Handler.Shutdown(SocketShutdown.Both);
                client.Handler.Close();
                window.Dispatcher.Invoke(() => Clients.Remove(client));

                if (!cousedByClientLimit)
                    window.MsgClientDisconnected(client);
                else
                    window.MsgClientLimit(client);
            }
        }

        /*
         * Nazwa: DisconnectClient
         * Opis: Funkcja rozłaczająca sesję wskazanego klienta.
         * Opis: Przeciążenie dwuargumentowej wersji tej funkcji która przyjmuje, że rozłączenie klienta nie nastąpiło z powodu limitu połączeń.
         * Argumenty: client - client, którego sesja ma być zakończona
         * Zwraca: void
         * Używa: nie dotyczy
         * Modyfikuje: nie dotyczy
         * Autor: Adrian Pędziwiatr
         */

        public void DisconnectClient(ClientState client)
        {
            DisconnectClient(client, false);
        }

        /*
         * Nazwa: AsyncUnbind
         * Opis: Funkcja kończąca nasłuch serwera. Przez zamknięciem serwera wszyscy klienci są rozłączani.
         * Argumenty: brak
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        public void AsyncUnbind()
        {
            lock (socket)
            {
                while (Clients.Count > 0)
                {
                    DisconnectClient(Clients[0], false);
                }

                socket.Close();
                window.MsgUnbind();
            }
        }
    }
}