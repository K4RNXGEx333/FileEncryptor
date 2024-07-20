#define DEBUG

namespace FileEncryptor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new File_Encryptor());
        }
    }
}

#if DEBUG
// Made and programmed by K4rnxge
#endif
