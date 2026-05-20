using System;
using System.Collections.Generic;

namespace DualCalc.Models
{
    /// <summary>
    /// 核心計算引擎：實作先乘除後加減（Shunting-yard 算法）
    /// </summary>
    public class CalculatorEngine
    {
        // ── 運算子優先級 ──────────────────────────────────────
        private static readonly Dictionary<string, int> Precedence = new()
        {
            { "+", 1 }, { "-", 1 },
            { "×", 2 }, { "÷", 2 }, { "%", 2 }
        };

        // ── Token 化 ──────────────────────────────────────────
        /// <summary>
        /// 將算式字串切成 token 清單
        /// e.g. "3+5×2" → ["3", "+", "5", "×", "2"]
        /// </summary>
        public static List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (char.IsDigit(c) || c == '.')
                {
                    sb.Append(c);
                }
                else if (c == '-' && (i == 0 || IsOperator(expression[i - 1].ToString())))
                {
                    // 負號（unary minus）
                    sb.Append(c);
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                    if (!char.IsWhiteSpace(c))
                        tokens.Add(c.ToString());
                }
            }
            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens;
        }

        private static bool IsOperator(string s) =>
            s == "+" || s == "-" || s == "×" || s == "÷" || s == "%";

        // ── Shunting-yard → RPN ───────────────────────────────
        private static Queue<string> ToRPN(List<string> tokens)
        {
            var output = new Queue<string>();
            var ops = new Stack<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    output.Enqueue(token);
                }
                else if (IsOperator(token))
                {
                    while (ops.Count > 0 && IsOperator(ops.Peek()) &&
                           Precedence[ops.Peek()] >= Precedence[token])
                    {
                        output.Enqueue(ops.Pop());
                    }
                    ops.Push(token);
                }
                else if (token == "(")
                {
                    ops.Push(token);
                }
                else if (token == ")")
                {
                    while (ops.Count > 0 && ops.Peek() != "(")
                        output.Enqueue(ops.Pop());
                    if (ops.Count > 0) ops.Pop(); // 移除 "("
                }
            }

            while (ops.Count > 0)
                output.Enqueue(ops.Pop());

            return output;
        }

        // ── 計算 RPN ──────────────────────────────────────────
        private static double EvaluateRPN(Queue<string> rpn)
        {
            var stack = new Stack<double>();

            while (rpn.Count > 0)
            {
                var token = rpn.Dequeue();
                if (double.TryParse(token, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double num))
                {
                    stack.Push(num);
                }
                else
                {
                    if (stack.Count < 2)
                        throw new InvalidOperationException("算式格式錯誤");

                    double b = stack.Pop();
                    double a = stack.Pop();

                    double result = token switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "×" => a * b,
                        "÷" => b == 0 ? throw new DivideByZeroException("除數不可為零") : a / b,
                        "%" => a % b,
                        _ => throw new InvalidOperationException($"未知運算子: {token}")
                    };
                    stack.Push(result);
                }
            }

            if (stack.Count != 1)
                throw new InvalidOperationException("算式格式錯誤");

            return stack.Pop();
        }

        // ── 公開 API ──────────────────────────────────────────

        /// <summary>
        /// 計算整條算式字串（支援先乘除後加減）
        /// </summary>
        public static (double Result, string Error) Evaluate(string expression)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                    return (0, string.Empty);

                var tokens = Tokenize(expression);
                var rpn = ToRPN(tokens);
                var result = EvaluateRPN(rpn);
                return (result, string.Empty);
            }
            catch (DivideByZeroException)
            {
                return (0, "除數不可為零");
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }

        /// <summary>
        /// 特殊函數計算
        /// </summary>
        public static double Reciprocal(double x)
        {
            if (x == 0) throw new DivideByZeroException("除數不可為零");
            return 1.0 / x;
        }

        public static double Square(double x) => x * x;

        public static double SquareRoot(double x)
        {
            if (x < 0) throw new InvalidOperationException("無效的輸入");
            return Math.Sqrt(x);
        }

        public static double Negate(double x) => -x;

        /// <summary>
        /// 格式化顯示：移除多餘小數點零
        /// </summary>
        public static string Format(double value)
        {
            if (double.IsNaN(value)) return "無效的輸入";
            if (double.IsInfinity(value)) return "除數不可為零";

            // 最多顯示 15 位有效數字
            string s = value.ToString("G15", System.Globalization.CultureInfo.InvariantCulture);
            return s;
        }
    }
}
