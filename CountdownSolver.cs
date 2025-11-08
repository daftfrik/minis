using System.Diagnostics;

namespace Countdown;

public class Expression(int value, string text) {
    public int Value { get; } = value;
    public string Text { get; } = text;
}

public class SolutionResult(int value, string expression, int distance, long elapsedMs) {
    public int Value { get; } = value;
    public string Expression { get; } = expression;
    public int Distance { get; } = distance;
    public long ElapsedMilliseconds { get; } = elapsedMs;

    public bool IsExact => Distance == 0;
}

public class CountdownSolver {
    private const int MaxValue = 100000;
    private const int MinValue = 1;
    private const int MinNumberForMultiplication = 2;
    
    private readonly int _target;
    private readonly Dictionary<string, List<Expression>> _cache;
    
    public CountdownSolver(int target) {
        if (target <= 0) throw new ArgumentException("Target must be positive", nameof(target));
        _target = target;
        _cache = new Dictionary<string, List<Expression>>();
    }
    
    public SolutionResult Solve(List<int> numbers) {
        ValidateNumbers(numbers);
        
        var stopwatch = Stopwatch.StartNew();
        _cache.Clear();
        
        var solution = FindBestSolution(numbers);
        stopwatch.Stop();
        
        if (solution == null) throw new InvalidOperationException("No solution found - this should never happen");
        
        int distance = Math.Abs(_target - solution.Value);
        return new SolutionResult(solution.Value, solution.Text, distance, stopwatch.ElapsedMilliseconds);
    }
    
    private static void ValidateNumbers(List<int> numbers) {
        if (numbers == null || numbers.Count == 0) throw new ArgumentException("Numbers list cannot be empty", nameof(numbers));
        if (numbers.Any(n => n <= 0)) throw new ArgumentException("All numbers must be positive", nameof(numbers));
        if (numbers.Any(n => n > 100)) Console.WriteLine("Warning: Numbers greater than 100 are unusual for Countdown");
    }
    
    private Expression? FindBestSolution(List<int> numbers) {
        Expression? bestResult = null;
        int bestDistance = int.MaxValue;
        
        var allResults = GenerateAllExpressions(numbers);
        
        foreach (var result in allResults) {
            int dist = Math.Abs(_target - result.Value);
            if (dist >= bestDistance) continue;
            
            bestDistance = dist;
            bestResult = result;
            if (dist == 0) return bestResult;
        }
        
        return bestResult;
    }
    
    private List<Expression> GenerateAllExpressions(List<int> numbers) {
        var sorted = numbers.OrderBy(x => x).ToList();
        string key = CreateCacheKey(sorted);
        
        if (_cache.TryGetValue(key, out var cachedResults)) return cachedResults;
        var results = new List<Expression>();
        
        if (numbers.Count == 1) {
            results.Add(new Expression(numbers[0], numbers[0].ToString()));
            _cache[key] = results;
            return results;
        }
        
        var seenValues = new HashSet<int>();
        int totalPartitions = 1 << numbers.Count;
        for (int mask = 1; mask < totalPartitions - 1; mask++) {
            var (left, right) = PartitionByMask(numbers, mask);
            
            if (left.Count > right.Count) continue;
            
            var leftResults = GenerateAllExpressions(left);
            var rightResults = GenerateAllExpressions(right);
            
            foreach (var expr in leftResults.SelectMany(leftExpr => 
                         rightResults.Select(rightExpr => 
                             CombineExpressions(leftExpr, rightExpr)).SelectMany(combinedExpressions => 
                             combinedExpressions.Where(expr => seenValues.Add(expr.Value))))) {
                results.Add(expr);
                if (expr.Value != _target) continue;
                _cache[key] = results;
                return results;
            }
        }
        
        _cache[key] = results;
        return results;
    }
    
    private static string CreateCacheKey(List<int> sortedNumbers) => string.Join(",", sortedNumbers);

    private static (List<int> left, List<int> right) PartitionByMask(List<int> numbers, int mask) {
        var left = new List<int>();
        var right = new List<int>();
        
        for (int i = 0; i < numbers.Count; i++) {
            if ((mask & (1 << i)) != 0) left.Add(numbers[i]);
            else right.Add(numbers[i]);
        }
        
        return (left, right);
    }
    
    private static List<Expression> CombineExpressions(Expression left, Expression right) {
        var results = new List<Expression>();
        int a = left.Value;
        int b = right.Value;
        
        // Addition (commutative - order doesn't matter)
        TryAddExpression(results, a + b, $"({left.Text} + {right.Text})");
        
        // Subtraction (only if result is positive, try both orders)
        if (a > b) TryAddExpression(results, a - b, $"({left.Text} - {right.Text})");
        else if (b > a) TryAddExpression(results, b - a, $"({right.Text} - {left.Text})");
        
        // Multiplication (skip multiplying by 1, check for overflow)
        if (a >= MinNumberForMultiplication && b >= MinNumberForMultiplication) {
            long product = (long)a * b;
            if (product <= MaxValue) TryAddExpression(results, (int)product, $"({left.Text} * {right.Text})");
        }
        
        // Division (only exact division, skip dividing by 1, try both orders)
        if (b > 1 && a % b == 0) TryAddExpression(results, a / b, $"({left.Text} / {right.Text})");
        if (a > 1 && b % a == 0 && a != b) TryAddExpression(results, b / a, $"({right.Text} / {left.Text})");
        
        return results;
    }
    
    private static void TryAddExpression(List<Expression> results, int value, string text) {
        if (value is >= MinValue and <= MaxValue) results.Add(new Expression(value, text));
    }
}