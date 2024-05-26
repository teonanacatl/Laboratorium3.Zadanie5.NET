using System.Runtime.InteropServices;

namespace Laboratorium3.Zadanie5.NET;

public static class SinusoidApplication
{
    // Definicje stałych używanych w API Windows
    private const int WmClose = 0x0010; // Komunikat o zamknięciu okna
    private const int WmPaint = 0x000F; // Komunikat o odświeżeniu okna

    private const int IdcArrow = 32512; // Identyfikator kursora strzałki
    private const int ColorWindow = 5; // Identyfikator koloru tła okna

    private const uint WsOverlapped = 0x00000000; // Styl okna: nakładające się okno
    private const uint WsCaption = 0x00C00000; // Styl okna: okno z tytułem
    private const uint WsSysmenu = 0x00080000; // Styl okna: okno z menu systemowym

    private const uint CsHredraw = 0x0002; // Styl klasy: przerysuj poziomo przy zmianie rozmiaru
    private const uint CsVredraw = 0x0001; // Styl klasy: przerysuj pionowo przy zmianie rozmiaru

    // Deklaracje funkcji API Windows

    // Funkcja domyślnej procedury okna, obsługuje komunikaty, które nie zostały obsłużone w aplikacji
    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    // Funkcja pobierająca uchwyt modułu na podstawie podanej nazwy
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    // Funkcja rejestrująca klasę okna, aby mogła być używana do tworzenia okien
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterClassEx([In] ref Wndclassex lpwcx);

    // Funkcja wyświetlająca lub ukrywająca okno
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // Funkcja przygotowująca okno do rysowania i wypełniająca strukturę Paintstruct informacjami o obszarze do malowania
    [DllImport("user32.dll")]
    private static extern IntPtr BeginPaint(IntPtr hWnd, out Paintstruct lpPaint);

    // Funkcja kończąca operację rysowania w oknie, zwalniająca zasoby używane podczas malowania
    [DllImport("user32.dll")]
    private static extern bool EndPaint(IntPtr hWnd, [In] ref Paintstruct lpPaint);

    // Funkcja pobierająca komunikat z kolejki komunikatów okna
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    // Funkcja tłumacząca komunikat, przekształcająca komunikaty klawiatury na bardziej przydatne formy
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool TranslateMessage([In] ref Msg lpMsg);

    // Funkcja przekazująca komunikat do odpowiedniej procedury okna
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr DispatchMessage([In] ref Msg lpmsg);

    // Funkcja wysyłająca komunikat o zakończeniu aplikacji do kolejki komunikatów
    [DllImport("user32.dll", SetLastError = true)]
    private static extern void PostQuitMessage(int nExitCode);

    // Funkcja ładująca kursor z zasobów aplikacji lub systemu
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    // Funkcja ustawiająca punkt początkowy dla rysowania w kontekście urządzenia
    [DllImport("gdi32.dll")]
    private static extern bool MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);

    // Funkcja rysująca linię od bieżącego punktu do określonego punktu w kontekście urządzenia
    [DllImport("gdi32.dll")]
    private static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);

    // Funkcja tworząca okno na podstawie zarejestrowanej klasy
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, // Dodatkowy styl okna
        string lpClassName, // Nazwa klasy okna
        string lpWindowName, // Nazwa okna (tytuł)
        uint dwStyle, // Styl okna
        int x, // Pozycja X okna
        int y, // Pozycja Y okna
        int nWidth, // Szerokość okna
        int nHeight, // Wysokość okna
        IntPtr hWndParent, // Uchwyt okna nadrzędnego
        IntPtr hMenu, // Uchwyt menu
        IntPtr hInstance, // Uchwyt instancji aplikacji
        IntPtr lpParam // Parametr dodatkowy
    );

    private static void Main()
    {
        // Pobranie uchwytu do bieżącego procesu, który jest niezbędny do rejestracji klasy okna
        var hInstance = GetModuleHandle(null);

        // Utworzenie i rejestracja klasy okna
        var wcex = new Wndclassex
        {
            cbSize = (uint)Marshal.SizeOf(typeof(Wndclassex)), // Rozmiar struktury
            style = CsHredraw | CsVredraw, // Style klasy okna
            lpfnWndProc = WndProc, // Wskaźnik na funkcję obsługi komunikatów okna
            hInstance = hInstance, // Uchwyt instancji aplikacji
            hCursor = LoadCursor(IntPtr.Zero, IdcArrow), // Ustawienie kursora
            hbrBackground = 1 + ColorWindow, // Ustawienie koloru tła okna
            lpszClassName = "SinusoidWindowClass" // Nazwa klasy okna
        };

        // Rejestracja klasy okna w systemie
        RegisterClassEx(ref wcex);

        // Utworzenie okna
        var hwnd = CreateWindowEx(
            0, // Dodatkowe style okna
            "SinusoidWindowClass", // Nazwa klasy okna
            "Sinusoid Application", // Tytuł okna
            WsOverlapped | WsCaption | WsSysmenu, // Style okna
            100, // Pozycja X okna
            100, // Pozycja Y okna
            800, // Szerokość okna
            600, // Wysokość okna
            IntPtr.Zero, // Uchwyt okna nadrzędnego
            IntPtr.Zero, // Uchwyt menu
            hInstance, // Uchwyt instancji aplikacji
            IntPtr.Zero // Dodatkowe parametry
        );

        // Wyświetlenie okna
        ShowWindow(hwnd, 1);

        // Pętla komunikatów - pobiera komunikaty z kolejki komunikatów i przekazuje je do odpowiednich funkcji
        while (GetMessage(out var msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg); // Tłumaczenie komunikatu
            DispatchMessage(ref msg); // Przekazywanie komunikatu do funkcji WndProc
        }
    }

    // Funkcja obsługi komunikatów okna
    private static IntPtr WndProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
    {
        // Przełącznik obsługujący różne typy komunikatów
        switch (message)
        {
            case WmPaint:
                // Komunikat o odświeżeniu okna - rysowanie sinusoidy
                var hdc = BeginPaint(hWnd, out var ps); // Rozpoczęcie malowania okna
                DrawSinusoid(hdc, ps.rcPaint); // Rysowanie sinusoidy
                EndPaint(hWnd, ref ps); // Zakończenie malowania okna
                break;
            case WmClose:
                // Komunikat o zamknięciu okna - zakończenie aplikacji
                PostQuitMessage(0); // Wysłanie komunikatu o zakończeniu aplikacji
                break;
            default:
                // Domyślne przetwarzanie nieobsłużonych komunikatów
                return DefWindowProc(hWnd, message, wParam, lParam);
        }

        return IntPtr.Zero;
    }

    // Funkcja rysująca sinusoidę w oknie
    private static void DrawSinusoid(IntPtr hdc, Rect rcPaint)
    {
        var width = rcPaint.Right - rcPaint.Left; // Szerokość obszaru do malowania
        var height = rcPaint.Bottom - rcPaint.Top; // Wysokość obszaru do malowania
        var midY = height / 2; // Środkowa współrzędna Y

        // Ustawienie początkowego punktu rysowania na lewy górny róg obszaru do malowania
        var x = rcPaint.Left;
        var y = (int)(Math.Sin((double)x / width * 2 * Math.PI) * midY + midY);
        MoveToEx(hdc, x, y, IntPtr.Zero); // Ustawienie początkowego punktu rysowania

        // Rysowanie linii do kolejnych punktów sinusoidy
        for (x = rcPaint.Left + 1; x < rcPaint.Right; x++)
        {
            y = (int)(Math.Sin((double)x / width * 2 * Math.PI) * midY + midY);
            LineTo(hdc, x, y); // Rysowanie linii do kolejnego punktu
        }
    }

    // Definicje struktur używanych w API Windows

    // Struktura opisująca klasę okna
    [StructLayout(LayoutKind.Sequential)]
    private struct Wndclassex
    {
        public uint cbSize; // Rozmiar struktury
        public uint style; // Style klasy okna

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public Wndproc lpfnWndProc; // Wskaźnik na funkcję obsługi komunikatów okna

        public int cbClsExtra; // Dodatkowe bajty pamięci dla klasy okna
        public int cbWndExtra; // Dodatkowe bajty pamięci dla instancji okna
        public IntPtr hInstance; // Uchwyt instancji aplikacji
        public IntPtr hIcon; // Uchwyt ikony okna
        public IntPtr hCursor; // Uchwyt kursora
        public IntPtr hbrBackground; // Uchwyt pędzla tła
        public string lpszMenuName; // Nazwa menu
        public string lpszClassName; // Nazwa klasy okna
        public IntPtr hIconSm; // Uchwyt małej ikony
    }

    // Struktura opisująca obszar do malowania
    [StructLayout(LayoutKind.Sequential)]
    private struct Paintstruct
    {
        public IntPtr hdc; // Kontekst urządzenia
        public bool fErase; // Czy obszar powinien być wymazany
        public Rect rcPaint; // Obszar do malowania
        public bool fRestore; // Czy kontekst urządzenia powinien być przywrócony
        public bool fIncUpdate; // Czy tło zostało zaktualizowane

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved; // Zarezerwowane
    }

    // Struktura opisująca prostokąt
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left; // Lewa krawędź
        public int Top; // Górna krawędź
        public int Right; // Prawa krawędź
        public int Bottom; // Dolna krawędź
    }

    // Struktura opisująca komunikat okna
    [StructLayout(LayoutKind.Sequential)]
    private struct Msg
    {
        public IntPtr hwnd; // Uchwyt okna
        public uint message; // Komunikat
        public IntPtr wParam; // Parametr komunikatu
        public IntPtr lParam; // Parametr komunikatu
        public uint time; // Czas komunikatu
        public Point pt; // Punkt na ekranie
    }

    // Struktura opisująca punkt na ekranie
    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int x; // Współrzędna X
        public int y; // Współrzędna Y
    }

    // Delegat reprezentujący funkcję obsługi komunikatów okna
    private delegate IntPtr Wndproc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);
}