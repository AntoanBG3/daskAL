using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SchoolManagementSystem
{
    public class Student
    {
        public int Id { get; set; }
        public string Ime { get; set; } = "";
        public string Familiya { get; set; } = "";
        public string Klas { get; set; } = "";
        public DateTime DataRazhdane { get; set; }
        public Dictionary<string, List<int>> PredmetiOcenki { get; set; } = new();

        public string PulnoIme => $"{Ime} {Familiya}";
        public int Vazrast => DateTime.Today.Year - DataRazhdane.Year - (DateTime.Today.DayOfYear < DataRazhdane.DayOfYear ? 1 : 0);
        
        public double VzemiSrednaOcenkaPredmet(string predmet)
        {
            if (PredmetiOcenki.TryGetValue(predmet, out var ocenki) && ocenki.Any())
                return ocenki.Average();
            return 0;
        }

        public double VzemiObshtaSrednaOcenka()
        {
            if (!PredmetiOcenki.Any()) return 0;
            return PredmetiOcenki.Values.SelectMany(o => o).Average();
        }
    }

    public class Teacher
    {
        public int Id { get; set; }
        public string Ime { get; set; } = "";
        public string Familiya { get; set; } = "";
        public List<string> PredavashtePredmeti { get; set; } = new();

        public string PulnoIme => $"{Ime} {Familiya}";
        
        public bool PredavaPredmet(string predmet)
        {
            return PredavashtePredmeti.Any(p => p.Equals(predmet, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class Subject
    {
        public int Id { get; set; }
        public string Ime { get; set; } = "";
        public string Opisanie { get; set; } = "";
        public int? UchitelId { get; set; }
    }

    public class UchilishtnaBazaDanni
    {
        public List<Student> Uchenici { get; set; } = new();
        public List<Teacher> Uchiteli { get; set; } = new();
        public List<Subject> Predmeti { get; set; } = new();
    }

    public class UchilishtenMenadzhar
    {
        private const string FAIL_DANNI = "uchilishtni_danni.json";
        private UchilishtnaBazaDanni bazaDanni = new();

        public UchilishtenMenadzhar()
        {
            ZarediBazaDanni();
        }

        private void ZarediBazaDanni()
        {
            try
            {
                if (File.Exists(FAIL_DANNI))
                {
                    string jsonSadarzhanost = File.ReadAllText(FAIL_DANNI);
                    bazaDanni = JsonSerializer.Deserialize<UchilishtnaBazaDanni>(jsonSadarzhanost) ?? new UchilishtnaBazaDanni();
                    Console.WriteLine("📚 Училищните данни са заредени успешно!");
                }
                else
                {
                    bazaDanni = new UchilishtnaBazaDanni();
                    Console.WriteLine("🆕 Започваме с нова училищна база данни.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Грешка при зареждане на данните: {ex.Message}");
                Console.WriteLine("Започваме с празна база данни.");
                bazaDanni = new UchilishtnaBazaDanni();
            }
        }

        public void ZapaziBazaDanni()
        {
            try
            {
                var jsonOpcii = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                string jsonSadarzhanost = JsonSerializer.Serialize(bazaDanni, jsonOpcii);
                File.WriteAllText(FAIL_DANNI, jsonSadarzhanost);
                Console.WriteLine("✅ Данните са запазени успешно!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Грешка при запазване на данните: {ex.Message}");
            }
        }

        public void RegistrirajUchenik(Student uchenik)
        {
            uchenik.Id = VzemiSlvashtoIdUchenik();
            bazaDanni.Uchenici.Add(uchenik);
            Console.WriteLine($"✅ Ученик {uchenik.PulnoIme} е регистриран успешно!");
        }

        public void RegistrirajUchitel(Teacher uchitel)
        {
            uchitel.Id = VzemiSlvashtoIdUchitel();
            bazaDanni.Uchiteli.Add(uchitel);
            Console.WriteLine($"✅ Учител {uchitel.PulnoIme} е регистриран успешно!");
        }

        public void DobaviPredmet(Subject predmet)
        {
            predmet.Id = VzemiSlvashtoIdPredmet();
            bazaDanni.Predmeti.Add(predmet);
            Console.WriteLine($"✅ Предмет '{predmet.Ime}' е добавен успешно!");
        }

        public bool ZapishiOcenka(int uchenikId, string imePredmet, int ocenka)
        {
            var uchenik = bazaDanni.Uchenici.FirstOrDefault(u => u.Id == uchenikId);
            if (uchenik == null)
            {
                Console.WriteLine("❌ Ученикът не е намерен!");
                return false;
            }

            if (!bazaDanni.Predmeti.Any(p => p.Ime.Equals(imePredmet, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"❌ Предмет '{imePredmet}' не е намерен! Моля, първо добавете предмета.");
                return false;
            }

            if (!uchenik.PredmetiOcenki.ContainsKey(imePredmet))
            {
                uchenik.PredmetiOcenki[imePredmet] = new List<int>();
            }

            uchenik.PredmetiOcenki[imePredmet].Add(ocenka);
            Console.WriteLine($"✅ Оценка {ocenka} е записана за {uchenik.PulnoIme} по {imePredmet}");
            return true;
        }

        public bool NaznachPredmetNaUchitel(int uchitelId, string imePredmet)
        {
            var predmet = bazaDanni.Predmeti.FirstOrDefault(p => p.Ime.Equals(imePredmet, StringComparison.OrdinalIgnoreCase));
            if (predmet == null)
            {
                Console.WriteLine($"❌ Предмет '{imePredmet}' не е намерен! Моля, първо добавете предмета от главното меню.");
                return false;
            }

            var uchitel = bazaDanni.Uchiteli.FirstOrDefault(u => u.Id == uchitelId);
            if (uchitel == null)
            {
                Console.WriteLine("❌ Учителят не е намерен!");
                return false;
            }

            if (uchitel.PredavashtePredmeti.Any(p => p.Equals(imePredmet, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"❌ Учителят {uchitel.PulnoIme} вече преподава {imePredmet}!");
                return false;
            }
            
            predmet.UchitelId = uchitelId;
            uchitel.PredavashtePredmeti.Add(imePredmet);
            Console.WriteLine($"✅ Предмет '{imePredmet}' е назначен на учител {uchitel.PulnoIme}");
            return true;
        }

        public bool PremahniPredmetOtUchitel(int uchitelId, string imePredmet)
        {
            var uchitel = bazaDanni.Uchiteli.FirstOrDefault(u => u.Id == uchitelId);
            if (uchitel == null)
            {
                Console.WriteLine("❌ Учителят не е намерен!");
                return false;
            }

            var predmetZaPremahvane = uchitel.PredavashtePredmeti.FirstOrDefault(p => p.Equals(imePredmet, StringComparison.OrdinalIgnoreCase));
            if (predmetZaPremahvane == null)
            {
                Console.WriteLine($"❌ Учителят {uchitel.PulnoIme} не преподава {imePredmet}!");
                return false;
            }

            uchitel.PredavashtePredmeti.Remove(predmetZaPremahvane);
            
            var predmet = bazaDanni.Predmeti.FirstOrDefault(p => p.Ime.Equals(imePredmet, StringComparison.OrdinalIgnoreCase));
            if (predmet != null && predmet.UchitelId == uchitelId)
            {
                predmet.UchitelId = null;
            }

            Console.WriteLine($"✅ Предмет '{imePredmet}' е премахнат от учител {uchitel.PulnoIme}");
            return true;
        }

        private int VzemiSlvashtoIdUchenik() => bazaDanni.Uchenici.Any() ? bazaDanni.Uchenici.Max(u => u.Id) + 1 : 1;
        private int VzemiSlvashtoIdUchitel() => bazaDanni.Uchiteli.Any() ? bazaDanni.Uchiteli.Max(u => u.Id) + 1 : 1;
        private int VzemiSlvashtoIdPredmet() => bazaDanni.Predmeti.Any() ? bazaDanni.Predmeti.Max(p => p.Id) + 1 : 1;

        public List<Student> VzemiVsichkiUchenici() => bazaDanni.Uchenici.ToList();
        public List<Teacher> VzemiVsichkiUchiteli() => bazaDanni.Uchiteli.ToList();
        public List<Subject> VzemiVsichkiPredmeti() => bazaDanni.Predmeti.ToList();

        public Student? NamerUchenik(int id) => bazaDanni.Uchenici.FirstOrDefault(u => u.Id == id);
        public Teacher? NamerUchitel(int id) => bazaDanni.Uchiteli.FirstOrDefault(u => u.Id == id);
    }

    class Program
    {
        private static readonly UchilishtenMenadzhar uchilishtenMenadzhar = new();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            PokasjiPozdravitelnoSaobshtenie();
            
            while (true)
            {
                PokasjiGlavnoMenu();
                
                string? izbor = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(izbor))
                {
                    Console.WriteLine("Моля, въведете валидна опция.");
                    continue;
                }

                bool tryvaDaNapredi = ObrabotiIzborNaPotvrebitel(izbor);
                if (!tryvaDaNapredi) break;
            }
        }

        private static void PokasjiPozdravitelnoSaobshtenie()
        {
            Console.WriteLine("🏫 Добре дошли в daskAL!");
            Console.WriteLine("Тази система ви помага да управлявате ученици, учители и оценки.");
            Console.WriteLine("Нека започнем!\n");
        }

        private static void PokasjiGlavnoMenu()
        {
            Console.WriteLine("\n📋 Какво бихте искали да направите?");
            Console.WriteLine("1️⃣  Добавяне на нов ученик");
            Console.WriteLine("2️⃣  Добавяне на нов учител");
            Console.WriteLine("3️⃣  Добавяне на нов предмет");
            Console.WriteLine("4️⃣  Записване на оценка");
            Console.WriteLine("5️⃣  Преглед на всички ученици");
            Console.WriteLine("6️⃣  Преглед на всички учители");
            Console.WriteLine("7️⃣  Преглед на всички предмети");
            Console.WriteLine("8️⃣  Преглед на оценките на ученик");
            Console.WriteLine("9️⃣  Назначаване на предмет на учител");
            Console.WriteLine("🔟 Премахване на предмет от учител");
            Console.WriteLine("1️⃣1️⃣ Запазване на данните");
            Console.WriteLine("0️⃣  Изход");
            Console.Write("\nВашият избор: ");
        }

        private static bool ObrabotiIzborNaPotvrebitel(string izbor)
        {
            switch (izbor)
            {
                case "1":
                    DobaviNovUchenik();
                    break;
                case "2":
                    DobaviNovUchitel();
                    break;
                case "3":
                    DobaviNovPredmet();
                    break;
                case "4":
                    ZapishiOcenkaNaUchenik();
                    break;
                case "5":
                    PokasjiVsichkiUchenici();
                    break;
                case "6":
                    PokasjiVsichkiUchiteli();
                    break;
                case "7":
                    PokasjiVsichkiPredmeti();
                    break;
                case "8":
                    PokasjiOcenkiNaUchenik();
                    break;
                case "9":
                    NaznachPredmetNaUchitel();
                    break;
                case "10":
                    PremahniPredmetOtUchitel();
                    break;
                case "11":
                    uchilishtenMenadzhar.ZapaziBazaDanni();
                    break;
                case "0":
                    ObrabotiIzhod();
                    return false;
                default:
                    Console.WriteLine("❌ Невалиден избор. Моля, опитайте отново.");
                    break;
            }
            return true;
        }

        private static void DobaviNovUchenik()
        {
            Console.WriteLine("\n👤 Добавяне на нов ученик...");
            
            string ime = VzemiVhavodOtPotrebitel("Име: ");
            string familiya = VzemiVhavodOtPotrebitel("Фамилия: ");
            string klas = VzemiVhavodOtPotrebitel("Клас: ");
            
            Console.Write("Дата на раждане (дд.мм.гггг): ");
            string? dataVhavod = Console.ReadLine();
            
            if (DateTime.TryParseExact(dataVhavod, "dd.MM.yyyy", null, 
                System.Globalization.DateTimeStyles.None, out DateTime dataRazhdane))
            {
                var novUchenik = new Student
                {
                    Ime = ime,
                    Familiya = familiya,
                    Klas = klas,
                    DataRazhdane = dataRazhdane
                };
                
                uchilishtenMenadzhar.RegistrirajUchenik(novUchenik);
            }
            else
            {
                Console.WriteLine("❌ Невалиден формат на дата. Моля, използвайте дд.мм.гггг формат.");
            }
        }

        private static void DobaviNovUchitel()
        {
            Console.WriteLine("\n👨‍🏫 Добавяне на нов учител...");
            
            string ime = VzemiVhavodOtPotrebitel("Име: ");
            string familiya = VzemiVhavodOtPotrebitel("Фамилия: ");
            
            var novUchitel = new Teacher
            {
                Ime = ime,
                Familiya = familiya
            };
            
            uchilishtenMenadzhar.RegistrirajUchitel(novUchitel);
        }

        private static void DobaviNovPredmet()
        {
            Console.WriteLine("\n📚 Добавяне на нов предмет...");
            
            string imePredmet = VzemiVhavodOtPotrebitel("Име на предмет: ");
            string opisanie = VzemiVhavodOtPotrebitel("Описание: ");

            if (uchilishtenMenadzhar.VzemiVsichkiPredmeti().Any(p => p.Ime.Equals(imePredmet, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"❌ Предмет с име '{imePredmet}' вече съществува.");
                return;
            }
            
            var novPredmet = new Subject
            {
                Ime = imePredmet,
                Opisanie = opisanie
            };
            
            uchilishtenMenadzhar.DobaviPredmet(novPredmet);
            Console.WriteLine("💡 Можете да назначите учител на този предмет по-късно от менюто (опция 9).");
        }

        private static void ZapishiOcenkaNaUchenik()
        {
            Console.WriteLine("\n📝 Записване на оценка...");
            
            var uchenici = uchilishtenMenadzhar.VzemiVsichkiUchenici();
            if (!uchenici.Any())
            {
                Console.WriteLine("❌ Няма налични ученици. Моля, първо добавете ученици.");
                return;
            }
            
            Console.WriteLine("\nУченици:");
            foreach (var uchenik in uchenici)
            {
                Console.WriteLine($"{uchenik.Id}. {uchenik.PulnoIme} (Клас: {uchenik.Klas})");
            }
            
            Console.Write("ID на ученик: ");
            if (!int.TryParse(Console.ReadLine(), out int uchenikId))
            {
                Console.WriteLine("❌ Невалиден ID на ученик!");
                return;
            }
            
            string imePredmet = VzemiVhavodOtPotrebitel("Предмет: ");
            
            Console.Write("Оценка (2-6): ");
            if (int.TryParse(Console.ReadLine(), out int ocenka) && ocenka >= 2 && ocenka <= 6)
            {
                uchilishtenMenadzhar.ZapishiOcenka(uchenikId, imePredmet, ocenka);
            }
            else
            {
                Console.WriteLine("❌ Невалидна оценка! Трябва да бъде между 2 и 6.");
            }
        }

        private static void NaznachPredmetNaUchitel()
        {
            Console.WriteLine("\n👨‍🏫 Назначаване на предмет на учител...");

            var uchiteli = uchilishtenMenadzhar.VzemiVsichkiUchiteli();
            if (!uchiteli.Any())
            {
                Console.WriteLine("❌ Няма налични учители. Моля, първо добавете учители.");
                return;
            }

            var predmeti = uchilishtenMenadzhar.VzemiVsichkiPredmeti();
            if (!predmeti.Any())
            {
                Console.WriteLine("❌ Няма налични предмети. Моля, първо добавете предмети.");
                return;
            }

            Console.WriteLine("\nУчители:");
            foreach (var uchitel in uchiteli)
            {
                string predavashti = uchitel.PredavashtePredmeti.Any()
                    ? string.Join(", ", uchitel.PredavashtePredmeti)
                    : "Няма назначени предмети";
                Console.WriteLine($"{uchitel.Id}. {uchitel.PulnoIme} ({predavashti})");
            }

            Console.Write("ID на учител: ");
            if (!int.TryParse(Console.ReadLine(), out int uchitelId))
            {
                Console.WriteLine("❌ Невалиден ID на учител!");
                return;
            }
            
            Console.WriteLine("\nНалични предмети:");
            foreach (var predmet in predmeti)
            {
                Console.WriteLine($"- {predmet.Ime}");
            }

            string imePredmet = VzemiVhavodOtPotrebitel("Име на предмет за назначаване: ");

            uchilishtenMenadzhar.NaznachPredmetNaUchitel(uchitelId, imePredmet);
        }

        private static void PremahniPredmetOtUchitel()
        {
            Console.WriteLine("\n👨‍🏫 Премахване на предмет от учител...");
            
            var uchiteli = uchilishtenMenadzhar.VzemiVsichkiUchiteli();
            if (!uchiteli.Any())
            {
                Console.WriteLine("❌ Няма налични учители.");
                return;
            }
            
            var uchiteliSPredmeti = uchiteli.Where(u => u.PredavashtePredmeti.Any()).ToList();
            if (!uchiteliSPredmeti.Any())
            {
                Console.WriteLine("❌ Няма учители с назначени предмети.");
                return;
            }
            
            Console.WriteLine("\nУчители с предмети:");
            foreach (var uchitel in uchiteliSPredmeti)
            {
                Console.WriteLine($"{uchitel.Id}. {uchitel.PulnoIme} ({string.Join(", ", uchitel.PredavashtePredmeti)})");
            }
            
            Console.Write("ID на учител: ");
            if (!int.TryParse(Console.ReadLine(), out int uchitelId))
            {
                Console.WriteLine("❌ Невалиден ID на учител!");
                return;
            }
            
            var izbranUchitel = uchiteli.FirstOrDefault(u => u.Id == uchitelId);
            if (izbranUchitel == null || !izbranUchitel.PredavashtePredmeti.Any())
            {
                Console.WriteLine("❌ Учителят не е намерен или няма назначени предмети!");
                return;
            }
            
            Console.WriteLine($"\nПредмети на {izbranUchitel.PulnoIme}:");
            for (int i = 0; i < izbranUchitel.PredavashtePredmeti.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {izbranUchitel.PredavashtePredmeti[i]}");
            }
            
            Console.Write("Номер на предмет за премахване: ");
            if (int.TryParse(Console.ReadLine(), out int predmetIndex) && 
                predmetIndex >= 1 && predmetIndex <= izbranUchitel.PredavashtePredmeti.Count)
            {
                string imePredmet = izbranUchitel.PredavashtePredmeti[predmetIndex - 1];
                uchilishtenMenadzhar.PremahniPredmetOtUchitel(uchitelId, imePredmet);
            }
            else
            {
                Console.WriteLine("❌ Невалиден номер на предмет!");
            }
        }

        private static void PokasjiVsichkiUchenici()
        {
            Console.WriteLine("\n👥 Всички ученици:");
            var uchenici = uchilishtenMenadzhar.VzemiVsichkiUchenici();
            
            if (!uchenici.Any())
            {
                Console.WriteLine("Все още няма регистрирани ученици.");
                return;
            }
            
            foreach (var uchenik in uchenici)
            {
                Console.WriteLine($"\n📋 ID: {uchenik.Id}");
                Console.WriteLine($"👤 Име: {uchenik.PulnoIme}");
                Console.WriteLine($"🎓 Клас: {uchenik.Klas}");
                Console.WriteLine($"🎂 Възраст: {uchenik.Vazrast} години");
                Console.WriteLine($"📈 Обща средна оценка: {uchenik.VzemiObshtaSrednaOcenka():F2}");
                Console.WriteLine("─────────────────────");
            }
        }

        private static void PokasjiVsichkiUchiteli()
        {
            Console.WriteLine("\n👨‍🏫 Всички учители:");
            var uchiteli = uchilishtenMenadzhar.VzemiVsichkiUchiteli();
            
            if (!uchiteli.Any())
            {
                Console.WriteLine("Все още няма регистрирани учители.");
                return;
            }
            
            foreach (var uchitel in uchiteli)
            {
                Console.WriteLine($"\n📋 ID: {uchitel.Id}");
                Console.WriteLine($"👤 Име: {uchitel.PulnoIme}");
                
                if (uchitel.PredavashtePredmeti.Any())
                {
                    Console.WriteLine($"📚 Предмети: {string.Join(", ", uchitel.PredavashtePredmeti)}");
                }
                else
                {
                    Console.WriteLine("📚 Предмети: Няма назначени предмети");
                }
                
                Console.WriteLine("─────────────────────");
            }
        }

        private static void PokasjiVsichkiPredmeti()
        {
            Console.WriteLine("\n📚 Всички предмети:");
            var predmeti = uchilishtenMenadzhar.VzemiVsichkiPredmeti();
            var uchiteli = uchilishtenMenadzhar.VzemiVsichkiUchiteli();
            
            if (!predmeti.Any())
            {
                Console.WriteLine("Все още няма добавени предмети.");
                return;
            }
            
            foreach (var predmet in predmeti)
            {
                var uchitel = predmet.UchitelId.HasValue ? uchiteli.FirstOrDefault(u => u.Id == predmet.UchitelId.Value) : null;
                Console.WriteLine($"\n📋 ID: {predmet.Id}");
                Console.WriteLine($"📖 Предмет: {predmet.Ime}");
                Console.WriteLine($"📝 Описание: {predmet.Opisanie}");
                Console.WriteLine($"👨‍🏫 Учител: {uchitel?.PulnoIme ?? "Неназначен"}");
                Console.WriteLine("─────────────────────");
            }
        }

        private static void PokasjiOcenkiNaUchenik()
        {
            Console.WriteLine("\n📊 Оценки на ученик:");
            var uchenici = uchilishtenMenadzhar.VzemiVsichkiUchenici();
            
            if (!uchenici.Any())
            {
                Console.WriteLine("Все още няма регистрирани ученици.");
                return;
            }
            
            Console.WriteLine("\nУченици:");
            foreach (var uchenik in uchenici)
            {
                Console.WriteLine($"{uchenik.Id}. {uchenik.PulnoIme} (Клас: {uchenik.Klas})");
            }
            
            Console.Write("ID на ученик: ");
            if (int.TryParse(Console.ReadLine(), out int uchenikId))
            {
                var uchenik = uchilishtenMenadzhar.NamerUchenik(uchenikId);
                if (uchenik != null)
                {
                    Console.WriteLine($"\n📊 Оценки на {uchenik.PulnoIme}:");
                    
                    if (!uchenik.PredmetiOcenki.Any())
                    {
                        Console.WriteLine("Все още няма записани оценки.");
                        return;
                    }
                    
                    foreach (var predmetOcenki in uchenik.PredmetiOcenki)
                    {
                        var srednaOcenka = predmetOcenki.Value.Average();
                        Console.WriteLine($"📚 {predmetOcenki.Key}: {string.Join(", ", predmetOcenki.Value)} (Средна: {srednaOcenka:F2})");
                    }
                    
                    Console.WriteLine($"\n🎯 Обща средна оценка: {uchenik.VzemiObshtaSrednaOcenka():F2}");
                }
                else
                {
                    Console.WriteLine("❌ Ученикът не е намерен!");
                }
            }
            else
            {
                Console.WriteLine("❌ Невалиден ID на ученик!");
            }
        }

        private static void ObrabotiIzhod()
        {
            Console.Write("💾 Да се запазят данните преди изход? (д/н): ");
            string? izborZapazi = Console.ReadLine()?.ToLower();
            
            if (izborZapazi == "д" || izborZapazi == "да