using System.Net;
using System.Net.Sockets;

namespace Kis
{
    /*
     * Nazwa: ClientState
     * Opis: Klasa reprezentująca stan klienta.
     * Opis: Przechowuje jego adres IP, uchwyt indywidualnego socket, buffor wiadomości otrzymywanych.
     * Autor: Adrian Pędziwiatr
     */

    public class ClientState
    {
        public const int BuffersSize = 4096;
        public readonly string ClientId;
        public readonly Socket Handler;
        public readonly byte[] ReadBuffer = new byte[BuffersSize];
        public readonly IPEndPoint RemoteEndPoint;
        public bool Disconnected = false;
        /*
         * Nazwa: ClientState (kontruktor)
         * Opis: Tworzy obiekt stanu klienta.
         * Argumenty: handler - socket handler tworzonego klienta, zwrócony przez funkcję akceptacji połączenia.
         * Zwraca: nie dotyczy
         * Używa: brak
         * Modyfikuje: Handler, RemoteEndPoint, ClientId
         * Autor: Adrian Pędziwiatr
         */

        public ClientState(Socket handler)
        {
            Handler = handler;
            RemoteEndPoint = (IPEndPoint) Handler.RemoteEndPoint;
            ClientId = RemoteEndPoint.Address + ":" + RemoteEndPoint.Port;
        }

        /*
         * Nazwa: ToString
         * Opis: Przeciążenie funkcji ToString, które ma zapewnić czytelną nazwę klienta na liście klientów.
         * Opis: Opisem klienta jest jego id, na który składa się jego adres ip, i port połączenia.
         * Argumenty: brak
         * Zwraca: ClientId - czytelny identyfikator klienta
         * Używa: brak
         * Modyfikuje: brak 
         * Autor: Adrian Pędziwiatr
         */

        public override string ToString()
        {
            return ClientId;
        }
    }
}