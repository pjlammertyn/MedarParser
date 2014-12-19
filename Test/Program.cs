using System;
using MedarParser;
using Newtonsoft.Json;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var medarParser = new Parser();

            var text = @"/FROM    : Callens Francis| ||||1/01217/19/930|RXRIJ|Radiologie Rijselstraat
/TO      : VANDERHAEGHE KARLEEN|MENENSTRAAT 62 |8940|WERVIK|059/303938|1/33194/84/004
/SUBJECT : KATRIEN HELEEN|POLLET|Bergstraat 62 |8940|WERVIK|13/02/1945|F|198059|5702136044|5702136044
/INFO    : created on 12/15/2014|MDR4846441
 
/TITLE Radiologie Rijselstraat
/DATE 18/12/2014
  
/EXAM Radiologie Rijselstraat
/DESCR
Geachte collega,

Op 08/12/2014 werd(en) na akkoord van uw patiënt(e), 
Pollet Katrien Heleen, geboren op 13/02/1957, volgende 
onderzoek(en) 
uitgevoerd op onze afdeling :

RX MAMMOGRAFIE RECHTS EN ECHOGRAFIE BILATERAAL.

Klinische inlichtingen
 mastectomie links in 2000

Bevindingen
Mammografie rechts:
Gevorderde involutie van parenchymweefsel met voornamelijk nog 
klierweefsel in bovenste buitenste kwadrant.
Ongewijzigd aspect met de opnamen welke dateren van mei 2012. Geen 
verdachte stellaire densiteiten of clusters van microcalcificaties.

Echografie 
Aanwezigheid van borstprothese aan de linkerzijde. Intact 
voorkomen.
Echografisch ook vrij voorkomen van de linker axilla.
In rechterborst aanwezigheid van klein cystje van 4 tot 5 mm rechts 
op 10 uur. Geen verdachte kenmerken. Verder normaal voorkomen van 
het residuele klierweefsel. Vrij voorkomen van de axilla.

/CONCL
BESLUIT
Mastectomie links. Borstprothese links met intacte voorkomen.
Mammografisch en echografisch geen verdachte kenmerken rechts.
Klein inliggend cystje rechts op 10 uur geen verdachte kenmerken.
Vrij voorkomen van de axillae.

Tweede lezer: Dr. F. Lambert.


Met collegiale groeten,



Dr. Callens Francis




/END
   
";

            var letters = medarParser.ParseLetter(text);
            foreach (var parserError in medarParser.ParserErrors)
                Console.WriteLine("error on line {0}: {1}", parserError.Key, string.Join(Environment.NewLine, parserError.Value)); 
            Console.WriteLine("Press enter to view result"); 
            Console.ReadLine();
            var json = JsonConvert.SerializeObject(letters, Formatting.Indented);
            Console.WriteLine(json);
            Console.ReadLine();
        }
    }
}
