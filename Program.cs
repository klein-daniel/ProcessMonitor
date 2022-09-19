using System.ComponentModel;
using System.Diagnostics;

public class Processes
{
    public static void Main(String[] args)
    {
        bool validArguments = AreValidArguments(args);
        if (validArguments)
        {
            int maximumLifeTime = int.Parse(args[1]);
            int monitoringFrequency = (int) TimeSpan.FromMinutes(int.Parse(args[2])).TotalMilliseconds;

            Console.WriteLine($"Monitoring\n" +
                            $"Process name: {args[0]}\n" +
                            $"Maximum lifetime scope: {args[1]} minute(s)\n" +
                            $"Monitoring Frequency: {args[2]} minute(s)\n");
            do
            {
                RunProgram(args, maximumLifeTime, monitoringFrequency);
            } while ((!Console.KeyAvailable) || (Console.ReadKey().Key != ConsoleKey.Q));
        }
    }

    private static void RunProgram(String[] args, int maximumLifeTime, int monitoringFrequency)
    {
        IEnumerable<Process> processes = Process.GetProcesses();
        var eligibleProcesses = processes.Where(p => (p.ProcessName.ToLower() == args[0].ToLower()) && IsEligibleForRemoval(p, maximumLifeTime)).ToList<Process>();
        if (!eligibleProcesses.Any())
        {
            Console.WriteLine($"No process named {args[0].ToLower()} is eligible for removal ... waiting for {monitoringFrequency} millis.");
            SafeSleep(monitoringFrequency);
        }
        else
        {
            foreach (var eligibleProcess in eligibleProcesses)
            {
                Console.WriteLine($"{eligibleProcess.ProcessName} - ELIGIBLE FOR REMOVAL - start time: {eligibleProcess.StartTime}");
                Console.WriteLine($"Minutes passed: {(DateTime.Now - eligibleProcess.StartTime).TotalMinutes}");
                KillProcess(eligibleProcess);
            }
            SafeSleep(monitoringFrequency);
        }
    }

    private static bool IsEligibleForRemoval(Process process, int maximumLifeTime)
    {
        if (process == null) throw new ArgumentNullException($"The given process does not exist!");
        if (maximumLifeTime == 0) throw new ArgumentNullException($"The maximum lifetime or monitor frequency passed it's not valid!");

        TimeSpan timeFromProcessStart;
        try
        {
            timeFromProcessStart = DateTime.Now - process.StartTime;
            if (timeFromProcessStart.TotalMinutes >= maximumLifeTime) return true;
            else return false;
        }
        catch (Win32Exception ex)
        {
            Console.WriteLine($"Process ID: {process.Id} can't be accessed, moving on ... Ex: {ex.Message}");
            return false;
        }
    }

    private static void KillProcess(Process process)
    {
        try
        {
            process.Kill();
            Console.WriteLine($"Process: {process.ProcessName} - has been terminated!\n");
            Log($"Process {process.ProcessName} - terminated!");
        }
        catch (Win32Exception ex)
        {
            Console.WriteLine($"Process {process.ProcessName} hasn't been terminated! Exception: {ex.Message}");
        }    
    }

    private static bool AreValidArguments(params String[] arguments)
    {
        if ((arguments.Length == 0) || arguments.Length != 3)
        {
            Console.WriteLine("A number of 3 arguments are considered valid! " +
                "Example: processName maximumLifeTime MonitoringFrequency");
            return false;
        }
        if (String.IsNullOrEmpty(arguments[0]))
        {
            Console.WriteLine("No process name has been passed!");
            return false;
        }

        bool parseFirstArgument = int.TryParse(arguments[1], out int firstArgument);
        bool parseSecondArgument = int.TryParse(arguments[2], out int secondArgument);

        if (!parseFirstArgument || !parseSecondArgument)
        {
            Console.WriteLine($"Attempted conversion of {arguments[1]} or {arguments[2]} failed!");
            return false;
        }
        if ((firstArgument < 0) || (secondArgument < 0))
        {
            Console.WriteLine($"The maximum lifetime and monitoring frequency values must be greater than 0!");
            return false;

        }
        return true;
    }

    private static void SafeSleep(int time)
    {
        try
        {
            Thread.Sleep(time);
        }
        catch (ThreadInterruptedException)
        {
            Console.WriteLine("Program exits...");
        }
    }

    private static void Log(string message)
    {
        try
        {
            FileStream filestream = new("out.txt", FileMode.Create);
            var streamwriter = new StreamWriter(filestream)
            {
                AutoFlush = true
            };
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);
            Console.WriteLine($"{DateTime.Now} - {message}\n");
        }
        catch (IOException ex)
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
            Console.SetError(standardOutput);
            Console.WriteLine($"Could not write to the log file - Exception: {ex.Message}");
        }

        
    }
}