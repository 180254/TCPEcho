using System;

namespace Kis
{
    /*
     * Nazwa: ClientStateMsg
     * Opis: Klasa, będąca wrappere, na typ ClientState.
     * Opis: Przechowuje dodatkowo informację o wiadomości wysłanej do danego klienta.
     * Opis: Została dodana, by nie zgubić ten informacji pomiędzy wywołaniem asynchronicznym wysłania, a jego callbackiem.
     * Autor: Adrian Pędziwiatr
     */

    public class ClientStateMsg
    {
        public readonly ClientState Client;
        public readonly byte[] SentBuffer;
        public readonly string SentData;
        /*
         * Nazwa: ClientStateMsg (kontruktor)
         * Opis: Kontruuje obiekt na podstawie clienta, i wysłane informacji.
         * Argumenty: client - klient, do którego wiadomość będzie wysłana
         * Argumenty: echoData - tekst, który wysłał klient
         * Aegumenty: read - ilość wysłanych bajtów
         * Zwraca: nie dotyczy
         * Używa: brak
         * Modyfikuje: Client, SendData, SentBuffer
         * Autor: Adrian Pędziwiatr
         */

        public ClientStateMsg(ClientState client, string echoData, int read)
        {
            Client = client;
            SentBuffer = new byte[read];
            Array.Copy(client.ReadBuffer, SentBuffer, SentBuffer.Length);
            SentData = echoData;
        }

        /*
         * Nazwa: ClientStateMsg (kontruktor)
         * Opis:  Kontruuje obiekt na podstawie clienta, i wysłane informacji.
         * Argumenty: client - klient, do którego wiadomość będzie wysłana.
         * Argumenty: msgBytes - wysyłane bajty
         * Argumenty: msg - przesyłana wiadomość w formie klasy string
         * Zwraca: nie dotyczy 
         * Używa: brak
         * Modyfikuje: Client, SendData, SentBuffer
         * Autor: Adrian Pędziwiatr
         */

        public ClientStateMsg(ClientState client, byte[] msgBytes, string msg)
        {
            Client = client;
            SentBuffer = msgBytes;
            SentData = msg;
        }
    }
}