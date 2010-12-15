using System;

namespace MineWorld
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (MineWorldGame game = new MineWorldGame(args))
            {
                //try
                //{
                    game.Run();
                //}
                //catch (Exception e)
                //{
                //    System.Windows.Forms.MessageBox.Show(e.Message + "\r\n\r\n" + e.StackTrace);
                //}
            }
        }
    }
}

