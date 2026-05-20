using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DualCalc.Models;

namespace DualCalc.ViewModels
{
    /// <summary>
    /// 單一計算機的完整狀態與邏輯
    /// </summary>
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        // ── Display ───────────────────────────────────────────
        private string _display = "0";
        public string Display
        {
            get => _display;
            private set { _display = value; OnPropertyChanged(); }
        }

        private string _expression = string.Empty;
        public string Expression
        {
            get => _expression;
            private set { _expression = value; OnPropertyChanged(); }
        }

        // ── Internal state ────────────────────────────────────
        private string _currentInput = "0";
        private string _pendingExpression = string.Empty; // 完整算式（含歷史）
        private bool _justEvaluated = false;   // 剛按下 = 後
        private bool _hasError = false;

        // ── Memory ───────────────────────────────────────────
        private double _memory = 0;
        private bool _hasMemory = false;

        public bool HasMemory
        {
            get => _hasMemory;
            private set { _hasMemory = value; OnPropertyChanged(); }
        }

        // ── Calculator Label (A or B) ─────────────────────────
        public string Label { get; }
        public CalculatorViewModel(string label = "A") { Label = label; }

        // ─────────────────────────────────────────────────────
        // Button Commands
        // ─────────────────────────────────────────────────────

        public void OnDigit(string digit)
        {
            if (_hasError) return;

            if (_justEvaluated)
            {
                // 新計算：清除結果，重新輸入
                _pendingExpression = string.Empty;
                _currentInput = digit == "." ? "0." : digit;
                _justEvaluated = false;
            }
            else
            {
                if (_currentInput == "0" && digit != ".")
                    _currentInput = digit;
                else if (digit == "." && _currentInput.Contains('.'))
                    return; // 不重複小數點
                else
                    _currentInput += digit;
            }

            Display = _currentInput;
            UpdateExpression();
        }

        public void OnOperator(string op)
        {
            if (_hasError) return;

            if (_justEvaluated)
            {
                // 繼續用上一個結果做運算
                _pendingExpression = Display;
                _justEvaluated = false;
            }
            else
            {
                _pendingExpression += _currentInput;
            }

            _pendingExpression += op;
            _currentInput = "0";
            Expression = _pendingExpression;
            Display = _currentInput;
        }

        public void OnEquals()
        {
            if (_hasError) return;

            string fullExpr = _pendingExpression + _currentInput;
            Expression = fullExpr + "=";

            var (result, error) = CalculatorEngine.Evaluate(fullExpr);

            if (!string.IsNullOrEmpty(error))
            {
                Display = error;
                _hasError = true;
                return;
            }

            Display = CalculatorEngine.Format(result);
            _currentInput = Display;
            _pendingExpression = string.Empty;
            _justEvaluated = true;
        }

        // ── Special Functions ─────────────────────────────────

        public void OnPercent()
        {
            if (_hasError || !double.TryParse(_currentInput, out double val)) return;
            // 百分比：取當前數字的 1/100
            double pct = val / 100.0;
            _currentInput = CalculatorEngine.Format(pct);
            Display = _currentInput;
        }

        public void OnReciprocal()
        {
            if (_hasError || !double.TryParse(_currentInput, out double val)) return;
            try
            {
                double r = CalculatorEngine.Reciprocal(val);
                Expression = $"1/({_currentInput})";
                _currentInput = CalculatorEngine.Format(r);
                Display = _currentInput;
            }
            catch (Exception ex)
            {
                Display = ex.Message;
                _hasError = true;
            }
        }

        public void OnSquare()
        {
            if (_hasError || !double.TryParse(_currentInput, out double val)) return;
            double r = CalculatorEngine.Square(val);
            Expression = $"sqr({_currentInput})";
            _currentInput = CalculatorEngine.Format(r);
            Display = _currentInput;
        }

        public void OnSquareRoot()
        {
            if (_hasError || !double.TryParse(_currentInput, out double val)) return;
            try
            {
                double r = CalculatorEngine.SquareRoot(val);
                Expression = $"√({_currentInput})";
                _currentInput = CalculatorEngine.Format(r);
                Display = _currentInput;
            }
            catch (Exception ex)
            {
                Display = ex.Message;
                _hasError = true;
            }
        }

        public void OnNegate()
        {
            if (_hasError || !double.TryParse(_currentInput, out double val)) return;
            double r = CalculatorEngine.Negate(val);
            _currentInput = CalculatorEngine.Format(r);
            Display = _currentInput;
        }

        // ── Clear ─────────────────────────────────────────────

        public void OnClearEntry()
        {
            _currentInput = "0";
            _hasError = false;
            Display = "0";
            UpdateExpression();
        }

        public void OnClear()
        {
            _currentInput = "0";
            _pendingExpression = string.Empty;
            _hasError = false;
            _justEvaluated = false;
            Display = "0";
            Expression = string.Empty;
        }

        public void OnBackspace()
        {
            if (_hasError || _justEvaluated) { OnClear(); return; }
            if (_currentInput.Length <= 1 || (_currentInput.Length == 2 && _currentInput[0] == '-'))
                _currentInput = "0";
            else
                _currentInput = _currentInput[..^1];
            Display = _currentInput;
            UpdateExpression();
        }

        // ── Memory ───────────────────────────────────────────

        public void OnMemoryClear()
        {
            _memory = 0;
            HasMemory = false;
        }

        public void OnMemoryRecall()
        {
            if (!_hasMemory) return;
            _currentInput = CalculatorEngine.Format(_memory);
            Display = _currentInput;
        }

        public void OnMemoryAdd()
        {
            if (!double.TryParse(Display, out double val)) return;
            _memory += val;
            HasMemory = true;
        }

        public void OnMemorySubtract()
        {
            if (!double.TryParse(Display, out double val)) return;
            _memory -= val;
            HasMemory = true;
        }

        public void OnMemoryStore()
        {
            if (!double.TryParse(Display, out double val)) return;
            _memory = val;
            HasMemory = true;
        }

        // ── Helper ────────────────────────────────────────────

        private void UpdateExpression()
        {
            Expression = _pendingExpression.Length > 0
                ? _pendingExpression  // 顯示運算式（不含當前輸入，等按=才補）
                : string.Empty;
        }

        // ── INotifyPropertyChanged ────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
