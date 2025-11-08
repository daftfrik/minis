namespace Countdown;

public static class Program {
    public static void Main() {
        Console.WriteLine("\x1b[4mCountdown Numbers Solver\x1b[0m");
        Console.WriteLine();
        
        try {
            int target = ReadTarget();
            var numbers = ReadNumbers();
            
            Console.WriteLine();
            Console.WriteLine($"Target: {target}");
            Console.WriteLine($"Numbers: {string.Join(", ", numbers)}");
            Console.WriteLine();
            Console.WriteLine("Solving...");
            Console.WriteLine();
            
            var solver = new CountdownSolver(target);
            var solution = solver.Solve(numbers);
            
            DisplaySolution(solution);
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.WriteLine("any key to quit ");
        string? quit = Console.ReadLine();
    }
    
    private static int ReadTarget() {
        Console.WriteLine("Enter target number: ");
        string? input = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input, out int target) || target <= 0)
            throw new ArgumentException("Invalid target number. Must be a positive integer.");
        
        return target;
    }
    
    private static List<int> ReadNumbers() {
        Console.Write("Enter 6 numbers (separated by spaces): ");
        string? input = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("No numbers entered.");
        
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 6) throw new ArgumentException($"Expected 6 numbers, but got {parts.Length}");
        
        var numbers = new List<int>();
        foreach (string part in parts) {
            if (!int.TryParse(part, out int number) || number <= 0) throw new ArgumentException($"Invalid number: {part}. All numbers must be positive integers.");
            numbers.Add(number);
        }
        
        return numbers;
    }
    
    private static void DisplaySolution(SolutionResult solution) {
        Console.WriteLine($"Solution: {solution.Expression} = {solution.Value}");

        Console.WriteLine(solution.IsExact ? "Exact match!" : $"Distance from target: {solution.Distance}");

        Console.WriteLine($"Time taken: {solution.ElapsedMilliseconds} ms");
    }
}