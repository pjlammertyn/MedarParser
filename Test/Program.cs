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

            text = @"0000181561412RS1181561412R\19580922VANA05
0001181561412RS2Vanhooren\Julles Antoinette
0002181561412RS3Vogelstraat 27\8800 ROESELARE
0003181561412RS422/09/1962\V
0004181561412RS5 \ \ \ \ 
0005181561412RS6\
0006181561412RA115/12/2014
0007181561412RD1Herreman\Luc
0008181561412RR1\\\\
0009181561412RR1\\\\Hematologie
0010181561412RR1WBC\leucocytose\4.20-9.80\x10³/mm³\  6.04
0011181561412RR1RBC\erythrocyten\3.75-5.11\x10E6/mm³\  5.11
0012181561412RR1Hb\hemoglobine\11.8-15.5\g/dL\  14.5
0013181561412RR1Hct\hematocriet\35.5-46.5\%\  42.4
0014181561412RR1MCV\MCV\84.0-98.3\fL\L 83.0
0015181561412RR1MCH\MCH\27.6-32.9\pg\  28.4
0016181561412RR1MCHC\MCHC\31.6-35.1\g/dL\  34.2
0017181561412RR1PLT\thrombocyten\162-351\x10³/mm³\H 401
0018181561412RR1\\\\Formule :
0019181561412RR1IMMATGRAN\immature granulocyten\0.1-0.8\%\  0.3
0020181561412RR1NEU\neutrofielen\39.6-71.1\%\  68.4
0021181561412RR1EOS\eosinofielen\0.4-5.0\%\  1.3
0022181561412RR1BASO\basofielen\0.3-1.4\%\  1.2
0023181561412RR1LYMFO\lymfocyten\18.5-46.7\%\  21.5
0024181561412RR1MONO\monocyten\4.6-11.5\%\  7.3
0025181561412RR1NORMOBL\normoblasten\\/100 WBC\  0.0
0026181561412RR1NEUT\neutrofilie (abs)\1.70-7.50\x10³/mm³\  4.13
0027181561412RR1LYMFOT\absoluut aantal lymfo's\1.15-3.25\x10³/mm³\  1.30
0028181561412RR1\\\\
0029181561412RR1\\\\Biochemie
0030181561412RR1CRP_U\CRP (mg/L)\0.0-7.0\mg/L\  4.6
0031181561412RR1SEDI_1\sedimentatie 1 uur\1-14\mm\H 18
END.
";
            var labo = medarParser.ParseLabo(text);
            foreach (var parserError in medarParser.ParserErrors)
                Console.WriteLine("error on line {0}: {1}", parserError.Key, string.Join(Environment.NewLine, parserError.Value)); 
            Console.WriteLine("Press enter to view result"); 
            Console.ReadLine();
            json = JsonConvert.SerializeObject(labo, Formatting.Indented);
            Console.WriteLine(json);
            Console.ReadLine();
        }
    }
}
