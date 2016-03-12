using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Kis
{
    /*
     * Nazwa: EnhancedNetworkInterface
     * Opis: Klasa jest wrapperem na typ NetworkInterface.
     * Opis: Jest w formie identyfikującej się czytelny sposób w kontrolce select.
     * Autor: Adrian Pędziwiatr
     */

    public class EnhancedNetworkInterface
    {
        public readonly IPAddress IpAddress;
        public readonly string Name;
        public readonly NetworkInterface NetworkInterface;
        /*
         * Nazwa: EnhancedNetworkInterface (konstruktor).
         * Opis: Konstruuje obiekt na podstawie instancji bazowej klasy NetworkInterface.
         * Opis: Dopisuje jego nazwę, a także znajduje adres ip tego interfejsu.
         * Argumenty: networkInterface - instancja interfejsu bazowego
         * Zwraca: nie dotyczy
         * Używa: brak
         * Modyfikuje: NetworkInterface, IpAddress, Name
         * Autor: Adrian Pędziwiatr
         */

        public EnhancedNetworkInterface(NetworkInterface networkInterface)
        {
            NetworkInterface = networkInterface;
            IpAddress = GetIpForNetworkInterface(networkInterface);
            Name = networkInterface.Name;
        }

        /*
         * Nazwa: EnhancedNetworkInterface (konstruktor).
         * Opis: Konstruuje obiekt domyślny wskazujący na dowolny interfejs sieciowy.
         * Opis: Taki przypadek zachodzi np. dla próby nasłuchu na wszystkich interfejsach.
         * Argumenty: brak
         * Zwraca: nie dotyczy
         * Używa: brak
         * Modyfikuje: IpAddress, Name
         * Autor: Adrian Pędziwiatr
         */

        private EnhancedNetworkInterface()
        {
            IpAddress = IPAddress.Any;
            Name = "ALL NETWORKS";
        }

        /*
         * Nazwa: GetDefaultInstance
         * Opis: Zwraca domyślny interfejs sieciowych.
         * Opis: Wykorzystuje prywarny kostruktor tworzący taki interfejs.
         * Argumenty: brak
         * Zwraca: brak
         * Używa: nie dotyczy - static
         * Modyfikuje: nie dotyczy - static
         * Autor: Adrian Pędziwiatr
         */

        public static EnhancedNetworkInterface GetDefaultInstance()
        {
            return new EnhancedNetworkInterface();
        }

        /*
         * Nazwa: ToString
         * Opis: Przeciążenie funkcji ToString, które ma zapewnić czytelną nazwę klienta na liście interfejsów sieciowych.
         * Opis: Opisem intefejsu jest jego nazwa.
         * Argumenty: brak
         * Zwraca: Name - czytelny identyfikator interfejsu sieciowego
         * Używa: brak
         * Modyfikuje: brak 
         * Autor: Adrian Pędziwiatr
         */

        public override string ToString()
        {
            return Name;
        }

        /*
         * Nazwa: GetIpForNetworkInterface
         * Opis: Znajduje adres ip dla interfejsu sieciowego.
         * Argumenty: networkInterface - interfejs sieciowy dla którego ma być znaleziony adres ip
         * Zwraca: IPAddress - adres ip
         * Używa: brak
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private static IPAddress GetIpForNetworkInterface(NetworkInterface networkInterface)
        {
            return (
                from ipAddressInformation in networkInterface.GetIPProperties().UnicastAddresses
                where ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork
                select ipAddressInformation.Address)
                .FirstOrDefault();
        }
    }
}