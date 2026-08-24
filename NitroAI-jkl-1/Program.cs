using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace jkl1;

class Program
{
    private const int MaxContextMessages = 8;

    static void Main(string[] args)
    {
        Console.Title = "NitroAI-JKL-1";
        Console.WriteLine("NitroAI-JKL-1");
        Console.WriteLine("Speak naturally — I can help with questions, explanations, ideas, text, and code.");
        Console.WriteLine("Type exit to close.");
        Console.WriteLine();

        var conversationHistory = new List<string>();

        while (true)
        {
            Console.Write("agent> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("NitroAI-JKL-1: Please enter a request.");
                continue;
            }

            var trimmedInput = input.Trim();

            if (string.Equals(trimmedInput, "exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("NitroAI-JKL-1: Goodbye!");
                Thread.Sleep(1500);
                break;
            }

            conversationHistory.Add($"User: {trimmedInput}");
            var reply = GenerateCodingAgentReply(trimmedInput, conversationHistory);
            var displayReply = FormatAgentOutput(trimmedInput, reply);
            conversationHistory.Add($"AI: {displayReply}");

            Console.WriteLine($"NitroAI-JKL-1: {displayReply}");
            Console.WriteLine();
        }
    }

    static string GenerateCodingAgentReply(string input, List<string> conversationHistory)
    {
        var userLanguage = DetectUserLanguage(input);
        if (userLanguage == "unknown")
        {
            return "I do not understand this language yet. Please write in Czech or English.";
        }

        var recentContext = conversationHistory
            .TakeLast(MaxContextMessages)
            .ToList();

        var previousUserMessage = recentContext
            .Where(item => item.StartsWith("User:", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Substring("User:".Length).Trim())
            .LastOrDefault();

        if (string.Equals(input, "ahoj", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(input, "hello", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(input, "hi", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(input, "hey", StringComparison.OrdinalIgnoreCase))
        {
            return userLanguage == "english"
                ? "Hello! I am NitroAI-JKL-1. I can help with questions, explanations, planning, text, and code."
                : "Ahoj! Jsem NitroAI-JKL-1. Můžu pomoci s otázkami, vysvětlováním, plánováním, textem i kódem.";
        }

        if (input.StartsWith("ahoj", StringComparison.OrdinalIgnoreCase) &&
            input.Length > "ahoj".Length &&
            !char.IsWhiteSpace(input["ahoj".Length]))
        {
            return userLanguage == "english"
                ? "If you want to greet me, write 'hello'. I can also help with questions, tasks, or code."
                : "Jestli chceš pozdrav, napiš přesně 'ahoj'. Jinak můžu pomoci s otázkami nebo kódem.";
        }

        if (string.Equals(input, "help", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("pomoc", StringComparison.OrdinalIgnoreCase))
        {
            return "Dostupné úkoly: help, explain <co>, review <co>, fix <co>, generate <název>, status, exit. " +
                   "Podporované jazyky: C#, C++, Python, Batch. Příklad: generate kalkulacka v C#, generate hello world in python, generate app in c++.";
        }

        if (string.Equals(input, "status", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("stav", StringComparison.OrdinalIgnoreCase))
        {
            var contextSummary = recentContext.Count == 0
                ? "žádný kontext"
                : string.Join(" | ", recentContext.TakeLast(3));

            return $"Agent status: ready. Kontext: {contextSummary}. Důležité: mám krátkou paměť pro posledních {MaxContextMessages} zpráv.";
        }

        if (input.StartsWith("ask ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("otazka ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("otázka ", StringComparison.OrdinalIgnoreCase) ||
            (input.EndsWith("?", StringComparison.OrdinalIgnoreCase) && !LooksLikeCodeRequest(input)))
        {
            var question = input.EndsWith("?", StringComparison.OrdinalIgnoreCase)
                ? input
                : GetArgument(input, input.StartsWith("ask ", StringComparison.OrdinalIgnoreCase) ? "ask" : "otazka");
            return AnswerGeneralQuestion(question);
        }

        if (input.StartsWith("summarize ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("shrň ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("shrn ", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("shrň mi", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("shrn mi", StringComparison.OrdinalIgnoreCase))
        {
            var text = GetArgument(input, input.StartsWith("summarize ", StringComparison.OrdinalIgnoreCase) ? "summarize" : "shrn");
            return SummarizeText(text);
        }

        if (input.StartsWith("translate ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("preloz ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("přelož ", StringComparison.OrdinalIgnoreCase))
        {
            var text = GetArgument(input, input.StartsWith("translate ", StringComparison.OrdinalIgnoreCase) ? "translate" : "preloz");
            return $"Překladový úkol: \"{text}\". Zadej také cílový jazyk, například: translate ahoj to English.";
        }

        if (input.StartsWith("plan ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("naplanuj ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("naplánuj ", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("naplánuj mi", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("naplanuj mi", StringComparison.OrdinalIgnoreCase))
        {
            var goal = GetArgument(input, input.StartsWith("plan ", StringComparison.OrdinalIgnoreCase) ? "plan" : "naplanuj");
            return CreatePlan(goal);
        }

        if (input.StartsWith("brainstorm ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("napady ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("nápady ", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("vymysli", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("nápad", StringComparison.OrdinalIgnoreCase))
        {
            var topic = GetArgument(input, input.StartsWith("brainstorm ", StringComparison.OrdinalIgnoreCase) ? "brainstorm" : "napady");
            return Brainstorm(topic);
        }

        if (input.StartsWith("calculate ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("spocitej ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("spočítej ", StringComparison.OrdinalIgnoreCase) ||
            LooksLikeMath(input))
        {
            var expression = input.Any(char.IsDigit) && !input.StartsWith("calculate ", StringComparison.OrdinalIgnoreCase)
                ? input
                : GetArgument(input, input.StartsWith("calculate ", StringComparison.OrdinalIgnoreCase) ? "calculate" : "spocitej");
            return CalculateSimpleExpression(expression);
        }

        if (input.StartsWith("explain ", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("vysvětli", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("vysvetli", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("co je", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("jak funguje", StringComparison.OrdinalIgnoreCase))
        {
            var topic = GetArgument(input, "explain");
            return $"Analyzuji: {topic}.\n" +
                   "1. Identifikuj problém nebo koncept.\n" +
                   "2. Rozděl na hlavní části.\n" +
                   "3. Navrhni bezpečný návrh řešení.\n" +
                   "4. Zkontroluj edge cases a testy.";
        }

        if (input.StartsWith("review ", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("review", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("zkontroluj kód", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("zkontroluj kod", StringComparison.OrdinalIgnoreCase))
        {
            var topic = GetArgument(input, "review");
            return $"Code review pro: {topic}.\n" +
                   "- Zhodnotím architekturu a čitelnost.\n" +
                   "- Ověřím bezpečnostní a výkonové riziko.\n" +
                   "- Navrhnu konkrétní doporučení a následné kroky.";
        }

        if (input.StartsWith("fix ", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("opravi", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("fixni", StringComparison.OrdinalIgnoreCase))
        {
            var topic = GetArgument(input, "fix");
            return $"Plán opravy pro: {topic}.\n" +
                   "1. Najdu kořen příčiny.\n" +
                   "2. Opravím konkrétní bod v kódu.\n" +
                   "3. Přidám základní validaci.\n" +
                   "4. Ověřím, že neporuším existující chování.";
        }

        if (input.StartsWith("generate ", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("vygeneruj", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("vytvoř", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("vytvor", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("naprogramuj", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("udělej", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("udelej", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("napiš program", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("napis program", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("make a", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("create a", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("write a", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("build a", StringComparison.OrdinalIgnoreCase) ||
            DetectProgramType(input) != "generic")
        {
            var topic = GetArgument(input, "generate");
            return GenerateCodeSnippet(topic);
        }

        if (input.Contains("kdo jsi", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("jméno", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("who are you", StringComparison.OrdinalIgnoreCase))
        {
            return "Jsem NitroAI-JKL-1, univerzální AI asistent. Umím pomáhat s běžnými otázkami, texty, učením, plánováním i programováním.";
        }

        if (input.Contains("debug", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("chyba", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            return "Error-handling workflow:\n" +
                   "- Reproduce the error.\n" +
                   "- Collect the stack trace or exact error message.\n" +
                   "- Reduce the problem to a minimal example.\n" +
                   "- Fix the root cause and add a regression test.";
        }

        if (!string.IsNullOrWhiteSpace(previousUserMessage) &&
            previousUserMessage.Contains("ahoj", StringComparison.OrdinalIgnoreCase))
        {
            return "Pamatuji si, že jsi mě pozdravil. Jaký úkol máš teď v kódu nebo v projektu?";
        }

        return userLanguage == "english"
            ? $"I received your request: \"{input}\". Tell me whether you need an explanation, a plan, ideas, text, or code."
            : $"Rozumím požadavku: \"{input}\". Napiš, jestli chceš vysvětlení, plán, nápady, text, nebo kód.";
    }

    static string FormatAgentOutput(string input, string reply)
    {
        if (!input.StartsWith("generate ", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("vygeneruj", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("vytvoř", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("vytvor", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("naprogramuj", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("udělej", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("udelej", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("napiš program", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("napis program", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("make a", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("create a", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("write a", StringComparison.OrdinalIgnoreCase) &&
            !input.Contains("build a", StringComparison.OrdinalIgnoreCase))
        {
            return reply;
        }

        var language = DetectLanguage(input);
        var purpose = DetectUserLanguage(input) == "english"
            ? DetectProgramType(input) switch
            {
                "calculator" => "calculator",
                "hello" => "Hello World program",
                "game" => DetectGameType(input) == "rock-paper-scissors" ? "Rock Paper Scissors game" : "number guessing game",
                "todo" => "simple TODO list",
                "application" => "console application",
                _ => "short program"
            }
            : DetectProgramType(input) switch
            {
                "calculator" => "kalkulačku",
                "hello" => "hello world",
                "game" => DetectGameType(input) == "rock-paper-scissors" ? "hru Kámen, nůžky, papír" : "hru hádání čísla",
                "todo" => "jednoduchý todo seznam",
                "application" => "konzolovou aplikaci",
                _ => "krátký program"
            };

        var languageName = language switch
        {
            "csharp" => "C#",
            "cpp" => "C++",
            "c" => "C",
            "python" => "Python",
            "batch" => "Batch",
            "wpf" => "WPF",
            "html" => "HTML",
            _ => "C#"
        };

        var summary = DetectUserLanguage(input) == "english"
            ? $"I created a {purpose} in {languageName}. The code contains a working starting point that you can extend."
            : language switch
            {
                "csharp" => $"Vytvořil jsem {purpose} v {languageName}. Kód obsahuje základní strukturu programu a můžeš ho hned dále rozšiřovat.",
                "cpp" => $"Vytvořil jsem {purpose} v {languageName}. Program používá jednoduchý vstup, výpočet a výpis výsledku.",
                "c" => $"Vytvořil jsem {purpose} v {languageName}. Program používá základní funkce, vstup a výstup.",
                "python" => $"Vytvořil jsem {purpose} v {languageName}. Skript pracuje s jednoduchým vstupem a výstupem.",
                "batch" => $"Vytvořil jsem {purpose} v {languageName}. Soubor pracuje s promptem a zobrazením výsledku v cmd okně.",
                "wpf" => $"Vytvořil jsem základ WPF aplikace v XAML a C#. Obsahuje okno, ovládací prvky a událost tlačítka.",
            "html" => $"I created a web application in {languageName}. The page contains structure, styling, and basic interaction.",
                _ => $"Vytvořil jsem {purpose} v {languageName}."
            };

        var normalizedCode = reply
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        if (DetectUserLanguage(input) == "english")
        {
            normalizedCode = LocalizeGeneratedCodeToEnglish(normalizedCode);
        }

        var educationalTitle = DetectUserLanguage(input) == "english"
            ? "How it works:"
            : "Jak to funguje:";

        var codeTitle = DetectUserLanguage(input) == "english" ? "Code:" : "Kód:";
        var codeEnd = DetectUserLanguage(input) == "english"
            ? "--- End of code ---"
            : "--- Konec kódu ---";

        return summary + Environment.NewLine +
               codeTitle + Environment.NewLine +
               normalizedCode.TrimEnd() + Environment.NewLine +
               codeEnd + Environment.NewLine + Environment.NewLine +
               educationalTitle + Environment.NewLine +
               GetEducationalMaterial(DetectProgramType(input), language, DetectUserLanguage(input) == "english", input);
    }

    static string LocalizeGeneratedCodeToEnglish(string code)
    {
        var translations = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Myslim si cislo od 1 do 100."] = "I am thinking of a number from 1 to 100.",
            ["Tvuj tip: "] = "Your guess: ",
            ["Moje cislo je vetsi."] = "My number is higher.",
            ["Moje cislo je mensi."] = "My number is lower.",
            ["Vyhral jsi!"] = "You won!",
            ["Zadej prvni cislo: "] = "Enter the first number: ",
            ["Zadej druhe cislo: "] = "Enter the second number: ",
            ["Zadej operaci (+, -, *, /): "] = "Enter an operation (+, -, *, /): ",
            ["Vysledek: "] = "Result: ",
            ["Neplatna operace"] = "Invalid operation",
            ["Nulou nelze delit."] = "Cannot divide by zero.",
            ["Zadej cele cislo."] = "Enter a whole number.",
            ["Neplatna volba."] = "Invalid choice.",
            ["Moje aplikace"] = "My application",
            ["Pozdrav"] = "Greeting",
            ["Informace"] = "Information",
            ["Konec"] = "Exit",
            ["Vyber moznost: "] = "Choose an option: ",
            ["Ahoj!"] = "Hello!",
            ["Toto je moje prvni C aplikace."] = "This is my first C application.",
            ["Toto je moje prvni C++ aplikace."] = "This is my first C++ application.",
            ["Toto je moje prvni C# aplikace."] = "This is my first C# application.",
            ["Toto je moje prvni Batch aplikace."] = "This is my first Batch application.",
            ["Toto je moje prvni Python aplikace."] = "This is my first Python application.",
            ["Aplikace se ukoncuje."] = "The application is closing.",
            ["Myslim si cislo od 1 do 10."] = "I am thinking of a number from 1 to 10.",
            ["Novy ukol: "] = "New task: ",
            ["Ukol: "] = "Task: ",
            ["<html lang=\"cs\">"] = "<html lang=\"en\">",
            ["Klikni na tlačítko."] = "Click the button."
        };

        foreach (var translation in translations)
        {
            code = code.Replace(translation.Key, translation.Value, StringComparison.Ordinal);
        }

        return code;
    }

    static string GetEducationalMaterial(string programType, string language, bool english, string topic)
    {
        if (english)
        {
            var programHint = programType switch
            {
                "calculator" => "The calculator reads two values, checks the selected operator, and performs the calculation.",
                "game" => DetectGameType(topic) == "rock-paper-scissors"
                    ? "The game chooses a move for the computer, reads your move, and compares both choices to determine the winner."
                    : "The game creates a secret number, reads guesses in a loop, and tells the player whether to guess higher or lower.",
                "todo" => "The TODO program stores tasks in a list. Add creates a task, list displays tasks, and exit closes the program.",
                "application" => "The application uses a main menu loop. The user selects an option, the program performs an action, and the menu appears again.",
                "hello" => "Hello World prints a message and then the program ends.",
                _ => "The program is divided into input, processing, and output."
            };

            var languageHint = language switch
            {
                "csharp" => "In C#, Main is the entry point. Console.ReadLine reads console input and TryParse can validate it safely.",
                "cpp" => "In C++, main is the entry point. cin reads input, cout prints output, and #include adds libraries.",
                "c" => "In C, main is the entry point. scanf reads input, printf prints output, and #include adds libraries.",
                "python" => "Python runs code from top to bottom. input reads text, int or float converts numbers, and print displays output.",
                "batch" => "Batch commands run in a .bat file. set stores values and echo prints them in the command window.",
                "wpf" => "WPF uses XAML for the interface and C# for logic. A button can react to its Click event.",
                "html" => "HTML defines the page structure, CSS controls its appearance, and JavaScript adds interaction.",
                _ => "The program is divided into input, processing, and output."
            };

            return $"- {programHint}{Environment.NewLine}- {languageHint}{Environment.NewLine}- Try changing the text, values, or conditions and observe the result.";
        }

        var czechLanguageHint = language switch
        {
            "csharp" => "V C# začíná spuštění metodou Main. Console.ReadLine načte text z konzole a TryParse bezpečně ověří vstup.",
            "cpp" => "V C++ začíná spuštění funkcí main. cin načítá vstup a cout vypisuje výsledek. Knihovny se přidávají pomocí #include.",
            "c" => "V jazyce C začíná spuštění funkcí main. scanf načítá vstup, printf vypisuje výsledek a knihovny se přidávají pomocí #include.",
            "python" => "V Pythonu se program vykonává shora dolů. input načte text, int nebo float ho převede na číslo a print zobrazí výsledek.",
            "batch" => "V Batch se příkazy vykonávají v souboru .bat. set načte nebo uloží hodnotu a echo ji vypíše do konzole.",
            "wpf" => "WPF používá XAML pro vzhled okna a C# pro logiku. Tlačítko může reagovat na událost Click a měnit obsah rozhraní.",
            "html" => "HTML definuje obsah stránky pomocí elementů. CSS řeší vzhled a JavaScript může reagovat na kliknutí uživatele.",
            _ => "Program se skládá ze vstupu, zpracování dat a výstupu."
        };

        var czechProgramHint = programType switch
        {
            "calculator" => "Kalkulačka načte dvě hodnoty, zjistí operátor a pomocí podmínek nebo switch provede výpočet.",
            "game" => DetectGameType(topic) == "rock-paper-scissors"
                ? "Hra vybere tah počítače, načte tah hráče a porovná obě volby, aby určila vítěze."
                : "Hra vygeneruje tajné číslo, opakovaně čte tip hráče a pomocí cyklu poskytuje nápovědu, dokud hráč nevyhraje.",
            "todo" => "TODO program ukládá úkoly do seznamu. Příkaz add přidá úkol, list je vypíše a exit program ukončí.",
            "application" => "Aplikace má hlavní smyčku s nabídkou. Uživatel vybere možnost, program provede příslušnou akci a menu se zobrazí znovu.",
            "hello" => "Hello World je nejjednodušší program: vypíše text do konzole a skončí.",
            _ => "Nejprve si projdi vstupní část, potom hlavní logiku a nakonec výstup programu."
        };

        return $"- {czechProgramHint}{Environment.NewLine}- {czechLanguageHint}{Environment.NewLine}" +
               "- Zkus změnit texty, hodnoty nebo podmínky a sleduj, jak se program chová.";
    }

    static string DetectUserLanguage(string input)
    {
        var czechMarkers = new[]
        {
            "ahoj", "č", "ě", "š", "ř", "ž", "ý", "á", "í", "é", "ů", "ú",
            "naprogramuj", "vytvoř", "vysvětli", "hra", "hru", "kalkulač", "aplikac"
        };

        var englishMarkers = new[]
        {
            "hello", "hi", "please", "create", "make", "write", "build", "explain",
            "game", "calculator", "application", "question", "how", "what", "why", "the", " in "
        };

        var hasCzech = czechMarkers.Any(marker => input.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var hasEnglish = englishMarkers.Any(marker => input.Contains(marker, StringComparison.OrdinalIgnoreCase));

        if (hasCzech && !hasEnglish)
        {
            return "czech";
        }

        if (hasEnglish && !hasCzech)
        {
            return "english";
        }

        if (hasCzech && hasEnglish)
        {
            return input.Contains(" v ", StringComparison.OrdinalIgnoreCase) ||
                   input.Contains("naprogramuj", StringComparison.OrdinalIgnoreCase)
                ? "czech"
                : "english";
        }

        return "unknown";
    }

    static string GetArgument(string input, string command)
    {
        var commandText = command + " ";
        if (input.StartsWith(commandText, StringComparison.OrdinalIgnoreCase))
        {
            return input.Substring(commandText.Length).Trim();
        }

        return input.Replace(command, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
    }

    static string AnswerGeneralQuestion(string question)
    {
        var english = DetectUserLanguage(question) == "english";
        if (string.IsNullOrWhiteSpace(question))
        {
            return english
                ? "Please enter a question, for example: what is the internet?"
                : "Napiš otázku. Například: co je internet?";
        }

        if (question.Contains("čas", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("datum", StringComparison.OrdinalIgnoreCase))
        {
            return english
                ? $"According to the computer's local time, it is {DateTime.Now:MM/dd/yyyy HH:mm}."
                : $"Podle lokálního času počítače je {DateTime.Now:dd.MM.yyyy HH:mm}.";
        }

        if (question.Contains("jídlo", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("jidlo", StringComparison.OrdinalIgnoreCase) ||
            question.Contains("recept", StringComparison.OrdinalIgnoreCase))
        {
            return english
                ? "Quick idea: pasta with garlic, oil, and parmesan. Tell me the ingredients and number of servings for a more precise recipe."
                : "Rychlý tip: těstoviny s česnekem, olejem a parmazánem. Pokud chceš přesný recept, napiš suroviny a počet porcí.";
        }

        return english
            ? $"I need more context for \"{question}\". Tell me whether you want an explanation, instructions, ideas, text, or code."
            : $"Na otázku \"{question}\" potřebuji více kontextu. Zkus ji rozdělit na konkrétní části nebo napiš, jestli chceš vysvětlení, návod, seznam možností, text, nebo kód.";
    }

    static string SummarizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "Zadej text za příkaz summarize nebo shrň.";
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var preview = string.Join(' ', words.Take(20));
        var suffix = words.Length > 20 ? "…" : string.Empty;
        return $"Shrnutí: Text má přibližně {words.Length} slov. Hlavní obsah: {preview}{suffix}\n" +
               "Pro přesnější shrnutí pošli celý text a případně uveď požadovanou délku.";
    }

    static string CreatePlan(string goal)
    {
        if (string.IsNullOrWhiteSpace(goal))
        {
            return "Zadej cíl za příkaz plan. Například: plan vytvořit osobní web.";
        }

        return $"Plán pro: {goal}\n" +
               "1. Ujasni cíl a požadovaný výsledek.\n" +
               "2. Rozděl úkol na menší kroky.\n" +
               "3. Urči potřebné nástroje, čas a rizika.\n" +
               "4. Udělej první nejmenší proveditelný krok.\n" +
               "5. Ověř výsledek a uprav další postup.";
    }

    static string Brainstorm(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return "Zadej téma za příkaz brainstorm. Například: brainstorm nápady na hru.";
        }

        return $"Nápady pro téma \"{topic}\":\n" +
               "1. Jednoduchá verze pro rychlý začátek.\n" +
               "2. Verze s uživatelskými účty nebo ukládáním dat.\n" +
               "3. Soutěžní nebo multiplayer varianta.\n" +
               "4. Mobilní nebo webová verze.\n" +
               "5. Přidej statistiky, nastavení a možnost sdílení.";
    }

    static string CalculateSimpleExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return "Zadej výraz za příkaz calculate. Například: calculate 12 + 8.";
        }

        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3 || !double.TryParse(parts[0], out var left) || !double.TryParse(parts[2], out var right))
        {
            return "Zatím podporuji formát: číslo operátor číslo, například calculate 12 + 8.";
        }

        var result = parts[1] switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" when right != 0 => left / right,
            _ => double.NaN
        };

        return double.IsNaN(result)
            ? "Invalid operator or division by zero. Use +, -, *, or /."
            : $"Výsledek: {result}";
    }

    static bool LooksLikeMath(string input)
    {
        return input.Any(char.IsDigit) &&
               (input.Contains(" + ", StringComparison.Ordinal) ||
                input.Contains(" - ", StringComparison.Ordinal) ||
                input.Contains(" * ", StringComparison.Ordinal) ||
                input.Contains(" / ", StringComparison.Ordinal));
    }

    static bool LooksLikeCodeRequest(string input)
    {
        var codeWords = new[]
        {
            "code", "program", "game", "app", "application", "c#", "c++", "python", "batch",
            "html", "wpf", "kód", "kod", "programuj", "naprogramuj", "vytvoř", "vytvor"
        };

        return codeWords.Any(word => input.Contains(word, StringComparison.OrdinalIgnoreCase));
    }

    static string GenerateCodeSnippet(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return "Zadej, co má generátor vytvořit. Například: generate kalkulacka v C#, generate hello world v python, generate app in c++.";
        }

        var safeTopic = topic.Trim();
        var language = DetectLanguage(safeTopic);
        var programType = DetectProgramType(safeTopic);

        if (programType == "calculator")
        {
            return GenerateCalculatorCode(language, safeTopic);
        }

        if (programType == "hello")
        {
            return GenerateHelloWorldCode(language, safeTopic);
        }

        if (programType == "game")
        {
            return GenerateGameCode(language, safeTopic);
        }

        if (programType == "todo")
        {
            return GenerateTodoCode(language);
        }

        if (programType == "application")
        {
            return GenerateApplicationCode(language);
        }

        if (safeTopic.Contains("dto", StringComparison.OrdinalIgnoreCase) ||
            safeTopic.Contains("model", StringComparison.OrdinalIgnoreCase))
        {
            if (language == "csharp")
            {
                return "public class UserDto\n{\n    public int Id { get; set; }\n    public string Name { get; set; }\n    public string Email { get; set; }\n}\n";
            }

            if (language == "cpp")
            {
                return "#include <string>\n\nstruct UserDto\n{\n    int id;\n    std::string name;\n    std::string email;\n};\n";
            }

            if (language == "python")
            {
                return "class UserDto:\n    def __init__(self, id, name, email):\n        self.id = id\n        self.name = name\n        self.email = email\n";
            }

            if (language == "batch")
            {
                return "@echo off\nset /p name=Zadej jmeno: \n echo Vytvoren model pro %name%\n";
            }
        }

        if (safeTopic.Contains("service", StringComparison.OrdinalIgnoreCase) ||
            safeTopic.Contains("class", StringComparison.OrdinalIgnoreCase))
        {
            if (language == "csharp")
            {
                return "public class UserService\n{\n    public string GetGreeting(string name)\n    {\n        return $\"Ahoj, {name}!\";\n    }\n}\n";
            }

            if (language == "cpp")
            {
                return "#include <string>\n\nclass UserService\n{\npublic:\n    std::string GetGreeting(const std::string& name) const\n    {\n        return \"Ahoj, \" + name + \"!\";\n    }\n};\n";
            }

            if (language == "python")
            {
                return "class UserService:\n    def get_greeting(self, name):\n        return f\"Ahoj, {name}!\"\n";
            }

            if (language == "batch")
            {
                return "@echo off\nset name=uzivatel\necho Ahoj, %name%!\n";
            }
        }

        if (safeTopic.Contains("api", StringComparison.OrdinalIgnoreCase) ||
            safeTopic.Contains("controller", StringComparison.OrdinalIgnoreCase))
        {
            if (language == "csharp")
            {
                return "[ApiController]\n[Route(\"api/[controller]\")]\npublic class UsersController : ControllerBase\n{\n    [HttpGet]\n    public IActionResult Get()\n    {\n        return Ok(new { message = \"Hello from agent\" });\n    }\n}\n";
            }

            if (language == "python")
            {
                return "from flask import Flask\n\napp = Flask(__name__)\n\n@app.route('/api/users')\ndef users():\n    return {'message': 'Hello from agent'}\n";
            }
        }

        return GenerateDefaultCode(language, safeTopic);
    }

    static string GenerateCalculatorCode(string language, string topic)
    {
        if (language == "c")
        {
            return "#include <stdio.h>\n\nint main(void)\n{\n    double a, b;\n    char op;\n\n    printf(\"Zadej prvni cislo: \" );\n    scanf(\"%lf\", &a);\n    printf(\"Zadej operaci (+, -, *, /): \" );\n    scanf(\" %c\", &op);\n    printf(\"Zadej druhe cislo: \" );\n    scanf(\"%lf\", &b);\n\n    switch (op)\n    {\n        case '+': printf(\"Vysledek: %.2f\\n\", a + b); break;\n        case '-': printf(\"Vysledek: %.2f\\n\", a - b); break;\n        case '*': printf(\"Vysledek: %.2f\\n\", a * b); break;\n        case '/':\n            if (b != 0) printf(\"Vysledek: %.2f\\n\", a / b);\n            else printf(\"Nulou nelze delit.\\n\");\n            break;\n        default: printf(\"Neplatna operace\\n\");\n    }\n\n    return 0;\n}\n";
        }

        if (language == "html")
        {
            return "<!DOCTYPE html>\n<html lang=\"cs\">\n<head>\n    <meta charset=\"UTF-8\">\n    <title>Kalkulacka</title>\n</head>\n<body>\n    <input id=\"a\" type=\"number\" placeholder=\"Prvni cislo\">\n    <select id=\"op\"><option>+</option><option>-</option><option>*</option><option>/</option></select>\n    <input id=\"b\" type=\"number\" placeholder=\"Druhe cislo\">\n    <button onclick=\"calculate()\">Spocitat</button>\n    <p id=\"result\"></p>\n    <script>\n        function calculate() {\n            const a = Number(document.getElementById('a').value);\n            const b = Number(document.getElementById('b').value);\n            const op = document.getElementById('op').value;\n            const result = op === '+' ? a + b : op === '-' ? a - b : op === '*' ? a * b : a / b;\n            document.getElementById('result').textContent = `Vysledek: ${result}`;\n        }\n    </script>\n</body>\n</html>\n";
        }

        if (language == "wpf")
        {
            return "MainWindow.xaml:\n<Window x:Class=\"Calculator.MainWindow\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Title=\"Kalkulacka\">\n    <StackPanel Margin=\"20\">\n        <TextBox x:Name=\"FirstNumber\" Margin=\"0,0,0,8\" />\n        <TextBox x:Name=\"SecondNumber\" Margin=\"0,0,0,8\" />\n        <Button Content=\"Spocitat\" Click=\"Calculate_Click\" />\n        <TextBlock x:Name=\"Result\" Margin=\"0,8,0,0\" />\n    </StackPanel>\n</Window>\n\nMainWindow.xaml.cs:\nusing System.Windows;\n\nnamespace Calculator;\n\npublic partial class MainWindow : Window\n{\n    public MainWindow() => InitializeComponent();\n\n    private void Calculate_Click(object sender, RoutedEventArgs e)\n    {\n        if (double.TryParse(FirstNumber.Text, out var first) && double.TryParse(SecondNumber.Text, out var second))\n            Result.Text = $\"Vysledek: {first + second}\";\n        else\n            Result.Text = \"Zadej platna cisla.\";\n    }\n}\n";
        }

        if (language == "cpp")
        {
            return "#include <iostream>\n\nint main()\n{\n    double a, b;\n    char op;\n\n    std::cout << \"Zadej prvni cislo: \";\n    std::cin >> a;\n    std::cout << \"Zadej operaci (+, -, *, /): \";\n    std::cin >> op;\n    std::cout << \"Zadej druhe cislo: \";\n    std::cin >> b;\n\n    switch (op)\n    {\n        case '+': std::cout << \"Vysledek: \" << (a + b) << std::endl; break;\n        case '-': std::cout << \"Vysledek: \" << (a - b) << std::endl; break;\n        case '*': std::cout << \"Vysledek: \" << (a * b) << std::endl; break;\n        case '/': std::cout << \"Vysledek: \" << (a / b) << std::endl; break;\n        default: std::cout << \"Neplatna operace\" << std::endl; break;\n    }\n\n    return 0;\n}\n";
        }

        if (language == "python")
        {
            return "a = float(input(\"Zadej prvni cislo: \") )\nop = input(\"Zadej operaci (+, -, *, /): \")\nb = float(input(\"Zadej druhe cislo: \") )\n\nif op == '+':\n    print(\"Vysledek:\", a + b)\nelif op == '-':\n    print(\"Vysledek:\", a - b)\nelif op == '*':\n    print(\"Vysledek:\", a * b)\nelif op == '/':\n    print(\"Vysledek:\", a / b)\nelse:\n    print(\"Neplatna operace\")\n";
        }

        if (language == "batch")
        {
            return "@echo off\nset /p a=Zadej prvni cislo: \nset /p op=Zadej operaci (+, -, *, /): \nset /p b=Zadej druhe cislo: \n\nif \"%op%\" == \"+\" (set /a vysledek=%a%+%b%)\nif \"%op%\" == \"-\" (set /a vysledek=%a%-%b%)\nif \"%op%\" == \"*\" (set /a vysledek=%a%*%b%)\nif \"%op%\" == \"/\" (set /a vysledek=%a%/%b%)\n\necho Vysledek: %vysledek%\n";
        }

        return "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        Console.Write(\"Zadej prvni cislo: \");\n        double a = double.Parse(Console.ReadLine());\n        Console.Write(\"Zadej operaci (+, -, *, /): \");\n        char op = Console.ReadKey().KeyChar;\n        Console.WriteLine();\n        Console.Write(\"Zadej druhe cislo: \");\n        double b = double.Parse(Console.ReadLine());\n\n        switch (op)\n        {\n            case '+': Console.WriteLine($\"Vysledek: {a + b}\"); break;\n            case '-': Console.WriteLine($\"Vysledek: {a - b}\"); break;\n            case '*': Console.WriteLine($\"Vysledek: {a * b}\"); break;\n            case '/': Console.WriteLine($\"Vysledek: {a / b}\"); break;\n            default: Console.WriteLine(\"Neplatna operace\"); break;\n        }\n    }\n}\n";
    }

    static string GenerateHelloWorldCode(string language, string topic)
    {
        if (language == "cpp")
        {
            return "#include <iostream>\n\nint main()\n{\n    std::cout << \"Hello from C++!\" << std::endl;\n    return 0;\n}\n";
        }

        if (language == "python")
        {
            return "print(\"Hello from Python!\")\n";
        }

        if (language == "batch")
        {
            return "@echo off\necho Hello from Batch!\npause\n";
        }

        return "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        Console.WriteLine(\"Hello from C#!\");\n    }\n}\n";
    }

    static string GenerateGameCode(string language, string topic)
    {
        if (DetectGameType(topic) == "rock-paper-scissors")
        {
            return GenerateRockPaperScissorsCode(language);
        }

        if (language == "html")
        {
            return "<!DOCTYPE html>\n<html lang=\"cs\">\n<head>\n    <meta charset=\"UTF-8\">\n    <title>Hádej číslo</title>\n    <style>\n        body { font-family: sans-serif; max-width: 500px; margin: 40px auto; text-align: center; }\n        input, button { padding: 8px; margin: 4px; }\n    </style>\n</head>\n<body>\n    <h1>Hádej číslo</h1>\n    <p>Myslím si číslo od 1 do 100.</p>\n    <input id=\"guess\" type=\"number\" min=\"1\" max=\"100\">\n    <button onclick=\"checkGuess()\">Zkusit</button>\n    <p id=\"message\"></p>\n    <script>\n        const secret = Math.floor(Math.random() * 100) + 1;\n        function checkGuess() {\n            const guess = Number(document.getElementById('guess').value);\n            const message = document.getElementById('message');\n            if (guess < secret) message.textContent = 'Moje číslo je větší.';\n            else if (guess > secret) message.textContent = 'Moje číslo je menší.';\n            else message.textContent = 'Vyhrál jsi!';\n        }\n    </script>\n</body>\n</html>\n";
        }

        if (language == "python")
        {
            return "import random\n\nsecret = random.randint(1, 100)\nprint(\"Myslim si cislo od 1 do 100.\")\n\nwhile True:\n    guess = int(input(\"Tvuj tip: \"))\n    if guess < secret:\n        print(\"Moje cislo je vetsi.\")\n    elif guess > secret:\n        print(\"Moje cislo je mensi.\")\n    else:\n        print(\"Vyhral jsi!\")\n        break\n";
        }

        if (language == "cpp")
        {
            return "#include <iostream>\n#include <random>\n\nint main()\n{\n    std::random_device device;\n    std::mt19937 generator(device());\n    std::uniform_int_distribution<int> distribution(1, 100);\n    const int secret = distribution(generator);\n    int guess;\n\n    std::cout << \"Myslim si cislo od 1 do 100.\\n\";\n    do\n    {\n        std::cout << \"Tvuj tip: \";\n        std::cin >> guess;\n        if (guess < secret) std::cout << \"Moje cislo je vetsi.\\n\";\n        if (guess > secret) std::cout << \"Moje cislo je mensi.\\n\";\n    } while (guess != secret);\n\n    std::cout << \"Vyhral jsi!\\n\";\n    return 0;\n}\n";
        }

        if (language == "batch")
        {
            return "@echo off\nset /a secret=%random% %% 10 + 1\necho Myslim si cislo od 1 do 10.\n:guess\nset /p tip=Tvuj tip: \nif %tip% LSS %secret% echo Moje cislo je vetsi.\nif %tip% GTR %secret% echo Moje cislo je mensi.\nif not %tip%==%secret% goto guess\necho Vyhral jsi!\npause\n";
        }

        return "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        var random = new Random();\n        int secret = random.Next(1, 101);\n        int guess;\n\n        Console.WriteLine(\"Myslim si cislo od 1 do 100.\");\n        do\n        {\n            Console.Write(\"Tvuj tip: \" );\n            if (!int.TryParse(Console.ReadLine(), out guess))\n            {\n                Console.WriteLine(\"Zadej cele cislo.\");\n                continue;\n            }\n\n            if (guess < secret) Console.WriteLine(\"Moje cislo je vetsi.\");\n            else if (guess > secret) Console.WriteLine(\"Moje cislo je mensi.\");\n        } while (guess != secret);\n\n        Console.WriteLine(\"Vyhral jsi!\");\n    }\n}\n";
    }

    static string GenerateRockPaperScissorsCode(string language)
    {
        if (language == "python")
        {
            return "import random\n\nchoices = ['rock', 'paper', 'scissors']\ncomputer = random.choice(choices)\nplayer = input('Choose rock, paper, or scissors: ').lower()\n\nprint(f'Computer chose: {computer}')\nif player not in choices:\n    print('Invalid choice.')\nelif player == computer:\n    print('Draw!')\nelif (player, computer) in [('rock', 'scissors'), ('paper', 'rock'), ('scissors', 'paper')]:\n    print('You won!')\nelse:\n    print('Computer won!')\n";
        }

        if (language == "cpp")
        {
            return "#include <iostream>\n#include <random>\n#include <string>\n\nint main()\n{\n    const std::string choices[] = { \"rock\", \"paper\", \"scissors\" };\n    std::random_device device;\n    std::mt19937 generator(device());\n    std::uniform_int_distribution<int> distribution(0, 2);\n    std::string player;\n    std::cout << \"Choose rock, paper, or scissors: \";\n    std::cin >> player;\n    const std::string computer = choices[distribution(generator)];\n    std::cout << \"Computer chose: \" << computer << std::endl;\n    if (player != \"rock\" && player != \"paper\" && player != \"scissors\") std::cout << \"Invalid choice.\";\n    else if (player == computer) std::cout << \"Draw!\";\n    else if ((player == \"rock\" && computer == \"scissors\") || (player == \"paper\" && computer == \"rock\") || (player == \"scissors\" && computer == \"paper\")) std::cout << \"You won!\";\n    else std::cout << \"Computer won!\";\n}\n";
        }

        if (language == "batch")
        {
            return "@echo off\nset /a computer=%random% %% 3 + 1\nset /p player=Choose rock, paper, or scissors: \nif /i \"%player%\"==\"rock\" set player=1\nif /i \"%player%\"==\"paper\" set player=2\nif /i \"%player%\"==\"scissors\" set player=3\nif not defined player echo Invalid choice. & pause & exit /b\necho Computer chose number %computer%\nif %player%==%computer% echo Draw!\nif %player%==1 if %computer%==3 echo You won!\nif %player%==2 if %computer%==1 echo You won!\nif %player%==3 if %computer%==2 echo You won!\npause\n";
        }

        if (language == "html")
        {
            return "<!DOCTYPE html>\n<html lang=\"en\">\n<body>\n    <h1>Rock, Paper, Scissors</h1>\n    <button onclick=\"play('rock')\">Rock</button>\n    <button onclick=\"play('paper')\">Paper</button>\n    <button onclick=\"play('scissors')\">Scissors</button>\n    <p id=\"result\"></p>\n    <script>\n        function play(player) {\n            const choices = ['rock', 'paper', 'scissors'];\n            const computer = choices[Math.floor(Math.random() * choices.length)];\n            const win = (player === 'rock' && computer === 'scissors') || (player === 'paper' && computer === 'rock') || (player === 'scissors' && computer === 'paper');\n            const result = player === computer ? 'Draw!' : win ? 'You won!' : 'Computer won!';\n            document.getElementById('result').textContent = `Computer chose ${computer}. ${result}`;\n        }\n    </script>\n</body>\n</html>\n";
        }

        return "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        var choices = new[] { \"rock\", \"paper\", \"scissors\" };\n        var computer = choices[new Random().Next(choices.Length)];\n        Console.Write(\"Choose rock, paper, or scissors: \" );\n        var player = Console.ReadLine()?.ToLowerInvariant();\n        Console.WriteLine($\"Computer chose: {computer}\");\n        Console.WriteLine(player == computer ? \"Draw!\" : \"Try adding the win-condition logic here.\");\n    }\n}\n";
    }

    static string GenerateTodoCode(string language)
    {
        if (language == "python")
        {
            return "tasks = []\n\nwhile True:\n    command = input(\"todo> \").strip().lower()\n    if command == \"add\":\n        tasks.append(input(\"Ukol: \"))\n    elif command == \"list\":\n        for index, task in enumerate(tasks, 1):\n            print(f\"{index}. {task}\")\n    elif command == \"exit\":\n        break\n    else:\n        print(\"Pouzij add, list nebo exit.\")\n";
        }

        if (language == "cpp")
        {
            return "#include <iostream>\n#include <string>\n#include <vector>\n\nint main()\n{\n    std::vector<std::string> tasks;\n    std::string command;\n    while (command != \"exit\")\n    {\n        std::cout << \"todo> \";\n        std::cin >> command;\n        if (command == \"add\")\n        {\n            std::string task;\n            std::cin.ignore();\n            std::getline(std::cin, task);\n            tasks.push_back(task);\n        }\n        else if (command == \"list\")\n        {\n            for (std::size_t i = 0; i < tasks.size(); ++i)\n                std::cout << i + 1 << \". \" << tasks[i] << std::endl;\n        }\n    }\n}\n";
        }

        if (language == "batch")
        {
            return "@echo off\nset /p task=Novy ukol: \necho [ ] %task%>>todo.txt\necho Ukol byl ulozen do todo.txt.\npause\n";
        }

        return "using System;\nusing System.Collections.Generic;\n\nclass Program\n{\n    static void Main()\n    {\n        var tasks = new List<string>();\n        while (true)\n        {\n            Console.Write(\"todo> \" );\n            var command = Console.ReadLine()?.Trim().ToLowerInvariant();\n            if (command == \"add\") tasks.Add(ReadTask());\n            else if (command == \"list\")\n                for (int i = 0; i < tasks.Count; i++) Console.WriteLine($\"{i + 1}. {tasks[i]}\");\n            else if (command == \"exit\") break;\n            else Console.WriteLine(\"Pouzij add, list nebo exit.\");\n        }\n    }\n\n    static string ReadTask()\n    {\n        Console.Write(\"Ukol: \" );\n        return Console.ReadLine() ?? string.Empty;\n    }\n}\n";
    }

    static string GenerateApplicationCode(string language)
    {
        if (language == "html")
        {
            return "<!DOCTYPE html>\n<html lang=\"cs\">\n<head>\n    <meta charset=\"UTF-8\">\n    <title>Moje aplikace</title>\n</head>\n<body>\n    <h1>Moje aplikace</h1>\n    <button onclick=\"showMessage()\">Pozdrav</button>\n    <p id=\"message\">Klikni na tlačítko.</p>\n    <script>\n        function showMessage() {\n            document.getElementById('message').textContent = 'Ahoj!';\n        }\n    </script>\n</body>\n</html>\n";
        }

        if (language == "wpf")
        {
            return "MainWindow.xaml:\n<Window x:Class=\"UniversalApp.MainWindow\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Title=\"Moje aplikace\">\n    <StackPanel Margin=\"20\">\n        <TextBlock Text=\"Moje první WPF aplikace\" FontSize=\"24\" />\n        <Button Content=\"Pozdrav\" Click=\"Button_Click\" Margin=\"0,12,0,0\" />\n        <TextBlock x:Name=\"Message\" Margin=\"0,8,0,0\" />\n    </StackPanel>\n</Window>\n\nMainWindow.xaml.cs:\nusing System.Windows;\n\nnamespace UniversalApp;\n\npublic partial class MainWindow : Window\n{\n    public MainWindow() => InitializeComponent();\n\n    private void Button_Click(object sender, RoutedEventArgs e)\n    {\n        Message.Text = \"Ahoj z WPF!\";\n    }\n}\n";
        }

        if (language == "c")
        {
            return "#include <stdio.h>\n\nint main(void)\n{\n    int choice;\n    do\n    {\n        printf(\"\\n=== Moje aplikace ===\\n1 - Pozdrav\\n2 - Informace\\n0 - Konec\\nVyber moznost: \" );\n        scanf(\"%d\", &choice);\n        if (choice == 1) printf(\"Ahoj!\\n\");\n        else if (choice == 2) printf(\"Toto je moje prvni C aplikace.\\n\");\n        else if (choice != 0) printf(\"Neplatna volba.\\n\");\n    } while (choice != 0);\n    return 0;\n}\n";
        }

        if (language == "python")
        {
            return "def show_menu():\n    print(\"\\n=== Moje aplikace ===\")\n    print(\"1 - Pozdrav\")\n    print(\"2 - Informace\")\n    print(\"0 - Konec\")\n\nwhile True:\n    show_menu()\n    choice = input(\"Vyber moznost: \")\n    if choice == \"1\":\n        print(\"Ahoj!\")\n    elif choice == \"2\":\n        print(\"Toto je moje prvni Python aplikace.\")\n    elif choice == \"0\":\n        print(\"Aplikace se ukoncuje.\")\n        break\n    else:\n        print(\"Neplatna volba.\")\n";
        }

        if (language == "cpp")
        {
            return "#include <iostream>\n\nint main()\n{\n    int choice;\n    do\n    {\n        std::cout << \"\\n=== Moje aplikace ===\\n\";\n        std::cout << \"1 - Pozdrav\\n2 - Informace\\n0 - Konec\\n\";\n        std::cout << \"Vyber moznost: \";\n        std::cin >> choice;\n\n        switch (choice)\n        {\n            case 1: std::cout << \"Ahoj!\\n\"; break;\n            case 2: std::cout << \"Toto je moje prvni C++ aplikace.\\n\"; break;\n            case 0: std::cout << \"Aplikace se ukoncuje.\\n\"; break;\n            default: std::cout << \"Neplatna volba.\\n\"; break;\n        }\n    } while (choice != 0);\n\n    return 0;\n}\n";
        }

        if (language == "batch")
        {
            return "@echo off\n:menu\ncls\necho === Moje aplikace ===\necho 1 - Pozdrav\necho 2 - Informace\necho 0 - Konec\nset /p choice=Vyber moznost: \nif \"%choice%\"==\"1\" echo Ahoj!\nif \"%choice%\"==\"2\" echo Toto je moje prvni Batch aplikace.\nif \"%choice%\"==\"0\" goto end\npause\ngoto menu\n:end\necho Aplikace se ukoncuje.\n";
        }

        return "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        string choice;\n        do\n        {\n            Console.WriteLine(\"\\n=== Moje aplikace ===\");\n            Console.WriteLine(\"1 - Pozdrav\");\n            Console.WriteLine(\"2 - Informace\");\n            Console.WriteLine(\"0 - Konec\");\n            Console.Write(\"Vyber moznost: \" );\n            choice = Console.ReadLine() ?? string.Empty;\n\n            switch (choice)\n            {\n                case \"1\": Console.WriteLine(\"Ahoj!\"); break;\n                case \"2\": Console.WriteLine(\"Toto je moje prvni C# aplikace.\"); break;\n                case \"0\": Console.WriteLine(\"Aplikace se ukoncuje.\"); break;\n                default: Console.WriteLine(\"Neplatna volba.\"); break;\n            }\n        } while (choice != \"0\");\n    }\n}\n";
    }

    static string GenerateDefaultCode(string language, string safeTopic)
    {
        var generatedName = ToPascalCase(safeTopic);

        if (language == "html")
        {
            return "<!DOCTYPE html>\n<html lang=\"cs\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>" + safeTopic + "</title>\n</head>\n<body>\n    <h1>" + safeTopic + "</h1>\n    <p>Vítej na mé první webové stránce.</p>\n</body>\n</html>\n";
        }

        if (language == "wpf")
        {
            return "MainWindow.xaml:\n<Window x:Class=\"MyApp.MainWindow\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Title=\"" + safeTopic + "\">\n    <Grid>\n        <TextBlock Text=\"Moje WPF aplikace\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" FontSize=\"24\" />\n    </Grid>\n</Window>\n";
        }

        if (language == "c")
        {
            return "#include <stdio.h>\n\nint main(void)\n{\n    printf(\"Program vytvořený v jazyce C: " + safeTopic + "\\n\");\n    return 0;\n}\n";
        }

        if (language == "cpp")
        {
            return "#include <iostream>\n\nclass " + generatedName + "\n{\npublic:\n    void Run()\n    {\n        std::cout << \"Generated in C++\" << std::endl;\n    }\n};\n\nint main()\n{\n    " + generatedName + " app;\n    app.Run();\n    return 0;\n}\n";
        }

        if (language == "python")
        {
            return "class " + generatedName + ":\n    def run(self):\n        print(\"Generated in Python\")\n\napp = " + generatedName + "()\napp.run()\n";
        }

        if (language == "batch")
        {
            return "@echo off\necho Generated in Batch\npause\n";
        }

        return "using System;\n\npublic class " + generatedName + "\n{\n    public static void Main()\n    {\n        Console.WriteLine(\"Generated in C#!\");\n    }\n}\n";
    }

    static string DetectLanguage(string value)
    {
        if (value.Contains("wpf", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("xaml", StringComparison.OrdinalIgnoreCase))
        {
            return "wpf";
        }

        if (value.Contains("html", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("webovou stranku", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("webovou stránku", StringComparison.OrdinalIgnoreCase))
        {
            return "html";
        }

        if (value.Contains("c++", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("cpp", StringComparison.OrdinalIgnoreCase))
        {
            return "cpp";
        }

        if (value.Contains("python", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("py", StringComparison.OrdinalIgnoreCase))
        {
            return "python";
        }

        if (value.Contains("batch", StringComparison.OrdinalIgnoreCase) ||
            value.Contains(".bat", StringComparison.OrdinalIgnoreCase))
        {
            return "batch";
        }

        if (value.Contains("c#", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return "csharp";
        }

        if (value.Contains(" v c", StringComparison.OrdinalIgnoreCase) ||
            value.Contains(" in c", StringComparison.OrdinalIgnoreCase) ||
            value.Contains(" c app", StringComparison.OrdinalIgnoreCase) ||
            value.Contains(" c program", StringComparison.OrdinalIgnoreCase) ||
            value.EndsWith(" c", StringComparison.OrdinalIgnoreCase) ||
            value.EndsWith(".c", StringComparison.OrdinalIgnoreCase))
        {
            return "c";
        }

        return "csharp";
    }

    static string DetectProgramType(string value)
    {
        if (value.Contains("kalkula", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("calculator", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("scitani", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("aritmet", StringComparison.OrdinalIgnoreCase))
        {
            return "calculator";
        }

        if (value.Contains("hello", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("ahoj", StringComparison.OrdinalIgnoreCase))
        {
            return "hello";
        }

        if (value.Contains("hra", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("hru", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("hry", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("game", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("hadani", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("hádan", StringComparison.OrdinalIgnoreCase))
        {
            return "game";
        }

        if (value.Contains("todo", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("ukol", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("úkol", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("seznam", StringComparison.OrdinalIgnoreCase))
        {
            return "todo";
        }

        if (value.Contains("aplikac", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("app", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("program", StringComparison.OrdinalIgnoreCase))
        {
            return "application";
        }

        return "generic";
    }

    static string DetectGameType(string value)
    {
        if (value.Contains("rock paper", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("kamen papir", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("kámen papír", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("scissors", StringComparison.OrdinalIgnoreCase))
        {
            return "rock-paper-scissors";
        }

        return "number-guessing";
    }

    static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "GeneratedClass";
        }

        var cleaned = new List<char>();
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                cleaned.Add(ch);
            }
            else if (cleaned.Count > 0 && cleaned[^1] != ' ')
            {
                cleaned.Add(' ');
            }
        }

        var parts = new string(cleaned.ToArray())
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (parts.Length == 0)
        {
            return "GeneratedClass";
        }

        return string.Concat(parts.Select(part =>
        {
            var normalized = part.Trim();
            if (normalized.Length == 0)
            {
                return string.Empty;
            }

            var first = char.ToUpperInvariant(normalized[0]);
            var rest = normalized.Substring(1).ToLowerInvariant();
            return first + rest;
        }));
    }
}
