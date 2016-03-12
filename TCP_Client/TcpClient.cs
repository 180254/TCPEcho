using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Kis
{
    /*
     * Nazwa: TcpClient
     * Opis: Klasa reprezentująca klienta TCP. Realizuje połączenia sieciowe, i przekazuje stan do okna MainWindow.
     * Autor: Adrian Pędziwiatr
     */

    public class TcpClient
    {
        private const int ReadBufferSize = 4096;
        private readonly byte[] readBuffer = new byte[ReadBufferSize];
        private readonly object socketShouldBeConnectedLock = new object();
        private readonly MainWindow window;
        private Socket socket;
        private bool socketShouldBeConnected;
        /*
         * Nazwa: TcpClient (konstruktor)
         * Opis: Konstruktor ustawia referencję do głównego okna, i wstępnie inicjalizuje socket.
         * Argumenty: window - referencja do głównego okna
         * Zwraca: nie dotyczy
         * Używa: nic
         * Modyfikuje: window, socket
         * Autor: Adrian Pędziwiatr
         */

        public TcpClient(MainWindow window)
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
         * Nazwa: AsyncConnect
         * Opis: Funkcja inicjalizująca asynchroniczne połączenie do serwera.
         * Argumenty: host - nazwa domenowa lub ip serwera, port - nr portu na którym na nastapić połączenie
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: nic
         * Autor: Adrian Pędziwiatr
         */

        public void AsyncConnect(String host, int port)
        {
            lock (socket)
            {
                InitializeSocket();
                socket.BeginConnect(host, port, AsyncConnectCallback, null);
            }
        }

        /*
         * Nazwa: AsyncConnectCallback
         * Opis: Callback dla funkcji AsyncConnect. Wywołana po zrealizowaniu połączenia.
         * Opis: Informuje okno główne o wyniku połączenia.
         * Opis: W przypadku połączenia sukcesem rozpoczyna odbieranie danych od serwera.
         * Opis: W przypadku niepowodzenia przekazuje do okna głównego komunikat błędu.
         * Argumenty: IAsyncResult ar - ten argument zawiera stan próby połączenia.
         * Zwraca: void
         * Używa: socket, window
         * Modyfikuje: nic
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncConnectCallback(IAsyncResult ar)
        {
            try
            {
                lock (socket)
                {
                    lock (socketShouldBeConnectedLock)
                    {
                        socket.EndConnect(ar);
                        socket.BeginReceive(readBuffer, 0, readBuffer.Length, 0, AsyncReceiveDataCallback, null);

                        IPEndPoint remoteIpEndPoint = (IPEndPoint) socket.RemoteEndPoint;
                        string host = remoteIpEndPoint.Address.ToString();
                        int port = remoteIpEndPoint.Port;

                        socketShouldBeConnected = true;
                        window.MsgConnectSuccess(host, port);
                    }
                }
            }
            catch (SocketException ex)
            {
                window.MsgConnectError(ex.Message);
            }
        }

        /*
         * Nazwa: AsyncSendData
         * Opis: Funkcja rozpoczyna wysyłanie do serwera danych w formie tekstowej.
         * Argumenty: data - string do wysłania.
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: nie
         * Autor: Adrian Pędziwiatr
         */

        public void AsyncSendData(string data)
        {
            if (data.Length == 0)
            {
                return;
            }

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            lock (socket)
            {
                socket.BeginSend(dataBytes, 0, dataBytes.Length, 0, AsyncSendDataCallback, data);
            }
        }

        /*
         * Nazwa: AsyncSendDataCallback
         * Opis: Callback dla funkcji AsyncSendData. Wywołana zostanie po zrealizowaniu wysyłania.
         * Opis: informuje okno główne, że tekst został wysłany.
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego. Zawiera między innymi informację jaki tekst został wysłany,
         * Argumenty: gdyż informacja ta została przekazana jako parametr z funkcji wywołującej..
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: nic
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncSendDataCallback(IAsyncResult ar)
        {
            string data = (string) ar.AsyncState;
            lock (socket)
            {
                socket.EndSend(ar);
            }

            window.MsgDataSentSuccess(data);
        }

        /*
         * Nazwa: AsyncReceiveDataCallback
         * Opis: Callback wykonywany, gdy klient otrzyma dane od serwera.
         * Opis: Zostaje wykonana także w przypadku zerwania połączenia z serwerem.
         * Opis: Przekazuje do okna głównego informację jaki tekst został otrzymany.
         * Opis: Przekazuje do okna głównego informację o rozłączeniu się klienta wraz ze sposobem (poprawne rozłączenie lub nagłe).
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego. 
         * Zwraca: void
         * Używa: socket, readBuffer
         * Modyfikuje: nic
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncReceiveDataCallback(IAsyncResult ar)
        {
            lock (socket)
            {
                int read;
                bool disconnectedSuddenly = false;

                try
                {
                    read = socket.EndReceive(ar);
                }
                catch (SocketException)
                {
                    read = 0;
                    disconnectedSuddenly = true;
                }

                if (read > 0)
                {
                    string rcvedText = Encoding.UTF8.GetString(readBuffer, 0, read);

                    try
                    {
                        socket.BeginReceive(readBuffer, 0, readBuffer.Length, 0, AsyncReceiveDataCallback, null);
                    }
                    catch (SocketException)
                    {
                        // May me thrown if message was sent after socket clsoe request.
                    }

                    window.MsgReceived(rcvedText);
                }
                else
                {
                    AsyncDisconnect(disconnectedSuddenly != true
                        ? SocketDisconnectedBy.Server
                        : SocketDisconnectedBy.Suddenly);
                }
            }
        }

        /*
         * Nazwa: AsyncDisconnect
         * Opis: Funkcja rozpoczynająca asynchroniczne rozłączenie połączenia z serwer.
         * Argumenty: brak
         * Argumenty: Jest to przeciążenie funkcji argumentowej AsyncDisconnect(SocketDisconnectedBy).
         * Argumenty: Do funkcji przeciążenej przekazywana jest informacja, że połączenie zerwać chce klient.
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: brak 
         * Autor: Adrian Pędziwiatr
         */

        public void AsyncDisconnect()
        {
            AsyncDisconnect(SocketDisconnectedBy.Me);
        }

        /*
         * Nazwa: AsyncDisconnect
         * Opis: Funkcja rozpoczynająca asynchroniczne rozłączenie połączenia z serwer.
         * Opis: Funkcja realizuje rozłączenie
         * Argumenty: disconnectedBy - informacja kto zarządził rozłączenie - klient, serwer, czy też połączenie zostało zerwane nagle.
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: nic
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncDisconnect(SocketDisconnectedBy disconnectedBy)
        {
            lock (socket)
            {
                lock (socketShouldBeConnectedLock)
                {
                    if (socketShouldBeConnected)
                    {
                        socketShouldBeConnected = false;
                        socket.BeginDisconnect(true, AsyncDisconnectCallback, disconnectedBy);
                    }
                }
            }
        }

        /*
         * Nazwa: AsyncDisconnectCallback
         * Opis: Callback dla funkcji AsyncDisconnect.
         * Opis: Informuje okno główne o rozłączeniu, w sposób odpowiedni do powodu.
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego.
         * Argumenty: Z niego uzyskana jest między innymi informacja o powodzie rozłączenia,
         * Argumenty: gdyż została przekazana jako parametr z funkcji wywołującej.
         * Zwraca: void
         * Używa: socket
         * Modyfikuje: sc
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncDisconnectCallback(IAsyncResult ar)
        {
            lock (socket)
            {
                socket.EndDisconnect(ar);
            }

            SocketDisconnectedBy disconnectedBy = (SocketDisconnectedBy) ar.AsyncState;

            switch (disconnectedBy)
            {
                case (SocketDisconnectedBy.Me):
                    window.MsgDisconnected();
                    break;
                case (SocketDisconnectedBy.Server):
                    window.MsgDisconnectedByServer();
                    break;
                case (SocketDisconnectedBy.Suddenly):
                    window.MsgDisconnectedServerStoppedWorking();
                    break;
            }
        }

        /*
         * Nazwa: SocketDisconnectedBy
         * Opis: Enum wyróżniające powody rozłączenia się z serwerem.
         * Opis: Wyróżnia się: rozłączenie na żądanie klienta, na żadanie serwera, oraz nagłe.
         * Opis: Zastosowane w celu wyeliminowania magic numbers.
         * Argumenty: nie dotyczy
         * Zwraca: nie dotyczy
         * Używa: nie dotyczy
         * Modyfikuje: nie dotyczy
         * Autor: Adrian Pędziwiatr
         */

        private enum SocketDisconnectedBy
        {
            Me,
            Server,
            Suddenly
        };
    }
}