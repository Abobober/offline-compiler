using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly string _tempPath = Path.Combine(Directory.GetCurrentDirectory(), "tempCodeFiles");

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;

        if (!Directory.Exists(_tempPath))
        {
            Directory.CreateDirectory(_tempPath);
        }
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("Home/ExecuteCode")] // Явный атрибут маршрута для метода GET
    public IActionResult ExecuteCode()
    {
        return View();
    }

    [HttpPost("Home/ExecuteCode")] // Явный атрибут маршрута для метода POST
    public ActionResult ExecuteCode(string code, string language, string userData)
    {
        var result = RunCode(code, language, userData);
        return View(Tuple.Create(result.Item1, result.Item2));
    }

    private Tuple<string, string> RunCode(string code, string language, string userData)
    {
        string output = string.Empty;
        string errors = string.Empty;

        try
        {
            switch (language.ToLower())
            {
                case "python":
                    (output, errors) = ExecutePython(code, userData);
                    break;
                case "cpp":
                    (output, errors) = ExecuteCpp(code, userData);
                    break;
                case "java":
                    (output, errors) = ExecuteJava(code, userData);
                    break;
                case "csharp":
                    (output, errors) = ExecuteCSharp(code, userData);
                    break;
                default:
                    errors = "Unsupported language";
                    break;
            }
        }
        catch (Exception ex)
        {
            errors = ex.Message;
        }

        return Tuple.Create(output, errors);
    }

    private Tuple<string, string> ExecutePython(string code, string userData)
    {
        string fileName = Path.Combine(_tempPath, "script.py");
        System.IO.File.WriteAllText(fileName, code);

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{fileName}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        return ExecuteProcess(psi, fileName, userData);
    }

    private Tuple<string, string> ExecuteCpp(string code, string userData)
    {
        string sourceFile = Path.Combine(_tempPath, "program.cpp");
        string exeFile = Path.Combine(_tempPath, "program.exe");
        System.IO.File.WriteAllText(sourceFile, code);

        var compileProcess = new ProcessStartInfo
        {
            FileName = "g++",
            Arguments = $"\"{sourceFile}\" -o \"{exeFile}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var compileResult = ExecuteProcess(compileProcess);

        if (string.IsNullOrEmpty(compileResult.Item2))
        {
            var runProcess = new ProcessStartInfo
            {
                FileName = $"\"{exeFile}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var runResult = ExecuteProcess(runProcess, null, userData);

            System.IO.File.Delete(sourceFile);
            System.IO.File.Delete(exeFile);

            return runResult;
        }

        System.IO.File.Delete(sourceFile);

        return compileResult;
    }

    private Tuple<string, string> ExecuteJava(string code, string userData)
    {
        string sourceFile = Path.Combine(_tempPath, "Program.java");
        string classFile = Path.Combine(_tempPath, "Program.class");
        System.IO.File.WriteAllText(sourceFile, code);

        var compileProcess = new ProcessStartInfo
        {
            FileName = "javac",
            Arguments = $"\"{sourceFile}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var compileResult = ExecuteProcess(compileProcess);

        if (string.IsNullOrEmpty(compileResult.Item2))
        {
            var runProcess = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $"-cp \"{_tempPath}\" Program",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var runResult = ExecuteProcess(runProcess, null, userData);

            System.IO.File.Delete(sourceFile);
            System.IO.File.Delete(classFile);

            return runResult;
        }

        System.IO.File.Delete(sourceFile);

        return compileResult;
    }

    private Tuple<string, string> ExecuteCSharp(string code, string userData)
    {
        string sourceFile = Path.Combine(_tempPath, "Program.cs");
        string exeFile = Path.Combine(_tempPath, "Program.exe");
        System.IO.File.WriteAllText(sourceFile, code);

        var compileProcess = new ProcessStartInfo
        {
            FileName = "csc",
            Arguments = $"/out:\"{exeFile}\" \"{sourceFile}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var compileResult = ExecuteProcess(compileProcess);

        if (string.IsNullOrEmpty(compileResult.Item2))
        {
            var runProcess = new ProcessStartInfo
            {
                FileName = $"\"{exeFile}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var runResult = ExecuteProcess(runProcess, null, userData);

            System.IO.File.Delete(sourceFile);
            System.IO.File.Delete(exeFile);

            return runResult;
        }

        System.IO.File.Delete(sourceFile);

        return compileResult;
    }

    private Tuple<string, string> ExecuteProcess(ProcessStartInfo psi, string? fileToDelete = null, string? userData = null)
    {
        string output = string.Empty;
        string errors = string.Empty;

        try
        {
            using (var process = new Process())
            {
                process.StartInfo = psi;
                process.Start();

                if (!string.IsNullOrEmpty(userData))
                {
                    using (StreamWriter writer = process.StandardInput)
                    {
                        writer.WriteLine(userData);
                        writer.Flush();
                    }
                }

                if (!process.WaitForExit(5000)) // 5 секунд таймаут
                {
                    process.Kill();
                    errors = "Process timed out.";
                }
                else
                {
                    output = process.StandardOutput.ReadToEnd();
                    errors = process.StandardError.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            errors = ex.Message;
        }
        finally
        {
            if (!string.IsNullOrEmpty(fileToDelete) && System.IO.File.Exists(fileToDelete))
            {
                System.IO.File.Delete(fileToDelete);
            }
        }

        return Tuple.Create(output, errors);
    }
}

