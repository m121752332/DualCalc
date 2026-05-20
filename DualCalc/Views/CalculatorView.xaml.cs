using DualCalc.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DualCalc.Views
{
    public sealed partial class CalculatorView : UserControl
    {
        public CalculatorViewModel ViewModel { get; internal set; }

        public CalculatorView()
        {
            this.InitializeComponent();
            ViewModel = new CalculatorViewModel();
        }

        public CalculatorView(CalculatorViewModel vm)
        {
            this.InitializeComponent();
            ViewModel = vm;
        }

        // ── Digit ─────────────────────────────────────────────
        private void OnDigitClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string digit)
                ViewModel.OnDigit(digit);
        }

        // ── Operator ──────────────────────────────────────────
        private void OnOperatorClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string op)
                ViewModel.OnOperator(op);
        }

        // ── Special ───────────────────────────────────────────
        private void OnEquals(object sender, RoutedEventArgs e)      => ViewModel.OnEquals();
        private void OnPercent(object sender, RoutedEventArgs e)     => ViewModel.OnPercent();
        private void OnReciprocal(object sender, RoutedEventArgs e)  => ViewModel.OnReciprocal();
        private void OnSquare(object sender, RoutedEventArgs e)      => ViewModel.OnSquare();
        private void OnSquareRoot(object sender, RoutedEventArgs e)  => ViewModel.OnSquareRoot();
        private void OnNegate(object sender, RoutedEventArgs e)      => ViewModel.OnNegate();

        // ── Clear ─────────────────────────────────────────────
        private void OnClearEntry(object sender, RoutedEventArgs e)  => ViewModel.OnClearEntry();
        private void OnClear(object sender, RoutedEventArgs e)       => ViewModel.OnClear();
        private void OnBackspace(object sender, RoutedEventArgs e)   => ViewModel.OnBackspace();

        // ── Memory ───────────────────────────────────────────
        private void OnMemoryClear(object sender, RoutedEventArgs e)    => ViewModel.OnMemoryClear();
        private void OnMemoryRecall(object sender, RoutedEventArgs e)   => ViewModel.OnMemoryRecall();
        private void OnMemoryAdd(object sender, RoutedEventArgs e)      => ViewModel.OnMemoryAdd();
        private void OnMemorySubtract(object sender, RoutedEventArgs e) => ViewModel.OnMemorySubtract();
        private void OnMemoryStore(object sender, RoutedEventArgs e)    => ViewModel.OnMemoryStore();
    }
}
