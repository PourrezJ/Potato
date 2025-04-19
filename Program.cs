using System;
using System.Runtime.InteropServices;

namespace Potato
{
    public static class Program
    {
        // Pour attacher une console à l'application Windows
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [STAThread]
        static void Main()
        {
            // Activer la console
            AllocConsole();
            Console.WriteLine("Démarrage du jeu Potato...");

            try
            {
                using var game = new Potato.GameManager();
                Console.WriteLine("Instance de jeu créée avec succès.");
                game.Run();
            }
            catch (Exception ex)
            {
                // Afficher l'erreur qui a causé le crash
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERREUR FATALE:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Stack Trace:");
                Console.WriteLine(ex.StackTrace);

                // Vérifier s'il y a une exception interne
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Exception interne:");
                    Console.WriteLine(ex.InnerException.Message);
                    Console.WriteLine(ex.InnerException.StackTrace);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nAppuyez sur une touche pour fermer le programme...");
                Console.ResetColor();
                Console.ReadKey();
            }
        }
    }
}
