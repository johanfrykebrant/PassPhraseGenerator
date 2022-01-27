using System.IO;
using System;

namespace Generator
{
    public class Generator
    {
        private List<string> SPEC_CHARS = new List<string>() { "!", "#", "%", "&", "/", "(", ")", "=", "?", " ", "<", ">", ":", ";", "-", "_", "*", "§" };
        private List<string> NUMBERS = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        private string CAPITAL_LETTERS = "FIRST"; //FIRST, LAST, NONE
        private string CHAR_POSITION = "BETWEEN"; //BETWEEN RANDOM, NONE
        public int NBR_SPEC_CHAR = 1;
        public int NBR_NUMBERS = 1;
        public int NBR_WORDS = 10;
        public int MIN_CHAR_NBR = 12;
        public int MAX_CHAR_NBR = 20;
        private string WORD_FILE = "word_list.csv";
        private List<string> WORD_LIST = new List<string>();

        // Load word list from csv and save it in a list
        public void Setup() {
            string path = Directory.GetCurrentDirectory();
            string wordfile = Path.Combine(path, WORD_FILE);

            using (var reader = new StreamReader(@wordfile)) {
                    //List<string> WORD_LIST = new List<string>();
                    while (!reader.EndOfStream) {
                        var line = reader.ReadLine();
                        WORD_LIST.Add(line);
                    }
                }
        }

        //Check so that all public variables are reasonable
        private void Variable_check()
        {
            if (MIN_CHAR_NBR >= MAX_CHAR_NBR)
            {
                throw new ArgumentException(String.Format("MIN_CHAR_NBR must be less than MAX_CHAR_NBR. MIN_CHAR_NBR = {0}, MAX_CHAR_NB = {1}", MIN_CHAR_NBR, MAX_CHAR_NBR));
            }

        }

        // Returns random word from WORD_LIST
        private string Get_word()
        {
            var rand = new Random();
            int index = rand.Next(WORD_LIST.Count);
            string word = WORD_LIST[index];
            //WORD_LIST.Remove(WORD_LIST[index]);
            return word;
        }

        // Remove largest word in list
        private List<string> Remove_longest_word(List<string> words)
        {
            int max = 0;
            string longest_word = "";

            foreach (string word in words)
            {
                if (word.Length > max)
                {
                    longest_word = word;
                    max = word.Length;
                }
            }

            words.Remove(longest_word);
            return words;
        }

        // Returns the total amount of letters in a list of strings
        private int Count_letters(List<string> words) {
            int sum = 0;
            foreach (string word in words)
            {
                sum = sum + word.Length;
            }
            return sum;
        }

        // Returns a list of words for the passphrase
        private List<string> Get_phrase_words()
        {
            List<string> passphrase = new List<string>();
            while (passphrase.Count < NBR_WORDS | Count_letters(passphrase) < (MIN_CHAR_NBR - NBR_NUMBERS - NBR_SPEC_CHAR)) { 
                passphrase.Add(Get_word());
                
                if (Count_letters(passphrase) > (MAX_CHAR_NBR - NBR_NUMBERS - NBR_SPEC_CHAR))
                {
                    Remove_longest_word(passphrase);
                }
            }
            return passphrase;
        }

        // Return a list of special characters and numbers, in random order, to be added to the passphrase
        private List<string> Get_phrase_chars(){
            var rand = new Random();
            List<string> chars = new List<string>();
            int nbrs = 0;
            int chrs = 0;
            bool NBR_OR_CHAR = rand.NextDouble() > 0.5; // Generate random boolean
            while (nbrs < NBR_NUMBERS)
            {
                // Add a number to the list of characters
                int index = rand.Next(NUMBERS.Count);
                string c = NUMBERS[index];
                chars.Add(c);
                nbrs++;
            }
            while (chrs < NBR_SPEC_CHAR)
            {
                // Add a number to the list of characters
                int index = rand.Next(SPEC_CHARS.Count);
                string c = SPEC_CHARS[index];
                chars.Add(c);
                chrs++;
            }
            // Shuffle list
            List<string> chars_random = chars.OrderBy(a => rand.Next()).ToList();
            //print_list(chars_random);
            return chars_random;
        }

        // Generates and returns passphrase according to specifications given by 
        public string Generate_passphrase(){
            Variable_check();
            List<string> words = Set_capital_letters( Get_phrase_words() );
            List<string> chars = Get_phrase_chars();
            string passphrase = "";
            
            if (CHAR_POSITION == "RANDOM")
            {
                passphrase = Insert_char_random(words, chars);
            }
            else if (CHAR_POSITION == "BETWEEN")
            {
                passphrase = Insert_char_between(words, chars);
            }
            else
            {
                throw new ArgumentException(String.Format("CHAR_POSITION variable not assigned correctly. Should be BETWEEN or RANDOM, is {0}", CHAR_POSITION));
            }


            return passphrase;
        }

        // Format words in passphrase to be capitalized according to specifications given by CAPITAL_LETTERS
        private List<string> Set_capital_letters(List<string> words)
        {
            List<string> capitalized_words = new List<string>();
            if (CAPITAL_LETTERS == "FIRST"){
                foreach (string word in words){
                    string capitalized = char.ToUpper(word[0]) + word.Substring(1);
                    capitalized_words.Add(capitalized);
                }
            }
            else if (CAPITAL_LETTERS == "LAST"){
                foreach (string word in words){
                    string capitalized = word.Substring(0, (word.Length - 1)) + word.Substring(word.Length - 1).ToUpper();
                    capitalized_words.Add(capitalized);
                }
            }

            return capitalized_words;
        }

        // Insert spacial character or number between every word in passphrase
        private string Insert_char_between(List<string> words, List<string> chars)
        {
            // Check so there is room for all the chars to be placed out between the words
            int nbr_chars = chars.Count;
            int nbr_words = words.Count;
            string passphrase = "";
            
            var rand = new Random();
            var spaces = Enumerable.Range(0, (nbr_words + nbr_chars)).ToList();
            List<int> positions = new List<int>();
                
            // Decide on what possitions the spacial characters and numbers will be
            for (int i = 0; i < nbr_chars; i++)
            {
                int index = rand.Next(spaces.Count);
                positions.Add(spaces[index]);
                spaces.Remove(spaces[index]);
            }
                
            // Append words, spec. chars and numbers to passphrase
            for (int i = 0; i < (nbr_words + nbr_chars); i++)
            {
                if (positions.Contains(i))
                {
                    //Append char and remove it from chars
                    passphrase = passphrase + chars.First();
                    chars.RemoveAt(0);
                }else
                {
                    //Append word and remove it from words
                    passphrase = passphrase + words.First();
                    words.RemoveAt(0);
                }            
            }
            return passphrase;
        }

        // Insert spacial character or number randomly in passphrase
        private string Insert_char_random(List<string> words, List<string> chars)
        {
            string passphrase = "";
            var rand = new Random();

            for (int i = 0;i < words.Count; i++) 
            {
                passphrase = passphrase + words[i];
            }
            for (int i = 0; i< chars.Count; i++)
            {
                int index = rand.Next(passphrase.Length);
                passphrase = passphrase.Insert(index, chars[i]);
            }
            return passphrase;
        }

        // Public functions to use outside of the class when configuring variables
        public void Cap_letters_set_none()
        {
            CAPITAL_LETTERS = "NONE";
        }
        public void Cap_letters_set_first()
        {
            CAPITAL_LETTERS = "FIRST";
        }
        public void Cap_letters_set_last()
        {
            CAPITAL_LETTERS = "LAST";
        }
        public void char_pos_set_random()
        {
            CHAR_POSITION = "RANDOM";
        }
        public void char_pos_set_between()
        {
            CHAR_POSITION = "BETWEEN";
        }
        public void char_pos_set_none()
        {
            CHAR_POSITION = "NONE";
        }

        // For debugging purposes
        private void print_list(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Console.Write(list[i]);
                Console.Write(" ");
            }
            Console.WriteLine();
            }

        private static void Main(string[] args)
        {
            Generator g = new Generator();
            g.Setup();
            for (int i = 0; i< 30;i ++)
            {
                string phrase = g.Generate_passphrase();
                Console.WriteLine(phrase + " ," + Convert.ToString(phrase.Length));
            }
            
        }
    }
}
